using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

/// <summary>
/// Objective: GatherSmallItems
///
/// Gather small items from around the level. This is a fairly simple objective
/// that can be completed in a variety of ways.
/// </summary>
public partial record WardenObjective
{
    public void PreBuild_GatherSmallItems(BuildDirector director, Level level)
    {
        GatherRequiredCount = (level.Tier, director.Bulkhead) switch
        {
            ("A", Bulkhead.Main) => Generator.Between(4, 8),
            ("A", _) =>             Generator.Between(2, 3),

            ("B", Bulkhead.Main) => Generator.Between(6, 10),
            ("B", _) =>             Generator.Between(2,  4),

            ("C", Bulkhead.Main)     => Generator.Between(7, 12),
            ("C", Bulkhead.Extreme)  => Generator.Between(5, 7),
            ("C", Bulkhead.Overload) => Generator.Between(2, 4),

            ("D", Bulkhead.Main)     => Generator.Between(8, 13),
            ("D", Bulkhead.Extreme)  => Generator.Between(5, 8),
            ("D", Bulkhead.Overload) => Generator.Between(3, 5),

            ("E", Bulkhead.Main)     => Generator.Between(9, 16),
            ("E", Bulkhead.Extreme)  => Generator.Between(6, 8),
            ("E", Bulkhead.Overload) => Generator.Between(3, 6),

            _ => 1,
        };

        GatherSpawnCount = GatherRequiredCount switch
        {
            < 4  => Generator.Between(GatherRequiredCount, GatherRequiredCount + 1),
            < 6  => Generator.Between(GatherRequiredCount, GatherRequiredCount + 2),
            < 9  => Generator.Between(GatherRequiredCount, GatherRequiredCount + 4),
            < 12 => Generator.Between(GatherRequiredCount, GatherRequiredCount + 6),
            _    => Generator.Between(GatherRequiredCount, GatherRequiredCount + 8)
        };
    }

    public void Build_GatherSmallItems(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        var (itemId, name, description) = Generator.Pick(BuildSmallPickupPack(level.Tier));

        MainObjective = description;
        FindLocationInfo = $"Look for {name}s in the complex";
        FindLocationInfoHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";

        GatherItemId = (uint)itemId;

        /*
         * We want to distribute the items evenly across all the zones marked as `find_items`.
         * The LevelLayout code will generate an interesting layout for these
         */
        var placements = Gather_PlacementNodes
            .Select(node => new ZonePlacementData()
            {
                LocalIndex = node.ZoneNumber,
                Weights = ZonePlacementWeights.EvenlyDistributed
            }).ToList();
        dataLayer.ObjectiveData.ZonePlacementDatas.Add(placements);

        GatherMaxPerZone = GatherSpawnCount / placements.Count + GatherSpawnCount % placements.Count;
    }
}
