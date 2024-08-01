using AutogenRundown.DataBlocks.Alarms;
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

        // Now we build the zones for the codes
        for (var i = 0; i < objective.TimedTerminalSequence_NumberOfTerminals; i++)
        {
            var size = director.Tier switch
            {
                "D" => Generator.Select(new List<(double, int)>
                {
                    (0.65, 1),
                    (0.35, 2)
                }),
                "E" => Generator.Select(new List<(double, int)>
                {
                    (0.3, 1),
                    (0.7, 2)
                }),

                _ => 1,
            };
            var end = BuildBranch(hub, size, $"timed_terminal_{i}");
            var endZone = planner.GetZone(end)!;

            // Place 3 terminals in the zone
            endZone.TerminalPlacements = new List<TerminalPlacement>
            {
                new() { PlacementWeights = ZonePlacementWeights.NotAtStart },
                new() { PlacementWeights = ZonePlacementWeights.NotAtStart },
                new() { PlacementWeights = ZonePlacementWeights.NotAtStart }
            };
            endZone.Coverage = new CoverageMinMax() { Min = 32, Max = 64 };

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
    }
}
