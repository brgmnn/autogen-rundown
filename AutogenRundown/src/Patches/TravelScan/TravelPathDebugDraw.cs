#if DEBUG
using UnityEngine;

namespace AutogenRundown.Patches.TravelScan;

/// <summary>
/// Renders generated travel scan paths in-game: a sphere per waypoint, a cone per segment.
///
/// Uses the game's own DebugDraw3D manager, which is registered in Global.m_ManagersAlwaysLoaded
/// and set up by Global.SetupManagers in retail builds, so its prefabs, materials, layers and
/// level-cleanup are all handled for us.
///
/// Segments are coloured by residual sag: green where the straight chord stays above the walkable
/// surface, red where it still dips below it. Red segments are exactly the ones where the scan
/// will clip through the floor.
/// </summary>
internal static class TravelPathDebugDraw
{
    /// <summary>
    /// DebugDraw3D's pools are GameObjectPoolType.LoopAndReuse at a fixed size (200 spheres and
    /// 200 cones). In that mode GameObjectPool.GetPooledObject ignores CanCreateNew and simply
    /// wraps m_loopIndex, silently handing back a shape that is still in use. Stay under the
    /// pool size — longer paths are drawn with a stride and the stride is logged.
    /// </summary>
    private const int MaxDrawnWaypoints = 180;

    /// <summary>
    /// DebugDraw3D removes a shape once Clock.Time passes its timer. This is effectively
    /// "until level cleanup", which clears every active shape anyway.
    /// </summary>
    private const float Persist = 1_000_000f;

    private const float WaypointRadius = 0.18f;
    private const float OriginRadius = 0.30f;
    private const float SegmentRadius = 0.05f;

    /// <summary>
    /// How far a waypoint may sit from the surface before it counts as off-mesh. Small enough to
    /// catch the drift that put a run of waypoints under the floor, loose enough to ignore the
    /// sub-centimetre wobble of repeated sampling.
    /// </summary>
    private const float OnMeshTolerance = 0.1f;

    private static readonly Color Orange = new(1f, 0.55f, 0f);

    private static bool _warnedUnavailable;

    public static void DrawAll()
    {
        var paths = TravelScanRegistry.GeneratedPaths;

        if (paths.Count == 0)
            return;

        if (DebugDraw3D.Current == null)
        {
            if (!_warnedUnavailable)
            {
                _warnedUnavailable = true;
                Plugin.Logger.LogWarning(
                    "[TravelPathDebug] DebugDraw3D.Current is null, cannot draw travel paths");
            }

            return;
        }

        for (var i = 0; i < paths.Count; i++)
            Draw(paths[i], i);
    }

    private static void Draw(List<Vector3> path, int pathIndex)
    {
        if (path.Count < 2)
            return;

        // Spheres and cones come from separate pools, so the budget applies to waypoints.
        var stride = Mathf.CeilToInt(path.Count / (float)MaxDrawnWaypoints);

        if (stride > 1)
            Plugin.Logger.LogWarning(
                $"[TravelPathDebug] Path {pathIndex} has {path.Count} waypoints, over the " +
                $"{MaxDrawnWaypoints} shape budget — drawing every {stride}th waypoint");

        try
        {
            var drawn = 0;
            var previous = -1;

            for (var i = 0; i < path.Count; i += stride)
            {
                var isOrigin = i == 0;

                // Orange calls out a waypoint the probe cannot place on the surface — the scan
                // will pass through geometry there regardless of what the segments look like.
                var colour = isOrigin
                    ? Color.magenta
                    : IsOffMesh(path[i]) ? Orange : Color.cyan;

                DebugDraw3D.DrawSphere(
                    path[i],
                    isOrigin ? OriginRadius : WaypointRadius,
                    colour,
                    Persist,
                    $"agTravelPath_{pathIndex}_p{drawn}");

                if (previous >= 0)
                {
                    // Tip at the later waypoint so the cone reads as a direction-of-travel arrow.
                    DebugDraw3D.DrawCone(
                        path[i],
                        path[previous],
                        SegmentRadius,
                        SegmentColour(path[previous], path[i]),
                        Persist,
                        $"agTravelPath_{pathIndex}_s{drawn}");
                }

                previous = i;
                drawn++;
            }

            // Movement is Circular, so close the loop back to waypoint 0.
            if (previous > 0)
                DebugDraw3D.DrawCone(
                    path[0],
                    path[previous],
                    SegmentRadius,
                    SegmentColour(path[previous], path[0]),
                    Persist,
                    $"agTravelPath_{pathIndex}_s{drawn}");

            Plugin.Logger.LogDebug(
                $"[TravelPathDebug] Drew path {pathIndex}: {drawn} of {path.Count} waypoints");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"[TravelPathDebug] Draw failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Red   — the straight line between the two waypoints leaves the walkable surface. The scan
    ///         will clip through geometry here. This is the signal that matters: a vertical drop
    ///         between floors registers here and nowhere else.
    /// Orange — an endpoint is off the walkable surface, so sag cannot be judged at all.
    /// Yellow — walkable, but the chord dips further below the surface than this segment's slope
    ///          allows. After sag subdivision runs there should be none of these.
    /// Green  — good.
    /// </summary>
    private static Color SegmentColour(Vector3 from, Vector3 to)
    {
        if (!TravelPathGenerator.IsWalkableSegment(from, to))
            return Color.red;

        // Never let "cannot tell" render as "fine". An off-mesh waypoint used to come back from
        // the probe unchanged, which read as zero sag and painted green — a run of waypoints
        // under the floor looked flawless.
        if (IsOffMesh(from) || IsOffMesh(to))
            return Orange;

        var chordMid = (from + to) * 0.5f;

        // Same reference and tolerance the generator uses, so what is drawn over-tolerance is
        // exactly what subdivision would have tried to fix.
        if (!NavMeshSurfaceProbe.Instance.TrySnap(
                chordMid, Mathf.Max(from.y, to.y), chordMid.y, out var surfaceMid))
            return Orange;

        return surfaceMid.y - chordMid.y > SurfaceGeometry.MaxSagFor(from, to)
            ? Color.yellow
            : Color.green;
    }

    /// <summary>
    /// True when the probe cannot place this waypoint on the walkable surface, or has to move it
    /// to do so — either way the scan will not be where the geometry is.
    /// </summary>
    private static bool IsOffMesh(Vector3 point)
    {
        if (!NavMeshSurfaceProbe.Instance.TrySnap(point, point.y, point.y, out var snapped))
            return true;

        return (snapped - point).sqrMagnitude > OnMeshTolerance * OnMeshTolerance;
    }
}
#endif
