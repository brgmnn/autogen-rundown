using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record class LevelLayout : DataBlock
{
    /// <summary>
    /// Builds the level layout for the Matter Wave Projector big item retrieval objective.
    ///
    /// TODO: re-evaluate this objective
    /// </summary>
    public static void BuildLayout_MatterWaveProjector(
        Level level,
        BuildDirector director,
        WardenObjective objective,
        ZoneNode start)
    {
        var objectiveLayerData = level.GetObjectiveLayerData(director.Bulkhead);
        var branchZoneCount = Generator.Random.Next(3, 4);
        var prev = start;

        // Generate some zones leading to the objective
        for (int i = 0; i < branchZoneCount; i++)
        {
            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
            var next = new ZoneNode(director.Bulkhead, zoneIndex);
            var nextZone = new Zone(level.Tier)
            {
                Coverage = CoverageMinMax.GenNormalSize(),
                LightSettings = Lights.GenRandomLight(),
            };
            nextZone.RollFog(level);

            level.Planner.Connect(prev, next);
            level.Planner.AddZone(next, nextZone);

            prev = next;
        }

        // Penultimate zone is an I-corridor
        var corridorIndex = level.Planner.NextIndex(director.Bulkhead);
        var corridor = new ZoneNode(director.Bulkhead, corridorIndex);
        corridor.MaxConnections = 1;

        var corridorZone = new Zone(level.Tier) { LightSettings = Lights.GenRandomLight() };
        corridorZone.GenCorridorGeomorph(director.Complex);
        corridorZone.RollFog(level);

        level.Planner.Connect(prev, corridor);
        level.Planner.AddZone(corridor, corridorZone);

        // Final zone is the matter wave projector
        var mwpIndex = level.Planner.NextIndex(director.Bulkhead);
        var mwp = new ZoneNode(director.Bulkhead, mwpIndex);
        mwp.MaxConnections = 3;

        var mwpZone = new Zone(level.Tier) { LightSettings = Lights.GenRandomLight() };
        mwpZone.GenMatterWaveProjectorGeomorph(director.Complex);
        mwpZone.RollFog(level);

        level.Planner.Connect(corridor, mwp);
        level.Planner.AddZone(mwp, mwpZone);

        // Assign the zone placement data for the objective text
        objectiveLayerData.ObjectiveData.ZonePlacementDatas.Add(
            new List<ZonePlacementData>
            {
                new ZonePlacementData
                {
                    LocalIndex = mwp.ZoneNumber,
                    Weights = ZonePlacementWeights.NotAtStart
                }
            });

        // Ensure there's a nice spicy hoard at the end, also include bosses for harder levels
        // You best stealth this zone!
        switch (level.Tier)
        {
            case "A":
                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.TierA with { Points = 10 });
                break;

            case "B":
                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.TierB with { Points = 20 });
                break;

            case "C":
                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.TierC with { Points = 30 });
                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Striker with { Points = 20 });

                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Mother with { Points = 10 });
                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Baby with { Points = 10 });
                break;

            case "D":
                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.TierD with { Points = 30 });
                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Striker with { Points = 30 });

                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Tank with { Points = 10 });
                break;

            case "E":
                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.TierE with { Points = 40 });
                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Striker with { Points = 30 });

                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.PMother with { Points = 10 });
                mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Baby with { Points = 10 });
                break;
        }
    }

    /// <summary>
    /// TODO: Hisec cargo C3 in Feb monthly built bulkhead main to nowhere
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="start"></param>
    /// <exception cref="Exception"></exception>
    public void BuildLayout_RetrieveBigItems(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode? startish)
    {
        var layerData = level.GetObjectiveLayerData(director.Bulkhead);

        var itemCount = objective.RetrieveItems.Count();
        var item = objective.RetrieveItems.First();

        // Zone 1 is normal
        var entrance = (ZoneNode)startish;
        entrance.MaxConnections = 2;
        level.Planner.UpdateNode(entrance);

        var entranceZone = level.Planner.GetZone(entrance)!;

        if (Generator.Flip())
        {
            entranceZone.GenHubGeomorph(director.Complex);
            entrance.MaxConnections = 3;
        }
        else
        {
            entranceZone.Coverage = new CoverageMinMax { Min = 50, Max = 60 };
        }

        entranceZone.RollFog(level);

        // // Zone 2 is an I corridor
        // var corridorIndex = level.Planner.NextIndex(director.Bulkhead);
        // var corridor = new ZoneNode(director.Bulkhead, corridorIndex);
        // corridor.MaxConnections = 1;
        //
        // var corridorZone = new Zone { LightSettings = Lights.GenRandomLight() };
        // //corridorZone.GenCorridorGeomorph(director.Complex);
        // corridorZone.RollFog(level);
        //
        // level.Planner.Connect(entrance, corridor);
        // level.Planner.AddZone(corridor, corridorZone);

        // // Zone 3 is a hub zone regarduless
        // var hubIndex = level.Planner.NextIndex(director.Bulkhead);
        // var hub = new ZoneNode(director.Bulkhead, hubIndex);
        // hub.MaxConnections = 3;
        //
        // var zone = new Zone { LightSettings = Lights.GenRandomLight() };
        // zone.GenHubGeomorph(director.Complex);
        // zone.RollFog(level);
        //
        // level.Planner.Connect(corridor, hub);
        // level.Planner.AddZone(hub, zone);

        if (item == WardenObjectiveItem.MatterWaveProjector)
        {
            BuildLayout_MatterWaveProjector(level, director, objective, entrance);
            return;
        }

        // Create base branches for each item
        for (var g = 0; g < Math.Min(itemCount, 3); g++)
        {
            var zoneCount = itemCount switch
            {
                1 => Generator.Between(2, 3),
                2 => Generator.Between(1, 2),
                3 => Generator.Between(1, 2),
                _ => 1
            };
            var zoneNode = BuildBranch(entrance, zoneCount, $"bigitem_{g}");

            layerData.ObjectiveData.ZonePlacementDatas.Add(
                new List<ZonePlacementData>
                {
                    new()
                    {
                        LocalIndex = zoneNode.ZoneNumber,
                        Weights = ZonePlacementWeights.NotAtStart
                    }
                });
        }

        if (itemCount <= 3)
            return;

        // We have more than 3 items and so need to place more zones
        var secondHubBase = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead, "bigitem_2")!;

        // Case where we want two more zones. Add a hub with two more cells
        var hub2 = new ZoneNode(director.Bulkhead, level.Planner.NextIndex(director.Bulkhead));
        hub2.MaxConnections = 3;

        var zoneHub2 = new Zone(level.Tier) { LightSettings = Lights.GenRandomLight() };
        zoneHub2.GenHubGeomorph(director.Complex);
        zoneHub2.RollFog(level);

        level.Planner.Connect(secondHubBase, hub2);
        level.Planner.AddZone(hub2, zoneHub2);

        for (var g = 3; g < objective.RetrieveItems.Count; g++)
        {
            var zoneNode = BuildBranch(hub2, Generator.Between(1, 2), $"bigitem_{g}");

            layerData.ObjectiveData.ZonePlacementDatas.Add(
                new List<ZonePlacementData>
                {
                    new()
                    {
                        LocalIndex = zoneNode.ZoneNumber,
                        Weights = ZonePlacementWeights.NotAtStart
                    }
                });
        }
    }
}
