using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using AutogenRundown.DataBlocks.Custom.ZoneSensors;
using AutogenRundown.DataBlocks.Objectives;
using GameData;
using GTFO.API;
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
    /// Active sensor groups in the current level.
    /// </summary>
    private List<ZoneSensorGroup> activeSensorGroups = new();

    private ZoneSensorManager()
    {
        LevelAPI.OnBuildDone += BuildSensors;
        LevelAPI.OnLevelCleanup += Clear;
    }

    /// <summary>
    /// Checks if a sensor group is currently enabled.
    /// </summary>
    public bool IsGroupEnabled(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= activeSensorGroups.Count)
            return false;

        return activeSensorGroups[groupIndex].Enabled;
    }

    /// <summary>
    /// Called when a sensor is triggered by a player.
    /// Executes the configured events for that sensor group.
    /// </summary>
    public void SensorTriggered(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= activeSensorGroups.Count)
            return;

        var group = activeSensorGroups[groupIndex];

        // Only master should execute events
        if (!SNet.IsMaster)
            return;

        Plugin.Logger.LogDebug($"ZoneSensor: Group {groupIndex} triggered, executing {group.EventsOnTrigger.Count} events");

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
    public void SensorTriggeredIndividual(int groupIndex, int sensorIndex)
    {
        if (groupIndex < 0 || groupIndex >= activeSensorGroups.Count)
            return;

        var group = activeSensorGroups[groupIndex];

        // Only master should execute events
        if (!SNet.IsMaster)
            return;

        Plugin.Logger.LogDebug($"ZoneSensor: Group {groupIndex} sensor {sensorIndex} triggered individually");

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
    public void ToggleSensorGroup(int groupIndex, bool enabled)
    {
        if (groupIndex < 0 || groupIndex >= activeSensorGroups.Count)
            return;

        var group = activeSensorGroups[groupIndex];
        group.SetEnabled(enabled);

        // Reset collider states when re-enabling
        if (enabled)
        {
            foreach (var sensor in group.Sensors)
            {
                var collider = sensor?.GetComponent<ZoneSensorCollider>();
                collider?.ResetState();
            }
        }

        Plugin.Logger.LogDebug($"ZoneSensor: Group {groupIndex} set to {(enabled ? "enabled" : "disabled")}");
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

        Plugin.Logger.LogDebug($"ZoneSensor: Building sensors for level {levelLayoutId}, {levelDefinitions.Count} definitions, IsMaster={SNet.IsMaster}");

        var groupIndex = 0;

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

            // Check total sensor count doesn't exceed position state limit
            int totalSensors = definition.SensorGroups.Sum(g => g.Count);
            if (totalSensors > ZoneSensorPositionState.MaxSensors)
            {
                Plugin.Logger.LogWarning($"ZoneSensor: Group {groupIndex} has {totalSensors} sensors, exceeds max {ZoneSensorPositionState.MaxSensors}. Some sensors will not be created.");
            }

            // Determine if any sensor group uses TriggerEach mode
            bool hasTriggerEach = definition.SensorGroups.Any(g => g.TriggerEach);

            // Create sensor group with replicators
            var sensorGroup = new ZoneSensorGroup();
            sensorGroup.Initialize(groupIndex, definition.EventsOnTrigger, hasTriggerEach);

            // Set pending spawn data (sensors will spawn when positions are received)
            sensorGroup.SetPendingSpawnData(zone, definition.SensorGroups);

            // Host generates random positions and triggers spawning on all clients
            if (SNet.IsMaster)
            {
                var positionState = GeneratePositions(zone, definition.SensorGroups);
                sensorGroup.SetPositionsAndSpawn(positionState);
            }
            // Clients wait for OnPositionStateChanged callback to spawn

            activeSensorGroups.Add(sensorGroup);
            groupIndex++;
        }

        Plugin.Logger.LogDebug($"ZoneSensor: Created {activeSensorGroups.Count} sensor groups");
    }

    /// <summary>
    /// Generates random positions for sensors within a zone.
    /// Host-only: Uses SessionSeedRandom for deterministic generation.
    /// </summary>
    private ZoneSensorPositionState GeneratePositions(LG_Zone zone, List<ZoneSensorGroupDefinition> groupDefinitions)
    {
        var state = new ZoneSensorPositionState();
        var placedSensors = new List<(Vector3 pos, float radius)>();
        int positionIndex = 0;

        foreach (var groupDef in groupDefinitions)
        {
            var sensorRadius = (float)groupDef.Radius;

            for (int i = 0; i < groupDef.Count && positionIndex < ZoneSensorPositionState.MaxSensors; i++)
            {
                const int maxPlacementAttempts = 5;
                Vector3 position = Vector3.zero;
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

                    if (!OverlapsExistingSensor(position, sensorRadius, placedSensors))
                        break;

                    attempts++;
                }

                placedSensors.Add((position, sensorRadius));
                state.SetPosition(positionIndex, position);

                // Store waypoint count for moving sensors (used by clients for deterministic generation)
                if (groupDef.Moving > 1)
                {
                    state.SetWaypointCount(positionIndex, Math.Min(groupDef.Moving - 1, 3));
                }

                positionIndex++;
            }
        }

        state.SensorCount = (byte)positionIndex;
        Plugin.Logger.LogDebug($"ZoneSensor: Generated {positionIndex} positions");

        return state;
    }

    /// <summary>
    /// Checks if a position overlaps with any existing sensor positions.
    /// </summary>
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
    /// Cleans up all sensors on level cleanup.
    /// </summary>
    private void Clear()
    {
        foreach (var group in activeSensorGroups)
        {
            group.Cleanup();
        }

        activeSensorGroups.Clear();

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

        Plugin.Logger.LogDebug($"ZoneSensorManager: Loaded {allLevelSensors.Count} level definitions");
    }
}
