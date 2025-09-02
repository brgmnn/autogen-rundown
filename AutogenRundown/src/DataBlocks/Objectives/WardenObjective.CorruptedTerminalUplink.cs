using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;

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

            ("D", Bulkhead.Main) => 2,
            ("D", _) => Generator.Between(1, 2),

            ("E", Bulkhead.Main) => 3,
            ("E", _) => 2,

            (_, _) => 1
        };
        Uplink_NumberOfVerificationRounds = (level.Tier, Uplink_NumberOfTerminals) switch
        {
            ("A", _) => 2,

            ("B", _) => 3,

            ("C", 1) => 4,
            ("C", 2) => 2,

            ("D", 1) => 4,
            ("D", 2) => 3,

            ("E", 2) => 4,
            ("E", 3) => 5,

            (_, _) => 1,
        };
    }

    public void Build_CorruptedTerminalUplink(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        if (Uplink_NumberOfTerminals > 1)
        {
            MainObjective = new Text("Find the <u>Uplink Terminals</u> [ALL_ITEMS] and establish an external uplink from each terminal");
            FindLocationInfo = "Gather information about the location of the terminals";
        }
        else
        {
            MainObjective = new Text("Find the <u>Uplink Terminal</u> [ALL_ITEMS] and establish an external uplink");
            FindLocationInfo = "Gather information about the location of the terminal";
        }

        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = new Text("Navigate to [ITEM_ZONE] and find [ALL_ITEMS]");
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Use maintenance terminal command PING to find [ITEM_SERIAL]";
        InZoneFindItemHelp = "CORTEX INTERFACE ESTABLISHED";
        SolveItem = "Use [ITEM_SERIAL] to create an uplink to [UPLINK_ADDRESS]";
        SolveItemHelp = "Use the UPLINK_CONNECT command to establish the connection";

        GoToWinCondition_Elevator = new Text(() =>
            $"Neural Imprinting Protocols retrieved. Return to the point of entrance in {Intel.Zone(level.ExtractionZone, level.Planner)}");
        GoToWinConditionHelp_Elevator =
            "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_CustomGeo =
            "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition";

        var placements = new List<ZonePlacementData>();
        var nodes = level.Planner.GetZonesByTag(director.Bulkhead, "uplink_terminal")
            .TakeLast(Uplink_NumberOfTerminals).ToList();

        for (var i = 0; i < Uplink_NumberOfTerminals; i++)
        {
            var node = nodes[i % nodes.Count];
            var zone = level.Planner.GetZone(node);

            // Always add another terminal for the uplink
            zone.TerminalPlacements.Add(new TerminalPlacement());

            if (i < nodes.Count)
            {
                // Add extra terminal placements for the verification codes
                // This is only if we didn't add them yet
                zone.TerminalPlacements.Add(new TerminalPlacement());
                zone.TerminalPlacements.Add(new TerminalPlacement());
            }

            placements.Add(new ZonePlacementData
            {
                LocalIndex = node.ZoneNumber,
                Weights = ZonePlacementWeights.NotAtStart
            });
        }

        dataLayer.ObjectiveData.ZonePlacementDatas.Add(placements);

        // TODO: add more interesting waves
        // Alarms do indeed get canceled after completing the uplink
        var waves = level.Tier switch
        {
            "A" => GenericWave.Uplink_Easy,
            "B" => GenericWave.Uplink_Easy,
            _ => GenericWave.Uplink_Medium,
        };
        WavesOnActivate.Add(waves);

        AddCompletedObjectiveWaves(level, director);
    }
}
