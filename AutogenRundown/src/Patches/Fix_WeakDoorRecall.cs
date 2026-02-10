using HarmonyLib;
using LevelGeneration;
using SNetwork;

namespace AutogenRundown.Patches;

/// <summary>
/// Prevents NullReferenceExceptions during late-joiner state recall when stale door
/// replicators (from a pre-rebuild level) fire callbacks into destroyed GameObjects.
/// The exception aborts SNet_Capture.RecallBuffer, which prevents all subsequent
/// replicators (including lights) from being recalled.
/// </summary>
[HarmonyPatch]
public class Fix_WeakDoorRecall
{
    /// <summary>
    /// Catch-all safety net: suppresses any exception thrown during a single replicator's
    /// recall so that SNet_Replication.RecallBytes can continue to the next replicator.
    /// Without this, a NullRef from any stale replicator type (weak locks, terminals,
    /// generators, etc.) aborts the entire recall loop — breaking lights and other state.
    /// </summary>
    [HarmonyPatch(typeof(SNet_Replicator), nameof(SNet_Replicator.RevieveBytes))]
    [HarmonyFinalizer]
    public static Exception? Fin_RevieveBytes(Exception __exception)
    {
        if (__exception != null)
        {
            Plugin.Logger.LogWarning(
                $"SNet_Replicator.RevieveBytes: suppressing exception during recall " +
                $"to prevent buffer abort: {__exception.Message}");
        }

        return null;
    }

    /// <summary>
    /// Broad guard: skip ALL door recall callbacks when the door sync object is destroyed.
    /// LG_Door_Sync.OnStateChange is the single entry point for all door state replication
    /// (security doors, weak doors, bulkheads, etc.). Guarding here when isDropinState==true
    /// catches all downstream crashes from stale door replicators in one patch.
    /// </summary>
    [HarmonyPatch(typeof(LG_Door_Sync), nameof(LG_Door_Sync.OnStateChange))]
    [HarmonyPrefix]
    public static bool Pre_DoorSyncOnStateChange(
        LG_Door_Sync __instance,
        bool isDropinState)
    {
        if (isDropinState && (UnityEngine.Object)__instance == null)
        {
            Plugin.Logger.LogWarning(
                "LG_Door_Sync.OnStateChange: door is destroyed, " +
                "skipping stale recall to prevent recall buffer abort");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Belt-and-suspenders: handles the case where the door sync is alive but the animator
    /// component is destroyed.
    /// </summary>
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
