using AIGraph;
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
    /// Called when level build is complete. Spawns all registered sensors.
    /// </summary>
    private void BuildSensors()
    {
        var levelLayoutId = RundownManager.ActiveExpedition.LevelLayoutData;

        if (!definitions.TryGetValue(levelLayoutId, out var levelDefinitions))
            return;

        Plugin.Logger.LogDebug($"ZoneSensor: Building sensors for level {levelLayoutId}, {levelDefinitions.Count} definitions");

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

            // Create sensor group
            var sensorGroup = new ZoneSensorGroup();
            sensorGroup.Initialize(groupIndex, definition.EventsOnTrigger);

            // Spawn sensors for each group definition
            foreach (var groupDef in definition.SensorGroups)
            {
                SpawnSensorsInZone(zone, groupDef, sensorGroup, groupIndex);
            }

            activeSensorGroups.Add(sensorGroup);
            groupIndex++;
        }

        Plugin.Logger.LogDebug($"ZoneSensor: Built {activeSensorGroups.Count} sensor groups");
    }

    /// <summary>
    /// Spawns sensors within a zone based on the group definition.
    /// </summary>
    private void SpawnSensorsInZone(LG_Zone zone, ZoneSensorGroupDefinition groupDef, ZoneSensorGroup sensorGroup, int groupIndex)
    {
        for (var i = 0; i < groupDef.Count; i++)
        {
            // Get position from zone's navigation mesh
            Vector3 position;

            if (groupDef.AreaIndex >= 0 && groupDef.AreaIndex < zone.m_areas.Count)
            {
                // Use specific area
                var area = zone.m_areas[groupDef.AreaIndex];
                position = area.m_courseNode.GetRandomPositionInside();
            }
            else
            {
                // Use random area
                var randomAreaIndex = UnityEngine.Random.Range(0, zone.m_areas.Count);
                var area = zone.m_areas[randomAreaIndex];
                position = area.m_courseNode.GetRandomPositionInside();
            }

            // Create sensor GameObject
            var sensorGO = CreateSensorVisual(position, groupDef, groupIndex);
            sensorGroup.AddSensor(sensorGO);
        }
    }

    /// <summary>
    /// Creates the visual representation of a sensor.
    /// </summary>
    private GameObject CreateSensorVisual(Vector3 position, ZoneSensorGroupDefinition groupDef, int groupIndex)
    {
        if (!ZoneSensorAssets.AssetsLoaded)
        {
            Plugin.Logger.LogError("ZoneSensor: CircleSensor prefab not loaded!");
            return new GameObject($"ZoneSensor_{groupIndex}_Error");
        }

        // Instantiate CircleSensor prefab
        var sensorGO = UnityEngine.Object.Instantiate(ZoneSensorAssets.CircleSensor);
        sensorGO.name = $"ZoneSensor_{groupIndex}";

        // Position and scale per EOS pattern
        sensorGO.transform.SetPositionAndRotation(position, Quaternion.identity);
        float height = 0.6f / 3.7f;
        sensorGO.transform.localScale = new Vector3(
            (float)groupDef.Radius,
            (float)groupDef.Radius,
            (float)groupDef.Radius);
        sensorGO.transform.localPosition += Vector3.up * height;

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

        // Add detection collider
        var collider = sensorGO.AddComponent<ZoneSensorCollider>();
        collider.GroupIndex = groupIndex;
        collider.Radius = (float)groupDef.Radius;

        Plugin.Logger.LogDebug($"ZoneSensor: Created sensor at {position} with radius {groupDef.Radius}");
        return sensorGO;
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
