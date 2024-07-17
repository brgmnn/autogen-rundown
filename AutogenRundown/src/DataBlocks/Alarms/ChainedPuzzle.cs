using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Levels;
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
    public uint SurvivalWaveSettings { get; set; } = 0;

    /// <summary>
    /// Determine what type(s) of enemy would spawn.
    /// </summary>
    public uint SurvivalWavePopulation { get; set; } = 0;

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
        => $"ChainedPuzzle {{ PersistentId = {PersistentId}, PublicAlarmName = {PublicAlarmName} }}";

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
    public static List<ChainedPuzzle> BuildPack(string tier, LevelSettings settings)
    {
        switch (tier)
        {
            case "A":
                return new List<ChainedPuzzle>
                {
                    None, None,
                    TeamScan, TeamScan, TeamScan,
                    AlarmClass2, AlarmClass2, AlarmClass2, AlarmClass2, AlarmClass2,
                    AlarmClass3, AlarmClass3, AlarmClass3, AlarmClass3, AlarmClass3,
                    AlarmClass4, AlarmClass4,
                };

            case "B":
                return new List<ChainedPuzzle>
                {
                    None, None,
                    TeamScan, TeamScan,
                    AlarmClass3, AlarmClass3, AlarmClass3,
                    AlarmClass4, AlarmClass4, AlarmClass4,
                    AlarmClass5,

                    AlarmClass1_Sustained,
                };

            case "C":
                return new List<ChainedPuzzle>
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
                };

            case "D":
            {
                var pack = new List<ChainedPuzzle>
                {
                    // Easy scans
                    TeamScan, TeamScan,
                    AlarmClass4,

                    // Stealth and Surprise scans. Secret scans are grouped with their regular
                    // counterpart
                    StealthScan4, Secret_StealthScan4_WithChargers,
                    None, Secret_SpawnTank,

                    // // Moderately difficult scans
                    // AlarmClass5_Hard, AlarmClass5_Hybrids, AlarmClass5_Chargers, AlarmClass5_Hard,
                    // AlarmClass5_Cluster, AlarmClass5_Cluster_Nightmare,
                    // AlarmClass5_Mixed, AlarmClass5_Mixed,

                    // // Challenging scans
                    // AlarmClass6_Chargers,
                    // AlarmClass6_Nightmare,
                    // AlarmClass6_Mixed,
                    //
                    // AlarmClass7_Chargers,
                    // AlarmClass7_Mixed,

                    // Surge (very challenging)
                    AlarmClass2_Surge,
                    AlarmClass3_Surge,

                    // // Sustained
                    // AlarmClass1_Sustained, AlarmClass1_Sustained, AlarmClass1_Sustained,
                };

                if (settings.Modifiers.Contains(LevelModifiers.Chargers) || settings.Modifiers.Contains(LevelModifiers.ManyChargers))
                    pack.AddRange(new List<ChainedPuzzle>
                    {
                        // Moderately difficult scans
                        AlarmClass5_Chargers, AlarmClass5_Chargers,

                        AlarmClass5_Cluster with { SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass5_Mixed with { SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId, PersistentId = Generator.GetPersistentId() },

                        AlarmClass5_Hard with { SurvivalWavePopulation = WavePopulation.OnlyChargers.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass5_Hard with { SurvivalWavePopulation = WavePopulation.OnlyChargers.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass5_Cluster with { SurvivalWavePopulation = WavePopulation.OnlyChargers.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass5_Mixed with { SurvivalWavePopulation = WavePopulation.OnlyChargers.PersistentId, PersistentId = Generator.GetPersistentId() },

                        // Challenging scans
                        AlarmClass6_Hard with { SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass6_Mixed with { SurvivalWavePopulation = WavePopulation.OnlyChargers.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass7_Hard with { SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass7_Mixed with { SurvivalWavePopulation = WavePopulation.OnlyChargers.PersistentId, PersistentId = Generator.GetPersistentId() },

                        // Sustained
                        AlarmClass1_Sustained,
                        AlarmClass1_Sustained with { SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass1_Sustained with { SurvivalWavePopulation = WavePopulation.OnlyChargers.PersistentId, PersistentId = Generator.GetPersistentId() }
                    });
                if (settings.Modifiers.Contains(LevelModifiers.Shadows) || settings.Modifiers.Contains(LevelModifiers.ManyShadows))
                    pack.AddRange(new List<ChainedPuzzle>
                    {
                        // Moderately difficult scans
                        AlarmClass5_Hard with { SurvivalWavePopulation = WavePopulation.Baseline_Shadows.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass5_Hard with { SurvivalWavePopulation = WavePopulation.Baseline_Shadows.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass5_Cluster with { SurvivalWavePopulation = WavePopulation.Baseline_Shadows.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass5_Mixed with { SurvivalWavePopulation = WavePopulation.Baseline_Shadows.PersistentId, PersistentId = Generator.GetPersistentId() },

                        AlarmClass5_Hard with { SurvivalWavePopulation = WavePopulation.OnlyShadows.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass5_Hard with { SurvivalWavePopulation = WavePopulation.OnlyShadows.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass5_Cluster with { SurvivalWavePopulation = WavePopulation.OnlyShadows.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass5_Mixed with { SurvivalWavePopulation = WavePopulation.OnlyShadows.PersistentId, PersistentId = Generator.GetPersistentId() },

                        // Challenging scans
                        AlarmClass6_Hard with { SurvivalWavePopulation = WavePopulation.Baseline_Shadows.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass6_Mixed with { SurvivalWavePopulation = WavePopulation.OnlyShadows.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass7_Hard with { SurvivalWavePopulation = WavePopulation.Baseline_Shadows.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass7_Mixed with { SurvivalWavePopulation = WavePopulation.OnlyShadows.PersistentId, PersistentId = Generator.GetPersistentId() },

                        // Sustained
                        AlarmClass1_Sustained,
                        AlarmClass1_Sustained with { SurvivalWavePopulation = WavePopulation.Baseline_Shadows.PersistentId, PersistentId = Generator.GetPersistentId() },
                        AlarmClass1_Sustained with { SurvivalWavePopulation = WavePopulation.OnlyShadows.PersistentId, PersistentId = Generator.GetPersistentId() }
                    });
                if (settings.Modifiers.Contains(LevelModifiers.NoChargers) && settings.Modifiers.Contains(LevelModifiers.NoShadows))
                    pack.AddRange(new List<ChainedPuzzle>
                    {
                        // Moderately difficult scans
                        AlarmClass5_Hard, AlarmClass5_Hybrids, AlarmClass5_Chargers, AlarmClass5_Hard,
                        AlarmClass5_Cluster, AlarmClass5_Cluster_Nightmare,
                        AlarmClass5_Mixed, AlarmClass5_Mixed,

                        // Challenging scans
                        AlarmClass6_Chargers,
                        AlarmClass6_Nightmare,
                        AlarmClass6_Mixed,

                        AlarmClass7_Chargers,
                        AlarmClass7_Mixed,

                        // Sustained
                        AlarmClass1_Sustained, AlarmClass1_Sustained, AlarmClass1_Sustained
                    });

                return pack;
            }

            // TODO: balance this
            case "E":
                return new List<ChainedPuzzle>
                {
                    // Easy
                    TeamScan,
                    AlarmClass4, AlarmClass4,

                    // Stealth and Surprise scans. Secret scans are grouped with their regular
                    // counterpart
                    StealthScan4, Secret_StealthScan4_WithChargers,
                    None, Secret_SpawnTank,

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
                };

            default:
                return new List<ChainedPuzzle>();
        };
    }

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
    public static readonly ChainedPuzzle None = new() { PersistentId = 0 };

    /// <summary>
    /// Special chain puzzle that indicates the zone should be skipped for processing.
    /// </summary>
    public static readonly ChainedPuzzle SkipZone = new() { PersistentId = 0 };

    /******************** Non-Alarm Scans ********************/
    /// <summary>
    /// Standard team (orange) scan
    /// </summary>
    public static readonly ChainedPuzzle TeamScan = new()
    {
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
        SurvivalWaveSettings = WaveSettings.Baseline_Easy.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline.PersistentId,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig
        },
    };

    public static ChainedPuzzle AlarmClass2 = new()
    {
        PublicAlarmName = "Class II Alarm",
        SurvivalWaveSettings = WaveSettings.Baseline_Easy.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline.PersistentId,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall
        },
    };

    public static readonly ChainedPuzzle AlarmClass3 = new()
    {
        PublicAlarmName = "Class III Alarm",
        SurvivalWaveSettings = WaveSettings.Baseline_Normal.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline.PersistentId,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterSmall
        },
    };

    public static readonly ChainedPuzzle AlarmClass3_Hard = AlarmClass3 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass4 = new()
    {
        PublicAlarmName = "Class IV Alarm",
        SurvivalWaveSettings = WaveSettings.Baseline_Normal.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline.PersistentId,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge
        },
    };

    public static readonly ChainedPuzzle AlarmClass4_Hard = AlarmClass4 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass4_Chargers = AlarmClass4 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass4_Hybrids = AlarmClass4 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass4_Nightmare = AlarmClass4 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Nightmare.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass5 = new()
    {
        PublicAlarmName = "Class V Alarm",
        SurvivalWaveSettings = WaveSettings.Baseline_Normal.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline.PersistentId,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge
        },
    };

    public static readonly ChainedPuzzle AlarmClass5_Hard = AlarmClass5 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass5_Chargers = AlarmClass5 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass5_Hybrids = AlarmClass5 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass5_Nightmare = AlarmClass5 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Nightmare.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass6 = new()
    {
        PublicAlarmName = "Class VI Alarm",
        SurvivalWaveSettings = WaveSettings.Baseline_Normal.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline.PersistentId,
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

    public static readonly ChainedPuzzle AlarmClass6_Hard = AlarmClass6 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass6_Chargers = AlarmClass6 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass6_Hybrids = AlarmClass6 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
        PersistentId = Generator.GetPersistentId(),
        // Name = "AlarmClass6_Hybrids"
    };

    public static readonly ChainedPuzzle AlarmClass6_Nightmare = AlarmClass6 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Nightmare.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass7 = new()
    {
        PublicAlarmName = "Class VII Alarm",
        SurvivalWaveSettings = WaveSettings.Baseline_Normal.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline.PersistentId,
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

    public static readonly ChainedPuzzle AlarmClass7_Hard = AlarmClass7 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass7_Chargers = AlarmClass7 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass7_Hybrids = AlarmClass7 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass7_Nightmare = AlarmClass7 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Nightmare.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass8 = new()
    {
        PublicAlarmName = "Class VIII Alarm",
        SurvivalWaveSettings = WaveSettings.Baseline_Normal.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline.PersistentId,
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

    public static readonly ChainedPuzzle AlarmClass8_Hard = AlarmClass8 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass8_Chargers = AlarmClass8 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Chargers.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass8_Hybrids = AlarmClass8 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };

    public static readonly ChainedPuzzle AlarmClass8_Nightmare = AlarmClass8 with
    {
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Nightmare.PersistentId,
        PersistentId = Generator.GetPersistentId(),
    };
    #endregion

    #region Alarms: Cluster
    public static readonly ChainedPuzzle AlarmClass2_Cluster = new()
    {
        PublicAlarmName = "Class II Cluster Alarm",
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline.PersistentId,
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
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline.PersistentId,
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
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
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
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
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

    public static readonly ChainedPuzzle AlarmClass5_Cluster_Nightmare = new()
    {
        PublicAlarmName = "Class V Cluster Alarm",
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Nightmare.PersistentId,
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
    #endregion

    #region Alarms: Mixed
    public static readonly ChainedPuzzle AlarmClass4_Mixed = new()
    {
        PublicAlarmName = "Class IV M Alarm",
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
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
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
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
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
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
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
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
        SurvivalWaveSettings = (uint)VanillaWaveSettings.Surge,
        SurvivalWavePopulation = (uint)VanillaWavePopulation.Baseline,
        WantedDistanceFromStartPos = 10.0,
        WantedDistanceBetweenPuzzleComponents = 20.0,
        Puzzle = new List<PuzzleComponent>()
        {
            PuzzleComponent.AllLarge,
            PuzzleComponent.AllLarge
        }
    };

    public static readonly ChainedPuzzle AlarmClass3_Surge = new()
    {
        PublicAlarmName = "Class III Surge Alarm",
        SurvivalWaveSettings = (uint)VanillaWaveSettings.Surge,
        SurvivalWavePopulation = (uint)VanillaWavePopulation.Baseline,
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
    public static readonly ChainedPuzzle AlarmClass4_Surge = new()
    {
        PublicAlarmName = "Class IV Surge Alarm",
        SurvivalWaveSettings = (uint)VanillaWaveSettings.Surge,
        SurvivalWavePopulation = (uint)VanillaWavePopulation.Baseline,
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
    // TODO: Fix duration
    public static readonly ChainedPuzzle AlarmClass1_Sustained = new()
    {
        PublicAlarmName = "Class S I Alarm",
        SurvivalWaveSettings = WaveSettings.Baseline_Hard.PersistentId,
        SurvivalWavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
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
        SurvivalWaveSettings = (uint)VanillaWaveSettings.Trickle_352_SSpB,
        SurvivalWavePopulation = (uint)VanillaWavePopulation.Baseline,
        Puzzle = new List<PuzzleComponent>() { PuzzleComponent.AllLarge }
    };
    #endregion

    #region Alarms: Secret alarms
    /******************** Autogen Special Alarms ********************/
    public static readonly ChainedPuzzle Secret_SpawnTank = new()
    {
        PersistentId = 0,
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
        PublicAlarmName = "Alarm",
        TriggerAlarmOnActivate = false,
        SurvivalWaveSettings = (uint)VanillaWaveSettings.Apex,
        SurvivalWavePopulation = (uint)VanillaWavePopulation.Baseline,
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
        /*
        Bins.ChainedPuzzles.AddBlock(TeamScan);

        #region Apex
        Bins.ChainedPuzzles.AddBlock(AlarmClass1);
        Bins.ChainedPuzzles.AddBlock(AlarmClass2);
        Bins.ChainedPuzzles.AddBlock(AlarmClass3);
        Bins.ChainedPuzzles.AddBlock(AlarmClass4);
        Bins.ChainedPuzzles.AddBlock(AlarmClass5);
        Bins.ChainedPuzzles.AddBlock(AlarmClass6);
        Bins.ChainedPuzzles.AddBlock(AlarmClass7);
        Bins.ChainedPuzzles.AddBlock(AlarmClass8);

        Bins.ChainedPuzzles.AddBlock(AlarmClass3_Hard);
        Bins.ChainedPuzzles.AddBlock(AlarmClass4_Hard);
        Bins.ChainedPuzzles.AddBlock(AlarmClass5_Hard);
        Bins.ChainedPuzzles.AddBlock(AlarmClass6_Hard);
        Bins.ChainedPuzzles.AddBlock(AlarmClass7_Hard);
        Bins.ChainedPuzzles.AddBlock(AlarmClass8_Hard);

        Bins.ChainedPuzzles.AddBlock(AlarmClass4_Chargers);
        Bins.ChainedPuzzles.AddBlock(AlarmClass5_Chargers);
        Bins.ChainedPuzzles.AddBlock(AlarmClass6_Chargers);
        Bins.ChainedPuzzles.AddBlock(AlarmClass7_Chargers);
        Bins.ChainedPuzzles.AddBlock(AlarmClass8_Chargers);

        Bins.ChainedPuzzles.AddBlock(AlarmClass4_Hybrids);
        Bins.ChainedPuzzles.AddBlock(AlarmClass5_Hybrids);
        Bins.ChainedPuzzles.AddBlock(AlarmClass6_Hybrids);
        Bins.ChainedPuzzles.AddBlock(AlarmClass7_Hybrids);
        Bins.ChainedPuzzles.AddBlock(AlarmClass8_Hybrids);

        Bins.ChainedPuzzles.AddBlock(AlarmClass4_Nightmare);
        Bins.ChainedPuzzles.AddBlock(AlarmClass5_Nightmare);
        Bins.ChainedPuzzles.AddBlock(AlarmClass6_Nightmare);
        Bins.ChainedPuzzles.AddBlock(AlarmClass7_Nightmare);
        Bins.ChainedPuzzles.AddBlock(AlarmClass8_Nightmare);
        #endregion

        Bins.ChainedPuzzles.AddBlock(AlarmClass2_Cluster);
        Bins.ChainedPuzzles.AddBlock(AlarmClass3_Cluster);
        Bins.ChainedPuzzles.AddBlock(AlarmClass4_Cluster);
        Bins.ChainedPuzzles.AddBlock(AlarmClass5_Cluster);
        Bins.ChainedPuzzles.AddBlock(AlarmClass5_Cluster_Nightmare);

        Bins.ChainedPuzzles.AddBlock(AlarmClass4_Mixed);
        Bins.ChainedPuzzles.AddBlock(AlarmClass5_Mixed);
        Bins.ChainedPuzzles.AddBlock(AlarmClass6_Mixed);
        Bins.ChainedPuzzles.AddBlock(AlarmClass7_Mixed);

        Bins.ChainedPuzzles.AddBlock(AlarmClass2_Surge);
        Bins.ChainedPuzzles.AddBlock(AlarmClass3_Surge);
        Bins.ChainedPuzzles.AddBlock(AlarmClass4_Surge);

        Bins.ChainedPuzzles.AddBlock(AlarmClass1_Sustained);
        */

        // Bulkhead scans are referenced by the game
        BulkheadSelect_Main.Persist();
        BulkheadSelect_Secondary.Persist();
        BulkheadSelect_Overload.Persist();

        // TODO: move this to lazy generation
        // Several objectives use the exit alarm
        ExitAlarm.Persist();

        /*
            // TODO: we think we always have to assign some of these
        Bins.ChainedPuzzles.AddBlock(AlarmError_Baseline);
        Bins.ChainedPuzzles.AddBlock(ExitAlarm);

        Bins.ChainedPuzzles.AddBlock(BulkheadSelect_Main);
        Bins.ChainedPuzzles.AddBlock(BulkheadSelect_Secondary);
        Bins.ChainedPuzzles.AddBlock(BulkheadSelect_Overload);
        */
    }
}
