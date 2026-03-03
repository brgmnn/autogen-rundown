using System.Collections.Generic;
using AIGraph;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches;

/// <summary>
/// Replaces the vanilla LG_NodeTools.TryGetPositionsOnRadiusDistancedFromEachother with an
/// improved version that better enforces distance between puzzle scan components.
///
/// The vanilla method uses a soft +10 score penalty which is easily overwhelmed by the
/// distance-from-source score, a 30-node candidate cap, and Euclidean distance (ignoring
/// walls/floors). This causes scan components to frequently cluster too close together.
///
/// Five independently toggleable improvements:
///   1. Hard distance filter — discard candidates closer than the required distance
///   2. Iterative relaxation — reduce required distance and retry when placement fails
///   3. Larger candidate pool — evaluate more nodes for better spread
///   4. Combined scoring — unified score combining source distance + separation
///   5. Graph distance — BFS hop count through AI graph instead of Euclidean distance
/// </summary>
[HarmonyPatch]
public static class Patch_LG_NodeTools
{
    // Toggle each improvement independently
    private const bool UseHardDistanceFilter = true;       // #1
    private const bool UseIterativeRelaxation = true;      // #2
    private const int  CandidatePoolSize = 100;            // #3 (vanilla: 30)
    private const bool UseCombinedScoring = true;           // #4
    private const bool UseGraphDistance = true;             // #5

    // Tuning
    private const float RelaxationFactor = 0.75f;
    private const int   MaxRelaxationPasses = 3;
    private const float SourceDistanceWeight = 1.0f;
    private const float SeparationWeight = 1.0f;
    private const float OcclusionScale = 20f;
    private const float VanillaPenalty = 10f;

    // Graph distance settings
    private const int   MaxBfsHops = 100;
    private const float EstimatedNodeSpacing = 2.5f;

    private struct ScoredNode
    {
        public AIG_INode node;
        public float score;
    }

