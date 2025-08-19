using AIGraph;
using GameData;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// What this solves:
///
/// Generator cluster placement is queued per-zone, then resolved by selecting a zone area and
/// consuming a marker spawner for `GeneratorCluster`. Failures occur when the selected area has
/// no eligible spawners or when the spawner group has zero total weight, causing selection to
/// return null. Because generator clusters are enqueued with `allowFunctionFallback: false`,
/// these items are dropped instead of retrying elsewhere. Re-rolling `MarkerSubseed` changes the
/// RNG that picks spawners and can incidentally select an area with candidates, which explains
/// why re-rolling seeds eventually yields a working generator cluster.
///
/// Why It Fails Intermittently?
///
///     * Area availability not checked: Selected area may have 0 `GeneratorCluster` spawners.
///       With no fallback, the item is lost.
///     * Zero-weight groups: Marker data (`FunctionPotential`) can sum to 0, so
///       `GetRandomSpawner` returns null despite spawners existing.
///     * Seed coupling: `MarkerSubseed` drives per-zone spawner selection, but not the area/node
///       choice. Changing it can land on a different spawner/area path and "fix" the run.
///
/// How we attempt to fix it
///
/// Prefer areas that actually have generator spawners
///     * Target: `LevelGeneration.LG_DistributionJobUtils.GetRandomNodeFromZoneForFunction(LG_Zone, ExpeditionFunction, float, float)`
///     * Prefix: for `GeneratorCluster`, filter `zone.m_areas` to those with `area.GetMarkerSpawnerCount(GeneratorCluster) > 0`, then pick deterministically by `randomValue`.
/// </summary>
[HarmonyPatch]
public class Patch_LG_DistributionJobUtils
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LG_DistributionJobUtils), nameof(LG_DistributionJobUtils.GetRandomNodeFromZoneForFunction))]
    public static bool FixGenerator(LG_Zone zone, ExpeditionFunction func, float randomValue, ref AIG_CourseNode __result)
    {
        // Skip patching for all other functions
        if (func != ExpeditionFunction.GeneratorCluster)
            return true;

        // Here we apply the generator cluster fix:
        // We filter `zone.m_areas` to those with `area.GetMarkerSpawnerCount(GeneratorCluster) > 0`,
        // then pick deterministically by `randomValue`
        var areas = zone.m_areas;
        var candidates = new List<LG_Area>();

        foreach (var a in areas)
            if (a.GetMarkerSpawnerCount(ExpeditionFunction.GeneratorCluster) > 0)
                candidates.Add(a);

        // fall back to original if we found no candidates
        if (candidates.Count == 0)
            return true;

        // Select one of the valid areas deterministically
        var index = Mathf.Clamp((int)(randomValue * candidates.Count), 0, candidates.Count - 1);
        var area = candidates[index];

        Plugin.Logger.LogInfo($"Selected area for generator cluster: {area.m_navInfo.m_suffix}");

        __result = area.m_courseNode;

        return false;
    }
}
