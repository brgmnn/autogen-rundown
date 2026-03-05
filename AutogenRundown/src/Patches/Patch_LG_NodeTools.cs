using System.Collections.Generic;
using System.Reflection;
using AIGraph;
using BepInEx.Unity.IL2CPP.Hook;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime.Runtime;
using LevelGeneration;
using UnityEngine;
using UnityEngine.AI;

namespace AutogenRundown.Patches;

/// <summary>
/// Replaces the vanilla LG_NodeTools.TryGetPositionsOnRadiusDistancedFromEachother with an
/// improved version that better enforces distance between puzzle scan components.
///
/// Uses a native detour (INativeDetour) instead of Harmony to bypass a HarmonyX bug where
/// DMD codegen fails on the method's out List&lt;Vector3&gt; parameter, causing
/// InvalidProgramException at runtime.
///
/// Five independently toggleable improvements:
///   1. Hard distance filter — discard candidates closer than the required distance
///   2. Iterative relaxation — reduce required distance and retry when placement fails
///   3. Larger candidate pool — evaluate more nodes for better spread
///   4. Combined scoring — unified score combining source distance + separation
///   5. Graph distance — NavMesh walking distance instead of Euclidean distance
/// </summary>
public static class Patch_LG_NodeTools
{
    // Toggle each improvement independently
    private const bool UseHardDistanceFilter = true;       // #1
    private const bool UseIterativeRelaxation = true;      // #2
    private const int  CandidatePoolSize = 30;            // #3 (vanilla: 30)
    private const bool UseCombinedScoring = true;           // #4
    private const bool UseGraphDistance = true;             // #5

    // Tuning
    private const float RelaxationFactor = 0.75f;
    private const int   MaxRelaxationPasses = 3;
    private const float SourceDistanceWeight = 1.0f;
    private const float SeparationWeight = 1.0f;
    private const float OcclusionScale = 20f;
    private const float VanillaPenalty = 10f;

    // NavMesh path computation (reusable buffers)
    private static readonly NavMeshPath s_navPath = new NavMeshPath();
    private static readonly Vector3[] s_corners = new Vector3[64];

    private struct ScoredNode
    {
        public AIG_INode node;
        public float score;
    }

    // IL2CPP native delegate — matches the platform ABI:
    //   bool → byte, IL2CPP objects → IntPtr, out → pointer
    private unsafe delegate byte d_TryGetPositions(
        IntPtr area,
        Vector3 sourcePos,
        int wantedCount,
        float atRadiusFromSourcePos,
        float distanceFromEachother,
        byte useRandomPosition,
        IntPtr* positions,
        int fixedSeed,
        int maxEval,
        Il2CppMethodInfo* methodInfo);

    private static INativeDetour _detour;
    private static d_TryGetPositions _original;

    public static unsafe void Setup()
    {
        var method = typeof(LG_NodeTools).GetMethod(
            nameof(LG_NodeTools.TryGetPositionsOnRadiusDistancedFromEachother),
            BindingFlags.Public | BindingFlags.Static);

        var ptrField = Il2CppInteropUtils
            .GetIl2CppMethodInfoPointerFieldForGeneratedMethod(method);
        var methodInfoPtr = (IntPtr)ptrField.GetValue(null);
        nint functionPtr = *(nint*)(nint)methodInfoPtr;

        _detour = INativeDetour.CreateAndApply(
            functionPtr, Detour_TryGetPositions, out _original);
    }

