using AutogenRundown.Managers;
using CellMenu;
using GameData;
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
    private static void Post_PlaceRundown(CM_PageRundown_New __instance, RundownDataBlock data)
    {
        EventManager.UpdateRundown();

        var visuals = data.StorytellingData.Visuals;
        CenterSingleIcon(__instance.m_expIconsTier1, visuals.TierAVisuals);
        CenterSingleIcon(__instance.m_expIconsTier2, visuals.TierBVisuals);
        CenterSingleIcon(__instance.m_expIconsTier3, visuals.TierCVisuals);
        CenterSingleIcon(__instance.m_expIconsTier4, visuals.TierDVisuals);
        CenterSingleIcon(__instance.m_expIconsTier5, visuals.TierEVisuals);
    }

    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
    [HarmonyPostfix]
    private static void Post_OnEnable(CM_PageRundown_New __instance)
    {
        EventManager.UpdateRundown();
    }

    /// <summary>
    /// When a tier has a single expedition, the game places it at ratio=0
    /// (left edge of the arc). This repositions it to ratio=0.5, the
    /// center/front of the ellipse: (0, -ovalSize.y).
    /// </summary>
    private static void CenterSingleIcon(
        Il2CppSystem.Collections.Generic.List<CM_ExpeditionIcon_New> icons,
        TierVisualData visData)
    {
        if (icons == null || icons.Count != 1)
            return;

        var scale = visData.Scale;
        var ovalY = 450f * scale * visData.ScaleYModifier;
        icons[0].transform.localPosition = new Vector3(0f, -ovalY, 0f);
    }
}
