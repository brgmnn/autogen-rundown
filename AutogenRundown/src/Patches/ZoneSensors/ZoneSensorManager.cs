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
using UnityEngine.AI;

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
            // Determine if any sensor group uses TriggerEach mode
            bool hasTriggerEach = definition.SensorGroups.Any(g => g.TriggerEach);
            var sensorGroup = new ZoneSensorGroup();
            sensorGroup.Initialize(groupIndex, definition.EventsOnTrigger, hasTriggerEach);

            // Spawn sensors for each group definition
            // Track sensor index across all group definitions
            int sensorIndex = 0;
            foreach (var groupDef in definition.SensorGroups)
            {
                sensorIndex = SpawnSensorsInZone(zone, groupDef, sensorGroup, groupIndex, sensorIndex);
            }

            activeSensorGroups.Add(sensorGroup);
            groupIndex++;
        }

        Plugin.Logger.LogDebug($"ZoneSensor: Built {activeSensorGroups.Count} sensor groups");
    }

    /// <summary>
    /// Spawns sensors within a zone based on the group definition.
    /// Returns the next sensor index to use.
    /// </summary>
    private int SpawnSensorsInZone(LG_Zone zone, ZoneSensorGroupDefinition groupDef, ZoneSensorGroup sensorGroup, int groupIndex, int startingSensorIndex)
    {
        const int maxPlacementAttempts = 5;
        int sensorIndex = startingSensorIndex;
        var placedSensors = new List<(Vector3 pos, float radius)>();
        var sensorRadius = (float)groupDef.Radius;

        for (var i = 0; i < groupDef.Count; i++)
        {
            Vector3 position = Vector3.zero;
            var attempts = 0;

            while (attempts < maxPlacementAttempts)
            {
                if (groupDef.AreaIndex >= 0 && groupDef.AreaIndex < zone.m_areas.Count)
                {
                    var area = zone.m_areas[groupDef.AreaIndex];
                    position = area.m_courseNode.GetRandomPositionInside();
                }
                else
                {
                    var randomAreaIndex = UnityEngine.Random.Range(0, zone.m_areas.Count);
                    var area = zone.m_areas[randomAreaIndex];
                    position = area.m_courseNode.GetRandomPositionInside();
                }

                if (!OverlapsExistingSensor(position, sensorRadius, placedSensors))
                    break;

                attempts++;
            }

            // Place sensor at final position (either non-overlapping or after max attempts)
            placedSensors.Add((position, sensorRadius));
            var sensorGO = CreateSensorVisual(position, groupDef, groupIndex, sensorIndex);
            sensorGroup.AddSensor(sensorGO);

            // Add movement if enabled (Moving > 1 means patrol between multiple points)
            if (groupDef.Moving > 1)
            {
                InitializeSensorMovement(zone, groupDef, sensorGO, position);
            }

            sensorIndex++;
        }

        return sensorIndex;
    }

    /// <summary>
    /// Initializes movement for a sensor by calculating paths through multiple patrol points.
    /// </summary>
    private void InitializeSensorMovement(LG_Zone zone, ZoneSensorGroupDefinition groupDef, GameObject sensorGO, Vector3 startPosition)
    {
        var positions = new List<Vector3> { startPosition };

        // Generate (Moving - 1) additional random positions
        for (int i = 1; i < groupDef.Moving; i++)
        {
            Vector3 pos;
            if (groupDef.AreaIndex >= 0 && groupDef.AreaIndex < zone.m_areas.Count)
            {
                var area = zone.m_areas[groupDef.AreaIndex];
                pos = area.m_courseNode.GetRandomPositionInside();
            }
            else
            {
                var randomAreaIndex = UnityEngine.Random.Range(0, zone.m_areas.Count);
                var area = zone.m_areas[randomAreaIndex];
                pos = area.m_courseNode.GetRandomPositionInside();
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
    /// Attempts to snap a position to the NavMesh.
    /// </summary>
    private bool TryGetPosOnNavMesh(ref Vector3 pos)
    {
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(pos + Vector3.up * 0.15f, out hit, 1f, -1))
            return false;
        pos = hit.position;
        return true;
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
    /// Creates the visual representation of a sensor.
    /// </summary>
    private GameObject CreateSensorVisual(Vector3 position, ZoneSensorGroupDefinition groupDef, int groupIndex, int sensorIndex)
    {
        if (!ZoneSensorAssets.AssetsLoaded)
        {
            Plugin.Logger.LogError("ZoneSensor: CircleSensor prefab not loaded!");
            return new GameObject($"ZoneSensor_{groupIndex}_{sensorIndex}_Error");
        }

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
                        animator.Initialize(groupDef.Text, normalColor, encryptedColor, text);
                    }
                    else
                    {
                        text.SetText(groupDef.Text);
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
