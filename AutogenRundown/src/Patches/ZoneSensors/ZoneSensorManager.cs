using AIGraph;
using AutogenRundown.DataBlocks.Custom.ZoneSensors;
using AutogenRundown.DataBlocks.Objectives;
using GameData;
using GTFO.API;
using LevelGeneration;
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
    /// Definitions indexed by MainLevelLayout ID.
    /// </summary>
    private Dictionary<uint, List<ZoneSensorDefinition>> definitions = new();

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
    /// Registers a definition for a level.
    /// Called during level generation.
    /// </summary>
    public void RegisterDefinition(uint levelLayoutId, ZoneSensorDefinition definition)
    {
        if (!definitions.ContainsKey(levelLayoutId))
            definitions[levelLayoutId] = new List<ZoneSensorDefinition>();

        definitions[levelLayoutId].Add(definition);
    }

    /// <summary>
    /// Clears all definitions for a level.
    /// </summary>
    public void ClearDefinitions(uint levelLayoutId)
    {
        if (definitions.ContainsKey(levelLayoutId))
            definitions[levelLayoutId].Clear();
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

        // Execute events using the game's warden objective system
        // Note: This is a simplified approach. For full event support,
        // you may need to convert WardenObjectiveEvent to WardenObjectiveEventData
        foreach (var evt in group.EventsOnTrigger)
        {
            // Log the event for now - full implementation would convert and execute
            Plugin.Logger.LogDebug($"ZoneSensor: Would execute event type {evt.Type} with delay {evt.Delay}");
        }
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

        int groupIndex = 0;

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

            var layerType = definition.Bulkhead switch
            {
                Bulkhead.Main => LG_LayerType.MainLayer,
                Bulkhead.Extreme => LG_LayerType.SecondaryLayer,
                Bulkhead.Overload => LG_LayerType.ThirdLayer,
                _ => LG_LayerType.MainLayer
            };

            var localZoneIndex = (eLocalZoneIndex)definition.ZoneNumber;

            if (!Builder.CurrentFloor.TryGetZoneByLocalIndex(dimensionIndex, layerType, localZoneIndex, out var zone))
            {
                Plugin.Logger.LogWarning($"ZoneSensor: Could not find zone {definition.ZoneNumber} in layer {layerType}");
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
        for (int i = 0; i < groupDef.Count; i++)
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
        // Create parent object
        var sensorGO = new GameObject($"ZoneSensor_{groupIndex}");
        sensorGO.transform.position = position;

        // Add detection collider component
        var collider = sensorGO.AddComponent<ZoneSensorCollider>();
        collider.GroupIndex = groupIndex;
        collider.Radius = (float)groupDef.Radius;

        // Create visual ring (cylinder)
        var ringGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ringGO.name = "SensorRing";
        ringGO.transform.SetParent(sensorGO.transform);
        ringGO.transform.localPosition = Vector3.zero;

        // Scale cylinder to match radius (diameter = radius * 2)
        float diameter = (float)groupDef.Radius * 2f;
        ringGO.transform.localScale = new Vector3(diameter, 0.1f, diameter);

        // Remove the default collider from the primitive
        var primitiveCollider = ringGO.GetComponent<Collider>();
        if (primitiveCollider != null)
            UnityEngine.Object.Destroy(primitiveCollider);

        // Apply material with sensor color
        var renderer = ringGO.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // Create emissive material
            var material = new Material(Shader.Find("Standard"));
            var color = new UnityEngine.Color(
                (float)groupDef.Color.Red,
                (float)groupDef.Color.Green,
                (float)groupDef.Color.Blue,
                (float)groupDef.Color.Alpha);

            material.SetColor("_Color", color);
            material.SetColor("_EmissionColor", color * 2f);
            material.EnableKeyword("_EMISSION");
            material.renderQueue = 3000; // Transparent queue

            renderer.material = material;
        }

        // TODO: Add TextMeshPro for sensor text display
        // This requires TextMeshPro asset which may need additional setup

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
    /// Force initialization (call from Plugin.Load).
    /// </summary>
    public static void Setup()
    {
        // Access Current to ensure singleton is created
        _ = Current;
        Plugin.Logger.LogDebug("ZoneSensorManager initialized");
    }
}
