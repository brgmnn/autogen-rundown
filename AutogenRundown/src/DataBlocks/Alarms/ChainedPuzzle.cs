using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Alarms;

/// <summary>
/// https://gtfo-modding.gitbook.io/wiki/reference/datablocks/main/chainedpuzzle
/// </summary>
public record ChainedPuzzle : DataBlock
{
    #region Internal for Autogen
    // These events get appended to the zone when the alarm is selected
    [JsonIgnore]
    public List<WardenObjectiveEvent> EventsOnApproachDoor { get; set; } = new();

    [JsonIgnore]
    public List<WardenObjectiveEvent> EventsOnUnlockDoor { get; set; } = new();

    [JsonIgnore]
    public List<WardenObjectiveEvent> EventsOnOpenDoor { get; set; } = new();

    [JsonIgnore]
    public List<WardenObjectiveEvent> EventsOnDoorScanStart { get; set; } = new();

    [JsonIgnore]
    public List<WardenObjectiveEvent> EventsOnDoorScanDone { get; set; } = new();

    /// <summary>
    /// Whether this alarm can be used as a base to roll new settings/pops on
    /// </summary>
    [JsonIgnore]
    public bool FixedAlarm { get; set; } = false;
    #endregion

    #region Properties
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
    public uint SurvivalWaveSettings
    {
        get => Settings.PersistentId;
        private set { }
    }

    [JsonIgnore]
    public WaveSettings Settings { get; set; } = WaveSettings.None;

    /// <summary>
    /// Determine what type(s) of enemy would spawn.
    /// </summary>
    public uint SurvivalWavePopulation
    {
        get => Population.PersistentId;
        private set { }
    }

    [JsonIgnore]
    public WavePopulation Population { get; set; } = WavePopulation.None;

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

    /// <summary>
    /// Just return the persistent ID as the name
    /// </summary>
    [JsonProperty("name")]
    public new string Name { get => PersistentId.ToString(); }
    #endregion

    #region Rarely set by us
    public int SurvivalWaveAreaDistance { get; set; } = 2;
    public UInt32 AlarmSoundStart = 3339129407u;
    public UInt32 AlarmSoundStop = 42633153u;
    #endregion

    public override string ToString()
        => $"ChainedPuzzle {{ PublicAlarmName = {PublicAlarmName}, Population = {Population}, Settings = {Settings} }}";

    /// <summary>
    /// Resaves the datablock with a new persistent Id. Very useful for modifying the alarm
    /// </summary>
    public void Persist(LazyBlocksBin<ChainedPuzzle>? bin = null)
    {
        bin ??= Bins.ChainedPuzzles;
        bin.AddBlock(this);
    }

    /// <summary>
    /// We explicitly want to not assign a persistent ID on creation of this object as we are
    /// lazily persisting them.
    /// </summary>
    public ChainedPuzzle() : base(0u)
    {}

    #region Pack builders

