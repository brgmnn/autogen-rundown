using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Utils;
using GameData;
using LevelGeneration;

namespace AutogenRundown.Managers;

/// <summary>
/// Check Patch_LG_Layer for CreateZone prefix
/// </summary>
public static class ZoneSeedManager
{
    private static readonly HashSet<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index)> FailedSubSeeds = new();

    public static readonly Dictionary<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index), uint> SubSeeds = new();

    /// <summary>
    /// Per-zone overrides for ExpeditionZoneData.MarkerSubSeed, applied at LG_Layer.CreateZone
    /// so they survive rebuilds, same as SubSeeds. Used by Fix_MissingGeneratorCluster to
    /// re-roll marker spawner selection without perturbing zone geometry.
    /// </summary>
    public static readonly Dictionary<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index), uint> MarkerSubSeeds = new();

    /// <summary>
    /// Per-zone overrides for ExpeditionZoneData.StartExpansion. Applied at LG_Layer.CreateZone
    /// so they survive rebuilds, same as SubSeeds. Used by Fix_FailedToFindStartArea once
    /// subseed rerolls alone aren't escaping a topology dead-end.
    /// </summary>
    public static readonly Dictionary<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index), ZoneBuildExpansion> StartExpansionOverrides = new();


    #region Public methods

    public static void Setup()
    {
        FactoryJobManager.OnDoneValidate += ValidateSubSeeds;
    }

    public static void Reroll_SubSeed(LG_Zone zone)
        => Reroll_SubSeed(zone.LocalIndex, zone.DimensionIndex, zone.Layer.m_type);

    public static void Reroll_SubSeed(
        eLocalZoneIndex localIndex,
        eDimensionIndex dimension = eDimensionIndex.Reality,
        LG_LayerType layer = LG_LayerType.MainLayer)
    {
        FactoryJobManager.MarkForRebuild();

        FailedSubSeeds.Add((dimension, layer, localIndex));
    }

    public static void Override_StartExpansion(LG_Zone zone, ZoneBuildExpansion expansion)
        => Override_StartExpansion(zone.LocalIndex, zone.DimensionIndex, zone.Layer.m_type, expansion);

    public static void Override_StartExpansion(
        eLocalZoneIndex localIndex,
        eDimensionIndex dimension,
        LG_LayerType layer,
        ZoneBuildExpansion expansion)
    {
        StartExpansionOverrides[(dimension, layer, localIndex)] = expansion;
        FactoryJobManager.MarkForRebuild();
    }

    #endregion

    #region Private methods

    private static bool ValidateSubSeeds()
    {
        var successful = FailedSubSeeds.Count == 0;

        foreach (var (dimension, layer, index) in FailedSubSeeds)
        {
            var zone = Game.FindZone(index, dimension, layer);

            if (zone == null)
                continue;

            var subSeed = SubSeeds.TryGetValue((dimension, layer, index), out var value)
                ? value
                : zone.m_subSeed;

            SubSeeds[(dimension, layer, index)] = subSeed + 137;
        }

        FailedSubSeeds.Clear();

        if (successful)
            SubSeeds.Clear();

        return successful;
    }

    #endregion
}
