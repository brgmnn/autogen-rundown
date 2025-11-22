using AutogenRundown.Managers;
using GameData;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

[HarmonyPatch]
internal static class Patch_LG_Layer
{
    /// <summary>
    /// Prefixes the CreateZone method to override seed values
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="floor"></param>
    /// <param name="zoneData"></param>
    /// <param name="zoneAliasStart"></param>
    [HarmonyPatch(typeof(LG_Layer), nameof(LG_Layer.CreateZone))]
    [HarmonyPrefix]
    static void Prefix_Layer_CreateZone(
        LG_Layer __instance,
        LG_Floor floor,
        ref ExpeditionZoneData zoneData,
        int zoneAliasStart)
    {
        // TODO: we probably want to extract this to it's own manager/patcher so we can override seeds from other places too
        // TODO: also patch MarkerSubSeeds in this file

        var zoneKey = (__instance.m_dimension.DimensionIndex, __instance.m_type, zoneData.LocalIndex);

        if (ZoneSeedManager.SubSeeds.TryGetValue(zoneKey, out var overrideSubSeed))
        {
            zoneData.SubSeed = (int)overrideSubSeed;

            Plugin.Logger.LogDebug($"Applied override m_subSeed={overrideSubSeed} to Zone_{zoneData.LocalIndex}");
        }
    }
}
