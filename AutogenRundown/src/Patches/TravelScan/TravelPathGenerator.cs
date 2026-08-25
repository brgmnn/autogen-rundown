using AIGraph;
using LevelGeneration;
using UnityEngine;
using UnityEngine.AI;

namespace AutogenRundown.Patches.TravelScan;

/// <summary>
/// Generates a looping walking path through a zone using NavMesh pathfinding.
///
/// Approach:
///   1. Pick 3 destination nodes far from source and each other (by NavMesh distance)
///   2. Pathfind 4 legs on the NavMesh: start → dest1 → dest2 → dest3 → start
///   3. Trace the walkable surface between the resulting corners, following the navmesh triangles
///      and emitting a point at every edge crossing
///   4. Thin the trace down to waypoints, keeping every fold
///
/// The invariant this maintains: consecutive waypoints are always joined by a straight line that
/// stays on walkable ground. CP_BasicMovable.DoMoveScanner lerps between them with no smoothing,
/// so any segment that violates it is one the scan visibly travels through geometry.
///
/// Step 3 is what makes that true rather than merely intended. Every point in a trace lies on a
/// triangle and every segment between two of them lies inside one triangle, so the line the scan
/// lerps along is a line across a flat surface — it cannot pass through the floor. Earlier versions
/// sampled points and snapped them to the mesh instead, which cannot express which polygon a point
/// is on and so could not tell a ramp from a ledge; the detour, edge-pull, repair and subdivision
/// passes that used to live here all existed to patch up the resulting damage.
/// </summary>
public static class TravelPathGenerator
{
    // How many candidate nodes to evaluate with NavMesh distance
    private const int CandidatePoolSize = 20;

    /// <summary>
    /// Generates a looping path of waypoints through the source area, spaced at roughly
    /// stepDistance along the walkable surface. Returns an empty list when no usable circuit
    /// exists, which makes the caller fall back to base game behaviour.
    /// </summary>
    public static List<Vector3> GenerateLoop(
        LG_Area sourceArea,
        Vector3 sourcePos,
        float stepDistance = TravelScanRegistry.StepDistance)
    {
        var positions = new List<Vector3>();

        if (!sourceArea.m_courseNode.IsValid)
        {
            Plugin.Logger.LogWarning("[TravelPath] CourseNode not valid");
            return positions;
        }

        var nodeCluster = sourceArea.m_courseNode.m_nodeCluster;
        var clusterId = nodeCluster.ID;

        // Gather candidate nodes: in-cluster, non-edge (>= 4 links)
        var candidates = GatherCandidates(nodeCluster, clusterId);
        if (candidates.Count < 2)
        {
            Plugin.Logger.LogWarning(
                $"[TravelPath] Only {candidates.Count} candidate nodes, need at least 2");
            return positions;
        }

        // Pick 3 far-apart destination nodes
        var (dest1, dest2, dest3) = PickDestinations(candidates, sourcePos, sourceArea);

        Plugin.Logger.LogDebug(
            $"[TravelPath] Destinations: dest1={dest1}, dest2={dest2}, dest3={dest3}");

        // Pathfind the legs: start → dest1 → dest2 → dest3 → start.
        //
        // A leg that cannot be pathed on walkable ground is skipped rather than joined with a
        // straight line — a direct line between two nodes runs through whatever geometry is in
        // the way, and once the scan is lerping along it there is nothing downstream that can
        // recover. Dropping the destination just makes the circuit smaller.
        var rawPath = new List<Vector3>();
        var cursor = sourcePos;

        foreach (var dest in new[] { dest1, dest2, dest3 })
        {
            if (AppendNavMeshLeg(rawPath, cursor, dest))
                cursor = dest;
            else
                Plugin.Logger.LogWarning(
                    $"[TravelPath] No walkable route from {cursor} to {dest}, skipping destination");
        }

        if (!AppendNavMeshLeg(rawPath, cursor, sourcePos))
        {
            Plugin.Logger.LogWarning(
                "[TravelPath] Cannot close the loop back to source, falling back to base game");
            return new List<Vector3>();
        }

        if (rawPath.Count < 2)
        {
            Plugin.Logger.LogWarning("[TravelPath] NavMesh pathing produced no usable path");
            return positions;
        }

        Plugin.Logger.LogDebug(
            $"[TravelPath] Raw NavMesh path: {rawPath.Count} corners");

        var surface = TravelScanRegistry.GetSurface();

        if (surface == null)
        {
            // Nothing to trace against. Unrefined corners cut across folds the way vanilla's own
            // holopath does, but every one of them is on the mesh — which beats guessing heights.
            Plugin.Logger.LogWarning(
                "[TravelPath] No navmesh surface available, falling back to raw pathfinder corners");
            return rawPath;
        }

        // Follow the triangles between corners. Every point this produces sits on the surface, and
        // every segment between two of them lies inside one flat triangle.
        var traced = TraceSurface(rawPath, surface, Reroute);

        if (traced.Count < 2)
        {
            Plugin.Logger.LogWarning(
                "[TravelPath] Surface trace produced nothing usable, falling back to raw corners");
            return rawPath;
        }

        Plugin.Logger.LogDebug(
            $"[TravelPath] Surface trace: {rawPath.Count} corners → {traced.Count} points");

        positions = Decimate(traced, TravelScanRegistry.MaxTraceDeviation, stepDistance);

        Plugin.Logger.LogDebug(
            $"[TravelPath] Decimated to {positions.Count} waypoints " +
            $"({stepDistance}m max spacing, {TravelScanRegistry.MaxTraceDeviation}m max deviation)");

        if (positions.Count > 300)
            Plugin.Logger.LogWarning(
                $"[TravelPath] Path has {positions.Count} waypoints, unusually many — " +
                "check for geometry folding far more finely than the bake should allow");

        LogOffSurfaceSegments(positions, sourcePos, surface);

        if (positions.Count > 0)
        {
            var closingDist = (positions[positions.Count - 1] - sourcePos).magnitude;
            Plugin.Logger.LogDebug(
                $"[TravelPath] Loop closing distance: {closingDist:F2}m");
        }

        return positions;
    }

