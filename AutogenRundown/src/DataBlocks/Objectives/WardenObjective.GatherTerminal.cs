using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: Gather Terminal
 *
 *
 * TBD
 */
public partial record WardenObjective
{
    private void PreBuild_GatherTerminal(BuildDirector director, Level level)
    {
        GatherTerminal_SpawnCount = (level.Tier, director.Bulkhead) switch
        {
            ("A", _) => 2,

            ("B", Bulkhead.Main) => 3,
            ("B", _) => 2,

            ("C", Bulkhead.Main) => 4,
            ("C", _) => 2,

            ("D", Bulkhead.Main) => 4,
            ("D", Bulkhead.Extreme) => 2,
            ("D", Bulkhead.Overload) => 3,

            ("E", Bulkhead.Main) => 6,
            ("E", Bulkhead.Extreme) => 3,
            ("E", Bulkhead.Overload) => 4,

            (_, _) => 2,
        };
        GatherTerminal_RequiredCount = Math.Min(GatherTerminal_SpawnCount, (level.Tier, director.Bulkhead) switch
        {
            ("A", _) => 2,

            ("B", Bulkhead.Main) => Generator.Between(2, 3),
            ("B", _) => 2,

            ("C", Bulkhead.Main) => Generator.Between(3, 4),
            ("C", _) => 2,

            ("D", Bulkhead.Main) => Generator.Between(3, 4),
            ("D", Bulkhead.Extreme) => 2,
            ("D", Bulkhead.Overload) => 3,

            ("E", Bulkhead.Main) => Generator.Between(4, 5),
            ("E", Bulkhead.Extreme) => 3,
            ("E", Bulkhead.Overload) => Generator.Between(3, 4),

            (_) => 2,
        });
    }

    private void Build_GatherTerminal(BuildDirector director, Level level)
    {
        MainObjective = "Download <color=orange>Decryption Keys</color> on terminals in the sector and then proceed to the exit point.";
        SolveItem = "Use <color=orange>\"Extract_Decryption_Key\"</color> command to transfer data to portable unit.";
        SolveItemHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";
        GoToWinCondition_Elevator = "Proceed to the extraction point in [EXTRACTION_ZONE]";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        GatherTerminal_Command = "Extract_Decryption_Key";
        GatherTerminal_CommandHelp = "Extract decryption key on local system and transfer to portable unit";
        GatherTerminal_DownloadingText = "";
        GatherTerminal_DownloadCompleteText = "";
        GatherTerminal_DownloadTime = level.Tier switch
        {
            "A" => Generator.Between(2, 4),
            "B" => Generator.Between(3, 6),
            "C" => Generator.Between(5, 8),
            "D" => Generator.Between(6, 10),
            "E" => Generator.Between(8, 14),
        };
    }
}
