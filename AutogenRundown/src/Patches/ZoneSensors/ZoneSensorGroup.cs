using AIGraph;
using AmorLib.Networking.StateReplicators;
using AutogenRundown.DataBlocks.Custom.ZoneSensors;
using AutogenRundown.DataBlocks.Objectives;
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
///
/// Flow:
/// - Host: Generates positions → Sets PositionReplicator → Spawns sensors
/// - Client: Creates group → OnPositionStateChanged → Spawns sensors
/// - Late Joiner: OnPositionStateChanged (isRecall=true) → Spawns sensors
/// </summary>
public class ZoneSensorGroup
{
    // Base IDs for zone sensor replicators (avoid collisions with other mods)
    private const uint STATE_REPLICATOR_BASE_ID = 0x5A534E00; // "ZSN" prefix + group index
    private const uint POSITION_REPLICATOR_BASE_ID = 0x5A535000; // "ZSP" prefix + group index

    public int GroupIndex { get; private set; }
    public bool Enabled { get; private set; } = true;
    public List<GameObject> Sensors { get; } = new();
    public List<WardenObjectiveEvent> EventsOnTrigger { get; set; } = new();

    public StateReplicator<ZoneSensorGroupState>? Replicator { get; private set; }
    public StateReplicator<ZoneSensorPositionState>? PositionReplicator { get; private set; }

    private bool triggerEach;
    private uint currentSensorMask = uint.MaxValue;

    // Pending spawn data - stored until positions are received
    private LG_Zone? pendingZone;
    private List<ZoneSensorGroupDefinition>? pendingGroupDefinitions;
    private bool sensorsSpawned = false;

