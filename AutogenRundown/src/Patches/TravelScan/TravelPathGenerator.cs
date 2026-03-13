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
///   1. Pick 2 destination nodes far from source and each other (by NavMesh distance)
///   2. Pathfind 3 legs on the NavMesh: start → dest1 → dest2 → start
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

        // Pick 2 far-apart destination nodes
        var (dest1, dest2) = PickDestinations(candidates, sourcePos);

        Plugin.Logger.LogDebug(
            $"[TravelPath] Destinations: dest1={dest1}, dest2={dest2}");

        // Pathfind 3 legs: start → dest1 → dest2 → start
        var rawPath = new List<Vector3>();
        AppendNavMeshLeg(rawPath, sourcePos, dest1);
        AppendNavMeshLeg(rawPath, dest1, dest2);
        AppendNavMeshLeg(rawPath, dest2, sourcePos);

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
    /// Picks 2 destination positions that are far from sourcePos and each other.
    /// Pre-filters by Euclidean distance, then ranks finalists by NavMesh distance.
    /// </summary>
    private static (Vector3 dest1, Vector3 dest2) PickDestinations(
        List<AIG_INode> candidates, Vector3 sourcePos)
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

        // Pick dest2: highest min(navDist to source, navDist to dest1)
        var dest2 = candidates[0].Position;
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
            $"dest2 minLeg={bestDist2:F1}m");

        return (dest1, dest2);
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
        resampled.Add(PullAwayFromEdge(prev, TravelScanRegistry.EdgeDistance));

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
                resampled.Add(PullAwayFromEdge(point, TravelScanRegistry.EdgeDistance));
            }

            remaining = segLen - walked;
            prev = next;
        }

        // Don't add the final corner — it's sourcePos (the scan's start position)
        // which is already position[0] in the caller. This keeps the loop tight:
        // the last resampled point will be within stepDistance of sourcePos.

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

    private static Vector3 PullAwayFromEdge(Vector3 position, float minDistance)
    {
        if (!NavMesh.FindClosestEdge(position, out var hit, -1))
            return position;

        if (hit.distance >= minDistance)
            return position;

        var pullAmount = minDistance - hit.distance;
        var newPos = position + hit.normal * pullAmount;

        if (NavMesh.SamplePosition(newPos, out var sample, 0.5f, -1))
            return sample.position;

        return position;
    }
}
