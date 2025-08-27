namespace AutogenRundown.DataBlocks.Zones;

/// <summary>
/// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ezonedistributionamount
/// </summary>
public enum DistributionAmount
{
    None = 0,

    /// <summary>
    /// Count = 2
    /// </summary>
    Pair = 1,

    /// <summary>
    /// Count = 5
    /// </summary>
    Few = 2,

    /// <summary>
    /// Count = 10
    /// </summary>
    Some = 3,

    /// <summary>
    /// Count = 15
    /// </summary>
    SomeMore = 4,

    /// <summary>
    /// Count = 20
    /// </summary>
    Many = 5,

    /// <summary>
    /// Count = 30
    /// </summary>
    Alot = 6,

    /// <summary>
    /// Count = 50
    /// </summary>
    Tons = 7
}