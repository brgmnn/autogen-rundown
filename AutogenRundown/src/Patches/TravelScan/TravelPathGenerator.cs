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
///   3. Walk the surface between the resulting corners, emitting a waypoint every step distance,
///      re-routing around anything that blocks a sub-step
///   4. Pull each waypoint away from NavMesh edges
///   5. Subdivide any segment whose straight chord would sag below the walkable surface
///   6. Re-walk any segment the scan could not lerp along, or that is far longer than a step
///
/// The invariant this maintains: consecutive waypoints are always joined by a straight line that
/// stays on walkable ground. CP_BasicMovable.DoMoveScanner lerps between them with no smoothing,
/// so any segment that violates it is one the scan visibly travels through geometry.
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

        var probe = NavMeshSurfaceProbe.Instance;

        // Walk the surface between corners rather than sampling along the straight chords
        positions = WalkSurface(rawPath, stepDistance, probe);

        Plugin.Logger.LogDebug(
            $"[TravelPath] Surface walk produced {positions.Count} waypoints " +
            $"at {stepDistance}m intervals");

        // Refine until it settles.
        //
        // Repair re-walks segments the scan could not lerp along, or that are far longer than a
        // step — and the waypoints it inserts may themselves sag. Subdivision splits sagging
        // segments — and the waypoints it inserts must still be walkable. Each creates work for
        // the other, so a single pass of each leaves whichever ran first stale.
        //
        // Repair goes first within a pass so subdivision always sees the final waypoint set. Both
        // only ever add points, and both are bounded (MinSegmentLength below, MaxSegmentLength
        // above), so this converges well inside the pass limit.
        var beforeRefinement = positions.Count;
        var passes = 0;

        for (; passes < TravelScanRegistry.MaxRefinementPasses; passes++)
        {
            var before = positions.Count;

            positions = RepairUnwalkableSegments(positions, sourcePos, probe);
            positions = SubdivideSaggingSegments(positions, sourcePos, probe);

            if (positions.Count == before)
                break;
        }

        Plugin.Logger.LogDebug(
            $"[TravelPath] Refinement: {beforeRefinement} → {positions.Count} waypoints " +
            $"in {passes + 1} pass(es)");

        if (positions.Count > 300)
            Plugin.Logger.LogWarning(
                $"[TravelPath] Path has {positions.Count} waypoints, unusually many — " +
                "check for geometry fighting the sag tolerance");

        LogUnwalkableSegments(positions, sourcePos, probe);

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
    /// Walks the walkable surface along a path of NavMesh corners, emitting a waypoint every
    /// stepDistance of travel.
    ///
    /// This replaces sampling points along the straight chords between corners. Unity's funnel
    /// algorithm only emits a corner where the path turns *horizontally*, so those chords cut
    /// through staircase crests and across stairwell voids. Snapping the sampled points
    /// afterwards does not fix it: each point is probed independently, so two neighbours can
    /// resolve onto different floors and the scan drops vertically between them.
    ///
    /// Instead the cursor advances in small SurfaceStepDistance increments, carrying the surface
    /// height forward as the reference for the next probe, and every increment is checked for
    /// walkability. The probe is therefore never more than one sub-step from ground already known
    /// to be good, and the cursor cannot appear on another floor.
    ///
    /// This is the technique the game uses in PlayerBotActionBase.SnapSegmentToNav, which marches
    /// a segment in fifths feeding each hit's Y into the next probe.
    /// </summary>
    internal static List<Vector3> WalkSurface(
        List<Vector3> corners, float stepDistance, ISurfaceProbe probe)
    {
        var emitted = new List<Vector3>();
        if (corners.Count < 2)
            return emitted;

        // Copied so detour corners can be spliced in ahead of the current target
        var route = new List<Vector3>(corners);

        var cursor = probe.Snap(route[0], route[0].y, route[0].y);
        emitted.Add(probe.PullFromEdge(cursor, TravelScanRegistry.EdgeDistance));

        var accumulated = 0f;
        var detours = 0;

        for (var i = 1; i < route.Count; i++)
        {
            var corner = route[i];

            // Sized from the work actually required rather than a flat cap, so a genuine stall
            // shows up as the progress check below and not as a long spin.
            var guard = Mathf.CeilToInt(
                HorizontalDistance(cursor, corner) / TravelScanRegistry.SurfaceStepDistance) * 2 + 16;

            while (true)
            {
                // Direction and remaining distance are measured in the horizontal plane: the
                // surface supplies the height, we only decide where to step in XZ.
                var toCorner = corner - cursor;
                toCorner.y = 0f;

                var remaining = toCorner.magnitude;
                if (remaining < 0.01f)
                    break;

                if (--guard < 0)
                {
                    Plugin.Logger.LogWarning(
                        "[TravelPath] Surface walk exceeded its step budget, skipping to corner");
                    cursor = probe.Snap(corner, corner.y, corner.y);
                    accumulated = 0f;
                    break;
                }

                var advance = Mathf.Min(TravelScanRegistry.SurfaceStepDistance, remaining);
                var target = cursor + toCorner / remaining * advance;

                // The corner's height is the routing hint: it is where the pathfinder says this
                // leg ends up, so it tells the probe which way to look when more than one surface
                // is in range. Without it the walk stays on whatever floor it started on and
                // simply tracks the corner's XZ — it would stroll along the lower floor beneath a
                // mezzanine instead of taking the stairs.
                var next = probe.Snap(target, cursor.y, corner.y);

                // Two ways a sub-step is unusable, and both must be caught here. The straight line
                // may leave the surface — or the probe may have pulled the target back off the
                // line, in which case the cursor makes no headway and the walk would spin. The
                // walkability check cannot see the second case: cursor and next are nearly
                // identical, so it passes trivially.
                var blocked = !probe.IsWalkable(cursor, next)
                              || remaining - HorizontalDistance(next, corner)
                                 < advance * TravelScanRegistry.MinWalkProgressFraction;

                if (blocked)
                {
                    if (detours < TravelScanRegistry.MaxWalkDetours
                        && probe.TryFindRoute(cursor, corner, out var via))
                    {
                        // Walk the detour first, then carry on to this corner. InsertRange pushes
                        // the current corner further down the route, so it is still visited.
                        detours++;
                        route.InsertRange(i, via);
                        corner = route[i];
                        guard = Mathf.CeilToInt(
                            HorizontalDistance(cursor, corner)
                            / TravelScanRegistry.SurfaceStepDistance) * 2 + 16;
                        continue;
                    }

                    // Out of detours, or genuinely unreachable. Skip to the corner so the circuit
                    // still completes — RepairUnwalkableSegments re-walks the seam.
                    cursor = probe.Snap(corner, corner.y, corner.y);
                    accumulated = 0f;
                    break;
                }

                accumulated += (next - cursor).magnitude;
                cursor = next;

                if (accumulated >= stepDistance)
                {
                    emitted.Add(probe.PullFromEdge(cursor, TravelScanRegistry.EdgeDistance));
                    accumulated = 0f;
                }
            }
        }

        if (detours > 0)
            Plugin.Logger.LogDebug($"[TravelPath] Surface walk took {detours} detour(s)");

        // Don't emit the final corner — it's sourcePos (the scan's start position), which the
        // caller re-inserts as position 0. Circular movement wraps back to it.

        // Runs here, before subdivision. Re-running it afterwards at this spacing would delete
        // exactly the crest waypoints subdivision just inserted.
        emitted = RemoveBunchedPoints(emitted, stepDistance * 0.5f);
        return emitted;
    }

    /// <summary>
    /// True when the scan can lerp straight from a to b without leaving walkable ground.
    /// </summary>
    internal static bool IsWalkableSegment(Vector3 a, Vector3 b)
        => NavMeshSurfaceProbe.Instance.IsWalkable(a, b);

    /// <summary>
    /// Distance in the horizontal plane. The walk steps in XZ and lets the surface supply the
    /// height, so progress has to be measured the same way.
    /// </summary>
    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        var delta = b - a;
        return new Vector2(delta.x, delta.z).magnitude;
    }


    /// <summary>
    /// Inserts extra waypoints wherever the straight chord between two waypoints passes below
    /// the walkable surface.
    ///
    /// CP_BasicMovable.DoMoveScanner moves the scan with a plain Vector3.Lerp between
    /// consecutive ScanPositions, so a chord spanning the crest of a staircase drags the scan
    /// sphere through the floor no matter how accurate the endpoints are.
    ///
    /// <paramref name="closingPoint"/> is the scan's source position. It is not part of
    /// <paramref name="points"/> (the caller re-inserts it at index 0), but movement is
    /// Circular so the last → source segment is real and needs subdividing too.
    ///
    /// The surface probe is injected so the recursion can be unit tested without NavMesh.
    /// </summary>
    internal static List<Vector3> SubdivideSaggingSegments(
        List<Vector3> points, Vector3 closingPoint, ISurfaceProbe probe)
    {
        if (points.Count < 1)
            return points;

        var refined = new List<Vector3>(points.Count) { points[0] };

        for (var i = 1; i < points.Count; i++)
            AppendSubdivided(refined, refined[refined.Count - 1], points[i], 0, probe);

        // Subdivide the wrap-around segment, then drop closingPoint itself.
        var closing = new List<Vector3>();
        AppendSubdivided(closing, refined[refined.Count - 1], closingPoint, 0, probe);

        if (closing.Count > 0)
            closing.RemoveAt(closing.Count - 1);

        refined.AddRange(closing);

        return refined;
    }

    private static void AppendSubdivided(
        List<Vector3> output, Vector3 a, Vector3 b, int depth, ISurfaceProbe probe)
    {
        if (depth < TravelScanRegistry.MaxSubdivisionDepth
            && (b - a).magnitude >= TravelScanRegistry.MinSegmentLength * 2f)
        {
            var chordMid = (a + b) * 0.5f;

            // Reference off the *higher* endpoint, not the segment start.
            //
            // Snap rejects a result further than MaxSurfaceSnapRise from its reference, because
            // that normally means a different floor. Anchored to `a`, that guard swallows the
            // crest of a staircase whenever the walk is climbing: the crest sits more than a
            // metre above the low end, the probe result is thrown away as a floor jump, and sag
            // reads as zero. Descending the same stairs it worked, because `a` was then the upper
            // landing — the bug was direction-dependent, and steeper stairs made it worse.
            //
            // Both endpoints are known-good surface points joined by a walkable segment, so the
            // accept band should span their heights. Floor safety is unaffected: another floor is
            // at least agentHeight (2m) away and these two points are metres apart at most.
            var surfaceMid = probe.Snap(chordMid, Mathf.Max(a.y, b.y), chordMid.y);

            // Only sag matters. A chord passing above the surface — the concave break at the
            // foot of a staircase — just floats the scan slightly and is harmless.
            if (surfaceMid.y - chordMid.y > SurfaceGeometry.MaxSagFor(a, b))
            {
                AppendSubdivided(output, a, surfaceMid, depth + 1, probe);
                AppendSubdivided(output, surfaceMid, b, depth + 1, probe);
                return;
            }
        }

        output.Add(b);
    }

    /// <summary>
    /// Re-walks any segment the scan could not lerp along, or that is far longer than a step.
    ///
    /// This is the pass that enforces the invariant: every consecutive pair of waypoints —
    /// including the Circular wrap back to <paramref name="closingPoint"/> — must be joined by a
    /// straight line that stays on walkable ground. Sag subdivision cannot do it, because a
    /// vertical drop between two floors has no sag to measure.
    ///
    /// The game applies the same test when it builds the AI graph: NodeLinksJob only links two
    /// nodes when a navmesh raycast between them comes back clear.
    ///
    /// Over-long segments are repaired for a different reason. A long chord across an open room is
    /// perfectly walkable, so nothing else objects to it — but it means the walk skipped a stretch
    /// of route, and the scan slides straight across bypassing everything it should have covered.
    /// </summary>
    internal static List<Vector3> RepairUnwalkableSegments(
        List<Vector3> points, Vector3 closingPoint, ISurfaceProbe probe)
    {
        if (points.Count < 1)
            return points;

        var repaired = new List<Vector3>(points.Count) { points[0] };

        for (var i = 1; i < points.Count; i++)
            AppendRepaired(repaired, repaired[repaired.Count - 1], points[i], probe);

        // The wrap-around segment is real too. Repair it, then drop closingPoint itself — the
        // caller re-inserts it at index 0.
        var closing = new List<Vector3>();
        AppendRepaired(closing, repaired[repaired.Count - 1], closingPoint, probe);

        if (closing.Count > 0)
            closing.RemoveAt(closing.Count - 1);

        repaired.AddRange(closing);

        return repaired;
    }

    private static void AppendRepaired(
        List<Vector3> output, Vector3 a, Vector3 b, ISurfaceProbe probe)
    {
        var length = (b - a).magnitude;

        // Drop a waypoint that sits on top of its predecessor rather than passing it through.
        // DoMoveScanner divides by segment length and ReverseMovement bails out below 0.001m, so
        // the output invariant has to hold whatever the input looked like.
        if (length < TravelScanRegistry.MinSegmentLength)
            return;

        if (probe.IsWalkable(a, b) && length <= TravelScanRegistry.MaxSegmentLength)
        {
            output.Add(b);
            return;
        }

        foreach (var point in RewalkBetween(a, b, probe))
            output.Add(point);

        output.Add(b);
    }

    /// <summary>
    /// Interior waypoints of a walked route from a to b.
    ///
    /// Splicing raw CalculatePath corners here is not enough: the funnel algorithm emits corners
    /// only where the route turns, so a 40m detour would gain two waypoints and remain a chain of
    /// long chords — the very thing being repaired. Running the corners back through
    /// <see cref="WalkSurface"/> gives the same StepDistance spacing as the rest of the path.
    ///
    /// Both endpoints are snapped first. PullFromEdge moves emitted waypoints after the walk, so
    /// a repair endpoint can sit slightly off-mesh, and CalculatePath from an off-mesh point
    /// fails. Returns empty when no route exists — the segment then stays as it was and is
    /// reported by <see cref="LogUnwalkableSegments"/>.
    /// </summary>
    private static List<Vector3> RewalkBetween(Vector3 a, Vector3 b, ISurfaceProbe probe)
    {
        var from = probe.Snap(a, a.y, a.y);
        var to = probe.Snap(b, b.y, b.y);

        if (!probe.TryFindRoute(from, to, out var via))
            via = new List<Vector3>();

        var route = new List<Vector3>(via.Count + 2) { from };
        route.AddRange(via);
        route.Add(to);

        var walked = WalkSurface(route, TravelScanRegistry.StepDistance, probe);

        // WalkSurface emits its first corner, which duplicates a
        if (walked.Count > 0)
            walked.RemoveAt(0);

        // Drop anything that would leave a degenerate hop at either end — DoMoveScanner divides
        // by segment length and ReverseMovement bails out below 0.001m.
        while (walked.Count > 0
               && (walked[0] - a).magnitude < TravelScanRegistry.MinSegmentLength)
            walked.RemoveAt(0);

        while (walked.Count > 0
               && (b - walked[walked.Count - 1]).magnitude < TravelScanRegistry.MinSegmentLength)
            walked.RemoveAt(walked.Count - 1);

        return walked;
    }

    /// <summary>
    /// Warns about any segment still not walkable after repair, so it can be found in-game.
    /// The debug overlay draws these red.
    /// </summary>
    private static void LogUnwalkableSegments(
        List<Vector3> points, Vector3 closingPoint, ISurfaceProbe probe)
    {
        var failures = 0;

        for (var i = 1; i < points.Count; i++)
        {
            if (probe.IsWalkable(points[i - 1], points[i]))
                continue;

            failures++;
            Plugin.Logger.LogWarning(
                $"[TravelPath] Segment {i - 1}→{i} is still not walkable: " +
                $"{points[i - 1]} → {points[i]} {DescribeObstruction(points[i - 1], points[i])}");
        }

        if (points.Count > 0 && !probe.IsWalkable(points[points.Count - 1], closingPoint))
        {
            failures++;
            Plugin.Logger.LogWarning(
                $"[TravelPath] Closing segment is still not walkable: " +
                $"{points[points.Count - 1]} → {closingPoint} " +
                DescribeObstruction(points[points.Count - 1], closingPoint));
        }

        if (failures == 0)
            Plugin.Logger.LogDebug("[TravelPath] All segments walkable");
    }

    /// <summary>
    /// Narrows down why a segment is blocked, so a log line is enough to tell the cases apart
    /// without another round of guessing.
    ///
    /// Unity reserves area 1 for "Not Walkable", and geometry rasterized into it is *removed* from
    /// the mesh rather than kept as area-1 polygons. So a hole blocks every area mask equally,
    /// while an area exclusion (a Jump or Ladder off-mesh link) only blocks our walkable-only one.
    /// Testing with -1 separates them.
    ///
    /// Holes come from carving NavMeshObstacles — LG_InternalGate/LG_PlugDoorAlign enable one
    /// while the gate is shut, spanning the whole doorway — or from anything with less than
    /// agentHeight (2m) of clearance above it, which strips the mesh beneath a low pipe or
    /// catwalk on otherwise flat floor.
    /// </summary>
    private static string DescribeObstruction(Vector3 a, Vector3 b)
    {
        if (!SurfaceGeometry.IsSlopeWalkable(a, b))
            return "(too steep — a floor change, not an obstruction)";

        // -1 admits every area, including the Jump and Ladder off-mesh links our mask excludes
        var blockedForAnyArea = NavMesh.Raycast(a, b, out _, -1);

        if (!blockedForAnyArea)
            return "(passable on some other area — an off-mesh link, not a hole)";

        var endpointsOnMesh =
            NavMesh.SamplePosition(a, out _, 0.5f, -1) && NavMesh.SamplePosition(b, out _, 0.5f, -1);

        return endpointsOnMesh
            ? "(hole in the mesh between two on-mesh endpoints — carving obstacle or low ceiling)"
            : "(an endpoint is off-mesh)";
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


    private static List<Vector3> RemoveBunchedPoints(List<Vector3> points, float minSpacing)
    {
        if (points.Count < 3)
            return points;

        var minSpacingSqr = minSpacing * minSpacing;
        var result = new List<Vector3>(points.Count) { points[0] };

        for (var i = 1; i < points.Count; i++)
        {
            var prev = result[result.Count - 1];
            var candidate = points[i];

            if ((candidate - prev).sqrMagnitude < minSpacingSqr)
                continue;

            result.Add(candidate);
        }

        return result;
    }
}
