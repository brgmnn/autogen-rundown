using AutogenRundown.Managers;
using GameData;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

using MainStatus = LG_ZoneJob_CreateExpandFromData.MainStatus;

[HarmonyPatch]
public class Fix_SourceExpanderNull
{
    /// <summary>
    /// Catches zones that reach Done state without a sourceExpander and triggers immediate rebuild.
    /// Instead of letting the code crash trying to access null sourceExpander, we call FactoryDone()
    /// directly which triggers the existing rebuild pathway via Patch_LG_Factory.
    /// </summary>
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyPrefix]
    public static bool Pre_Build(LG_ZoneJob_CreateExpandFromData __instance, ref bool __result)
    {
        // Only intercept when in Done state
        if (__instance.m_mainStatus != MainStatus.Done)
            return true;

        var zone = __instance.m_zone;
        if (zone == null)
            return true;

        // Check if sourceExpander is null - this would cause a crash in Done handler
        if (zone.m_sourceExpander == null)
        {
            Plugin.Logger.LogWarning(
                $"Zone {zone.LocalIndex} reached Done without sourceExpander. " +
                $"Triggering immediate rebuild...");

            // Register rerolls for next rebuild attempt
            ZoneSeedManager.Reroll_SubSeed(zone);

            // Also reroll parent zone if applicable
            if (zone.LocalIndex != eLocalZoneIndex.Zone_0 && __instance.m_zoneData != null)
            {
                var parentIndex = __instance.m_zoneData.BuildFromLocalIndex;
                ZoneSeedManager.Reroll_SubSeed(parentIndex, zone.DimensionIndex, zone.Layer.m_type);
                Plugin.Logger.LogDebug($"Also rerolling parent zone {parentIndex}");
            }

            // Force immediate rebuild by calling FactoryDone
            // Patch_LG_Factory intercepts this and triggers the rebuild
            FactoryJobManager.MarkForRebuild();
            LG_Factory.Current.FactoryDone();

            // Skip original Build() and return true to signal job "complete"
            __result = true;
            return false;
        }

        return true;
    }
}
