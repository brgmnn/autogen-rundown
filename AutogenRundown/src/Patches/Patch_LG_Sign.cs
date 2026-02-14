using AutogenRundown.Components;
using AutogenRundown.Managers;
using GameData;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_LG_Sign
{
    [HarmonyPatch(typeof(Builder), nameof(Builder.Build))]
    [HarmonyPostfix]
    private static void Post_Build()
    {
        var activeExp = RundownManager.ActiveExpedition;
        if (activeExp == null || activeExp.LevelLayoutData == 0)
            return;

        var block = GameDataBlockBase<LevelLayoutDataBlock>.GetBlock(activeExp.LevelLayoutData);
        if (block == null)
            return;

        SignBorderManager.SetBorderColor(block.ZoneAliasStart, new Color(1f, 0f, 0f, 0.8f));
    }

    [HarmonyPatch(typeof(LG_Sign), nameof(LG_Sign.SetZoneInfo))]
    [HarmonyPostfix]
    private static void Post_SetZoneInfo(LG_Sign __instance, LG_NavInfo info)
    {
        var color = SignBorderManager.GetBorderColor(info.Number);
        if (color == null)
            return;

        if (__instance.GetComponent<SignBorder>() != null)
            return;

        var border = __instance.gameObject.AddComponent<SignBorder>();
        border.Setup(__instance, color.Value, info.Number);
    }
}
