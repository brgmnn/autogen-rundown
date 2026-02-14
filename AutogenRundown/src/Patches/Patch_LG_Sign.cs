using AutogenRundown.Components;
using AutogenRundown.Managers;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_LG_Sign
{
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
