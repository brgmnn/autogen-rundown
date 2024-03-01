//using GameData;
//using HarmonyLib;

namespace AutogenRundown.Patches
{
    /*
    [HarmonyPatch(typeof(BoosterImplants.ArtifactInventory), nameof(BoosterImplants.ArtifactInventory.GetArtifactCount))]
    public class ArtifactFarm
    {
        public static void Postfix(ref int __result, ArtifactCategory category)
        {
            Plugin.Logger.LogInfo($"CM_ArtifactCounter.SetCounterValues() called with: {category}");

            // 20 Booster slots, 0.15 progres per artifact
            // -> 134 artifacts needed to max out boosters
            __result = 134;
        }
    }
    */
}
