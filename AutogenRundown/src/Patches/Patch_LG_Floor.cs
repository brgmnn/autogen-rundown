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
    /// This patch adds the LG_FloorTransition component at runtime if it's missing,
    /// allowing any geomorph to be used as a dimension origin.
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
        var comp = spawned.GetComponent<LG_FloorTransition>();

        if (comp == null)
        {
            // The geomorph has LG_Geomorph but not LG_FloorTransition. Destroy the
            // original so we don't end up with two LG_Geomorph-derived components —
            // that causes GetComponent<LG_Geomorph>() to find the wrong (uninitialised)
            // one, breaking gate dimension linking, culling, and node volumes.
            var existingGeo = spawned.GetComponent<LG_Geomorph>();
            if (existingGeo != null)
                UnityEngine.Object.DestroyImmediate(existingGeo);

            comp = spawned.AddComponent<LG_FloorTransition>();
            comp.m_transitionType = LG_FloorTransitionType.Elevator;
        }

        comp.m_geoPrefab = transitionOverridePrefab;
        comp.SetupAreas(xxHash.NextSubSeed());

        if (!isStatic)
            comp.SetPlaced();

        __result = comp;
        return false;
    }
}
