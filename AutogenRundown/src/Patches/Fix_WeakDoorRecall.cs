using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

/// <summary>
/// Prevents a NullReferenceException in LG_WeakDoor_Anim.OnDoorState during late-joiner
/// state recall. When a stale door replicator (from a pre-rebuild level) fires into a
/// destroyed Animator, the exception aborts SNet_Capture.RecallBuffer, which prevents all
/// subsequent replicators (including lights) from being recalled.
/// </summary>
[HarmonyPatch]
public class Fix_WeakDoorRecall
{
    [HarmonyPatch(typeof(LG_WeakDoor_Anim), nameof(LG_WeakDoor_Anim.OnDoorState))]
    [HarmonyPrefix]
    public static bool Pre_OnDoorState(LG_WeakDoor_Anim __instance)
    {
        if (__instance.m_operationAnim == null)
        {
            Plugin.Logger.LogWarning(
                "LG_WeakDoor_Anim.OnDoorState: Animator is null/destroyed, " +
                "skipping to prevent recall buffer abort");
            return false;
        }

        return true;
    }
}
