namespace AutogenRundown.DataBlocks.Enemies;

/// <summary>
/// This enum determins how an enemy group should spawn in a zone. Normally leave this as default
/// but the other values are used to spawn enemies in boss aligned spawn points in specific geos.
///
/// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#espawnplacementtype
/// </summary>
public enum SpawnPlacementType
{
    Default = 0,
    Align_0 = 1,
    Align_1 = 2,
    Align_2 = 3,
    Align_3 = 4,
    Align_4 = 5,
    Align_5 = 6,
    CycleAllAligns = 7
}
