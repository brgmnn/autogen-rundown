using Newtonsoft.Json;

namespace MyFirstPlugin.DataBlocks.Alarms
{
    /// <summary>
    /// Persistent IDs for Survival Wave Populations
    /// </summary>
    enum WavePopulation : UInt32
    {
        None = 0,

        // Regular enemy pops
        Baseline = 1,
        Shooters = 3,
        BigStrikers = 4,
        Chargers = 5, // Called Bullrush
        Shadows = 7,
        Strikers = 18,

        // Shadows
        Shadows_BigsOnly = 38,

        BaselineHybrid = 8,
        BaselineHybrid_M_NoShooter = 11,
        Baseline_Sp_Shadows = 29,

        ModifiedSpHybrid = 9,
        ModifiedSpStrikerBig = 21,

        // Chargers [x] Confirmed
        Bullrush_mix = 17,
        Bullrush_mix2 = 19,
        Bullrush_mix3 = 24,
        Bullrush_mix4 = 13,
        Bullrush_mix5 = 14,
        Bullrush_mix6 = 36,
        Bullrush_mix7 = 40,
        BullrushBigs = 15,
        Baseline_M_Bullrush = 23, // M = maybe? medium?
        Bullrush_Only = 47,

        // Bigs
        BigsAndBosses = 22,
        BigsAndBosses_M_Hybrid = 25,
        BigsAndHybrid = 28,
        BigsAndBosses_v2 = 33,

        // Mother?
        BaselineBirther_MB = 12,
        Birther = 31,

        // Tank
        Tank = 16,

        // Grabber, Pouncer
        Pouncer = 56,
        WavePouncer = 39,
        WavePouncerCombo = 45,

        BigsAndBosses_S_Hybrid = 20, // S = small?

        // Reactor
        ReactorBaseline = 6,
        ReactorBaselineHybrid = 10,
        ReactorBaseline_S_Child = 44,

        // Flyers
        Flyers = 35,
        FlyersBig = 37,
        Boss_Flyer = 26,
        Modified_S_Flyer = 27
    }

    /// <summary>
    /// https://gtfo-modding.gitbook.io/wiki/reference/datablocks/main/chainedpuzzle
    /// </summary>
    internal class ChainedPuzzle : DataBlock
    {
        /// <summary>
        /// The Alarm name. For example, Class S Surge Alarm
        /// </summary>
        public string PublicAlarmName { get; set; } = "";

        /// <summary>
        /// Whether to trigger alarm when the puzzle starts. Typically set to false for scans
        /// without an alarm and enemy wave. However, you can set this field to true even if you 
        /// don't specify the enemy wave for the puzzle via the following fields. In that case, 
        /// there will still be alarm sound but no enemy waves.
        /// </summary>
        public bool TriggerAlarmOnActivate { get; set; } = false;

        /// <summary>
        /// Determine how the wave spawns. You set this field to make the wave either a relatively
        /// regular, surge, diminished, or ://ERROR! alarm wave.
        /// 
        /// You may take those wave settings already available in the vanilla datablocks into good 
        /// use.
        /// </summary>
        public VanillaWaveSettings SurvivalWaveSettings { get; set; } = 0;

        /// <summary>
        /// Determine what type(s) of enemy would spawn.
        /// </summary>
        public WavePopulation SurvivalWavePopulation { get; set; } = 0;

        /// <summary>
        /// Specify whether to stop the wave after Chained Puzzle Completion. Typically set to 
        /// false for ://ERROR! alarm.
        /// </summary>
        public bool DisableSurvivalWaveOnComplete { get; set; } = true;

