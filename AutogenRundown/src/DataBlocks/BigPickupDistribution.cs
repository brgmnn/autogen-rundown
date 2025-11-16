using AutogenRundown.DataBlocks.Items;

namespace AutogenRundown.DataBlocks;

public record BigPickupDistribution : DataBlock<BigPickupDistribution>
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

    public const uint PowerCellId_1 = 90_101u;
    public const uint PowerCellId_2 = 90_102u;
    public const uint PowerCellId_3 = 90_103u;
    public const uint PowerCellId_4 = 90_104u;
    public const uint PowerCellId_5 = 90_105u;

    public static readonly BigPickupDistribution PowerCell_1 = new()
    {
        PersistentId = PowerCellId_1,
        SpawnsPerZone = 1,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };

    public static readonly BigPickupDistribution PowerCell_2 = new()
    {
        PersistentId = PowerCellId_2,
        SpawnsPerZone = 2,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };

    public static readonly BigPickupDistribution PowerCell_3 = new()
    {
        PersistentId = PowerCellId_3,
        SpawnsPerZone = 3,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };

    public static readonly BigPickupDistribution PowerCell_4 = new()
    {
        PersistentId = PowerCellId_4,
        SpawnsPerZone = 4,
        SpawnData = { new ItemSpawn { Item = Items.Item.PowerCell } }
    };

    public static readonly BigPickupDistribution PowerCell_5 = new()
    {
        PersistentId = PowerCellId_5,
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
