namespace AutogenRundown.DataBlocks;

/// <summary>
/// Which group of tiles this data will generate for
///
/// This seems like it needs to be the same for the entire level.
/// </summary>
public enum Complex
{
    Mining = 1,
    Tech = 3,

    // TODO: this seems to just be service, no gardens
    Service = 27 // Just floodways, no gardens
    // Service = 53,
}

/// <summary>
/// Complex types
/// </summary>
public enum SubComplex
{
    // ComplexResourceData.Mining
    DigSite = 0,
    Refinery = 1,
    Storage = 2,
    // MiningReactor = 7,

    // ComplexResourceData.Tech
    DataCenter = 3,
    Lab = 4,

    // ComplexResourceData
    Floodways = 6,
    Gardens = 11,

    // Choose anything valid
    All = 5,
}
