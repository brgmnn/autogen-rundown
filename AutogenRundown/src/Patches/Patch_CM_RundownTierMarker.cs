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
        // Setup can run more than once on the same marker. Without this we stack a second
        // component, subscription and sector icon on top of the first.
        if (__instance.gameObject.GetComponent<RundownTierMarkerArchivist>() != null)
            return;

        var p = __instance.gameObject.AddComponent<RundownTierMarkerArchivist>();
        p.m_tierMarker = __instance;
        p.Setup();
    }
}
