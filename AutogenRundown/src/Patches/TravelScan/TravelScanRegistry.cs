using System;
using System.Collections.Generic;
using GTFO.API;

namespace AutogenRundown.Patches.TravelScan;

/// <summary>
/// Static registry tracking which PuzzleType IDs use travel path generation
/// and configuration defaults for path generation.
/// </summary>
public static class TravelScanRegistry
{
    /// <summary>
    /// All PuzzleType IDs that have CP_BasicMovable (moving scan prefabs).
    /// These are the prefab types we override position generation for.
    /// </summary>
    public static readonly HashSet<uint> MovingPuzzleTypes = new()
    {
        22,  // SecurityScan_Big_RequireAll_Movable
        31,  // SecurityScan_Big_Movable_FadeIn
        38,  // SecurityScan_Big_Movable_FadeIn_RequireAll
        42,  // SecurityScan_Big_Movable
        43,  // SecurityScan_Movable_Small
        52,  // SecurityScan_Big_Movable_Slow
        60,  // SecurityScan_Big_Movable_FadeIn_Slow
        100, // SecurityScan_Sustained_Travel
    };

    /// <summary>
    /// PuzzleType IDs that use sustained+travel (CP_BasicMovable injected at runtime).
    /// </summary>
    public static readonly HashSet<uint> SustainedTravelTypes = new() { 100 };

    /// <summary>
    /// Flag set by ChainedPuzzleInstance.Setup prefix when the current puzzle
    /// contains a sustained travel component. Cleared in postfix.
    /// </summary>
    public static bool PendingSustainedTravel;

    /// <summary>
    /// Tracks CP_Bioscan_Core IL2CPP pointers that are sustained travel instances.
    /// </summary>
    public static readonly HashSet<IntPtr> SustainedTravelInstances = new();

    public const float SustainedTravelSpeed = 2.0f;
    public const float SustainedTravelReverseSpeed = 1.0f;

    public const float StepDistance = 3f;
    public const float EdgeDistance = 2f;

    public static void Setup()
    {
        LevelAPI.OnLevelCleanup += Clear;
    }

    public static void Clear()
    {
        SustainedTravelInstances.Clear();
        PendingSustainedTravel = false;
        Patch_SustainedTravelReverse.Clear();
    }
}
