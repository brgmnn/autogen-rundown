using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_LG_ZoneJob_CreateExpandFromData
{
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyPostfix]
    public static void Post_LG_ComputerTerminal_Setup(LG_ZoneJob_CreateExpandFromData __instance)
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
    }
}
