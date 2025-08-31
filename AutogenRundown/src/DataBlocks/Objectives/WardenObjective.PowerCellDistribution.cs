using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: PowerCellDistribution
 *
 *
 * Drop in with power cells and distribute them to generators in various zones.
 *
 * The power cells set with PowerCellsToDistribute are dropped in with you
 * automatically.
 *      Edit: THE POWER CELLS DO NOT APPEAR IN THE FIRST ZONE. YOU MUST PLACE THEM.
 *
 ***************************************************************************************************
 *      TODO List
 *
 *  - Interesting power cell placement requirements?
 */
public partial record class WardenObjective : DataBlock
{
    public void PreBuild_PowerCellDistribution(BuildDirector director, Level level)
    {
        PowerCellsToDistribute = director.Tier switch
        {
            "A" => Generator.Random.Next(1, 2),
            "B" => Generator.Random.Next(1, 2),
            "C" => Generator.Random.Next(2, 3),
            "D" => Generator.Random.Next(3, 4),
            "E" => Generator.Random.Next(3, 5),
            _ => 2
        };
    }

    public void Build_PowerCellDistribution(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = new Text("Distribute Power Cells from the elevator cargo container to [ALL_ITEMS]");
        FindLocationInfo = "Locate the Generators and bring the Power Cells to them";
        FindLocationInfoHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";
        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        if (!director.Bulkhead.HasFlag(Bulkhead.Main))
        {
            // Place the cells in the first zone of the bulkhead if we are not in Main
            var node = level.Planner.GetZones(director.Bulkhead).First();
            var zone = level.Planner.GetZone(node)!;

            switch (PowerCellsToDistribute)
            {
                case 1:
                    zone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_1.PersistentId;
                    break;
                case 2:
                    zone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_2.PersistentId;
                    break;
                case 3:
                    zone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_3.PersistentId;
                    break;
                case 4:
                    zone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_4.PersistentId;
                    break;
                case 5:
                    zone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_5.PersistentId;
                    break;
                default:
                    Plugin.Logger.LogError($"Unhandled number of power cells ({PowerCellsToDistribute}) to distribute");
                    throw new Exception($"Unhandled number of power cells to distribute");
            }
        }
    }
}
