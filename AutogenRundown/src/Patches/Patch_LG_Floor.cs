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
        var xxHash = new XXHashSequence(seed);

        if ((dimension == null || dimension.IsMainDimension)
            && Builder.LayerBuildDatas[0].m_zoneBuildDatas != null
            && Builder.LayerBuildDatas[0].m_zoneBuildDatas.Count > 0)
        {
            _ = (int)Builder.LayerBuildDatas[0].m_zoneBuildDatas[0].SubComplex;
        }

        var seed1 = xxHash.NextSubSeed();

        if (transitionOverridePrefab == null)
            transitionOverridePrefab = Builder.ComplexResourceSetBlock.GetElevatorTile(seed1);
        if (transitionOverridePrefab == null)
            throw new System.Exception("ERROR : No start tile found in LG_SetupFloor!");

        var spawned = UnityEngine.Object.Instantiate(transitionOverridePrefab, pos, rotation);
        var comp = spawned.AddComponent<LG_FloorTransition>();
        comp.m_transitionType = LG_FloorTransitionType.Elevator;

        // Regular geomorphs don't have spawn points. Create one at the geomorph's
        // center so dimension warps have a valid destination.
        var spawnPoint = new GameObject("DimensionSpawnPoint_0").transform;
        spawnPoint.SetParent(spawned.transform);
        spawnPoint.localPosition = Vector3.zero;
        spawnPoint.localRotation = Quaternion.identity;
        comp.m_spawnPoints = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Transform>(new[] { spawnPoint });

        comp.m_geoPrefab = transitionOverridePrefab;
        comp.SetupAreas(xxHash.NextSubSeed());

        if (!isStatic)
            comp.SetPlaced();

        __result = comp;
        return false;
    }
}
