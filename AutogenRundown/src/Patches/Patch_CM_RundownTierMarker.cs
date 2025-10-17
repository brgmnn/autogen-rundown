using AutogenRundown.Components;
using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_CM_RundownTierMarker
{
    [HarmonyPatch(typeof(CM_RundownTierMarker), nameof(CM_RundownTierMarker.Setup))]
    [HarmonyPostfix]
    private static void Post_Setup(CM_RundownTierMarker __instance)
    {
        var p = __instance.gameObject.AddComponent<RundownTierMarkerArchivist>();
        p.m_tierMarker = __instance;
        p.Setup();
    }
}
