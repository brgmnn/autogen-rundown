namespace AutogenRundown.DataBlocks.Objectives;

/// <summary>
/// Base game objective types. Some of these seem to be quite similar and some it's not clear
/// what they would be for such as Empty.
/// </summary>
public enum WardenObjectiveType
{
    HsuFindSample = 0,            // H :done:
    ReactorStartup = 1,           // R :done:
    ReactorShutdown = 2,          // S :done:
    GatherSmallItems = 3,         // G :done:
    ClearPath = 4,                // P :done:
    SpecialTerminalCommand = 5,   // T :done:
    RetrieveBigItems = 6,         // B :done:
    PowerCellDistribution = 7,    // C :done:
    TerminalUplink = 8,           // U :done:
    CentralGeneratorCluster = 9,  // C :done:
    HsuActivateSmall = 10,        // N :done:
    Survival = 11,                // X :done:
    GatherTerminal = 12,          // G :done:
    CorruptedTerminalUplink = 13, // Y :done:
    TimedTerminalSequence = 15,   // W

    Empty = 14,

    #region Autogen Custom objectives

    ReachKdsDeep = 20             // K

    #endregion
}
