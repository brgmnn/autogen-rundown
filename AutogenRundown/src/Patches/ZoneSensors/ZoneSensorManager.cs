using AmorLib.Networking.StateReplicators;
using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using AutogenRundown.DataBlocks.Custom.ZoneSensors;
using AutogenRundown.DataBlocks.Objectives;
using GameData;
using GTFO.API;
using GTFO.API.Resources;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;
using Localization;
using SNetwork;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Manages zone-based security sensors that are placed automatically within zones.
/// Sensors detect players and trigger configured events.
///
/// Network sync architecture:
/// - Host generates random positions during OnBuildDone
/// - Positions are synced via ZoneSensorPositionState replicator
/// - Clients/late joiners spawn sensors when receiving position state
/// - Enabled/disabled state synced via ZoneSensorGroupState replicator
/// </summary>
public sealed class ZoneSensorManager
{
    public static ZoneSensorManager Current { get; } = new();

    /// <summary>
    /// Definitions indexed by MainLevelLayout ID, loaded from JSON.
    /// </summary>
    private static Dictionary<uint, List<ZoneSensorDefinition>> definitions = new();

    /// <summary>
    /// Active sensor groups in the current level, keyed by definition Id.
    /// </summary>
    private Dictionary<int, ZoneSensorGroup> activeSensorGroups = new();

    /// <summary>
    /// Counter for allocating unique replicator IDs (separate from definition Id).
    /// </summary>
    private int nextReplicatorIndex = 0;

    // Update helper component instance
    private ZoneSensorManagerUpdater? updater;

    // Track synced player count for event-driven rebroadcast (host only)
    private int lastSyncedPlayerCount;

    // Repeated rebroadcast so slow clients have time to create replicators
    private float rebroadcastIntervalTimer;
    private int rebroadcastAttemptsRemaining;
    private const float REBROADCAST_INTERVAL = 5.0f;
    private const int REBROADCAST_ATTEMPTS = 6;

    private ZoneSensorManager()
    {
        LevelAPI.OnBuildDone += BuildSensors;
        LevelAPI.OnLevelCleanup += Clear;
    }

    /// <summary>
    /// Called every frame to update sensor groups (movement sync).
    /// </summary>
    internal void Update()
    {
        foreach (var group in activeSensorGroups.Values)
        {
            group.Update();
        }

        // Host: repeatedly re-broadcast position states when a new player joins.
        // A single delayed rebroadcast isn't enough — slow clients may not have
        // created their replicators yet. Fire multiple attempts over ~30s so at
        // least one arrives after the client's OnBuildDone runs.
        if (SNet.IsMaster)
        {
            var currentCount = SNet.Slots.PlayersSynchedWithGame.Count;
            if (currentCount > lastSyncedPlayerCount)
            {
                lastSyncedPlayerCount = currentCount;
                rebroadcastAttemptsRemaining = REBROADCAST_ATTEMPTS;
                rebroadcastIntervalTimer = REBROADCAST_INTERVAL;
            }

            if (rebroadcastAttemptsRemaining > 0)
            {
                rebroadcastIntervalTimer -= Time.deltaTime;
                if (rebroadcastIntervalTimer <= 0f)
                {
                    RebroadcastStates();
                    rebroadcastAttemptsRemaining--;

                    if (rebroadcastAttemptsRemaining > 0)
                        rebroadcastIntervalTimer = REBROADCAST_INTERVAL;
                }
            }
        }
    }

    /// <summary>
    /// Checks if a sensor group is currently enabled.
    /// </summary>
    public bool IsGroupEnabled(int definitionId)
    {
        return activeSensorGroups.TryGetValue(definitionId, out var group) && group.Enabled;
    }

    /// <summary>
    /// Called when a sensor is triggered by a player.
    /// Executes the configured events for that sensor group.
    /// </summary>
    public void SensorTriggered(int definitionId)
    {
        if (!activeSensorGroups.TryGetValue(definitionId, out var group))
            return;

        // Only master should execute events
        if (!SNet.IsMaster)
            return;

        Plugin.Logger.LogDebug($"ZoneSensor: Group {definitionId} triggered, executing {group.EventsOnTrigger.Count} events");

        // Disable the sensor group after trigger
        group.SetEnabled(false);

        // Execute events
        foreach (var evt in group.EventsOnTrigger)
        {
            var eventData = ConvertToEventData(evt);
            WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(
                eventData,
                eWardenObjectiveEventTrigger.None,
                ignoreTrigger: true
            );
        }
    }

