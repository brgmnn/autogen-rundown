using System;
using ChainedPuzzles;
using GameData;
using HarmonyLib;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace AutogenRundown.Patches.TravelScan;

/// <summary>
/// Sets <see cref="TravelScanRegistry.PendingSustainedTravel"/> when the current
/// ChainedPuzzleInstance contains a sustained-travel puzzle component.
/// </summary>
[HarmonyPatch(typeof(ChainedPuzzleInstance), nameof(ChainedPuzzleInstance.Setup))]
public static class Patch_ChainedPuzzleInstance_Setup
{
    static void Prefix(ChainedPuzzleDataBlock data)
    {
        TravelScanRegistry.PendingSustainedTravel = false;

        if (data?.ChainedPuzzle == null)
            return;

        for (int i = 0; i < data.ChainedPuzzle.Count; i++)
        {
            if (TravelScanRegistry.SustainedTravelTypes.Contains(data.ChainedPuzzle[i].PuzzleType))
            {
                TravelScanRegistry.PendingSustainedTravel = true;
                Plugin.Logger.LogDebug("[SustainedTravel] Detected sustained travel component in puzzle setup");
                return;
            }
        }
    }

    static void Postfix()
    {
        TravelScanRegistry.PendingSustainedTravel = false;
    }
}

/// <summary>
/// Injects <see cref="CP_BasicMovable"/> onto sustained-travel bioscan cores so that
/// the base <c>CP_Bioscan_Core.Setup</c> recognises them as movable and the existing
/// <see cref="Patch_SetupMovement"/> generates a NavMesh path.
/// </summary>
[HarmonyPatch(typeof(CP_Bioscan_Core), nameof(CP_Bioscan_Core.Setup))]
public static class Patch_CP_Bioscan_Core_Setup
{
    // Cached IL2CPP field offsets for CP_BasicMovable
    private static int _amountOfPositionsOffset = -1;
    private static int _typeOfMovementOffset = -1;
    private static int _movementSpeedOffset = -1;
    private static bool _offsetsResolved;

    static void Prefix(CP_Bioscan_Core __instance)
    {
        if (!TravelScanRegistry.PendingSustainedTravel)
            return;

        if (__instance.m_movingComp != null)
            return;

        Plugin.Logger.LogDebug("[SustainedTravel] Injecting CP_BasicMovable onto sustained scan");

        var movable = __instance.gameObject.AddComponent<CP_BasicMovable>();

        if (!TrySetIl2CppFields(movable))
        {
            Plugin.Logger.LogWarning("[SustainedTravel] Failed to set IL2CPP fields on injected CP_BasicMovable");
            return;
        }

        __instance.m_movingComp = movable;

        Plugin.Logger.LogDebug("[SustainedTravel] CP_BasicMovable injected and assigned to m_movingComp");
    }

    private static unsafe bool TrySetIl2CppFields(CP_BasicMovable movable)
    {
        try
        {
            if (!_offsetsResolved)
            {
                ResolveFieldOffsets();
                _offsetsResolved = true;
            }

            if (_amountOfPositionsOffset < 0 || _typeOfMovementOffset < 0 || _movementSpeedOffset < 0)
                return false;

            var objectPtr = movable.Pointer;
            if (objectPtr == IntPtr.Zero)
                return false;

            // m_amountOfPositions (int) — placeholder, will be overwritten by SetupMovement
            *(int*)(objectPtr + _amountOfPositionsOffset) = 2;

            // m_typeOfMovement (MovementType enum, backed by int) — Circular = 2
            *(int*)(objectPtr + _typeOfMovementOffset) = (int)MovementType.Circular;

            // m_movementSpeed (float)
            *(float*)(objectPtr + _movementSpeedOffset) = TravelScanRegistry.SustainedTravelSpeed;

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"[SustainedTravel] IL2CPP field write failed: {ex.Message}");
            return false;
        }
    }

    private static void ResolveFieldOffsets()
    {
        try
        {
            var il2cppClass = Il2CppClassPointerStore<CP_BasicMovable>.NativeClassPtr;
            if (il2cppClass == IntPtr.Zero)
            {
                Plugin.Logger.LogWarning("[SustainedTravel] Could not get IL2CPP class pointer for CP_BasicMovable");
                return;
            }

            _amountOfPositionsOffset = GetFieldOffset(il2cppClass, "m_amountOfPositions");
            _typeOfMovementOffset = GetFieldOffset(il2cppClass, "m_typeOfMovement");
            _movementSpeedOffset = GetFieldOffset(il2cppClass, "m_movementSpeed");

            Plugin.Logger.LogDebug(
                $"[SustainedTravel] Field offsets: m_amountOfPositions={_amountOfPositionsOffset}, " +
                $"m_typeOfMovement={_typeOfMovementOffset}, m_movementSpeed={_movementSpeedOffset}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"[SustainedTravel] Failed to resolve field offsets: {ex.Message}");
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
}
