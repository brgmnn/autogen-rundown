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
    /// Red    — the chord between the two waypoints passes below the floor by more than the trace
    ///          tolerance. The scan visibly clips through geometry here.
    /// Orange — an endpoint is not on the walkable surface at all, so nothing about the segment
    ///          can be judged.
    /// Green  — good.
    ///
    /// Both failure colours now mean a real bug rather than a tolerance being grazed: tracing puts
    /// every waypoint on a triangle and every segment inside one, so anything else is the surface
    /// and the path having genuinely diverged.
    /// </summary>
    private static Color SegmentColour(Vector3 from, Vector3 to)
    {
        var surface = TravelScanRegistry.GetSurface();

        if (surface == null)
            return Color.cyan;

        // Never let "cannot tell" render as "fine". An off-surface waypoint used to come back from
        // the probe unchanged, which read as zero sag and painted green — a run of waypoints under
        // the floor looked flawless.
        if (IsOffMesh(from) || IsOffMesh(to))
            return Orange;

        // Sample across the chord rather than only at its midpoint: a segment can straddle a fold
        // with both ends and the middle on the floor while the quarter points are underneath it.
        for (var sample = 1; sample < 4; sample++)
        {
            var chord = Vector3.Lerp(from, to, sample / 4f);

            if (!surface.TryLocate(chord, chord.y, out var onSurface))
                return Orange;

            if (onSurface.y - chord.y > TravelScanRegistry.MaxTraceDeviation)
                return Color.red;
        }

        return Color.green;
    }

    /// <summary>
    /// True when the waypoint cannot be placed on the walkable surface, or sits noticeably off it —
    /// either way the scan will not be where the geometry is.
    /// </summary>
    private static bool IsOffMesh(Vector3 point)
    {
        var surface = TravelScanRegistry.GetSurface();

        if (surface == null)
            return false;

        if (!surface.TryLocate(point, point.y, out var located))
            return true;

        return Mathf.Abs(located.y - point.y) > OnMeshTolerance;
    }
}
#endif
