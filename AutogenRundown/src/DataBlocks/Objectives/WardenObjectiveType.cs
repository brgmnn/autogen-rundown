namespace AutogenRundown.DataBlocks.Objectives
{
    /// <summary>
    /// Base game objective types. Some of these seem to be quite similar and some it's not clear
    /// what they would be for such as Empty.
    /// </summary>
    internal enum WardenObjectiveType
    {
        HsuFindSample = 0,
        ReactorShutdown = 2,
        GatherSmallItems = 3,
        ClearPath = 4,
        SpecialTerminalCommand = 5,
        TerminalUplink = 8,

        ReactorStartup = 1,
        RetrieveBigItems = 6,
        PowerCellDistribution = 7,
        CentralGeneratorCluster = 9,
        //HsuActivateSmall = 10,
        Survival = 11,
        GatherTerminal = 12,
        CorruptedTerminalUplink = 13,
        Empty = 14,
        TimedTerminalSequence = 15
    }
}
