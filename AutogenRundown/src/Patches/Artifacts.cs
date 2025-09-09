using CellMenu;
using HarmonyLib;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Patches for artifacts and boosters
/// </summary>
[HarmonyPatch]
internal static class Artifacts
{
    /// <summary>
    /// Fixes boosters to not be consumed upon use in levels
    /// </summary>
    /// <returns></returns>
    [HarmonyPatch(typeof(DropServerGameSession), nameof(DropServerGameSession.ConsumeBoosters))]
    [HarmonyPrefix]
    internal static bool Pre_DropServerGameSession_ConsumeBoosters()
    {
        Plugin.Logger.LogInfo($"Skipping booster consumption");
        return false;
    }

    /// <summary>
    /// Hides the artifact heat from the rundown level selection screen
    ///
    /// Unity game object path
    ///     GUI/CellUI_Camera(Clone)/MainMenuLayer/CM_PageRundown_New_CellUI_ALT(Clone)/MovingContent/Rundown/GUIX_layer_Tier_2/CM_ExpeditionIcon_New(Clone)/
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(CM_ExpeditionIcon_New), nameof(CM_ExpeditionIcon_New.Setup))]
    [HarmonyPostfix]
    internal static void Post_CM_ExpeditionIcon_New_Setup(CM_ExpeditionIcon_New __instance)
    {
        // "Hides" the artifact heat text by moving it off screen
        __instance.m_artifactHeatText.gameObject.transform.localPosition += Vector3.down * 10_000;

        // Shifts up the level completion text
        __instance.m_statusText.transform.localPosition += new Vector3(0f, 30.0f, 0f);
    }
}
