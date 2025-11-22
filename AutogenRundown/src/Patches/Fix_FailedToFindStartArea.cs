using AutogenRundown.Managers;
using GameData;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Fix_FailedToFindStartArea
{
    public static readonly Dictionary<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index), uint> zoneFailures = new();

    /// <summary>
    /// This is where we need to catch and itemize failed zones
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__exception"></param>
    [HarmonyPatch(typeof(LG_ZoneJob_CreateExpandFromData), nameof(LG_ZoneJob_CreateExpandFromData.Build))]
    [HarmonyFinalizer]
    public static void Post_LG_ZoneJob_CreateExpandFromData_Build(LG_ZoneJob_CreateExpandFromData __instance, ref Exception? __exception)
    {
        // [Warning:AutogenRundown] Re-rolling subSeed=5699
        // [Error  :     Unity] WARNING : Zone1 (Zone_1 - 315): Failed to find any good StartAreas in zone 0 (314) expansionType:Towards_Random m_buildFromZone.m_areas: 1 scoredCount:0 dim: Reality

        // TODO: can't call Reroll_Subseed here as it still seems to be stuck?

        if (__instance.m_mainStatus == LG_ZoneJob_CreateExpandFromData.MainStatus.FindStartArea &&
            __instance.m_subStatus == LG_ZoneJob_CreateExpandFromData.SubStatus.SelectArea &&
            __instance.m_scoredStartAreas.Count < 1)
        {
            var zone = __instance.m_zone;
            var zoneKey = (zone.m_dimensionIndex, zone.m_layer.m_type, zone.LocalIndex);

            var rollCount = 0u;

            if (zoneFailures.TryGetValue(zoneKey, out var count))
            {
                rollCount = count;

                if (count > 50)
                {
                    ZoneSeedManager.Reroll_SubSeed(zone);
                    zoneFailures[zoneKey] = 0;

                    return;
                }
            }
            else
                zoneFailures[zoneKey] = 0;

            zoneFailures[zoneKey] += 1;

            Plugin.Logger.LogWarning($"Re-rolling ({rollCount}) NEW subSeed={__instance.m_subSeed}");
            __instance.m_subSeed++;
        }
    }
}