    /// <summary>
    /// Called when an individual sensor is triggered (TriggerEach mode).
    /// Disables only that sensor and executes the group's events.
    /// </summary>
    public void SensorTriggeredIndividual(int definitionId, int sensorIndex)
    {
        if (!activeSensorGroups.TryGetValue(definitionId, out var group))
            return;

        // Only master should execute events
        if (!SNet.IsMaster)
            return;

        Plugin.Logger.LogDebug($"ZoneSensor: Group {definitionId} sensor {sensorIndex} triggered individually");

        // Disable only this sensor
        group.DisableSensor(sensorIndex);

        // Execute events (same as group trigger)
        foreach (var evt in group.EventsOnTrigger)
        {
            var eventData = ConvertToEventData(evt);
            WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(
                eventData,
                eWardenObjectiveEventTrigger.None,
                ignoreTrigger: true
            );
        }
    }

    /// <summary>
    /// Converts autogen's WardenObjectiveEvent to the game's WardenObjectiveEventData.
    /// </summary>
    private static WardenObjectiveEventData ConvertToEventData(WardenObjectiveEvent evt)
    {
        var eventData = new WardenObjectiveEventData
        {
            Type = (eWardenObjectiveEventType)evt.Type,
            Trigger = eWardenObjectiveEventTrigger.None,
            Layer = (LG_LayerType)evt.Layer,
            DimensionIndex = (eDimensionIndex)evt.Dimension,
            LocalIndex = (eLocalZoneIndex)evt.LocalIndex,
            Delay = (float)evt.Delay,
            Duration = (float)evt.Duration,
            WardenIntel = ToLocalizedText(evt.WardenIntel),
            SoundID = (uint)evt.SoundId,
            DialogueID = evt.DialogueId,
            FogSetting = evt.FogSetting,
            FogTransitionDuration = (float)evt.FogTransitionDuration,
            ChainPuzzle = evt.ChainPuzzle,
            ClearDimension = evt.ClearDimension,
            UseStaticBioscanPoints = evt.UseStaticBioscanPoints,
            Count = evt.Count,
            Enabled = evt.Enabled
        };

        // Convert EnemyWaveData if present (check serialized uint, not [JsonIgnore] property)
        if (evt.EnemyWaveData != null && evt.EnemyWaveData.SurvivalWaveSettings != 0)
        {
            eventData.EnemyWaveData = new GenericEnemyWaveData
            {
                WaveSettings = evt.EnemyWaveData.SurvivalWaveSettings,
                WavePopulation = evt.EnemyWaveData.SurvivalWavePopulation,
                SpawnDelay = (float)evt.EnemyWaveData.SpawnDelay,
                TriggerAlarm = evt.EnemyWaveData.TriggerAlarm,
                IntelMessage = ToLocalizedText(evt.EnemyWaveData.IntelMessage)
            };
        }

        return eventData;
    }

    /// <summary>
    /// Converts a string to LocalizedText for the game's event system.
    /// </summary>
    private static LocalizedText ToLocalizedText(string text)
    {
        return new LocalizedText
        {
            Id = 0u,
            UntranslatedText = text ?? ""
        };
    }

    /// <summary>
    /// Toggles a sensor group on or off.
    /// </summary>
    /// <param name="definitionId">ID of the sensor group to toggle</param>
    /// <param name="enabled">Whether to enable or disable the group</param>
    /// <param name="preserveTriggered">When true, only re-enable sensors that haven't been triggered</param>
    /// <param name="resetTriggered">When true, clear triggered state before enabling (all sensors reappear)</param>
    public void ToggleSensorGroup(int definitionId, bool enabled, bool preserveTriggered = false, bool resetTriggered = false)
    {
        if (!activeSensorGroups.TryGetValue(definitionId, out var group))
            return;

        group.SetEnabled(enabled, preserveTriggered, resetTriggered);

        // Note: ResetState is now called inside UpdateVisualsUnsynced for each sensor

        Plugin.Logger.LogDebug($"ZoneSensor: Group {definitionId} set to {(enabled ? "enabled" : "disabled")} (preserveTriggered={preserveTriggered}, resetTriggered={resetTriggered})");
    }

