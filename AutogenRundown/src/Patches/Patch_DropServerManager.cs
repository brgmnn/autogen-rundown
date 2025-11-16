using AutogenRundown.Managers;
using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_DropServerManager
{
    /// <summary>
    /// Adds support for displaying other messages than "Server Sync"
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__result"></param>
    /// <returns></returns>
    [HarmonyPatch(typeof(DropServerManager), nameof(DropServerManager.GetStatusText))]
    [HarmonyPrefix]
    static bool Prefix_GetStatusText(DropServerManager __instance, ref string __result)
    {
        if (__instance.IsBusy)
            return true;

        if (!FactoryJobManager.ShowMessage)
            return true;

        __result = $"REBUILD #{FactoryJobManager.RebuildCount}";

        return false;
    }
}
