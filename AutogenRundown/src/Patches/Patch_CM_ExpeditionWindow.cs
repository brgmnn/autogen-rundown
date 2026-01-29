using AutogenRundown.Components;
using CellMenu;
using HarmonyLib;
using UnityEngine;
using LevelGeneration;
using SNetwork;

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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CM_PageExpeditionSuccess), nameof(CM_PageExpeditionSuccess.OnEnable))]
    private static void Post_OnEnable_Success(CM_PageExpeditionSuccess __instance)
    {
        __instance.m_ArtifactInventoryDisplay.transform.localPosition = Vector3.up * 2000f;
        __instance.m_artifactInfo_text.transform.localPosition = Vector3.up * 2000f;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CM_PageExpeditionFail), nameof(CM_PageExpeditionFail.OnEnable))]
    private static void Post_OnEnable_Fail(CM_PageExpeditionFail __instance)
    {
        __instance.m_ArtifactInventoryDisplay.transform.localPosition = Vector3.up * 2000f;
        __instance.m_artifactInfo_text.transform.localPosition = Vector3.up * 2000f;
    }
}
