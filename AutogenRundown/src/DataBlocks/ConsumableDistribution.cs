using AutogenRundown.DataBlocks.Items;

namespace AutogenRundown.DataBlocks;

public record class ConsumableDistribution : DataBlock
{
    /// <summary>
    /// Mostly glowsticks and lock melters, with a small chance of tripmines and c-foam. Also include long range
    /// flashlight by popular demand.
    /// This distribution is intended to be applicable for any zone.
    /// </summary>
    public static ConsumableDistribution Baseline = new()
    {
        SpawnsPerZone = 6,
        SpawnData = Collections.Flatten(
            ItemSpawn.GlowSticks(8.0),
            new List<ItemSpawn> {
                new() { Weight = 8.0, Item = Items.Item.LockMelter },
                new() { Weight = 1.0, Item = Items.Item.CfoamGrenade },
                new() { Weight = 1.0, Item = Items.Item.ExplosiveTripmine },
                new() { Weight = 1.0, Item = Items.Item.LongRangeFlashlight }
            }
        ),
    };

    /// <summary>
    /// Baseline with heavy weighting to fog repellers. Averages to 4 repellers, 2 glowsticks,
    /// and 1 lock melter per zone.
    /// </summary>
    public static ConsumableDistribution Baseline_FogRepellers = new()
    {
        SpawnsPerZone = 8,
        SpawnData = Collections.Flatten(
            ItemSpawn.GlowSticks(3.0),
            new List<ItemSpawn> {
                new() { Weight = 1.0, Item = Items.Item.LockMelter },
                new() { Weight = 4.0, Item = Items.Item.FogRepeller }
            }
        )
    };

    /// <summary>
    /// Spawns a lot of fog repellers. We need this for certain alarm zones
    /// </summary>
    public static ConsumableDistribution Alarms_FogRepellers = new()
    {
        SpawnsPerZone = 12,
        SpawnData = Collections.Flatten(
            ItemSpawn.GlowSticks(),
            new List<ItemSpawn> {
                new() { Weight = 8.0, Item = Items.Item.FogRepeller }
            }
        )
    };

    /// <summary>
    /// Baseline with heavy weighting to fog repellers. Averages to 4 repellers, 2 glowsticks,
    /// and 1 lock melter per zone.
    /// </summary>
    public static ConsumableDistribution Reactor_FogRepellers = new()
    {
        SpawnsPerZone = 10,
        SpawnData = Collections.Flatten(
            ItemSpawn.GlowSticks(2.0),
            new List<ItemSpawn> {
                new() { Weight = 8.0, Item = Items.Item.FogRepeller }
            }
        )
    };

    public new static void SaveStatic()
    {
        Bins.ConsumableDistributions.AddBlock(Baseline);
        Bins.ConsumableDistributions.AddBlock(Baseline_FogRepellers);

        Bins.ConsumableDistributions.AddBlock(Alarms_FogRepellers);

        Bins.ConsumableDistributions.AddBlock(Reactor_FogRepellers);
    }

    public uint SpawnsPerZone { get; set; } = 5;

    public double ChanceToSpawnInResourceContainer { get; set; } = 0.9;

    public List<ItemSpawn> SpawnData { get; set; } = new List<ItemSpawn>();
}
