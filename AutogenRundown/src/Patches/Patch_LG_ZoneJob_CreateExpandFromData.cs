using GameData;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches;

using MainStatus = LG_ZoneJob_CreateExpandFromData.MainStatus;
using SubStatus = LG_ZoneJob_CreateExpandFromData.SubStatus;

internal record BuildState(MainStatus MainStatus, SubStatus SubStatus);

/// <summary>
/// Improves several methods in the level generation
/// </summary>
[HarmonyPatch]
internal static class Patch_LG_ZoneJob_CreateExpandFromData
{
    private static Dictionary<(LG_LayerType layer, eLocalZoneIndex zoneNumber), int> jobFailures = new();

    /// <summary>
    /// This is where we need to catch and itemize failed zones
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__exception"></param>
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyFinalizer]
    public static void Post_LG_ZoneJob_CreateExpandFromData_Build(LG_ZoneJob_CreateExpandFromData __instance, ref Exception? __exception)
    {
        // [Warning:AutogenRundown] Re-rolling subSeed=5699
        // [Error  :     Unity] WARNING : Zone1 (Zone_1 - 315): Failed to find any good StartAreas in zone 0 (314) expansionType:Towards_Random m_buildFromZone.m_areas: 1 scoredCount:0 dim: Reality

        if (__instance.m_mainStatus == MainStatus.FindStartArea &&
            __instance.m_subStatus == SubStatus.SelectArea &&
            __instance.m_scoredStartAreas.Count < 1)
        {
            if (__instance.m_subSeed < 0xffff)
            {
                Plugin.Logger.LogWarning($"Re-rolling subSeed={__instance.m_subSeed}");
                __instance.m_subSeed++;
            }
            else
            {
                Plugin.Logger.LogError($"Exhausted SubSeed rolls. Zone placement is unlikely to ever succeed");
            }
        }
    }

    #region Fix: Directional expansion being blocked but not falling back to valid open other directions

    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyPrefix]
    public static void CaptureState(LG_ZoneJob_CreateExpandFromData __instance, out BuildState __state)
    {
        __state = new BuildState(__instance.m_mainStatus, __instance.m_subStatus);
    }

    /// <summary>
    /// This is to help catch situations where zone expansion fails because the builder attempted
    /// to build a zone, say, forward and failed because there were no forward spaces available.
    /// But the starting zone does have space in other directions.
    ///
    /// This is fairly common with directional expansion settings because it excludes the other
    /// directions normally from consideration. This adds those back in as a fallback.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__state"></param>
    // [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    // [HarmonyPostfix]
    // public static void AddFallback_StartAreas(LG_ZoneJob_CreateExpandFromData __instance, BuildState __state)
    // {
    //     // Preconditions: only act if no start areas are scored
    //     var scoredStartAreas = __instance.m_scoredStartAreas; // public field
    //
    //     var mainStatus = __state.MainStatus;
    //     var subStatus = __state.SubStatus;
    //
    //     // This will be called after we transition from (FindStartArea, ScoreAreas), where we
    //     // transition to SelectArea as the last call
    //     if (mainStatus != MainStatus.FindStartArea || subStatus != SubStatus.ScoreAreas)
    //         return;
    //
    //     // Plugin.Logger.LogWarning($"Now we continue supposedly jobIndex={jobIndex}");
    //
    //     if (scoredStartAreas == null || scoredStartAreas.Count != 0)
    //         return;
    //
    //     // If we already have a chosen start area, nothing to do
    //     if (__instance.m_startArea != null)
    //         return;
    //
    //     // Ensure we have a valid zone to pick areas from
    //     var fromZone = __instance.m_buildFromZone;
    //
    //     if (fromZone == null || fromZone.m_areas == null || fromZone.m_areas.Count == 0)
    //         return;
    //
    //     // Check requested StartExpansion; only fallback if it was directional (not Random)
    //     var zoneData = __instance.m_zoneData;
    //     var settings = __instance.m_zoneSettings;
    //
    //     if (zoneData == null)
    //         return; // safety: cannot verify policy without zone data
    //
    //     // if (settings.HasCustomGeomorphPrefab)
    //     //     return;
    //
    //     try
    //     {
    //         // Fallback: include all directions with lower priority
    //         // 1) Create low, positive scores for all areas
    //         var fallback = LG_Scoring.CreateScores(fromZone.m_areas, 0.1f);
    //         var subSeed = __instance.m_rnd.Random.NextSubSeed();
    //
    //         // 2) Randomize within the low-priority bucket for tie-breaking
    //         LG_Scoring.AssignRandomScores(fallback, seed: subSeed, max: 1f);
    //
    //         // 3) Sort and assign back
    //         fallback = LG_Scoring.ScoreSort<LG_Area>(fallback);
    //         // __instance.m_scoredStartAreas = fallback;
    //
    //         foreach (var score in fallback)
    //             scoredStartAreas.Add(score);
    //
    //         // 4) Mark current expansion type as inclusive/random for the start-area phase
    //         // __instance.m_currentExpansionType = eZoneExpansionType.Directional_Random;
    //
    //         Plugin.Logger.LogWarning($"Fallback enabled for start areas in {__instance.m_zone?.Alias} " +
    //                                  $"(dim:{__instance.m_dimension?.DimensionIndex}). " +
    //                                  $"Requested: {zoneData.StartExpansion}; ");
    //     }
    //     catch (Exception ex)
    //     {
    //         Plugin.Logger.LogError($"Fallback start-area patch failed: {ex}");
    //     }
    // }

    #endregion

    #region Fix: Zone expansion getting trapped in dead ends

    static bool HasFreePlug(LG_Area area)
    {
        if (area?.m_zoneExpanders == null)
            return false;

        foreach (var exp in area.m_zoneExpanders)
        {
            if (exp is LG_Plug plug)
            {
                if (!plug.IsZoneBuildBlocked())
                {
                    var to = plug.m_linksTo; // may be null if not paired yet

                    if (to == null || to.m_zone == null)
                        return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Fixes a serious edge case where the level generation would select areas which are free in
    /// the tile but don't have any free connections themselves. This results in some zones getting
    /// boxed in and not being able to build futher even though the zone they are built from has
    /// free plugs available.
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    public static void FixDeadEndZoneExpansion(LG_ZoneJob_CreateExpandFromData __instance)
    {
        var mainStatus = __instance.m_mainStatus;
        var subStatus = __instance.m_subStatus;
        var zone = __instance.m_zone;

        if (zone == null)
            return;

        // 1) Before selecting start area, bias toward areas with a free plug
        if (mainStatus == MainStatus.FindStartArea && subStatus == SubStatus.SelectArea)
        {
            var scoredStartAreas = __instance.m_scoredStartAreas;

            // Here we try and ensure that we pick areas that aren't dead ends
            // currently this only runs on the first zone of a layout when trying to find the
            // very first place where it should go.
            if (zone.LocalIndex == eLocalZoneIndex.Zone_0 && zone.m_areas.Count == 0)
            {
                Plugin.Logger.LogInfo($"=== FORCING NEW ZONE TO BUILD FROM PLUG ===");
                Plugin.Logger.LogInfo($"{zone.Alias}, {zone.LocalIndex}, numareas={zone.m_areas.Count}, " +
                                      $"aim direction = {__instance.m_currentBuildDirection}");

                // We track the discarded values in case we need them.
                var discarded = new List<LG_Scoring.Score<LG_Area>>();

                // foreach (var s in scoredStartAreas)
                for (var i = scoredStartAreas.Count - 1; i >= 0; i--)
                {
                    var score = scoredStartAreas[i];

                    Plugin.Logger.LogInfo($"    score={score.score} -> {HasFreePlug(score.item)}, {score.item.m_zoneExpanders}");

                    var hasFreePlug = false;

                    foreach (var exp in score.item.m_zoneExpanders)
                    {
                        var isFree = exp.m_linksTo == null || exp.m_linksTo.m_zone == null;

                        if (exp.ExpanderType == LG_ZoneExpanderType.Plug && isFree)
                            hasFreePlug = true;

                        if (isFree)
                            Plugin.Logger.LogInfo($"        expander: type={exp.ExpanderType}, free={isFree}");
                        else
                            Plugin.Logger.LogInfo($"        expander: type={exp.ExpanderType}, free={isFree} -> " +
                                                  $"{exp.m_linksTo.m_zone.LocalIndex} {exp.m_linksTo?.m_navInfo?.m_suffix}");
                    }

                    if (!hasFreePlug)
                    {
                        scoredStartAreas.RemoveAt(i);
                        discarded.Add(score);

                        Plugin.Logger.LogInfo($"    discarded area: score={score.score}, position={score.item.Position}");
                    }
                }

                // This is a problem, we ended up discarding everything so we will add back everything we discarded.
                if (scoredStartAreas.Count == 0)
                {
                    discarded.Reverse();

                    foreach (var score in scoredStartAreas)
                        __instance.m_scoredStartAreas.Add(score);
                }
            }

            // Not convinced this works?
            if (scoredStartAreas != null && scoredStartAreas.Count > 1)
            {
                // stable-partition: keep order within partitions
                var withPlug = new List<LG_Scoring.Score<LG_Area>>();
                var withoutPlug = new List<LG_Scoring.Score<LG_Area>>();

                foreach (var s in scoredStartAreas)
                {
                    if (HasFreePlug(s.item))
                        withPlug.Add(s);
                    else
                        withoutPlug.Add(s);
                }

                if (withPlug.Count > 0 && withoutPlug.Count > 0)
                {
                    scoredStartAreas.Clear();

                    // ascending list; last element is picked => put preferred last
                    foreach (var area in withoutPlug)
                        scoredStartAreas.Add(area);
                    foreach (var area in withPlug)
                        scoredStartAreas.Add(area);
                }
            }
        }

        // 2) Before first expander pick, bias toward plugs
        if (mainStatus == MainStatus.ExpandFromArea && subStatus == SubStatus.BuildExpander)
        {
            if (zone.m_sourceExpander == null)
            {
                var scoredExpanders = __instance.m_scoredExpanders;

                if (scoredExpanders != null && scoredExpanders.Count > 1)
                {
                    var plugs  = new List<LG_Scoring.Score<LG_ZoneExpander>>();
                    var others = new List<LG_Scoring.Score<LG_ZoneExpander>>();

                    foreach (var s in scoredExpanders)
                    {
                        if (s.item != null && s.item.ExpanderType == LG_ZoneExpanderType.Plug)
                            plugs.Add(s);
                        else
                            others.Add(s);
                    }

                    if (plugs.Count > 0 && others.Count > 0)
                    {
                        scoredExpanders.Clear();

                        // ascending list; last element is picked => put preferred last
                        foreach (var expander in others)
                            scoredExpanders.Add(expander);
                        foreach (var expander in plugs)
                            scoredExpanders.Add(expander);
                    }
                }
            }
        }
    }

    #endregion

    #region Fix: Custom Geos failing to generate

    /// <summary>
    /// This fixed C1 Rotheart
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__result"></param>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    public static void CustomGeoPrioritizeExternal(LG_ZoneJob_CreateExpandFromData __instance, ref bool __result)
    {
        var zoneSettings = __instance.m_zoneSettings;

        // For zones with custom geomorphs, heavily bias toward plugs (external connections)
        if (__instance.m_mainStatus == MainStatus.ExpandFromArea &&
            __instance.m_subStatus == SubStatus.BuildExpander &&
            zoneSettings?.HasCustomGeomorphPrefab == true)
        {
            var zone = __instance.m_zone;
            var scoredExpanders = __instance.m_scoredExpanders;

            if (zone != null && scoredExpanders != null && scoredExpanders.Count > 1)
            {
                // Separate plugs from gates
                var plugs = new List<LG_Scoring.Score<LG_ZoneExpander>>();
                var gates = new List<LG_Scoring.Score<LG_ZoneExpander>>();

                foreach (var scored in scoredExpanders)
                {
                    if (scored.item?.ExpanderType == LG_ZoneExpanderType.Plug)
                        plugs.Add(scored);
                    else
                        gates.Add(scored);
                }

                if (plugs.Count > 0)
                {
                    // For custom geomorph zones, prioritize plugs heavily
                    scoredExpanders.Clear();

                    // Gates go first (lower priority)
                    foreach (var score in gates)
                        scoredExpanders.Add(score);

                    // Plugs go last (higher priority - selected first)
                    foreach (var score in plugs)
                        scoredExpanders.Add(score);

                    // scoredExpanders.AddRange(gates);      // Gates go first (lower priority)
                    // scoredExpanders.AddRange(plugs);      // Plugs go last (higher priority - selected first)
                }
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    public static bool FixCustomGeoCoverageExhaustion(LG_ZoneJob_CreateExpandFromData __instance, ref bool __result)
    {
        if (__instance.m_mainStatus != MainStatus.ExpandFromArea ||
            __instance.m_subStatus != SubStatus.SelectArea)
            return true;

        var zs = __instance.m_zoneSettings;

        if (zs != null && zs.HasCustomGeomorphPrefab && !zs.HasCustomGeomorphInstance)
        {
            var zone = __instance.m_zone;

            // Equivalent to CoverageStatus(zone) == CoverageResult.Ok:
            var isOk = zone.m_coverage >= zs.m_minCoverage && zone.m_coverage <= zs.m_maxCoverage;

            if (isOk)
            {
                // Mirror the non-early-exit branch: clean and retry from start area
                __instance.BlockAndCleanFailedAreasFromZone(
                    __instance.m_floor,
                    zone,
                    __instance.m_buildFromZone,
                    __instance.m_startArea
                );

                __instance.m_mainStatus = MainStatus.FindStartArea;
                __instance.m_subStatus = SubStatus.SelectArea;

                // keep Build() running on next tick
                __result = false;

                return false;
            }
        }

        return true;
    }

    #endregion
}
