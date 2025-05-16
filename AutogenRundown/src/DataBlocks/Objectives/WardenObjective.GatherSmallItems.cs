using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: GatherSmallItems
 *
 *
 * Gather small items from around the level. This is a fairly simple objective
 * that can be completed in a variety of ways.
 */
public partial record WardenObjective
{
    public void Build_GatherSmallItems(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        var (itemId, name, description) = Generator.Pick(BuildSmallPickupPack(level.Tier));
        var strategy = Generator.Pick(new List<DistributionStrategy>
        {
            DistributionStrategy.Random,
            DistributionStrategy.SingleZone,
            DistributionStrategy.EvenlyAcrossZones
        });

        MainObjective = description;
        FindLocationInfo = $"Look for {name}s in the complex";
        FindLocationInfoHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";

        GatherRequiredCount = level.Tier switch
        {
            "A" => Generator.Random.Next(4, 8),
            "B" => Generator.Random.Next(6, 10),
            "C" => Generator.Random.Next(7, 12),
            "D" => Generator.Random.Next(8, 13),
            "E" => Generator.Random.Next(9, 16),
            _ => 1,
        };

        GatherItemId = (uint)itemId;
        GatherSpawnCount = Generator.Random.Next(
            GatherRequiredCount,
            GatherRequiredCount + 6);

        DistributeObjectiveItems(level, director.Bulkhead, strategy);

        var zoneSpawns = dataLayer.ObjectiveData.ZonePlacementDatas[0].Count;

        GatherMaxPerZone = GatherSpawnCount / zoneSpawns + GatherSpawnCount % zoneSpawns;
    }
}
