namespace AutogenRundown.DataBlocks
{


    internal record class BigPickupDistribution : DataBlock
    {
        public uint SpawnsPerZone { get; set; } = 0;

        public List<BigPickupSpawnData> SpawnData { get; set; } = new List<BigPickupSpawnData>();
    }
}
