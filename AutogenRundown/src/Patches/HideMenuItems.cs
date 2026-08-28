using CellMenu;
using HarmonyLib;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Hides the vanity / cosmetics widgets and the left-hand buttons (Tutorial, Matchmake, etc.)
/// on the rundown page by moving them off-screen.
///
/// The live build re-places the rundown page content on every rundown switch, so the hide is
/// re-applied from every hook that (re)lays the page out.
/// </summary>
[HarmonyPatch]
public class HideMenuItems
{
    private static readonly (string path, Vector3 position)[] hiddenItems =
    {
        // Cosmetic items (Right-hand side)
        ("MovingContent/Rundown/Button VanityItemDrops", Vector3.up * 1000f),
        ("MovingContent/Rundown/CM_PageRundown_VanityItemDropsNext", Vector3.up * 2000f),

        // Left hand side buttons: Tutorial button, matchmake, etc.
        ("MovingContent/PasteAndJoinOnLobbyID/TutorialButton", Vector3.left * 1000f),
        ("MovingContent/PasteAndJoinOnLobbyID/Button Matchmake All", Vector3.left * 1000f),
        ("MovingContent/PasteAndJoinOnLobbyID/Button Rundown", Vector3.left * 1000f),
        ("MovingContent/PasteAndJoinOnLobbyID/Button Discord", Vector3.left * 1000f),
        ("MovingContent/PasteAndJoinOnLobbyID/ButtonGIF", Vector3.left * 1000f),
    };

    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
    [HarmonyPostfix]
    private static void Post_OnEnable(CM_PageRundown_New __instance) => Hide(__instance);

    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.PlaceRundown))]
    [HarmonyPostfix]
    private static void Post_PlaceRundown(CM_PageRundown_New __instance) => Hide(__instance);

    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnExpeditionUpdated))]
    [HarmonyPostfix]
    private static void Post_OnExpeditionUpdated(CM_PageRundown_New __instance) => Hide(__instance);

    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.UpdateVanityItemUnlocks))]
    [HarmonyPostfix]
    private static void Post_UpdateVanityItemUnlocks(CM_PageRundown_New __instance) => Hide(__instance);

    private static void Hide(CM_PageRundown_New page)
    {
        foreach (var (path, position) in hiddenItems)
        {
            var child = page.gameObject.transform.FindChild(path);

            if (child == null)
            {
                Plugin.Logger.LogWarning($"[HideMenuItems] child not found: {path}");
                continue;
            }

            child.localPosition = position;
        }
    }
}