    /// <summary>
    /// Finds all definition IDs for sensor groups in the specified zone.
    /// </summary>
    /// <param name="dimension">The dimension to search in</param>
    /// <param name="layer">The layer type to search in</param>
    /// <param name="zoneIndex">The local zone index to search in</param>
    /// <returns>List of definition IDs matching the zone criteria</returns>
    public List<int> GetIdsForZone(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex zoneIndex)
    {
        return activeSensorGroups.Values
            .Where(g => g.DimensionIndex == dimension && g.LayerType == layer && g.LocalZoneIndex == zoneIndex)
            .Select(g => g.Id)
            .ToList();
    }

    /// <summary>
    /// Called when level build is complete. Creates sensor groups and spawns sensors.
    ///
    /// Host: Generates random positions and syncs via position replicator.
    /// Clients: Create groups with pending data, spawn on receiving positions.
    /// Late joiners: Receive position state via recall, spawn sensors.
    /// </summary>
    private void BuildSensors()
    {
        var levelLayoutId = RundownManager.ActiveExpedition.LevelLayoutData;

        if (!definitions.TryGetValue(levelLayoutId, out var levelDefinitions))
            return;

        // Safety check: NetworkAPI must be ready before creating StateReplicators.
        // By OnBuildDone, NetworkAPI should always be ready (created during GameDataInit).
        // If this triggers, something is fundamentally wrong with initialization order.
        if (!APIStatus.Network.Ready)
        {
            Plugin.Logger.LogError("ZoneSensor: NetworkAPI not ready during BuildSensors! Cannot create sensors.");
            return;
        }

        Plugin.Logger.LogDebug($"ZoneSensor: Building sensors for level {levelLayoutId}, {levelDefinitions.Count} definitions, IsMaster={SNet.IsMaster}");

        // Reset replicator index counter for this level
        nextReplicatorIndex = 0;

        foreach (var definition in levelDefinitions)
        {
            // Get the zone
            var dimensionIndex = definition.DimensionIndex switch
            {
                "Reality" => eDimensionIndex.Reality,
                "Dimension_1" => eDimensionIndex.Dimension_1,
                "Dimension_2" => eDimensionIndex.Dimension_2,
                _ => eDimensionIndex.Reality
            };

            var layerType = definition.LayerType switch
            {
                "MainLayer" => LG_LayerType.MainLayer,
                "SecondaryLayer" => LG_LayerType.SecondaryLayer,
                "ThirdLayer" => LG_LayerType.ThirdLayer,
                _ => LG_LayerType.MainLayer
            };

            var localZoneIndex = (eLocalZoneIndex)(definition.LocalIndex ?? 0);

            if (!Builder.CurrentFloor.TryGetZoneByLocalIndex(dimensionIndex, layerType, localZoneIndex, out var zone))
            {
                Plugin.Logger.LogWarning($"ZoneSensor: Could not find zone {definition.LocalIndex} in layer {layerType}");
                continue;
            }

            // Calculate total sensor count for batch planning (uses density if specified)
            var totalSensors = definition.SensorGroups.Sum(g => GetEffectiveSensorCount(g, zone));
            var batchCount = ZoneSensorPositionState.CalculateBatchCount(totalSensors);

            // Warn if we're hitting the maximum supported sensors (128 = 8 batches * 16 per batch)
            const int maxSupportedSensors = 8 * ZoneSensorPositionState.MaxSensorsPerBatch;
            if (totalSensors > maxSupportedSensors)
            {
                Plugin.Logger.LogWarning($"ZoneSensor: Group {definition.Id} has {totalSensors} sensors, exceeds max {maxSupportedSensors}. Some sensors will not be created.");
                totalSensors = maxSupportedSensors;
            }

            Plugin.Logger.LogDebug($"ZoneSensor: Group {definition.Id} will have {totalSensors} sensors in {batchCount} batches");

            // Determine if any sensor group uses TriggerEach mode
            var hasTriggerEach = definition.SensorGroups.Any(g => g.TriggerEach);

            // Determine if any sensor in the group is moving
            var hasMovingSensors = definition.SensorGroups.Any(g => g.Moving > 1);

            // Create sensor group with replicators (pass expected count for batch allocation)
            var sensorGroup = new ZoneSensorGroup();
            sensorGroup.Initialize(nextReplicatorIndex++, definition.Id, definition.EventsOnTrigger, hasTriggerEach,
                totalSensors, hasMovingSensors, dimensionIndex, layerType, localZoneIndex, definition.StartEnabled);

            // Set pending spawn data (sensors will spawn when positions are received)
            sensorGroup.SetPendingSpawnData(zone, definition.SensorGroups);

            // Host generates random positions and triggers spawning on all clients
            if (SNet.IsMaster)
            {
                var positionBatches = GeneratePositionBatches(zone, definition.SensorGroups);
                sensorGroup.SetPositionsAndSpawn(positionBatches);
            }
            // Clients wait for OnPositionStateChanged callback to spawn

            activeSensorGroups[definition.Id] = sensorGroup;
        }

        // Create updater component if we have any active groups
        // Needed for movement sync and position re-broadcast for slow clients
        if (activeSensorGroups.Count > 0)
        {
            EnsureUpdaterExists();

            // Initialize synced player count (host only)
            if (SNet.IsMaster)
            {
                lastSyncedPlayerCount = SNet.Slots.PlayersSynchedWithGame.Count;
            }
        }

        Plugin.Logger.LogDebug($"ZoneSensor: Created {activeSensorGroups.Count} sensor groups");
    }

