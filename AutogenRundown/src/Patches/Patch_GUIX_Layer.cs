using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_GUIX_Layer
{
    // // [HarmonyPatch(typeof(GUIX_Layer), nameof(GUIX_Layer.OnDisable))]
    // [HarmonyPatch(typeof(GUIX_Layer), nameof(GUIX_Layer.OnEnable))]
    // [HarmonyPostfix]
    // public static void Postfix_GUIX_Layer(GUIX_Layer __instance)
    // {
    //     // GUIX_layer_Tier_1
    //
    //     if (__instance.gameObject.name == "GUIX_layer_Tier_1")
    //         Plugin.Logger.LogWarning("GUIX_layer_Tier_1 has been enabled");
    //
    //     // // Check if this transform is under "Rundown_Surface_SelectionALT_R3"
    //     // var current = layer.gameObject.transform;
    //     //
    //     // while (current != null)
    //     // {
    //     //     if (current.name == parentName)
    //     //         return true;
    //     //
    //     //     current = current.parent;
    //     // }
    //     // return false;
    // }
}
