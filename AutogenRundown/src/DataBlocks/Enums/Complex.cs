namespace AutogenRundown.DataBlocks.Enums;

/// <summary>
/// Which group of tiles this data will generate for
///
/// This seems like it needs to be the same for the entire level.
/// </summary>
public enum Complex
{
    Mining = 1,
    Tech = 3,
    Service = 27 // Just floodways, no gardens
    // Service = 53, // With gardens
}
