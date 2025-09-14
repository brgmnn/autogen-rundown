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
    private static CM_ExpeditionIcon_New? TopBarExpeditionIcon { get; set; }

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
        // "Hides" the artifact heat text by moving it off-screen
        __instance.m_artifactHeatText.gameObject.transform.localPosition += Vector3.down * 10_000;

        // Shifts up the level completion text
        __instance.m_statusText.transform.localPosition += new Vector3(0f, 25.0f, 0f);
    }

    /// <summary>
    /// Repositions the level icon in the top bar now that the artifact heat is gone from it
    ///
    /// TODO: can we add the level name where the artifact heat went?
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(CM_MenuBar), nameof(CM_MenuBar.UpdateMenuOptions))]
    [HarmonyPostfix]
    internal static void Post_CM_MenuBar_UpdateMenuOptions(CM_MenuBar __instance)
    {
        // __instance.m_expIconAlign.position.y = 677.5467;

        // TopBarExpeditionIcon = __instance.m_expIcon;
        // TopBarExpeditionIcon.m_artifactHeatText.text = "Test string";

        __instance.m_expIcon.SetPosition(new Vector2(-677.0f, 0f));
    }
}