    /// <summary>
    /// Chained puzzle pack builders. Generates a pack of alarms that can be Pick()ed from
    /// for building levels.
    /// </summary>
    /// <param name="tier"></param>
    /// <returns></returns>
    public static List<(double, int, ChainedPuzzle)> BuildPack(string tier, LevelSettings settings)
    {
        switch (tier)
        {
            case "A":
                return new List<(double, int, ChainedPuzzle)>
                {
                    (1.0, 2, None),
                    (1.0, 3, TeamScan),
                    (1.0, 5, AlarmClass2),
                    (1.0, 5, AlarmClass3),
                    (1.0, 2, AlarmClass4)
                };

            case "B":
                return new List<(double, int, ChainedPuzzle)>
                {
                    (1.0, 2, None),
                    (1.0, 2, TeamScan),
                    (1.0, 3, AlarmClass3),
                    (1.0, 3, AlarmClass4),
                    (1.0, 1, AlarmClass5),

                    (1.0, 1, AlarmClass1_Sustained)
                };

            case "C":
                return new List<(double, int, ChainedPuzzle)>
                {
                    (1.0, 1, None),
                    (1.0, 2, TeamScan),

                    (1.0, 2, AlarmClass3),
                    (1.0, 4, AlarmClass4),
                    (1.0, 2, AlarmClass4_Cluster),
                    (1.0, 2, AlarmClass4_Mixed),
                    (1.0, 3, AlarmClass5),
                    (1.0, 1, AlarmClass5_Cluster),
                    (1.0, 2, AlarmClass5_Mixed),

                    (1.0, 1, AlarmClass2_Surge),

                    (1.0, 1, AlarmClass1_Sustained),
                };

            case "D":
            {
                return new List<(double, int, ChainedPuzzle)>
                {
                    // Easy scans
                    (1.0, 2, TeamScan),
                    (1.0, 1, AlarmClass4),

                    // Stealth and Surprise scans. Secret scans are grouped with their regular
                    // counterpart
                    (1.0, 1, StealthScan4),
                    (1.0, 1, Secret_StealthScan4_WithChargers),
                    (1.0, 1, None),
                    (1.0, 1, Secret_SpawnTank),

                    // Moderate
                    (1.0, 4, AlarmClass5),
                    (1.0, 2, AlarmClass5_Cluster),
                    (1.0, 2, AlarmClass5_Mixed),

                    // Hard
                    (1.0, 2, AlarmClass6),
                    (1.0, 1, AlarmClass6_Mixed),
                    (1.0, 1, AlarmClass7),
                    (1.0, 1, AlarmClass7_Mixed),

                    // Surge (very challenging)
                    (1.0, 1, AlarmClass2_Surge),
                    (1.0, 1, AlarmClass3_Surge),

                    // Sustained
                    (1.0, 3, AlarmClass1_Sustained),
                };
            }

            // TODO: balance this
            case "E":
                return new List<(double, int, ChainedPuzzle)>
                {
                    // Easy
                    (1.0, 1, TeamScan),
                    (1.0, 2, AlarmClass4),

                    // Stealth and Surprise scans. Secret scans are grouped with their regular
                    // counterpart
                    (1.0, 1, StealthScan4),
                    (1.0, 1, Secret_StealthScan4_WithChargers),
                    (1.0, 1, None),
                    (1.0, 1, Secret_SpawnTank),

                    // Moderate
                    (1.0, 3, AlarmClass5),
                    (1.0, 2, AlarmClass5_Cluster),
                    (1.0, 2, AlarmClass5_Mixed),

                    // Difficult
                    (1.0, 3, AlarmClass6),
                    (1.0, 2, AlarmClass6_Mixed),
                    (1.0, 1, AlarmClass7),
                    (1.0, 1, AlarmClass7_Mixed),
                    (1.0, 1, AlarmClass8),

                    // Pure pain scans, TODO: this might be too much
                    (1.0, 2, AlarmClass3_Surge),
                    (1.0, 1, AlarmClass4_Surge), // !!! Is this possible? We think yes (have cleared)

                    // Sustained
                    (1.0, 3, AlarmClass1_Sustained),
                };

            default:
                return new List<(double, int, ChainedPuzzle)>();
        };
    }

    // TODO: We need to finish this with the new system
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
                            SurvivalWaveSettings = (uint)VanillaWaveSettings.ApexIncreased,
                            SurvivalWavePopulation = (uint)VanillaWavePopulation.ReactorBaselineHybrid,
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
                            SurvivalWaveSettings = (uint)VanillaWaveSettings.ApexIncreased,
                            SurvivalWavePopulation = (uint)VanillaWavePopulation.ReactorBaselineHybrid,
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
    public static readonly ChainedPuzzle None = new() { PersistentId = 0, PublicAlarmName = "None" };

    /// <summary>
    /// Special chain puzzle that indicates the zone should be skipped for processing.
    /// </summary>
    public static readonly ChainedPuzzle SkipZone = new() { PersistentId = 0, PublicAlarmName = "Skip" };

