using AutogenRundown.DataBlocks.Items;
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
    public void BuildLayout_CentralGeneratorCluster(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;

        var prev = start;

        // Place the generator cluster in the first zone
        var firstZone = level.Planner.GetZone(prev)!;
        firstZone.GenGeneratorClusterGeomorph(director.Complex);

        // Place out some cell zones
        for (int i = 1; i < director.ZoneCount; i++)
        {
            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
            var next = new ZoneNode(director.Bulkhead, zoneIndex);

            level.Planner.Connect(prev, next);
            level.Planner.AddZone(
                next,
                new Zone(level.Tier)
                {
                    Coverage = CoverageMinMax.GenNormalSize(),
                    LightSettings = Lights.GenRandomLight(),
                });

            prev = next;
        }

        // Distribute cells
        var pickup = new BigPickupDistribution
        {
            SpawnsPerZone = 2,
            SpawnData = new List<ItemSpawn>
            {
                new() { Item = Items.Item.PowerCell },
                new() { Item = Items.Item.PowerCell }
            }
        };
        Bins.BigPickupDistributions.AddBlock(pickup);

        var last = prev;
        var lastZone = level.Planner.GetZone(last)!;

        lastZone.BigPickupDistributionInZone = pickup.PersistentId;
    }
}
