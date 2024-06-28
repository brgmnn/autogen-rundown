namespace AutogenRundown.DataBlocks.Objectives.Reactor
{
    public class ReactorWave
    {
        public double Warmup { get; set; } = 90.0;

        public double WarmupFail { get; set; } = 20.0;

        public double Wave { get; set; } = 60.0;

        /// <summary>
        /// Duration given to input the verification code the first time.
        /// </summary>
        public double Verify { get; set; } = 0.0;

        /// <summary>
        /// Duration given to input the verification code if the last wave was failed.
        /// </summary>
        public double VerifyFail { get; set; } = 45.0;

        public bool VerifyInOtherZone { get; set; } = false;

        public int ZoneForVerification { get; set; } = 0;

        public List<ReactorEnemyWave> EnemyWaves { get; set; } = new List<ReactorEnemyWave>();

        public List<WardenObjectiveEvent> Events { get; set; } = new List<WardenObjectiveEvent>();

        /// <summary>
        /// Recalculates the wave duration as well as rel spawn times for waves.
        ///
        /// Each wave is assigned a Duration, which is the estimated time required to clear that
        /// wave. Each wave also can be assigned a SpawnTime (in seconds), which is the start time
        /// after the reactor wave has started that the wave should begin spawning. The game
        /// supports overlaying waves on each other.
        ///
        /// GTFO uses SpawnTimeRel (not SpawnTime) to determine when to spawn the wave. This value
        /// is relative to the wave duration. As such this function is called to set the correct
        /// relative values for our desired exact second spawn times.
        /// </summary>
        public void RecalculateWaveSpawnTimes()
        {
            // Give a 45 second additional buffer for stragglers.
            Wave = 60 + EnemyWaves.Max(wave => wave.Duration + wave.SpawnTime);

            // Update each individual wave in the enemy waves to set the right SpawnTimeRel.
            foreach (var wave in EnemyWaves)
                wave.SpawnTimeRel = wave.SpawnTime / Wave;
        }
    }
}
