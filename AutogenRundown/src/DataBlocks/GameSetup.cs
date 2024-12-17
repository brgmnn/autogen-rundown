namespace AutogenRundown.DataBlocks;

public record GameSetup : DataBlock
{
    /// <summary>
    /// List of rundown ids to load. It appears these directly correspond in order to the rundowns that are loaded on
    /// the first screen.
    ///
    ///     R1 -> 32,
    ///     R7 -> 31,
    ///     R2 -> 33,
    ///     R3 -> 34,
    ///     R4 -> 37,
    ///     R5 -> 38,
    ///     R6 -> 41,
    ///     R8 -> 35,
    ///
    /// By replacing the ID of these locations with a custom rundown ID, it should use that rundown when selecting it
    /// from the rundown selection screen.
    ///
    /// Looks like we don't actually need to load in the rundown IDs for any rundowns we aren't using
    /// </summary>
    public List<uint> RundownIdsToLoad { get; set; } = new();

    #region Base game settings
    public uint StartupScreenToLoad = 3;
    #endregion
}
