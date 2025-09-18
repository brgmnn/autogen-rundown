using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// When building the power cell distribution layout, here we are modelling a hub with offshoot zones.
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    public void BuildLayout_PowerCellDistribution(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;

        // Zone 1 is an entrance I-geo
        startZone.GenCorridorGeomorph(director.Complex);
        startZone.RollFog(level);
        start.MaxConnections = 1;
        level.Planner.UpdateNode(start);

        // Zone 2 is a hub zone for branches where generators live
        var hubIndex = level.Planner.NextIndex(director.Bulkhead);
        var hub = new ZoneNode(director.Bulkhead, hubIndex);
        hub.MaxConnections = 3;

        var zone = new Zone(level, this) { LightSettings = Lights.GenRandomLight() };
        zone.GenHubGeomorph(director.Complex);
        zone.RollFog(level);

        level.Planner.Connect(start, hub);
        level.Planner.AddZone(hub, zone);

        // Builds a branch with a generator in it. Reused for a lot of places.
        void BuildGeneratorBranch(ZoneNode baseNode, string branch)
        {
            var objectiveLayerData = level.GetObjectiveLayerData(director.Bulkhead);
            var branchZoneCount = Generator.Between(2, 3);
            var prev = baseNode;

            // Generate the zones for this generators branch
            for (int i = 0; i < branchZoneCount; i++)
            {
                var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                var next = new ZoneNode(director.Bulkhead, zoneIndex, branch);
                var nextZone = new Zone(level, this)
                {
                    Coverage = CoverageMinMax.GenNormalSize(),
                    LightSettings = Lights.GenRandomLight(),
                };
                nextZone.RollFog(level);

                level.Planner.Connect(prev, next);
                level.Planner.AddZone(next, nextZone);

                prev = next;
            }

            // Place the generator in the last zone of the branch
            var lastZone = level.Planner.GetZone(prev)!;
            lastZone.PowerGeneratorPlacements.Add(
                new FunctionPlacementData()
                {
                    PlacementWeights = ZonePlacementWeights.NotAtStart
                });

            // Assign the zone placement data for the objective text
            objectiveLayerData.ObjectiveData.ZonePlacementDatas.Add(
                new List<ZonePlacementData>
                {
                    new ZonePlacementData
                    {
                        LocalIndex = prev.ZoneNumber,
                        Weights = ZonePlacementWeights.NotAtStart
                    }
                });
        }

        // Create base branches for each generator
        for (int g = 0; g < Math.Min(objective.PowerCellsToDistribute, 3); g++)
        {
            BuildGeneratorBranch(hub, $"generator_{g}");
        }

        if (objective.PowerCellsToDistribute > 3)
        {
            var secondHubBase = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead, "generator_2")!;

            // Special case where we need one more zone. Just add another generator branch.
            if (objective.PowerCellsToDistribute == 4)
            {
                BuildGeneratorBranch(secondHubBase, "generator_3");
            }
            else if (objective.PowerCellsToDistribute == 5)
            {
                // Case where we want two more zones. Add a hub with two more cells
                var hub2 = new ZoneNode(director.Bulkhead, level.Planner.NextIndex(director.Bulkhead));
                hub2.MaxConnections = 3;

                var zoneHub2 = new Zone(level, this) { LightSettings = Lights.GenRandomLight() };
                zoneHub2.GenHubGeomorph(director.Complex);
                zoneHub2.RollFog(level);

                level.Planner.Connect(secondHubBase, hub2);
                level.Planner.AddZone(hub2, zoneHub2);

                for (int g = 3; g < 5; g++)
                {
                    BuildGeneratorBranch(hub2, $"generator_{g}");
                }
            }
        }
    }
}
