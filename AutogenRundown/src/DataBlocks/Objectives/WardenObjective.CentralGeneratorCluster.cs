using AutogenRundown.DataBlocks.Alarms;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: CentralGeneratorCluster
 *
 *
 * TODO: Broken! Currently spawning the geomorph doesn't reliably get a generator
 *       cluster to appear.
 *
 * Discord says getting the generator cluster to spawn can be tricky and require
 * re-rolls with the zone seed. Still waiting on seeing if this is a problem.
 */
public partial record class WardenObjective : DataBlock
{
    public void Build_CentralGeneratorCluster(BuildDirector director, Level level)
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

        ChainedPuzzleMidObjective = ChainedPuzzle.AlarmClass1.PersistentId;
        //"ChainedPuzzleAtExit": 11,

        PowerCellsToDistribute = 3;
        CentralPowerGenClustser_NumberOfGenerators = 2;
        CentralPowerGenClustser_NumberOfPowerCells = 2;

    }
}
