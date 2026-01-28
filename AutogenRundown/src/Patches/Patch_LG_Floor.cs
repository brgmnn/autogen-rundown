using HarmonyLib;
using LevelGeneration;
using UnityEngine;

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
    /// <param name="dimensionIndex"></param>
    /// <param name="arenaDimension"></param>
    /// <param name="position"></param>
    [HarmonyPatch(typeof(LG_Floor), nameof(LG_Floor.CreateDimension))]
    [HarmonyPrefix]
    static void CreateDimension_Prefix(eDimensionIndex dimensionIndex, bool arenaDimension, ref Vector3 position)
    {
        if (arenaDimension)
        {
            position += new Vector3 { y = -500f };
        }
    }
}
