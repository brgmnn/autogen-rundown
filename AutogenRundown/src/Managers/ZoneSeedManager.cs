using AutogenRundown.Utils;
using GameData;
using LevelGeneration;

namespace AutogenRundown.Managers;

/// <summary>
/// Check Patch_LG_Layer for CreateZone prefix
/// </summary>
public static class ZoneSeedManager
{
    // TODO: Move marker subseed logic to here
    // private static readonly Dictionary<(eDimensionIndex dim, eLocalZoneIndex lz), int> ZoneAttempts = new();
    //
    // private static readonly HashSet<(eDimensionIndex dim, eLocalZoneIndex lz)> TargetsDetected = new();
    //
    // private static readonly Dictionary<(eDimensionIndex dim, eLocalZoneIndex lz), uint> MarkerSubSeeds = new();

    private static readonly HashSet<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index)> FailedSubSeeds = new();

    public static readonly Dictionary<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index), uint> SubSeeds = new();

    private static readonly HashSet<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index)> FailedMarkerSubSeeds = new();

    public static readonly Dictionary<(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex index), uint> MarkerSubSeeds = new();


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

            SubSeeds[(dimension, layer, index)] = subSeed + 1;
        }

        FailedSubSeeds.Clear();

        if (successful)
            SubSeeds.Clear();

        return successful;
    }

    #endregion
}
