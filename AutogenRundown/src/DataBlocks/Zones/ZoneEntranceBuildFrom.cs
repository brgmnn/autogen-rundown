namespace AutogenRundown.DataBlocks.Zones;

/// <summary>
/// Zone.StartPosition
///
/// Where in the source zone to try to make the entrance to this zone.
/// Note that a valid gate may not generate around the set source position/area.
/// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezonebuildfromtype
/// </summary>
public enum ZoneEntranceBuildFrom
{
    Random = 0,
    Start = 1,
    AverageCenter = 2,
    Furthest = 3,
    BetweenStartAndFurthest = 4,
    IndexWeight = 5
}
