using AutogenRundown.Managers;
using CellMenu;
using HarmonyLib;
using UnityEngine;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_CM_PageRundown_New
{
    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.Setup))]
    [HarmonyPostfix]
    private static void Post_Setup(CM_PageRundown_New __instance)
    {
        EventManager.RegisterPage(__instance);
        EventManager.UpdateRundown();
    }

    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.PlaceRundown))]
    [HarmonyPostfix]
    private static void Post_PlaceRundown(CM_PageRundown_New __instance)
    {
        EventManager.UpdateRundown();

        CenterTierIcons(__instance.m_expIconsTier1);
        CenterTierIcons(__instance.m_expIconsTier2);
        CenterTierIcons(__instance.m_expIconsTier3);
        CenterTierIcons(__instance.m_expIconsTier4);
        CenterTierIcons(__instance.m_expIconsTier5);
    }

    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
    [HarmonyPostfix]
    private static void Post_OnEnable(CM_PageRundown_New __instance)
    {
        EventManager.UpdateRundown();
    }

    private static void CenterTierIcons(Il2CppSystem.Collections.Generic.List<CM_ExpeditionIcon_New> icons)
    {
        if (icons == null || icons.Count == 0)
            return;

        var sumX = 0f;
        for (var i = 0; i < icons.Count; i++)
            sumX += icons[i].transform.localPosition.x;

        var avgX = sumX / icons.Count;

        for (var i = 0; i < icons.Count; i++)
        {
            var pos = icons[i].transform.localPosition;
            icons[i].transform.localPosition = new Vector3(pos.x - avgX, pos.y, pos.z);
        }
    }
}
