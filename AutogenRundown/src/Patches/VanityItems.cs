using CellMenu;
using HarmonyLib;

namespace AutogenRundown.Patches;

/// <summary>
/// Hides the vanity / cosmetics items widgets on the rundown pages
/// </summary>
[HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnExpeditionUpdated))]
[HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.UpdateVanityItemUnlocks))]
public class VanityItems
{
    [HarmonyAfter]
    public static void Postfix(CM_PageRundown_New __instance)
    {
        __instance.gameObject.transform.FindChild("MovingContent/Rundown/Button VanityItemDrops").transform
            .localPosition = UnityEngine.Vector3.up * 1000f;
        __instance.gameObject.transform.FindChild("MovingContent/Rundown/CM_PageRundown_VanityItemDropsNext").transform
            .localPosition = UnityEngine.Vector3.up * 2000f;
    }
}
