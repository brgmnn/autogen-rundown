using AutogenRundown.DataBlocks.Alarms;

namespace AutogenRundown.DataBlocks.Enemies
{
    public class GenericWave
    {
        /// <summary>
        /// Exit trickle alarm for running to extraction at the end of the level.
        /// </summary>
        public static GenericWave ExitTrickle = new GenericWave
        {
            WaveSettings = (uint)VanillaWaveSettings.ExitTrickle_38S_Original,
            WavePopulation = (uint)VanillaWavePopulation.Baseline,
            SpawnDelay = 4.0,
            TriggerAlarm = true,
        };

        /// <summary>
        ///
        /// </summary>
        public static GenericWave Exit_Surge = new GenericWave
        {
            WaveSettings = Alarms.WaveSettings.Exit_Baseline.PersistentId,
        };

        #region Uplink Waves
        public static GenericWave Uplink_Easy = new GenericWave
        {
            WaveSettings = Alarms.WaveSettings.Baseline_Normal.PersistentId,
            WavePopulation = Alarms.WavePopulation.Baseline.PersistentId,
            SpawnDelay = 2.0,
            TriggerAlarm = true,
        };

        public static GenericWave Uplink_Medium = new GenericWave
        {
            WaveSettings = Alarms.WaveSettings.Baseline_Hard.PersistentId,
            WavePopulation = Alarms.WavePopulation.Baseline.PersistentId,
            SpawnDelay = 2.0,
            TriggerAlarm = true
        };
        #endregion

        #region Chargers
        public static GenericWave GiantChargers_35pts = new GenericWave
        {
            WaveSettings = Alarms.WaveSettings.SingleWave_35pts.PersistentId,
            WavePopulation = Alarms.WavePopulation.OnlyChargers.PersistentId
        };
        #endregion

        #region Survival objective waves
        public static GenericWave Survival_ErrorAlarm = new()
        {
            WaveSettings = Alarms.WaveSettings.Error_Normal.PersistentId,
            WavePopulation = Alarms.WavePopulation.Baseline.PersistentId
        };

        public static GenericWave Survival_Impossible_TankPotato = new()
        {
            WaveSettings = Alarms.WaveSettings.Survival_Impossible_MiniBoss.PersistentId,
            WavePopulation = Alarms.WavePopulation.SingleEnemy_TankPotato.PersistentId
        };
        #endregion

        #region Single enemy waves
        public static GenericWave SingleMother = new GenericWave
        {
            WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
            WavePopulation = Alarms.WavePopulation.SingleEnemy_Mother.PersistentId,
        };

        public static GenericWave SinglePMother = new GenericWave
        {
            WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
            WavePopulation = Alarms.WavePopulation.SingleEnemy_PMother.PersistentId,
        };

        public static GenericWave SingleTank = new GenericWave
        {
            WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
            WavePopulation = Alarms.WavePopulation.SingleEnemy_Tank.PersistentId,
        };

        public static GenericWave SingleTankPotato = new GenericWave
        {
            WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
            WavePopulation = Alarms.WavePopulation.SingleEnemy_TankPotato.PersistentId,
        };

        public static GenericWave SinglePouncer = new GenericWave
        {
            WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
            WavePopulation = Alarms.WavePopulation.SingleEnemy_Pouncer.PersistentId,
        };
        #endregion

        public uint WaveSettings { get; set; } = 0;

        public uint WavePopulation { get; set; } = 0;

        /// <summary>
        /// Room distance, in general this should always be left at 2.
        /// </summary>
        public int AreaDistance { get; set; } = 2;

        /// <summary>
        /// Delay in seconds before spawning the wave
        /// </summary>
        public double SpawnDelay { get; set; } = 0.0;

        /// <summary>
        /// Whether this should trigger an alarm
        /// </summary>
        public bool TriggerAlarm { get; set; } = false;

        /// <summary>
        /// Message to display
        /// </summary>
        public string IntelMessage { get; set; } = "";
    }
}
