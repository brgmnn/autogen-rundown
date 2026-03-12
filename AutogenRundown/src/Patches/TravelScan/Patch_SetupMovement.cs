using System;
using System.Runtime.InteropServices;
using ChainedPuzzles;
using HarmonyLib;
using Il2CppInterop.Runtime;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches.TravelScan;

/// <summary>
/// Harmony prefix on ChainedPuzzleInstance.SetupMovement that replaces the base game's
/// position generation with a directional walking path for moving scan puzzle types.
///
/// SetupMovement is called during ChainedPuzzleInstance.Setup on all clients during level
/// build, so all clients generate identical paths from the same AI graph and seed.
/// </summary>
[HarmonyPatch(typeof(ChainedPuzzleInstance), "SetupMovement")]
public static class Patch_SetupMovement
{
    // Cached IL2CPP field offsets, resolved once at first use
    private static int _amountOfPositionsOffset = -1;
    private static int _typeOfMovementOffset = -1;
    private static bool _offsetsResolved;

    static bool Prefix(GameObject gameObject, LG_Area sourceArea)
    {
        var movable = gameObject.GetComponent<iChainedPuzzleMovable>();
        if (movable == null)
            return true; // Let base game handle the error logging

        var basicMovable = gameObject.GetComponent<CP_BasicMovable>();
        if (basicMovable == null)
            return true;

        // Only override movable scans (prefabs with movement configured)
        if (!basicMovable.IsMoveConfigured)
            return true;

        Plugin.Logger.LogDebug(
            $"[TravelScan] Generating travel path for movable in area {sourceArea.name}");

        // Generate the looping path
        var pathPositions = TravelPathGenerator.GenerateLoop(
            sourceArea,
            gameObject.transform.position);

        if (pathPositions.Count < 2)
        {
            Plugin.Logger.LogWarning(
                $"[TravelScan] Path generation returned {pathPositions.Count} positions, " +
                "falling back to base game");
            return true;
        }

        // Insert current position as the first waypoint (same as base game)
        pathPositions.Insert(0, gameObject.transform.position);

        // Set the scan positions via the interface property
        var il2cppPositions = new Il2CppSystem.Collections.Generic.List<Vector3>();
        for (int i = 0; i < pathPositions.Count; i++)
            il2cppPositions.Add(pathPositions[i]);

        movable.ScanPositions = il2cppPositions;

        // Fix m_amountOfPositions and m_typeOfMovement via IL2CPP field access.
        // These are [SerializeField] private fields stored in IL2CPP native memory,
        // so we use il2cpp_field_get_offset to write them directly.
        if (TrySetIl2CppFields(basicMovable, pathPositions.Count))
        {
            Plugin.Logger.LogDebug(
                $"[TravelScan] Set {pathPositions.Count} positions with Circular movement");
        }
        else
        {
            Plugin.Logger.LogWarning(
                "[TravelScan] Failed to set IL2CPP fields, scan may not move correctly");
        }

        return false; // Skip the base game's SetupMovement
    }

    private static unsafe bool TrySetIl2CppFields(CP_BasicMovable movable, int positionCount)
    {
        try
        {
            if (!_offsetsResolved)
            {
                ResolveFieldOffsets();
                _offsetsResolved = true;
            }

            if (_amountOfPositionsOffset < 0 || _typeOfMovementOffset < 0)
                return false;

            // Get the IL2CPP object pointer
            var objectPtr = movable.Pointer;
            if (objectPtr == IntPtr.Zero)
                return false;

            // Write m_amountOfPositions (int)
            *(int*)(objectPtr + _amountOfPositionsOffset) = positionCount;

            // Write m_typeOfMovement (MovementType enum, backed by int)
            *(int*)(objectPtr + _typeOfMovementOffset) = (int)MovementType.Circular;

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"[TravelScan] IL2CPP field write failed: {ex.Message}");
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
                Plugin.Logger.LogWarning("[TravelScan] Could not get IL2CPP class pointer");
                return;
            }

            _amountOfPositionsOffset = GetFieldOffset(il2cppClass, "m_amountOfPositions");
            _typeOfMovementOffset = GetFieldOffset(il2cppClass, "m_typeOfMovement");

            Plugin.Logger.LogDebug(
                $"[TravelScan] Field offsets: m_amountOfPositions={_amountOfPositionsOffset}, " +
                $"m_typeOfMovement={_typeOfMovementOffset}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"[TravelScan] Failed to resolve field offsets: {ex.Message}");
        }
    }

    private static int GetFieldOffset(IntPtr classPtr, string fieldName)
    {
        var fieldPtr = IL2CPP.il2cpp_class_get_field_from_name(classPtr, fieldName);
        if (fieldPtr == IntPtr.Zero)
        {
            Plugin.Logger.LogWarning($"[TravelScan] Field '{fieldName}' not found");
            return -1;
        }

        return (int)IL2CPP.il2cpp_field_get_offset(fieldPtr);
    }
}
