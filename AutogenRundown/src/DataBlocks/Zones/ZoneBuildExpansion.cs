namespace AutogenRundown.DataBlocks.Zones;

/// <summary>
/// eZoneBuildFromExpansionType
///
/// What direction to try and place the entrance door for this zone
///
/// Direction is global and forward is looking from the drop elevator
/// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezonebuildfromexpansiontype
/// </summary>
public enum ZoneBuildExpansion
{
    Random = 0,
    Forward = 1,
    Backward = 2,
    Right = 3,
    Left = 4
}