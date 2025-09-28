using AutogenRundown.Components;
using CellMenu;
using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_CM_ExpeditionWindow
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CM_PageExpeditionSuccess), nameof(CM_PageExpeditionSuccess.Setup))]
    private static void Post_Setup(CM_PageExpeditionSuccess __instance)
    {
        if (__instance.GetComponent<ExpeditionSuccessPage_ArchivistIcon>() == null)
        {
            var icon = __instance.gameObject.AddComponent<ExpeditionSuccessPage_ArchivistIcon>();

            icon.m_page = __instance;
            icon.Setup();
        }
    }
}
