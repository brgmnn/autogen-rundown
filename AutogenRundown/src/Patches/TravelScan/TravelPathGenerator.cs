using System.Collections.Generic;
using AIGraph;
using LevelGeneration;
using UnityEngine;
using UnityEngine.AI;

namespace AutogenRundown.Patches.TravelScan;

/// <summary>
/// Generates a looping walking path through a zone's AI graph nodes.
///
/// Reimplements the directional walk from LG_NodeTools.TryGetPlayerReachablePositionsWithProgressiveDistance
/// but returns the positions and closes the loop for circular movement.
/// </summary>
public static class TravelPathGenerator
{
    // Priority queues for BFS traversal, ranked by directional alignment
    private static readonly Queue<AIG_INode> SearchA = new();
    private static readonly Queue<AIG_INode> SearchB = new();
    private static readonly Queue<AIG_INode> SearchC = new();
    private static readonly Queue<AIG_INode> SearchD = new();

    // Max additional waypoints for the closing leg to prevent runaway
    private const int MaxClosingWaypoints = 40;

    /// <summary>
    /// Generates a looping path of waypoints through the source area.
    /// Phase 1: Outward exploration with direction variation.
    /// Phase 2: Dynamic closing — generates additional waypoints biased toward start
    ///          until the last waypoint is within stepDistance of sourcePos.
    /// </summary>
    public static List<Vector3> GenerateLoop(
        LG_Area sourceArea,
        Vector3 sourcePos,
        int waypointCount = TravelScanRegistry.WaypointCount,
        float stepDistance = TravelScanRegistry.StepDistance,
        float directionVariation = TravelScanRegistry.DirectionVariation)
    {
        var positions = new List<Vector3>();

        if (!sourceArea.m_courseNode.IsValid)
        {
            Plugin.Logger.LogWarning("[TravelPath] CourseNode not valid");
            return positions;
        }

        var nodeCluster = sourceArea.m_courseNode.m_nodeCluster;
        AIG_INode startNode;
        if (!nodeCluster.TryGetClosestNodeInCluster(sourcePos, out startNode))
        {
            Plugin.Logger.LogWarning("[TravelPath] No node found in cluster");
            return positions;
        }

        var clusterId = nodeCluster.ID;
        var stepDistSqr = stepDistance * stepDistance;

        // Pick initial direction using seeded random
        var rng = Builder.SessionSeedRandom;
        var sourceDir = new Vector3(
            rng.Range(-1f, 1f), 0f, rng.Range(-1f, 1f)).normalized;

        if (sourceDir.sqrMagnitude < 0.01f)
            sourceDir = Vector3.forward;

        var currentPos = sourcePos;
        var currentNode = startNode;

        // Phase 1: Outward exploration
        for (int wp = 0; wp < waypointCount; wp++)
        {
            var bestNode = FindBestNode(
                currentNode, currentPos, sourcePos, sourceDir,
                stepDistance, stepDistSqr, clusterId,
                closingLeg: false);

            if (bestNode == null)
            {
                Plugin.Logger.LogDebug(
                    $"[TravelPath] Could not find waypoint {wp + 1}/{waypointCount}, stopping outward phase");
                break;
            }

            var waypointPos = PullAwayFromEdge(
                bestNode.Position, TravelScanRegistry.EdgeDistance);
            positions.Add(waypointPos);

            var dir = (waypointPos - currentPos);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                sourceDir = dir.normalized;

            currentPos = waypointPos;
            currentNode = bestNode;

            VaryDirection(ref sourceDir, directionVariation);
        }

        Plugin.Logger.LogDebug(
            $"[TravelPath] Outward phase: {positions.Count} waypoints");

        // Phase 2: Close the loop — keep generating waypoints toward start
        // until within stepDistance of sourcePos
        float closingDist = (currentPos - sourcePos).magnitude;
        int closingGenerated = 0;

        while (closingDist > stepDistance && closingGenerated < MaxClosingWaypoints)
        {
            // Bias direction strongly toward start
            var toStart = (sourcePos - currentPos);
            toStart.y = 0f;
            if (toStart.sqrMagnitude > 0.01f)
                sourceDir = Vector3.Slerp(sourceDir, toStart.normalized, 0.7f).normalized;

            var bestNode = FindBestNode(
                currentNode, currentPos, sourcePos, sourceDir,
                stepDistance, stepDistSqr, clusterId,
                closingLeg: true);

            if (bestNode == null)
            {
                Plugin.Logger.LogDebug(
                    $"[TravelPath] Closing leg stalled at {closingDist:F2}m from start");
                break;
            }

            var waypointPos = PullAwayFromEdge(
                bestNode.Position, TravelScanRegistry.EdgeDistance);
            positions.Add(waypointPos);

            var dir = (waypointPos - currentPos);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                sourceDir = dir.normalized;

            currentPos = waypointPos;
            currentNode = bestNode;
            closingGenerated++;

            closingDist = (currentPos - sourcePos).magnitude;
        }

        Plugin.Logger.LogDebug(
            $"[TravelPath] Generated {positions.Count} total waypoints " +
            $"({closingGenerated} closing), loop distance: {closingDist:F2}m");

        return positions;
    }

