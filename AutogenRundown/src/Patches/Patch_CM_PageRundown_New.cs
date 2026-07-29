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

        ScrambleFailedIcons(__instance);
    }

    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
    [HarmonyPostfix]
    private static void Post_OnEnable(CM_PageRundown_New __instance)
    {
        EventManager.UpdateRundown();

        ScrambleFailedIcons(__instance);
    }

    /// <summary>
    /// Re-applies the scrambled state to icons for levels that failed to generate.
    ///
    /// The data block mutation done at startup is picked up by PlaceTier, but a level that fails
    /// during this session is locked after its icon was already placed. The game's private
    /// UpdateExpeditionIconProgression() has run by the time we get here, so we set the status
    /// ourselves rather than waiting for the next refresh.
    /// </summary>
    private static void ScrambleFailedIcons(CM_PageRundown_New page)
    {
        // m_expIconsAll is allocated but never filled by PlaceTier, only the per tier lists are
        ScrambleTier(page.m_expIconsTier1);
        ScrambleTier(page.m_expIconsTier2);
        ScrambleTier(page.m_expIconsTier3);
        ScrambleTier(page.m_expIconsTier4);
        ScrambleTier(page.m_expIconsTier5);
    }

    private static void ScrambleTier(Il2CppSystem.Collections.Generic.List<CM_ExpeditionIcon_New> icons)
    {
        if (icons == null)
            return;

        foreach (var icon in icons)
        {
            if (icon?.DataBlock == null)
                continue;

            if (icon.Accessibility == eExpeditionAccessibility.BlockedAndScrambled)
                continue;

            if (!BuildFailureManager.IsLocked(icon.DataBlock.LevelLayoutData))
                continue;

            // Blocks the expedition window click callback
            icon.Accessibility = eExpeditionAccessibility.BlockedAndScrambled;

            // Randomised hex name + DECRYPT ERROR text
            icon.SetStatus(eExpeditionIconStatus.LockedAndScrambled);
        }
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
