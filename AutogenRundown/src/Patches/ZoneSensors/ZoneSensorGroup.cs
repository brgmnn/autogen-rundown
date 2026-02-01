using AmorLib.Networking.StateReplicators;
using AutogenRundown.DataBlocks.Custom.ZoneSensors;
using AutogenRundown.DataBlocks.Objectives;
using GameData;
using LevelGeneration;
using SNetwork;
using UnityEngine;
using UnityEngine.AI;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Represents a group of sensors that share the same trigger events.
/// Manages the enabled/disabled state and visual updates for all sensors in the group.
///
/// Network sync architecture:
/// - StateReplicator: Syncs enabled/disabled state and per-sensor mask
/// - PositionReplicator: Syncs spawn positions from host to clients/late joiners
/// - WaypointReplicator: Syncs NavMesh waypoints for moving sensors
/// - MovementReplicator: Periodically syncs movement progress for moving sensors
///
/// Flow:
/// - Host: Generates positions → Sets PositionReplicator → Spawns sensors
/// - Host (moving): Generates waypoints → Sets WaypointReplicator → Periodic movement sync
/// - Client: Creates group → OnPositionStateChanged → Spawns sensors
/// - Late Joiner: OnPositionStateChanged + OnWaypointStateChanged → Spawns/syncs sensors
/// </summary>
public class ZoneSensorGroup
{
    // Base IDs for zone sensor replicators (avoid collisions with other mods)
    private const uint STATE_REPLICATOR_BASE_ID = 0x5A534E00; // "ZSN" prefix + group index
    private const uint POSITION_REPLICATOR_BASE_ID = 0x5A535000; // "ZSP" prefix + group index * 8 + batch index
    private const uint WAYPOINT_REPLICATOR_BASE_ID = 0x5A535700; // "ZSW" prefix + group index * 1024 + sensor index * 8 + batch index
    private const uint MOVEMENT_REPLICATOR_BASE_ID = 0x5A536000; // "ZSM" prefix + group index * 4 + batch index

    // Maximum batches per group (supports up to 128 sensors: 8 * 16)
    private const int MAX_BATCHES_PER_GROUP = 8;

    // Maximum waypoint batches per sensor (supports up to 160 waypoints: 8 * 20)
    private const int MAX_WAYPOINT_BATCHES_PER_SENSOR = 8;

    // Maximum waypoint replicators per group (8 batches per sensor * 128 sensors = 1024)
    private const int MAX_WAYPOINT_REPLICATORS = 128 * MAX_WAYPOINT_BATCHES_PER_SENSOR;

    // Maximum movement batches per group (32 sensors per batch, 4 batches = 128 sensors)
    private const int MAX_MOVEMENT_BATCHES = 4;

    // Movement sync interval in seconds
    private const float MOVEMENT_SYNC_INTERVAL = 0.5f;

    public int ReplicatorIndex { get; private set; }
    public int Id { get; private set; }
    public eDimensionIndex DimensionIndex { get; private set; }
    public LG_LayerType LayerType { get; private set; }
    public eLocalZoneIndex LocalZoneIndex { get; private set; }
    public bool Enabled { get; private set; } = true;
    public List<GameObject> Sensors { get; } = new();
    public List<WardenObjectiveEvent> EventsOnTrigger { get; set; } = new();

    public StateReplicator<ZoneSensorGroupState>? Replicator { get; private set; }

    // Multiple position replicators for groups with more than 16 sensors
    public List<StateReplicator<ZoneSensorPositionState>> PositionReplicators { get; } = new();

    // Waypoint replicators: 1 per moving sensor (syncs actual NavMesh path waypoints)
    public List<StateReplicator<ZoneSensorWaypointState>> WaypointReplicators { get; } = new();

    // Movement replicators: 1 per batch of 32 sensors (syncs movement progress)
    public List<StateReplicator<ZoneSensorMovementState>> MovementReplicators { get; } = new();

    // Host stores generated waypoints by sensor index
    private Dictionary<int, Vector3[]> generatedWaypoints = new();

    // Movement sync timer (host only)
    private float movementSyncTimer = 0f;

    // Track which sensors have moving behavior
    private HashSet<int> movingSensorIndices = new();

    // Track if waypoints have been received for late joiners
    private Dictionary<int, Vector3[]> receivedWaypoints = new();
    private Dictionary<int, float> receivedWaypointSpeeds = new();

    // Track received waypoint batches for assembly (sensorIndex -> (batchIndex -> state))
    private Dictionary<int, Dictionary<int, ZoneSensorWaypointState>> receivedWaypointBatches = new();

    // Track received movement state for late joiners (applied after sensors spawn)
    private Dictionary<int, (int waypointIndex, bool forward, float progress)> receivedMovementState = new();

    // Track if this is a late joiner spawn (affects sensor creation)
    private bool isLateJoinerSpawn = false;

    private bool triggerEach;
    private ZoneSensorGroupState currentState;
    private ZoneSensorGroupState previousState;  // Track which sensors were visible for transition detection

    // Pending spawn data - stored until positions are received
    private LG_Zone? pendingZone;
    private List<ZoneSensorGroupDefinition>? pendingGroupDefinitions;
    private bool sensorsSpawned = false;

    // Batch tracking for multi-replicator position sync
    private int expectedBatchCount = 0;
    private Dictionary<int, ZoneSensorPositionState> receivedBatches = new();

