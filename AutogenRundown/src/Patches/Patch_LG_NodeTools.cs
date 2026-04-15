using System;
using System.Collections.Generic;
using System.Reflection;
using AIGraph;
using BepInEx.Unity.IL2CPP.Hook;
using ChainedPuzzles;
using HarmonyLib;
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
    // ── Feature toggles ──────────────────────────────────────────────────
    // #1: Hard distance filter — reject any candidate closer than
    //     HardFilterFraction * minDistance to an already-placed node.
    //     Prevents clumping by culling obviously too-close candidates early.
    private const bool UseHardDistanceFilter = true;

    // #2: Iterative relaxation — when not enough candidates survive filtering,
    //     reduce minDistance by RelaxationFactor and retry up to MaxRelaxationPasses
    //     times. Gracefully handles tight maps where full spacing isn't possible.
    private const bool UseIterativeRelaxation = true;

    // #3: Candidate pool size — number of nodes to pre-score by source distance
    //     before placement begins. Larger = better spread but slower.
    //     Vanilla uses 30; we use 100 for better quality.
    private const int  CandidatePoolSize = 100;

    // #4: Combined scoring — during placement, rank candidates by a weighted sum
    //     of source-distance score and separation score (distance to nearest
    //     already-placed node). When off, falls back to vanilla penalty-based scoring.
    private const bool UseCombinedScoring = true;

    // #5: Graph distance — use NavMesh walking distance instead of straight-line
    //     Euclidean distance for separation checks. More accurate in complex geometry
    //     but falls back to Euclidean if the NavMesh query fails.
    private const bool UseGraphDistance = true;

    // ── Tuning constants ─────────────────────────────────────────────────
    // How much to shrink minDistance each relaxation pass (0.75 = 25% reduction).
    private const float RelaxationFactor = 0.75f;

    // Maximum number of relaxation passes before giving up on spacing.
    private const int   MaxRelaxationPasses = 3;

    // Weight of the source-distance term in the combined placement score.
    // Higher = prefer nodes at the correct distance from source over good separation.
    // Both terms are normalized 0–1 (fractional deviation from target).
    private const float SourceDistanceWeight = 1.0f;

    // Weight of the separation term in the combined placement score.
    // Higher = prefer nodes far from already-placed nodes.
    // At 8× source weight, separation dominates placement decisions.
    private const float SeparationWeight = 8.0f;

    // Hard filter threshold as a fraction of minDistance.
    // Candidates closer than minDistance * 0.5 to any placed node are rejected.
    private const float HardFilterFraction = 0.5f;

    // Weight of the occlusion score (0–255 from the game engine, normalized to 0–1).
    // Bonus for nodes with higher occlusion — favors visually interesting placements.
    private const float OcclusionScale = 3f;

    // Weight of the angular-diversity term in the combined placement score.
    // Only applies when atRadiusFromSourcePos > 0 (Mode B: a puzzle type laying out
    // its own sub-scans around a single landing point, e.g. CP_Cluster_Core).
    // Penalizes candidates whose XZ direction from sourcePos is near an already-placed
    // direction — produces equidistant spread around the circle.
    // Sits between SourceDistanceWeight (1.0) and SeparationWeight (8.0).
    private const float AngularDiversityWeight = 5.0f;

    // Direction vectors with XZ magnitude below this are treated as "at source"
    // and skipped from angular scoring (angle is ill-defined at the origin).
    private const float AngularEpsilon = 0.01f;

    // Flat penalty added in vanilla-style scoring when a candidate is too close
    // to a placed node. Only used when UseCombinedScoring is false.
    private const float VanillaPenalty = 10f;

    // Diagnostics & circuit-breaker
    private static bool _loggedFirstCall;
    private static bool _loggedNavMeshStatus;
    private static bool _navMeshFailed;

    // Set by Patch_CP_Cluster_Core to switch scoring mode for cluster sub-scans
    internal static bool _isClusterPlacement;

    private struct ScoredNode
    {
        public AIG_INode node;
        public float score;
    }

    // IL2CPP native delegate — matches the platform ABI:
    //   bool → byte, IL2CPP objects → IntPtr, out → pointer
    // TODO: use atRadiusFromSourcePos
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
        var functionPtr = *(nint*)(nint)methodInfoPtr;

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
            $"CombinedScore={UseCombinedScoring} GraphDist={UseGraphDistance} " +
            $"ClusterFan={_isClusterPlacement}");
        Plugin.Logger.LogDebug(
            $"[NodeTools] wantedCount={wantedCount} radius={atRadiusFromSourcePos} " +
            $"minDist={distanceFromEachother} totalNodes={nodes.Count}");

        // Build candidate list directly from nodes (replaces TryGetScoreNodes + ShrinkRandomly/ShrinkEvenly)
        var poolSize = CandidatePoolSize;
        var randomSeed = fixedSeed == 0
            ? Builder.SessionSeedRandom.Range(0, int.MaxValue)
            : fixedSeed;

        var candidates = new List<ScoredNode>();

        if (nodes.Count <= poolSize)
        {
            // All nodes fit in pool
            for (var i = 0; i < nodes.Count; i++)
                candidates.Add(new ScoredNode { node = nodes[i], score = 0f });
        }
        else if (useRandomPosition != 0)
        {
            // ShrinkRandomly equivalent: Fisher-Yates sampling
            var indices = new List<int>(nodes.Count);
            for (var i = 0; i < nodes.Count; i++)
                indices.Add(i);

            UnityEngine.Random.InitState(randomSeed);
            for (var i = 0; i < poolSize && indices.Count > 0; i++)
            {
                var pick = UnityEngine.Random.Range(0, indices.Count);
                candidates.Add(new ScoredNode { node = nodes[indices[pick]], score = 0f });
                indices.RemoveAt(pick);
            }
        }
        else
        {
            // ShrinkEvenly equivalent: uniform stride sampling
            var step = (float)nodes.Count / poolSize;
            var f = 0f;
            var idx = 0;
            for (var i = 0; idx < nodes.Count && i < poolSize; i++)
            {
                candidates.Add(new ScoredNode { node = nodes[idx], score = 0f });
                f += step;
                idx = Mathf.RoundToInt(f);
            }
        }

        Plugin.Logger.LogDebug($"[NodeTools] Candidate pool: {candidates.Count} nodes");

        // Step 3: Score candidates
        ScoreOnPreferredDistance_Combined(sourcePos, candidates, atRadiusFromSourcePos);

        // Step 4: Sort by score (ascending = best)
        candidates.Sort((a, b) => a.score.CompareTo(b.score));

        // Step 5: Placement loop with distance enforcement
        var currentMinDistance = distanceFromEachother;
        var placedPositions = new List<Vector3>();
        var placedNodes = new List<AIG_INode>();
        var placed = 0;

        // When radius=0, game places component 0 at sourcePos (the door).
        // Seed the anchor so separation is measured from the actual door position.
        if (atRadiusFromSourcePos <= 0.01f && wantedCount > 0)
        {
            var nearestNode = candidates[0].node;
            var nearestDist = float.MaxValue;

            for (var i = 0; i < candidates.Count; i++)
            {
                var d = (candidates[i].node.Position - sourcePos).sqrMagnitude;

                if (d >= nearestDist)
                    continue;

                nearestDist = d;
                nearestNode = candidates[i].node;
            }

            placedPositions.Add(sourcePos);
            placedNodes.Add(nearestNode);
            placed = 1;

            Plugin.Logger.LogDebug("[NodeTools]   Component 1 (seeded at source): distToSource=0.00");
        }

        placed += PlaceCandidates(
            candidates, placedPositions, placedNodes,
            wantedCount - placed, currentMinDistance, sourcePos, atRadiusFromSourcePos);

        // Step 6: Iterative relaxation (#2)
        if (UseIterativeRelaxation && placed < wantedCount)
        {
            for (var pass = 0; pass < MaxRelaxationPasses && placed < wantedCount; pass++)
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
                    wantedCount - placed, currentMinDistance, sourcePos, atRadiusFromSourcePos);
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
                    var node = reachableNodes[UnityEngine.Random.Range(0, int.MaxValue) % reachableNodes.Count];

                    placedPositions.Add(node.Position);
                }
            }
            else
            {
                for (; placed < wantedCount; placed++)
                {
                    var node = reachableNodes[Builder.SessionSeedRandom.Range(0, int.MaxValue) % reachableNodes.Count];

                    placedPositions.Add(node.Position);
                }
            }
        }

        var positionsList = new Il2CppSystem.Collections.Generic.List<Vector3>();

        for (var i = 0; i < placedPositions.Count; i++)
            positionsList.Add(placedPositions[i]);

        *positions = positionsList.Pointer;

        Plugin.Logger.LogDebug($"[NodeTools] Final: placed {positionsList.Count}/{wantedCount} positions");

        if (placedPositions.Count > 1)
        {
            // Consecutive distances (what the player walks scan-to-scan)
            if (placedNodes.Count > 1)
            {
                float minConsec = float.MaxValue, maxConsec = 0f, totalConsec = 0f;

                for (var i = 1; i < placedNodes.Count; i++)
                {
                    var d = UseGraphDistance
                        ? GetGraphDistance(placedNodes[i], placedNodes[i - 1])
                        : (placedPositions[i] - placedPositions[i - 1]).magnitude;
                    if (d < minConsec) minConsec = d;
                    if (d > maxConsec) maxConsec = d;
                    totalConsec += d;
                }
                var consecCount = placedNodes.Count - 1;
                Plugin.Logger.LogDebug(
                    $"[NodeTools] Consecutive: min={minConsec:F2} max={maxConsec:F2} " +
                    $"mean={totalConsec / consecCount:F2} (walk distance)");
            }

            // Pairwise distances (minimum separation between any two scans)
            float minSep = float.MaxValue, maxSep = 0f, totalSep = 0f;
            var sepCount = 0;

            for (var i = 0; i < placedPositions.Count; i++)
                for (var j = i + 1; j < placedPositions.Count; j++)
                {
                    var d = (placedPositions[i] - placedPositions[j]).magnitude;
                    if (d < minSep) minSep = d;
                    if (d > maxSep) maxSep = d;
                    totalSep += d;
                    sepCount++;
                }
            Plugin.Logger.LogDebug(
                $"[NodeTools] Pairwise: min={minSep:F2} max={maxSep:F2} " +
                $"mean={totalSep / sepCount:F2} (Euclidean)");
        }

        return 1;
    }

    private static int PlaceCandidates(
        List<ScoredNode> candidates,
        List<Vector3> placedPositions,
        List<AIG_INode> placedNodes,
        int wantedCount,
        float minDistance,
        Vector3 sourcePos,
        float atRadiusFromSourcePos)
    {
        var placed = 0;

        // Work on a mutable copy
        var working = new List<ScoredNode>(candidates);

        for (var i = 0; i < wantedCount; i++)
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

                var distToSource = (best.node.Position - sourcePos).magnitude;
                Plugin.Logger.LogDebug(
                    $"[NodeTools]   Component {placedPositions.Count}: distToSource={distToSource:F2}");
                continue;
            }

            // Find the best candidate considering distance to placed positions
            AIG_INode bestNode = null;
            var bestScore = float.MaxValue;
            var bestIdx = -1;
            var bestAngularScore = 0f;

            for (var j = 0; j < working.Count; j++)
            {
                var candidate = working[j];

                // Hard filter: reject if too close to ANY placed scan (pairwise floor)
                var minDistToPlaced = GetMinDistanceToPlaced(candidate.node, placedPositions, placedNodes);

                if (UseHardDistanceFilter)
                    if (minDistToPlaced < minDistance * HardFilterFraction)
                        continue;

                // Choose separation metric based on placement context
                float separationDist;
                if (_isClusterPlacement)
                {
                    // Fan scoring: maximize spread from ALL placed scans
                    separationDist = minDistToPlaced;
                }
                else
                {
                    // Consecutive scoring: optimize walk path from previous scan
                    var prevNode = placedNodes[placedNodes.Count - 1];
                    separationDist = UseGraphDistance
                        ? GetGraphDistance(candidate.node, prevNode)
                        : (candidate.node.Position - placedPositions[placedPositions.Count - 1]).magnitude;
                }

                // Angular diversity (Mode B only): reward candidates whose direction
                // from sourcePos is far from any already-placed direction.
                var angularScore = 0f;
                if (atRadiusFromSourcePos > 0.01f && placedPositions.Count > 0)
                    angularScore = GetAngularDiversityScore(
                        candidate.node.Position, sourcePos, placedPositions);

                float score;
                if (UseCombinedScoring)
                {
                    var separationScore = Mathf.Abs(separationDist - minDistance) / Mathf.Max(minDistance, 0.001f);

                    score = SourceDistanceWeight * candidate.score
                          + SeparationWeight * separationScore
                          + AngularDiversityWeight * angularScore;
                }
                else
                {
                    score = candidate.score;
                    if (!UseHardDistanceFilter && separationDist < minDistance)
                        score += VanillaPenalty;
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    bestNode = candidate.node;
                    bestIdx = j;
                    bestAngularScore = angularScore;
                }
            }

            if (bestNode == null)
                break;

            working.RemoveAt(bestIdx);
            placedPositions.Add(bestNode.Position);
            placedNodes.Add(bestNode);
            placed++;

            var dSource = (bestNode.Position - sourcePos).magnitude;
            var dNearest = GetMinDistanceToPlaced(
                bestNode, placedPositions, placedNodes,
                excludeSelf: true);

            // Distance to previous consecutive scan (what the player walks)
            var logPrevNode = placedNodes[placedNodes.Count - 2];
            var dPrev = UseGraphDistance
                ? GetGraphDistance(bestNode, logPrevNode)
                : (bestNode.Position - placedPositions[placedPositions.Count - 2]).magnitude;

            var angleSuffix = atRadiusFromSourcePos > 0.01f
                ? $" minAngleToPlaced={(1f - bestAngularScore) * 180f:F0}°"
                : "";

            Plugin.Logger.LogDebug(
                $"[NodeTools]   Component {placedPositions.Count}: " +
                $"distToSource={dSource:F2} distToPrev={dPrev:F2} distToNearest={dNearest:F2}" +
                angleSuffix);
        }

        return placed;
    }

    private static float GetMinDistanceToPlaced(
        AIG_INode candidate,
        List<Vector3> placedPositions,
        List<AIG_INode> placedNodes,
        bool excludeSelf = false)
    {
        var minDist = float.MaxValue;

        for (var i = 0; i < placedNodes.Count; i++)
        {
            if (excludeSelf && (candidate.Position - placedNodes[i].Position).sqrMagnitude < 0.0001f)
                continue;

            float dist;

            dist = UseGraphDistance
                ? GetGraphDistance(candidate, placedNodes[i])
                : (candidate.Position - placedPositions[i]).magnitude;

            if (dist < minDist)
                minDist = dist;
        }

        return minDist;
    }

    // Returns 0 when candidate direction is opposite to all placed directions (ideal
    // spread), 1 when candidate sits on top of a placed direction (worst). XZ-projected
    // — levels can be multi-floor and angular spread in the horizontal plane is what
    // matters for gameplay. Returns 0 when no valid placed direction exists.
    private static float GetAngularDiversityScore(
        Vector3 candidatePos,
        Vector3 sourcePos,
        List<Vector3> placedPositions)
    {
        var cDir = new Vector2(candidatePos.x - sourcePos.x, candidatePos.z - sourcePos.z);

        if (cDir.sqrMagnitude < AngularEpsilon * AngularEpsilon)
            return 0f;

        cDir.Normalize();

        var maxDot = -1f;

        for (var i = 0; i < placedPositions.Count; i++)
        {
            var pDir = new Vector2(
                placedPositions[i].x - sourcePos.x,
                placedPositions[i].z - sourcePos.z);

            if (pDir.sqrMagnitude < AngularEpsilon * AngularEpsilon)
                continue;

            pDir.Normalize();

            var dot = Mathf.Clamp(Vector2.Dot(cDir, pDir), -1f, 1f);

            if (dot > maxDot)
                maxDot = dot;
        }

        if (maxDot < -0.9999f)
            return 0f;

        var minAngle = Mathf.Acos(maxDot);

        return 1f - (minAngle / Mathf.PI);
    }

    private static float GetGraphDistance(AIG_INode from, AIG_INode to)
    {
        try
        {
            var fromPos = from.Position;
            var toPos = to.Position;
            var diff = fromPos - toPos;

            if (!_loggedFirstCall)
            {
                _loggedFirstCall = true;
                Plugin.Logger.LogDebug(
                    $"[NodeTools] GetGraphDistance first call: " +
                    $"from={fromPos} to={toPos} " +
                    $"sqrDist={diff.sqrMagnitude:F6} " +
                    $"eq={fromPos == toPos}");
            }

            if (diff.sqrMagnitude < 0.0001f)
                return 0f;

            if (_navMeshFailed)
                return diff.magnitude;

            var navDist = GetNavMeshDistance(fromPos, toPos);

            if (!_loggedNavMeshStatus)
            {
                _loggedNavMeshStatus = true;
                Plugin.Logger.LogDebug(
                    $"[NodeTools] NavMesh distance: " +
                    $"nav={navDist:F2} euclidean={diff.magnitude:F2}");
            }

            return navDist;
        }
        catch (Exception ex)
        {
            if (!_navMeshFailed)
            {
                _navMeshFailed = true;
                Plugin.Logger.LogWarning(
                    $"[NodeTools] GetGraphDistance threw, disabling NavMesh: {ex}");
            }
            try { return (from.Position - to.Position).magnitude; }
            catch { return float.MaxValue; }
        }
    }

    private static float GetNavMeshDistance(Vector3 from, Vector3 to)
    {
        try
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
        }
        catch (Exception ex)
        {
            if (!_navMeshFailed)
            {
                _navMeshFailed = true;
                Plugin.Logger.LogWarning($"[NodeTools] NavMesh.CalculatePath threw: {ex}");
            }
        }
        return (from - to).magnitude;
    }

    private static void ScoreOnPreferredDistance_Combined(
        Vector3 sourcePos,
        List<ScoredNode> scoredNodes,
        float preferredDistance)
    {
        for (var i = 0; i < scoredNodes.Count; i++)
        {
            var scored = scoredNodes[i];
            var distToSource = (scored.node.Position - sourcePos).magnitude;
            // When radius=0, source distance is irrelevant (first scan is placed at
            // the door by ChainedPuzzleInstance). Only score by occlusion.
            var sourceScore = preferredDistance > 0.01f
                ? Mathf.Abs(distToSource - preferredDistance) / preferredDistance
                : 0f;

            scored.score = sourceScore
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

        for (var i = 0; i < original.Count; i++)
            if (!excludeSet.Contains(original[i].node))
                result.Add(original[i]);

        return result;
    }
}

[HarmonyPatch(typeof(CP_Cluster_Core))]
internal static class Patch_CP_Cluster_Core
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CP_Cluster_Core.Setup))]
    static void Prefix() => Patch_LG_NodeTools._isClusterPlacement = true;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CP_Cluster_Core.Setup))]
    static void Postfix() => Patch_LG_NodeTools._isClusterPlacement = false;
}