    /******************** Non-Alarm Scans ********************/
    /// <summary>
    /// Standard team (orange) scan
    /// </summary>
    public static readonly ChainedPuzzle TeamScan = new()
    {
        PersistentId = 51,
        PublicAlarmName = "Scan",
        TriggerAlarmOnActivate = false,
        Puzzle = new List<PuzzleComponent> { PuzzleComponent.AllBig },
    };

    /// <summary>
    /// Team scan utilizing the AllLarge_Slow scan type. This scan takes a _long_ time to
    /// complete, about 2 minutes to fully complete.
    /// </summary>
    public static readonly ChainedPuzzle TeamScan_Slow = new()
    {
        PersistentId = 52,
        PublicAlarmName = "Scan_Blue",
        TriggerAlarmOnActivate = false,
        Puzzle = new List<PuzzleComponent> { PuzzleComponent.AllLarge_Slow },
    };

    /******************** Stealth Scans ********************/
    #region Stealth scans
    public static readonly ChainedPuzzle StealthScan2 = new()
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

    public static readonly ChainedPuzzle StealthScan3 = new()
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

    public static readonly ChainedPuzzle StealthScan4 = new()
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
    public static readonly ChainedPuzzle AlarmClass1 = new()
    {
        PublicAlarmName = "Class I Alarm",
        Settings = WaveSettings.Baseline_Easy,
        Population = WavePopulation.Baseline,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig
        },
    };

    public static ChainedPuzzle AlarmClass2 = new()
    {
        PublicAlarmName = "Class II Alarm",
        Settings = WaveSettings.Baseline_Easy,
        Population = WavePopulation.Baseline,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall
        },
    };

    public static readonly ChainedPuzzle AlarmClass3 = new()
    {
        PublicAlarmName = "Class III Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterSmall
        },
    };

    public static readonly ChainedPuzzle AlarmClass4 = new()
    {
        PublicAlarmName = "Class IV Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge
        },
    };

    public static readonly ChainedPuzzle AlarmClass5 = new()
    {
        PublicAlarmName = "Class V Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge
        },
    };

    // public static readonly ChainedPuzzle AlarmClass5_Hard = AlarmClass5 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass5_Chargers = AlarmClass5 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass5_Hybrids = AlarmClass5 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass5_Nightmare = AlarmClass5 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Nightmare.PersistentId,
    // };

    public static readonly ChainedPuzzle AlarmClass6 = new()
    {
        PublicAlarmName = "Class VI Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
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

    // public static readonly ChainedPuzzle AlarmClass6_Hard = AlarmClass6 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass6_Chargers = AlarmClass6 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass6_Hybrids = AlarmClass6 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass6_Nightmare = AlarmClass6 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Nightmare.PersistentId,
    // };

    public static readonly ChainedPuzzle AlarmClass7 = new()
    {
        PublicAlarmName = "Class VII Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
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

    // public static readonly ChainedPuzzle AlarmClass7_Hard = AlarmClass7 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass7_Chargers = AlarmClass7 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass7_Hybrids = AlarmClass7 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass7_Nightmare = AlarmClass7 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Nightmare.PersistentId,
    // };

    public static readonly ChainedPuzzle AlarmClass8 = new()
    {
        PublicAlarmName = "Class VIII Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
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

    // public static readonly ChainedPuzzle AlarmClass8_Hard = AlarmClass8 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass8_Chargers = AlarmClass8 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass8_Hybrids = AlarmClass8 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
    // };
    //
    // public static readonly ChainedPuzzle AlarmClass8_Nightmare = AlarmClass8 with
    // {
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Nightmare.PersistentId,
    // };

    /// <summary>
    /// These start to get quite hard. The fabled Class IX (9) alarm was a whole level on its own
    /// in R2 and that was after it was nerfed from a Class X (10)
    /// </summary>
    public static readonly ChainedPuzzle AlarmClass9 = new()
    {
        PublicAlarmName = "Class IX Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 15.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge,
            PuzzleComponent.AllBig
        },
    };

    /// <summary>
    /// This is the longest and will likely be a very challenging scan on any level. Ammo is likely
    /// to be a big problem and the possibility of using strategies such as C-foam holding are
    /// unlikely unless this spawns at a place with plenty of resources. Even moderate enemy
    /// wave settings will make this extremely challenging.
    /// </summary>
    public static readonly ChainedPuzzle AlarmClass10 = new()
    {
        PublicAlarmName = "Class X Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 15.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge,
            PuzzleComponent.AllBig
        },
    };
    #endregion

    #region Alarms: Cluster
    public static readonly ChainedPuzzle AlarmClass2_Cluster = new()
    {
        PublicAlarmName = "Class II Cluster Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 5.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.Cluster
        },
    };

    public static readonly ChainedPuzzle AlarmClass3_Cluster = new()
    {
        PublicAlarmName = "Class III Cluster Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 5.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster
        },
    };

    public static readonly ChainedPuzzle AlarmClass4_Cluster = new()
    {
        PublicAlarmName = "Class IV Cluster Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 5.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
        },
    };

    public static readonly ChainedPuzzle AlarmClass5_Cluster = new()
    {
        PublicAlarmName = "Class V Cluster Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 5.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
        },
    };

    // public static readonly ChainedPuzzle AlarmClass5_Cluster_Nightmare = new()
    // {
    //     PublicAlarmName = "Class V Cluster Alarm",
    //     SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
    //     SurvivalWavePopulation = WavePopulation.Baseline_Nightmare.PersistentId,
    //     WantedDistanceBetweenPuzzleComponents = 5.0,
    //     Puzzle = new List<PuzzleComponent>
    //     {
    //         PuzzleComponent.AllBig,
    //         PuzzleComponent.Cluster,
    //         PuzzleComponent.Cluster,
    //         PuzzleComponent.Cluster,
    //         PuzzleComponent.Cluster,
    //     },
    // };
    #endregion

    #region Alarms: Mixed
    public static readonly ChainedPuzzle AlarmClass4_Mixed = new()
    {
        PublicAlarmName = "Class IV M Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 20.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.SustainedSmall,
        },
    };

    public static readonly ChainedPuzzle AlarmClass5_Mixed = new()
    {
        PublicAlarmName = "Class V M Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
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

    public static readonly ChainedPuzzle AlarmClass6_Mixed = new()
    {
        PublicAlarmName = "Class VI M Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
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

    public static readonly ChainedPuzzle AlarmClass7_Mixed = new()
    {
        PublicAlarmName = "Class VII M Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
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
    public static readonly ChainedPuzzle AlarmClass2_Surge = new()
    {
        PublicAlarmName = "Class II Surge Alarm",
        Settings = WaveSettings.Surge,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 10.0,
        WantedDistanceBetweenPuzzleComponents = 20.0,
        FixedAlarm = true,
        Puzzle = new List<PuzzleComponent>()
        {
            PuzzleComponent.AllLarge,
            PuzzleComponent.AllLarge
        }
    };

    public static readonly ChainedPuzzle AlarmClass3_Surge = new()
    {
        PublicAlarmName = "Class III Surge Alarm",
        Settings = WaveSettings.Surge,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 10.0,
        WantedDistanceBetweenPuzzleComponents = 20.0,
        FixedAlarm = true,
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
    public static readonly ChainedPuzzle AlarmClass4_Surge = new()
    {
        PublicAlarmName = "Class IV Surge Alarm",
        Settings = WaveSettings.Surge,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 10.0,
        WantedDistanceBetweenPuzzleComponents = 20.0,
        FixedAlarm = true,
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
    public static readonly ChainedPuzzle AlarmClass1_Sustained = new()
    {
        PublicAlarmName = "Class S I Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 12.0,
        WantedDistanceBetweenPuzzleComponents = 2.0,
        Puzzle = new List<PuzzleComponent>()
        {
            PuzzleComponent.Sustained
        }
    };
    #endregion

    #region Alarms: Error alarms
    public static readonly ChainedPuzzle AlarmError_Baseline = new()
    {
        PublicAlarmName = "Class ://ERROR! Alarm",
        DisableSurvivalWaveOnComplete = false,
        AlarmSoundStart = 2200133294,
        AlarmSoundStop = 1190355274,

        Settings = WaveSettings.Error_Normal,
        Population = WavePopulation.Baseline,
        // SurvivalWaveSettings = (uint)VanillaWaveSettings.Trickle_352_SSpB,
        // SurvivalWavePopulation = (uint)VanillaWavePopulation.Baseline,

        Puzzle = new List<PuzzleComponent>() { PuzzleComponent.AllLarge }
    };
    #endregion

    #region Alarms: Secret alarms
    /******************** Autogen Special Alarms ********************/
    public static readonly ChainedPuzzle Secret_SpawnTank = new()
    {
        PersistentId = 0,
        FixedAlarm = true,
        TriggerAlarmOnActivate = false,
        EventsOnOpenDoor = new List<WardenObjectiveEvent>
        {
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlaySound,
                Trigger = WardenObjectiveEventTrigger.OnStart,
                // TODO: this just doesn't seem to work
                SoundId = Sound.Environment_DoorUnstuck,
                Delay = 5.0
            },
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlaySound,
                Trigger = WardenObjectiveEventTrigger.OnStart,
                SoundId = Sound.Enemies_DistantLowRoar,
                Delay = 8.0
            },
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SpawnEnemyWave,
                Delay = 9.0,
                EnemyWaveData = GenericWave.SingleTank
            },
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.None,
                Delay = 7.0,
                WardenIntel = ":://WARNING - UNKN0wИ .3rr0R: Err0r оcçurr..."
            },
        }
    };

    public static readonly ChainedPuzzle Secret_StealthScan4_WithChargers = new()
    {
        PublicAlarmName = "Class IV Scan",
        FixedAlarm = true,
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

        EventsOnDoorScanStart = new List<WardenObjectiveEvent>
        {
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlaySound,
                Trigger = WardenObjectiveEventTrigger.OnStart,
                SoundId = Sound.SheetMetalLand,
                Delay = 15.0
            },
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SpawnEnemyWave,
                Delay = 20.0,
                EnemyWaveData = GenericWave.GiantChargers_35pts
            },
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlaySound,
                SoundId = Sound.TankRoar,
                Delay = 20.0
            },
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.None,
                Delay = 20.0,
                WardenIntel = ":://WARNING - UNKN0wИ .3rr0R: Err0r оcçurr..."
            },
        }
    };
    #endregion

    /******************** Exit Alarm Scans ********************/
    public static readonly ChainedPuzzle ExitAlarm = new()
    {
        PersistentId = 50,
        PublicAlarmName = "Exit Alarm",
        TriggerAlarmOnActivate = false,
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        UseRandomPositions = false,
        WantedDistanceFromStartPos = 0.0,
        WantedDistanceBetweenPuzzleComponents = 1.0,
        Puzzle = new List<PuzzleComponent>() { new PuzzleComponent { PuzzleType = PuzzleType.ExpeditionExit } }
    };

    #region Bulkhead DC scans
    public static readonly ChainedPuzzle BulkheadSelect_Main = new()
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

    public static readonly ChainedPuzzle BulkheadSelect_Secondary = new()
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

    public static readonly ChainedPuzzle BulkheadSelect_Overload = new()
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
        ///
        /// IMPORTANT NOTE:
        ///
        /// ChainedPuzzles are lazily persisted in the game, as a result you should manually assign
        /// a persistent ID to any records persisted here, or run the risk of attempting to
        /// reference their persistent ID in game and having it be zero.
        ///

        // Bulkhead scans are referenced by the game
        BulkheadSelect_Main.Persist();
        BulkheadSelect_Secondary.Persist();
        BulkheadSelect_Overload.Persist();

        // Special terminal command, HSUFindSample etc. use these puzzles
        TeamScan.Persist();
        TeamScan_Slow.Persist();

        // Several objectives use the exit alarm
        ExitAlarm.Persist();
    }
}