    /// <summary>
    /// Gathers reachable nodes in the area that are non-edge (>= 4 links).
    /// </summary>
    private static List<AIG_INode> GatherCandidates(
        AIG_NodeCluster nodeCluster, ushort clusterId)
    {
        var nodes = nodeCluster.m_reachableNodes;
        if (nodes.Count < 2)
            nodes = nodeCluster.m_nodes;

        var candidates = new List<AIG_INode>();
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (node.ClusterID == clusterId && node.Links.Count >= 4)
                candidates.Add(node);
        }

        return candidates;
    }

    /// <summary>
    /// Picks 3 destination positions that are far from sourcePos and each other.
    /// Pre-filters by Euclidean distance, then ranks finalists by NavMesh distance.
    /// </summary>
    private static (Vector3 dest1, Vector3 dest2, Vector3 dest3) PickDestinations(
        List<AIG_INode> candidates, Vector3 sourcePos, LG_Area sourceArea)
    {
        // Sort by Euclidean distance from source (descending) and take top pool
        candidates.Sort((a, b) =>
        {
            var da = (a.Position - sourcePos).sqrMagnitude;
            var db = (b.Position - sourcePos).sqrMagnitude;
            return db.CompareTo(da); // descending
        });

        var poolSize = Mathf.Min(CandidatePoolSize, candidates.Count);

        // Pick dest1: highest NavMesh distance from source
        var dest1 = candidates[0].Position;
        var bestDist1 = 0f;

        for (var i = 0; i < poolSize; i++)
        {
            var navDist = GetNavMeshDistance(sourcePos, candidates[i].Position);
            if (navDist > bestDist1)
            {
                bestDist1 = navDist;
                dest1 = candidates[i].Position;
            }
        }

        // Pick dest2: try gate-based placement first, then fall back to triangle spread
        Vector3 dest2;
        if (TryFindGateDestination(sourceArea, sourcePos, dest1, out var gateDest2))
        {
            Plugin.Logger.LogDebug(
                $"[TravelPath] dest1 navDist={bestDist1:F1}m, " +
                $"dest2 placed near gate");
            dest2 = gateDest2;
        }
        else
        {
            // Fallback: pick dest2 by triangle spread
            dest2 = candidates[0].Position;
            var bestDist2 = 0f;

            for (var i = 0; i < poolSize; i++)
            {
                var pos = candidates[i].Position;
                // Skip if too close to dest1
                if ((pos - dest1).sqrMagnitude < 1f)
                    continue;

                var distToSource = GetNavMeshDistance(sourcePos, pos);
                var distToDest1 = GetNavMeshDistance(dest1, pos);
                // Maximize the minimum leg length to form a well-spread triangle
                var score = Mathf.Min(distToSource, distToDest1);

                if (score > bestDist2)
                {
                    bestDist2 = score;
                    dest2 = pos;
                }
            }

            Plugin.Logger.LogDebug(
                $"[TravelPath] dest1 navDist={bestDist1:F1}m, " +
                $"dest2 minLeg={bestDist2:F1}m (triangle-spread fallback)");
        }

        // Pick dest3: far from both source and dest1 (triangle spread)
        var dest3 = candidates[0].Position;
        var bestDist3 = 0f;

        for (var i = 0; i < poolSize; i++)
        {
            var pos = candidates[i].Position;
            // Skip if too close to dest1
            if ((pos - dest1).sqrMagnitude < 1f)
                continue;

            var distToSource = GetNavMeshDistance(sourcePos, pos);
            var distToDest1 = GetNavMeshDistance(dest1, pos);
            var score = Mathf.Min(distToSource, distToDest1);

            if (score > bestDist3)
            {
                bestDist3 = score;
                dest3 = pos;
            }
        }

        Plugin.Logger.LogDebug(
            $"[TravelPath] dest3 minLeg={bestDist3:F1}m (triangle-spread)");

        return (dest1, dest2, dest3);
    }

    /// <summary>
    /// Tries to find a dest2 position near a gate where enemies enter.
    /// Priority 1: the zone's source gate (previous zone entrance) if it's in this area.
    /// Priority 2: any other gate in this area, preferring zone-crossing gates farthest from dest1.
    /// </summary>
    private static bool TryFindGateDestination(
        LG_Area sourceArea, Vector3 sourcePos, Vector3 dest1, out Vector3 gatePos)
    {
        gatePos = default;

        // Priority 1: zone source gate (previous zone entrance) in this area
        var sourceGate = sourceArea.m_zone.m_sourceGate;
        if (sourceGate != null)
        {
            var linksFrom = sourceGate.m_linksFrom;
            var linksTo = sourceGate.m_linksTo;

            if (linksFrom == sourceArea || linksTo == sourceArea)
            {
                var sourceGatePos = sourceGate.GetPosition();

                if (TrySnapToNavMesh(sourceGatePos, sourceGatePos.y, out var snapped))
                {
                    snapped = OffsetFromGate(snapped, sourceArea.Position);

                    var distToSource = (snapped - sourcePos).magnitude;
                    var distToDest1 = (snapped - dest1).magnitude;

                    if (distToSource >= 3f && distToDest1 >= 1f && IsReachable(sourcePos, snapped))
                    {
                        gatePos = snapped;
                        Plugin.Logger.LogDebug(
                            "[TravelPath] dest2 placed near zone entrance gate");
                        return true;
                    }
                }
            }
        }

        // Priority 2: any other gate in this area
        if (sourceArea.m_gates == null || sourceArea.m_gates.Count == 0)
            return false;

        Vector3 bestGatePos = default;
        var bestDist = -1f;
        var bestCrossesZone = false;

        for (var i = 0; i < sourceArea.m_gates.Count; i++)
        {
            var gate = sourceArea.m_gates[i];
            if (gate == null)
                continue;

            var gatePosition = gate.GetPosition();

            if (!TrySnapToNavMesh(gatePosition, gatePosition.y, out var snapped))
                continue;

            snapped = OffsetFromGate(snapped, sourceArea.Position);

            var distToSource = (snapped - sourcePos).magnitude;
            if (distToSource < 3f)
                continue;

            if (!IsReachable(sourcePos, snapped))
                continue;

            var crossesZone = gate.m_linksFrom?.m_zone != gate.m_linksTo?.m_zone;
            var distToDest1 = (snapped - dest1).magnitude;

            // Prefer zone-crossing gates, then pick farthest from dest1
            if (crossesZone && !bestCrossesZone)
            {
                bestCrossesZone = true;
                bestDist = distToDest1;
                bestGatePos = snapped;
            }
            else if (crossesZone == bestCrossesZone && distToDest1 > bestDist)
            {
                bestDist = distToDest1;
                bestGatePos = snapped;
            }
        }

        if (bestDist >= 0f)
        {
            gatePos = bestGatePos;
            Plugin.Logger.LogDebug(
                $"[TravelPath] dest2 placed near area gate (crossesZone={bestCrossesZone})");
            return true;
        }

        Plugin.Logger.LogDebug("[TravelPath] dest2 using triangle-spread fallback");
        return false;
    }

    /// <summary>
    /// Snaps a position onto the walkable surface using a wide search radius, for positions that
    /// may start well off the mesh (gate transforms sit in the door frame, not on the floor).
    ///
    /// The wide radius is why the reference height matters here: without it a stairwell door
    /// resolves just as happily onto the floor below as onto its own.
    /// </summary>
    private static bool TrySnapToNavMesh(Vector3 position, float referenceY, out Vector3 snapped)
    {
        if (NavMesh.SamplePosition(position, out var hit, 3f, TravelScanRegistry.WalkableAreaMask)
            && Mathf.Abs(hit.position.y - referenceY) <= TravelScanRegistry.MaxSurfaceSnapRise)
        {
            snapped = hit.position;
            return true;
        }

        snapped = default;
        return false;
    }

    private static Vector3 OffsetFromGate(Vector3 gateSnapped, Vector3 areaCenter, float distance = 3f)
    {
        var direction = (areaCenter - gateSnapped).normalized;
        var offset = gateSnapped + direction * distance;

        if (TrySnapToNavMesh(offset, gateSnapped.y, out var snapped))
            return snapped;

        return gateSnapped;
    }

    private static bool IsReachable(Vector3 from, Vector3 to)
    {
        var path = new NavMeshPath();
        return NavMesh.CalculatePath(from, to, TravelScanRegistry.WalkableAreaMask, path)
               && path.status == NavMeshPathStatus.PathComplete;
    }

    /// <summary>
    /// Appends NavMesh path corners from 'from' to 'to' onto the path list.
    /// Skips the first corner of subsequent legs to avoid duplicates.
    ///
    /// Returns false when there is no complete walkable route. There is deliberately no
    /// straight-line fallback: a direct line between two nodes runs through whatever geometry
    /// separates them, and every downstream stage would then be resampling a chord that was
    /// never walkable to begin with.
    /// </summary>
    private static bool AppendNavMeshLeg(List<Vector3> path, Vector3 from, Vector3 to)
    {
        var navPath = new NavMeshPath();

        if (!NavMesh.CalculatePath(from, to, TravelScanRegistry.WalkableAreaMask, navPath)
            || navPath.status != NavMeshPathStatus.PathComplete
            || navPath.corners.Length < 2)
            return false;

        var startIdx = path.Count == 0 ? 0 : 1; // skip first corner on subsequent legs
        for (var i = startIdx; i < navPath.corners.Length; i++)
            path.Add(navPath.corners[i]);

        return true;
    }



    /// <summary>
    /// Follows the walkable surface along a path of pathfinder corners, emitting a waypoint
    /// wherever the route crosses a triangle edge.
    ///
    /// This is what replaces sampling points along the straight chords between corners. Unity's
    /// funnel algorithm only emits a corner where the path turns *horizontally*, so those chords
    /// cut through staircase crests and across stairwell voids no matter how accurate the corners
    /// themselves are. Tracing the triangles puts a waypoint at every fold in the floor, because a
    /// fold is by definition where one triangle ends and the next begins.
    ///
    /// <paramref name="reroute"/> supplies an alternative route between two corners the surface
    /// says are not directly connected. Production passes the pathfinder; tests pass null.
    /// </summary>
    internal static List<Vector3> TraceSurface(
        List<Vector3> corners,
        INavSurface surface,
        Func<Vector3, Vector3, List<Vector3>>? reroute = null)
    {
        var traced = new List<Vector3>();

        if (corners.Count == 0)
            return traced;

        if (!surface.TryLocate(corners[0], corners[0].y, out var start))
        {
            Plugin.Logger.LogWarning(
                $"[TravelPath] Path start {corners[0]} is not on the walkable surface");
            return traced;
        }

        traced.Add(start);

        for (var i = 1; i < corners.Count; i++)
        {
            var from = traced[traced.Count - 1];
            var to = corners[i];
            var mark = traced.Count;

            if (surface.TryTrace(from, to, traced))
                continue;

            // A partial trace is not usable — it ends wherever the surface ran out.
            traced.RemoveRange(mark, traced.Count - mark);

            // The funnel and the triangles disagree. CalculatePath already said these two corners
            // were connected, so ask it how, and trace that instead. This is exactly the
            // information the old detour walk asked for and then threw away once its budget ran
            // out, which is how a whole stretch of route became one chord.
            var via = reroute?.Invoke(from, to);

            if (via != null && via.Count > 0 && TryTraceVia(from, via, to, surface, traced))
                continue;

            Plugin.Logger.LogWarning(
                $"[TravelPath] Could not trace the surface from {from} to {to}; " +
                $"the scan will cut straight across. {DescribeRoute(from, to)}");

            traced.Add(to);
        }

        return traced;
    }

    /// <summary>
    /// Traces from → via… → to, all or nothing. A half-traced detour is worse than none: it ends
    /// mid-route and leaves the gap somewhere less obvious.
    /// </summary>
    private static bool TryTraceVia(
        Vector3 from, List<Vector3> via, Vector3 to, INavSurface surface, List<Vector3> output)
    {
        var mark = output.Count;
        var cursor = from;

        foreach (var point in via)
        {
            if (!surface.TryTrace(cursor, point, output))
            {
                output.RemoveRange(mark, output.Count - mark);
                return false;
            }

            cursor = output[output.Count - 1];
        }

        if (surface.TryTrace(cursor, to, output))
            return true;

        output.RemoveRange(mark, output.Count - mark);

        return false;
    }

    /// <summary>
    /// Interior corners of the walkable route between two points, or empty if there is none.
    /// </summary>
    private static List<Vector3> Reroute(Vector3 from, Vector3 to)
    {
        var via = new List<Vector3>();
        var path = new NavMeshPath();

        if (!NavMesh.CalculatePath(from, to, TravelScanRegistry.WalkableAreaMask, path)
            || path.status != NavMeshPathStatus.PathComplete)
            return via;

        var corners = path.corners;

        // Skip corners[0] (== from) and the last (== to); the caller has both already.
        for (var i = 1; i < corners.Length - 1; i++)
            via.Add(corners[i]);

        return via;
    }

    /// <summary>
    /// Thins the traced polyline down to a waypoint list, keeping every point whose removal would
    /// let the path deviate more than <paramref name="tolerance"/> from the surface it traced, and
    /// splitting anything longer than <paramref name="maxSpacing"/>.
    ///
    /// Both halves matter. Douglas-Peucker on its own keeps folds and drops flat runs, which is
    /// what stops a stair nose being thinned away; the spacing pass then stops a long flat run
    /// becoming a single chord the scan crosses in one lerp. Every point it emits came from the
    /// trace, so the output cannot leave the surface — the tolerance bounds deviation from real
    /// geometry rather than from a re-sampled guess at it.
    /// </summary>
    internal static List<Vector3> Decimate(List<Vector3> traced, float tolerance, float maxSpacing)
    {
        var points = RemoveDegenerate(traced);

        if (points.Count <= 2)
            return points;

        var keep = new bool[points.Count];
        keep[0] = true;
        keep[points.Count - 1] = true;

        // Iterative rather than recursive: the trace can be thousands of points on fine geometry,
        // and the recursion depth is O(n) in the worst case.
        var pending = new Stack<(int First, int Last)>();
        pending.Push((0, points.Count - 1));

        while (pending.Count > 0)
        {
            var (first, last) = pending.Pop();

            if (last <= first + 1)
                continue;

            var worst = 0f;
            var worstIndex = -1;

            for (var i = first + 1; i < last; i++)
            {
                var deviation = DistanceToSegment(points[i], points[first], points[last]);

                if (deviation <= worst)
                    continue;

                worst = deviation;
                worstIndex = i;
            }

            if (worstIndex < 0 || worst <= tolerance)
                continue;

            keep[worstIndex] = true;
            pending.Push((first, worstIndex));
            pending.Push((worstIndex, last));
        }

        var result = new List<Vector3> { points[0] };
        var previous = 0;

        for (var i = 1; i < points.Count; i++)
        {
            if (!keep[i])
                continue;

            AppendSpaced(result, points, previous, i, maxSpacing);
            previous = i;
        }

        return result;
    }

    /// <summary>
    /// Drops points that sit on top of their predecessor. DoMoveScanner divides by segment length
    /// and Patch_SustainedTravelReverse bails out below 0.001m, so a degenerate segment either
    /// teleports the scan or stalls reverse movement permanently.
    ///
    /// Done up front so everything after it can add points unconditionally and the spacing bound
    /// comes out exact.
    /// </summary>
    private static List<Vector3> RemoveDegenerate(List<Vector3> points)
    {
        var result = new List<Vector3>(points.Count);
        var minimum = TravelScanRegistry.MinSegmentLength * TravelScanRegistry.MinSegmentLength;

        foreach (var point in points)
        {
            if (result.Count > 0 && (point - result[result.Count - 1]).sqrMagnitude < minimum)
                continue;

            result.Add(point);
        }

        return result;
    }

    /// <summary>
    /// Appends points[to], first breaking the run from points[from] up if it would exceed
    /// maxSpacing. Splits are taken at even arclength along the trace, so inserted points follow
    /// the floor rather than the chord being broken up.
    /// </summary>
    private static void AppendSpaced(
        List<Vector3> result, List<Vector3> points, int from, int to, float maxSpacing)
    {
        if ((points[to] - points[from]).magnitude <= maxSpacing)
        {
            result.Add(points[to]);
            return;
        }

        var lengths = new float[to - from + 1];

        for (var i = from + 1; i <= to; i++)
            lengths[i - from] = lengths[i - from - 1] + (points[i] - points[i - 1]).magnitude;

        var total = lengths[to - from];

        // Split on arclength rather than on the chord. Straight-line distance is never more than
        // distance along the polyline, so dividing the arclength evenly bounds the spacing whatever
        // shape the run is.
        var splits = Mathf.CeilToInt(total / maxSpacing) - 1;

        for (var split = 1; split <= splits; split++)
            result.Add(PointAlong(points, from, to, lengths, total * split / (splits + 1f)));

        result.Add(points[to]);
    }

    /// <summary>
    /// The point a given distance along a stretch of the trace.
    ///
    /// Interpolating between two consecutive traced points is exact rather than approximate: the
    /// trace emits a point at every triangle edge it crosses, so the segment between two of them
    /// lies inside one triangle and the surface across it is flat.
    /// </summary>
    private static Vector3 PointAlong(
        List<Vector3> points, int from, int to, float[] lengths, float target)
    {
        var index = from;

        while (index < to - 1 && lengths[index - from + 1] < target)
            index++;

        var span = lengths[index - from + 1] - lengths[index - from];

        if (span < 1e-6f)
            return points[index];

        return Vector3.Lerp(
            points[index], points[index + 1], Mathf.Clamp01((target - lengths[index - from]) / span));
    }

    private static float DistanceToSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        var lengthSqr = ab.sqrMagnitude;

        if (lengthSqr < 1e-8f)
            return (point - a).magnitude;

        var t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / lengthSqr);

        return (point - (a + ab * t)).magnitude;
    }

    /// <summary>
    /// Checks the finished waypoint list against the surface it was traced from, so a failure
    /// shows up in the log rather than only in-game. The debug overlay draws the same failures red.
    ///
    /// Segment midpoints are sampled as well as the waypoints themselves: both ends of a chord can
    /// sit perfectly on the floor with the chord between them passing underneath it, and that is
    /// precisely what dragged the scan through staircases.
    /// </summary>
    private static void LogOffSurfaceSegments(
        List<Vector3> points, Vector3 closingPoint, INavSurface surface)
    {
        var failures = 0;

        for (var i = 0; i < points.Count; i++)
        {
            if (surface.TryLocate(points[i], points[i].y, out var onSurface)
                && Mathf.Abs(onSurface.y - points[i].y) <= TravelScanRegistry.MaxTraceDeviation)
                continue;

            failures++;
            Plugin.Logger.LogWarning(
                $"[TravelPath] Waypoint {i} is off the walkable surface: {points[i]}");
        }

        for (var i = 1; i < points.Count; i++)
            failures += ReportSag(i - 1, i, points[i - 1], points[i], surface);

        // Movement is Circular, so the last waypoint joins back to the scan's start position.
        if (points.Count > 0)
            failures += ReportSag(
                points.Count - 1, 0, points[points.Count - 1], closingPoint, surface);

        if (failures == 0)
            Plugin.Logger.LogDebug(
                "[TravelPath] Every waypoint and segment lies on the walkable surface");
    }

    /// <summary>
    /// Reports a segment whose chord passes below the floor, and by how much.
    /// </summary>
    private static int ReportSag(int from, int to, Vector3 a, Vector3 b, INavSurface surface)
    {
        const int samples = 4;

        var worst = 0f;

        for (var sample = 1; sample < samples; sample++)
        {
            var chord = Vector3.Lerp(a, b, sample / (float)samples);

            if (!surface.TryLocate(chord, chord.y, out var onSurface))
                continue;

            worst = Mathf.Max(worst, onSurface.y - chord.y);
        }

        if (worst <= TravelScanRegistry.MaxTraceDeviation)
            return 0;

        Plugin.Logger.LogWarning(
            $"[TravelPath] Segment {from}→{to} passes {worst:F2}m below the surface: " +
            $"{a} → {b} {DescribeRoute(a, b)}");

        return 1;
    }

    /// <summary>
    /// What the pathfinder makes of a segment the raycast rejected. These are different
    /// algorithms and they can disagree, so which one is wrong decides what to do about it:
    ///
    ///   complete, 2 corners  — the pathfinder says a straight walk works. The raycast is the one
    ///                          in error, most likely at a 64m bake-tile seam, and the segment is
    ///                          probably fine as it stands.
    ///   complete, >2 corners — a route around exists, so a failed repair is our bug, not the
    ///                          geometry's.
    ///   partial / invalid    — genuinely severed under the walkable-only mask. Nothing can cross.
    /// </summary>
    private static string DescribeRoute(Vector3 a, Vector3 b)
    {
        var path = new NavMeshPath();

        if (!NavMesh.CalculatePath(a, b, TravelScanRegistry.WalkableAreaMask, path))
            return "CalculatePath refused the request";

        return $"CalculatePath {path.status} with {path.corners.Length} corner(s)";
    }

    private static float GetNavMeshDistance(Vector3 from, Vector3 to)
    {
        var path = new NavMeshPath();
        if (NavMesh.CalculatePath(from, to, TravelScanRegistry.WalkableAreaMask, path)
            && path.status == NavMeshPathStatus.PathComplete
            && path.corners.Length > 1)
        {
            var dist = 0f;
            var corners = path.corners;
            for (var i = 1; i < corners.Length; i++)
                dist += (corners[i] - corners[i - 1]).magnitude;
            return dist;
        }

        return (from - to).magnitude;
    }
}
