using AutogenRundown.Managers;
using GameData;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Fix_SourceExpanderNull
{
    /// <summary>
    /// Catches zones that reach Done state without a sourceExpander and triggers a reroll.
    /// This prevents the NullReferenceException that would otherwise crash the build.
    /// </summary>
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyPrefix]
    public static bool Pre_Build(LG_ZoneJob_CreateExpandFromData __instance, ref bool __result)
    {
        // Only intercept when transitioning to Done state
        if (__instance.m_mainStatus != LG_ZoneJob_CreateExpandFromData.MainStatus.Done)
            return true;

        var zone = __instance.m_zone;
        if (zone == null)
            return true;

        // Check if sourceExpander is null - this would cause a crash
        if (zone.m_sourceExpander == null)
        {
            Plugin.Logger.LogWarning(
                $"Zone {zone.LocalIndex} reached Done state without sourceExpander. " +
                $"Triggering subseed reroll.");

            // Trigger a reroll for this zone
            ZoneSeedManager.Reroll_SubSeed(zone);

            // Also reroll parent zone if applicable
            if (zone.LocalIndex != eLocalZoneIndex.Zone_0 && __instance.m_zoneData != null)
            {
                var parentIndex = __instance.m_zoneData.BuildFromLocalIndex;
                ZoneSeedManager.Reroll_SubSeed(parentIndex, zone.DimensionIndex, zone.Layer.m_type);
                Plugin.Logger.LogDebug($"Also rerolling parent zone {parentIndex}");
            }

            // Return false to skip the original Build() which would crash
            // The factory will detect the rebuild flag and restart
            __result = false;
            return false;
        }

        return true;
    }
}
