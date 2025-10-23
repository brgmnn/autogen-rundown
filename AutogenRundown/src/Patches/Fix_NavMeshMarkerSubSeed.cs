using AIGraph;
using GameData;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AutogenRundown.Patches;

// [HarmonyPatch(typeof(LG_NodeTools), nameof(LG_NodeTools.CalculateReachableNodesAndLocationScores))]
// static class CalcReachables_Patch
// {
//     static void Postfix(LG_Area area, Vector3 sourcePos)
//     {
//         var cluster = area?.m_courseNode?.m_nodeCluster;
//         if (cluster == null) return;
//         if (cluster.m_reachableNodes.Count > 1) return;
//
//         var zone = area.m_zone;
//         if (RerollRuntime.TryBegin(zone))
//             RerollRuntime.RerollAndRebuildZone(zone);
//     }
// }

// public class RerollBarrierJob : LG_FactoryJob
// {
//     private readonly LG_Zone _zone;
//
//     public RerollBarrierJob(LG_Zone zone)
//     {
//         _zone = zone;
//     }
//
//     public override string GetName() => $"RerollBarrier {_zone?.name}";
//
//     public override bool Build()
//     {
//         Fix_NavMeshMarkerSubSeed.End(_zone);
//         return true;
//     }
// }

[HarmonyPatch]
public class Fix_NavMeshMarkerSubSeed
{
    private static readonly HashSet<int> s_building = new();
    private static bool s_suppressHook = false; // prevent recursion while we recompute
    // private const int MaxAttempts = 8; // your choice

    private const int MaxAttemptsPerZone = 128;

    private static bool initialized = false;
    private static bool rebuildInProgress = false;
    private static bool shouldSuppressFactoryDone = false;

    private static readonly Dictionary<(eDimensionIndex dim, eLocalZoneIndex lz), int> zoneAttempts = new();

    private static readonly HashSet<(eDimensionIndex dim, eLocalZoneIndex lz)> s_targetsDetected = new();

    private static readonly Dictionary<(eDimensionIndex dim, eLocalZoneIndex lz), uint> markerSubSeeds = new();

    public static void Setup()
    {
        if (initialized)
            return;

        initialized = true;

        // // Let first encountered value (from data) seed our map at construction time
        // markerSubSeeds[(TargetDimension, TargetLocalIndex)] = 0;

        // React when the whole factory finishes a pass
        LG_Factory.OnFactoryBuildDone += new Action(OnFactoryDone);
    }

