using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Alarms
{
    /// <summary>
    /// https://gtfo-modding.gitbook.io/wiki/reference/datablocks/main/chainedpuzzle
    /// </summary>
    public record class ChainedPuzzle : DataBlock
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
        public VanillaWavePopulation SurvivalWavePopulation { get; set; } = 0;

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
        /// Resaves the datablock with a new persistent Id. Very useful for modifying the alarm
        /// </summary>
        public void Persist()
        {
            PersistentId = Generator.GetPersistentId();
            Bins.ChainedPuzzles.AddBlock(this);
        }

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
                    None, None,
                    TeamScan, TeamScan,
                    AlarmClass3, AlarmClass3, AlarmClass3,
                    AlarmClass4, AlarmClass4, AlarmClass4,
                    AlarmClass5,

                    AlarmClass1_Sustained,
                },
                "C" => new List<ChainedPuzzle>
                {
                    None,

                    TeamScan, TeamScan,

                    AlarmClass3, AlarmClass3,

                    AlarmClass4, AlarmClass4, AlarmClass4, AlarmClass4,
                    AlarmClass4_Cluster, AlarmClass4_Cluster,
                    AlarmClass4_Mixed, AlarmClass4_Mixed,

                    AlarmClass5, AlarmClass5, AlarmClass5,
                    AlarmClass5_Cluster,
                    AlarmClass5_Mixed,
                    AlarmClass5_Mixed,

                    AlarmClass2_Surge,

                    AlarmClass1_Sustained, AlarmClass1_Sustained,
                },
                "D" => new List<ChainedPuzzle>
                {
                    // Easy scans
                    None,
                    TeamScan, TeamScan,
                    AlarmClass3,
                    AlarmClass4, AlarmClass4,

                    // Moderately difficult scans
                    AlarmClass5, AlarmClass5, AlarmClass5, AlarmClass5,
                    AlarmClass5_Cluster, AlarmClass5_Cluster,
                    AlarmClass5_Mixed, AlarmClass5_Mixed,

                    // Challenging scans
                    AlarmClass6_Mixed,
                    AlarmClass7_Mixed,

                    // Surge (very challenging)
                    AlarmClass2_Surge,
                    AlarmClass3_Surge,

                    // Sustained
                    AlarmClass1_Sustained, AlarmClass1_Sustained, AlarmClass1_Sustained,
                },
                "E" => new List<ChainedPuzzle>
                {
                    // Easy
                    None,
                    TeamScan,
                    AlarmClass4, AlarmClass4,

                    // Moderate
                    AlarmClass5, AlarmClass5, AlarmClass5,
                    AlarmClass5_Cluster, AlarmClass5_Cluster,
                    AlarmClass5_Mixed, AlarmClass5_Mixed,

                    // Difficult
                    AlarmClass6, AlarmClass6, AlarmClass6,
                    AlarmClass6_Mixed, AlarmClass6_Mixed,
                    AlarmClass7,
                    AlarmClass7_Mixed,
                    AlarmClass8,

                    // Pure pain scans, TODO: this might be too much
                    AlarmClass3_Surge, AlarmClass3_Surge,
                    AlarmClass4_Surge, // !!! Is this possible?

                    // Sustained
                    AlarmClass1_Sustained, AlarmClass1_Sustained, AlarmClass1_Sustained,
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
                                SurvivalWavePopulation = VanillaWavePopulation.ReactorBaselineHybrid,
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

                            // S-class Hybrids
                            new ChainedPuzzle
                            {
                                PublicAlarmName = AlarmClass1_Sustained.PublicAlarmName,
                                SurvivalWaveSettings = VanillaWaveSettings.ApexIncreased,
                                SurvivalWavePopulation = VanillaWavePopulation.ReactorBaselineHybrid,
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

        /// <summary>
        /// Team scan utilizing the AllLarge_Slow scan type. This scan takes a _long_ time to
        /// complete, about 2 minutes to fully complete.
        /// </summary>
        public static ChainedPuzzle TeamScan_Slow = new ChainedPuzzle
        {
            PublicAlarmName = "Scan_Blue",
            TriggerAlarmOnActivate = false,
            Puzzle = new List<PuzzleComponent> { PuzzleComponent.AllLarge_Slow },
        };

        #region Stealth scans
        public static ChainedPuzzle StealthScan2 = new ChainedPuzzle
        {
            PublicAlarmName = "Class II Scan",
            TriggerAlarmOnActivate = false,
            WantedDistanceFromStartPos = 0.0,
            WantedDistanceBetweenPuzzleComponents = 35.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.ScanLarge,
                PuzzleComponent.ClusterSmall
            },
        };

        public static ChainedPuzzle StealthScan3 = new ChainedPuzzle
        {
            PublicAlarmName = "Class III Scan",
            TriggerAlarmOnActivate = false,
            WantedDistanceFromStartPos = 0.0,
            WantedDistanceBetweenPuzzleComponents = 35.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.ScanLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall
            },
        };

        public static ChainedPuzzle StealthScan4 = new ChainedPuzzle
        {
            PublicAlarmName = "Class IV Scan",
            TriggerAlarmOnActivate = false,
            WantedDistanceFromStartPos = 0.0,
            WantedDistanceBetweenPuzzleComponents = 35.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.ScanLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.Cluster,
                PuzzleComponent.ClusterSmall
            },
        };
        #endregion

        /******************** Alarm Scans ********************/
        #region Alarms: Apex
        public static ChainedPuzzle AlarmClass1 = new ChainedPuzzle
        {
            PublicAlarmName = "Class I Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig
            },
        };

        public static ChainedPuzzle AlarmClass2 = new ChainedPuzzle
        {
            PublicAlarmName = "Class II Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.ClusterSmall
            },
        };

        public static ChainedPuzzle AlarmClass3 = new ChainedPuzzle
        {
            PublicAlarmName = "Class III Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall
            },
        };

        public static ChainedPuzzle AlarmClass4 = new ChainedPuzzle
        {
            PublicAlarmName = "Class IV Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ScanLarge
            },
        };

        public static ChainedPuzzle AlarmClass5 = new ChainedPuzzle
        {
            PublicAlarmName = "Class V Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ScanLarge
            },
        };

        public static ChainedPuzzle AlarmClass6 = new ChainedPuzzle
        {
            PublicAlarmName = "Class VI Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            WantedDistanceBetweenPuzzleComponents = 15.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ScanLarge
            },
        };

        public static ChainedPuzzle AlarmClass7 = new ChainedPuzzle
        {
            PublicAlarmName = "Class VII Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            WantedDistanceBetweenPuzzleComponents = 15.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
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
            PublicAlarmName = "Class VIII Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            WantedDistanceBetweenPuzzleComponents = 15.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ScanLarge
            },
        };
        #endregion

        #region Alarms: Cluster
        public static ChainedPuzzle AlarmClass2_Cluster = new ChainedPuzzle
        {
            PublicAlarmName = "Class II Cluster Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            WantedDistanceBetweenPuzzleComponents = 5.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.Cluster
            },
        };

        public static ChainedPuzzle AlarmClass3_Cluster = new ChainedPuzzle
        {
            PublicAlarmName = "Class III Cluster Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            WantedDistanceBetweenPuzzleComponents = 5.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.Cluster,
                PuzzleComponent.Cluster
            },
        };

        public static ChainedPuzzle AlarmClass4_Cluster = new ChainedPuzzle
        {
            PublicAlarmName = "Class IV Cluster Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            WantedDistanceBetweenPuzzleComponents = 5.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.Cluster,
                PuzzleComponent.Cluster,
                PuzzleComponent.Cluster,
            },
        };

        public static ChainedPuzzle AlarmClass5_Cluster = new ChainedPuzzle
        {
            PublicAlarmName = "Class V Cluster Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            WantedDistanceBetweenPuzzleComponents = 5.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.Cluster,
                PuzzleComponent.Cluster,
                PuzzleComponent.Cluster,
            },
        };
        #endregion

        #region Alarms: Mixed
        public static ChainedPuzzle AlarmClass4_Mixed = new ChainedPuzzle
        {
            PublicAlarmName = "Class IV M Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.ApexIncreased,
            SurvivalWavePopulation = VanillaWavePopulation.ModifiedSpHybrid,
            WantedDistanceBetweenPuzzleComponents = 20.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.SustainedSmall,
            },
        };

        public static ChainedPuzzle AlarmClass5_Mixed = new ChainedPuzzle
        {
            PublicAlarmName = "Class V M Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.ApexIncreased,
            SurvivalWavePopulation = VanillaWavePopulation.ModifiedSpHybrid,
            WantedDistanceBetweenPuzzleComponents = 20.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.SustainedSmall,
            },
        };

        public static ChainedPuzzle AlarmClass6_Mixed = new ChainedPuzzle
        {
            PublicAlarmName = "Class VI M Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.ApexIncreased,
            SurvivalWavePopulation = VanillaWavePopulation.ModifiedSpHybrid,
            WantedDistanceBetweenPuzzleComponents = 20.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.AllBig,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterSmall,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.SustainedSmall,
            },
        };

        public static ChainedPuzzle AlarmClass7_Mixed = new ChainedPuzzle
        {
            PublicAlarmName = "Class VII M Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.ApexIncreased,
            SurvivalWavePopulation = VanillaWavePopulation.ModifiedSpHybrid,
            WantedDistanceBetweenPuzzleComponents = 20.0,
            Puzzle = new List<PuzzleComponent>
            {
                PuzzleComponent.SustainedSmall,
                PuzzleComponent.SustainedSmall,
                PuzzleComponent.SustainedSmall,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.ClusterLarge,
                PuzzleComponent.Cluster,
                PuzzleComponent.Cluster,
            },
        };
        #endregion

        #region Alarms: Surge
        public static ChainedPuzzle AlarmClass2_Surge = new ChainedPuzzle
        {
            PublicAlarmName = "Class II Surge Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Surge,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            WantedDistanceFromStartPos = 10.0,
            WantedDistanceBetweenPuzzleComponents = 20.0,
            Puzzle = new List<PuzzleComponent>()
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.AllLarge
            }
        };

        public static ChainedPuzzle AlarmClass3_Surge = new ChainedPuzzle
        {
            PublicAlarmName = "Class III Surge Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Surge,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            WantedDistanceFromStartPos = 10.0,
            WantedDistanceBetweenPuzzleComponents = 20.0,
            Puzzle = new List<PuzzleComponent>()
            {
                PuzzleComponent.AllLarge,
                PuzzleComponent.AllLarge,
                PuzzleComponent.AllLarge
            }
        };

        /// <summary>
        /// Very difficult alarm, not recommended without preparing a geomorph or similar for success
        /// </summary>
        public static ChainedPuzzle AlarmClass4_Surge = new ChainedPuzzle
        {
            PublicAlarmName = "Class IV Surge Alarm",
            SurvivalWaveSettings = VanillaWaveSettings.Surge,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
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
            SurvivalWavePopulation = VanillaWavePopulation.ReactorBaseline,
            WantedDistanceFromStartPos = 12.0,
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
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
            Puzzle = new List<PuzzleComponent>() { PuzzleComponent.AllLarge }
        };
        #endregion

        /******************** Exit Alarm Scans ********************/
        public static ChainedPuzzle ExitAlarm = new ChainedPuzzle
        {
            PublicAlarmName = "Alarm",
            TriggerAlarmOnActivate = false,
            SurvivalWaveSettings = VanillaWaveSettings.Apex,
            SurvivalWavePopulation = VanillaWavePopulation.Baseline,
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
            Bins.ChainedPuzzles.AddBlock(AlarmClass1);
            Bins.ChainedPuzzles.AddBlock(AlarmClass2);
            Bins.ChainedPuzzles.AddBlock(AlarmClass3);
            Bins.ChainedPuzzles.AddBlock(AlarmClass4);
            Bins.ChainedPuzzles.AddBlock(AlarmClass5);
            Bins.ChainedPuzzles.AddBlock(AlarmClass6);
            Bins.ChainedPuzzles.AddBlock(AlarmClass7);
            Bins.ChainedPuzzles.AddBlock(AlarmClass8);

            Bins.ChainedPuzzles.AddBlock(AlarmClass2_Cluster);
            Bins.ChainedPuzzles.AddBlock(AlarmClass3_Cluster);
            Bins.ChainedPuzzles.AddBlock(AlarmClass4_Cluster);
            Bins.ChainedPuzzles.AddBlock(AlarmClass5_Cluster);

            Bins.ChainedPuzzles.AddBlock(AlarmClass4_Mixed);
            Bins.ChainedPuzzles.AddBlock(AlarmClass5_Mixed);
            Bins.ChainedPuzzles.AddBlock(AlarmClass6_Mixed);
            Bins.ChainedPuzzles.AddBlock(AlarmClass7_Mixed);

            Bins.ChainedPuzzles.AddBlock(AlarmClass2_Surge);
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
