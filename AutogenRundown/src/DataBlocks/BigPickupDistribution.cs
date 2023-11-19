using AutogenRundown.DataBlocks.Items;

namespace AutogenRundown.DataBlocks
{
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

        public static new void SaveStatic()
        {
            Bins.BigPickupDistributions.AddBlock(FogTurbine);
        }

        public uint SpawnsPerZone { get; set; } = 0;

        public List<ItemSpawn> SpawnData { get; set; } = new List<ItemSpawn>();
    }
}
