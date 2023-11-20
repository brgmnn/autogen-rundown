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
            WaveSettings = (uint)VanillaWaveSettings.Apex,
            WavePopulation = (uint)Alarms.VanillaWavePopulation.Baseline,
            SpawnDelay = 2.0,
            TriggerAlarm = true,
        };

        public static GenericWave Uplink_Medium = new GenericWave
        {
            WaveSettings = (uint)VanillaWaveSettings.ApexIncreased,
            WavePopulation = (uint)Alarms.VanillaWavePopulation.Baseline,
            SpawnDelay = 2.0,
            TriggerAlarm = true
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