    /// <summary>
    /// Initializes the sensor group with network replicators.
    /// Call this before setting positions or spawning sensors.
    /// </summary>
    /// <param name="replicatorIndex">Index for replicator ID allocation (separate from definition Id)</param>
    /// <param name="id">Unique ID from the sensor definition</param>
    /// <param name="events">Events to trigger when sensors are activated</param>
    /// <param name="triggerEach">Whether each sensor triggers independently</param>
    /// <param name="expectedSensorCount">Expected total sensors to calculate batch count</param>
    /// <param name="hasMovingSensors">Whether this group has moving sensors (creates waypoint/movement replicators)</param>
    /// <param name="dimension">Dimension index where this group is located</param>
    /// <param name="layer">Layer type where this group is located</param>
    /// <param name="zoneIndex">Local zone index where this group is located</param>
    public void Initialize(int replicatorIndex, int id, List<WardenObjectiveEvent> events, bool triggerEach,
        int expectedSensorCount, bool hasMovingSensors,
        eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex zoneIndex, bool startEnabled = true)
    {
        ReplicatorIndex = replicatorIndex;
        Id = id;
        DimensionIndex = dimension;
        LayerType = layer;
        LocalZoneIndex = zoneIndex;
        EventsOnTrigger = events;
        Enabled = startEnabled;
        this.triggerEach = triggerEach;
        currentState = new ZoneSensorGroupState(startEnabled);
        previousState = new ZoneSensorGroupState(false);
        isLateJoinerSpawn = false;
        sensorsSpawned = false;
        receivedBatches.Clear();
        generatedWaypoints.Clear();
        receivedWaypoints.Clear();
        receivedWaypointSpeeds.Clear();
        receivedWaypointBatches.Clear();
        receivedMovementState.Clear();
        movingSensorIndices.Clear();
        movementSyncTimer = 0f;

        // Calculate number of position batches needed
        expectedBatchCount = ZoneSensorPositionState.CalculateBatchCount(expectedSensorCount);
        if (expectedBatchCount > MAX_BATCHES_PER_GROUP)
        {
            Plugin.Logger.LogWarning($"ZoneSensorGroup {id}: Sensor count {expectedSensorCount} requires {expectedBatchCount} batches, clamping to {MAX_BATCHES_PER_GROUP}");
            expectedBatchCount = MAX_BATCHES_PER_GROUP;
        }

        // Create network replicators with unique IDs
        var stateReplicatorId = STATE_REPLICATOR_BASE_ID + (uint)replicatorIndex;

        // State replicator for enabled/disabled
        Replicator = StateReplicator<ZoneSensorGroupState>.Create(
            stateReplicatorId,
            new ZoneSensorGroupState(startEnabled),
            LifeTimeType.Session
        );

        if (Replicator != null)
        {
            Replicator.OnStateChanged += OnStateChanged;
        }

        // Create position replicators for each batch
        // ID scheme: BASE_ID + replicatorIndex * 8 + batchIndex
        PositionReplicators.Clear();
        for (var batchIndex = 0; batchIndex < expectedBatchCount; batchIndex++)
        {
            var positionReplicatorId = POSITION_REPLICATOR_BASE_ID + (uint)(replicatorIndex * MAX_BATCHES_PER_GROUP + batchIndex);

            var posReplicator = StateReplicator<ZoneSensorPositionState>.Create(
                positionReplicatorId,
                new ZoneSensorPositionState(),
                LifeTimeType.Session
            );

            if (posReplicator != null)
            {
                posReplicator.OnStateChanged += OnPositionStateChanged;
                PositionReplicators.Add(posReplicator);
            }
        }

        // Create waypoint and movement replicators if this group has moving sensors
        if (hasMovingSensors)
        {
            InitializeMovementReplicators(replicatorIndex, expectedSensorCount);
        }

        Plugin.Logger.LogDebug($"ZoneSensorGroup {id}: Initialized with {expectedBatchCount} position replicators for {expectedSensorCount} sensors, hasMovingSensors={hasMovingSensors}");
    }

    /// <summary>
    /// Creates waypoint and movement replicators for groups with moving sensors.
    /// </summary>
    private void InitializeMovementReplicators(int replicatorIndex, int expectedSensorCount)
    {
        // Waypoint replicators: 8 batches per sensor (max 128 sensors = 1024 replicators)
        // ID scheme: BASE_ID + replicatorIndex * 1024 + sensorIndex * 8 + batchIndex
        WaypointReplicators.Clear();
        var maxSensors = Math.Min(expectedSensorCount, 128);
        var waypointReplicatorCount = maxSensors * MAX_WAYPOINT_BATCHES_PER_SENSOR;

        for (var sensorIndex = 0; sensorIndex < maxSensors; sensorIndex++)
        {
            for (var batchIndex = 0; batchIndex < MAX_WAYPOINT_BATCHES_PER_SENSOR; batchIndex++)
            {
                var waypointReplicatorId = WAYPOINT_REPLICATOR_BASE_ID +
                                           (uint)(replicatorIndex * MAX_WAYPOINT_REPLICATORS + sensorIndex * MAX_WAYPOINT_BATCHES_PER_SENSOR + batchIndex);

                var waypointReplicator = StateReplicator<ZoneSensorWaypointState>.Create(
                    waypointReplicatorId,
                    new ZoneSensorWaypointState(),
                    LifeTimeType.Session
                );

                if (waypointReplicator != null)
                {
                    waypointReplicator.OnStateChanged += OnWaypointStateChanged;
                    WaypointReplicators.Add(waypointReplicator);
                }
            }
        }

        // Movement replicators: 1 per batch of 32 sensors (max 4 batches)
        // ID scheme: BASE_ID + replicatorIndex * 4 + batchIndex
        MovementReplicators.Clear();
        var movementBatchCount = ZoneSensorMovementState.CalculateBatchCount(expectedSensorCount);
        movementBatchCount = Math.Min(movementBatchCount, MAX_MOVEMENT_BATCHES);

        for (var batchIndex = 0; batchIndex < movementBatchCount; batchIndex++)
        {
            var movementReplicatorId = MOVEMENT_REPLICATOR_BASE_ID + (uint)(replicatorIndex * MAX_MOVEMENT_BATCHES + batchIndex);

            var movementReplicator = StateReplicator<ZoneSensorMovementState>.Create(
                movementReplicatorId,
                new ZoneSensorMovementState(),
                LifeTimeType.Session
            );

            if (movementReplicator != null)
            {
                movementReplicator.OnStateChanged += OnMovementStateChanged;
                MovementReplicators.Add(movementReplicator);
            }
        }

        Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Created {WaypointReplicators.Count} waypoint replicators ({maxSensors} sensors * {MAX_WAYPOINT_BATCHES_PER_SENSOR} batches) and {MovementReplicators.Count} movement replicators");
    }

