namespace AutogenRundown.DataBlocks;

/**
 * Objective: Gather Terminal
 *
 *
 * TBD
 */
public partial record WardenObjective
{
    public void PreBuild_GatherTerminal(BuildDirector director, Level level)
    {
        GatherTerminal_SpawnCount = 3;
        GatherTerminal_RequiredCount = 2;
    }

    public void Build_GatherTerminal(BuildDirector director, Level level)
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
        GatherTerminal_DownloadTime = -1.0;
    }
}
