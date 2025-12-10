namespace AutogenRundown.DataBlocks.Objectives;

/// <summary>
/// Base game objective types. Some of these seem to be quite similar and some it's not clear
/// what they would be for such as Empty.
/// </summary>
public enum WardenObjectiveType
{
    HsuFindSample = 0,            // H
    ReactorStartup = 1,           // R
    ReactorShutdown = 2,          // S
    GatherSmallItems = 3,         // G
    ClearPath = 4,                // P
    SpecialTerminalCommand = 5,   // T
    RetrieveBigItems = 6,         // B
    PowerCellDistribution = 7,    // C
    TerminalUplink = 8,           // U
    CentralGeneratorCluster = 9,  // C
    HsuActivateSmall = 10,        // N
    Survival = 11,                // X
    GatherTerminal = 12,          // G
    CorruptedTerminalUplink = 13, // Y
    TimedTerminalSequence = 15,   // W

    Empty = 14,

    #region Autogen Custom objectives

    ReachKdsDeep = 20             // K

    #endregion
}
