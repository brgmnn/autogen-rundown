using AutogenRundown.Managers;
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
    /// Either hides or sets the logs that have been read here
    ///
    /// Unity game object path
    ///     GUI/CellUI_Camera(Clone)/MainMenuLayer/CM_PageRundown_New_CellUI_ALT(Clone)/MovingContent/Rundown/GUIX_layer_Tier_2/CM_ExpeditionIcon_New(Clone)/
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(CM_ExpeditionIcon_New), nameof(CM_ExpeditionIcon_New.Setup))]
    [HarmonyPostfix]
    internal static void Post_CM_ExpeditionIcon_New_Setup(CM_ExpeditionIcon_New __instance)
    {
        LogArchivistManager.RegisterIcon(__instance);
    }

    [HarmonyPatch(typeof(CM_ExpeditionIcon_New), nameof(CM_ExpeditionIcon_New.SetArtifactHeat))]
    [HarmonyPostfix]
    internal static void Postfix_CM_ExpeditionIcon_New_SetArtifactHeat(CM_ExpeditionIcon_New __instance)
    {
        LogArchivistManager.RegisterIcon(__instance);
    }
}
