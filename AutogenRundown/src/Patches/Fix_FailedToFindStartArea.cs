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
    /// Once per-zone failures exceed this, also bulk-reroll all MainLayer zones in the
    /// dimension to force broader topology variation across rebuilds.
    /// </summary>
    private const uint kBroadenThreshold = 50;

    /// <summary>
    /// Hard ceiling on retries. Beyond this we stop scheduling rerolls and emit a fatal log
    /// rather than spinning forever.
    /// </summary>
    private const uint kFatalThreshold = 500;

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
        if (__instance.m_mainStatus != LG_ZoneJob_CreateExpandFromData.MainStatus.FindStartArea ||
            __instance.m_subStatus != LG_ZoneJob_CreateExpandFromData.SubStatus.SelectArea ||
            __instance.m_scoredStartAreas.Count >= 1)
            return;

        var zone = __instance.m_zone;
        var zoneKey = (zone.m_dimensionIndex, zone.m_layer.m_type, zone.LocalIndex);

        var rollCount = zoneFailures.GetValueOrDefault(zoneKey, 0u);
        zoneFailures[zoneKey] = rollCount + 1;

        Plugin.Logger.LogWarning($"Zone {zone.LocalIndex} failed to find start area (attempt {rollCount}). Triggering subseed reroll.");

        if (rollCount > kFatalThreshold)
        {
            Plugin.Logger.LogError(
                $"Zone {zone.LocalIndex} in {zone.m_layer.m_type} (dim {zone.m_dimensionIndex}) " +
                $"exhausted {kFatalThreshold} reroll attempts — giving up on this seed");
            return;
        }

        // Reroll the failing zone itself
        ZoneSeedManager.Reroll_SubSeed(zone);

        // Walk same-layer ancestors up to (and including) Zone_0
        if (zone.LocalIndex != eLocalZoneIndex.Zone_0 && __instance.m_zoneData != null)
        {
            WalkAncestors(
                __instance.m_zoneData.BuildFromLocalIndex,
                zone.DimensionIndex,
                zone.Layer.m_type);
        }

        // Cross-layer hop: when the failing zone is in a non-Main layer, the same-layer walk
        // terminates at Zone_0 of that layer but the actual source zone lives in another layer
        // (Main, or Extreme for chained Overload). Without rerolling that source, its geomorph
        // never changes and the topology dead-end persists indefinitely.
        if (zone.Layer.m_type != LG_LayerType.MainLayer && zone.Layer.m_buildData != null)
        {
            var srcLayer = zone.Layer.m_buildData.m_buildFromLayer;
            var srcIndex = zone.Layer.m_buildData.m_buildFromZone;

            ZoneSeedManager.Reroll_SubSeed(srcIndex, zone.DimensionIndex, srcLayer);
            Plugin.Logger.LogDebug($"Cross-layer cascade: also rerolling source {srcIndex} in {srcLayer}");

            if (srcIndex != eLocalZoneIndex.Zone_0)
                WalkAncestors(srcIndex, zone.DimensionIndex, srcLayer);
        }

        // Escalation: subseed variation alone may not be enough to escape a topology dead-end,
        // so after a threshold we bulk-reroll every MainLayer zone in the dimension to force
        // broader geometric variation across the next rebuild.
        if (rollCount > kBroadenThreshold)
        {
            Plugin.Logger.LogWarning($"Broadening reroll to all MainLayer zones (attempt {rollCount})");

            var floor = Builder.CurrentFloor;
            if (floor != null)
            {
                foreach (var z in floor.allZones)
                {
                    if (z?.Layer?.m_type == LG_LayerType.MainLayer && z.DimensionIndex == zone.DimensionIndex)
                        ZoneSeedManager.Reroll_SubSeed(z);
                }
            }
        }
    }

    private static void WalkAncestors(eLocalZoneIndex startIndex, eDimensionIndex dimension, LG_LayerType layer)
    {
        var currentIndex = startIndex;

        while (true)
        {
            ZoneSeedManager.Reroll_SubSeed(currentIndex, dimension, layer);
            Plugin.Logger.LogDebug($"Also rerolling ancestor zone {currentIndex} in {layer}");

            // Zone_0 has no same-layer parent, stop here
            if (currentIndex == eLocalZoneIndex.Zone_0)
                break;

            var currentZone = Game.FindZone(currentIndex, dimension, layer);
            if (currentZone?.m_settings?.m_zoneData == null)
                break;

            currentIndex = currentZone.m_settings.m_zoneData.BuildFromLocalIndex;
        }
    }
}
