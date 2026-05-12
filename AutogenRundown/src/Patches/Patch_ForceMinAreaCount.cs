using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using GameData;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Two coordinated behaviors for zones registered in LevelAreaCounts:
///
/// 1) Force first tile: if an override prefab path is registered, intercept
///    the first tile pick for that zone in LG_Floor.FindExternalArea and
///    substitute our prefab via the ComplexResourceSetDataBlock.GetGeomorphTile
///    postfix. Reuses Builder.ComplexResourceSetBlock.GetCustomGeomorph to
///    resolve the asset path (same resolver vanilla uses for
///    Zone.CustomGeomorph in LG_ZoneSettings.CustomGeomorphPrefab).
///
/// 2) Force min area count: while m_zone.m_areas.Count is below the recorded
///    Count, bump m_minCoverage above current coverage on Build() so the
///    game's CoverageStatus returns NotEnough and the expansion loop keeps
///    running. Restored in the postfix.
///
/// (1) is required because setting Zone.CustomGeomorph triggers an atomic
/// prefab dump (LG_ZoneJob_CreateExpandFromData.cs:340, 848) that bypasses
/// the expander and the m_minCoverage bump entirely; routing through the
/// normal pick path keeps us inside the coverage-driven build loop.
/// </summary>
[HarmonyPatch]
internal static class Patch_ForceMinAreaCount
{
    private static List<LevelAreaCounts> _levelAreaCounts = new();
    private static readonly Dictionary<(int dim, int layer, int zone), (int Count, string? Geo)> _map = new();

    /// <summary>
    /// Bridges Pre_FindExternalArea (decides we want an override for the next
    /// pick) to Post_GetGeomorphTile (substitutes the prefab in the result).
    /// ThreadStatic because Unity builds the level on the main thread but we
    /// don't want any leakage if anyone ever calls these off-thread.
    /// </summary>
    [ThreadStatic] private static GameObject? _pendingFirstTilePrefab;

    internal static void Setup()
    {
        _levelAreaCounts = LevelAreaCounts.LoadAll();

        LevelAPI.OnBuildDone += BuildMap;
        LevelAPI.OnLevelCleanup += () =>
        {
            _map.Clear();
            _pendingFirstTilePrefab = null;
        };
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
            _map[((int)z.Dimension, z.Layer, z.ZoneNumber)] = (z.Count, z.Geomorph);
    }

    #region First-tile override

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LG_Floor), nameof(LG_Floor.FindExternalArea))]
    public static void Pre_FindExternalArea(LG_Zone zone)
    {
        if (zone == null || zone.m_areas.Count != 0)
            return;

        var key = ((int)zone.DimensionIndex, (int)zone.Layer.m_type, (int)zone.LocalIndex);
        if (!_map.TryGetValue(key, out var entry) || string.IsNullOrEmpty(entry.Geo))
            return;

        var prefab = Builder.ComplexResourceSetBlock.GetCustomGeomorph(entry.Geo);
        if (prefab != null)
            _pendingFirstTilePrefab = prefab;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LG_Floor), nameof(LG_Floor.FindExternalArea))]
    public static void Post_FindExternalArea()
    {
        // Safety: drop the override if GetGeomorphTile wasn't reached (e.g.
        // FindExternalArea took the HasCustomGeomorphPrefab branch).
        _pendingFirstTilePrefab = null;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ComplexResourceSetDataBlock), nameof(ComplexResourceSetDataBlock.GetGeomorphTile))]
    public static void Post_GetGeomorphTile(ref GameObject __result)
    {
        if (_pendingFirstTilePrefab == null)
            return;

        __result = _pendingFirstTilePrefab;
        _pendingFirstTilePrefab = null;
    }

    #endregion

    #region Min-area-count enforcement

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
        if (!_map.TryGetValue(key, out var entry))
            return;

        if (zone.m_areas.Count >= entry.Count)
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

    #endregion
}
