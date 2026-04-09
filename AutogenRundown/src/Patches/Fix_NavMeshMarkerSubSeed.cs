using AIGraph;
using AutogenRundown.Managers;
using GameData;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches;

using MainStatus = LG_ZoneJob_CreateExpandFromData.MainStatus;

[HarmonyPatch]
public class Fix_NavMeshMarkerSubSeed
{
    private static bool s_suppressHook = false; // prevent recursion while we recompute

    private const int MaxAttemptsPerZone = 128;

    private const int SubSeedEscalationThreshold = 16;

    /// <summary>
    /// Maximum grid radius to search when bridging a voxel gap between
    /// the source cluster and an unreachable gate region.
    /// </summary>
    private const int MaxBridgeSearchRadius = 10;

    public static readonly Dictionary<(eDimensionIndex dim, eLocalZoneIndex lz), int> ZoneAttempts = new();

    public static readonly HashSet<(eDimensionIndex dim, eLocalZoneIndex lz)> TargetsDetected = new();

    public static readonly Dictionary<(eDimensionIndex dim, eLocalZoneIndex lz), uint> MarkerSubSeeds = new();

    public static void Setup()
    {
        FactoryJobManager.OnDoneValidate += Validate;
    }

    private static bool Validate()
    {
        try
        {
            // Find first unhealthy detected target
            foreach (var key in TargetsDetected)
            {
                var zone = FindZone(key);

                if (zone == null)
                    continue;

                if (IsZoneHealthy(zone))
                    continue;

                // Bump and rebuild
                var current = MarkerSubSeeds.TryGetValue(key, out var v) ? v : zone.m_markerSubSeed;
                var next = current + 1;
                MarkerSubSeeds[key] = next;
                ZoneAttempts[key] = (ZoneAttempts.TryGetValue(key, out var a) ? a : 0) + 1;

                if (ZoneAttempts[key] > MaxAttemptsPerZone)
                {
                    Plugin.Logger.LogError($"[Reroll] Max attempts reached for {key}. Last m_markerSubSeed={current}");
                    TargetsDetected.Remove(key);
                    break;
                }

                // Escalate to full subseed reroll periodically for stronger geometry changes
                if (ZoneAttempts[key] % SubSeedEscalationThreshold == 0)
                {
                    Plugin.Logger.LogWarning($"[Reroll] Escalating {key} to subseed reroll after {ZoneAttempts[key]} attempts");
                    ZoneSeedManager.Reroll_SubSeed(zone);
                }

                Plugin.Logger.LogDebug($"[Reroll] Rebuilding {key} with m_markerSubSeed={next} (attempt {ZoneAttempts[key]})");

                return false;
            }

            // All detected targets healthy? Clean up
            // (re-check and prune)
            var toRemove = new List<(eDimensionIndex, eLocalZoneIndex)>();

            foreach (var key in TargetsDetected)
            {
                var zone = FindZone(key);

                if (zone != null && IsZoneHealthy(zone))
                {
                    Plugin.Logger.LogDebug($"[Reroll] {key} healthy. Attempts={ZoneAttempts.GetValueOrDefault(key, 0)}, m_markerSubSeed={zone.m_markerSubSeed}");
                    toRemove.Add(key);
                }
            }

            foreach (var key in toRemove)
                TargetsDetected.Remove(key);

            // Release suppression when done
            if (TargetsDetected.Count == 0)
            {
                Plugin.Logger.LogDebug("[Reroll] All detected zones healthy. Releasing factory done suppression.");

                return true;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Encountered a bad error: {ex}");
        }

        return true;
    }

    private static bool IsZoneHealthy(LG_Zone zone)
    {
        foreach (var cn in zone.m_courseNodes)
        {
            var cl = cn?.m_nodeCluster;

            if (cl == null || cl.m_reachableNodes == null || cl.m_reachableNodes.Count <= 1)
                return false;

            // Portal match failed: AssignMatch was never called
            if (cl.CourseNode == null)
                return false;
        }
        return true;
    }

    private static LG_Zone? FindZone((eDimensionIndex dim, eLocalZoneIndex lz) key)
    {
        var floor = Builder.CurrentFloor;

        if (floor == null)
            return null;

        Dimension dim;

        if (!floor.GetDimension(key.dim, out dim) || dim == null)
            return null;

        foreach (var layer in dim.Layers)
        {
            foreach (var zone in layer.m_zones)
                if (zone.LocalIndex == key.lz)
                    return zone;
        }

        return null;
    }

    [HarmonyPatch(typeof(LG_Layer), nameof(LG_Layer.CreateZone))]
    [HarmonyPrefix]
    static void Prefix_Layer_CreateZone(LG_Layer __instance, LG_Floor floor, ref ExpeditionZoneData zoneData, int zoneAliasStart)
    {
        // TODO: we need to account for the different layers

        // TODO: we probably want to extract this to it's own manager/patcher so we can override seeds from other places too

        Plugin.Logger.LogDebug($"--------------------------> [Reroll] GOTCHA!!! " +
                               $"m_markerSubSeed={zoneData.MarkerSubSeed} to " +
                               $"{zoneData.LocalIndex} (dim:{__instance.m_dimension.DimensionIndex})");

        var key = (__instance.m_dimension.DimensionIndex, zoneData.LocalIndex);

        // if (ZoneSeedManager.SubSeeds.TryGetValue(
        //         (__instance.m_dimension.DimensionIndex, __instance.m_type, zoneData.LocalIndex),
        //         out var overrideSubSeed))
        // {
        //     zoneData.SubSeed = (int)overrideSubSeed;
        //
        //     Plugin.Logger.LogDebug($"[Reroll] Applied override m_subSeed={overrideSubSeed} to Zone_{zoneData.LocalIndex}");
        // }

        if (MarkerSubSeeds.TryGetValue(key, out var overrideSeed))
        {
            zoneData.MarkerSubSeed = (int)overrideSeed;

            Plugin.Logger.LogDebug($"[Reroll] Applied override m_markerSubSeed={overrideSeed} to Zone_{zoneData.LocalIndex}");
        }
    }

    // Optional: detect degenerate areas early (no action here; we rebuild on factory done)
    [HarmonyPatch(typeof(LG_NodeTools), nameof(LG_NodeTools.CalculateReachableNodesAndLocationScores))]
    [HarmonyPostfix]
    static void Postfix_CalculateReachableNodesAndLocationScores(LG_Area area, Vector3 sourcePos)
    {
        try
        {
            var zone = area?.m_zone;

            if (zone == null)
                return;

            var cluster = area.m_courseNode?.m_nodeCluster;

            // Skip arena dimensions — these are simple single-tile arenas (e.g. pouncer)
            // that naturally have m_reachableNodes.Count <= 1
            if (Builder.CurrentFloor.GetDimension(zone.DimensionIndex, out var dim) && dim.IsArenaDimension)
                return;

            if (cluster == null || cluster.m_reachableNodes.Count > 1)
                return;

            var key = (zone.DimensionIndex, zone.LocalIndex);

            if (!TargetsDetected.Contains(key))
            {
                TargetsDetected.Add(key);

                FactoryJobManager.MarkForRebuild();

                // Initialize override baseline if not present
                if (!MarkerSubSeeds.ContainsKey(key))
                    MarkerSubSeeds[key] = zone.m_markerSubSeed;

                if (!ZoneAttempts.ContainsKey(key))
                    ZoneAttempts[key] = 0;

                Plugin.Logger.LogDebug($"[Reroll] Detected unhealthy zone {key}. Will reroll after factory completion.");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex);
        }
    }


    /// <summary>
    /// Detects portal mismatches after node cluster BFS completes and attempts an
    /// in-place repair by bridging the voxel gap between the source cluster and the
    /// unreachable gate region. Falls back to marking for reroll if repair fails.
    /// </summary>
    [HarmonyPatch(typeof(LG_BuildNodeClusterJobData), nameof(LG_BuildNodeClusterJobData.TryComplete))]
    [HarmonyPostfix]
    static void Postfix_TryComplete(LG_BuildNodeClusterJobData __instance, bool __result)
    {
        try
        {
            if (!__result)
                return;

            var scanData = __instance.m_scanData;
            var courseNode = scanData.m_sourceCourseNode;
            var cluster = scanData.m_clusterResult;

            if (cluster == null || courseNode == null)
                return;

            var hitCount = scanData.CoursePortalsHit.Count;
            var expectedCount = courseNode.m_portals.Count;

            if (hitCount == expectedCount)
                return;

            var zone = courseNode.m_zone;

            // Skip arena dimensions
            if (Builder.CurrentFloor.GetDimension(zone.DimensionIndex, out var dim) && dim.IsArenaDimension)
                return;

            Plugin.Logger.LogDebug(
                $"[Repair] Portal mismatch in {courseNode.Name} (zone {zone.LocalIndex}): " +
                $"expected={expectedCount}, hit={hitCount}. Attempting repair.");

            var volume = scanData.m_voxelNodeVolume;
            var repaired = false;

            foreach (var portal in courseNode.m_portals)
            {
                if (scanData.CoursePortalsHit.Contains(portal))
                    continue;

                var gate = portal.Gate;
                if (gate == null)
                    continue;

                // Collect unclaimed voxel nodes adjacent to gate nodes
                var gateRegion = new List<AIG_INode>();
                foreach (var gateNode in gate.Nodes)
                {
                    foreach (var link in gateNode.Links)
                    {
                        if (!link.BelongsToGate && link.ClusterID == 0)
                            gateRegion.Add(link);
                    }
                }

                if (gateRegion.Count == 0)
                {
                    Plugin.Logger.LogWarning(
                        $"[Repair] No unclaimed voxel nodes near gate {gate.name}. Cannot bridge.");
                    continue;
                }

                // Search for the closest (clusterNode, gateRegionNode) pair
                AIG_INode bestCluster = null;
                AIG_INode bestGateRegion = null;
                float bestDistSq = float.MaxValue;

                foreach (var grNode in gateRegion)
                {
                    volume.GetGridPosition(grNode.Position, out var gx, out var gz);
                    float minH = grNode.Position.y - 3f;
                    float maxH = grNode.Position.y + 3f;

                    for (int radius = 1; radius <= MaxBridgeSearchRadius; radius++)
                    {
                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            for (int dz = -radius; dz <= radius; dz++)
                            {
                                // Only check the outer ring at this radius
                                if (Math.Abs(dx) < radius && Math.Abs(dz) < radius)
                                    continue;

                                if (!volume.TryGetPillar(gx + dx, gz + dz, out var pillar))
                                    continue;

                                // TryGetVoxelNode returns AIG_INode, avoiding IL2CPP cast issues.
                                // The (minH, maxH) overload returns the first node in the height range.
                                if (!pillar.TryGetVoxelNode(minH, maxH, out var candidateNode))
                                    continue;

                                if (candidateNode.ClusterID != cluster.ID)
                                    continue;

                                var distSq = (candidateNode.Position - grNode.Position).sqrMagnitude;
                                if (distSq < bestDistSq)
                                {
                                    bestDistSq = distSq;
                                    bestCluster = candidateNode;
                                    bestGateRegion = grNode;
                                }
                            }
                        }

                        // Early out if we found something at this radius
                        if (bestCluster != null)
                            break;
                    }
                }

                if (bestCluster == null || bestGateRegion == null)
                {
                    Plugin.Logger.LogWarning(
                        $"[Repair] No cluster node found within search radius for gate {gate.name}.");
                    continue;
                }

                Plugin.Logger.LogDebug(
                    $"[Repair] Bridging gap: dist={Math.Sqrt(bestDistSq):F1}m " +
                    $"for gate {gate.name}");

                // Create bidirectional bridge link
                bestCluster.Links.Add(bestGateRegion);
                bestGateRegion.Links.Add(bestCluster);

                // Mini-BFS from bridge point to absorb gate-side nodes into cluster
                var queue = new Queue<AIG_INode>();
                cluster.AddNode(bestGateRegion);
                queue.Enqueue(bestGateRegion);

                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();
                    foreach (var link in node.Links)
                    {
                        if (link.ClusterID == cluster.ID)
                            continue; // Already in cluster

                        if (link.BelongsToGate)
                        {
                            // Record portal hit without adding gate node to cluster
                            var cp = link.Gate.CoursePortal;
                            if (cp != null && !scanData.CoursePortalsHit.Contains(cp))
                                scanData.CoursePortalsHit.Add(cp);
                        }
                        else if (link.ClusterID == 0)
                        {
                            cluster.AddNode(link);
                            queue.Enqueue(link);
                        }
                    }
                }

                repaired = true;
            }

            // Re-check portal matching after repair
            if (scanData.CoursePortalsHit.Count == courseNode.m_portals.Count)
            {
                cluster.AssignMatch(courseNode);
                Plugin.Logger.LogInfo(
                    $"[Repair] Successfully repaired portal connectivity for {courseNode.Name}");
            }
            else if (repaired)
            {
                Plugin.Logger.LogWarning(
                    $"[Repair] Partial repair for {courseNode.Name}: " +
                    $"hit={scanData.CoursePortalsHit.Count}/{courseNode.m_portals.Count}. " +
                    $"Falling back to reroll.");
                MarkZoneForReroll(zone);
            }
            else
            {
                Plugin.Logger.LogWarning(
                    $"[Repair] Could not repair {courseNode.Name}. Falling back to reroll.");
                MarkZoneForReroll(zone);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"[Repair] Error in Postfix_TryComplete: {ex}");
        }
    }

    private static void MarkZoneForReroll(LG_Zone zone)
    {
        var key = (zone.DimensionIndex, zone.LocalIndex);

        if (TargetsDetected.Contains(key))
            return;

        TargetsDetected.Add(key);
        FactoryJobManager.MarkForRebuild();

        if (!MarkerSubSeeds.ContainsKey(key))
            MarkerSubSeeds[key] = zone.m_markerSubSeed;

        if (!ZoneAttempts.ContainsKey(key))
            ZoneAttempts[key] = 0;

        Plugin.Logger.LogDebug($"[Reroll] Marked zone {key} for reroll.");
    }

    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyPrefix]
    public static void CaptureState(LG_ZoneJob_CreateExpandFromData __instance, out BuildState __state)
    {
        __state = new BuildState(__instance.m_mainStatus, __instance.m_subStatus);
    }

    /// <summary>
    /// Rerolls zones that didn't get their custom geos
    ///
    /// Also re-roll the parent zones subseed. We often see that the issue is the parent zone
    /// needs to be re-rolled.
    ///
    /// TODO: Handle zone_0 zones, notably secondary/extreme
    /// TODO: Handle dimensions
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__state"></param>
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyPostfix]
    public static void ForceRerollForMissingCustomGeos(LG_ZoneJob_CreateExpandFromData __instance, BuildState __state)
    {
        var mainStatus = __state.MainStatus;
        // var subStatus = __state.SubStatus;

        var zone = __instance.m_zone;
        var data = __instance.m_zoneData;
        var settings = __instance.m_zoneSettings;

        if (mainStatus != MainStatus.Done)
            return;

        if (data?.CustomGeomorph != null && !settings.HasCustomGeomorphInstance)
        {
            ZoneSeedManager.Reroll_SubSeed(zone);

            if (zone.LocalIndex == eLocalZoneIndex.Zone_0)
                return;

            ZoneSeedManager.Reroll_SubSeed(data.BuildFromLocalIndex, zone.DimensionIndex, zone.Layer.m_type);
        }
    }
}
