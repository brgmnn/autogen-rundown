using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: CorruptedTerminalUplink
 *
 *
 * Terminal uplink but the codes get sent to a second terminal
 */
public partial record WardenObjective
{
    public void PreBuild_CorruptedTerminalUplink(BuildDirector director, Level level)
    {
        Uplink_NumberOfTerminals = (level.Tier, director.Bulkhead) switch
        {
            ("A", _) => 1,
            ("B", _) => 1,

            ("C", Bulkhead.Main) => Generator.Between(1, 2),
            ("C", _) => 1,

            ("D", Bulkhead.Main) => Generator.Between(2, 3),
            ("D", _) => 2,

            ("E", Bulkhead.Main) => Generator.Between(2, 4),
            ("E", _) => 2,

            (_, _) => 1
        };
        Uplink_NumberOfVerificationRounds = (level.Tier, Uplink_NumberOfTerminals) switch
        {
            ("A", _) => 3,

            ("B", _) => Generator.Between(3, 4),

            ("C", 1) => Generator.Between(4, 6),
            ("C", 2) => Generator.Between(4, 5),
            ("C", 3) => Generator.Between(3, 4),

            ("D", 1) => Generator.Between(5, 6),
            ("D", 2) => Generator.Between(4, 6),
            ("D", 3) => 4,

            ("E", 1) => Generator.Between(8, 12),
            ("E", 2) => Generator.Between(5, 6),
            ("E", 3) => 5,
            ("E", 4) => 5,

            (_, _) => 1,
        };

        Uplink_NumberOfTerminals = 1;
        Uplink_NumberOfVerificationRounds = 2;
    }

    public void Build_CorruptedTerminalUplink(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = "Find the <u>Uplink Terminals</u> [ALL_ITEMS] and establish an external uplink from each terminal";
        FindLocationInfo = "Gather information about the location of the terminals";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = "Navigate to [ITEM_ZONE] and find [ALL_ITEMS]";
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Use maintenance terminal command PING to find [ITEM_SERIAL]";
        InZoneFindItemHelp = "CORTEX INTERFACE ESTABLISHED";
        SolveItem = "Use [ITEM_SERIAL] to create an uplink to [UPLINK_ADDRESS]";
        SolveItemHelp = "Use the UPLINK_CONNECT command to establish the connection";
        GoToWinCondition_Elevator =
            "Neural Imprinting Protocols retrieved. Return to the point of entrance in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_Elevator =
            "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_CustomGeo =
            "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition";

        var placements = new List<ZonePlacementData>();
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

        dataLayer.ObjectiveData.ZonePlacementDatas.Add(placements);

        AddCompletedObjectiveWaves(level, director);
    }
}
