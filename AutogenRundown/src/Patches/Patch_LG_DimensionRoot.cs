using HarmonyLib;
using UnityEngine;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_LG_DimensionRoot
{
    private static LG_DimensionRoot? dimension17 = null;
    private static LG_DimensionRoot? dimension18 = null;
    private static LG_DimensionRoot? dimension19 = null;
    private static LG_DimensionRoot? dimension20 = null;


    [HarmonyPatch(typeof(LG_DimensionRoot), nameof(LG_DimensionRoot.Setup))]
    [HarmonyPostfix]
    public static void Setup(LG_DimensionRoot __instance, eDimensionIndex dimensionIndex)
    {
        if (dimensionIndex is eDimensionIndex.Dimension_17)
            dimension17 = __instance;
        if (dimensionIndex is eDimensionIndex.Dimension_18)
            dimension18 = __instance;
        if (dimensionIndex is eDimensionIndex.Dimension_19)
            dimension19 = __instance;
        if (dimensionIndex is eDimensionIndex.Dimension_20)
            dimension20 = __instance;


        // if (dimensionIndex is
        //     eDimensionIndex.Dimension_17 or
        //     eDimensionIndex.Dimension_18 or
        //     eDimensionIndex.Dimension_19 or
        //     eDimensionIndex.Dimension_20)
        // {
        //     // __instance.transform.localPosition += new Vector3 { y = -100f };
        //     __instance.transform.localPosition += new Vector3 { y = -400f };
        //
        //     Plugin.Logger.LogDebug($"Moved the snatcher zone");
        // }
        // else
        // {
        //     Plugin.Logger.LogDebug($"Nope: eDimensionIndex = {dimensionIndex}");
        // }
    }

    public static void OnLevelDone()
    {
        if (dimension17 != null)
            dimension17.transform.localPosition += new Vector3 { y = -400f };
        if (dimension18 != null)
            dimension18.transform.localPosition += new Vector3 { y = -400f };
        if (dimension19 != null)
            dimension19.transform.localPosition += new Vector3 { y = -400f };
        if (dimension20 != null)
            dimension20.transform.localPosition += new Vector3 { y = -400f };
    }
}
