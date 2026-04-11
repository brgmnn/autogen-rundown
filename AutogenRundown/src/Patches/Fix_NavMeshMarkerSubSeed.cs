using AutogenRundown.Managers;
using GameData;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches;

using MainStatus = LG_ZoneJob_CreateExpandFromData.MainStatus;

[HarmonyPatch]
public class Fix_NavMeshMarkerSubSeed
{
    private static bool s_suppressHook = false; // prevent recursion while we recompute

    private const int MaxAttemptsPerZone = 128;

    public static readonly Dictionary<(eDimensionIndex dim, eLocalZoneIndex lz), int> ZoneAttempts = new();

    public static readonly HashSet<(eDimensionIndex dim, eLocalZoneIndex lz)> TargetsDetected = new();

    public static readonly Dictionary<(eDimensionIndex dim, eLocalZoneIndex lz), uint> MarkerSubSeeds = new();

    public static void Setup()
    {
        FactoryJobManager.OnDoneValidate += Validate;
    }

    private static bool Validate()
    {
        try
        {
            // Find first unhealthy detected target
            foreach (var key in TargetsDetected)
            {
                var zone = FindZone(key);

                if (zone == null)
                    continue;

                if (IsZoneHealthy(zone))
                    continue;

                // Bump and rebuild
                var current = MarkerSubSeeds.TryGetValue(key, out var v) ? v : zone.m_markerSubSeed;
                var next = current + 1;
                MarkerSubSeeds[key] = next;
                ZoneAttempts[key] = (ZoneAttempts.TryGetValue(key, out var a) ? a : 0) + 1;

                if (ZoneAttempts[key] > MaxAttemptsPerZone)
                {
                    Plugin.Logger.LogError($"[Reroll] Max attempts reached for {key}. Last m_markerSubSeed={current}");
                    // Give up on this key but keep others (remove from detected set)
                    // Optionally: keep it to retry later
                    TargetsDetected.Remove(key);
                    break;
                }

                Plugin.Logger.LogDebug($"[Reroll] Rebuilding {key} with m_markerSubSeed={next} (attempt {ZoneAttempts[key]})");

                return false;
            }

            // All detected targets healthy? Clean up
            // (re-check and prune)
            var toRemove = new List<(eDimensionIndex, eLocalZoneIndex)>();

            foreach (var key in TargetsDetected)
            {
                var zone = FindZone(key);

                if (zone != null && IsZoneHealthy(zone))
                {
                    Plugin.Logger.LogDebug($"[Reroll] {key} healthy. Attempts={ZoneAttempts.GetValueOrDefault(key, 0)}, m_markerSubSeed={zone.m_markerSubSeed}");
                    toRemove.Add(key);
                }
            }

            foreach (var key in toRemove)
                TargetsDetected.Remove(key);

            // Release suppression when done
            if (TargetsDetected.Count == 0)
            {
                Plugin.Logger.LogDebug("[Reroll] All detected zones healthy. Releasing factory done suppression.");

                return true;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Encountered a bad error: {ex}");
        }

        return true;
    }

    private static bool IsZoneHealthy(LG_Zone zone)
    {
        foreach (var cn in zone.m_courseNodes)
        {
            var cl = cn?.m_nodeCluster;

            if (cl == null || cl.m_reachableNodes == null || cl.m_reachableNodes.Count <= 1)
                return false;
        }
        return true;
    }

    private static LG_Zone? FindZone((eDimensionIndex dim, eLocalZoneIndex lz) key)
    {
        var floor = Builder.CurrentFloor;

        if (floor == null)
            return null;

        Dimension dim;

        if (!floor.GetDimension(key.dim, out dim) || dim == null)
            return null;

        foreach (var layer in dim.Layers)
        {
            foreach (var zone in layer.m_zones)
                if (zone.LocalIndex == key.lz)
                    return zone;
        }

        return null;
    }

    [HarmonyPatch(typeof(LG_Layer), nameof(LG_Layer.CreateZone))]
    [HarmonyPrefix]
    static void Prefix_Layer_CreateZone(LG_Layer __instance, LG_Floor floor, ref ExpeditionZoneData zoneData, int zoneAliasStart)
    {
        // TODO: we need to account for the different layers

        // TODO: we probably want to extract this to it's own manager/patcher so we can override seeds from other places too

        Plugin.Logger.LogDebug($"--------------------------> [Reroll] GOTCHA!!! " +
                               $"m_markerSubSeed={zoneData.MarkerSubSeed} to " +
                               $"{zoneData.LocalIndex} (dim:{__instance.m_dimension.DimensionIndex})");

        var key = (__instance.m_dimension.DimensionIndex, zoneData.LocalIndex);

        // if (ZoneSeedManager.SubSeeds.TryGetValue(
        //         (__instance.m_dimension.DimensionIndex, __instance.m_type, zoneData.LocalIndex),
        //         out var overrideSubSeed))
        // {
        //     zoneData.SubSeed = (int)overrideSubSeed;
        //
        //     Plugin.Logger.LogDebug($"[Reroll] Applied override m_subSeed={overrideSubSeed} to Zone_{zoneData.LocalIndex}");
        // }

        if (MarkerSubSeeds.TryGetValue(key, out var overrideSeed))
        {
            zoneData.MarkerSubSeed = (int)overrideSeed;

            Plugin.Logger.LogDebug($"[Reroll] Applied override m_markerSubSeed={overrideSeed} to Zone_{zoneData.LocalIndex}");
        }
    }

    // Optional: detect degenerate areas early (no action here; we rebuild on factory done)
    [HarmonyPatch(typeof(LG_NodeTools), nameof(LG_NodeTools.CalculateReachableNodesAndLocationScores))]
    [HarmonyPostfix]
    static void Postfix_CalculateReachableNodesAndLocationScores(LG_Area area, Vector3 sourcePos)
    {
        try
        {
            var zone = area?.m_zone;

            if (zone == null)
                return;

            var cluster = area.m_courseNode?.m_nodeCluster;

            // Skip arena dimensions — these are simple single-tile arenas (e.g. pouncer)
            // that naturally have m_reachableNodes.Count <= 1
            if (Builder.CurrentFloor.GetDimension(zone.DimensionIndex, out var dim) && dim.IsArenaDimension)
                return;

            if (cluster == null || cluster.m_reachableNodes.Count > 1)
                return;

            var key = (zone.DimensionIndex, zone.LocalIndex);

            if (!TargetsDetected.Contains(key))
            {
                TargetsDetected.Add(key);

                FactoryJobManager.MarkForRebuild();

                // Initialize override baseline if not present
                if (!MarkerSubSeeds.ContainsKey(key))
                    MarkerSubSeeds[key] = zone.m_markerSubSeed;

                if (!ZoneAttempts.ContainsKey(key))
                    ZoneAttempts[key] = 0;

                Plugin.Logger.LogDebug($"[Reroll] Detected unhealthy zone {key}. Will reroll after factory completion.");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex);
        }
    }


    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyPrefix]
    public static void CaptureState(LG_ZoneJob_CreateExpandFromData __instance, out BuildState __state)
    {
        __state = new BuildState(__instance.m_mainStatus, __instance.m_subStatus);
    }

    /// <summary>
    /// Rerolls zones that didn't get their custom geos
    ///
    /// Also re-roll the parent zones subseed. We often see that the issue is the parent zone
    /// needs to be re-rolled.
    ///
    /// TODO: Handle zone_0 zones, notably secondary/extreme
    /// TODO: Handle dimensions
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__state"></param>
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyPostfix]
    public static void ForceRerollForMissingCustomGeos(LG_ZoneJob_CreateExpandFromData __instance, BuildState __state)
    {
        var mainStatus = __state.MainStatus;
        // var subStatus = __state.SubStatus;

        var zone = __instance.m_zone;
        var data = __instance.m_zoneData;
        var settings = __instance.m_zoneSettings;

        if (mainStatus != MainStatus.Done)
            return;

        if (data?.CustomGeomorph != null && !settings.HasCustomGeomorphInstance)
        {
            ZoneSeedManager.Reroll_SubSeed(zone);

            if (zone.LocalIndex == eLocalZoneIndex.Zone_0)
                return;

            ZoneSeedManager.Reroll_SubSeed(data.BuildFromLocalIndex, zone.DimensionIndex, zone.Layer.m_type);
        }
    }
}
