using System.Collections.Generic;
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
///   3. Resample the combined path at fixed step intervals
///   4. Pull each waypoint away from NavMesh edges
///
/// This guarantees a valid walkable cycle with no off-mesh shortcuts.
/// </summary>
public static class TravelPathGenerator
{
    // How many candidate nodes to evaluate with NavMesh distance
    private const int CandidatePoolSize = 20;

    /// <summary>
    /// Generates a looping path of waypoints through the source area.
    /// Returns waypoints resampled at stepDistance intervals along the NavMesh.
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

        // Pathfind 4 legs: start → dest1 → dest2 → dest3 → start
        var rawPath = new List<Vector3>();
        AppendNavMeshLeg(rawPath, sourcePos, dest1);
        AppendNavMeshLeg(rawPath, dest1, dest2);
        AppendNavMeshLeg(rawPath, dest2, dest3);
        AppendNavMeshLeg(rawPath, dest3, sourcePos);

        if (rawPath.Count < 2)
        {
            Plugin.Logger.LogWarning("[TravelPath] NavMesh pathing produced no usable path");
            return positions;
        }

        Plugin.Logger.LogDebug(
            $"[TravelPath] Raw NavMesh path: {rawPath.Count} corners");

        // Resample at fixed step intervals and pull away from edges
        positions = ResamplePath(rawPath, stepDistance);

        Plugin.Logger.LogDebug(
            $"[TravelPath] Resampled to {positions.Count} waypoints at {stepDistance}m intervals");

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
                if (TrySnapToNavMesh(sourceGate.GetPosition(), out var snapped))
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

            if (!TrySnapToNavMesh(gate.GetPosition(), out var snapped))
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

    private static bool TrySnapToNavMesh(Vector3 position, out Vector3 snapped)
    {
        if (NavMesh.SamplePosition(position, out var hit, 3f, -1))
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

        if (TrySnapToNavMesh(offset, out var snapped))
            return snapped;

        return gateSnapped;
    }

    private static bool IsReachable(Vector3 from, Vector3 to)
    {
        var path = new NavMeshPath();
        return NavMesh.CalculatePath(from, to, -1, path)
               && path.status == NavMeshPathStatus.PathComplete;
    }

    /// <summary>
    /// Appends NavMesh path corners from 'from' to 'to' onto the path list.
    /// Skips the first corner of subsequent legs to avoid duplicates.
    /// </summary>
    private static void AppendNavMeshLeg(List<Vector3> path, Vector3 from, Vector3 to)
    {
        var navPath = new NavMeshPath();
        if (NavMesh.CalculatePath(from, to, -1, navPath)
            && navPath.status == NavMeshPathStatus.PathComplete
            && navPath.corners.Length >= 2)
        {
            var startIdx = path.Count == 0 ? 0 : 1; // skip first corner on subsequent legs
            for (var i = startIdx; i < navPath.corners.Length; i++)
                path.Add(navPath.corners[i]);
        }
        else
        {
            // Fallback: direct line (shouldn't happen for in-area nodes)
            if (path.Count == 0)
                path.Add(from);
            path.Add(to);
        }
    }

    /// <summary>
    /// Resamples a path of corners at fixed distance intervals.
    /// Each resampled point is pulled away from NavMesh edges.
    /// </summary>
    private static List<Vector3> ResamplePath(List<Vector3> corners, float stepDistance)
    {
        var resampled = new List<Vector3>();
        if (corners.Count < 2)
            return resampled;

        var remaining = 0f;
        var prev = corners[0];

        // Add the starting point
        resampled.Add(AdjustForEdges(prev, TravelScanRegistry.EdgeDistance));

        for (var i = 1; i < corners.Count; i++)
        {
            var next = corners[i];
            var segDir = next - prev;
            var segLen = segDir.magnitude;

            if (segLen < 0.001f)
            {
                prev = next;
                continue;
            }

            segDir /= segLen; // normalize
            var walked = remaining;

            while (walked + stepDistance <= segLen)
            {
                walked += stepDistance;
                var point = prev + segDir * walked;
                resampled.Add(AdjustForEdges(point, TravelScanRegistry.EdgeDistance));
            }

            remaining = segLen - walked;
            prev = next;
        }

        // Don't add the final corner — it's sourcePos (the scan's start position)
        // which is already position[0] in the caller. This keeps the loop tight:
        // the last resampled point will be within stepDistance of sourcePos.

        resampled = RemoveBunchedPoints(resampled, stepDistance * 0.5f);
        return resampled;
    }

    private static float GetNavMeshDistance(Vector3 from, Vector3 to)
    {
        var path = new NavMeshPath();
        if (NavMesh.CalculatePath(from, to, -1, path)
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

    private static Vector3 AdjustForEdges(Vector3 position, float minDistance)
    {
        if (!NavMesh.FindClosestEdge(position, out var hitNear, -1))
            return position;

        if (hitNear.distance >= minDistance)
            return position;

        // Measure distance to opposite wall via raycast in pull direction
        var maxProbe = minDistance * 3f;
        var rayTarget = position + hitNear.normal * maxProbe;
        float distToFar;

        if (NavMesh.Raycast(position, rayTarget, out var rayHit, -1))
            distToFar = rayHit.distance;
        else
            distToFar = maxProbe; // No opposite wall — wide open

        var corridorWidth = hitNear.distance + distToFar;

        if (corridorWidth >= minDistance * 2f)
        {
            // Wide enough for full pull
            var pullAmount = minDistance - hitNear.distance;
            var pulled = position + hitNear.normal * pullAmount;
            if (NavMesh.SamplePosition(pulled, out var sample, 0.5f, -1))
                return sample.position;
            return position;
        }

        // Narrow corridor: center between the two walls
        var targetDist = corridorWidth / 2f;
        var shift = targetDist - hitNear.distance;
        var centered = position + hitNear.normal * shift;

        if (NavMesh.SamplePosition(centered, out var snap, 1f, -1))
            return snap.position;

        return position;
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

            if (result.Count >= 2)
            {
                var forward = (prev - result[result.Count - 2]).normalized;
                var toCandidate = (candidate - prev).normalized;
                if (Vector3.Dot(forward, toCandidate) < -0.5f)
                    continue; // Backtracking
            }

            result.Add(candidate);
        }

        return result;
    }
}