    /// <summary>
    /// Sets the pending spawn data. Sensors will be spawned when positions are received.
    /// </summary>
    public void SetPendingSpawnData(LG_Zone zone, List<ZoneSensorGroupDefinition> groupDefinitions)
    {
        pendingZone = zone;
        pendingGroupDefinitions = groupDefinitions;
    }

    /// <summary>
    /// Host-only: Sets position states across all replicators and triggers spawning.
    /// Call this after generating random positions.
    /// </summary>
    /// <param name="positionBatches">List of position state batches to broadcast</param>
    public void SetPositionsAndSpawn(List<ZoneSensorPositionState> positionBatches)
    {
        if (positionBatches.Count != PositionReplicators.Count)
        {
            Plugin.Logger.LogWarning($"ZoneSensorGroup {Id}: Batch count mismatch - {positionBatches.Count} batches vs {PositionReplicators.Count} replicators");
            return;
        }

        // Store batches and broadcast to clients
        for (var i = 0; i < positionBatches.Count; i++)
        {
            if (PositionReplicators[i] == null || !PositionReplicators[i].IsValid)
            {
                Plugin.Logger.LogWarning($"ZoneSensorGroup {Id}: Position replicator {i} not valid");
                continue;
            }

            PositionReplicators[i].SetState(positionBatches[i]);
            receivedBatches[positionBatches[i].BatchIndex] = positionBatches[i];
        }

        // Host spawns sensors locally using all batches
        SpawnSensorsFromBatches(isRecall: false);
    }

    private void OnPositionStateChanged(ZoneSensorPositionState oldState, ZoneSensorPositionState newState, bool isRecall)
    {
        Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Position state changed, batch={newState.BatchIndex}/{newState.TotalBatches}, sensorCount={newState.SensorCount}, isRecall={isRecall}, alreadySpawned={sensorsSpawned}");

        // Skip if no positions or already spawned
        if (!newState.HasPositions || sensorsSpawned)
            return;

        // Store this batch
        receivedBatches[newState.BatchIndex] = newState;

        // Only set expectedBatchCount from the first batch received to avoid mid-reception changes
        if (newState.TotalBatches > 0 && expectedBatchCount == 0)
        {
            expectedBatchCount = newState.TotalBatches;
        }
        else if (newState.TotalBatches > 0 && newState.TotalBatches != expectedBatchCount)
        {
            Plugin.Logger.LogWarning($"ZoneSensorGroup {Id}: Batch count mismatch - expected {expectedBatchCount}, got {newState.TotalBatches}");
        }

        // Check if we've received all batches (count check, actual sequential validation happens in SpawnSensorsFromBatches)
        if (receivedBatches.Count >= expectedBatchCount && expectedBatchCount > 0)
        {
            Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: All {expectedBatchCount} batches received, spawning sensors");
            SpawnSensorsFromBatches(isRecall);
        }
        else
        {
            Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Waiting for more batches ({receivedBatches.Count}/{expectedBatchCount})");
        }
    }

