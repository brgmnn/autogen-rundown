using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Objectives.Reactor
{
    public enum ReactorWaveSpawnType
    {
        ClosestToReactor = 0,
        InElevatorZone = 1,
    }

    public class ReactorEnemyWave
    {
        #region Preset waves
        public static ReactorEnemyWave Baseline_Easy = new()
        {
            WaveSettings = Alarms.WaveSettings.Reactor_Easy.PersistentId,
            WavePopulation = Alarms.WavePopulation.Baseline.PersistentId,
            Duration = 30
        };

        public static ReactorEnemyWave Baseline_Medium = new()
        {
            WaveSettings = Alarms.WaveSettings.Reactor_Medium.PersistentId,
            WavePopulation = Alarms.WavePopulation.Baseline.PersistentId,
            Duration = 40
        };

        public static ReactorEnemyWave Baseline_MediumMixed = new()
        {
            WaveSettings = Alarms.WaveSettings.Reactor_Medium.PersistentId,
            WavePopulation = Alarms.WavePopulation.Baseline.PersistentId,
            Duration = 40
        };

        public static ReactorEnemyWave Baseline_Hard = new()
        {
            WaveSettings = Alarms.WaveSettings.Reactor_Hard.PersistentId,
            WavePopulation = Alarms.WavePopulation.Baseline_Hybrid.PersistentId,
            Duration = 50
        };

        public static ReactorEnemyWave BaselineWithChargers_Hard = new()
        {
            WaveSettings = Alarms.WaveSettings.Reactor_Hard.PersistentId,
            WavePopulation = Alarms.WavePopulation.Baseline_Hybrid.PersistentId,
            Duration = 50
        };

        #region Charger waves
        public static ReactorEnemyWave OnlyChargers_Easy = new()
        {
            WaveSettings = Alarms.WaveSettings.ReactorChargers_Easy.PersistentId,
            WavePopulation = Alarms.WavePopulation.OnlyChargers.PersistentId,
            Duration = 20
        };

        public static ReactorEnemyWave OnlyChargers_Hard = new()
        {
            WaveSettings = Alarms.WaveSettings.ReactorChargers_Hard.PersistentId,
            WavePopulation = Alarms.WavePopulation.OnlyChargers.PersistentId,
            Duration = 30
        };
        #endregion

        #region Shadow waves
        public static ReactorEnemyWave OnlyShadows_Easy = new()
        {
            WaveSettings = Alarms.WaveSettings.ReactorShadows_Easy.PersistentId,
            WavePopulation = Alarms.WavePopulation.OnlyShadows.PersistentId,
            Duration = 20
        };

        public static ReactorEnemyWave OnlyShadows_Hard = new()
        {
            WaveSettings = Alarms.WaveSettings.ReactorShadows_Hard.PersistentId,
            WavePopulation = Alarms.WavePopulation.OnlyShadows.PersistentId,
            Duration = 30
        };
        #endregion

        #region Boss waves
        public static ReactorEnemyWave SingleMother = new()
        {
            WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
            WavePopulation = Alarms.WavePopulation.SingleEnemy_Mother.PersistentId,
            Duration = 45
        };

        public static ReactorEnemyWave SingleTank = new()
        {
            WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
            WavePopulation = Alarms.WavePopulation.SingleEnemy_Tank.PersistentId,
            Duration = 45
        };

        public static ReactorEnemyWave SinglePouncer = new()
        {
            WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
            WavePopulation = Alarms.WavePopulation.SingleEnemy_Pouncer.PersistentId,
            Duration = 25
        };
        #endregion
        #endregion

        /// <summary>
        /// Estimated duration required to complete this wave. Should be roughly equal to the
        /// number of points spawned.
        /// </summary>
        [JsonIgnore]
        public double Duration { get; set; } = 30.0;

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
