using BoosterImplants;
using GameData;
using HarmonyLib;

namespace AutogenRundown.Patches
{
    [HarmonyPatch(typeof(ArtifactInventory), nameof(ArtifactInventory.GetArtifactCountAndBoosterValue))]
    public class Artifacts
    {
        private const float Heat = 0.15f;
        private static float MutedBaseValue { get; set; } = 0.0f;
        private static float BoldBaseValue { get; set; } = 0.0f;
        private static float AggressiveBaseValue { get; set; } = 0.0f;

        /// <summary>
        /// Patches artifacts to always give 100% heat
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="category"></param>
        /// <param name="prePickup"></param>
        public static void Postfix(
            ref ArtifactCountBoosterValuePair __result,
            ArtifactCategory category,
            bool prePickup = false)
        {
            if (__result.ArtifactCount == 0)
            {
                // Assign the base values when we can measure them.
                switch (category)
                {
                    case ArtifactCategory.Common:
                        MutedBaseValue = __result.BoosterValue;
                        break;
                    case ArtifactCategory.Uncommon:
                        BoldBaseValue = __result.BoosterValue;
                        break;
                    case ArtifactCategory.Rare:
                        AggressiveBaseValue = __result.BoosterValue;
                        break;
                }
            }

            var boosterValue = category switch
            {
                ArtifactCategory.Common =>   MutedBaseValue      + __result.ArtifactCount * Heat,
                ArtifactCategory.Uncommon => BoldBaseValue       + __result.ArtifactCount * Heat,
                ArtifactCategory.Rare =>     AggressiveBaseValue + __result.ArtifactCount * Heat
            };

            Plugin.Logger.LogInfo(
                $"BoosterImplants.ArtifactInventory.GetArtifactCountAndBoosterValue() called with: {category} -- " +
                "{" + $"{__result.ArtifactCount}, {__result.BoosterValue}" + "}" +
                $"prePickup = {prePickup}");
            Plugin.Logger.LogInfo(
                $"BoosterImplants.ArtifactInventory.GetArtifactCountAndBoosterValue() returning: {category} -- " +
                "{" + $"{__result.ArtifactCount}, {boosterValue}" + "}");

            __result = new ArtifactCountBoosterValuePair
            {
                ArtifactCount = __result.ArtifactCount,
                BoosterValue  = boosterValue
            };
        }

        // LOADING
        //[Info: AutogenRundown] BoosterImplants.ArtifactInventory.GetArtifactCountAndBoosterValue() called with: Common -- {0, 0.075}
        //[Info: AutogenRundown] BoosterImplants.ArtifactInventory.GetArtifactCountAndBoosterValue() called with: Uncommon -- {0, 0.804}
        //[Info: AutogenRundown] BoosterImplants.ArtifactInventory.GetArtifactCountAndBoosterValue() called with: Rare -- {0, 0.029}

        // PICKUP MUTED
        //[Info: AutogenRundown] BoosterImplants.ArtifactInventory.GetArtifactCountAndBoosterValue() called with: Common -- {1, 0.105}
        //[Info: AutogenRundown] BoosterImplants.ArtifactInventory.GetArtifactCountAndBoosterValue() called with: Uncommon -- {0, 0.804}
        //[Info: AutogenRundown] BoosterImplants.ArtifactInventory.GetArtifactCountAndBoosterValue() called with: Rare -- {0, 0.029}


        // TODO: Old working version
        /*public static void Postfix(ref int __result, ArtifactCategory category)
        {
            Plugin.Logger.LogInfo($"CM_ArtifactCounter.SetCounterValues() called with: {category}");

            // 20 Booster slots, 0.15 progres per artifact
            // -> 134 artifacts needed to max out boosters
            // -> 1 for regular progress
            __result = 1;
        }*/
    }
}
