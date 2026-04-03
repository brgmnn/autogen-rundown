using AIGraph;
using HarmonyLib;
using LevelGeneration;
using Player;
using UnityEngine;
using XXHashing;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_LG_Floor
{
    // Track dimensions that used our patched CreateFloorTransition (non-elevator origin).
    // Only these need the force-enable renderer workaround on warp.
    private static readonly HashSet<eDimensionIndex> _patchedDimensions = new();

    /// <summary>
    /// This just moves the pouncer dimensions down to be below the reality dimension.
    ///
    /// Really this is only for levels that make use of other dimensions. If no changes
    /// are made the pouncer dimensions are visible on many of those levels as floating
    /// black boxes in the sky.
    /// </summary>
    [HarmonyPatch(typeof(LG_Floor), nameof(LG_Floor.CreateDimension))]
    [HarmonyPrefix]
    static void CreateDimension_Prefix(eDimensionIndex dimensionIndex, bool arenaDimension, ref Vector3 position)
    {
        if (arenaDimension)
        {
            position += new Vector3 { y = -500f };
        }
    }

    /// <summary>
    /// After CreateDimension, if the origin tile is a regular geomorph (not an elevator
    /// shaft), its LG_PrefabSpawners haven't been registered with the factory pipeline.
    /// Regular geomorphs have 0 baked MeshRenderers — all visual content comes from
    /// PrefabSpawners that must be built during level gen. Elevator shafts have their
    /// geometry baked directly and don't need this.
    /// </summary>
    [HarmonyPatch(typeof(LG_Floor), nameof(LG_Floor.CreateDimension))]
    [HarmonyPostfix]
    static void CreateDimension_Postfix(LG_Floor __instance, uint seed, eDimensionIndex dimensionIndex, bool arenaDimension, int __result)
    {
        if (arenaDimension) return;

        if (!__instance.GetDimension(dimensionIndex, out var dim)) return;
        var ft = dim.m_startTransition;
        if (ft == null) return;

        // If the origin tile has no baked MeshRenderers, it needs its PrefabSpawners built.
        // This is the case for regular zone geomorphs used as dimension origins.
        var renderers = ft.GetComponentsInChildren<MeshRenderer>(true);
        if (renderers.Length == 0)
        {
            Plugin.Logger.LogDebug($"[DimDebug] {dimensionIndex}: origin tile has 0 MeshRenderers, building PrefabSpawners immediately");
            LG_Factory.FindAndBuildSelectorsAndSpawners(ft.gameObject, seed, buildSpawnersInstantly: true);
        }


        var go = ft.gameObject;
        Plugin.Logger.LogDebug($"[DimDebug] === {dimensionIndex} origin tile: {go.name} ===");
        Plugin.Logger.LogDebug($"[DimDebug]   position: {go.transform.position}");
        Plugin.Logger.LogDebug($"[DimDebug]   parent: {go.transform.parent?.name ?? "NULL"}");
        Plugin.Logger.LogDebug($"[DimDebug]   isStatic: {go.isStatic}");
        Plugin.Logger.LogDebug($"[DimDebug]   layer: {go.layer} ({LayerMask.LayerToName(go.layer)})");
        Plugin.Logger.LogDebug($"[DimDebug]   activeInHierarchy: {go.activeInHierarchy}");

        // Components on root
        var allComps = go.GetComponents<Component>();
        Plugin.Logger.LogDebug($"[DimDebug]   root components ({allComps.Length}):");
        foreach (var c in allComps)
            Plugin.Logger.LogDebug($"[DimDebug]     - {c?.GetType().Name ?? "NULL"} (enabled: {(c is Behaviour b ? b.enabled.ToString() : "n/a")})");

        // LG_FloorTransition state
        Plugin.Logger.LogDebug($"[DimDebug]   FT.m_transitionType: {ft.m_transitionType}");
        Plugin.Logger.LogDebug($"[DimDebug]   FT.m_shapeType: {ft.m_shapeType}");
        Plugin.Logger.LogDebug($"[DimDebug]   FT.m_placed: {ft.m_placed}");
        Plugin.Logger.LogDebug($"[DimDebug]   FT.m_geoPrefab: {ft.m_geoPrefab?.name ?? "NULL"}");
        Plugin.Logger.LogDebug($"[DimDebug]   FT.m_spawnPoints: {ft.m_spawnPoints?.Length ?? -1}");
        Plugin.Logger.LogDebug($"[DimDebug]   FT.m_areas: {ft.m_areas?.Length ?? -1}");
        Plugin.Logger.LogDebug($"[DimDebug]   FT.m_plugs: {ft.m_plugs?.Count ?? -1}");
        Plugin.Logger.LogDebug($"[DimDebug]   FT.m_nodeVolume: {(ft.m_nodeVolume != null ? "SET" : "NULL")}");
        Plugin.Logger.LogDebug($"[DimDebug]   FT.m_tile: {(ft.m_tile != null ? "SET" : "NULL")}");
        Plugin.Logger.LogDebug($"[DimDebug]   FT.m_zone: {(ft.m_zone != null ? ft.m_zone.LocalIndex.ToString() : "NULL")}");

        // Check for duplicate LG_Geomorph
        var geos = go.GetComponents<LG_Geomorph>();
        Plugin.Logger.LogDebug($"[DimDebug]   LG_Geomorph count: {geos.Length}");
        for (int i = 0; i < geos.Length; i++)
        {
            var g = geos[i];
            Plugin.Logger.LogDebug($"[DimDebug]   geo[{i}] type={g.GetType().Name} placed={g.m_placed} areas={g.m_areas?.Length ?? -1} plugs={g.m_plugs?.Count ?? -1} nodeVol={(g.m_nodeVolume != null ? "SET" : "NULL")}");
        }

        // AIG_GeomorphNodeVolume
        var vol = go.GetComponent<AIG_GeomorphNodeVolume>();
        if (vol != null)
        {
            Plugin.Logger.LogDebug($"[DimDebug]   NodeVolume: pos={vol.Position} size={vol.Size}");
            Plugin.Logger.LogDebug($"[DimDebug]   NodeVolume.m_voxelNodeVolume: {(vol.m_voxelNodeVolume != null ? "SET" : "NULL")}");
        }
        else
        {
            Plugin.Logger.LogDebug($"[DimDebug]   NodeVolume: MISSING!");
        }

        // LG_Dimension
        var lgDim = go.GetComponent<LG_Dimension>();
        Plugin.Logger.LogDebug($"[DimDebug]   LG_Dimension: {(lgDim != null ? "present" : "MISSING")}");

        // Areas detail
        if (ft.m_areas != null)
        {
            for (int i = 0; i < ft.m_areas.Length; i++)
            {
                var area = ft.m_areas[i];
                Plugin.Logger.LogDebug($"[DimDebug]   area[{i}]: {area?.name ?? "NULL"} gates={area?.m_gates?.Count ?? -1}");
            }
        }

        // Plugs detail
        if (ft.m_plugs != null)
        {
            for (int i = 0; i < ft.m_plugs.Count; i++)
            {
                var plug = ft.m_plugs[i];
                Plugin.Logger.LogDebug($"[DimDebug]   plug[{i}]: paired={plug.m_pariedWith != null} pos={plug.transform.position}");
            }
        }

        // MeshRenderers
        var dbgRenderers = go.GetComponentsInChildren<MeshRenderer>();
        int enabledCount = 0, disabledCount = 0, staticCount = 0;
        foreach (var r in dbgRenderers)
        {
            if (r.enabled) enabledCount++; else disabledCount++;
            if (r.gameObject.isStatic) staticCount++;
        }
        Plugin.Logger.LogDebug($"[DimDebug]   MeshRenderers: total={dbgRenderers.Length} enabled={enabledCount} disabled={disabledCount} static={staticCount}");

        // Dimension root state
        var root = dim.DimensionRootTemp;
        if (root != null)
        {
            Plugin.Logger.LogDebug($"[DimDebug]   DimensionRoot: {root.gameObject.name} linkedDim={root.LinkedDimensionIndex}");
            var childRenderers = root.GetComponentsInChildren<MeshRenderer>();
            Plugin.Logger.LogDebug($"[DimDebug]   DimensionRoot total MeshRenderers: {childRenderers.Length}");
        }

        Plugin.Logger.LogDebug($"[DimDebug] === end {dimensionIndex} ===");
    }

    /// <summary>
    /// The vanilla CreateFloorTransition crashes with a NullReferenceException when the
    /// geomorph prefab doesn't have an LG_FloorTransition component. This happens when
    /// using regular zone geomorphs as the DimensionGeomorph for generated dimensions.
    ///
    /// This patch only intercepts when the prefab lacks LG_FloorTransition, adding the
    /// component at runtime. Prefabs that already have it (elevator shafts, dimension
    /// prefabs) pass through to the original method untouched.
    /// </summary>
    [HarmonyPatch(typeof(LG_Floor), "CreateFloorTransition")]
    [HarmonyPrefix]
    static bool CreateFloorTransition_Prefix(
        ref LG_FloorTransition __result,
        uint seed,
        Dimension dimension,
        Vector3 pos,
        Quaternion rotation,
        GameObject transitionOverridePrefab,
        bool isStatic)
    {
        // Resolve the prefab the same way the original does
        if (transitionOverridePrefab == null)
        {
            var tempHash = new XXHashSequence(seed);
            tempHash.NextSubSeed(); // consume the same seed the original would
            transitionOverridePrefab = Builder.ComplexResourceSetBlock.GetElevatorTile(tempHash.NextSubSeed());
        }

        // If the prefab already has LG_FloorTransition, let the original method handle it
        if (transitionOverridePrefab == null ||
            transitionOverridePrefab.GetComponent<LG_FloorTransition>() != null)
            return true;

        // --- Custom handling for geomorphs without LG_FloorTransition ---
        // Track this dimension so the TryWarpTo postfix knows to force-enable renderers.
        if (dimension != null)
            _patchedDimensions.Add(dimension.DimensionIndex);

        var spawned = UnityEngine.Object.Instantiate(transitionOverridePrefab, pos, rotation);

        // Destroy the original LG_Geomorph so there's only one LG_Geomorph-derived
        // component. With two, GetComponent<LG_Geomorph>() returns the original
        // (with placed=false, nodeVol=null) causing the culling system to skip the tile.
        // This is safe because the prefab has 0 baked MeshRenderers — all visual content
        // comes from PrefabSpawners that are built later in the postfix.
        var existingGeo = spawned.GetComponent<LG_Geomorph>();
        if (existingGeo != null)
            UnityEngine.Object.DestroyImmediate(existingGeo);

        var comp = spawned.AddComponent<LG_FloorTransition>();
        comp.m_transitionType = LG_FloorTransitionType.Node;

        // Consume the same seeds the original method would
        var xxHash = new XXHashSequence(seed);
        xxHash.NextSubSeed();

        comp.m_geoPrefab = transitionOverridePrefab;
        comp.SetupAreas(xxHash.NextSubSeed());

        if (!isStatic)
            comp.SetPlaced();

        __result = comp;
        return false;
    }

    /// <summary>
    /// When warping to a non-Reality dimension, the C_MovingCuller occlusion system
    /// may not recognize the origin tile's rooms — causing a black void even though
    /// geometry exists. Force-enable all MeshRenderers in the target dimension after
    /// warp so the player can see the environment. The culler takes over naturally
    /// as the player moves between rooms.
    /// </summary>
    [HarmonyPatch(typeof(PlayerAgent), nameof(PlayerAgent.TryWarpTo),
        new[] { typeof(eDimensionIndex), typeof(Vector3), typeof(Vector3), typeof(bool) })]
    [HarmonyPostfix]
    static void TryWarpTo_Postfix(bool __result, eDimensionIndex dimensionIndex)
    {
        // Only force-enable for dimensions that used our patched CreateFloorTransition.
        // Elevator shaft dimensions handle culling correctly on their own.
        if (!__result || !_patchedDimensions.Contains(dimensionIndex)) return;

        Dimension dim;
        if (!Dimension.GetDimension(dimensionIndex, out dim)) return;

        var root = dim.DimensionRootTemp;
        if (root == null) return;

        // Force-enable MeshRenderers — the culling system culls them on the origin tile.
        int enabledRenderers = 0;
        var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var r in renderers)
        {
            if (r.sharedMaterial == null) continue;
            r.enabled = true;
            enabledRenderers++;
        }

        // Force the player's C_MovingCuller to detect a node in a generated zone
        // (not the origin tile). This triggers the full culling pipeline including
        // light source nodes, beams, bloom, and volumetrics — same as walking through
        // a door. Without this, only raw Light components are on but not properly
        // integrated with the C_Light system.
        var player = PlayerManager.GetLocalPlayerAgent();
        if (player != null && dim.Layers.Count > 0)
        {
            // Find a valid cull node in any generated zone
            for (int i = 0; i < dim.Layers.Count && i < 1; i++)
            {
                var layer = dim.Layers[i];
                for (int j = 0; j < layer.m_zones.Count; j++)
                {
                    var zone = layer.m_zones[j];
                    for (int k = 0; k < zone.m_courseNodes.Count; k++)
                    {
                        var cn = zone.m_courseNodes[k];
                        if (cn?.m_cullNode != null)
                        {
                            player.m_movingCuller.SetCurrentNode(cn.m_cullNode);
                            Plugin.Logger.LogDebug($"[DimWarp] Forced culler to node in {zone.LocalIndex} of {dimensionIndex}");
                            goto cullerDone;
                        }
                    }
                }
            }
        }
        cullerDone:

        Plugin.Logger.LogDebug($"[DimWarp] Force-enabled {enabledRenderers}/{renderers.Length} renderers in {dimensionIndex}");
    }

    /// <summary>
    /// The game places a bulkhead gate on the first zone of each layer (IsLayerSource).
    /// For non-Reality dimensions this is wrong — the origin tile connects to zone 0
    /// which IS the first zone, but there's no bulkhead controller to go with it.
    /// Skip bulkhead gate placement for non-Reality dimensions.
    /// </summary>
    [HarmonyPatch(typeof(LG_ZoneExpander), nameof(LG_ZoneExpander.IsLayerSource), new Type[0])]
    [HarmonyPrefix]
    static bool IsLayerSource_Prefix(LG_ZoneExpander __instance, ref bool __result)
    {
        // Only intercept for non-Reality dimensions
        if (__instance.m_linksTo?.m_zone != null
            && __instance.m_linksTo.m_zone.DimensionIndex != eDimensionIndex.Reality)
        {
            __result = false;
            return false;
        }
        return true;
    }
}
