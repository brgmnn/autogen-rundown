using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout : DataBlock
{
    public void BuildLayout_TimedTerminalSequence(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode start)
    {
        var layerData = level.GetObjectiveLayerData(director.Bulkhead);

        // Set entrance zone to corridor
        var entranceZone = level.Planner.GetZone(start)!;
        entranceZone.GenCorridorGeomorph(director.Complex);
        entranceZone.RollFog(level);
        start.MaxConnections = 1;
        level.Planner.UpdateNode(start);

        // Create hub zone
        var hubIndex = level.Planner.NextIndex(director.Bulkhead);
        var hub = new ZoneNode(director.Bulkhead, hubIndex, "timed_terminal_hub");
        hub.MaxConnections = 3;

        var zone = new Zone { LightSettings = Lights.GenRandomLight() };
        zone.GenHubGeomorph(director.Complex);
        zone.SetOutOfFog(level);

        // Place first terminal in the hub
        layerData.ObjectiveData.ZonePlacementDatas.Add(
            new List<ZonePlacementData>
            {
                new ZonePlacementData()
                {
                    LocalIndex = hubIndex,
                    Weights = ZonePlacementWeights.EvenlyDistributed
                }
            });

        level.Planner.Connect(start, hub);
        level.Planner.AddZone(hub, zone);

        var errorPopulation = new List<(double, int, WavePopulation)>
        {
            (1.0, 2, WavePopulation.Baseline),
            (1.0, 1, WavePopulation.OnlyChargers)
        };

        // Now we build the zones for the codes
        for (var i = 0; i < objective.TimedTerminalSequence_NumberOfTerminals; i++)
        {
            var end = BuildBranch(hub, 1, $"timed_terminal_{i}");
            var endZone = planner.GetZone(end)!;

            // Place 3 terminals in the zone
            endZone.TerminalPlacements = new List<TerminalPlacement>
            {
                new() { PlacementWeights = ZonePlacementWeights.NotAtStart },
                new() { PlacementWeights = ZonePlacementWeights.NotAtStart },
                new() { PlacementWeights = ZonePlacementWeights.NotAtStart }
            };
            endZone.Coverage = new CoverageMinMax() { Min = 32, Max = 64 };

            // TODO: don't always do triple error alarms
            // Trial error alarm of baseline enemies
            var puzzle = ChainedPuzzle.AlarmError_Baseline with
            {
                Settings = WaveSettings.Error_Normal,
                Population = Generator.DrawSelect(errorPopulation)
            };
            endZone.Alarm = ChainedPuzzle.FindOrPersist(puzzle);

            // TODO: maybe some more resources
            endZone.AmmoPacks *= 2;
            endZone.ToolPacks *= 2;
            endZone.HealthPacks *= 2;

            // Mark the zone for the objective
            layerData.ObjectiveData.ZonePlacementDatas.Add(
                new List<ZonePlacementData>()
                {
                    new ZonePlacementData()
                    {
                        LocalIndex = end.ZoneNumber,
                        Weights = ZonePlacementWeights.NotAtStart
                    }
                });

            // Lock the first door to the first zone
            var first = planner.GetZones(director.Bulkhead, $"timed_terminal_{i}").First();
            var firstZone = planner.GetZone(first!);

            firstZone!.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;
        }

        var selectedIndex = Generator.Between(0, objective.TimedTerminalSequence_NumberOfTerminals - 1);
        var selected = planner.GetZones(director.Bulkhead, $"timed_terminal_{selectedIndex}").First();

        // Add turnoff zone
        var turnOff = new ZoneNode(director.Bulkhead, planner.NextIndex(director.Bulkhead), $"timed_terminal_error_off");
        var turnOffZone = new Zone
        {
            Coverage = new() { Min = 3.0, Max = 3.0 },
            LightSettings = Lights.GenRandomLight(),
            ProgressionPuzzleToEnter = ProgressionPuzzle.Locked,
            AliasPrefix = "Alarm Control, ZONE",
            AliasPrefixShortOverride = "Alarm Control, Z",
            TerminalPlacements = new()
            {
                new()
                {
                    PlacementWeights = ZonePlacementWeights.AtEnd,
                    UniqueCommands = new()
                    {
                        new()
                        {
                            Command = "DEACTIVATE_ALARMS",
                            CommandDesc = "Turn off all active alarms",
                            CommandEvents = new List<WardenObjectiveEvent>().AddTurnOffAlarms().ToList(),
                        }
                    }
                }
            }
        };

        planner.Connect(selected, turnOff);
        planner.AddZone(turnOff, turnOffZone);
    }
}