    /// <summary>
    /// Ensures the updater component exists for movement sync.
    /// </summary>
    private void EnsureUpdaterExists()
    {
        if (updater != null)
            return;

        var go = new GameObject("ZoneSensorManagerUpdater");
        UnityEngine.Object.DontDestroyOnLoad(go);
        updater = go.AddComponent<ZoneSensorManagerUpdater>();
        Plugin.Logger.LogDebug("ZoneSensor: Created updater component for movement sync");
    }

    /// <summary>
    /// Re-broadcasts all replicator states for all groups so slow clients can receive them.
    /// Host only. Safe because all rebroadcasts are idempotent.
    /// </summary>
    private void RebroadcastStates()
    {
        Plugin.Logger.LogDebug($"ZoneSensor: Re-broadcasting all states (syncedPlayers={lastSyncedPlayerCount})");

        foreach (var group in activeSensorGroups.Values)
        {
            group.RebroadcastAllStates();
        }
    }

    /// <summary>
    /// Generates random positions for sensors within a zone, split into batches.
    /// Host-only: Uses SessionSeedRandom for deterministic generation.
    /// Returns a list of position state batches, each containing up to 16 sensors.
    /// </summary>
    private List<ZoneSensorPositionState> GeneratePositionBatches(LG_Zone zone, List<ZoneSensorGroupDefinition> groupDefinitions)
    {
        // First, generate all positions
        var allPositions = new List<(Vector3 pos, int waypointCount)>();
        var placedSensors = new List<(Vector3 pos, float radius)>();

        // Maximum sensors supported (8 batches * 16 per batch)
        const int maxTotalSensors = 8 * ZoneSensorPositionState.MaxSensorsPerBatch;

        foreach (var groupDef in groupDefinitions)
        {
            var sensorRadius = (float)groupDef.Radius;
            var effectiveCount = GetEffectiveSensorCount(groupDef, zone);

            for (var i = 0; i < effectiveCount && allPositions.Count < maxTotalSensors; i++)
            {
                const int maxPlacementAttempts = 5;
                var position = Vector3.zero;
                var attempts = 0;

                while (attempts < maxPlacementAttempts)
                {
                    if (groupDef.AreaIndex >= 0 && groupDef.AreaIndex < zone.m_areas.Count)
                    {
                        var area = zone.m_areas[groupDef.AreaIndex];
                        position = area.m_courseNode.GetRandomPositionInside_SessionSeed();
                    }
                    else
                    {
                        var randomAreaIndex = Builder.SessionSeedRandom.Range(0, zone.m_areas.Count, "ZoneSensor_AreaSelect");
                        var area = zone.m_areas[randomAreaIndex];
                        position = area.m_courseNode.GetRandomPositionInside_SessionSeed();
                    }

                    if (!OverlapsExistingSensor(position, sensorRadius, placedSensors)
                        && !IsNearOrigin(position))
                        break;

                    attempts++;
                }

                placedSensors.Add((position, sensorRadius));

                // Store waypoint count for moving sensors
                var waypointCount = groupDef.Moving > 1 ? Math.Min(groupDef.Moving - 1, 3) : 0;
                allPositions.Add((position, waypointCount));
            }
        }

        // Split positions into batches of MaxSensorsPerBatch
        var totalBatches = ZoneSensorPositionState.CalculateBatchCount(allPositions.Count);
        var batches = new List<ZoneSensorPositionState>();

        for (var batchIndex = 0; batchIndex < totalBatches; batchIndex++)
        {
            var state = new ZoneSensorPositionState
            {
                BatchIndex = (byte)batchIndex,
                TotalBatches = (byte)totalBatches
            };

            var startIndex = batchIndex * ZoneSensorPositionState.MaxSensorsPerBatch;
            var endIndex = Math.Min(startIndex + ZoneSensorPositionState.MaxSensorsPerBatch, allPositions.Count);
            var batchSensorCount = endIndex - startIndex;

            for (var i = 0; i < batchSensorCount; i++)
            {
                var (pos, waypointCount) = allPositions[startIndex + i];
                state.SetPosition(i, pos);
                state.SetWaypointCount(i, waypointCount);
            }

            state.SensorCount = (byte)batchSensorCount;
            batches.Add(state);
        }

        Plugin.Logger.LogDebug($"ZoneSensor: Generated {allPositions.Count} positions in {totalBatches} batches");

        return batches;
    }

