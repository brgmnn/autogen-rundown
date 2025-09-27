using AIGraph;
using Expedition;
using GameData;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;
using XXHashing;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class DimensionTerminalMarkers
{
    [HarmonyPatch(typeof(LG_FunctionMarkerBuilder), nameof(LG_FunctionMarkerBuilder.BuildFloorFallback))]
    [HarmonyPrefix]
    private static bool Prefix2(LG_FunctionMarkerBuilder __instance, LG_Zone zone, uint seed)
    {
        return true;

        // var zone = (LG_Zone)AccessTools.Field(typeof(LG_Distribute_TerminalsPerZone), "m_zone").GetValue(__instance);

        if (zone == null)
            return true;

        // Only change behavior for static dimensions
        var dim = zone.Layer?.m_dimension;

        if (dim == null || dim.DimensionData == null || !dim.DimensionData.IsStaticDimension)
            return true;

        var zoneData = zone.m_settings?.m_zoneData;
        var placements = zoneData?.TerminalPlacements;

        if (placements == null || placements.Count == 0)
            return true;

        // Build a “slot list” of areas that have terminal spawners (one slot per spawner)
        var areaSlots = new List<LG_Area>();
        foreach (var area in zone.m_areas)
        {
            int count = area.GetMarkerSpawnerCount(ExpeditionFunction.Terminal);
            for (int i = 0; i < count; i++)
                areaSlots.Add(area);
        }

        if (areaSlots.Count == 0)
        {
            // No static terminal spawners in this zone -> fallback to original behavior
            return true;
        }

        // Assign terminals to areas that actually have static spawners
        int total = Mathf.Min(placements.Count, areaSlots.Count);
        for (int i = 0; i < total; i++)
        {
            var p = placements[i];
            var area = areaSlots[i]; // simple deterministic assignment; shuffle if desired
            var node = area.m_courseNode;

            var dist = new LG_DistributeItem(ExpeditionFunction.Terminal, 1f, node)
            {
                m_localTerminalLogFiles = p.LocalLogFiles,
                m_terminalStartStateData = p.StartingStateData,
                m_terminalPlacementData = p,
                m_markerSeedOffset = p.MarkerSeedOffset
            };

            zone.DistributionData.GenericFunctionItems.Enqueue(dist);
            node.FunctionDistributionPerAreaLookup[ExpeditionFunction.Terminal].Add(dist);
        }

        // If placements > static spawners, we let the original try for the remainder (or you can skip them)
        if (placements.Count > total)
        {
            // Optional: Log or skip remaining by consuming and ignoring extras.
            // For simplicity, let the original run for leftover terminals:
            // return true; // allows original to handle remaining placements
            return false; // comment this to combine behaviors; false = we fully handled it
        }

        // We fully handled distribution for all placements
        return false;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HarmonyPatch(typeof(LG_FunctionMarkerBuilder), nameof(LG_FunctionMarkerBuilder.BuildFloorFallback))]
    [HarmonyPrefix]
    internal static bool Prefix_BuildFloorFallback(LG_FunctionMarkerBuilder __instance, LG_Zone zone, uint seed)
    {
        // return true;

        // Default to base complex, but prefer the dimension’s complex when available
        var complexType = Builder.ComplexResourceSetBlock?.ComplexType ?? Complex.Mining;
        if (Dimension.GetDimension(zone.DimensionIndex, out var dim) && dim?.ResourceData != null)
        {
            complexType = dim.ResourceData.ComplexType;
        }

        // Map complex to marker block type + vanilla fallback IDs
        LG_MarkerDataBlockType type;
        uint dataBlockID;
        switch (complexType)
        {
            case Complex.Service:
                type = LG_MarkerDataBlockType.Service;
                dataBlockID = 3u;
                break;
            case Complex.Tech:
                type = LG_MarkerDataBlockType.Tech;
                dataBlockID = 13u;
                break;
            default:
                type = LG_MarkerDataBlockType.Mining;
                dataBlockID = 66u;
                break;
        }

        // Access protected m_node to locate spawn context
        // var nodeField = AccessTools.fallbackield(typeof(LG_FunctionMarkerBuilder), "m_node");
        // var node = nodeField.GetValue(__instance) as AIG_CourseNode;

        var node = __instance.m_node;

        if (node != null && node.TryGetValidSpawnNode(out var spawnNode))
        {
            var pos = spawnNode.Position;
            PhysicsUtil.SlamPos(ref pos, Vector3.down, 3f, hitOffsetBwd: 0f);

            var seq = new XXHashSequence(seed);
            var area = node.m_area;
            var spawner = new LG_MarkerSpawner(dataBlockID, type, area.gameObject, pos, zone, area, seq.NextSubSeed());

            __instance.BuildMarkerSpawner(seq.NextSubSeed(), spawner, out LG_MarkerSpawner _, debug: true);
        }
        else
        {
            Debug.LogError($"[StaticTerminalFallbackFix] Could not pick a valid spawn node for zone {zone.NavInfo.GetFormattedText(LG_NavInfoFormat.Full_And_Number_No_Formatting)}");
        }

        // Skip original fallback
        return false;
    }
}
