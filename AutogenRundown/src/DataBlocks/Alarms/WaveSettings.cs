using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Alarms
{
    internal enum VanillaWaveSettings : UInt32
    {
        None = 0,

        // Alarms?
        Apex = 1,
        ApexReducedShadows = 4,
        ApexReduced = 12,
        ApexS = 13,
        ApexIncreased = 38,

        // Surge alarms
        Surge = 21,

        // Scout Waves
        Scout_sspm = 3,

        // Error alarms
        Trickle_352_SSpB = 32,

        // Exit Trickles
        ExitTrickle_38S_Original = 5,
        ExitTrickle_18S = 11,
        ExitTrickle_410_S = 14,
        ExitTrickle_445_sspb = 15,
        ExitTrickle_46_b = 16,
        ExitTrickle_212_S = 20,

        // Reactor
        Reactor_24_sspmb = 7,
        Reactor_6_mb = 9,
    }

    /// <summary>
    /// Provides the settings for alarms, scout waves and similar types of waves (all referred to
    /// as "alarm" in sections below).
    ///
    /// 
    /// # Regarding population points and soft cap.
    /// 
    /// By default the game has some hardcoded values set that are used for score settings - cost
    /// of an enemy and soft cap.
    ///
    /// Enemy costs per type are the following: 0.75 1 1 2 2.
    ///
    /// Soft cap (MaxAllowedCost) is 25.
    ///
    /// All aggressive enemies count towards cap. If the remaining allowed cost is lower than the
    /// minimum required cost of a group, the group cannot spawn and the wave pauses until enough
    /// points are available. The enemy type here is determined in EnemyDataBlock.
    ///
    /// The enemy type for wave population point cost is determined by wave settings.
    /// 
    /// https://gtfo-modding.gitbook.io/wiki/reference/datablocks/main/survivalwavesettings
    /// </summary>
    internal record class WaveSettings : DataBlock
    {
        /// <summary>
        /// Delay before waves start spawning after alarm start.
        /// </summary>
        [JsonProperty("m_pauseBeforeStart")]
        public double PauseBeforeStart { get; set; } = 3.0;

        /// <summary>
        /// Delay between enemy groups.
        /// </summary>
        [JsonProperty("m_pauseBetweenGroups")]
        public double PauseBetweenGroups { get; set; } = 5.0;

        /// <summary>
        /// Minimum score boundary for pauses between waves.
        /// </summary>
        [JsonProperty("m_wavePauseMin_atCost")]
        public double WavePauseMin_atCost { get; set; } = 3.0;

        /// <summary>
        /// Maximum score boundary for pauses between waves.
        /// Above this threshold, the timer for a new wave doesn't move.
        /// Anywhere in-between min and max, the timer speed is lerped.
        /// </summary>
        [JsonProperty("m_wavePauseMax_atCost")]
        public double WavePauseMax_atCost { get; set; } = 10.0;

        /// <summary>
        /// Delay between waves at or below minimum score boundary.
        /// </summary>
        [JsonProperty("m_wavePauseMin")]
        public double WavePauseMin { get; set; } = 3.0;

        /// <summary>
        /// Delay between waves at maximum score boundary.
        /// </summary>
        [JsonProperty("m_wavePauseMax")]
        public double WavePauseMax { get; set; } = 30.0;

        /// <summary>
        /// List of enemy types in filter.
        /// </summary>
        [JsonProperty("m_populationFilter")]
        public List<uint> PopulationFilter { get; set; } = new List<uint>();

        /// <summary>
        /// Whether to spawn only, or spawn all but the types included in population filter.
        /// </summary>
        [JsonProperty("m_filterType")]
        public int FilterType { get; set; } = 0; // 0 or 1

        /// <summary>
        /// Chance for spawn direction to change between waves.
        /// </summary>
        [JsonProperty("m_chanceToRandomizeSpawnDirectionPerWave")]
        public double ChanceToRandomizeSpawnDirectionPerWave { get; set; } = 0.6;

        /// <summary>
        /// Change for spawn direction to change between groups.
        /// </summary>
        [JsonProperty("m_chanceToRandomizeSpawnDirectionPerGroup")]
        public double ChanceToRandomizeSpawnDirectionPerGroup { get; set; } = 0.1;

        /// <summary>
        /// Whether to override spawn type set in code.
        /// </summary>
        [JsonProperty("m_overrideWaveSpawnType")]
        public bool OverrideWaveSpawnType { get; set; } = false;

        /// <summary>
        /// The spawn type when override is set to true.
        /// </summary>
        [JsonProperty("m_survivalWaveSpawnType")]
        public int SurvivalWaveSpawnType { get; set; } = 0; // 0 or 1

        /// <summary>
        /// The total population points for waves. The alarm automatically stops if this runs out.
        /// -1 is infinite.
        /// </summary>
        [JsonProperty("m_populationPointsTotal")]
        public double PopulationPointsTotal { get; set; } = -1.0;

        /// <summary>
        /// Population points for a wave at start ramp.
        /// </summary>
        [JsonProperty("m_populationPointsPerWaveStart")]
        public double PopulationPointsPerWaveStart { get; set; } = 17.0;

        /// <summary>
        /// Population points for a wave at end ramp.
        /// </summary>
        [JsonProperty("m_populationPointsPerWaveEnd")]
        public double PopulationPointsPerWaveEnd { get; set; } = 25.0;

        /// <summary>
        /// Minimum required cost for a group to spawn. This setting is related to the soft cap
        /// of enemies.
        /// </summary>
        [JsonProperty("m_populationPointsMinPerGroup")]
        public double PopulationPointsMinPerGroup { get; set; } = 5.0;

        /// <summary>
        /// Population points for a group at start ramp.
        /// </summary>
        [JsonProperty("m_populationPointsPerGroupStart")]
        public double PopulationPointsPerGroupStart { get; set; } = 5.0;

        /// <summary>
        /// Population points for a group at end ramp.
        /// </summary>
        [JsonProperty("m_populationPointsPerGroupEnd")]
        public double PopulationPointsPerGroupEnd { get; set; } = 10.0;

        /// <summary>
        /// Lerp over time for start-end population point settings.
        /// </summary>
        [JsonProperty("m_populationRampOverTime")]
        public double PopulationRampOverTime { get; set; } = 120.0;
    }
}
