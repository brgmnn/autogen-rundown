using AutogenRundown.Managers;
using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch(typeof(PUI_Watermark), nameof(PUI_Watermark.UpdateWatermark))]
public class Watermark
{
    [HarmonyAfter("com.dak.MTFO")]
    public static void Postfix(PUI_Watermark __instance)
    {
        WatermarkManager.SetInstance(__instance);
    }
}
