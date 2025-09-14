using AutogenRundown.DataBlocks;
using AutogenRundown.Managers;
using CellMenu;
using GameData;
using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public static class RundownTimers
{
    // [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
    // [HarmonyPostfix]
    // static void AddTimers(CM_PageRundown_New __instance)
    // {
    //     // var trav = Traverse.Create(__instance);
    //     // var data = trav.Field("m_rundownTimerData").GetValue<RundownTimerData>() ?? new RundownTimerData();
    //     // data.ShowScrambledTimer = false;
    //     // data.ShowCountdownTimer = true;
    //     // trav.Field("m_rundownTimerData").SetValue(data);
    //
    //     __instance.m_rundownTimerData = new RundownTimerData
    //     {
    //         ShowCountdownTimer = true,
    //         ShowScrambledTimer = false,
    //
    //     };
    //
    //     if (__instance.m_currentRundownData != null)
    //         Plugin.Logger.LogDebug($"Ok here we go: pid = {__instance.m_currentRundownData.persistentID}");
    //
    //     // draw immediately; periodic refresh handled by Update()
    //     __instance.UpdateHeaderText();
    // }

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

            _ => null
        };


        if (timerData == null)
            return;

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

// [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
// static class Rundown_OnEnable_Countdown
// {
//     static void Postfix(CM_PageRundown_New __instance)
//     {
//         var trav = Traverse.Create(__instance);
//         var data = trav.Field("m_rundownTimerData").GetValue<RundownTimerData>() ?? new RundownTimerData();
//         data.ShowScrambledTimer = false;
//         data.ShowCountdownTimer = true;
//         trav.Field("m_rundownTimerData").SetValue(data);
//
//         // draw immediately; periodic refresh handled by Update()
//         __instance.UpdateHeaderText();
//     }
// }