    /// <summary>
    /// Spawns sensors using positions from all received batches.
    /// Called when all batches have been received.
    /// </summary>
    private void SpawnSensorsFromBatches(bool isRecall)
    {
        if (sensorsSpawned)
        {
            Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Sensors already spawned, skipping");
            return;
        }

        if (pendingZone == null || pendingGroupDefinitions == null || pendingGroupDefinitions.Count == 0)
        {
            Plugin.Logger.LogWarning($"ZoneSensorGroup {Id}: Missing pending spawn data");
            return;
        }

        if (!ZoneSensorAssets.AssetsLoaded)
        {
            Plugin.Logger.LogError($"ZoneSensorGroup {Id}: CircleSensor prefab not loaded!");
            return;
        }

        // Validate all sequential batch indices 0 through N-1 exist before spawning
        for (var i = 0; i < expectedBatchCount; i++)
        {
            if (!receivedBatches.ContainsKey(i))
            {
                Plugin.Logger.LogWarning($"ZoneSensorGroup {Id}: Missing batch {i}/{expectedBatchCount}, aborting spawn");
                return;
            }
        }

        var zone = pendingZone;

        // Build ordered list of all positions from batches
        var allPositions = new List<(Vector3 position, int waypointCount)>();
        for (var batchIndex = 0; batchIndex < expectedBatchCount; batchIndex++)
        {
            var batch = receivedBatches[batchIndex];
            for (var i = 0; i < batch.SensorCount; i++)
            {
                allPositions.Add((batch.GetPosition(i), batch.GetWaypointCount(i)));
            }
        }

        // Use first group definition for visual settings (typically only one group)
        var groupDef = pendingGroupDefinitions[0];

        // Track if this is a late joiner spawn (affects sensor creation)
        isLateJoinerSpawn = isRecall && !SNet.IsMaster;

        for (var i = 0; i < allPositions.Count; i++)
        {
            var (position, waypointCount) = allPositions[i];

            var sensorGO = CreateSensorVisual(position, groupDef, Id, i);
            Sensors.Add(sensorGO);

            // Add movement if enabled
            if (groupDef.Moving > 1)
            {
                // Generate waypoints deterministically using position index as seed factor
                InitializeSensorMovement(zone, groupDef, sensorGO, position, Id, i);
            }
        }

        // Mark as spawned only after successful sensor creation
        sensorsSpawned = true;

        // For clients: Apply any stored waypoints that were received before sensors spawned
        // This ensures clients use the host's authoritative waypoints instead of locally generated ones
        if (!SNet.IsMaster && receivedWaypoints.Count > 0)
        {
            foreach (var kvp in receivedWaypoints)
            {
                var sensorIndex = kvp.Key;
                var waypoints = kvp.Value;
                if (receivedWaypointSpeeds.TryGetValue(sensorIndex, out var speed))
                {
                    ApplyWaypointsToSensor(sensorIndex, waypoints, speed);
                }
            }
            Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Applied {receivedWaypoints.Count} stored waypoints to sensors");
        }

        // For clients: Apply any stored movement state that was received before sensors spawned
        if (!SNet.IsMaster && receivedMovementState.Count > 0)
        {
            foreach (var kvp in receivedMovementState)
            {
                var sensorIndex = kvp.Key;
                var (waypointIndex, forward, progress) = kvp.Value;
                ApplyMovementStateToSensor(sensorIndex, waypointIndex, forward, progress, snap: true);
            }
            Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Applied {receivedMovementState.Count} stored movement states");
        }

        // Apply current group state to newly spawned sensors
        // For late joiners, currentState already contains the correct state from recall
        // For host with startEnabled=false, this deactivates the sensor GameObjects
        UpdateVisualsUnsynced(currentState);
        Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Applied state to spawned sensors (isRecall={isRecall})");

        // Diagnostic: verify sensors are actually active
        for (var i = 0; i < Sensors.Count; i++)
        {
            var s = Sensors[i];
            Plugin.Logger.LogDebug($"ZoneSensor: Sensor {i} active={s.activeSelf}, " +
                $"activeInHierarchy={s.activeInHierarchy}, pos={s.transform.position}");
        }

        Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Spawned {Sensors.Count} sensors from {expectedBatchCount} batches (isRecall={isRecall})");

        // Clear pending data
        pendingZone = null;
        pendingGroupDefinitions = null;
        receivedBatches.Clear();
    }

    /// <summary>
    /// Creates the visual representation of a sensor.
    /// </summary>
    private GameObject CreateSensorVisual(Vector3 position, ZoneSensorGroupDefinition groupDef, int groupIndex, int sensorIndex)
    {
        // Instantiate CircleSensor prefab
        var sensorGO = UnityEngine.Object.Instantiate(ZoneSensorAssets.CircleSensor);
        sensorGO.name = $"ZoneSensor_{groupIndex}_{sensorIndex}";

        // Ensure all child objects are active (prefab may have disabled children)
        foreach (var transform in sensorGO.GetComponentsInChildren<Transform>(true))
            transform.gameObject.SetActive(true);

        // Position and scale per EOS pattern
        sensorGO.transform.SetPositionAndRotation(position, Quaternion.identity);

        // For 75% above floor: center at 25% of scaled height
        var targetHeight = 0.6f;
        var scaledHeight = (float)groupDef.Height * (float)groupDef.Radius;
        var heightOffset = (targetHeight - 0.5f) * scaledHeight;

        sensorGO.transform.localScale = new Vector3(
            (float)groupDef.Radius,
            (float)groupDef.Radius,
            (float)groupDef.Radius);
        sensorGO.transform.localPosition += Vector3.up * heightOffset;

        // Set color via material (child 0 -> child 1 -> renderer)
        var renderer = sensorGO.transform.GetChild(0).GetChild(1)
            .gameObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.SetColor("_ColorA", new Color(
                (float)groupDef.Color.Red,
                (float)groupDef.Color.Green,
                (float)groupDef.Color.Blue,
                (float)groupDef.Color.Alpha));
        }
        else
        {
            Plugin.Logger.LogWarning($"ZoneSensor: Sensor {sensorIndex} has no renderer at expected child path!");
        }

        // Set up text display
        var infoGO = sensorGO.transform.GetChild(0).GetChild(2);

        // Destroy corrupted TMPro from prefab
        var corruptedTMPro = infoGO.GetChild(0).gameObject;
        corruptedTMPro.transform.SetParent(null);
        UnityEngine.Object.Destroy(corruptedTMPro);