    public static void OnFactoryDone()
    {
        // return;

        try
        {
            if (rebuildInProgress)
                return; // guard

            // Find first unhealthy detected target
            foreach (var key in s_targetsDetected)
            {
                var zone = FindZone(key);

                if (zone == null)
                    continue;

                if (IsZoneHealthy(zone))
                    continue;

                // Bump and rebuild
                var current = markerSubSeeds.TryGetValue(key, out var v) ? v : zone.m_markerSubSeed;
                var next = current + 1;
                markerSubSeeds[key] = next;
                zoneAttempts[key] = (zoneAttempts.TryGetValue(key, out var a) ? a : 0) + 1;

                if (zoneAttempts[key] > MaxAttemptsPerZone)
                {
                    Plugin.Logger.LogError($"[Reroll] Max attempts reached for {key}. Last m_markerSubSeed={current}");
                    // Give up on this key but keep others (remove from detected set)
                    // Optionally: keep it to retry later
                    s_targetsDetected.Remove(key);
                    break;
                }

                Plugin.Logger.LogDebug($"[Reroll] Rebuilding {key} with m_markerSubSeed={next} (attempt {zoneAttempts[key]})");

                rebuildInProgress = true;

                Builder.Current.Build();

                return; // one rebuild per completion
            }

            // All detected targets healthy? Clean up
            // (re-check and prune)
            var toRemove = new List<(eDimensionIndex, eLocalZoneIndex)>();

            foreach (var key in s_targetsDetected)
            {
                var zone = FindZone(key);

                if (zone != null && IsZoneHealthy(zone))
                {
                    Plugin.Logger.LogDebug($"[Reroll] {key} healthy. Attempts={zoneAttempts.GetValueOrDefault(key, 0)}, m_markerSubSeed={zone.m_markerSubSeed}");
                    toRemove.Add(key);
                }
            }

            foreach (var key in toRemove)
                s_targetsDetected.Remove(key);

            // Release suppression when done
            if (s_targetsDetected.Count == 0)
            {
                Plugin.Logger.LogDebug("[Reroll] All detected zones healthy. Releasing factory‑done suppression.");
                shouldSuppressFactoryDone = false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Encountered a bad error: {ex}");
            shouldSuppressFactoryDone = false;
        }
        finally
        {
            rebuildInProgress = false;
        }
    }

    private static bool IsZoneHealthy(LG_Zone zone)
    {
        foreach (var cn in zone.m_courseNodes)
        {
            var cl = cn?.m_nodeCluster;

            if (cl == null || cl.m_reachableNodes == null || cl.m_reachableNodes.Count <= 1)
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
            //
            // if (layer?.m_zonesByLocalIndex != null && layer.m_zonesByLocalIndex.TryGetValue(key.lz, out var zone))
            //     return zone;
        }

        return null;
    }


    // Suppress default factory‑done listeners during reroll
    [HarmonyPatch(typeof(Builder), nameof(Builder.OnFactoryDone))]
    [HarmonyPrefix]
    public static bool Builder_OnFactoryDone_Suppress_Prefix()
    {
        if (shouldSuppressFactoryDone)
        {
            Debug.Log("[Reroll] Suppressing Builder.OnFactoryDone (rebuild in progress)");
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(EnvironmentStateManager), nameof(EnvironmentStateManager.OnFactoryBuildDone))]
    [HarmonyPrefix]
    public static bool Env_OnFactoryBuildDone_Suppress_Prefix()
    {
        if (shouldSuppressFactoryDone)
        {
            Debug.Log("[Reroll] Suppressing Builder.OnFactoryDone (rebuild in progress)");
            return false;
        }

        return true;
    }





    // [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), MethodType.Constructor,
    //     new Type[]
    //     {
    //         typeof(LG_Floor), typeof(Dimension), typeof(LG_Layer), typeof(int), typeof(GameData.ExpeditionZoneData),
    //         typeof(LG_Area)
    //     })]
    // [HarmonyPrefix]
    // static void Prefix_LG_ZoneJob_CreateExpandFromData_Ctor(
    //     LG_ZoneJob_CreateExpandFromData __instance,
    //     LG_Floor floor,
    //     Dimension dimension,
    //     LG_Layer layer,
    //     int id,
    //     ref ExpeditionZoneData zoneData,
    //     LG_Area? startArea)
    // {
    //     try
    //     {
    //         // var zone = __instance.m_zone; // public in decomp; use AccessTools if needed
    //
    //         // if (zone == null)
    //         //     return;
    //
    //         // var key = (zone.DimensionIndex, zone.LocalIndex);
    //         var key = (dimension.DimensionIndex, zoneData.LocalIndex);
    //
    //         if (markerSubSeeds.TryGetValue(key, out var overrideSeed))
    //         {
    //             // zone.m_markerSubSeed = overrideSeed;
    //             // Align the job’s internal field for consistency in logs
    //             // var f = AccessTools.Field(typeof(LG_ZoneJob_CreateExpandFromData), "m_markerSubSeed");
    //             // if (f != null) f.SetValue(__instance, overrideSeed);
    //
    //             zoneData.MarkerSubSeed = (int)overrideSeed;
    //
    //             Plugin.Logger.LogDebug($"[Reroll] Applied override m_markerSubSeed={overrideSeed} to Zone_{zoneData.LocalIndex} (dim:{dimension.DimensionIndex})");
    //         }
    //         // else
    //         // {
    //         //     // Seed our map from data default on first encounter
    //         //     markerSubSeeds[key] = zone.m_markerSubSeed;
    //         // }
    //     }
    //     catch (Exception ex)
    //     {
    //         Plugin.Logger.LogError(ex);
    //     }
    // }


    [HarmonyPatch(typeof(LG_Layer), nameof(LG_Layer.CreateZone))]
    [HarmonyPrefix]
    static void Prefix_Layer_CreateZone(LG_Layer __instance, LG_Floor floor, ref ExpeditionZoneData zoneData, int zoneAliasStart)
    {
        // TODO: we need to account for the different layers

        Plugin.Logger.LogDebug($"--------------------------> [Reroll] GOTCHA!!! " +
                               $"m_markerSubSeed={zoneData.MarkerSubSeed} to " +
                               $"{zoneData.LocalIndex} (dim:{__instance.m_dimension.DimensionIndex})");

        var key = (__instance.m_dimension.DimensionIndex, zoneData.LocalIndex);

        if (markerSubSeeds.TryGetValue(key, out var overrideSeed))
        {
            // zone.m_markerSubSeed = overrideSeed;
            // __instance.m_markerSubSeed = overrideSeed;

            zoneData.MarkerSubSeed = (int)overrideSeed;

            Plugin.Logger.LogDebug($"[Reroll] Applied override m_markerSubSeed={overrideSeed} to Zone_{zoneData.LocalIndex}");

            // // Align the job’s internal field for consistency in logs
            // var f = AccessTools.Field(typeof(LG_ZoneJob_CreateExpandFromData), "m_markerSubSeed");
            // if (f != null) f.SetValue(__instance, overrideSeed);
            // Debug.Log($"[Reroll] Applied override m_markerSubSeed={overrideSeed} to {zone.name}");
        }
    }


    // // Apply the override at zone creation
    // [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), MethodType.Constructor,
    //     new Type[] { typeof(LG_Floor), typeof(Dimension), typeof(LG_Layer), typeof(int), typeof(GameData.ExpeditionZoneData), typeof(LG_Area) })]
    // [HarmonyPostfix]
    // static void LG_ZoneJob_CreateExpandFromData_Ctor_Postfix(LG_ZoneJob_CreateExpandFromData __instance)
    // {
    //     try
    //     {
    //         // var zone = __instance.m_zone; // public in decomp; use AccessTools if needed
    //         //
    //         // if (zone == null)
    //         //     return;
    //
    //         // var key = (zone.DimensionIndex, zone.LocalIndex);
    //
    //         // if (markerSubSeeds.TryGetValue(key, out var overrideSeed))
    //         // {
    //         //     zone.m_markerSubSeed = overrideSeed;
    //         //     __instance.m_markerSubSeed = overrideSeed;
    //         //
    //         //     Plugin.Logger.LogDebug($"[Reroll] Applied override m_markerSubSeed={overrideSeed} to Zone_{zone.LocalIndex} (dim:{zone.DimensionIndex})");
    //         //
    //         //     // // Align the job’s internal field for consistency in logs
    //         //     // var f = AccessTools.Field(typeof(LG_ZoneJob_CreateExpandFromData), "m_markerSubSeed");
    //         //     // if (f != null) f.SetValue(__instance, overrideSeed);
    //         //     // Debug.Log($"[Reroll] Applied override m_markerSubSeed={overrideSeed} to {zone.name}");
    //         // }
    //
    //
    //         // else
    //         // {
    //         //     // Seed our map from data default on first encounter
    //         //     markerSubSeeds[key] = zone.m_markerSubSeed;
    //         // }
    //     }
    //     catch (Exception ex)
    //     {
    //         Plugin.Logger.LogError(ex);
    //     }
    // }

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

            // We don't care about the snatcher dimensions
            if (zone.DimensionIndex == eDimensionIndex.Dimension_20 ||
                zone.DimensionIndex == eDimensionIndex.Dimension_19 ||
                zone.DimensionIndex == eDimensionIndex.Dimension_18 ||
                zone.DimensionIndex == eDimensionIndex.Dimension_17)
                return;

            if (cluster == null || cluster.m_reachableNodes.Count > 1)
                return;

            var key = (zone.DimensionIndex, zone.LocalIndex);

            if (!s_targetsDetected.Contains(key))
            {
                s_targetsDetected.Add(key);

                shouldSuppressFactoryDone = true;

                // Initialize override baseline if not present
                if (!markerSubSeeds.ContainsKey(key))
                    markerSubSeeds[key] = zone.m_markerSubSeed;

                if (!zoneAttempts.ContainsKey(key))
                    zoneAttempts[key] = 0;

                Plugin.Logger.LogDebug($"[Reroll] Detected unhealthy zone {key}. Will reroll after factory completion.");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex);
        }
    }











    // public static bool TryBegin(LG_Zone zone)
    // {
    //     if (zone == null)
    //         return false;
    //
    //     var key = zone.IDinLayer;
    //
    //     if (zone.m_markerSubSeed >= 6000)
    //         return false;
    //
    //     if (!s_building.Add(key))
    //         return false;
    //
    //     return true;
    // }

    // public static void End(LG_Zone zone)
    // {
    //     if (zone == null)
    //         return;
    //
    //     s_building.Remove(zone.IDinLayer);
    //
    //     Plugin.Logger.LogDebug($"Finished fixing ZoneId={zone.IDinLayer}");
    // }

    // // Populate NodeVolumeJobDatas by preparing volumes in the dimension
    // private static void PrepareVolumesForDimension(eDimensionIndex dimIndex)
    // {
    //     var volumes = UnityEngine.Object.FindObjectsOfType<AIGraph.AIG_NodeVolume>();
    //     for (int i = 0; i < volumes.Length; i++)
    //     {
    //         var v = volumes[i];
    //         v.PrepareBuild(dimIndex);
    //         v.StartBuild();
    //     }
    //     for (int i = 0; i < volumes.Length; i++)
    //         volumes[i].EndBuild();
    // }

    // [HarmonyPatch(typeof(LG_NodeTools), nameof(LG_NodeTools.CalculateReachableNodesAndLocationScores))]
    // [HarmonyPostfix]
    // static void TryGetScoreNodes_Prefix(LG_Area area, Vector3 sourcePos)
    // {
    //     var cluster = area?.m_courseNode?.m_nodeCluster;
    //
    //     if (cluster == null)
    //         return;
    //
    //     if (cluster.m_reachableNodes.Count > 1)
    //         return;
    //
    //     var zone = area.m_zone;
    //
    //     // We know that the following work:
    //     //      - 47
    //     //      - 5437
    //
    //
    //     var maxAttempts = 50;
    //     var respawnMarkers = true;
    //
    //     if (TryBegin(zone))
    //     {
    //         try
    //         {
    //             var floor = Builder.CurrentFloor;
    //             if (floor == null) return;
    //
    //             for (int attempt = 0; attempt < maxAttempts; attempt++)
    //             {
    //                 zone.m_markerSubSeed += 1;
    //
    //                 // A) Optional: respawn markers so m_markerSubSeed takes effect on content
    //                 if (respawnMarkers)
    //                 {
    //                     LevelGeneration.LG_MarkerFactory.Cleanup();
    //
    //                     foreach (var cn in zone.m_courseNodes)
    //                     {
    //                         var area2 = cn?.m_area;
    //                         if (area2 == null) continue;
    //
    //                         // 1) Destroy old spawns
    //                         for (int f = 0; f < EnumUtil.GetValueLength<ExpeditionFunction>(); f++)
    //                         {
    //                             var fn = (ExpeditionFunction)f;
    //                             var spawners = area2.GetAllMarkerSpawners(fn);
    //                             for (int i = 0; i < spawners.Count; i++)
    //                                 spawners[i].DestroySpawnedGO();
    //                         }
    //
    //                         // 2) Recreate spawners deterministically
    //                         area2.CreateMarkerSpawners(area2.AreaSeed);
    //
    //                         // 3) Queue generic markers (None) for production
    //                         LevelGeneration.LG_MarkerFactory.Register(area2.GetAllMarkerSpawners(ExpeditionFunction.None));
    //                     }
    //
    //                     // Drain generic production synchronously
    //                     var produce = new LevelGeneration.LG_ProduceMarkersJob();
    //                     int safetyProduce = 0;
    //                     while (!produce.Build() && safetyProduce++ < 10000) {}
    //
    //                     // Rebuild function markers for the zone (uses zone.m_markerSubSeed)
    //                     var jobFunc = new LevelGeneration.LG_PopulateFunctionMarkersInZoneJob(zone, floor, fallbackMode: false);
    //                     int safetyFunc = 0;
    //                     while (!jobFunc.Build() && safetyFunc++ < 10000) {}
    //                 }
    //
    //                 // B) Prepare NodeVolume jobs BEFORE LG_BuildNodeVolumes
    //                 PrepareVolumesForDimension(zone.DimensionIndex);
    //
    //                 // C) Rebuild per-floor node volumes
    //                 var buildVolumes = new AIGraph.LG_BuildNodeVolumes(floor);
    //                 int safety = 0;
    //                 while (!buildVolumes.Build() && safety++ < 10000) {}
    //
    //                 // D) Rebuild clusters for this zone’s course nodes
    //                 var dim = zone.Dimension;
    //                 if (dim?.CourseGraph?.m_nodes != null)
    //                 {
    //                     foreach (var cn in dim.CourseGraph.m_nodes)
    //                     {
    //                         if (cn == null || cn.m_zone != zone || cn.m_area == null) continue;
    //                         var buildCluster = new AIGraph.LG_BuildNodeCluster(cn, cn.m_area.GraphSource);
    //                         safety = 0;
    //                         while (!buildCluster.Build() && safety++ < 10000) {}
    //                     }
    //                 }
    //
    //                 // E) Recompute reachables for all areas (suppress our Postfix)
    //                 s_suppressHook = true;
    //                 try
    //                 {
    //                     foreach (var cn in zone.m_courseNodes)
    //                     {
    //                         var a = cn?.m_area;
    //                         if (a == null) continue;
    //                         LevelGeneration.LG_NodeTools.CalculateReachableNodesAndLocationScores(a, a.GraphSource.Position);
    //                     }
    //                 }
    //                 finally { s_suppressHook = false; }
    //
    //                 // F) Health check: all areas must have >1 reachable node
    //                 bool healthy = true;
    //                 foreach (var cn in zone.m_courseNodes)
    //                 {
    //                     var cl = cn?.m_nodeCluster;
    //                     if (cl == null || cl.m_reachableNodes == null || cl.m_reachableNodes.Count <= 1)
    //                     {
    //                         healthy = false;
    //                         break;
    //                     }
    //                 }
    //
    //                 if (healthy)
    //                 {
    //                     // G) Re-link gates touching the zone
    //                     foreach (var cn in zone.m_courseNodes)
    //                     {
    //                         foreach (var portal in cn.m_portals)
    //                         {
    //                             var gate = portal?.Gate;
    //                             if (gate != null)
    //                             {
    //                                 gate.ConnectGraph();
    //                                 gate.IsTraversable = gate.m_isTraversableFromStart;
    //                             }
    //                         }
    //                     }
    //                     var sg = zone.m_sourceGate;
    //                     if (sg != null)
    //                     {
    //                         sg.ConnectGraph();
    //                         sg.IsTraversable = sg.m_isTraversableFromStart;
    //                     }
    //
    //                     Plugin.Logger.LogDebug($"[Reroll] Zone {zone.IDinLayer} healthy after {attempt + 1} attempts. m_markerSubSeed={zone.m_markerSubSeed}");
    //                     return;
    //                 }
    //             }
    //
    //             Plugin.Logger.LogError($"[Reroll] Zone {zone.IDinLayer} failed after {maxAttempts} attempts. Last m_markerSubSeed={zone.m_markerSubSeed}");
    //         }
    //         finally { End(zone); }
    //     }
    // }


    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(LG_Gate), nameof(LG_Gate.ProgressionSourceDirection), MethodType.Getter)]
    // static void ProgressionSourceDirection_Postfix(LG_Gate __instance)
    // {
    //     // Heuristic: if m_hasProgressionSourceDirection is true but the direction equals forward
    //     // and TryFindProgressionDirToArea returned false (the code keeps forward), the error was logged.
    //     // We can’t read the local result here, so use a simple check on area/cluster reachables.
    //     var area = __instance?.ProgressionSourceArea;
    //     if (area?.m_courseNode?.m_nodeCluster == null)
    //         return;
    //
    //     if (area.m_courseNode.m_nodeCluster.m_reachableNodes.Count <= 1)
    //     {
    //         var zone = area.m_zone; // adjust if accessor differs
    //         var markerSubSeed = area.m_zone.m_markerSubSeed;
    //
    //         // if (RerollState.TryBegin(zone))
    //         if (markerSubSeed < 20)
    //         {
    //             zone.m_markerSubSeed += 1;
    //             Plugin.Logger.LogDebug($"Rerolling MarkerSubSeed for Zone {area.m_zone.LocalIndex} - MarkerSubSeed = {zone.m_markerSubSeed}");
    //         }
    //     }
    // }
}
