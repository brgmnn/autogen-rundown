using AutogenRundown.Components;
using CellMenu;
using GameData;
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CM_ExpeditionWindow), nameof(CM_ExpeditionWindow.SetExpeditionInfo))]
    private static void Post_SetExpeditionInfo(CM_ExpeditionWindow __instance, ExpeditionInTierData data)
    {
        var complexBlock = GameDataBlockBase<ComplexResourceSetDataBlock>.GetBlock(
            data.Expedition.ComplexResourceData);
        if (complexBlock == null)
            return;

        __instance.m_depthTitle.text += $"\nComplex: <color=yellow>{complexBlock.ComplexType}</color>";

        // Remove artifact heat text to prevent overlap
        __instance.m_artifactHeatTitle.text = "";
    }
}