        if (groupDef.HideText)
        {
            Plugin.Logger.LogDebug($"ZoneSensor: Sensor {sensorIndex} has hidden text");
        }
        else
        {
            var tmproGO = GtfoTextMeshPro.Instantiate(infoGO.gameObject);
            if (tmproGO != null)
            {
                var text = tmproGO.GetComponent<TMPro.TextMeshPro>();
                if (text != null)
                {
                    var normalColor = new Color(
                        (float)groupDef.TextColor.Red,
                        (float)groupDef.TextColor.Green,
                        (float)groupDef.TextColor.Blue,
                        (float)groupDef.TextColor.Alpha);

                    if (groupDef.EncryptedText)
                    {
                        var encryptedColor = new Color(
                            (float)groupDef.EncryptedTextColor.Red,
                            (float)groupDef.EncryptedTextColor.Green,
                            (float)groupDef.EncryptedTextColor.Blue,
                            (float)groupDef.EncryptedTextColor.Alpha);

                        var animator = sensorGO.AddComponent<ZoneSensorTextAnimator>();
                        var sensorText = groupDef.Text ?? GetRandomSensorText(groupIndex, sensorIndex);
                        animator.Initialize(sensorText, normalColor, encryptedColor, text);
                    }
                    else
                    {
                        text.SetText(groupDef.Text ?? GetRandomSensorText(groupIndex, sensorIndex));
                        text.m_fontColor = text.m_fontColor32 = normalColor;
                    }
                }
            }
        }

        // Add detection collider
        var collider = sensorGO.AddComponent<ZoneSensorCollider>();
        collider.Id = groupIndex;
        collider.SensorIndex = sensorIndex;
        collider.TriggerEach = groupDef.TriggerEach;
        collider.Radius = (float)groupDef.Radius;

        // Late joiners create sensors inactive - they'll be activated when state is received
        if (isLateJoinerSpawn)
        {
            sensorGO.SetActive(false);
        }

