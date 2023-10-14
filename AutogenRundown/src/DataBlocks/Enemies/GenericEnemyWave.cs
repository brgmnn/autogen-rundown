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
