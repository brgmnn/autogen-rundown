using System.Reflection;
using AIGraph;
using AutogenRundown.DataBlocks.Custom.ZoneScan;
using BepInEx.Unity.IL2CPP.Hook;
using ChainedPuzzles;
using GameData;
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
    private const float AngularDiversityWeight = 0.5f;

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

    // Set by Patch_ChainedPuzzleInstance_AreaScans for alarms of several area scans (ZoneScan).
    // ZoneScan scans the area each scan sits in, so each scan goes in an area of the zone no
    // earlier scan used, just inside the gate it is entered through. See TryPlaceAreaScans.
    internal static bool _distinctAreas;

    // Area scans: preferred and minimum distance of the scan from the gate into its area
    private const float GateScanDistance = 6f;
    private const float MinGateScanDistance = 3f;

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

        if (_distinctAreas && atRadiusFromSourcePos <= 0.01f)
        {
            // An exception escaping a native detour crashes the game: fall back to the normal
            // placement instead
            List<Vector3>? areaPositions = null;

            try
            {
                if (!TryPlaceAreaScans(area, sourcePos, wantedCount, out areaPositions))
                    areaPositions = null;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogWarning($"[NodeTools] Area scan placement failed: {ex.Message}");
                areaPositions = null;
            }

            if (areaPositions != null)
            {
                var areaPositionsList = new Il2CppSystem.Collections.Generic.List<Vector3>();

                foreach (var position in areaPositions)
                    areaPositionsList.Add(position);

                *positions = areaPositionsList.Pointer;
                return 1;
            }
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

                // Mode B = radius > 0: a puzzle type fanning sub-scans around a single
                // source point (CP_Cluster_Core, bulkhead door controller, SetupMovement).
                // Mode A = radius ≈ 0: top-level alarm distributing along a walk path.
                var isFanMode = atRadiusFromSourcePos > 0.01f;

                // Choose separation metric based on placement context
                float separationDist;
                if (isFanMode || _isClusterPlacement)
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
                if (isFanMode && placedPositions.Count > 0)
                    angularScore = GetAngularDiversityScore(
                        candidate.node.Position, sourcePos, placedPositions);

                float score;
                if (UseCombinedScoring)
                {
                    // Mode A targets an ideal consecutive spacing = minDistance.
                    // Mode B treats minDistance as a FLOOR and rewards extra spread —
                    // the target-based formula would punish well-fanned candidates
                    // and fight the angular term.
                    float separationScore;
                    if (isFanMode)
                        separationScore = minDistance / Mathf.Max(separationDist, minDistance);
                    else
                        separationScore = Mathf.Abs(separationDist - minDistance) / Mathf.Max(minDistance, 0.001f);

                    var separationWeight = isFanMode ? 1.0f : SeparationWeight;

                    score = SourceDistanceWeight * candidate.score
                          + separationWeight * separationScore
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

    /// <summary>
    /// Areas passed through on the way to each area scan placed by the current
    /// <see cref="TryPlaceAreaScans"/>: for every gate, the nearest node on each side of it.
    /// Used by Patch_CP_Holopath_Spline_AreaScans to draw the holopath through the gates.
    /// </summary>
    private static readonly List<(Vector3 Target, List<Vector3> Waypoints)> _areaScanRoutes = new();

    internal static void ClearAreaScanRoutes() => _areaScanRoutes.Clear();

    /// <summary>
    /// Takes the gate waypoints recorded for the area scan at <paramref name="target"/>
    /// </summary>
    internal static bool TryTakeAreaScanRoute(Vector3 target, out List<Vector3> waypoints)
    {
        var index = _areaScanRoutes.FindIndex(route => (route.Target - target).sqrMagnitude < 0.25f);

        if (index < 0)
        {
            waypoints = null!;
            return false;
        }

        waypoints = _areaScanRoutes[index].Waypoints;
        _areaScanRoutes.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Places area scans: the first at the door (sourcePos), every next one in the nearest area
    /// (by gates, within the zone) that has no scan yet, a few metres in from the gate it is
    /// entered through. Keeps the scans off the walls and the holopath going through the gate.
    /// Once every area has a scan, areas are reused, except the previous scan's.
    /// </summary>
    private static bool TryPlaceAreaScans(
        LG_Area sourceArea,
        Vector3 sourcePos,
        int wantedCount,
        out List<Vector3> positions)
    {
        positions = new List<Vector3> { sourcePos };
        _areaScanRoutes.Clear();

        var zone = sourceArea.m_zone;

        if (zone == null)
            return false;

        var used = new HashSet<IntPtr> { sourceArea.Pointer };
        var previous = sourceArea;

        while (positions.Count < wantedCount)
        {
            if (!TryFindNextArea(previous, zone, used, out var route))
            {
                used.Clear();
                used.Add(previous.Pointer);

                if (!TryFindNextArea(previous, zone, used, out route))
                    return false;
            }

            var (_, gate, next) = route[^1];

            if (!TryGetPositionNearGate(next, gate, out var position))
                return false;

            var waypoints = new List<Vector3>();

            foreach (var (from, routeGate, to) in route)
            {
                var gatePosition = routeGate.transform.position;

                waypoints.Add(GetClosestNodePosition(from, gatePosition));
                waypoints.Add(GetClosestNodePosition(to, gatePosition));
            }

            _areaScanRoutes.Add((position, waypoints));

            positions.Add(position);
            used.Add(next.Pointer);
            previous = next;

            Plugin.Logger.LogDebug(
                $"[NodeTools] Area scan {positions.Count}: {next.name}, " +
                $"{(position - gate.transform.position).magnitude:F1}m from its gate, " +
                $"{route.Count} gate(s) from the previous scan");
        }

        return true;
    }

    /// <summary>
    /// Breadth first search over the gates of the zone's areas for the nearest area not in
    /// <paramref name="used"/>. <paramref name="route"/> is the gates passed through to reach
    /// it, with the areas on either side, the last one entering the found area.
    /// </summary>
    private static bool TryFindNextArea(
        LG_Area from,
        LG_Zone zone,
        HashSet<IntPtr> used,
        out List<(LG_Area From, LG_Gate Gate, LG_Area To)> route)
    {
        var cameFrom = new Dictionary<IntPtr, (LG_Area From, LG_Gate Gate, LG_Area To)>();
        var visited = new HashSet<IntPtr> { from.Pointer };
        var queue = new Queue<LG_Area>();

        queue.Enqueue(from);

        while (queue.Count > 0)
        {
            var area = queue.Dequeue();
            var gates = area.m_gates;

            if (gates == null)
                continue;

            for (var i = 0; i < gates.Count; i++)
            {
                var candidateGate = gates[i];

                if (candidateGate == null)
                    continue;

                var opposite = candidateGate.GetOppositeArea(area);

                if (opposite == null ||
                    opposite.m_zone == null ||
                    opposite.m_zone.Pointer != zone.Pointer ||
                    !visited.Add(opposite.Pointer))
                    continue;

                cameFrom[opposite.Pointer] = (area, candidateGate, opposite);

                if (!used.Contains(opposite.Pointer))
                {
                    route = new List<(LG_Area, LG_Gate, LG_Area)>();

                    for (var step = opposite; step.Pointer != from.Pointer;)
                    {
                        var hop = cameFrom[step.Pointer];
                        route.Insert(0, hop);
                        step = hop.From;
                    }

                    return true;
                }

                queue.Enqueue(opposite);
            }
        }

        route = null!;
        return false;
    }

    private static Il2CppSystem.Collections.Generic.List<AIG_INode>? GetAreaNodes(LG_Area area)
    {
        var courseNode = area.m_courseNode;

        if (courseNode == null || !courseNode.IsValid || courseNode.m_nodeCluster == null)
            return null;

        var cluster = courseNode.m_nodeCluster;

        return cluster.m_reachableNodes.Count >= 2 ? cluster.m_reachableNodes : cluster.m_nodes;
    }

    /// <summary>
    /// Position of the node of <paramref name="area"/> closest to <paramref name="position"/>,
    /// or <paramref name="position"/> itself if the area has no nodes.
    /// </summary>
    private static Vector3 GetClosestNodePosition(LG_Area area, Vector3 position)
    {
        var nodes = GetAreaNodes(area);
        var closest = position;
        var bestDistance = float.MaxValue;

        if (nodes == null)
            return closest;

        for (var i = 0; i < nodes.Count; i++)
        {
            var distance = (nodes[i].Position - position).sqrMagnitude;

            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            closest = nodes[i].Position;
        }

        return closest;
    }

    /// <summary>
    /// The node of <paramref name="area"/> closest to <see cref="GateScanDistance"/> from the
    /// gate, but no closer than <see cref="MinGateScanDistance"/>.
    /// </summary>
    private static bool TryGetPositionNearGate(LG_Area area, LG_Gate gate, out Vector3 position)
    {
        position = Vector3.zero;

        var nodes = GetAreaNodes(area);

        if (nodes == null)
            return false;

        var gatePosition = gate.transform.position;
        var bestScore = float.MaxValue;

        for (var i = 0; i < nodes.Count; i++)
        {
            var distance = (nodes[i].Position - gatePosition).magnitude;

            if (distance < MinGateScanDistance)
                continue;

            var score = Mathf.Abs(distance - GateScanDistance);

            if (score >= bestScore)
                continue;

            bestScore = score;
            position = nodes[i].Position;
        }

        return bestScore < float.MaxValue;
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

/// <summary>
/// Flags alarms made of several area scans (ZoneScan) so their scans are placed in different
/// areas of the zone, see Patch_LG_NodeTools._distinctAreas.
/// </summary>
[HarmonyPatch(typeof(ChainedPuzzleInstance), nameof(ChainedPuzzleInstance.Setup))]
internal static class Patch_ChainedPuzzleInstance_AreaScans
{
    static void Prefix(ChainedPuzzleDataBlock data)
    {
        Patch_LG_NodeTools._distinctAreas = false;
        Patch_LG_NodeTools.ClearAreaScanRoutes();

        if (data?.ChainedPuzzle == null || data.ChainedPuzzle.Count < 2)
            return;

        for (var i = 0; i < data.ChainedPuzzle.Count; i++)
        {
            var type = ZoneScans.Types.Find(
                t => (uint)t.Component.PuzzleType == data.ChainedPuzzle[i].PuzzleType);

            if (type is null || type.UseZone)
                return;
        }

        Patch_LG_NodeTools._distinctAreas = true;
    }

    static void Postfix()
    {
        Patch_LG_NodeTools._distinctAreas = false;
        Patch_LG_NodeTools.ClearAreaScanRoutes();
    }
}

/// <summary>
/// Holopath to an area scan in another area. The vanilla path is a single navmesh path, but
/// closed doors between areas cut the navmesh (LG_InternalGate's NavMeshObstacle), so it stops
/// at the first closed door. Vanilla never needs more: its scans are all in one area.
///
/// Instead walk the gates TryPlaceAreaScans went through: a navmesh path inside each area, and
/// a straight line through each gate.
/// </summary>
[HarmonyPatch(typeof(CP_Holopath_Spline), nameof(CP_Holopath_Spline.GeneratePath))]
internal static class Patch_CP_Holopath_Spline_AreaScans
{
    static bool Prefix(CP_Holopath_Spline __instance, Vector3 from, Vector3 to)
    {
        if (!__instance.m_isSetup || !Patch_LG_NodeTools.TryTakeAreaScanRoute(to, out var waypoints))
            return true;

        try
        {
            var start = from;
            __instance.TryGetPosOnNavMesh(ref start);

            var points = new List<Vector3> { start };

            for (var i = 0; i + 1 < waypoints.Count; i += 2)
            {
                AddLeg(points, waypoints[i]);
                points.Add(waypoints[i + 1]);
            }

            AddLeg(points, to);

            var previous = points[0];

            foreach (var point in points)
            {
                __instance.AddPositionOnSpline(point, (point - previous).magnitude);
                previous = point;
            }

            return false;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"[NodeTools] Area scan holopath failed: {ex.Message}");
            return true;
        }
    }

    /// <summary>
    /// Navmesh path from the last point to <paramref name="to"/>, and a straight line for
    /// whatever the navmesh doesn't reach.
    /// </summary>
    private static void AddLeg(List<Vector3> points, Vector3 to)
    {
        var path = new NavMeshPath();

        // -1: all navmesh areas, as vanilla's GeneratePath
        if (NavMesh.CalculatePath(points[^1], to, -1, path))
        {
            var corners = path.corners;

            for (var i = 1; i < corners.Length; i++)
                points.Add(corners[i]);
        }

        if ((points[^1] - to).sqrMagnitude > 0.01f)
            points.Add(to);
    }
}
