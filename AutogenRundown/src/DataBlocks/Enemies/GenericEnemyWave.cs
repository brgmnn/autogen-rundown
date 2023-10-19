using AutogenRundown.DataBlocks.Alarms;

namespace AutogenRundown.DataBlocks.Enemies
{
    internal class GenericWave
    {
        /// <summary>
        /// Exit trickle alarm for running to extraction at the end of the level.
        /// </summary>
        public static GenericWave ExitTrickle = new GenericWave
        {
            WaveSettings = (uint)VanillaWaveSettings.ExitTrickle_38S_Original,
            WavePopulation = (uint)Alarms.WavePopulation.Baseline,
            SpawnDelay = 4.0,
            TriggerAlarm = true,
        };

        #region Uplink Waves
        public static GenericWave Uplink_Easy = new GenericWave
        {
            WaveSettings = (uint)VanillaWaveSettings.Apex,
            WavePopulation = (uint)Alarms.WavePopulation.Baseline,
            SpawnDelay = 2.0,
            TriggerAlarm = true,
        };

        public static GenericWave Uplink_Medium = new GenericWave
        {
            WaveSettings = (uint)VanillaWaveSettings.ApexIncreased,
            WavePopulation = (uint)Alarms.WavePopulation.Baseline,
            SpawnDelay = 2.0,
            TriggerAlarm = true
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
