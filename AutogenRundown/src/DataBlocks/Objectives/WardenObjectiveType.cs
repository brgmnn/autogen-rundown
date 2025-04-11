namespace AutogenRundown.DataBlocks.Objectives
{
    /// <summary>
    /// Base game objective types. Some of these seem to be quite similar and some it's not clear
    /// what they would be for such as Empty.
    /// </summary>
    public enum WardenObjectiveType
    {
        // Implemented
        HsuFindSample = 0,          // H
        ReactorStartup = 1,         // R
        ReactorShutdown = 2,        // S
        GatherSmallItems = 3,       // G
        ClearPath = 4,              // P
        SpecialTerminalCommand = 5, // T
        RetrieveBigItems = 6,       // B
        PowerCellDistribution = 7,  // C
        TerminalUplink = 8,         // U
        Survival = 11,              // X
        TimedTerminalSequence = 15, // W

        CentralGeneratorCluster = 9, // TODO: Generator cluster spawn is bugged

        // Not yet implemented
        HsuActivateSmall = 10, // Seems to be like bring data sphere or neonate
        GatherTerminal = 12, // Unsure
        CorruptedTerminalUplink = 13, // Seems similar to timed terminal sequence?
        Empty = 14
    }
}
