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

    /// <summary>
    /// Longest gap allowed between consecutive waypoints. Decimation splits anything wider using
    /// points from the trace, so this is a spacing target rather than a repair threshold.
    /// </summary>
    public const float StepDistance = 2f;

    /// <summary>
    /// Walkable (area 0) only, for every NavMesh query the path generator makes.
    ///
    /// An areaMask of -1 admits Jump (area 2) and Ladder (area 3) off-mesh links.
    /// LG_Ladder.BuildOffmeshLink spawns ten of them per ladder, and a path routed through one
    /// yields a vertical pair of corners with nothing walkable in between — the scan drops
    /// straight through the floor. The game uses a mask for the same reason: PlayerBotActionTravel
    /// passes 17 (Walkable | PlayerBot).
    /// </summary>
    public const int WalkableAreaMask = 1;

    /// <summary>
    /// Reject a snap that moves a point further than this from its reference height — it means we
    /// found a different floor. CP_PlayerScanner tests scan membership with a full 3D sphere, so a
    /// destination on the wrong floor produces a bubble players cannot stand in.
    ///
    /// Used only when placing destinations, which start from AI graph nodes and gates rather than
    /// from the surface itself.
    /// </summary>
    public const float MaxSurfaceSnapRise = 1.0f;

    /// <summary>
    /// Edge length of the XZ buckets NavSurface indexes triangles into. Triangles are far smaller
    /// than this, so nearly all land in one or two cells.
    /// </summary>
    public const float TriangleCellSize = 4f;

    /// <summary>
    /// How far past a triangle edge the trace steps before asking which triangle it is now in.
    /// Also the guaranteed minimum advance per crossing, which is what bounds the march.
    /// </summary>
    public const float EdgeNudge = 0.01f;

    /// <summary>
    /// How far a point may sit from the surface before the trace gives up on it. Covers a
    /// pathfinder corner placed exactly on a mesh boundary, and a crossing that lands in the sliver
    /// of a gap where two 64m bake tiles meet without welded vertices.
    /// </summary>
    public const float LocateRadius = 0.5f;

    /// <summary>
    /// Largest height change accepted between one triangle and the next.
    ///
    /// This is agentClimb from the bake (LG_BuildUnityGraphJob), the most two adjacent walkable
    /// polygons can legitimately differ by at their shared edge. Anything larger is a different
    /// floor — which is exactly the ledge the scan used to chord straight across.
    /// </summary>
    public const float MaxTraceStep = 0.5f;

    /// <summary>
    /// How far a waypoint-to-waypoint chord may deviate from the traced surface. Decimation keeps
    /// any point whose removal would exceed this, so every fold — each stair nose, each crest —
    /// survives while flat runs thin out.
    ///
    /// Measured against the actual traced polyline rather than a re-sampled guess, so unlike the
    /// sag tolerance it replaces, it means what it says.
    /// </summary>
    public const float MaxTraceDeviation = 0.05f;

    /// <summary>
    /// Never emit a segment shorter than this. DoMoveScanner divides by segment length, and
    /// Patch_SustainedTravelReverse.ReverseMovement bails out below 0.001m — degenerate segments
    /// either teleport the scan or stall reverse movement permanently.
    ///
    /// Purely a division guard. It is deliberately far below the spacing of real geometry: at the
    /// bake's 8.3cm voxel size consecutive folds on a fine staircase can be a few centimetres
    /// apart, and thinning those away is what put chords through the floor in the first place.
    /// </summary>
    public const float MinSegmentLength = 0.05f;

    /// <summary>
    /// The baked walkable surface for the current level, extracted once and shared by every scan
    /// on it. Null until first use; null again after cleanup.
    /// </summary>
    private static NavSurface? surface;

    private static bool surfaceAttempted;

    /// <summary>
    /// The current level's surface, extracting it on first use.
    ///
    /// Built lazily rather than from a level-build hook because path generation runs during
    /// ChainedPuzzleInstance.SetupMovement, by which point the navmesh is demonstrably live —
    /// NavMesh.CalculatePath already succeeds there. Returns null if extraction failed, and does
    /// not retry within a level.
    /// </summary>
    public static NavSurface? GetSurface()
    {
        if (surfaceAttempted)
            return surface;

        surfaceAttempted = true;
        surface = NavSurface.Build();

        return surface;
    }

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

        // The triangulation is a snapshot of one level's bake, and it is large. Dropping it here
        // both frees the memory and stops the next level tracing against the previous one's floors.
        surface = null;
        surfaceAttempted = false;

#if DEBUG
        GeneratedPaths.Clear();
#endif
    }
}
