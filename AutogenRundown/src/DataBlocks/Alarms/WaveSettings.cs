using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Alarms
{
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
    ///     - Weakling = 0.75
    ///     - Standard = 1
    ///     - Special  = 1
    ///     - MiniBoss = 2
    ///     - Boss     = 2
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
    public record WaveSettings : DataBlock
    {
        #region Properties
        /// <summary>
        /// Delay before waves start spawning after alarm start.
        ///
        /// Default = 3.0
        /// </summary>
        [JsonProperty("m_pauseBeforeStart")]
        public double PauseBeforeStart { get; set; } = 3.0;

        /// <summary>
        /// Delay between enemy groups.
        ///
        /// Default = 5.0
        /// </summary>
        [JsonProperty("m_pauseBetweenGroups")]
        public double PauseBetweenGroups { get; set; } = 5.0;

        /// <summary>
        /// Minimum score boundary for pauses between waves.
        ///
        /// Default = 3.0
        /// </summary>
        [JsonProperty("m_wavePauseMin_atCost")]
        public double WavePauseMin_atCost { get; set; } = 3.0;

        /// <summary>
        /// Maximum score boundary for pauses between waves.
        /// Above this threshold, the timer for a new wave doesn't move.
        /// Anywhere in-between min and max, the timer speed is lerped.
        ///
        /// Default = 10.0
        /// </summary>
        [JsonProperty("m_wavePauseMax_atCost")]
        public double WavePauseMax_atCost { get; set; } = 10.0;

        /// <summary>
        /// Delay between waves at or below minimum score boundary.
        ///
        /// Default = 3.0
        /// </summary>
        [JsonProperty("m_wavePauseMin")]
        public double WavePauseMin { get; set; } = 3.0;

        /// <summary>
        /// Delay between waves at maximum score boundary.
        ///
        /// Default = 30.0
        /// </summary>
        [JsonProperty("m_wavePauseMax")]
        public double WavePauseMax { get; set; } = 30.0;

        /// <summary>
        /// List of enemy types in filter.
        ///
        /// Default = []
        /// </summary>
        [JsonProperty("m_populationFilter")]
        public List<Enemies.EnemyType> PopulationFilter { get; set; } = new();

        /// <summary>
        /// Whether to spawn only, or spawn all but the types included in population filter.
        ///
        /// Default = Include (0)
        /// </summary>
        [JsonProperty("m_filterType")]
        public PopulationFilterType FilterType { get; set; } = PopulationFilterType.Include;

        /// <summary>
        /// Chance for spawn direction to change between waves.
        ///
        /// Default = 0.75
        /// </summary>
        [JsonProperty("m_chanceToRandomizeSpawnDirectionPerWave")]
        public double ChanceToRandomizeSpawnDirectionPerWave { get; set; } = 0.75;

        /// <summary>
        /// Change for spawn direction to change between groups.
        ///
        /// Default = 0.1
        /// </summary>
        [JsonProperty("m_chanceToRandomizeSpawnDirectionPerGroup")]
        public double ChanceToRandomizeSpawnDirectionPerGroup { get; set; } = 0.1;

        /// <summary>
        /// Whether to override spawn type set in code.
        ///
        /// Default = false
        /// </summary>
        [JsonProperty("m_overrideWaveSpawnType")]
        public bool OverrideWaveSpawnType { get; set; } = false;

        /// <summary>
        /// The spawn type when override is set to true.
        ///
        /// Default = 0
        /// </summary>
        [JsonProperty("m_survivalWaveSpawnType")]
        public int SurvivalWaveSpawnType { get; set; } = 0; // 0 or 1

        /// <summary>
        /// The total population points for waves. The alarm automatically stops if this runs out.
        /// -1 is infinite.
        ///
        /// Default = -1
        /// </summary>
        [JsonProperty("m_populationPointsTotal")]
        public double PopulationPointsTotal { get; set; } = -1.0;

        /// <summary>
        /// Population points for a wave at start ramp.
        ///
        /// Default = 17
        /// </summary>
        [JsonProperty("m_populationPointsPerWaveStart")]
        public double PopulationPointsPerWaveStart { get; set; } = 17.0;

        /// <summary>
        /// Population points for a wave at end ramp.
        ///
        /// Default = 25
        /// </summary>
        [JsonProperty("m_populationPointsPerWaveEnd")]
        public double PopulationPointsPerWaveEnd { get; set; } = 25.0;

        /// <summary>
        /// Minimum required cost for a group to spawn. This setting is related to the soft cap
        /// of enemies.
        ///
        /// Default = 5
        /// </summary>
        [JsonProperty("m_populationPointsMinPerGroup")]
        public double PopulationPointsMinPerGroup { get; set; } = 5.0;

        /// <summary>
        /// Population points for a group at start ramp.
        ///
        /// Default = 5
        /// </summary>
        [JsonProperty("m_populationPointsPerGroupStart")]
        public double PopulationPointsPerGroupStart { get; set; } = 5.0;

        /// <summary>
        /// Population points for a group at end ramp.
        ///
        /// Default = 10.0
        /// </summary>
        [JsonProperty("m_populationPointsPerGroupEnd")]
        public double PopulationPointsPerGroupEnd { get; set; } = 10.0;

        /// <summary>
        /// Lerp over time for start-end population point settings.
        ///
        /// Default = 120.0
        /// </summary>
        [JsonProperty("m_populationRampOverTime")]
        public double PopulationRampOverTime { get; set; } = 120.0;
        #endregion

        public WaveSettings(PidOffsets offsets = PidOffsets.WaveSettings)
            : base(Generator.GetPersistentId(offsets))
        { }

        /*public static void Setup()
            => Setup<GameDataWaveSettings, WaveSettings>(Bins.WaveSettings, "SurvivalWaveSettings");*/
        public static void Setup() { }

        public override string ToString()
            => $"WaveSettings {{ Name = {Name}, PersistentId = {PersistentId} }}";

        public void Persist(BlocksBin<WaveSettings>? bin = null)
        {
            bin ??= Bins.WaveSettings;
            bin.AddBlock(this);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lights"></param>
        /// <returns></returns>
        public static WaveSettings FindOrPersist(WaveSettings settings)
        {
            // We specifically don't want to persist none, as we want to set the PersistentID to 0
            if (settings == None)
                return None;

            var existing = Bins.WaveSettings.GetBlock(settings);

            if (existing != null)
                return existing;

            if (settings.PersistentId == 0)
                settings.PersistentId = Generator.GetPersistentId(PidOffsets.WaveSettings);

            settings.Persist();

            return settings;
        }

        /// <summary>
        /// Instance version of static method
        /// </summary>
        /// <returns></returns>
        public WaveSettings FindOrPersist() => FindOrPersist(this);

        /// <summary>
        /// Return a DrawSelect list of wave settings to attach on alarms. Pack is for one LevelLayout. So we need probably
        /// 30 entries to draw from
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static List<(double, int, WaveSettings)> BuildPack(string tier)
        {
            return (tier) switch
            {
                "A" => new List<(double, int, WaveSettings)>
                {
                    (1.00, 15, Baseline_Easy),
                    (1.00, 15, Baseline_Normal),
                },

                "B" => new List<(double, int, WaveSettings)>
                {
                    (1.00, 15, Baseline_Normal),
                    (1.00, 15, Baseline_Hard),
                },

                "C" => new List<(double, int, WaveSettings)>
                {
                    (1.00,  5, Baseline_Normal),
                    (1.00, 20, Baseline_Hard),
                    (0.65,  5, Baseline_VeryHard),
                },

                "D" => new List<(double, int, WaveSettings)>
                {
                    (1.00, 15, Baseline_Hard),
                    (1.00, 10, Baseline_VeryHard),
                    (0.55, 5, MiniBoss_Hard),
                },

                "E" => new List<(double, int, WaveSettings)>
                {
                    (1.00, 5, Baseline_Hard),
                    (1.00, 20, Baseline_VeryHard),
                    (0.55, 5, MiniBoss_Hard),
                },
            };
        }

        public new static void SaveStatic()
        {
            // Alarms
            Bins.WaveSettings.AddBlock(Baseline_Easy);
            Bins.WaveSettings.AddBlock(Baseline_Normal);
            Bins.WaveSettings.AddBlock(Baseline_Hard);
            Bins.WaveSettings.AddBlock(Baseline_VeryHard);
            Bins.WaveSettings.AddBlock(MiniBoss_Hard);

            // Error
            Bins.WaveSettings.AddBlock(Error_Easy);
            Bins.WaveSettings.AddBlock(Error_Normal);

            // Exit
            Bins.WaveSettings.AddBlock(Exit_Baseline);
            Bins.WaveSettings.AddBlock(Exit_Objective_Easy);
            Bins.WaveSettings.AddBlock(Exit_Objective_Medium);
            Bins.WaveSettings.AddBlock(Exit_Objective_Hard);
            Bins.WaveSettings.AddBlock(Exit_Objective_VeryHard);

            // Surge
            Bins.WaveSettings.AddBlock(Surge);

            // Scout
            Bins.WaveSettings.AddBlock(Scout_Easy);

            // Reactor
            Bins.WaveSettings.AddBlock(Reactor_Easy);
            Bins.WaveSettings.AddBlock(Reactor_Medium);
            Bins.WaveSettings.AddBlock(Reactor_Hard);
            Bins.WaveSettings.AddBlock(ReactorChargers_Easy);
            Bins.WaveSettings.AddBlock(ReactorChargers_Hard);
            Bins.WaveSettings.AddBlock(ReactorHybrids_Medium);
            Bins.WaveSettings.AddBlock(ReactorShadows_Easy);
            Bins.WaveSettings.AddBlock(ReactorShadows_Hard);

            Bins.WaveSettings.AddBlock(ReactorPoints_Special_16pts);

            // Survival
            Bins.WaveSettings.AddBlock(Survival_Impossible_MiniBoss);

            // Fixed points (multiple waves)
            Bins.WaveSettings.AddBlock(Finite_35pts_Hard);

            // Single waves
            Bins.WaveSettings.AddBlock(SingleWave_8pts);
            Bins.WaveSettings.AddBlock(SingleWave_12pts);
            Bins.WaveSettings.AddBlock(SingleWave_16pts);
            Bins.WaveSettings.AddBlock(SingleWave_20pts);
            Bins.WaveSettings.AddBlock(SingleWave_28pts);
            Bins.WaveSettings.AddBlock(SingleWave_35pts);

            // Single enemy spawn
            Bins.WaveSettings.AddBlock(SingleMiniBoss);
        }

        public static readonly WaveSettings None = new() { PersistentId = 0, Name = "None" };

        #region Alarm waves -- Baseline
        public static WaveSettings Baseline_Easy = new()
        {
            PopulationFilter =
            {
                Enemies.EnemyType.Weakling,
                Enemies.EnemyType.MiniBoss,
                Enemies.EnemyType.Boss
            },
            FilterType = PopulationFilterType.Exclude,
            PopulationPointsPerWaveStart = 15,
            PopulationPointsPerWaveEnd = 25,
            PopulationRampOverTime = 200,
            Name = "Baseline_Easy"
        };

        /// <summary>
        /// Should be a good choice for many alarms. This is equivalent to Apex in the base game
        /// </summary>
        public static WaveSettings Baseline_Normal = new()
        {
            PopulationFilter = { Enemies.EnemyType.Weakling, Enemies.EnemyType.Boss },
            FilterType = PopulationFilterType.Exclude,
            PopulationPointsPerWaveStart = 17,
            PopulationPointsPerWaveEnd = 25,
            PopulationRampOverTime = 150,
            Name = "Baseline_Normal"
        };

        /// <summary>
        /// Harder choice, all enemy types can be included here. Will ramp up much faster than the others
        /// </summary>
        public static WaveSettings Baseline_Hard = new WaveSettings
        {
            PopulationFilter = { Enemies.EnemyType.Weakling },
            FilterType = PopulationFilterType.Exclude,
            PopulationPointsPerWaveStart = 18,
            PopulationPointsPerWaveEnd = 28,
            PopulationRampOverTime = 100,
            Name = "Baseline_Hard"
        };

        /// <summary>
        /// Harder choice, all enemy types can be included here. Will ramp up much faster than the others
        /// </summary>
        public static WaveSettings Baseline_VeryHard = new WaveSettings
        {
            PopulationFilter = { Enemies.EnemyType.Weakling },
            FilterType = PopulationFilterType.Exclude,
            PopulationPointsPerWaveStart = 25,
            PopulationPointsPerWaveEnd = 30,
            PopulationRampOverTime = 45,
            Name = "Baseline_VeryHard"
        };
        #endregion

        #region MiniBoss_Hard
        /// <summary>
        /// This is a hard miniboss (and special) only wave
        /// </summary>
        public static WaveSettings MiniBoss_Hard = new WaveSettings
        {
            PopulationFilter = { Enemies.EnemyType.Special, Enemies.EnemyType.MiniBoss },
            FilterType = PopulationFilterType.Include,
            PopulationPointsPerWaveStart = 18,
            PopulationPointsPerWaveEnd = 28,
            PopulationRampOverTime = 100,
            Name = "MiniBoss_Hard"
        };
        #endregion

        #region Error Alarms
        /// <summary>
        /// Somewhat equavlent to PersistentId=32 "Trickle 3-52 SSpB"
        ///
        /// Quite an easy error alarm.
        /// -> 3pts of enemies every 52 seconds.
        ///
        /// This should be very easy for one player to fully manage with just a hammer by
        /// themselves while the rest of the team continues with their objectives
        /// </summary>
        public static readonly WaveSettings Error_Easy = new()
        {
            PopulationFilter =
            {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special
            },
            FilterType = PopulationFilterType.Include,
            PauseBeforeStart = 3.0,
            PauseBetweenGroups = 52.0, // This is the key item, 52s between spawns

            PopulationPointsPerGroupStart = 3.0,
            PopulationPointsPerGroupEnd = 3.0,
            PopulationPointsMinPerGroup = 2.0,
            PopulationRampOverTime = 0,

            // This controls how many points of enemies. We don't want any ramping here.
            PopulationPointsPerWaveStart = 3.0,
            PopulationPointsPerWaveEnd = 3.0,

            ChanceToRandomizeSpawnDirectionPerGroup = 0.8,
            ChanceToRandomizeSpawnDirectionPerWave = 1.0,

            WavePauseMin = 1.0,
            WavePauseMax = 20.0,
            WavePauseMin_atCost = 1.0,
            WavePauseMax_atCost = 10.0,

            Name = "Error_Easy"
        };

        /// <summary>
        /// Harder than easy. This shouldn't feel relaxed to deal with
        /// </summary>
        public static WaveSettings Error_Normal = Error_Easy with
        {
            PopulationFilter = new()
            {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
                Enemies.EnemyType.MiniBoss,
            },
            PauseBetweenGroups = 45,
            PopulationPointsPerGroupStart = 4.0,
            PopulationPointsPerGroupEnd = 4.0,
            PopulationPointsPerWaveStart = 4.0,
            PopulationPointsPerWaveEnd = 4.0,

            Name = "Error_Normal"
        };
        #endregion

        #region Objective/Exit Error Alarms
        public static WaveSettings Exit_Baseline = new WaveSettings
        {
            PopulationFilter =
            {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
            },
            FilterType = PopulationFilterType.Include,
            PauseBeforeStart = 4.0,
            PauseBetweenGroups = 8.0,

            Name = "Exit_Baseline"
        };

        public static WaveSettings Exit_Objective_Easy = new()
        {
            PopulationFilter = { Enemies.EnemyType.Standard },
            FilterType = PopulationFilterType.Include,

            PauseBeforeStart = 2.0,
            PauseBetweenGroups = 12.0,
            PopulationPointsPerWaveStart = 2.0,
            PopulationPointsPerWaveEnd = 4.0,
            PopulationPointsMinPerGroup = 2,
            PopulationPointsPerGroupStart = 2,
            PopulationPointsPerGroupEnd = 4,
            PopulationRampOverTime = 240,
        };

        public static WaveSettings Exit_Objective_Medium = new()
        {
            PopulationFilter =
            {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special
            },
            FilterType = PopulationFilterType.Include,

            PauseBeforeStart = 2.0,
            PauseBetweenGroups = 12.0,
            PopulationPointsPerWaveStart = 3.0,
            PopulationPointsPerWaveEnd = 6.0,
            PopulationPointsMinPerGroup = 2,
            PopulationPointsPerGroupStart = 3,
            PopulationPointsPerGroupEnd = 6,
            PopulationRampOverTime = 180,
        };

        public static WaveSettings Exit_Objective_Hard = new()
        {
            PopulationFilter = { Enemies.EnemyType.Weakling, Enemies.EnemyType.Boss },
            FilterType = PopulationFilterType.Exclude,

            PauseBeforeStart = 2.0,
            PauseBetweenGroups = 12.0,
            PopulationPointsPerWaveStart = 5.0,
            PopulationPointsPerWaveEnd = 8.0,
            PopulationPointsMinPerGroup = 3,
            PopulationPointsPerGroupStart = 3,
            PopulationPointsPerGroupEnd = 5,
            PopulationRampOverTime = 180,
        };

        public static WaveSettings Exit_Objective_VeryHard = new()
        {
            PopulationFilter = { Enemies.EnemyType.Weakling },
            FilterType = PopulationFilterType.Exclude,

            PauseBeforeStart = 2.0,
            PauseBetweenGroups = 12.0,

            PopulationPointsPerWaveStart = 6.0,
            PopulationPointsPerWaveEnd = 10.0,
            PopulationPointsMinPerGroup = 3,
            PopulationPointsPerGroupStart = 4,
            PopulationPointsPerGroupEnd = 6,
            PopulationRampOverTime = 180,
        };
        #endregion

        #region Finite points value
        public static WaveSettings Finite_35pts_Hard = new WaveSettings
        {
            PopulationFilter = { Enemies.EnemyType.Weakling },
            FilterType = PopulationFilterType.Exclude,
            PauseBeforeStart = 3.0,

            PopulationPointsPerGroupStart = 7,
            PopulationPointsPerGroupEnd = 7,
            PopulationPointsMinPerGroup = 4,
            PopulationRampOverTime = 45.0,

            PopulationPointsTotal = 35,
            PopulationPointsPerWaveStart = 7,
            PopulationPointsPerWaveEnd = 14,

            ChanceToRandomizeSpawnDirectionPerGroup = 0.8,
            ChanceToRandomizeSpawnDirectionPerWave = 1.0,

            WavePauseMin = 1.0,
            WavePauseMax = 20.0,
            WavePauseMin_atCost = 1.0,
            WavePauseMax_atCost = 10.0,

            Name = "Finite_35pts_Hard"
        };
        #endregion

        #region Surge
        /// <summary>
        /// Surge alarms are very difficult as they flood the map with enemies immediately.
        /// Don't expect teams to be able to clear alarms beyond one or two scans if they have
        /// to clear enemies. Further scans can be cleared if the geomorphs allow strategies such
        /// as C-Foam holding of doors
        /// </summary>
        public static WaveSettings Surge = new WaveSettings
        {
            PopulationFilter =
            {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
            },
            FilterType = PopulationFilterType.Include,

            PauseBeforeStart = 1.0,
            PauseBetweenGroups = 3.0,
            PopulationPointsPerWaveStart = 10_000,
            PopulationPointsPerWaveEnd = 10_000,
            PopulationPointsMinPerGroup = 2,
            PopulationPointsPerGroupStart = 4,
            PopulationPointsPerGroupEnd = 7,
            PopulationRampOverTime = 0,

            Name = "Surge"
        };
        #endregion

        #region Scout
        /// <summary>
        /// Equivalent to PersistentId=32 "Trickle 3-52 SSpB"
        ///
        /// Quite an easy error alarm.
        /// </summary>
        public static WaveSettings Scout_Easy = new WaveSettings
        {
            PopulationFilter =
            {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
                Enemies.EnemyType.MiniBoss,
            },
            FilterType = PopulationFilterType.Include,

            PauseBeforeStart = 1.0,
            PauseBetweenGroups = 3.0,

            PopulationPointsPerGroupStart = 5.0,
            PopulationPointsPerGroupEnd = 5.0,
            PopulationPointsMinPerGroup = 3.0,

            PopulationPointsPerWaveStart = 12,
            PopulationPointsPerWaveEnd = 12,
            PopulationPointsTotal = 12,

            WavePauseMin = 1.0,
            WavePauseMax = 20.0,
            WavePauseMin_atCost = 1.0,
            WavePauseMax_atCost = 10.0,

            Name = "Scout_Easy"
        };
        #endregion

        #region === Reactor Waves ===

        #region Reactor Waves -- General Pop
        /// <summary>
        /// Can be a fairly gentle reactor wave depending on the population selected.
        /// Only spawns standard and specials.
        /// </summary>
        public static WaveSettings Reactor_Easy = new WaveSettings
        {
            PopulationFilter =
            {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
            },

            PopulationPointsTotal = 30,
            PopulationPointsPerWaveStart = 25,
            PopulationPointsPerWaveEnd = 25,
            PopulationPointsMinPerGroup = 5,
            PopulationPointsPerGroupStart = 5,
            PopulationPointsPerGroupEnd = 10,
            PopulationRampOverTime = 40
        };

        public static WaveSettings Reactor_Medium = new WaveSettings
        {
            PopulationFilter =
            {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
                Enemies.EnemyType.MiniBoss
            },

            PopulationPointsTotal = 40,
            PopulationPointsPerWaveStart = 25,
            PopulationPointsPerWaveEnd = 25,
            PopulationPointsMinPerGroup = 5,
            PopulationPointsPerGroupStart = 5,
            PopulationPointsPerGroupEnd = 15,
            PopulationRampOverTime = 40
        };

        public static WaveSettings Reactor_Hard = new WaveSettings
        {
            PopulationFilter =
            {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
                Enemies.EnemyType.MiniBoss,
                Enemies.EnemyType.Boss
            },

            PopulationPointsTotal = 50,
            PopulationPointsPerWaveStart = 25,
            PopulationPointsPerWaveEnd = 25,
            PopulationPointsMinPerGroup = 5,
            PopulationPointsPerGroupStart = 5,
            PopulationPointsPerGroupEnd = 20,
            PopulationRampOverTime = 30
        };
        #endregion

        #region Reactor Waves -- Hybrids
        public static WaveSettings ReactorHybrids_Medium = new WaveSettings
        {
            PopulationFilter = { Enemies.EnemyType.Special },

            PopulationPointsTotal = 16,
            PopulationPointsPerWaveStart = 25,
            PopulationPointsPerWaveEnd = 25,
            PopulationPointsMinPerGroup = 8,
            PopulationPointsPerGroupStart = 8,
            PopulationPointsPerGroupEnd = 20,
            PopulationRampOverTime = 50
        };

        public static WaveSettings ReactorHybrids_Group = new WaveSettings
        {
            PopulationFilter = { Enemies.EnemyType.Special },

            PopulationPointsTotal = 20,
            PopulationPointsPerWaveStart = 20,
            PopulationPointsPerWaveEnd = 20,
            PopulationPointsMinPerGroup = 8,
            PopulationPointsPerGroupStart = 8,
            PopulationPointsPerGroupEnd = 20,
            PopulationRampOverTime = 25
        };
        #endregion

        #region Reactor Waves -- Points Groups
        public static WaveSettings ReactorPoints_Special_16pts = new WaveSettings
        {
            PopulationFilter = { Enemies.EnemyType.Special },

            PopulationPointsTotal = 16,
            PopulationPointsPerWaveStart = 16,
            PopulationPointsPerWaveEnd = 16,
            PopulationPointsMinPerGroup = 16,
            PopulationPointsPerGroupStart = 16,
            PopulationPointsPerGroupEnd = 16,
            PopulationRampOverTime = 10
        };
        #endregion

        #region Reactor Waves -- Chargers
        public static WaveSettings ReactorChargers_Easy = new WaveSettings
        {
            PopulationFilter = { Enemies.EnemyType.Standard },
            PopulationPointsTotal = 14,
            PopulationPointsPerWaveStart = 14,
            PopulationPointsPerWaveEnd = 14,
            PopulationPointsMinPerGroup = 5,
            PopulationPointsPerGroupStart = 5,
            PopulationPointsPerGroupEnd = 10,
            PopulationRampOverTime = 40
        };

        public static WaveSettings ReactorChargers_Hard = new WaveSettings
        {
            PopulationFilter =
            {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.MiniBoss,
            },
            PopulationPointsTotal = 25,
            PopulationPointsPerWaveStart = 25,
            PopulationPointsPerWaveEnd = 25,
            PopulationPointsMinPerGroup = 4,
            PopulationPointsPerGroupStart = 5,
            PopulationPointsPerGroupEnd = 15,
            PopulationRampOverTime = 40
        };
        #endregion

        #region Reactor Waves -- Shadows
        public static WaveSettings ReactorShadows_Easy = new WaveSettings
        {
            PopulationFilter = { Enemies.EnemyType.Standard },
            PopulationPointsTotal = 20,
            PopulationPointsPerWaveStart = 20,
            PopulationPointsPerWaveEnd = 20,
            PopulationPointsMinPerGroup = 5,
            PopulationPointsPerGroupStart = 5,
            PopulationPointsPerGroupEnd = 10,
            PopulationRampOverTime = 50
        };

        public static WaveSettings ReactorShadows_Hard = new WaveSettings
        {
            PopulationFilter =
            {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.MiniBoss,
            },
            PopulationPointsTotal = 25,
            PopulationPointsPerWaveStart = 25,
            PopulationPointsPerWaveEnd = 25,
            PopulationPointsMinPerGroup = 5,
            PopulationPointsPerGroupStart = 5,
            PopulationPointsPerGroupEnd = 20,
            PopulationRampOverTime = 30
        };
        #endregion
        #endregion

        #region === Survival Waves ===
        public static WaveSettings Survival_Impossible_MiniBoss = new()
        {
            PopulationFilter = { Enemies.EnemyType.MiniBoss },
            PauseBeforeStart = 1.0,
            PauseBetweenGroups = 30.0,
            PopulationPointsTotal = -1,
            PopulationPointsMinPerGroup = 4,
            PopulationPointsPerGroupStart = 4,
            PopulationPointsPerGroupEnd = 12,
            PopulationPointsPerWaveEnd = 12,
            WavePauseMin = 1,
            WavePauseMax = 25,
            WavePauseMin_atCost = 1,
            WavePauseMax_atCost = 25,
        };
        #endregion

        #region Single wave spawns
        public static WaveSettings SingleWave_8pts = new WaveSettings
        {
            PopulationFilter = {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
                Enemies.EnemyType.MiniBoss
            },
            FilterType = PopulationFilterType.Include,

            PopulationPointsTotal = 8,
        };

        public static WaveSettings SingleWave_12pts = new WaveSettings
        {
            PopulationFilter = {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
                Enemies.EnemyType.MiniBoss
            },
            FilterType = PopulationFilterType.Include,

            PopulationPointsTotal = 12,
        };

        public static WaveSettings SingleWave_16pts = new WaveSettings
        {
            PopulationFilter = {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
                Enemies.EnemyType.MiniBoss
            },
            FilterType = PopulationFilterType.Include,

            PopulationPointsTotal = 16,
        };

        public static WaveSettings SingleWave_20pts = new WaveSettings
        {
            PopulationFilter = {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
                Enemies.EnemyType.MiniBoss
            },
            FilterType = PopulationFilterType.Include,

            PopulationPointsTotal = 20,
        };

        public static WaveSettings SingleWave_28pts = new WaveSettings
        {
            PopulationFilter = {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
                Enemies.EnemyType.MiniBoss
            },
            FilterType = PopulationFilterType.Include,

            PopulationPointsTotal = 28,
        };

        public static WaveSettings SingleWave_35pts = new WaveSettings
        {
            PopulationFilter = {
                Enemies.EnemyType.Standard,
                Enemies.EnemyType.Special,
                Enemies.EnemyType.MiniBoss
            },
            FilterType = PopulationFilterType.Include,

            PopulationPointsTotal = 35,
        };
        #endregion

        #region Single enemy spawns
        public static WaveSettings SingleMiniBoss = new WaveSettings
        {
            PopulationFilter = { Enemies.EnemyType.MiniBoss },
            PauseBeforeStart = 1.0,
            PauseBetweenGroups = 1,
            PopulationPointsTotal = 1,
            PopulationPointsMinPerGroup = 1,
            PopulationPointsPerGroupStart = 1,
            PopulationPointsPerGroupEnd = 1,
            PopulationPointsPerWaveEnd = 1,
            WavePauseMin = 1,
            WavePauseMax = 25,
            WavePauseMin_atCost = 1,
            WavePauseMax_atCost = 25,
        };
        #endregion
    }

    public record GameDataWaveSettings : WaveSettings
    {
        /// <summary>
        /// We explicitly want to not have PIDs set when loading data, they come with their own
        /// Pids. This stops the Pid counter getting incremented when loading vanilla data.
        /// </summary>
        public GameDataWaveSettings() : base(PidOffsets.None)
        { }
    }
}
