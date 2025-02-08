using AutogenRundown.DataBlocks.Items;

namespace AutogenRundown.DataBlocks;

public record class BigPickupDistribution : DataBlock
{
    public static BigPickupDistribution FogTurbine = new BigPickupDistribution
    {
        SpawnsPerZone = 1,
        SpawnData =
        {
            new ItemSpawn { Item = Items.Item.FogTurbine }
        }
    };

    public static BigPickupDistribution PowerCell_1 = new BigPickupDistribution
    {
        SpawnsPerZone = 1,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };
    public static BigPickupDistribution PowerCell_2 = new BigPickupDistribution
    {
        SpawnsPerZone = 2,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };
    public static BigPickupDistribution PowerCell_3 = new BigPickupDistribution
    {
        SpawnsPerZone = 3,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };
    public static BigPickupDistribution PowerCell_4 = new BigPickupDistribution
    {
        SpawnsPerZone = 4,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };
    public static BigPickupDistribution PowerCell_5 = new BigPickupDistribution
    {
        SpawnsPerZone = 5,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };

    public new static void SaveStatic()
    {
        // Fog clearing
        Bins.BigPickupDistributions.AddBlock(FogTurbine);

        // Power cells
        Bins.BigPickupDistributions.AddBlock(PowerCell_1);
        Bins.BigPickupDistributions.AddBlock(PowerCell_2);
        Bins.BigPickupDistributions.AddBlock(PowerCell_3);
        Bins.BigPickupDistributions.AddBlock(PowerCell_4);
        Bins.BigPickupDistributions.AddBlock(PowerCell_5);
    }

    public uint SpawnsPerZone { get; set; } = 0;

    public List<ItemSpawn> SpawnData { get; set; } = new();
}
