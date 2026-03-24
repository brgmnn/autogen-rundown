using System;
using System.Collections.Generic;
using ChainedPuzzles;
using HarmonyLib;
using Il2CppInterop.Runtime;
using SNetwork;
using UnityEngine;

namespace AutogenRundown.Patches.TravelScan;

/// <summary>
/// Drives sustained travel scans backwards when not all players are in the scan,
/// creating pressure to stay together. When all players return, the scan resumes
/// forward from its reversed position.
/// </summary>
[HarmonyPatch(typeof(CP_Bioscan_Core))]
public static class Patch_SustainedTravelReverse
{
    private static readonly Dictionary<IntPtr, bool> _reversing = new();

    // IL2CPP field offsets for CP_BasicMovable
    private static int _lerpAmountOffset = -1;
    private static int _resetOffset = -1;
    private static int _currentStateOffset = -1; // pMovableStateSync struct; lerp at +0, paused at +4
    private static bool _offsetsResolved;

    [HarmonyPostfix]
    [HarmonyPatch("OnSyncStateChange")]
    [HarmonyPriority(Priority.Low)] // Run after other postfixes (e.g., LobbyExpansion)
    static void OnSyncStateChange_Postfix(
        CP_Bioscan_Core __instance,
        eBioscanStatus status)
    {
        if (!TravelScanRegistry.SustainedTravelInstances.Contains(__instance.Pointer))
            return;

        var movable = __instance.m_movingComp;

        if (movable == null)
            return;

        // Derive movement decision from authoritative sync state, not from the paused
        // field which other mods (e.g., LobbyExpansion) may have modified using
        // physical-position counting that oscillates for moving scans.
        var shouldMove = false;

        if (status == eBioscanStatus.Scanning && movable.OnlyMoveWhenScannig)
        {
            var state = __instance.State;
            var requirement = __instance.PlayerScanner.ScanPlayersRequired;

            shouldMove = requirement == PlayerRequirement.None ||
                         (requirement.RequireAllPlayers() && state.playersInScan == state.playersMax) ||
                         (requirement.RequireSoloPlayer() && state.playersInScan == 1);
        }

        // Override pause/resume on all machines so the client-side Update_Postfix
        // (which reads paused) also sees the correct state.
        if (shouldMove)
            movable.ResumeMovement();
        else if (status == eBioscanStatus.Scanning || status == eBioscanStatus.Waiting)
            movable.PauseMovement();

        // Reverse logic is master-only
        if (!SNet.IsMaster)
            return;

        var wasReversing = _reversing.TryGetValue(__instance.Pointer, out var rev) && rev;

        if (!shouldMove && (status == eBioscanStatus.Scanning || status == eBioscanStatus.Waiting))
        {
            var lerpAmount = ReadLerpAmount(movable);

            if (lerpAmount > 0f)
            {
                if (!wasReversing)
                    Plugin.Logger.LogDebug("[SustainedTravel] Starting reverse movement");

                _reversing[__instance.Pointer] = true;
                return;
            }
        }

        // All players in scan, not scanning, or at origin — stop reversing
        if (wasReversing)
        {
            Plugin.Logger.LogDebug("[SustainedTravel] Stopping reverse, resetting movement");
            _reversing[__instance.Pointer] = false;

            // Force coroutine restart from current (reversed) position
            WriteReset(movable, true);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CP_Bioscan_Core.Update))]
    static void Update_Postfix(CP_Bioscan_Core __instance)
    {
        if (!TravelScanRegistry.SustainedTravelInstances.Contains(__instance.Pointer))
            return;

        var movable = __instance.m_movingComp;

        if (movable == null)
            return;

        if (!SNet.IsMaster)
        {
            // Client: independently drive reverse movement per-frame while paused.
            // The master's periodic sync (~0.3s) corrects drift via OnStateChange.
            if (!ReadPaused(movable))
                return;

            ReverseMovement(movable, writeSyncState: false);
            return;
        }

        // Master: drive reverse movement
        if (!_reversing.TryGetValue(__instance.Pointer, out var rev) || !rev)
            return;

        ReverseMovement(movable, writeSyncState: true);
    }

    private static void ReverseMovement(CP_BasicMovable movable, bool writeSyncState)
    {
        var lerpAmount = ReadLerpAmount(movable);

        if (lerpAmount <= 0f)
            return;

        var currentIndex = (int)lerpAmount;
        var nextIndex = currentIndex + 1;
        var amountOfPositions = movable.AmountOfPositions;

        if (nextIndex >= amountOfPositions)
            nextIndex = 0;

        var positions = movable.ScanPositions;
        var segmentDistance = Vector3.Distance(
            positions[currentIndex], positions[nextIndex]);

        if (segmentDistance < 0.001f)
            return;

        var delta = Clock.Delta * TravelScanRegistry.SustainedTravelReverseSpeed / segmentDistance;
        lerpAmount -= delta;

        if (lerpAmount < 0f)
            lerpAmount = 0f;

        // Recompute position from updated lerp
        var newCurrentIndex = (int)lerpAmount;
        var newNextIndex = newCurrentIndex + 1;

        if (newNextIndex >= amountOfPositions)
            newNextIndex = 0;

        var t = lerpAmount % 1f;
        movable.transform.position = Vector3.Lerp(
            positions[newCurrentIndex], positions[newNextIndex], t);

        // Write m_lerpAmount so the next frame reads the decremented value.
        // On master, also write m_currentState.lerp for the sync routine.
        WriteLerpAmount(movable, lerpAmount);

        if (writeSyncState)
            WriteCurrentStateLerp(movable, lerpAmount);
    }

    private static unsafe float ReadLerpAmount(CP_BasicMovable movable)
    {
        EnsureOffsets();

        if (_lerpAmountOffset < 0)
            return 0f;

        var ptr = movable.Pointer;

        if (ptr == IntPtr.Zero)
            return 0f;

        return *(float*)(ptr + _lerpAmountOffset);
    }

    private static unsafe void WriteLerpAmount(CP_BasicMovable movable, float value)
    {
        if (_lerpAmountOffset < 0)
            return;

        var ptr = movable.Pointer;

        if (ptr == IntPtr.Zero)
            return;

        *(float*)(ptr + _lerpAmountOffset) = value;
    }

    private static unsafe void WriteCurrentStateLerp(CP_BasicMovable movable, float value)
    {
        if (_currentStateOffset < 0)
            return;

        var ptr = movable.Pointer;

        if (ptr == IntPtr.Zero)
            return;

        // lerp is the first field in pMovableStateSync (offset 0 within struct)
        *(float*)(ptr + _currentStateOffset) = value;
    }

    private static unsafe bool ReadPaused(CP_BasicMovable movable)
    {
        EnsureOffsets();

        if (_currentStateOffset < 0)
            return false;

        var ptr = movable.Pointer;

        if (ptr == IntPtr.Zero)
            return false;

        // paused is the second field in pMovableStateSync (after float lerp, at +4)
        return *(bool*)(ptr + _currentStateOffset + 4);
    }

    internal static unsafe void WriteReset(CP_BasicMovable movable, bool value)
    {
        EnsureOffsets();

        if (_resetOffset < 0)
            return;

        var ptr = movable.Pointer;

        if (ptr == IntPtr.Zero)
            return;

        *(bool*)(ptr + _resetOffset) = value;
    }

    private static void EnsureOffsets()
    {
        if (_offsetsResolved)
            return;

        _offsetsResolved = true;

        try
        {
            var il2cppClass = Il2CppClassPointerStore<CP_BasicMovable>.NativeClassPtr;

            if (il2cppClass == IntPtr.Zero)
            {
                Plugin.Logger.LogWarning("[SustainedTravel] Could not get IL2CPP class pointer for reverse offsets");
                return;
            }

            _lerpAmountOffset = GetFieldOffset(il2cppClass, "m_lerpAmount");
            _resetOffset = GetFieldOffset(il2cppClass, "m_reset");
            _currentStateOffset = GetFieldOffset(il2cppClass, "m_currentState");

            Plugin.Logger.LogDebug(
                $"[SustainedTravel] Reverse offsets: m_lerpAmount={_lerpAmountOffset}, " +
                $"m_reset={_resetOffset}, m_currentState={_currentStateOffset}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"[SustainedTravel] Failed to resolve reverse offsets: {ex.Message}");
        }
    }

    private static int GetFieldOffset(IntPtr classPtr, string fieldName)
    {
        var fieldPtr = IL2CPP.il2cpp_class_get_field_from_name(classPtr, fieldName);

        if (fieldPtr == IntPtr.Zero)
        {
            Plugin.Logger.LogWarning($"[SustainedTravel] Field '{fieldName}' not found");
            return -1;
        }

        return (int)IL2CPP.il2cpp_field_get_offset(fieldPtr);
    }

    public static void Clear()
    {
        _reversing.Clear();
    }
}

/// <summary>
/// Resets the movement coroutine on clients when a sustained travel scan transitions
/// from paused (reversing) to unpaused (forward). Without this, the client's
/// DoMoveScanner coroutine retains stale captured segment endpoints and diverges
/// from the master's position.
/// </summary>
[HarmonyPatch(typeof(CP_BasicMovable), nameof(CP_BasicMovable.OnStateChange))]
public static class Patch_BasicMovable_OnStateChange
{
    static void Postfix(
        CP_BasicMovable __instance,
        pMovableStateSync oldState,
        pMovableStateSync newState,
        bool isRecall)
    {
        if (SNet.IsMaster)
            return;

        if (isRecall)
            return;

        if (!TravelScanRegistry.SustainedTravelMovables.Contains(__instance.Pointer))
            return;

        // Detect pause → unpause transition (reverse stopped, forward resuming)
        if (oldState.paused && !newState.paused)
        {
            Patch_SustainedTravelReverse.WriteReset(__instance, true);
        }
    }
}
