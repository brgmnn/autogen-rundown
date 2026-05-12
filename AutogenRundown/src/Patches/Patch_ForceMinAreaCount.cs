using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

/// <summary>
/// Forces zones registered in LevelAreaCounts to have at least N areas (tiles)
/// regardless of coverage. The game's LG_ZoneJob_CreateExpandFromData.Build state
/// machine exits as soon as coverage lands in the [min, max] band, which can
/// produce single-tile zones for small-coverage spawn rooms behind doors. This
/// patch temporarily inflates m_minCoverage on each Build() invocation while the
/// zone still has fewer than the required areas, so the game's own CoverageStatus
/// returns NotEnough and expansion continues. Once the area count is satisfied,
/// the original min coverage is restored and the loop completes normally.
/// </summary>
[HarmonyPatch]
internal static class Patch_ForceMinAreaCount
{
    private static List<LevelAreaCounts> _levelAreaCounts = new();
    private static readonly Dictionary<(int dim, int layer, int zone), int> _map = new();

    internal static void Setup()
    {
        _levelAreaCounts = LevelAreaCounts.LoadAll();

        LevelAPI.OnBuildDone += BuildMap;
        LevelAPI.OnLevelCleanup += () => _map.Clear();
    }

    private static void BuildMap()
    {
        _map.Clear();

        var layoutId = RundownManager.ActiveExpedition?.LevelLayoutData ?? 0;
        if (layoutId == 0)
            return;

        var lac = _levelAreaCounts.Find(x => x.MainLevelLayout == layoutId);
        if (lac == null)
            return;

        foreach (var z in lac.Zones)
            _map[((int)z.Dimension, z.Layer, z.ZoneNumber)] = z.Count;
    }

    public record struct ForceState(bool Bumped, float OriginalMin);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    public static void Pre_Build(LG_ZoneJob_CreateExpandFromData __instance, out ForceState __state)
    {
        __state = default;

        var zone = __instance.m_zone;
        var zs = __instance.m_zoneSettings;
        if (zone == null || zs == null)
            return;

        var key = ((int)zone.DimensionIndex, (int)zone.Layer.m_type, (int)zone.LocalIndex);
        if (!_map.TryGetValue(key, out var required))
            return;

        if (zone.m_areas.Count >= required)
            return;

        // Bump min coverage just above current so the game's CoverageStatus
        // returns NotEnough, keeping the expansion loop running for this tick.
        __state = new ForceState(true, zs.m_minCoverage);
        zs.m_minCoverage = zone.m_coverage + 1f;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    public static void Post_Build(LG_ZoneJob_CreateExpandFromData __instance, ForceState __state)
    {
        if (!__state.Bumped)
            return;

        var zs = __instance.m_zoneSettings;
        if (zs != null)
            zs.m_minCoverage = __state.OriginalMin;
    }
}
