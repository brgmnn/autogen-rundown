using AutogenRundown.Managers;
using CellMenu;
using HarmonyLib;
using UnityEngine;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_GUIX_Layer
{
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
    public static void Postfix_GUIX_Layer(GUIX_Layer __instance)
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
}