    [HarmonyPatch(typeof(LG_NodeTools), nameof(LG_NodeTools.TryGetPositionsOnRadiusDistancedFromEachother))]
    [HarmonyPostfix]
    public static void Post_TryGetPositionsOnRadiusDistancedFromEachother(
        LG_Area area,
        Vector3 sourcePos,
        int wantedCount,
        float atRadiusFromSourcePos,
        float distanceFromEachother,
        bool useRandomPosition,
        Il2CppSystem.Collections.Generic.List<Vector3> positions,
        ref bool __result,
        int fixedSeed = 0,
        int maxEval = 30)
    {
        // If the original method failed, nothing to do
        if (!__result || positions == null)
            return;

        // Call game API with IL2CPP types
        Il2CppSystem.Collections.Generic.List<LG_Scoring.Score<AIG_INode>> scoreNodes;
        if (!LG_NodeTools.TryGetScoreNodes(area, true, out scoreNodes))
            return;

        positions.Clear();

        Plugin.Logger.LogDebug(
            $"[NodeTools] Flags: HardFilter={UseHardDistanceFilter} " +
            $"Relaxation={UseIterativeRelaxation} Pool={CandidatePoolSize} " +
            $"CombinedScore={UseCombinedScoring} GraphDist={UseGraphDistance}");
        Plugin.Logger.LogDebug(
            $"[NodeTools] wantedCount={wantedCount} radius={atRadiusFromSourcePos} " +
            $"minDist={distanceFromEachother} totalNodes={scoreNodes.Count}");

        // Step 1: Shrink candidate pool (#3) via game API
        int poolSize = CandidatePoolSize;
        int randomSeed = fixedSeed == 0
            ? Builder.SessionSeedRandom.Range(0, int.MaxValue)
            : fixedSeed;

        var il2cppCandidates = useRandomPosition
            ? LG_Scoring.ShrinkRandomly<AIG_INode>(scoreNodes, poolSize, randomSeed)
            : LG_Scoring.ShrinkEvenly<AIG_INode>(scoreNodes, poolSize);

        // Step 2: Convert to managed list for our own scoring/placement logic
        var candidates = new List<ScoredNode>(il2cppCandidates.Count);
        for (int i = 0; i < il2cppCandidates.Count; i++)
        {
            var s = il2cppCandidates[i];
            candidates.Add(new ScoredNode { node = s.item, score = 0f });
        }

        Plugin.Logger.LogDebug($"[NodeTools] Candidate pool: {candidates.Count} nodes");

        // Step 3: Score candidates
        if (UseCombinedScoring)
        {
            ScoreOnPreferredDistance_Combined(sourcePos, candidates, atRadiusFromSourcePos);
        }
        else
        {
            ScoreOnPreferredDistance_Combined(sourcePos, candidates, atRadiusFromSourcePos);
        }

        // Step 4: Sort by score (ascending = best)
        candidates.Sort((a, b) => a.score.CompareTo(b.score));

        // Step 5: Placement loop with distance enforcement
        float currentMinDistance = distanceFromEachother;
        var placedPositions = new List<Vector3>();
        var placedNodes = new List<AIG_INode>();
        var graphDistanceCache = new Dictionary<(int, int), float>();

        int placed = PlaceCandidates(
            candidates, placedPositions, placedNodes,
            wantedCount, currentMinDistance, sourcePos,
            graphDistanceCache);

        // Step 6: Iterative relaxation (#2)
        if (UseIterativeRelaxation && placed < wantedCount)
        {
            for (int pass = 0; pass < MaxRelaxationPasses && placed < wantedCount; pass++)
            {
                currentMinDistance *= RelaxationFactor;
                Plugin.Logger.LogDebug(
                    $"[NodeTools] Relaxation pass {pass + 1}: " +
                    $"reduced minDist to {currentMinDistance:F2}, placed={placed}/{wantedCount}");

                var remaining = RebuildCandidates(candidates, placedNodes);
                if (remaining.Count == 0)
                    break;

                placed += PlaceCandidates(
                    remaining, placedPositions, placedNodes,
                    wantedCount - placed, currentMinDistance, sourcePos,
                    graphDistanceCache);
            }
        }

        // Step 7: Fallback — fill remaining with random reachable nodes
        if (placed < wantedCount)
        {
            Plugin.Logger.LogDebug($"[NodeTools] Fallback: filling {wantedCount - placed} remaining positions randomly");
            var reachableNodes = area.m_courseNode.m_nodeCluster.m_reachableNodes;

            if (fixedSeed != 0)
            {
                UnityEngine.Random.InitState(fixedSeed);
                for (; placed < wantedCount; placed++)
                {
                    var node = reachableNodes[
                        UnityEngine.Random.Range(0, int.MaxValue) % reachableNodes.Count];
                    placedPositions.Add(node.Position);
                }
            }
            else
            {
                for (; placed < wantedCount; placed++)
                {
                    var node = reachableNodes[
                        Builder.SessionSeedRandom.Range(0, int.MaxValue) % reachableNodes.Count];
                    placedPositions.Add(node.Position);
                }
            }
        }

        for (int i = 0; i < placedPositions.Count; i++)
            positions.Add(placedPositions[i]);

        Plugin.Logger.LogDebug($"[NodeTools] Final: placed {positions.Count}/{wantedCount} positions");

        __result = true;
    }

    private static int PlaceCandidates(
        List<ScoredNode> candidates,
        List<Vector3> placedPositions,
        List<AIG_INode> placedNodes,
        int wantedCount,
        float minDistance,
        Vector3 sourcePos,
        Dictionary<(int, int), float> graphDistanceCache)
    {
        int placed = 0;

        // Work on a mutable copy
        var working = new List<ScoredNode>(candidates);

        for (int i = 0; i < wantedCount; i++)
        {
            if (working.Count == 0)
                break;

            if (placedPositions.Count == 0)
            {
                // First placement: just pick the best-scored candidate
                var best = working[0];
                working.RemoveAt(0);

                placedPositions.Add(best.node.Position);
                placedNodes.Add(best.node);
                placed++;

                float distToSource = (best.node.Position - sourcePos).magnitude;
                Plugin.Logger.LogDebug(
                    $"[NodeTools]   Component {placedPositions.Count}: distToSource={distToSource:F2}");
                continue;
            }

            // Find best candidate considering distance to placed positions
            AIG_INode bestNode = null;
            float bestScore = float.MaxValue;
            int bestIdx = -1;

            for (int j = 0; j < working.Count; j++)
            {
                var candidate = working[j];
                float minDistToPlaced = GetMinDistanceToPlaced(
                    candidate.node, placedPositions, placedNodes,
                    graphDistanceCache);

                if (UseHardDistanceFilter)
                {
                    if (minDistToPlaced < minDistance)
                        continue;
                }

                float score = candidate.score;

                if (UseCombinedScoring)
                {
                    // #4: Add separation penalty for candidates closer than desired.
                    // Note: when UseHardDistanceFilter is active, all candidates with
                    // minDistToPlaced < minDistance are already filtered above, so
                    // deficit will always be 0 here. The penalty only has effect when
                    // the hard filter is disabled.
                    float deficit = Mathf.Max(0f, minDistance - minDistToPlaced);
                    score += SeparationWeight * deficit;
                }
                else if (!UseHardDistanceFilter)
                {
                    // Vanilla-style: soft penalty for being too close
                    if (minDistToPlaced < minDistance)
                        score += VanillaPenalty;
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    bestNode = candidate.node;
                    bestIdx = j;
                }
            }

            if (bestNode == null)
                break;

            working.RemoveAt(bestIdx);
            placedPositions.Add(bestNode.Position);
            placedNodes.Add(bestNode);
            placed++;

            float dSource = (bestNode.Position - sourcePos).magnitude;
            float dNearest = GetMinDistanceToPlaced(
                bestNode, placedPositions, placedNodes,
                graphDistanceCache, excludeSelf: true);
            Plugin.Logger.LogDebug(
                $"[NodeTools]   Component {placedPositions.Count}: " +
                $"distToSource={dSource:F2} distToNearest={dNearest:F2}");
        }

        return placed;
    }

