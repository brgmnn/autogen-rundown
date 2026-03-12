using System.Collections.Generic;

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
        22, // SecurityScan_Big_RequireAll_Movable
        31, // SecurityScan_Big_Movable_FadeIn
        38, // SecurityScan_Big_Movable_FadeIn_RequireAll
        42, // SecurityScan_Big_Movable
        43, // SecurityScan_Movable_Small
        52, // SecurityScan_Big_Movable_Slow
        60, // SecurityScan_Big_Movable_FadeIn_Slow
    };

    public const int WaypointCount = 24;
    public const float StepDistance = 4f;
    public const float DirectionVariation = 0.3f;
    public const float EdgeDistance = 1f;
}
