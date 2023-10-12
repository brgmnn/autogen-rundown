namespace AutogenRundown.DataBlocks.Objectives.Reactor
{
    enum ReactorWaveSpawnType
    {
        ClosestToReactor = 0,
        InElevatorZone = 1,
    }

    internal class ReactorEnemyWave
    {
        public uint WaveSettings { get; set; } = 0;

        public uint WavePopulation { get; set; } = 0;

        /// <summary>
        /// Room distance, in general this should always be left at 2.
        /// </summary>
        public int AreaDistance { get; set; } = 2;

        public double SpawnTimeRel { get; set; } = 0.0;

        public ReactorWaveSpawnType SpawnType { get; set; } = ReactorWaveSpawnType.ClosestToReactor;
    }
}
