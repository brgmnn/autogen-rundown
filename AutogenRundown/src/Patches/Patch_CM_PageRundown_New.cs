using AutogenRundown.Managers;
using CellMenu;
using HarmonyLib;

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
}
