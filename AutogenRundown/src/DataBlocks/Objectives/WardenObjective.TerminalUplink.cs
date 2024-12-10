using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: ClearPath
 *
 *
 * Fairly straight forward objective, get to the end zone. Some additional enemies
 * at the end make this a more interesting experience.
 *
 * This objective can only be for Main given it ends the level on completion
 */
public partial record class WardenObjective : DataBlock
{
    public void Build_TerminalUplink(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = "Find the <u>Uplink Terminals</u> [ALL_ITEMS] and establish an external uplink from each terminal";
        FindLocationInfo = "Gather information about the location of [ALL_ITEMS]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        SolveItem = "Use [ITEM_SERIAL] to create an uplink to [UPLINK_ADDRESS]";
        SolveItemHelp = "Use the UPLINK_CONNECT command to establish the connection";

        GoToWinCondition_Elevator = "Neural Imprinting Protocols retrieved. Return to the point of entrance in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        Uplink_NumberOfTerminals = (level.Tier, director.Bulkhead) switch
        {
            ("A", _) => 1,

            ("B", _) => Generator.Random.Next(1, 2),

            ("C", Bulkhead.Main) => Generator.Random.Next(1, 3),
            ("C", _) => Generator.Random.Next(1, 2),

            ("D", Bulkhead.Main) => Generator.Random.Next(1, 3),
            ("D", _) => Generator.Random.Next(1, 3),

            ("E", Bulkhead.Main) => Generator.Random.Next(2, 4),
            ("E", _) => Generator.Random.Next(1, 3),

            (_, _) => 1
        };
        Uplink_NumberOfVerificationRounds = (level.Tier, Uplink_NumberOfTerminals) switch
        {
            ("A", _) => 3,

            ("B", _) => Generator.Random.Next(3, 4),

            ("C", 1) => Generator.Random.Next(4, 6),
            ("C", 2) => Generator.Random.Next(4, 5),
            ("C", 3) => Generator.Random.Next(3, 4),

            ("D", 1) => Generator.Random.Next(5, 6),
            ("D", 2) => Generator.Random.Next(4, 6),
            ("D", 3) => 4,

            ("E", 1) => Generator.Random.Next(8, 12),
            ("E", 2) => Generator.Random.Next(5, 6),
            ("E", 3) => 5,
            ("E", 4) => 5,

            (_, _) => 1,
        };
        Uplink_WaveSpawnType = SurvivalWaveSpawnType.InSuppliedCourseNodeZone;

        var wave = level.Tier switch
        {
            "A" => GenericWave.Uplink_Easy,
            "B" => GenericWave.Uplink_Easy,
            _ => GenericWave.Uplink_Medium,
        };

        WavesOnActivate.Add(wave);

        var placements = new List<ZonePlacementData>();
        // TODO: Generate proper zones, one for each uplink terminal
        var zones = level.Planner.GetZones(director.Bulkhead, "uplink_terminals")
                                 .TakeLast(Uplink_NumberOfTerminals);



        foreach (var zone in zones)
        {
            placements.Add(new ZonePlacementData
            {
                LocalIndex = zone.ZoneNumber,
                Weights = ZonePlacementWeights.NotAtStart
            });
        }

        // TODO: Seems it picks randomly from the inner list? Let's split it a bit
        dataLayer.ObjectiveData.ZonePlacementDatas.Add(placements);
    }
}