    /// <summary>
    /// Checks if a position overlaps with any existing sensor positions.
    /// </summary>
    private static bool IsNearOrigin(Vector3 position)
    {
        return position.x > -15f && position.x < 15f
            && position.z > -15f && position.z < 15f;
    }

    private bool OverlapsExistingSensor(Vector3 position, float radius, List<(Vector3 pos, float radius)> existingSensors)
    {
        foreach (var (existingPos, existingRadius) in existingSensors)
        {
            var distance = Vector3.Distance(position, existingPos);
            var minDistance = radius + existingRadius;

            if (distance < minDistance)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Calculates sensor count based on zone's voxel coverage and density setting.
    /// VoxelCoverage represents actual traversable area from the AI graph nodes.
    /// </summary>
    /// <param name="zone">The zone to calculate coverage for</param>
    /// <param name="density">The density setting to use</param>
    /// <returns>Calculated sensor count, clamped between 1 and 128</returns>
    private int CalculateSensorCountFromDensity(LG_Zone zone, SensorDensity density, double radius)
    {
        // Sum VoxelCoverage from all areas - this is the actual walkable area
        var totalCoverage = 0f;

        foreach (var area in zone.m_areas)
        {
            totalCoverage += area.m_courseNode?.m_nodeCluster?.CalculateVoxelCoverage(0.9f) ?? 0f;
        }

        // Sensors per 100 units of voxel coverage
        // VoxelCoverage is derived from AI graph node count, not raw area
        var sensorsPerHundredCoverage = density switch
        {
            SensorDensity.Low => 1.5f,      // ~1.5 sensors per 100 coverage per unit radius
            SensorDensity.Medium => 3.0f,   // ~3.0 sensors per 100 coverage per unit radius
            SensorDensity.High => 4.5f,     // ~4.5 sensors per 100 coverage per unit radius
            SensorDensity.VeryHigh => 6.0f, // ~6.0 sensors per 100 coverage per unit radius
            _ => 6.0f
        };

        // Larger radius sensors cover more area, so fewer are needed for same density
        var rawCount = (int)(totalCoverage / 100f * sensorsPerHundredCoverage / radius);

        Plugin.Logger.LogDebug($"ZoneSensor density calc: zone has {zone.m_areas.Count} areas, " +
            $"totalCoverage={totalCoverage:F1}, density={density}, count={rawCount}");

        return Math.Clamp(rawCount, 1, 128);
    }

    /// <summary>
    /// Gets the effective sensor count for a group definition, either from explicit Count
    /// or calculated from zone density.
    /// </summary>
    private int GetEffectiveSensorCount(ZoneSensorGroupDefinition groupDef, LG_Zone zone)
    {
        if (groupDef.Density != SensorDensity.None)
        {
            return CalculateSensorCountFromDensity(zone, groupDef.Density, groupDef.Radius);
        }

        return groupDef.Count;
    }

    /// <summary>
    /// Cleans up all sensors on level cleanup.
    /// </summary>
    private void Clear()
    {
        foreach (var group in activeSensorGroups.Values)
        {
            group.Cleanup();
        }

        activeSensorGroups.Clear();
        nextReplicatorIndex = 0;
        lastSyncedPlayerCount = 0;
        rebroadcastIntervalTimer = 0f;
        rebroadcastAttemptsRemaining = 0;

        // Destroy updater component
        if (updater != null)
        {
            UnityEngine.Object.Destroy(updater.gameObject);
            updater = null;
        }

        // Clear any pending scheduled toggles to prevent stale toggles across levels
        ZoneSensorToggleScheduler.ClearPending();

        Plugin.Logger.LogDebug("ZoneSensor: Cleared all sensors");
    }

    /// <summary>
    /// Loads zone sensor definitions from JSON files.
    /// Called during game data initialization.
    /// </summary>
    public static void Setup()
    {
        // Clear any previous definitions
        definitions.Clear();

        // Load all zone sensor definitions from JSON
        var allLevelSensors = LevelZoneSensors.LoadAll();

        foreach (var levelSensors in allLevelSensors)
        {
            if (!definitions.ContainsKey(levelSensors.MainLevelLayout))
                definitions[levelSensors.MainLevelLayout] = new List<ZoneSensorDefinition>();

            definitions[levelSensors.MainLevelLayout].AddRange(levelSensors.Definitions);
        }

        // Ensure singleton is created to hook up level events
        _ = Current;

        // Pre-register state replicators to handle late joiner edge case
        PreRegisterStateReplicators();

        Plugin.Logger.LogDebug($"ZoneSensorManager: Loaded {allLevelSensors.Count} level definitions");
    }

    /// <summary>
    /// Pre-registers all StateReplicator event handlers to prevent late joiner issues.
    /// Creating a replicator instance triggers the static constructor which registers
    /// the network event handler. This must happen before any late joiner can receive
    /// recall events.
    /// </summary>
    private static void PreRegisterStateReplicators()
    {
        // NetworkAPI may not be ready during early initialization (OnGameDataInitialized fires
        // BEFORE NetworkAPI_Impl is created in GTFO-API's GameDataInit_Patches).
        // This is expected - skip pre-registration here; handlers will be created when
        // Initialize() runs during OnBuildDone (when NetworkAPI is definitely ready).
        if (!APIStatus.Network.Ready)
        {
            Plugin.Logger.LogDebug("ZoneSensorManager: NetworkAPI not ready, skipping pre-registration (expected on startup)");
            return;
        }

        // Reserved IDs that won't conflict with actual replicators (0x5A53FF00-FF range)
        const uint DUMMY_BASE_ID = 0x5A53FF00;

        // Pre-register ZoneSensorGroupState events
        var groupDummy = StateReplicator<ZoneSensorGroupState>.Create(
            DUMMY_BASE_ID,
            new ZoneSensorGroupState(),
            LifeTimeType.Session
        );
        groupDummy?.Unload();

        // Pre-register ZoneSensorPositionState events
        var positionDummy = StateReplicator<ZoneSensorPositionState>.Create(
            DUMMY_BASE_ID + 1,
            new ZoneSensorPositionState(),
            LifeTimeType.Session
        );
        positionDummy?.Unload();

        // Pre-register ZoneSensorWaypointState events
        var waypointDummy = StateReplicator<ZoneSensorWaypointState>.Create(
            DUMMY_BASE_ID + 2,
            new ZoneSensorWaypointState(),
            LifeTimeType.Session
        );
        waypointDummy?.Unload();

        // Pre-register ZoneSensorMovementState events
        var movementDummy = StateReplicator<ZoneSensorMovementState>.Create(
            DUMMY_BASE_ID + 3,
            new ZoneSensorMovementState(),
            LifeTimeType.Session
        );
        movementDummy?.Unload();

        Plugin.Logger.LogDebug("ZoneSensorManager: Pre-registered all state replicator event handlers");
    }
}

/// <summary>
/// MonoBehaviour helper that calls ZoneSensorManager.Update() every frame.
/// Used for periodic movement sync of moving sensors.
/// </summary>
public class ZoneSensorManagerUpdater : MonoBehaviour
{
    void Update()
    {
        ZoneSensorManager.Current.Update();
    }

    static ZoneSensorManagerUpdater()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ZoneSensorManagerUpdater>();
    }
}