    private static float GetMinDistanceToPlaced(
        AIG_INode candidate,
        List<Vector3> placedPositions,
        List<AIG_INode> placedNodes,
        Dictionary<(int, int), float> graphDistanceCache,
        bool excludeSelf = false)
    {
        float minDist = float.MaxValue;

        for (int i = 0; i < placedNodes.Count; i++)
        {
            if (excludeSelf && placedNodes[i] == candidate)
                continue;

            float dist;
            if (UseGraphDistance)
            {
                dist = GetGraphDistance(candidate, placedNodes[i], graphDistanceCache);
            }
            else
            {
                dist = (candidate.Position - placedPositions[i]).magnitude;
            }

            if (dist < minDist)
                minDist = dist;
        }

        return minDist;
    }

    private static float GetGraphDistance(
        AIG_INode from,
        AIG_INode to,
        Dictionary<(int, int), float> cache)
    {
        if (from == to)
            return 0f;

        int fromId = from.GetHashCode();
        int toId = to.GetHashCode();
        var key = fromId < toId ? (fromId, toId) : (toId, fromId);

        if (cache.TryGetValue(key, out float cached))
            return cached;

        int hops = GetGraphHopCount(from, to, MaxBfsHops);
        float distance;

        if (hops < 0)
        {
            // BFS failed — fall back to Euclidean
            distance = (from.Position - to.Position).magnitude;
        }
        else
        {
            distance = hops * EstimatedNodeSpacing;
        }

        cache[key] = distance;
        return distance;
    }

    private static int GetGraphHopCount(AIG_INode from, AIG_INode to, int maxHops)
    {
        if (from == to)
            return 0;

        AIG_SearchID.IncrementSearchID();
        ushort searchId = AIG_SearchID.SearchID;

        from.SearchID = searchId;

        var queue = new Queue<(AIG_INode node, int depth)>();
        queue.Enqueue((from, 0));

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();

            if (depth >= maxHops)
                continue;

            for (int i = 0; i < current.Links.Count; i++)
            {
                var neighbor = current.Links[i];

                if (neighbor.SearchID == searchId)
                    continue;

                if (!neighbor.IsTraversable)
                    continue;

                neighbor.SearchID = searchId;

                if (neighbor == to)
                    return depth + 1;

                queue.Enqueue((neighbor, depth + 1));
            }
        }

        return -1; // Not reachable within maxHops
    }

    private static void ScoreOnPreferredDistance_Combined(
        Vector3 sourcePos,
        List<ScoredNode> scoredNodes,
        float preferredDistance)
    {
        for (int i = 0; i < scoredNodes.Count; i++)
        {
            var scored = scoredNodes[i];
            float distToSource = (scored.node.Position - sourcePos).magnitude;
            scored.score = SourceDistanceWeight * Mathf.Abs(distToSource - preferredDistance)
                         + ((float)scored.node.OcclusionScore / 255f) * OcclusionScale;
            scoredNodes[i] = scored;
        }
    }

    private static List<ScoredNode> RebuildCandidates(
        List<ScoredNode> original,
        List<AIG_INode> excludeNodes)
    {
        var excludeSet = new HashSet<AIG_INode>(excludeNodes);
        var result = new List<ScoredNode>();

        for (int i = 0; i < original.Count; i++)
        {
            if (!excludeSet.Contains(original[i].node))
                result.Add(original[i]);
        }

        return result;
    }
}
