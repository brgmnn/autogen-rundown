using AutogenRundown.Managers;
using AutogenRundown.Utils;
using GameData;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Fix_FailedToFindStartArea
{
    public static readonly Dictionary<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index), uint> zoneFailures = new();

    /// <summary>
    /// Catches zones that fail to find valid start areas and triggers a proper subseed reroll.
    /// The reroll is persisted via ZoneSeedManager so it survives level rebuilds.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__exception"></param>
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyFinalizer]
    public static void Post_LG_ZoneJob_CreateExpandFromData_Build(LG_ZoneJob_CreateExpandFromData __instance, ref Exception? __exception)
    {
        if (__instance.m_mainStatus == LG_ZoneJob_CreateExpandFromData.MainStatus.FindStartArea &&
            __instance.m_subStatus == LG_ZoneJob_CreateExpandFromData.SubStatus.SelectArea &&
            __instance.m_scoredStartAreas.Count < 1)
        {
            var zone = __instance.m_zone;
            var zoneKey = (zone.m_dimensionIndex, zone.m_layer.m_type, zone.LocalIndex);

            var rollCount = zoneFailures.GetValueOrDefault(zoneKey, 0u);
            zoneFailures[zoneKey] = rollCount + 1;

            Plugin.Logger.LogWarning($"Zone {zone.LocalIndex} failed to find start area (attempt {rollCount}). Triggering subseed reroll.");

            // Trigger a proper reroll that persists across rebuilds
            ZoneSeedManager.Reroll_SubSeed(zone);

            // Reroll the entire parent chain - constraints can propagate from any ancestor
            if (zone.LocalIndex != eLocalZoneIndex.Zone_0 && __instance.m_zoneData != null)
            {
                var currentIndex = __instance.m_zoneData.BuildFromLocalIndex;

                while (currentIndex != eLocalZoneIndex.Zone_0)
                {
                    ZoneSeedManager.Reroll_SubSeed(currentIndex, zone.DimensionIndex, zone.Layer.m_type);
                    Plugin.Logger.LogDebug($"Also rerolling ancestor zone {currentIndex}");

                    // Move up to the next parent
                    var currentZone = Game.FindZone(currentIndex, zone.DimensionIndex, zone.Layer.m_type);
                    if (currentZone?.m_settings?.m_zoneData == null)
                        break;

                    currentIndex = currentZone.m_settings.m_zoneData.BuildFromLocalIndex;
                }
            }
        }
    }
}
