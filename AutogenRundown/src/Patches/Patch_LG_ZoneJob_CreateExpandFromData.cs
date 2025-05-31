using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_LG_ZoneJob_CreateExpandFromData
{
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyFinalizer]
    public static void Post_LG_ZoneJob_CreateExpandFromData_Build(LG_ZoneJob_CreateExpandFromData __instance, ref Exception? __exception)
    {
        if (__instance.m_mainStatus == LG_ZoneJob_CreateExpandFromData.MainStatus.FindStartArea &&
            __instance.m_subStatus == LG_ZoneJob_CreateExpandFromData.SubStatus.SelectArea &&
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

        // if (__exception is null)
        //     return;

        // ex.GetType().FullName == "System.ServiceModel.FaultException"

        // if (__exception.InnerException is NullReferenceException)
        // {
            // Plugin.Logger.LogWarning($"LG_ZoneJob_CreateExpandFromData.Build() threw an error. __exception={__exception.GetType().FullName},  inner={__exception.InnerException?.GetType().FullName}");
        //     return;
        // }

        // if (__instance.m_mainStatus == LG_ZoneJob_CreateExpandFromData.MainStatus.Done &&
        //     __instance.m_zone?.m_sourceExpander is null)
        // {
        //     Plugin.Logger.LogWarning($"Need to re-rolling subSeed={__instance.m_subSeed}, m_zone is null? {__instance.m_zone}");
        // }
    }
}
