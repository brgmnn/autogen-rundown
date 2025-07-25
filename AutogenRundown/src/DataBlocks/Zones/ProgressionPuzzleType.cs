namespace AutogenRundown.DataBlocks.Zones;

public enum ProgressionPuzzleType : uint
{
    None = 0,

    /// <summary>
    /// Game: Keycard_SecurityBox
    /// </summary>
    Keycard = 1,

    /// <summary>
    /// Game: PowerGenerator_And_PowerCell
    /// </summary>
    Generator = 2,

    /// <summary>
    /// Game: Locked_No_Key
    /// </summary>
    Locked = 3
}
