using AutogenRundown.DataBlocks.Items;

namespace AutogenRundown.DataBlocks
{
    public record class ConsumableDistribution : DataBlock
    {
        /// <summary>
        /// Mostly glowsticks and lock melters, with a small chance of tripmines and c-foam.
        /// This distribution is intended to be applicable for any zone.
        /// </summary>
        public static ConsumableDistribution Baseline = new ConsumableDistribution
        {
            SpawnsPerZone = 6,
            SpawnData = Collections.Flatten(
                ItemSpawn.GlowSticks(8.0),
                new List<ItemSpawn> {
                    new ItemSpawn
                    {
                        Item = Items.Item.LockMelter,
                        Weight = 8.0
                    },
                    new ItemSpawn
                    {
                        Item = Items.Item.CfoamGrenade,
                        Weight = 1.0
                    },
                    new ItemSpawn
                    {
                        Item = Items.Item.ExplosiveTripmine,
                        Weight = 1.0
                    }
                }
            ),
        };

        /// <summary>
        /// Baseline with heavy weighting to fog repellers. Averages to 4 repellers, 2 glowsticks,
        /// and 1 lock melter per zone.
        /// </summary>
        public static ConsumableDistribution Baseline_FogRepellers = new ConsumableDistribution
        {
            SpawnsPerZone = 8,
            SpawnData = Collections.Flatten(
                ItemSpawn.GlowSticks(3.0),
                new List<ItemSpawn> {
                    new ItemSpawn
                    {
                        Item = Items.Item.LockMelter,
                        Weight = 1.0
                    },
                    new ItemSpawn
                    {
                        Item = Items.Item.FogRepeller,
                        Weight = 4.0
                    }
                }
            )
        };

        public static new void SaveStatic()
        {
            Bins.ConsumableDistributions.AddBlock(Baseline);
            Bins.ConsumableDistributions.AddBlock(Baseline_FogRepellers);
        }

        public uint SpawnsPerZone { get; set; } = 5;

        public double ChanceToSpawnInResourceContainer { get; set; } = 0.9;

        public List<ItemSpawn> SpawnData { get; set; } = new List<ItemSpawn>();
    }
}
