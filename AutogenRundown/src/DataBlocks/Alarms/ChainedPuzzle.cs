using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Alarms;

/// <summary>
/// https://gtfo-modding.gitbook.io/wiki/reference/datablocks/main/chainedpuzzle
///
/// Note that the base game follows approximately the following class number averages for each tier:
///
///     * A-tier: Class 3
///     * B-tier: Class 4
///     * C-tier: Class 5
///     * D-tier: Class 6-7
///     * E-tier: Hardest. Class 7+
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
    public List<PuzzleComponent> Puzzle { get; set; } = new();

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

    // public virtual bool Equals(ChainedPuzzle? other)
    // {
    //     if (ReferenceEquals(this, other))
    //         return true;
    //     if (other is null || GetType() != other.GetType())
    //         return false;
    //
    //     return PersistentId == other.PersistentId &&
    //            TriggerAlarmOnActivate == other.TriggerAlarmOnActivate &&
    //            Settings == other.Settings &&
    //            Population == other.Population &&
    //            Puzzle.SequenceEqual(other.Puzzle) &&
    //            DisableSurvivalWaveOnComplete == other.DisableSurvivalWaveOnComplete &&
    //            UseRandomPositions == other.UseRandomPositions &&
    //            Utils.Math.Approximately(WantedDistanceFromStartPos, other.WantedDistanceFromStartPos) &&
    //            Utils.Math.Approximately(WantedDistanceBetweenPuzzleComponents,
    //                other.WantedDistanceBetweenPuzzleComponents) &&
    //            OnlyShowHUDWhenPlayerIsClose == other.OnlyShowHUDWhenPlayerIsClose &&
    //            SurvivalWaveAreaDistance == other.SurvivalWaveAreaDistance &&
    //            AlarmSoundStart == other.AlarmSoundStart &&
    //            AlarmSoundStart == other.AlarmSoundStop;
    // }

    public override string ToString()
        => Comment is not null ?
            $"ChainedPuzzle {{ PublicAlarmName = {PublicAlarmName}, Population = {Population}, Settings = {Settings}, Comment = {Comment} }}" :
            $"ChainedPuzzle {{ PublicAlarmName = {PublicAlarmName}, Population = {Population}, Settings = {Settings} }}";

    /// <summary>
    /// Time to complete the alarm
    /// We sum for the component durations, the distance from start pos, and the distance
    /// between the alarm components
    /// </summary>
    /// <param name="timeFactor">
    ///     Adjustment factor for time components.
    /// </param>
    /// <param name="traverseFactor">
    ///     Adjustment factor for traversing to the next puzzle coponent
    /// </param>
    /// <returns>Time to clear the alarm</returns>
    public double ClearTime(double timeFactor = 1.20, double traverseFactor = 1.20)
        => Puzzle.Sum(component => component.Duration) * timeFactor +
           WantedDistanceFromStartPos * traverseFactor +
           (Puzzle.Count - 1) * WantedDistanceBetweenPuzzleComponents * traverseFactor;

    /// <summary>
    /// Resaves the datablock with a new persistent Id. Very useful for modifying the alarm
    /// </summary>
    public void Persist(LazyBlocksBin<ChainedPuzzle>? bin = null)
    {
        bin ??= Bins.ChainedPuzzles;
        bin.AddBlock(this);
    }

    /// <summary>
    /// Given a puzzle, either find it's duplicate and use that or persist this one and return that
    /// instance.
    /// </summary>
    /// <param name="puzzle"></param>
    /// <returns></returns>
    public static ChainedPuzzle FindOrPersist(ChainedPuzzle puzzle)
    {
        // We specifically don't want to persist none, as we want to set the PersistentID to 0
        if (puzzle == None)
            return None;

        var existingPuzzle = Bins.ChainedPuzzles.GetBlock(puzzle);

        if (existingPuzzle != null)
            return existingPuzzle;

        // TODO: why didn't we do this before?
        if (puzzle.PersistentId == 0)
            puzzle.PersistentId = Generator.GetPersistentId();

        puzzle.Persist();

        return puzzle;
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
    ///
    /// We need to make sure each pack contains at least 20 alarms. A bulkhead can have up to
    /// 20 alarms in it.
    /// </summary>
    /// <param name="tier"></param>
    /// <param name="bulkhead"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static List<(double chance, int count, ChainedPuzzle puzzle)> BuildPack(
        string tier,
        Bulkhead bulkhead,
        LevelSettings settings)
    {
        var pack = (tier, bulkhead) switch
        {
            ("A", _) => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                // Free
                (0.4, 1, None),
                (1.0, 1, TeamScan),

                // Stealth and Surprise scans. Secret scans are grouped with their regular
                // counterpart
                (0.8, 1, Secret_TeamScan_EasyBaseline),

                // Easy
                (0.6, 2, AlarmClass1),
                (1.0, 3, AlarmClass2),
                (0.8, 1, AlarmClass2_Cluster),

                // Normal
                (1.0, 4, AlarmClass3),
                (1.0, 2, AlarmClass3_Cluster),
                (1.0, 2, AlarmClass3_Mixed),
                (0.8, 3, AlarmClass4),

                // Hard
                (0.3, 1, AlarmClass5)
            },

            ("B", _) => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                // Free
                (0.5, 1, TeamScan),

                // Stealth and Surprise scans
                (0.8, 1, Generator.Flip() ? StealthScan4 : Secret_StealthScan4_NormalBaseline),

                // Easy
                (0.6, 1, AlarmClass2),
                (1.0, 3, AlarmClass3),
                (0.8, 1, AlarmClass3_Cluster),

                // Normal
                (1.0, 4, AlarmClass4),
                (1.0, 2, AlarmClass4_Cluster),
                (1.0, 2, AlarmClass4_Mixed),
                (0.8, 3, AlarmClass5),

                // Hard
                (0.6, 1, AlarmClass1_Sustained),
                (0.6, 1, AlarmClass2_Surge)
            },

            ("C", _) => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                // Free
                (0.3, 1, TeamScan),

                // Stealth and Surprise scans
                (0.2, 1, StealthScan4),
                (0.8, 1, Secret_StealthScan4_NormalBaseline),

                // Easy
                (1.0, 2, AlarmClass4),
                (0.8, 1, AlarmClass4_Cluster),
                (0.8, 1, AlarmClass4_Mixed),

                // Normal
                (1.0, 4, AlarmClass5),
                (1.0, 2, AlarmClass5_Cluster),
                (1.0, 2, AlarmClass5_Mixed),
                (0.8, 2, AlarmClass6),
                (0.7, 1, AlarmClass6_Mixed),

                // Hard
                (1.0, 1, AlarmClass2_Surge),
                (0.7, 1, AlarmClass3_Surge),
                (1.0, 1, AlarmClass1_Sustained),
            },

            ("D", _) => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                // Free
                (0.2, 1, TeamScan),

                // Stealth and Surprise scans
                (0.3, 1, StealthScan4),
                (1.0, 1, Secret_StealthScan4_WithChargers),
                (1.0, 1, Secret_SpawnTank),
                (0.1, 1, Secret_SpawnMother),

                // Easy
                (0.3, 1, AlarmClass4_Mixed),
                (0.9, 1, AlarmClass5),
                (1.0, 1, AlarmClass5_Cluster),
                (1.0, 1, AlarmClass5_Mixed),

                // Normal
                (1.0, 3, AlarmClass6),
                (1.0, 2, AlarmClass6_Cluster),
                (1.0, 2, AlarmClass6_Mixed),
                (0.9, 2, AlarmClass7),

                // Hard
                (0.9, 2, AlarmClass7_Cluster),
                (0.8, 1, AlarmClass7_Mixed),
                (0.6, 1, AlarmClass8),

                // Hard specialty
                (1.0, 1, AlarmClass2_Surge),
                (1.0, 2, AlarmClass3_Surge),
                (0.5, 1, AlarmClass3_Surge_Extreme),
                (1.0, 2, AlarmClass1_Sustained),
            },

            ("E", _) => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                // Free - No free alarms on E-tier. Not sorry

                // Stealth and Surprise scans
                (0.05, 1, StealthScan4),
                (1.0,  1,  Secret_StealthScan4_WithChargers),
                (0.7,  1,  Secret_SpawnTank),
                (0.5,  1,  Secret_SpawnMother),

                // Easy
                (0.2, 1, AlarmClass5),
                (0.4, 1, AlarmClass5_Cluster),
                (0.7, 1, AlarmClass6),

                (0.7, 1, AlarmClass1_Sustained),

                // Normal
                (1.0, 1, AlarmClass6_Cluster),
                (1.0, 2, AlarmClass6_Mixed),
                (1.0, 4, AlarmClass7),
                (1.0, 2, AlarmClass7_Cluster),
                (1.0, 2, AlarmClass7_Mixed),

                // Hard
                (0.8, 3, AlarmClass8),
                (0.7, 1, AlarmClass9),

                // Hard specialty
                (1.0, 2, AlarmClass3_Surge),
                (0.7, 1, AlarmClass4_Surge),
                (0.7, 1, AlarmClass3_Surge_Extreme),
                (0.5, 1, AlarmClass3_Surge_Overload),
            },

            _ => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                (1.0, 20, TeamScan),
            }
        };

        if (settings.HasChargers() && tier is "D" or "E")
            pack.Add((1.0, 1, AlarmClass3_Surge_Extreme));

        if (settings.HasNightmares() && tier is "D" or "E")
            pack.Add((tier == "E" ? 1.0 : 0.7, 1, AlarmClass3_Surge_Overload));

        return pack;
    }
    #endregion

    /*************************************** Presets ***************************************/
    /// <summary>
    /// Special chain puzzle that has no puzzles to enter
    /// </summary>
    public static readonly ChainedPuzzle None = new()
    {
        PersistentId = 0,
        PublicAlarmName = "None",
        FixedAlarm = true
    };

    /// <summary>
    /// Special chain puzzle that indicates the zone should be skipped for processing.
    /// </summary>
    public static readonly ChainedPuzzle SkipZone = new()
    {
        PersistentId = 0,
        PublicAlarmName = "Skip",
        FixedAlarm = true
    };

    /******************** Non-Alarm Scans ********************/
    /// <summary>
    /// Some tiles depend on this
    /// </summary>
    public static readonly ChainedPuzzle Scan = new()
    {
        PersistentId = 4,
        PublicAlarmName = "Scan",
        TriggerAlarmOnActivate = false,
        FixedAlarm = true,
        Puzzle = new List<PuzzleComponent> { PuzzleComponent.AllBig },
    };

    /// <summary>
    /// Standard team (orange) scan
    /// </summary>
    public static readonly ChainedPuzzle TeamScan = new()
    {
        PersistentId = 51,
        PublicAlarmName = "Scan",
        TriggerAlarmOnActivate = false,
        FixedAlarm = true,
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
        FixedAlarm = true,
        Puzzle = new List<PuzzleComponent> { PuzzleComponent.AllLarge_Slow },
    };

    /******************** Stealth Scans ********************/
    #region Stealth scans
    public static readonly ChainedPuzzle StealthScan2 = new()
    {
        PublicAlarmName = "Class II Scan",
        TriggerAlarmOnActivate = false,
        FixedAlarm = true,
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
        FixedAlarm = true,
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
        FixedAlarm = true,
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

    public static readonly ChainedPuzzle StealthScan5 = new()
    {
        PublicAlarmName = "Class V Scan",
        TriggerAlarmOnActivate = false,
        FixedAlarm = true,
        WantedDistanceFromStartPos = 0.0,
        WantedDistanceBetweenPuzzleComponents = 35.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.ScanLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.Cluster,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.Cluster,
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

    public static readonly ChainedPuzzle AlarmClass2 = new()
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
        WantedDistanceFromStartPos = 15,
        WantedDistanceBetweenPuzzleComponents = 30.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge
        },
    };

    public static readonly ChainedPuzzle AlarmClass6 = new()
    {
        PublicAlarmName = "Class VI Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 20,
        WantedDistanceBetweenPuzzleComponents = 30.0,
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

    public static readonly ChainedPuzzle AlarmClass7 = new()
    {
        PublicAlarmName = "Class VII Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 20,
        WantedDistanceBetweenPuzzleComponents = 35.0,
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

    public static readonly ChainedPuzzle AlarmClass8 = new()
    {
        PublicAlarmName = "Class VIII Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 20,
        WantedDistanceBetweenPuzzleComponents = 35.0,
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

    /// <summary>
    /// These start to get quite hard. The fabled Class IX (9) alarm was a whole level on its own
    /// in R2 and that was after it was nerfed from a Class X (10)
    ///
    /// Apex Alarms for B-tier levels
    /// </summary>
    public static readonly ChainedPuzzle AlarmClass9 = new()
    {
        PublicAlarmName = "Class IX Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 20,
        WantedDistanceBetweenPuzzleComponents = 35.0,
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
    ///
    /// Apex Alarms for C-tier levels
    /// </summary>
    public static readonly ChainedPuzzle AlarmClass10 = new()
    {
        PublicAlarmName = "Class X Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 20,
        WantedDistanceBetweenPuzzleComponents = 35.0,
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

    /// <summary>
    /// This is the longest and will likely be a very challenging scan on any level. Ammo is likely
    /// to be a big problem and the possibility of using strategies such as C-foam holding are
    /// unlikely unless this spawns at a place with plenty of resources. Even moderate enemy
    /// wave settings will make this extremely challenging.
    ///
    /// Apex Alarms for D-tier levels
    /// </summary>
    public static readonly ChainedPuzzle AlarmClass11 = new()
    {
        PublicAlarmName = "Class XI Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 20,
        WantedDistanceBetweenPuzzleComponents = 35.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge,
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

    /// <summary>
    /// This is going to be very difficult. Only for E-tier levels
    ///
    /// Apex Alarms for E-tier levels
    /// </summary>
    public static readonly ChainedPuzzle AlarmClass12 = new()
    {
        PublicAlarmName = "Class XII Alarm",
        Settings = WaveSettings.Baseline_VeryHard,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 20,
        WantedDistanceBetweenPuzzleComponents = 35.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterSmall,
            PuzzleComponent.ScanLarge,
            PuzzleComponent.ClusterSmall,
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

    public static readonly ChainedPuzzle AlarmClass6_Cluster = new()
    {
        PublicAlarmName = "Class VI Cluster Alarm",
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
            PuzzleComponent.AllBig,
        },
    };

    public static readonly ChainedPuzzle AlarmClass7_Cluster = new()
    {
        PublicAlarmName = "Class VI Cluster Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 5.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
            PuzzleComponent.AllBig,
            PuzzleComponent.Cluster,
            PuzzleComponent.Cluster,
            PuzzleComponent.AllBig,
        },
    };
    #endregion

    #region Alarms: Mixed
    public static readonly ChainedPuzzle AlarmClass3_Mixed = new()
    {
        PublicAlarmName = "Class III M Alarm",
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        WantedDistanceBetweenPuzzleComponents = 20.0,
        Puzzle = new List<PuzzleComponent>
        {
            PuzzleComponent.AllBig,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.SustainedSmall,
        },
    };

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
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.Cluster,
            PuzzleComponent.SustainedSmall,
            PuzzleComponent.ClusterLarge,
            PuzzleComponent.Cluster,
            PuzzleComponent.SustainedSmall,
        },
    };
    #endregion

    #region Alarms: Surge
    public static readonly ChainedPuzzle AlarmClass2_Surge = new()
    {
        PublicAlarmName = "Class II Surge Alarm",
        Settings = WaveSettings.Surge,
        Population = WavePopulation.OnlyStrikers,
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
        Population = WavePopulation.OnlyStrikers,
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
        Population = WavePopulation.OnlyStrikers,
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

    #region Extreme Surge Alarms
    public static readonly ChainedPuzzle AlarmClass3_Surge_Extreme = new()
    {
        PublicAlarmName = "Class III Surge <color=orange>[EXTREME]</color> Alarm",
        Settings = WaveSettings.Surge,
        Population = WavePopulation.OnlyChargers,
        WantedDistanceFromStartPos = 20.0,
        WantedDistanceBetweenPuzzleComponents = 50.0,
        FixedAlarm = true,
        Puzzle = new List<PuzzleComponent>()
        {
            PuzzleComponent.AllLarge,
            PuzzleComponent.AllLarge,
            PuzzleComponent.AllLarge
        }
    };
    #endregion

    #region Overload Surge Alarms
    public static readonly ChainedPuzzle AlarmClass3_Surge_Overload = new()
    {
        PublicAlarmName = "Class III Surge <color=purple>[OVERLOAD]</color> Alarm",
        Settings = WaveSettings.Surge,
        Population = WavePopulation.OnlyNightmares,
        WantedDistanceFromStartPos = 20.0,
        WantedDistanceBetweenPuzzleComponents = 50.0,
        FixedAlarm = true,
        Puzzle = new List<PuzzleComponent>()
        {
            PuzzleComponent.AllLarge,
            PuzzleComponent.AllLarge,
            PuzzleComponent.AllLarge
        }
    };
    #endregion
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

    /// <summary>
    /// This scan is a gigantic-sized scan and usually should be used for a King of the Hill
    /// style objective.
    /// </summary>
    public static readonly ChainedPuzzle AlarmClass1_Sustained_MegaHuge = new()
    {
        PublicAlarmName = "Class S I Alarm",
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        WantedDistanceFromStartPos = 0.0,
        WantedDistanceBetweenPuzzleComponents = 0.0,
        Puzzle = new List<PuzzleComponent>()
        {
            PuzzleComponent.SustainedMegaHuge
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

        Settings = WaveSettings.Error_Easy,
        Population = WavePopulation.Baseline,

        Puzzle = new List<PuzzleComponent>() { PuzzleComponent.AllLarge }
    };

    public static readonly ChainedPuzzle AlarmError_Template = new()
    {
        PublicAlarmName = "Class ://ERROR! Alarm",
        Comment = "Error alarm template",
        AlarmSoundStart = 2200133294,
        AlarmSoundStop = 1190355274,
        TriggerAlarmOnActivate = false,
        FixedAlarm = true,
        Puzzle = new List<PuzzleComponent>() { PuzzleComponent.AllLarge }
    };
    #endregion

    #region Alarms: Secret alarms
    /******************** Autogen Special Alarms ********************/
    public static readonly ChainedPuzzle Secret_TeamScan_EasyBaseline = TeamScan with
    {
        PersistentId = 0,
        Comment = "Secret Alarm: Secret_TeamScan_EasyBaseline",
        FixedAlarm = true,
        EventsOnDoorScanStart = new List<WardenObjectiveEvent>
        {
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlaySound,
                Trigger = WardenObjectiveEventTrigger.OnStart,
                SoundId = Sound.SheetMetalLand,
                Delay = 27.0
            },
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SpawnEnemyWave,
                Delay = 32.0,
                EnemyWaveData = new GenericWave
                {
                    Settings = WaveSettings.SingleWave_20pts,
                    Population = WavePopulation.Baseline,
                    TriggerAlarm = false,
                }
            },
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlaySound,
                SoundId = Sound.TankRoar,
                Delay = 32.0
            },
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.None,
                Delay = 32.0,
                WardenIntel = ":://WARNING - UNKN0wИ .3rr0R: Err0r оcçurr..."
            },
        }
    };

    public static readonly ChainedPuzzle Secret_StealthScan4_NormalBaseline = StealthScan4 with
    {
        PersistentId = 0,
        Comment = "Secret Alarm: Secret_StealthScan4_NormalBaseline",
        FixedAlarm = true,
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
                EnemyWaveData = new GenericWave
                {
                    Settings = WaveSettings.SingleWave_28pts,
                    Population = WavePopulation.Baseline,
                    TriggerAlarm = false,
                }
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

    public static readonly ChainedPuzzle Secret_SpawnTank = TeamScan with
    {
        PersistentId = 0,
        Comment = "Secret Alarm: Secret_SpawnTank",
        FixedAlarm = true,
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

    public static readonly ChainedPuzzle Secret_SpawnMother = TeamScan with
    {
        PersistentId = 0,
        Comment = "Secret Alarm: Secret_SpawnMother",
        FixedAlarm = true,
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
                EnemyWaveData = GenericWave.SingleMother
            },
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.None,
                Delay = 7.0,
                WardenIntel = ":://WARNING - UNKN0wИ .3rr0R: Err0r оcçurr..."
            },
        }
    };

    public static readonly ChainedPuzzle Secret_StealthScan4_WithChargers = StealthScan4 with
    {
        PersistentId = 0,
        Comment = "Secret Alarm: Secret_StealthScan4_WithChargers",
        FixedAlarm = true,
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

    public new static void SaveStatic()
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
        Scan.Persist();
        TeamScan.Persist();
        TeamScan_Slow.Persist();

        AlarmError_Template.Persist();

        // Several objectives use the exit alarm
        ExitAlarm.Persist();
    }
}