        Plugin.Logger.LogDebug($"ZoneSensor: Created sensor {sensorIndex} at {position} with radius {groupDef.Radius}, TriggerEach={groupDef.TriggerEach}");
        return sensorGO;
    }

    /// <summary>
    /// Sensor texts for display.
    /// </summary>
    private static readonly List<string> SensorTexts = new()
    {
        "Se@#$urity Sc3AN",
        "S:_EC/uR_ITY S:/Ca_N",
        "SC4N_ACT!VE",
        "D3T:ECT_M0DE",
        "S3NS0R://ON",
        "//TR1GG3R_Z0NE",
        "AUT0_SC4N::",
        "PR0X_D3TECT"
    };

    /// <summary>
    /// Gets a random sensor text deterministically based on group and sensor index.
    /// </summary>
    private static string GetRandomSensorText(int groupIndex, int sensorIndex)
    {
        // Use deterministic index based on group and sensor
        var textIndex = (groupIndex * 31 + sensorIndex * 17) % SensorTexts.Count;
        return SensorTexts[textIndex];
    }

    /// <summary>
    /// Initializes movement for a sensor using deterministic waypoint generation.
    /// Uses a per-sensor seed to ensure all clients generate identical paths.
    /// Host broadcasts waypoints via replicators for late joiners.
    /// </summary>
    private void InitializeSensorMovement(LG_Zone zone, ZoneSensorGroupDefinition groupDef, GameObject sensorGO, Vector3 startPosition, int groupIndex, int sensorIndex)
    {
        var positions = new List<Vector3> { startPosition };

        // Create deterministic random for this specific sensor
        // Combines session seed with group/sensor indices for uniqueness
        var sensorSeed = Builder.SessionSeedRandom.Seed + groupIndex * 1000 + sensorIndex * 100;
        var sensorRandom = new System.Random(sensorSeed);

        // Generate (Moving - 1) additional random positions
        for (var i = 1; i < groupDef.Moving; i++)
        {
            Vector3 pos;
            if (groupDef.AreaIndex >= 0 && groupDef.AreaIndex < zone.m_areas.Count)
            {
                var area = zone.m_areas[groupDef.AreaIndex];
                pos = GetRandomPositionInArea(area, sensorRandom);
            }
            else
            {
                var randomAreaIndex = sensorRandom.Next(0, zone.m_areas.Count);
                var area = zone.m_areas[randomAreaIndex];
                pos = GetRandomPositionInArea(area, sensorRandom);
            }

            if (TryGetPosOnNavMesh(ref pos))
                positions.Add(pos);
        }

        if (positions.Count < 2)
        {
            Plugin.Logger.LogWarning("ZoneSensor: Not enough valid positions for moving sensor");
            return;
        }

        // Snap start position
        var start = positions[0];
        if (!TryGetPosOnNavMesh(ref start))
        {
            Plugin.Logger.LogWarning("ZoneSensor: Could not snap start position to NavMesh");
            return;
        }
        positions[0] = start;

        var mover = sensorGO.AddComponent<ZoneSensorMover>();
        var waypointsArray = mover.Initialize(positions, (float)groupDef.Speed, (float)groupDef.EdgeDistance);

        // Track this sensor as moving
        movingSensorIndices.Add(sensorIndex);

        // Host: Store and broadcast waypoints
        if (SNet.IsMaster && waypointsArray != null && waypointsArray.Length > 0)
        {
            generatedWaypoints[sensorIndex] = waypointsArray;
            BroadcastWaypoints(sensorIndex, waypointsArray, (float)groupDef.Speed);
        }
    }

    /// <summary>
    /// Broadcasts waypoints for a sensor to all clients.
    /// Splits large waypoint arrays into multiple batches.
    /// Host only.
    /// </summary>
    private void BroadcastWaypoints(int sensorIndex, Vector3[] waypoints, float speed)
    {
        // Calculate replicator index range for this sensor
        var baseReplicatorIndex = sensorIndex * MAX_WAYPOINT_BATCHES_PER_SENSOR;

        if (baseReplicatorIndex >= WaypointReplicators.Count)
        {
            Plugin.Logger.LogWarning($"ZoneSensorGroup {Id}: Cannot broadcast waypoints for sensor {sensorIndex}, no replicators");
            return;
        }

        // Create batched states
        var batches = ZoneSensorWaypointState.FromArrayBatched(sensorIndex, waypoints, speed);

        Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Broadcasting {waypoints.Length} waypoints for sensor {sensorIndex} in {batches.Count} batches, speed={speed}");

        // Broadcast each batch
        for (var batchIndex = 0; batchIndex < batches.Count; batchIndex++)
        {
            var replicatorIndex = baseReplicatorIndex + batchIndex;

            if (replicatorIndex >= WaypointReplicators.Count)
            {
                Plugin.Logger.LogWarning($"ZoneSensorGroup {Id}: Waypoint replicator index {replicatorIndex} out of range");
                break;
            }

            var replicator = WaypointReplicators[replicatorIndex];
            if (replicator == null || !replicator.IsValid)
            {
                Plugin.Logger.LogWarning($"ZoneSensorGroup {Id}: Waypoint replicator {replicatorIndex} not valid");
                continue;
            }

            replicator.SetState(batches[batchIndex]);
        }
    }

    /// <summary>
    /// Callback for waypoint state changes from replicator.
    /// Collects batches and assembles complete waypoint arrays when all batches are received.
    /// Applies waypoints to all clients (not just late joiners) to ensure consistency.
    /// </summary>
    private void OnWaypointStateChanged(ZoneSensorWaypointState oldState, ZoneSensorWaypointState newState, bool isRecall)
    {
        if (!newState.HasWaypoints)
            return;

        int sensorIndex = newState.SensorIndex;
        int batchIndex = newState.BatchIndex;
        int totalBatches = newState.TotalBatches;

        Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Waypoint batch received for sensor {sensorIndex}, batch {batchIndex + 1}/{totalBatches}, {newState.WaypointCount} waypoints, isRecall={isRecall}");

        // Initialize batch tracking for this sensor if needed
        if (!receivedWaypointBatches.ContainsKey(sensorIndex))
        {
            receivedWaypointBatches[sensorIndex] = new Dictionary<int, ZoneSensorWaypointState>();
        }

        // Store this batch
        receivedWaypointBatches[sensorIndex][batchIndex] = newState;

        // Store speed from batch 0
        if (batchIndex == 0)
        {
            receivedWaypointSpeeds[sensorIndex] = newState.Speed;
        }

        // Check if all batches received for this sensor
        if (receivedWaypointBatches[sensorIndex].Count >= totalBatches)
        {
            // Assemble complete waypoint array
            var waypoints = AssembleWaypointsFromBatches(sensorIndex, totalBatches);
            if (waypoints != null)
            {
                var speed = receivedWaypointSpeeds.TryGetValue(sensorIndex, out var s) ? s : 0f;

                Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Assembled {waypoints.Length} waypoints for sensor {sensorIndex} from {totalBatches} batches, speed={speed}");

                // Store assembled waypoints
                receivedWaypoints[sensorIndex] = waypoints;

                // Apply to sensor if spawned and we're not the host
                if (sensorsSpawned && !SNet.IsMaster)
                {
                    ApplyWaypointsToSensor(sensorIndex, waypoints, speed);
                }

                // Clean up batch tracking for this sensor
                receivedWaypointBatches.Remove(sensorIndex);
            }
        }
    }

    /// <summary>
    /// Assembles waypoints from received batches into a single array.
    /// </summary>
    private Vector3[]? AssembleWaypointsFromBatches(int sensorIndex, int totalBatches)
    {
        if (!receivedWaypointBatches.TryGetValue(sensorIndex, out var batches))
            return null;

        // Validate all batches are present
        for (var i = 0; i < totalBatches; i++)
        {
            if (!batches.ContainsKey(i))
            {
                Plugin.Logger.LogWarning($"ZoneSensorGroup {Id}: Missing batch {i} for sensor {sensorIndex}");
                return null;
            }
        }

        // Calculate total waypoint count
        var totalWaypoints = 0;
        for (var i = 0; i < totalBatches; i++)
        {
            totalWaypoints += batches[i].WaypointCount;
        }

        // Assemble waypoints in order
        var result = new Vector3[totalWaypoints];
        var waypointIndex = 0;
        for (var batchIdx = 0; batchIdx < totalBatches; batchIdx++)
        {
            var batch = batches[batchIdx];
            for (var i = 0; i < batch.WaypointCount; i++)
            {
                result[waypointIndex++] = batch.GetWaypoint(i);
            }
        }

        return result;
    }

    /// <summary>
    /// Applies received waypoints to an existing sensor's mover component.
    /// Used for client sync (both late joiners and clients at mission start).
    /// </summary>
    private void ApplyWaypointsToSensor(int sensorIndex, Vector3[] waypoints, float speed)
    {
        if (sensorIndex < 0 || sensorIndex >= Sensors.Count)
            return;

        var sensor = Sensors[sensorIndex];
        if (sensor == null)
            return;

        var mover = sensor.GetComponent<ZoneSensorMover>();
        if (mover != null)
        {
            mover.SetWaypoints(waypoints, speed);
            Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Applied {waypoints.Length} waypoints to sensor {sensorIndex}, speed={speed}");
        }
    }

    /// <summary>
    /// Callback for movement state changes from replicator.
    /// Queues state for late joiners if sensors haven't spawned yet.
    /// </summary>
    private void OnMovementStateChanged(ZoneSensorMovementState oldState, ZoneSensorMovementState newState, bool isRecall)
    {
        if (!newState.HasMovementData)
            return;

        // If sensors haven't spawned yet, store movement state for later application
        if (!sensorsSpawned)
        {
            for (var i = 0; i < newState.SensorCount; i++)
            {
                var globalSensorIndex = newState.GetGlobalSensorIndex(i);
                var (waypointIndex, forward, progress) = newState.GetMovementState(i);
                receivedMovementState[globalSensorIndex] = (waypointIndex, forward, progress);
            }
            Plugin.Logger.LogDebug($"ZoneSensorGroup {Id}: Stored movement state for {newState.SensorCount} sensors (sensors not yet spawned)");
            return;
        }

        // Apply movement state to sensors in this batch
        for (var i = 0; i < newState.SensorCount; i++)
        {
            var globalSensorIndex = newState.GetGlobalSensorIndex(i);
            var (waypointIndex, forward, progress) = newState.GetMovementState(i);

            ApplyMovementStateToSensor(globalSensorIndex, waypointIndex, forward, progress, snap: isRecall);
        }
    }

    /// <summary>
    /// Applies movement state to a sensor's mover component.
    /// </summary>
    private void ApplyMovementStateToSensor(int sensorIndex, int waypointIndex, bool forward, float progress, bool snap)
    {
        if (sensorIndex < 0 || sensorIndex >= Sensors.Count)
            return;

        // Don't apply movement to disabled sensors
        if (!currentState.IsSensorEnabled(sensorIndex))
            return;

        var sensor = Sensors[sensorIndex];
        if (sensor == null)
            return;

        var mover = sensor.GetComponent<ZoneSensorMover>();
        if (mover != null)
        {
            mover.ApplyMovementState(waypointIndex, forward, progress, snap);
        }
    }

    /// <summary>
    /// Update method for periodic movement sync.
    /// Called by ZoneSensorManager.
    /// </summary>
    public void Update()
    {
        // Only host broadcasts state
        if (!SNet.IsMaster)
            return;

        // Movement state sync
        if (MovementReplicators.Count == 0 || movingSensorIndices.Count == 0)
            return;

        movementSyncTimer += Time.deltaTime;
        if (movementSyncTimer >= MOVEMENT_SYNC_INTERVAL)
        {
            movementSyncTimer = 0f;
            BroadcastMovementState();
        }
    }

    /// <summary>
    /// Broadcasts current movement state for all moving sensors.
    /// Host only. Packs entries consecutively with explicit global indices.
    /// </summary>
    private void BroadcastMovementState()
    {
        // Group sensors into batches
        var batchCount = MovementReplicators.Count;

        for (var batchIndex = 0; batchIndex < batchCount; batchIndex++)
        {
            var replicator = MovementReplicators[batchIndex];
            if (replicator == null || !replicator.IsValid)
                continue;

            var state = new ZoneSensorMovementState
            {
                BatchIndex = (byte)batchIndex
            };

            var startSensorIndex = batchIndex * ZoneSensorMovementState.MaxSensors;
            var entryIndex = 0;  // Packed entry index (consecutive)

            for (var localIndex = 0; localIndex < ZoneSensorMovementState.MaxSensors; localIndex++)
            {
                var globalIndex = startSensorIndex + localIndex;

                // Skip sensors that don't exist or aren't moving
                if (globalIndex >= Sensors.Count || !movingSensorIndices.Contains(globalIndex))
                    continue;

                var sensor = Sensors[globalIndex];
                if (sensor == null)
                    continue;

                var mover = sensor.GetComponent<ZoneSensorMover>();
                if (mover == null)
                    continue;

                var (waypointIndex, forward, progress) = mover.GetMovementState();
                state.SetMovementState(entryIndex, globalIndex, waypointIndex, forward, progress);
                entryIndex++;
            }

            if (entryIndex > 0)
            {
                state.SensorCount = (byte)entryIndex;
                replicator.SetState(state);
            }
        }
    }

    /// <summary>
    /// Gets a random position inside an area using the provided random instance.
    /// Uses the area's node cluster to find valid positions, similar to game's method.
    /// </summary>
    private Vector3 GetRandomPositionInArea(LG_Area area, System.Random random)
    {
        var courseNode = area.m_courseNode;
        if (courseNode?.m_nodeCluster == null)
            return courseNode?.Position ?? Vector3.zero;

        // Use our random instance to get a value for node selection
        var randomValue = (float)random.NextDouble();

        // Try to get a random node from the cluster
        if (courseNode.m_nodeCluster.TryGetRandomNode(randomValue, out var node))
            return node.Position;

        return courseNode.Position;
    }

    /// <summary>
    /// Attempts to snap a position to the NavMesh.
    /// </summary>
    private bool TryGetPosOnNavMesh(ref Vector3 pos)
    {
        if (!NavMesh.SamplePosition(pos + Vector3.up * 0.15f, out var hit, 1f, -1))
            return false;
        pos = hit.position;
        return true;
    }

    public void AddSensor(GameObject sensor)
    {
        Sensors.Add(sensor);
    }

    /// <summary>
    /// Sets the enabled state of the sensor group.
    /// Syncs to all clients via StateReplicator.
    /// </summary>
    /// <param name="enabled">Whether to enable or disable the sensor group</param>
    /// <param name="preserveTriggered">When true, only re-enable sensors that haven't been triggered</param>
    /// <param name="resetTriggered">When true, clear triggered state before enabling (all sensors reappear)</param>
    public void SetEnabled(bool enabled, bool preserveTriggered = false, bool resetTriggered = false)
    {
        // Reset triggered state if requested
        if (resetTriggered)
            currentState.ClearTriggered();

        // Reset mask when enabling
        if (enabled)
        {
            currentState.SetAllEnabled();
            if (preserveTriggered)
                currentState.ApplyTriggeredMask();  // Disable sensors that were triggered
        }

        currentState.Enabled = enabled;

        // Update visuals immediately for responsive feel
        UpdateVisualsUnsynced(currentState);

        // Sync to clients (only master can broadcast)
        if (Replicator != null && Replicator.IsValid)
        {
            Replicator.SetState(currentState);
        }
    }

    /// <summary>
    /// Disables a single sensor and syncs to all clients.
    /// Used when TriggerEach mode is enabled.
    /// Supports up to 128 sensors per group.
    /// </summary>
    public void DisableSensor(int sensorIndex)
    {
        if (sensorIndex < 0 || sensorIndex >= Sensors.Count || sensorIndex >= 128)
            return;

        currentState.SetSensorDisabled(sensorIndex);
        currentState.SetSensorTriggered(sensorIndex);  // Mark as triggered

        // Update local visual immediately
        var sensor = Sensors[sensorIndex];
        sensor?.SetActive(false);

        // Sync to all clients
        if (Replicator != null && Replicator.IsValid)
        {
            Replicator.SetState(currentState);
        }
    }

    /// <summary>
    /// Callback for network state changes.
    /// </summary>
    private void OnStateChanged(ZoneSensorGroupState oldState, ZoneSensorGroupState newState, bool isRecall)
    {
        currentState = newState;
        UpdateVisualsUnsynced(newState, oldState);
    }

    /// <summary>
    /// Updates the local visual state without network sync.
    /// Resets collider state only when transitioning from disabled to enabled.
    /// </summary>
    /// <param name="state">The new state to apply</param>
    /// <param name="oldState">The previous state for transition detection (optional)</param>
    private void UpdateVisualsUnsynced(ZoneSensorGroupState state, ZoneSensorGroupState? oldState = null)
    {
        Enabled = state.Enabled;

        // Use provided oldState if available, otherwise fall back to tracked previousState
        var priorState = oldState ?? previousState;

        for (var i = 0; i < Sensors.Count; i++)
        {
            var sensor = Sensors[i];
            if (sensor != null)
            {
                // Sensor is visible if group enabled AND individual sensor enabled
                var sensorEnabled = state.Enabled && state.IsSensorEnabled(i);
                var wasEnabled = priorState.Enabled && priorState.IsSensorEnabled(i);

                sensor.SetActive(sensorEnabled);

                // Only reset collider state when TRANSITIONING to enabled
                // This prevents immediate triggers when sensors reappear on top of players
                if (sensorEnabled && !wasEnabled)
                {
                    var collider = sensor.GetComponent<ZoneSensorCollider>();
                    collider?.ResetState();
                }
            }
        }

        // Update previous state for next comparison
        previousState = state;
    }

    /// <summary>
    /// Cleans up all sensor objects.
    /// </summary>
    public void Cleanup()
    {
        // Unsubscribe and unload state replicator
        if (Replicator != null)
        {
            Replicator.OnStateChanged -= OnStateChanged;
            Replicator.Unload();
        }
        Replicator = null;

        // Unsubscribe and unload position replicators
        foreach (var posReplicator in PositionReplicators)
        {
            if (posReplicator != null)
            {
                posReplicator.OnStateChanged -= OnPositionStateChanged;
                posReplicator.Unload();
            }
        }
        PositionReplicators.Clear();

        // Unsubscribe and unload waypoint replicators
        foreach (var waypointReplicator in WaypointReplicators)
        {
            if (waypointReplicator != null)
            {
                waypointReplicator.OnStateChanged -= OnWaypointStateChanged;
                waypointReplicator.Unload();
            }
        }
        WaypointReplicators.Clear();

        // Unsubscribe and unload movement replicators
        foreach (var movementReplicator in MovementReplicators)
        {
            if (movementReplicator != null)
            {
                movementReplicator.OnStateChanged -= OnMovementStateChanged;
                movementReplicator.Unload();
            }
        }
        MovementReplicators.Clear();

        // Destroy sensor GameObjects
        foreach (var sensor in Sensors)
        {
            if (sensor != null)
                UnityEngine.Object.Destroy(sensor);
        }
        Sensors.Clear();

        // Reset state for level reload
        currentState = new ZoneSensorGroupState(false);
        previousState = new ZoneSensorGroupState(false);
        isLateJoinerSpawn = false;

        // Clear pending data
        pendingZone = null;
        pendingGroupDefinitions = null;
        sensorsSpawned = false;
        receivedBatches.Clear();
        expectedBatchCount = 0;

        // Clear movement sync data
        generatedWaypoints.Clear();
        receivedWaypoints.Clear();
        receivedWaypointSpeeds.Clear();
        receivedWaypointBatches.Clear();
        receivedMovementState.Clear();
        movingSensorIndices.Clear();
        movementSyncTimer = 0f;
    }
}
