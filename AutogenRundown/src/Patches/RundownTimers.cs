using AutogenRundown.Managers;
using CellMenu;
using GameData;
using HarmonyLib;
using UnityEngine;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public static class RundownTimers
{
    private static RundownTimerData? TimerData { get; set; }

    private static GameObject? GetParent(GameObject gameObject, string parentName)
    {
        // Check if this transform is under "Rundown_Surface_SelectionALT_R3"
        var current = gameObject.transform;

        while (current != null)
        {
            if (current.name == parentName)
                return current.gameObject;

            current = current.parent;
        }

        return null;
    }

    /// <summary>
    /// Checks and disables the rundown timer if we can't view the A-tier levels layer.
    ///
    /// This should guarantee that we only show the timer on viewing the rundown screen
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(GUIX_Layer), nameof(GUIX_Layer.OnDisable))]
    [HarmonyPostfix]
    public static void HideTimers(GUIX_Layer __instance)
    {
        if (__instance.gameObject.name != "GUIX_layer_Tier_1")
            return;

        var page = GetParent(__instance.gameObject, "CM_PageRundown_New_CellUI_ALT(Clone)");

        if (page != null)
        {
            var component = page.GetComponent<CM_PageRundown_New>();

            if (component != null)
            {
                // Disable the countdown timer if one's not been set.
                component.m_rundownTimerData = new RundownTimerData
                {
                    ShowCountdownTimer = false,
                    ShowScrambledTimer = false
                };
                component.UpdateHeaderText();
            }
        }

        Plugin.Logger.LogWarning("GUIX_layer_Tier_1 has been disabled");
    }

    [HarmonyPatch(typeof(GUIX_Layer), nameof(GUIX_Layer.OnEnable))]
    [HarmonyPostfix]
    public static void ShowTimers(GUIX_Layer __instance)
    {
        if (__instance.gameObject.name != "GUIX_layer_Tier_1")
            return;

        var page = GetParent(__instance.gameObject, "CM_PageRundown_New_CellUI_ALT(Clone)");

        if (page != null)
        {
            var component = page.GetComponent<CM_PageRundown_New>();

            if (component != null)
            {
                // Disable the countdown timer if one's not been set.
                component.m_rundownTimerData = TimerData ?? new RundownTimerData
                {
                    ShowCountdownTimer = false,
                    ShowScrambledTimer = false
                };

                if (component.m_rundownTimerData.ShowCountdownTimer)
                {
                    var target = new DateTime(
                        component.m_rundownTimerData.UTC_Target_Year,
                        component.m_rundownTimerData.UTC_Target_Month,
                        component.m_rundownTimerData.UTC_Target_Day,
                        component.m_rundownTimerData.UTC_Target_Hour,
                        component.m_rundownTimerData.UTC_Target_Minute,
                        0,
                        DateTimeKind.Utc);

                    component.m_countDownTargetDate = new Il2CppSystem.DateTime(target.Ticks);
                }

                component.UpdateHeaderText();
            }
        }

        Plugin.Logger.LogWarning("GUIX_layer_Tier_1 has been enabled");
    }

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
            2 => RundownSelection.WeeklyTimer,
            1 => RundownSelection.DailyTimer,

            _ => null
        };

        TimerData = timerData;

        // if (timerData == null)
        // {
        //     // Disable the countdown timer if one's not been set.
        //     __instance.m_rundownTimerData = new RundownTimerData
        //     {
        //         ShowCountdownTimer = false,
        //         ShowScrambledTimer = false
        //     };
        //     __instance.UpdateHeaderText();
        //
        //     return;
        // }
        //
        // __instance.m_rundownTimerData = timerData;
        //
        // var target = new DateTime(
        //     timerData.UTC_Target_Year,
        //     timerData.UTC_Target_Month,
        //     timerData.UTC_Target_Day,
        //     timerData.UTC_Target_Hour,
        //     timerData.UTC_Target_Minute,
        //     0,
        //     DateTimeKind.Utc);
        //
        // __instance.m_countDownTargetDate = new Il2CppSystem.DateTime(target.Ticks);
        //
        // // draw immediately; periodic refresh handled by Update()
        // __instance.UpdateHeaderText();
    }
}
