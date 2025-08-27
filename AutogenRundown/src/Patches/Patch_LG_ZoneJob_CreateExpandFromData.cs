using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

using MainStatus = LG_ZoneJob_CreateExpandFromData.MainStatus;
using SubStatus = LG_ZoneJob_CreateExpandFromData.SubStatus;

/// <summary>
/// Improves several methods in the level generation
/// </summary>
[HarmonyPatch]
internal static class Patch_LG_ZoneJob_CreateExpandFromData
{
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyFinalizer]
    public static void Post_LG_ZoneJob_CreateExpandFromData_Build(LG_ZoneJob_CreateExpandFromData __instance, ref Exception? __exception)
    {
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

        // 1) Before selecting start area, bias toward areas with a free plug
        if (mainStatus == MainStatus.FindStartArea && subStatus == SubStatus.SelectArea)
        {
            var scoredStartAreas = __instance.m_scoredStartAreas;

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
            // var zone = _zoneRef(__instance);
            var zone = __instance.m_zone;
            if (zone != null && zone.m_sourceExpander == null)
            {
                // var scoredExpanders = _scoredExpandersRef(__instance);
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
}
