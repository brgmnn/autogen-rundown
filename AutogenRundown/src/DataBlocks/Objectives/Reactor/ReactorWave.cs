namespace AutogenRundown.DataBlocks.Objectives.Reactor
{
    internal class ReactorWave
    {
        public double Warmup { get; set; } = 90.0;

        public double WarmupFail { get; set; } = 20.0;

        public double Wave { get; set; } = 60.0;

        public double Verify { get; set; } = 0.0;

        public double VerifyFail { get; set; } = 45.0;

        public bool VerifyInOtherZone { get; set; } = false;

        public int ZoneForVerification { get; set; } = 0;

        public List<ReactorEnemyWave> EnemyWaves { get; set; } = new List<ReactorEnemyWave>();

        public List<WardenObjectiveEvent> Events { get; set; } = new List<WardenObjectiveEvent>();
    }
}
