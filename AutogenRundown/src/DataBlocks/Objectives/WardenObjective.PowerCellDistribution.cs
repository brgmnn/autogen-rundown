namespace AutogenRundown.DataBlocks;

/**
 * Objective: PowerCellDistribution
 *
 *
 * Drop in with power cells and distribute them to generators in various zones.
 *
 * The power cells set with PowerCellsToDistribute are dropped in with you
 * automatically.
 */
public partial record class WardenObjective : DataBlock
{
    public void Build_PowerCellDistribution(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = "Distribute Power Cells from the elevator cargo container to [ALL_ITEMS]";
        FindLocationInfo = "Locate the Generators and bring the Power Cells to them";
        FindLocationInfoHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";
        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";
    }
}
