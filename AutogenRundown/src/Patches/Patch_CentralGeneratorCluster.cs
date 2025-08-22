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
///     * Area availability not checked: Selected area may have 0 `GeneratorCluster` spawners.
///       With no fallback, the item is lost.
///     * Zero-weight groups: Marker data (`FunctionPotential`) can sum to 0, so
///       `GetRandomSpawner` returns null despite spawners existing.
///     * Seed coupling: `MarkerSubseed` drives per-zone spawner selection, but not the area/node
///       choice. Changing it can land on a different spawner/area path and "fix" the run.
///
/// Validation Steps
///     * A sample layout was generated with a valid generator cluster geomorph in Zone_1.
///     * A large coverage was set for Zone_0 (the elevator drop zone) to get the security door
///       to Zone_1 to spawn in the middle of another tile.
///     * The generator cluster was verified to be in Area B of Zone_1.
///     * The function LG_DistributionJobUtils.GetRandomNodeFromZoneForFunction() was instrumented
///       to print out which area was selected when picking a node for the generator cluster job
///     * A failed seed was found and loading into the level verified that no cluster spawned
///     * GetRandomNodeFromZoneForFunction() was observed selecting Area A to try and spawn the
///       generator cluster.
///     * APPLIED FIX: when re-running the exact same level with this patch, we observe that
///       Area B is correctly selected, and the generator cluster spawns correctly.
///
/// How we attempt to fix it
///
/// Prefer areas that actually have generator spawners
///     * Target: `LevelGeneration.LG_DistributionJobUtils.GetRandomNodeFromZoneForFunction(LG_Zone, ExpeditionFunction, float, float)`
///     * Prefix: for `GeneratorCluster`, filter `zone.m_areas` to those with
///       `area.GetMarkerSpawnerCount(GeneratorCluster) > 0`, then pick deterministically by `randomValue`.
/// </summary>
[HarmonyPatch]
public class Patch_CentralGeneratorCluster
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LG_DistributionJobUtils), nameof(LG_DistributionJobUtils.GetRandomNodeFromZoneForFunction))]
    public static bool FixGeneratorClusterSpawn(
        LG_Zone zone,
        ExpeditionFunction func,
        float randomValue,
        ref AIG_CourseNode __result)
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

        Plugin.Logger.LogDebug($"Selected area for generator cluster: {area.m_navInfo.m_suffix}");

        __result = area.m_courseNode;

        return false;
    }

    /// <summary>
    /// This method sets up the ServiceMarker data blocks to add the extra marker needed for the
    /// Service Complex geomorphs to be able to spawn the generator cluster marker
    ///
    /// It can be called multiple times, but once after the datablocks are loaded is sufficient
    /// </summary>
    public static void Setup()
    {
        var block = ServiceMarkerDataBlock.GetBlock(11);

        var alreadySetup = false;

        foreach (var composition in block.CommonData.Compositions)
            if (composition.prefab == "Assets/GameObject/Floodways_open_1400x1000x1000_V09.prefab")
                alreadySetup = true;

        if (alreadySetup)
            return;

        block.CommonData.Compositions.Add(new MarkerComposition
        {
            weight = 0.0f,
            preppedInstance = null,
            prefab = "Assets/GameObject/Floodways_open_1400x1000x1000_V09.prefab",
            function = ExpeditionFunction.GeneratorCluster,
            Shard = AssetBundleShard.S5
        });

        Plugin.Logger.LogDebug($"Patched ServiceMarkerDataBlock for Generator Cluster in Complex=Service");
    }
}
