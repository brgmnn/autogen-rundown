using CellMenu;
using HarmonyLib;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Hides the vanity / cosmetics items widgets on the rundown pages
/// </summary>
[HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnExpeditionUpdated))]
[HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.UpdateVanityItemUnlocks))]
public class HideMenuItems
{
    public static void Postfix(CM_PageRundown_New __instance)
    {
        // Cosmetic items (Right-hand side)
        __instance.gameObject.transform.FindChild("MovingContent/Rundown/Button VanityItemDrops").transform
            .localPosition = Vector3.up * 1000f;
        __instance.gameObject.transform.FindChild("MovingContent/Rundown/CM_PageRundown_VanityItemDropsNext").transform
            .localPosition = Vector3.up * 2000f;

        // Left hand side buttons: Tutorial button, matchmake, etc.
        // TODO: what event will make these work?
        __instance.gameObject.transform.FindChild("MovingContent/PasteAndJoinOnLobbyID/TutorialButton")
            .transform.localPosition = Vector3.left * 1000f;
        __instance.gameObject.transform.FindChild("MovingContent/PasteAndJoinOnLobbyID/Button Matchmake All")
            .transform.localPosition = Vector3.left * 1000f;
        __instance.gameObject.transform.FindChild("MovingContent/PasteAndJoinOnLobbyID/Button Rundown")
            .transform.localPosition = Vector3.left * 1000f;
        __instance.gameObject.transform.FindChild("MovingContent/PasteAndJoinOnLobbyID/Button Discord")
            .transform.localPosition = Vector3.left * 1000f;
        __instance.gameObject.transform.FindChild("MovingContent/PasteAndJoinOnLobbyID/ButtonGIF")
            .transform.localPosition = Vector3.left * 1000f;
    }
}
