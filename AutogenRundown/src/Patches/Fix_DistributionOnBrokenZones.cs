using AutogenRundown.Managers;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

/// <summary>
/// When Fix_FailedToFindStartArea hits its fatalReached threshold it short-circuits failing
/// zones with __result=true and MarkForRebuild() but lets the engine drain remaining batches
/// naturally. Those broken zones (m_areas.Count == 0) stay in floor.allZones, and the
/// Distribution batch queues per-zone distribution jobs for every zone — including the broken
/// ones. The per-zone jobs call LG_DistributionJobUtils.GetRandomNodeFromZoneForFunction which
/// AOORs on zone.m_areas[weightedAreaIndex] when m_areas is empty.
///
/// This patch short-circuits each per-zone distribution job's Build() when the target zone is
/// broken. The job is marked complete so the engine continues draining toward FactoryDone, at
/// which point Patch_LG_Factory.Prefix_FactoryDone runs the rebuild.
/// </summary>
[HarmonyPatch]
public class Fix_DistributionOnBrokenZones
{
    /// <summary>
    /// De-dupes warnings: one log line per (jobName, zone) instead of one per frame.
    /// Cleared between build passes via ResetDiagnostics().
    /// </summary>
    private static readonly HashSet<int> s_loggedBrokenJobs = new();

    public static void ResetDiagnostics() => s_loggedBrokenJobs.Clear();

    private static bool IsBrokenZone(LG_Zone zone, string jobName)
    {
        if (zone != null && zone.m_areas != null && zone.m_areas.Count > 0)
            return false;

        var key = jobName.GetHashCode() ^ (zone?.GetInstanceID() ?? 0);
        if (s_loggedBrokenJobs.Add(key))
        {
            Plugin.Logger.LogDebug(
                $"Skipping {jobName} for {zone?.name ?? "<null>"} " +
                $"(LocalIndex={zone?.LocalIndex.ToString() ?? "?"}) — zone has no areas.");
        }
        return true;
    }

    [HarmonyPatch(typeof(LG_Distribute_PickupItemsPerZone), nameof(LG_Distribute_PickupItemsPerZone.Build))]
    [HarmonyPrefix]
    public static bool Pre_PickupItems(LG_Distribute_PickupItemsPerZone __instance, ref bool __result)
    {
        if (IsBrokenZone(__instance.m_zone, nameof(LG_Distribute_PickupItemsPerZone)))
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(LG_Distribute_ResourcePacksPerZone), nameof(LG_Distribute_ResourcePacksPerZone.Build))]
    [HarmonyPrefix]
    public static bool Pre_ResourcePacks(LG_Distribute_ResourcePacksPerZone __instance, ref bool __result)
    {
        if (IsBrokenZone(__instance.m_zone, nameof(LG_Distribute_ResourcePacksPerZone)))
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(LG_Distribute_FunctionPerZone), nameof(LG_Distribute_FunctionPerZone.Build))]
    [HarmonyPrefix]
    public static bool Pre_FunctionPerZone(LG_Distribute_FunctionPerZone __instance, ref bool __result)
    {
        if (IsBrokenZone(__instance.m_zone, nameof(LG_Distribute_FunctionPerZone)))
        {
            __result = true;
            return false;
        }
        return true;
    }
}
