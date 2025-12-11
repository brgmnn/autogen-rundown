using AutogenRundown.Managers;
using GameData;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

using MainStatus = LG_ZoneJob_CreateExpandFromData.MainStatus;
using SubStatus = LG_ZoneJob_CreateExpandFromData.SubStatus;

[HarmonyPatch]
public class Fix_SourceExpanderNull
{
    /// <summary>
    /// Catches zones that reach Done state without a sourceExpander and redirects to Failed state.
    /// The Failed state has a fallback mechanism that eventually returns true, allowing the factory
    /// to proceed. This prevents the NullReferenceException that would otherwise crash the build.
    /// </summary>
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyPrefix]
    public static void Pre_Build(LG_ZoneJob_CreateExpandFromData __instance)
    {
        // Only intercept when in Done state
        if (__instance.m_mainStatus != MainStatus.Done)
            return;

        var zone = __instance.m_zone;
        if (zone == null)
            return;

        // Check if sourceExpander is null - this would cause a crash in Done handler
        if (zone.m_sourceExpander == null)
        {
            Plugin.Logger.LogWarning(
                $"Zone {zone.LocalIndex} reached Done state without sourceExpander. " +
                $"Transitioning to Failed state and triggering subseed reroll.");

            // Trigger a reroll for this zone (for next rebuild attempt)
            ZoneSeedManager.Reroll_SubSeed(zone);

            // Also reroll parent zone if applicable
            if (zone.LocalIndex != eLocalZoneIndex.Zone_0 && __instance.m_zoneData != null)
            {
                var parentIndex = __instance.m_zoneData.BuildFromLocalIndex;
                ZoneSeedManager.Reroll_SubSeed(parentIndex, zone.DimensionIndex, zone.Layer.m_type);
                Plugin.Logger.LogDebug($"Also rerolling parent zone {parentIndex}");
            }

            // Transition to Failed state - this has a fallback mechanism that:
            // 1. Relaxes coverage requirements
            // 2. Changes expansion type to Random
            // 3. Eventually returns true so factory can proceed
            __instance.m_mainStatus = MainStatus.Failed;
            __instance.m_subStatus = SubStatus.Init;
        }
    }
}
