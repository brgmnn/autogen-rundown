using GTFO.API;
#if DEBUG
using UnityEngine;
#endif

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

    /// <summary>
    /// Tracks CP_BasicMovable IL2CPP pointers for sustained travel instances.
    /// Keyed on movable pointer for O(1) lookup from movable-side patches.
    /// </summary>
    public static readonly HashSet<IntPtr> SustainedTravelMovables = new();

    public const float SustainedTravelSpeed = 2.0f;
    public const float SustainedTravelReverseSpeed = 1.0f;

    public const float StepDistance = 2f;
    public const float EdgeDistance = 2f;

    /// <summary>
    /// Vertical bias applied before sampling the NavMesh. Matches the game's own convention in
    /// CP_Holopath_Spline.TryGetPosOnNavMesh, which samples at pos + Vector3.up * 0.15f.
    /// </summary>
    public const float SurfaceSampleLift = 0.15f;

    /// <summary>
    /// Max distance NavMesh.SamplePosition may travel when snapping a waypoint to the surface.
    /// </summary>
    public const float SurfaceSampleRadius = 1.5f;

    /// <summary>
    /// Reject a snap that moves a point further than this vertically — it means we found a
    /// different floor. CP_PlayerScanner tests scan membership with a full 3D sphere, so a
    /// waypoint on the wrong floor produces a bubble players cannot stand in.
    /// </summary>
    public const float MaxSurfaceSnapRise = 1.5f;

    /// <summary>
    /// How far the straight chord between two waypoints may pass below the walkable surface
    /// before another waypoint is inserted. CP_BasicMovable.DoMoveScanner lerps in a straight
    /// line between consecutive positions, so an unsplit chord across the crest of a staircase
    /// drags the scan through the floor.
    /// </summary>
    public const float MaxChordSag = 0.25f;

    /// <summary>
    /// Never emit a segment shorter than this. DoMoveScanner divides by segment length, and
    /// Patch_SustainedTravelReverse.ReverseMovement bails out below 0.001m — degenerate
    /// segments either teleport the scan or stall reverse movement permanently.
    /// </summary>
    public const float MinSegmentLength = 0.35f;

    /// <summary>
    /// Bounds subdivision to at most 2^4 - 1 = 15 inserted points per original segment.
    /// </summary>
    public const int MaxSubdivisionDepth = 4;

#if DEBUG
    /// <summary>
    /// Every travel path generated during the current level build, exactly as handed to
    /// CP_BasicMovable.ScanPositions. Consumed by TravelPathDebugDraw on OnBuildDone.
    /// </summary>
    public static readonly List<List<Vector3>> GeneratedPaths = new();
#endif

    public static void Setup()
    {
        LevelAPI.OnLevelCleanup += Clear;

#if DEBUG
        LevelAPI.OnBuildDone += TravelPathDebugDraw.DrawAll;
#endif
    }

    public static void Clear()
    {
        SustainedTravelInstances.Clear();
        SustainedTravelMovables.Clear();
        PendingSustainedTravel = false;
        Patch_SustainedTravelReverse.Clear();

#if DEBUG
        GeneratedPaths.Clear();
#endif
    }
}