    private static AIG_INode? FindBestNode(
        AIG_INode fromNode,
        Vector3 currentPos,
        Vector3 sourcePos,
        Vector3 sourceDir,
        float stepDistance,
        float stepDistSqr,
        ushort clusterId,
        bool closingLeg)
    {
        SearchA.Clear();
        SearchB.Clear();
        SearchC.Clear();
        SearchD.Clear();

        AIG_SearchID.IncrementSearchID();
        var searchId = AIG_SearchID.SearchID;

        fromNode.SearchID = searchId;
        SearchA.Enqueue(fromNode);

        AIG_INode? bestNode = null;
        float bestScore = -1f;

        int maxEval = 200;
        int highScoreCount = 25;

        while (maxEval > 0 && highScoreCount > 0)
        {
            AIG_INode current;
            if (SearchA.Count > 0)
                current = SearchA.Dequeue();
            else if (SearchB.Count > 0)
                current = SearchB.Dequeue();
            else if (SearchC.Count > 0)
                current = SearchC.Dequeue();
            else if (SearchD.Count > 0)
                current = SearchD.Dequeue();
            else
                break;

            for (int i = 0; i < current.Links.Count; i++)
            {
                var link = current.Links[i];
                if (link.SearchID == searchId)
                    continue;

                link.SearchID = searchId;

                // Stay within the source area's cluster and avoid edge nodes
                if (link.ClusterID != clusterId || link.Links.Count < 4)
                    continue;

                maxEval--;

                var position = link.Position;
                var toNode = position - currentPos;
                toNode.y *= 0.1f;

                // Radius deviation score: how close is the distance to our target step distance
                float radiusScore = 1f - Mathf.Clamp01(
                    Mathf.Abs(toNode.magnitude - stepDistance) / 16f);

                toNode.Normalize();

                // Directional alignment score
                float dot = Mathf.Max(0f, Vector3.Dot(toNode, sourceDir));
                float dotSqr = dot * dot;

                // Outside-radius bonus
                float outsideBonus = (position - currentPos).sqrMagnitude > stepDistSqr
                    ? 1f : 0f;

                float score = outsideBonus + dotSqr + radiusScore;

                // On closing leg, add proximity-to-start bonus
                if (closingLeg)
                {
                    float distToStart = (position - sourcePos).magnitude;
                    float closingScore = 1f - Mathf.Clamp01(distToStart / (stepDistance * 3f));
                    score += closingScore * 0.5f;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestNode = link;
                    if (score > 2.8f)
                        break;
                }

                if (score > 2.5f)
                    highScoreCount--;

                // Enqueue into priority queue based on directional alignment
                if (dotSqr < 0.01f)
                    SearchD.Enqueue(link);
                else if (dotSqr < 0.5f)
                    SearchC.Enqueue(link);
                else if (dotSqr < 0.8f)
                    SearchB.Enqueue(link);
                else
                    SearchA.Enqueue(link);
            }
        }

        return bestNode;
    }

    private static Vector3 PullAwayFromEdge(Vector3 position, float minDistance)
    {
        if (!NavMesh.FindClosestEdge(position, out var hit, -1))
            return position;

        if (hit.distance >= minDistance)
            return position;

        // Move away from edge (hit.normal points away from edge)
        var pullAmount = minDistance - hit.distance;
        var newPos = position + hit.normal * pullAmount;

        // Verify new position is still on NavMesh
        if (NavMesh.SamplePosition(newPos, out var sample, 0.5f, -1))
            return sample.position;

        return position;
    }

    private static void VaryDirection(ref Vector3 dir, float amount)
    {
        if (amount < 0.01f)
            return;

        var rng = Builder.SessionSeedRandom;
        var randomDir = new Vector3(
            rng.Range(-1f, 1f), 0f, rng.Range(-1f, 1f)) * 2f;
        dir = Vector3.Slerp(dir, randomDir, amount).normalized;
    }
}