        /// <summary>
        /// Whether to use random position for each scan. Usually set to true.
        /// 
        /// By setting this field to false, the scan position for each puzzle would be relatively
        /// static, i.e. it's not static all the time.
        /// </summary>
        public bool UseRandomPositions { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public double WantedDistanceFromStartPos { get; set; } = 0.0;

        /// <summary>
        /// 
        /// </summary>
        public double WantedDistanceBetweenPuzzleComponents { get; set; } = 5.0;

        /// <summary>
        /// Determines the count and types of scans.
        /// </summary>
        [JsonProperty("ChainedPuzzle")]
        public List<PuzzleComponent> Puzzle { get; set; } = new List<PuzzleComponent>();

        /// <summary>
        /// If set to true, the HUD won't show up if you are a certain distance (seems to be 25m) 
        /// away from the scan.
        /// 
        /// Typically set to false for regular scan, and true for extraction scan.
        /// </summary>
        public bool OnlyShowHUDWhenPlayerIsClose { get; set; } = true;

        #region Rarely set by us
        public int SurvivalWaveAreaDistance { get; set; } = 2;
        public UInt32 AlarmSoundStart = 3339129407u;
        public UInt32 AlarmSoundStop = 42633153u;
        #endregion

        /// <summary>
        /// Chained puzzle pack builders. Generates a pack of alarms that can be Pick()ed from
        /// for building levels.
        /// </summary>
        /// <param name="tier"></param>
        /// <returns></returns>
        public static List<ChainedPuzzle> BuildPack(string tier)
            => (tier) switch
            {
                ("A") => new List<ChainedPuzzle> 
                { 
                    None,
                    None,
                    TeamScan,
                    TeamScan,
                    TeamScan,
                    AlarmClass2,
                    AlarmClass2,
                    AlarmClass2,
                    AlarmClass2,
                    AlarmClass2,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass4,
                    AlarmClass4,
                },
                ("B") => new List<ChainedPuzzle> { AlarmClass2 },
                ("C") => new List<ChainedPuzzle> { AlarmClass2 },
                ("D") => new List<ChainedPuzzle> { AlarmClass2 },
                ("E") => new List<ChainedPuzzle> { AlarmClass2 },

                (_) => new List<ChainedPuzzle>(),
            };

        /*************************************** Presets ***************************************/
        /// <summary>
        /// Special chain puzzle that has no puzzles to enter
        /// </summary>
        public static ChainedPuzzle None = new ChainedPuzzle
        {
            PersistentId = 0
        };

        /******************** Non-Alarm Scans ********************/
        public static ChainedPuzzle TeamScan = new ChainedPuzzle
        {
            PublicAlarmName = "Scan",
            Puzzle = new List<PuzzleComponent> { PuzzleComponent.AllBig },
        };

        /******************** Alarm Scans ********************/
        public static ChainedPuzzle AlarmClass2 = new ChainedPuzzle
        {
            PublicAlarmName = "Class II",
            TriggerAlarmOnActivate = true,
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = WavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.ClusterSmall
            },
        };

        public static ChainedPuzzle AlarmClass3 = new ChainedPuzzle
        {
            PublicAlarmName = "Class III",
            TriggerAlarmOnActivate = true,
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = WavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall
            },
        };

        public static ChainedPuzzle AlarmClass4 = new ChainedPuzzle
        {
            PublicAlarmName = "Class IV",
            TriggerAlarmOnActivate = true,
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = WavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ScanLarge
            },
        };

        public static ChainedPuzzle AlarmClass5 = new ChainedPuzzle
        {
            PublicAlarmName = "Class IV",
            TriggerAlarmOnActivate = true,
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = WavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ScanLarge
            },
        };

        public static ChainedPuzzle AlarmClass3_Surge = new ChainedPuzzle
        {
            PublicAlarmName = "Class III Surge",
            TriggerAlarmOnActivate = true,
            SurvivalWaveSettings = VanillaWaveSettings.Surge,
            SurvivalWavePopulation = WavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>()
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.AllLarge,
                PuzzleComponent.AllLarge
            }
        };

        public static ChainedPuzzle AlarmClass4_Surge = new ChainedPuzzle
        {
            PublicAlarmName = "Class IV Surge",
            TriggerAlarmOnActivate = true,
            SurvivalWaveSettings = VanillaWaveSettings.Surge,
            SurvivalWavePopulation = WavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>()
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.AllLarge,
                PuzzleComponent.AllLarge,
                PuzzleComponent.AllLarge
            }
        };
    }
}