    private static unsafe byte Detour_TryGetPositions(
        IntPtr areaPtr, Vector3 sourcePos, int wantedCount,
        float atRadiusFromSourcePos, float distanceFromEachother,
        byte useRandomPosition, IntPtr* positions,
        int fixedSeed, int maxEval, Il2CppMethodInfo* methodInfo)
    {
        var area = new LG_Area(areaPtr);

        // Access nodes directly — bypasses IL2CPP generic types that cause TypeLoadException
        if (area.m_courseNode == null || !area.m_courseNode.IsValid)
        {
            *positions = IntPtr.Zero;
            return 0;
        }

        var nodeCluster = area.m_courseNode.m_nodeCluster;
        var nodes = nodeCluster.m_reachableNodes;
        if (nodes.Count < 2)
            nodes = nodeCluster.m_nodes;

        if (nodes.Count == 0)
        {
            *positions = IntPtr.Zero;
            return 0;
        }

        Plugin.Logger.LogDebug(
            $"[NodeTools] Flags: HardFilter={UseHardDistanceFilter} " +
            $"Relaxation={UseIterativeRelaxation} Pool={CandidatePoolSize} " +
            $"CombinedScore={UseCombinedScoring} GraphDist={UseGraphDistance}");
        Plugin.Logger.LogDebug(
            $"[NodeTools] wantedCount={wantedCount} radius={atRadiusFromSourcePos} " +
            $"minDist={distanceFromEachother} totalNodes={nodes.Count}");

        // Build candidate list directly from nodes (replaces TryGetScoreNodes + ShrinkRandomly/ShrinkEvenly)
        int poolSize = CandidatePoolSize;
        int randomSeed = fixedSeed == 0
            ? Builder.SessionSeedRandom.Range(0, int.MaxValue)
            : fixedSeed;

        var candidates = new List<ScoredNode>();

        if (nodes.Count <= poolSize)
        {
            // All nodes fit in pool
            for (int i = 0; i < nodes.Count; i++)
                candidates.Add(new ScoredNode { node = nodes[i], score = 0f });
        }
        else if (useRandomPosition != 0)
        {
            // ShrinkRandomly equivalent: Fisher-Yates sampling
            var indices = new List<int>(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
                indices.Add(i);

            UnityEngine.Random.InitState(randomSeed);
            for (int i = 0; i < poolSize && indices.Count > 0; i++)
            {
                int pick = UnityEngine.Random.Range(0, indices.Count);
                candidates.Add(new ScoredNode { node = nodes[indices[pick]], score = 0f });
                indices.RemoveAt(pick);
            }
        }
        else
        {
            // ShrinkEvenly equivalent: uniform stride sampling
            float step = (float)nodes.Count / poolSize;
            float f = 0f;
            int idx = 0;
            for (int i = 0; idx < nodes.Count && i < poolSize; i++)
            {
                candidates.Add(new ScoredNode { node = nodes[idx], score = 0f });
                f += step;
                idx = Mathf.RoundToInt(f);
            }
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

        var positionsList = new Il2CppSystem.Collections.Generic.List<Vector3>();
        for (int i = 0; i < placedPositions.Count; i++)
            positionsList.Add(placedPositions[i]);

        *positions = positionsList.Pointer;

        Plugin.Logger.LogDebug($"[NodeTools] Final: placed {positionsList.Count}/{wantedCount} positions");

        return 1;
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
            if (excludeSelf && placedNodes[i].Position == candidate.Position)
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
        if (from.Position == to.Position)
            return 0f;

        int fromId = from.Position.GetHashCode();
        int toId = to.Position.GetHashCode();
        var key = fromId < toId ? (fromId, toId) : (toId, fromId);

        if (cache.TryGetValue(key, out float cached))
            return cached;

        float distance = GetNavMeshDistance(from.Position, to.Position);
        cache[key] = distance;
        return distance;
    }

    private static float GetNavMeshDistance(Vector3 from, Vector3 to)
    {
        if (NavMesh.CalculatePath(from, to, -1, s_navPath)
            && s_navPath.status == NavMeshPathStatus.PathComplete)
        {
            int count = s_navPath.GetCornersNonAlloc(s_corners);
            float dist = 0f;
            for (int i = 1; i < count; i++)
                dist += (s_corners[i] - s_corners[i - 1]).magnitude;
            return dist;
        }
        // Fallback to Euclidean if NavMesh path fails
        return (from - to).magnitude;
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
