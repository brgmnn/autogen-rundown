﻿using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch(typeof(PUI_Watermark), nameof(PUI_Watermark.UpdateWatermark))]
public class Watermark
{
    [HarmonyAfter("com.dak.MTFO")]
    public static void Postfix(PUI_Watermark __instance)
    {
        __instance.m_watermarkText.SetText($"<size=14>Seed <color=orange>{Generator.Seed}</color>\nAR v{Plugin.Version}</size>");
    }
}
