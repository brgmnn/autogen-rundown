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
    }

    [HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
    [HarmonyPostfix]
    private static void Post_OnEnable(CM_PageRundown_New __instance)
    {
        EventManager.UpdateRundown();
    }

    /// <summary>
    /// When a tier has a single expedition (expCount == 0), the game places it
    /// at ratio=0 which is the left edge of the arc. This patch moves it to
    /// ratio=0.5, the center/front of the ellipse: (0, -ovalSize.y).
    /// </summary>
    [HarmonyPatch(typeof(CM_PageRundown_New), "GetExpIconLocalPos")]
    [HarmonyPostfix]
    private static void Post_GetExpIconLocalPos(int expCount, Vector2 ovalSize, ref Vector3 __result)
    {
        if (expCount == 0)
            __result = new Vector3(0f, -ovalSize.y, 0f);
    }
}
