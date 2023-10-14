using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Alarms
{
    /// <summary>
    /// https://gtfo-modding.gitbook.io/wiki/reference/datablocks/main/chainedpuzzle
    /// </summary>
    internal record class ChainedPuzzle : DataBlock
    {
        /// <summary>
        /// The Alarm name. For example, Class S Surge Alarm
        /// </summary>
        public string PublicAlarmName { get; set; } = "Alarm";

        /// <summary>
        /// Whether to trigger alarm when the puzzle starts. Typically set to false for scans
        /// without an alarm and enemy wave. However, you can set this field to true even if you 
        /// don't specify the enemy wave for the puzzle via the following fields. In that case, 
        /// there will still be alarm sound but no enemy waves.
        /// </summary>
        public bool TriggerAlarmOnActivate { get; set; } = true;

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

        #region Pack builders
        /// <summary>
        /// Chained puzzle pack builders. Generates a pack of alarms that can be Pick()ed from
        /// for building levels.
        /// </summary>
        /// <param name="tier"></param>
        /// <returns></returns>
        public static List<ChainedPuzzle> BuildPack(string tier)
            => (tier) switch
            {
                "A" => new List<ChainedPuzzle>
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
                "B" => new List<ChainedPuzzle>
                {
                    None,
                    None,
                    TeamScan,
                    TeamScan,
                    TeamScan,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass5,
                },
                "C" => new List<ChainedPuzzle>
                {
                    TeamScan,
                    TeamScan,
                    TeamScan,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass5,
                    AlarmClass5,
                    AlarmClass5,
                },
                "D" => new List<ChainedPuzzle>
                {
                    TeamScan,
                    TeamScan,
                    TeamScan,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass3,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass5,
                    AlarmClass5,
                    AlarmClass5,
                },
                "E" => new List<ChainedPuzzle>
                {
                    TeamScan,
                    TeamScan,
                    TeamScan,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass4,
                    AlarmClass5,
                    AlarmClass5,
                    AlarmClass5,
                    AlarmClass3_Surge,
                    AlarmClass3_Surge,
                    AlarmClass3_Surge,
                    AlarmClass3_Surge,
                    AlarmClass4_Surge,
                    AlarmClass4_Surge,
                    AlarmClass4_Surge,
                    AlarmClass4_Surge,
                },
                _ => new List<ChainedPuzzle>(),
            };

        public static List<ChainedPuzzle> BuildReactorShutdownPack(string tier)
        {
            switch (tier)
            {
                case "A":
                    return new List<ChainedPuzzle> { AlarmClass2, AlarmClass3, AlarmClass3 };

                case "B":
                    return new List<ChainedPuzzle> { AlarmClass3, AlarmClass4, AlarmClass4, AlarmClass5, AlarmClass5 };

                case "C":
                    {
                        // Now we get spicy
                        return new List<ChainedPuzzle>
                        {
                            // Class 8 alarm
                            new ChainedPuzzle
                            {
                                PublicAlarmName = "Class VIII",
                                SurvivalWaveSettings = VanillaWaveSettings.ApexIncreased,
                                SurvivalWavePopulation = WavePopulation.ReactorBaselineHybrid,
                                Puzzle = new List<PuzzleComponent>
                                {
                                    PuzzleComponent.AllLarge,
                                    PuzzleComponent.ClusterSmall,
                                    PuzzleComponent.ClusterSmall,
                                    PuzzleComponent.ScanLarge,
                                    PuzzleComponent.ClusterSmall,
                                    PuzzleComponent.ClusterLarge,
                                    PuzzleComponent.ClusterSmall,
                                    PuzzleComponent.AllLarge,
                                }
                            },

                            // Class 4 surge alarm
                            new ChainedPuzzle
                            {
                                PublicAlarmName = "Class IV",
                                SurvivalWaveSettings = VanillaWaveSettings.Surge,
                                SurvivalWavePopulation = WavePopulation.Baseline,
                                Puzzle = new List<PuzzleComponent>
                                {
                                    PuzzleComponent.AllLarge,
                                    PuzzleComponent.ClusterSmall,
                                    PuzzleComponent.ClusterSmall,
                                    PuzzleComponent.ScanLarge
                                },
                            },

                            // S-class Hybrids
                            new ChainedPuzzle
                            {
                                PublicAlarmName = AlarmClass1_Sustained.PublicAlarmName,
                                SurvivalWaveSettings = VanillaWaveSettings.ApexIncreased,
                                SurvivalWavePopulation = WavePopulation.ReactorBaselineHybrid,
                                WantedDistanceFromStartPos = 6.0,
                                WantedDistanceBetweenPuzzleComponents = 2.0,
                                Puzzle = new List<PuzzleComponent>()
                                {
                                    PuzzleComponent.Sustained
                                }
                            }
                        };
                    }

                default:
                    return new List<ChainedPuzzle>() { AlarmClass2 };
            }
        }
        #endregion

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
            TriggerAlarmOnActivate = false,
            Puzzle = new List<PuzzleComponent> { PuzzleComponent.AllBig },
        };

        /******************** Alarm Scans ********************/
        #region Alarms: Apex
        public static ChainedPuzzle AlarmClass2 = new ChainedPuzzle
        {
            PublicAlarmName = "Class II",
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
            PublicAlarmName = "Class V",
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

        public static ChainedPuzzle AlarmClass6 = new ChainedPuzzle
        {
            PublicAlarmName = "Class VI",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = WavePopulation.Baseline,
            WantedDistanceBetweenPuzzleComponents = 15.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ScanLarge
            },
        };

        public static ChainedPuzzle AlarmClass7 = new ChainedPuzzle
        {
            PublicAlarmName = "Class VII",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = WavePopulation.Baseline,
            WantedDistanceBetweenPuzzleComponents = 15.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ScanLarge
            },
        };

        public static ChainedPuzzle AlarmClass8 = new ChainedPuzzle
        {
            PublicAlarmName = "Class VIII",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = WavePopulation.Baseline,
            WantedDistanceBetweenPuzzleComponents = 15.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ScanLarge
            },
        };
        #endregion

        #region Alarms: Surge
        public static ChainedPuzzle AlarmClass3_Surge = new ChainedPuzzle
        {
            PublicAlarmName = "Class III Surge",
            SurvivalWaveSettings = VanillaWaveSettings.Surge,
            SurvivalWavePopulation = WavePopulation.Baseline,
            WantedDistanceFromStartPos = 10.0,
            WantedDistanceBetweenPuzzleComponents = 20.0,
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
            SurvivalWaveSettings = VanillaWaveSettings.Surge,
            SurvivalWavePopulation = WavePopulation.Baseline,
            WantedDistanceFromStartPos = 10.0,
            WantedDistanceBetweenPuzzleComponents = 20.0,
            Puzzle = new List<PuzzleComponent>()
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.AllLarge,
                PuzzleComponent.AllLarge,
                PuzzleComponent.AllLarge
            }
        };
        #endregion

        #region Alarms: Sustained
        public static ChainedPuzzle AlarmClass1_Sustained = new ChainedPuzzle
        {
            PublicAlarmName = "Class S I Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.ApexIncreased,
            SurvivalWavePopulation = WavePopulation.ReactorBaseline,
            WantedDistanceFromStartPos = 6.0,
            WantedDistanceBetweenPuzzleComponents = 2.0,
            Puzzle = new List<PuzzleComponent>()
            {
                PuzzleComponent.Sustained
            }
        };
        #endregion

        #region Alarms: Error alarms
        public static ChainedPuzzle AlarmError_Baseline = new ChainedPuzzle
        {
            PublicAlarmName = "Class ://ERROR! Alarm",
            DisableSurvivalWaveOnComplete = false,
            AlarmSoundStart = 2200133294,
            AlarmSoundStop = 1190355274,
            SurvivalWaveSettings = VanillaWaveSettings.Trickle_352_SSpB,
            SurvivalWavePopulation = WavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>() { PuzzleComponent.AllLarge }
        };
        #endregion

        /******************** Exit Alarm Scans ********************/
        public static ChainedPuzzle ExitAlarm = new ChainedPuzzle
        {
            PublicAlarmName = "Alarm",
            TriggerAlarmOnActivate = false,
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = WavePopulation.Baseline,
            UseRandomPositions = false,
            WantedDistanceFromStartPos = 0.0,
            WantedDistanceBetweenPuzzleComponents = 1.0,
            Puzzle = new List<PuzzleComponent>() { new PuzzleComponent { PuzzleType = PuzzleType.ExpeditionExit } }
        };

        #region Bulkhead DC scans
        public static ChainedPuzzle BulkheadSelect_Main = new ChainedPuzzle
        {
            PersistentId = 59,
            TriggerAlarmOnActivate = false,
            WantedDistanceFromStartPos = 0.0,
            WantedDistanceBetweenPuzzleComponents = 0.0,
            OnlyShowHUDWhenPlayerIsClose = false,
            Puzzle = new List<PuzzleComponent>()
            {
                PuzzleComponent.BulkheadMain
            }
        };

        public static ChainedPuzzle BulkheadSelect_Secondary = new ChainedPuzzle
        {
            PersistentId = 60,
            TriggerAlarmOnActivate = false,
            WantedDistanceFromStartPos = 0.0,
            WantedDistanceBetweenPuzzleComponents = 0.0,
            OnlyShowHUDWhenPlayerIsClose = false,
            Puzzle = new List<PuzzleComponent>()
            {
                PuzzleComponent.BulkheadSecondary
            }
        };

        public static ChainedPuzzle BulkheadSelect_Overload = new ChainedPuzzle
        {
            PersistentId = 61,
            TriggerAlarmOnActivate = false,
            WantedDistanceFromStartPos = 0.0,
            WantedDistanceBetweenPuzzleComponents = 0.0,
            OnlyShowHUDWhenPlayerIsClose = false,
            Puzzle = new List<PuzzleComponent>()
            {
                PuzzleComponent.BulkheadOverload
            }
        };
        #endregion

        public static new void SaveStatic()
        {
            Bins.ChainedPuzzles.AddBlock(TeamScan);
            Bins.ChainedPuzzles.AddBlock(AlarmClass2);
            Bins.ChainedPuzzles.AddBlock(AlarmClass3);
            Bins.ChainedPuzzles.AddBlock(AlarmClass4);
            Bins.ChainedPuzzles.AddBlock(AlarmClass5);
            Bins.ChainedPuzzles.AddBlock(AlarmClass6);
            Bins.ChainedPuzzles.AddBlock(AlarmClass7);
            Bins.ChainedPuzzles.AddBlock(AlarmClass8);
            Bins.ChainedPuzzles.AddBlock(AlarmClass3_Surge);
            Bins.ChainedPuzzles.AddBlock(AlarmClass4_Surge);
            Bins.ChainedPuzzles.AddBlock(AlarmClass1_Sustained);
            Bins.ChainedPuzzles.AddBlock(AlarmError_Baseline);
            Bins.ChainedPuzzles.AddBlock(ExitAlarm);
            Bins.ChainedPuzzles.AddBlock(BulkheadSelect_Main);
            Bins.ChainedPuzzles.AddBlock(BulkheadSelect_Secondary);
            Bins.ChainedPuzzles.AddBlock(BulkheadSelect_Overload);
        }
    }
}
