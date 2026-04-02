using AIGraph;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;
using XXHashing;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_LG_Floor
{
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
    /// Debug: dump the full state of a dimension's origin tile after CreateDimension.
    /// Compare output between elevator shaft (works) and regular geomorph (void).
    /// </summary>
    [HarmonyPatch(typeof(LG_Floor), nameof(LG_Floor.CreateDimension))]
    [HarmonyPostfix]
    static void CreateDimension_Postfix(LG_Floor __instance, eDimensionIndex dimensionIndex, bool arenaDimension, int __result)
    {
        if (arenaDimension) return;

        if (!__instance.GetDimension(dimensionIndex, out var dim)) return;
        var ft = dim.m_startTransition;
        if (ft == null) { Plugin.Logger.LogError($"[DimDebug] {dimensionIndex}: m_startTransition is NULL"); return; }

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
        var renderers = go.GetComponentsInChildren<MeshRenderer>();
        int enabledCount = 0, disabledCount = 0, staticCount = 0;
        foreach (var r in renderers)
        {
            if (r.enabled) enabledCount++; else disabledCount++;
            if (r.gameObject.isStatic) staticCount++;
        }
        Plugin.Logger.LogDebug($"[DimDebug]   MeshRenderers: total={renderers.Length} enabled={enabledCount} disabled={disabledCount} static={staticCount}");

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
        var spawned = UnityEngine.Object.Instantiate(transitionOverridePrefab, pos, rotation);

        var comp = spawned.AddComponent<LG_FloorTransition>();
        comp.m_transitionType = LG_FloorTransitionType.Elevator;

        // Don't set m_spawnPoints — without them, GetValidDimensionWarpPoint()
        // falls through to the CourseGraph fallback which picks the first valid
        // course node. This lands the player in a generated zone where culling
        // is properly initialized, avoiding the black void on the origin tile.

        // Consume the same seeds the original method would
        var xxHash = new XXHashSequence(seed);
        xxHash.NextSubSeed();

        comp.m_geoPrefab = transitionOverridePrefab;
        comp.SetupAreas(xxHash.NextSubSeed());

        // Sync areas/plugs to the original LG_Geomorph so that code using
        // GetComponent<LG_Geomorph>() (which finds the original first) still
        // gets valid area data for culling and node traversal.
        var originalGeo = spawned.GetComponent<LG_Geomorph>();
        if (originalGeo != null && originalGeo != comp)
        {
            originalGeo.m_areas = comp.m_areas;
            originalGeo.m_plugs = comp.m_plugs;
            originalGeo.m_geoPrefab = transitionOverridePrefab;
        }

        if (!isStatic)
            comp.SetPlaced();

        __result = comp;
        return false;
    }
}