    /// <summary>
    /// Initializes the sensor group with network replicators.
    /// Call this before setting positions or spawning sensors.
    /// </summary>
    public void Initialize(int index, List<WardenObjectiveEvent> events, bool triggerEach)
    {
        GroupIndex = index;
        EventsOnTrigger = events;
        Enabled = true;
        this.triggerEach = triggerEach;
        currentSensorMask = uint.MaxValue;
        sensorsSpawned = false;

        // Create network replicators with unique IDs
        uint stateReplicatorId = STATE_REPLICATOR_BASE_ID + (uint)index;
        uint positionReplicatorId = POSITION_REPLICATOR_BASE_ID + (uint)index;

        // State replicator for enabled/disabled
        Replicator = StateReplicator<ZoneSensorGroupState>.Create(
            stateReplicatorId,
            new ZoneSensorGroupState(true, uint.MaxValue),
            LifeTimeType.Session
        );

        if (Replicator != null)
        {
            Replicator.OnStateChanged += OnStateChanged;
        }

        // Position replicator for spawn positions
        PositionReplicator = StateReplicator<ZoneSensorPositionState>.Create(
            positionReplicatorId,
            new ZoneSensorPositionState(),
            LifeTimeType.Session
        );

        if (PositionReplicator != null)
        {
            PositionReplicator.OnStateChanged += OnPositionStateChanged;
        }
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
    /// Host-only: Sets position state and triggers spawning on all clients.
    /// Call this after generating random positions.
    /// </summary>
    public void SetPositionsAndSpawn(ZoneSensorPositionState positionState)
    {
        if (PositionReplicator == null || !PositionReplicator.IsValid)
        {
            Plugin.Logger.LogWarning($"ZoneSensorGroup {GroupIndex}: Position replicator not valid");
            return;
        }

        // Store state and broadcast to clients
        PositionReplicator.SetState(positionState);

        // Host also spawns sensors locally
        SpawnSensorsFromPositions(positionState, isRecall: false);
    }

    private void OnPositionStateChanged(ZoneSensorPositionState oldState, ZoneSensorPositionState newState, bool isRecall)
    {
        Plugin.Logger.LogDebug($"ZoneSensorGroup {GroupIndex}: Position state changed, isRecall={isRecall}, sensorCount={newState.SensorCount}, alreadySpawned={sensorsSpawned}");

        // Skip if no positions or already spawned
        if (!newState.HasPositions || sensorsSpawned)
            return;

        // Spawn sensors using received positions
        SpawnSensorsFromPositions(newState, isRecall);
    }

    /// <summary>
    /// Spawns sensors using positions from the state.
    /// Called by host immediately after setting positions, or by clients when receiving positions.
    /// </summary>
    private void SpawnSensorsFromPositions(ZoneSensorPositionState positionState, bool isRecall)
    {
        if (sensorsSpawned)
        {
            Plugin.Logger.LogDebug($"ZoneSensorGroup {GroupIndex}: Sensors already spawned, skipping");
            return;
        }

        if (pendingZone == null || pendingGroupDefinitions == null || pendingGroupDefinitions.Count == 0)
        {
            Plugin.Logger.LogWarning($"ZoneSensorGroup {GroupIndex}: Missing pending spawn data");
            return;
        }

        if (!ZoneSensorAssets.AssetsLoaded)
        {
            Plugin.Logger.LogError($"ZoneSensorGroup {GroupIndex}: CircleSensor prefab not loaded!");
            return;
        }

        sensorsSpawned = true;
        var zone = pendingZone;

        // Track position index across all group definitions
        int positionIndex = 0;

        foreach (var groupDef in pendingGroupDefinitions)
        {
            for (int i = 0; i < groupDef.Count && positionIndex < positionState.SensorCount; i++)
            {
                var position = positionState.GetPosition(positionIndex);
                int waypointCount = positionState.GetWaypointCount(positionIndex);

                var sensorGO = CreateSensorVisual(position, groupDef, GroupIndex, positionIndex);
                Sensors.Add(sensorGO);

                // Add movement if enabled
                if (groupDef.Moving > 1)
                {
                    // Generate waypoints deterministically using position index as seed factor
                    InitializeSensorMovement(zone, groupDef, sensorGO, position, GroupIndex, positionIndex);
                }

                positionIndex++;
            }
        }

        Plugin.Logger.LogDebug($"ZoneSensorGroup {GroupIndex}: Spawned {Sensors.Count} sensors (isRecall={isRecall})");

        // Clear pending data
        pendingZone = null;
        pendingGroupDefinitions = null;
    }

    /// <summary>
    /// Creates the visual representation of a sensor.
    /// </summary>
    private GameObject CreateSensorVisual(Vector3 position, ZoneSensorGroupDefinition groupDef, int groupIndex, int sensorIndex)
    {
        // Instantiate CircleSensor prefab
        var sensorGO = UnityEngine.Object.Instantiate(ZoneSensorAssets.CircleSensor);
        sensorGO.name = $"ZoneSensor_{groupIndex}_{sensorIndex}";

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
        collider.GroupIndex = groupIndex;
        collider.SensorIndex = sensorIndex;
        collider.TriggerEach = groupDef.TriggerEach;
        collider.Radius = (float)groupDef.Radius;

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
        int textIndex = (groupIndex * 31 + sensorIndex * 17) % SensorTexts.Count;
        return SensorTexts[textIndex];
    }

    /// <summary>
    /// Initializes movement for a sensor using deterministic waypoint generation.
    /// Uses a per-sensor seed to ensure all clients generate identical paths.
    /// </summary>
    private void InitializeSensorMovement(LG_Zone zone, ZoneSensorGroupDefinition groupDef, GameObject sensorGO, Vector3 startPosition, int groupIndex, int sensorIndex)
    {
        var positions = new List<Vector3> { startPosition };

        // Create deterministic random for this specific sensor
        // Combines session seed with group/sensor indices for uniqueness
        int sensorSeed = Builder.SessionSeedRandom.Seed + groupIndex * 1000 + sensorIndex * 100;
        var sensorRandom = new System.Random(sensorSeed);

        // Generate (Moving - 1) additional random positions
        for (int i = 1; i < groupDef.Moving; i++)
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
        mover.Initialize(positions, (float)groupDef.Speed, (float)groupDef.EdgeDistance);
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
        float randomValue = (float)random.NextDouble();

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
    public void SetEnabled(bool enabled)
    {
        // Reset mask when enabling (all sensors become active again)
        if (enabled)
            currentSensorMask = uint.MaxValue;

        // Update visuals immediately for responsive feel
        UpdateVisualsUnsynced(enabled, currentSensorMask);

        // Sync to clients (only master can broadcast)
        if (Replicator != null && Replicator.IsValid)
        {
            Replicator.SetState(new ZoneSensorGroupState(enabled, currentSensorMask));
        }
    }

    /// <summary>
    /// Disables a single sensor and syncs to all clients.
    /// Used when TriggerEach mode is enabled.
    /// Note: Maximum of 32 sensors per group due to bitmask limitation.
    /// Sensors beyond index 31 will be silently ignored.
    /// </summary>
    public void DisableSensor(int sensorIndex)
    {
        if (sensorIndex < 0 || sensorIndex >= Sensors.Count || sensorIndex >= 32)
            return;

        currentSensorMask &= ~(1u << sensorIndex);

        // Update local visual immediately
        var sensor = Sensors[sensorIndex];
        sensor?.SetActive(false);

        // Sync to all clients
        if (Replicator != null && Replicator.IsValid)
        {
            Replicator.SetState(new ZoneSensorGroupState(Enabled, currentSensorMask));
        }
    }

    /// <summary>
    /// Callback for network state changes.
    /// </summary>
    private void OnStateChanged(ZoneSensorGroupState oldState, ZoneSensorGroupState newState, bool isRecall)
    {
        currentSensorMask = newState.SensorMask;
        UpdateVisualsUnsynced(newState.Enabled, newState.SensorMask);
    }

    /// <summary>
    /// Updates the local visual state without network sync.
    /// </summary>
    private void UpdateVisualsUnsynced(bool enabled, uint sensorMask)
    {
        Enabled = enabled;

        for (int i = 0; i < Sensors.Count; i++)
        {
            var sensor = Sensors[i];
            if (sensor != null)
            {
                // Sensor is visible if group enabled AND individual sensor enabled
                bool sensorEnabled = enabled && ((sensorMask & (1u << i)) != 0);
                sensor.SetActive(sensorEnabled);
            }
        }
    }

    /// <summary>
    /// Cleans up all sensor objects.
    /// </summary>
    public void Cleanup()
    {
        // Unload replicators (though Session lifetime auto-cleans)
        Replicator?.Unload();
        Replicator = null;
        PositionReplicator?.Unload();
        PositionReplicator = null;

        foreach (var sensor in Sensors)
        {
            if (sensor != null)
                UnityEngine.Object.Destroy(sensor);
        }
        Sensors.Clear();

        // Clear pending data
        pendingZone = null;
        pendingGroupDefinitions = null;
        sensorsSpawned = false;
    }
}
