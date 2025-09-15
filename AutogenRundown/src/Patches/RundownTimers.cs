using AutogenRundown.Managers;
using CellMenu;
using GameData;
using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public static class RundownTimers
{
    /// <summary>
    /// This is the one
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="data"></param>
    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.PlaceRundown))]
    [HarmonyPostfix]
    static void OnRundownPlace(CM_PageRundown_New __instance, ref RundownDataBlock data)
    {
        if (__instance.m_currentRundownData != null)
            Plugin.Logger.LogDebug($"placing the rundown: pid = {__instance.m_currentRundownData.persistentID}");

        Plugin.Logger.LogDebug($"direct data block: pid = {data.persistentID}");


        var timerData = data.persistentID switch
        {
            4 => RundownSelection.SeasonalTimer,
            3 => RundownSelection.MonthlyTimer,

            _ => null
        };


        if (timerData == null)
        {
            // Disable the countdown timer if one's not been set.
            __instance.m_rundownTimerData = new RundownTimerData
            {
                ShowCountdownTimer = false,
                ShowScrambledTimer = false
            };
            __instance.UpdateHeaderText();

            return;
        }

        __instance.m_rundownTimerData = timerData;

        var target = new DateTime(
            timerData.UTC_Target_Year,
            timerData.UTC_Target_Month,
            timerData.UTC_Target_Day,
            timerData.UTC_Target_Hour,
            timerData.UTC_Target_Minute,
            0,
            DateTimeKind.Utc);

        __instance.m_countDownTargetDate = new Il2CppSystem.DateTime(target.Ticks);

        // draw immediately; periodic refresh handled by Update()
        __instance.UpdateHeaderText();
    }
}
