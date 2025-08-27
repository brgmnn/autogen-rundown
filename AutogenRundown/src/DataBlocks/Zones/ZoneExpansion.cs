namespace AutogenRundown.DataBlocks.Zones;

/// <summary>
/// eZoneExpansionType
///
/// What direction to build the zone towards
///
/// Direction is global and forward is looking from the drop elevator
/// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezoneexpansiontype
/// </summary>
public enum ZoneExpansion
{
    Random = 0,
    Collapsed = 1,
    Expansional = 2,
    Forward = 3,
    Backward = 4,
    Right = 5,
    Left = 6,
    DirectionalRandom = 7
}