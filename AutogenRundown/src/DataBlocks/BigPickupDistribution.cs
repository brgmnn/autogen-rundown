using AutogenRundown.DataBlocks.Items;

namespace AutogenRundown.DataBlocks;

public record BigPickupDistribution : DataBlock
{
    public static readonly BigPickupDistribution MatterWaveProjector = new()
    {
        SpawnsPerZone = 1,
        SpawnData =
        {
            new ItemSpawn { Item = Items.Item.MatterWaveProjector }
        }
    };

    public static readonly BigPickupDistribution FogTurbine = new()
    {
        SpawnsPerZone = 1,
        SpawnData =
        {
            new ItemSpawn { Item = Items.Item.FogTurbine }
        }
    };

    public static readonly BigPickupDistribution PowerCell_1 = new()
    {
        SpawnsPerZone = 1,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };

    public static readonly BigPickupDistribution PowerCell_2 = new()
    {
        SpawnsPerZone = 2,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };

    public static readonly BigPickupDistribution PowerCell_3 = new()
    {
        SpawnsPerZone = 3,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };

    public static readonly BigPickupDistribution PowerCell_4 = new()
    {
        SpawnsPerZone = 4,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };

    public static readonly BigPickupDistribution PowerCell_5 = new()
    {
        SpawnsPerZone = 5,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };

    public new static void SaveStatic()
    {
        // Objective
        Bins.BigPickupDistributions.AddBlock(MatterWaveProjector);

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
