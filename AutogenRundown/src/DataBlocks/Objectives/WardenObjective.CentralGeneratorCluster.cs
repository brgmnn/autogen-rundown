using AutogenRundown.DataBlocks.Alarms;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: CentralGeneratorCluster
 *
 *
 * Discord says getting the generator cluster to spawn can be tricky and require
 * re-rolls with the zone seed. Still waiting on seeing if this is a problem.
 */
public partial record WardenObjective
{
    private void PreBuild_CentralGeneratorCluster(BuildDirector director, Level level)
    {
        PowerCellsToDistribute = 5;
        CentralGeneratorCluster_NumberOfGenerators = 5;
        CentralGeneratorCluster_NumberOfPowerCells = 5;
    }

    private void Build_CentralGeneratorCluster(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = "Find [COUNT_REQUIRED] Power Cells and bring them to the Central Generator Cluster in [ITEM_ZONE]";
        FindLocationInfo = "Locate the Power Cells and use them to power up the Generator Cluster";
        FindLocationInfoHelp = "Generators Online: [COUNT_CURRENT] / [COUNT_REQUIRED]";
        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Malfunction in air purification system. Make your way for the forward emergency exit.";

        MidPuzzle = ChainedPuzzle.FindOrPersist(ChainedPuzzle.AlarmClass1);
        //"ChainedPuzzleAtExit": 11,
    }
}
