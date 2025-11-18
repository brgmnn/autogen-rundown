using GameData;
using LevelGeneration;

namespace AutogenRundown.Utils;

public static class Game
{
    /// <summary>
    /// Gets the base zone object
    /// </summary>
    /// <param name="localIndex"></param>
    /// <param name="dimension"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static LG_Zone? FindZone(
        eLocalZoneIndex localIndex,
        eDimensionIndex dimension = eDimensionIndex.Reality,
        LG_LayerType? layer = LG_LayerType.MainLayer)
    {
        var floor = Builder.CurrentFloor;

        if (floor == null)
            return null;

        foreach (var z in Builder.CurrentFloor.allZones)
            if (z != null &&
                z.LocalIndex == localIndex &&
                z.DimensionIndex == dimension &&
                z.m_layer.m_type == layer)
            {
                return z;
            }

        return null;
    }
}
