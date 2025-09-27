using AutogenRundown.Extensions;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Alarms;

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
///     - Special  = 1 (1.2?)
///     - MiniBoss = 2
///     - Boss     = 2
///
/// Score Boundary refers to the total number of points of enemies alive currently.
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
///
///
/// Just checked the logic:
/// m_wavePauseMin_atCost and m_wavePauseMax_atCost determine the cost range
/// in which the wave cooldown ticks down. Notably, the next wave will not
/// begin to count down while the total enemy cost is bigger than
/// m_wavePauseMax_atCost. Remember that cost is dependent on enemy type.
/// By default, weaklings are 0.75, standards/specials are 1, minibosses are 2.
/// If enemy cost is below the max, then the logic gets a bit more mathy.
/// Basically, it uses the (fraction) position of the cost between
/// Min/Max_atCost to lerp the pause time from m_wavePauseMax to m_wavePauseMin.
/// For example, if the cost is halfway between Min/Max_atCost, then the next
/// wave will take the time halfway between m_wavePauseMin/Max to appear.
/// Likewise, if the cost is below  m_wavePauseMin_atCost, then the
/// pause will be m_wavePauseMin.
///
/// In short:
///     * m_wavePauseMax_atCost is the maximum enemy cost that the next wave
///       timer will begin ticking down at. At this cost, the next wave takes
///       m_wavePauseMax time to show up.
///     * m_wavePauseMin_atCost is the minimum enemy cost at or below which
///       the next wave takes m_wavePauseMin time to show up. If enemy cost
///       is between Min and Max, the time is interpolated accordingly.
///
/// None of this applies while there are still groups spawning.
///
/// I believe vanilla typically uses a significantly lower m_wavePauseMin vs
/// m_wavePauseMax to decrease the downtime if you wipe out the alarm wave.
/// This does, of course, mean you fight more enemies the faster you kill them.
///</summary>
public record WaveSettings : DataBlock
{
    public static readonly double Points_Weakling = 0.75;
    public static readonly double Points_Standard = 1.0;
    public static readonly double Points_Special = 1.2;
    public static readonly double Points_MiniBoss = 2.0;

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
    /// This functions as a soft cap on enemy units.
    ///
    /// Default = 25.0
    /// </summary>
    [JsonProperty("m_wavePauseMax_atCost")]
    public double WavePauseMax_atCost { get; set; } = 25.0;

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

    public virtual bool Equals(WaveSettings? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null || GetType() != other.GetType())
            return false;

        return PersistentId == other.PersistentId && RecordEqual(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(base.GetHashCode());
        hashCode.Add(PauseBeforeStart);
        hashCode.Add(PauseBetweenGroups);
        hashCode.Add(WavePauseMin_atCost);
        hashCode.Add(WavePauseMax_atCost);
        hashCode.Add(WavePauseMin);
        hashCode.Add(WavePauseMax);
        hashCode.Add(PopulationFilter);
        hashCode.Add((int)FilterType);
        hashCode.Add(ChanceToRandomizeSpawnDirectionPerWave);
        hashCode.Add(ChanceToRandomizeSpawnDirectionPerGroup);
        hashCode.Add(OverrideWaveSpawnType);
        hashCode.Add(SurvivalWaveSpawnType);
        hashCode.Add(PopulationPointsTotal);
        hashCode.Add(PopulationPointsPerWaveStart);
        hashCode.Add(PopulationPointsPerWaveEnd);
        hashCode.Add(PopulationPointsMinPerGroup);
        hashCode.Add(PopulationPointsPerGroupStart);
        hashCode.Add(PopulationPointsPerGroupEnd);
        hashCode.Add(PopulationRampOverTime);

        return hashCode.ToHashCode();
    }

    public bool RecordEqual(WaveSettings? other)
    {
        if (other is null || GetType() != other.GetType())
            return false;

        return PauseBeforeStart.ApproxEqual(other.PauseBeforeStart) &&
               PauseBetweenGroups.ApproxEqual(other.PauseBetweenGroups) &&
               WavePauseMin_atCost.ApproxEqual(other.WavePauseMin_atCost) &&
               WavePauseMax_atCost.ApproxEqual(other.WavePauseMax_atCost) &&
               WavePauseMin.ApproxEqual(other.WavePauseMin) &&
               WavePauseMax.ApproxEqual(other.WavePauseMax) &&
               PopulationFilter.SequenceEqual(other.PopulationFilter) &&
               FilterType == other.FilterType &&
               ChanceToRandomizeSpawnDirectionPerWave.ApproxEqual(other.ChanceToRandomizeSpawnDirectionPerWave) &&
               ChanceToRandomizeSpawnDirectionPerGroup.ApproxEqual(other.ChanceToRandomizeSpawnDirectionPerGroup) &&
               OverrideWaveSpawnType == other.OverrideWaveSpawnType &&
               SurvivalWaveSpawnType == other.SurvivalWaveSpawnType &&
               PopulationPointsTotal.ApproxEqual(other.PopulationPointsTotal) &&
               PopulationPointsPerWaveStart.ApproxEqual(other.PopulationPointsPerWaveStart) &&
               PopulationPointsPerWaveEnd.ApproxEqual(other.PopulationPointsPerWaveEnd) &&
               PopulationPointsMinPerGroup.ApproxEqual(other.PopulationPointsMinPerGroup) &&
               PopulationPointsPerGroupStart.ApproxEqual(other.PopulationPointsPerGroupStart) &&
               PopulationPointsPerGroupEnd.ApproxEqual(other.PopulationPointsPerGroupEnd) &&
               PopulationRampOverTime.ApproxEqual(other.PopulationRampOverTime);
    }

    public static void Setup() { }

    public override string ToString()
        => $"WaveSettings {{ Name = {Name}, PersistentId = {PersistentId}, PointsPerWaveStart = {PopulationPointsPerWaveStart} }}";

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

        var existing = Bins.WaveSettings.GetBlock(settings.RecordEqual);

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
    /// Scales down the wave settings for the given difficulty.
    /// </summary>
    /// <param name="difficulty">Numeric difficulty</param>
    /// <returns></returns>
    public WaveSettings ScaleDownFor(double difficulty)
    {
        if (difficulty.ApproxEqual(1.0))
            return this;

        return this with
        {
            PersistentId = 0,
            PopulationPointsTotal = PopulationPointsTotal > 0.0 ?
                PopulationPointsTotal / difficulty :
                PopulationPointsTotal,
            PopulationPointsPerWaveStart = PopulationPointsPerWaveStart / difficulty,
            PopulationPointsPerWaveEnd = PopulationPointsPerWaveEnd / difficulty
        };
    }

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
        Bins.WaveSettings.AddBlock(Error_Hard);
        Bins.WaveSettings.AddBlock(Error_VeryHard);

        Bins.WaveSettings.AddBlock(Error_Specials_Hard);
        Bins.WaveSettings.AddBlock(Error_Specials_VeryHard);

        Bins.WaveSettings.AddBlock(Error_Diminished_Easy);
        Bins.WaveSettings.AddBlock(Error_Diminished_Normal);

        Bins.WaveSettings.AddBlock(Error_Boss_Hard);
        Bins.WaveSettings.AddBlock(Error_Boss_VeryHard);
        Bins.WaveSettings.AddBlock(Error_Boss_Impossible);

        // Objective Exit
        Bins.WaveSettings.AddBlock(Exit_Objective_Easy);
        Bins.WaveSettings.AddBlock(Exit_Objective_Medium);
        Bins.WaveSettings.AddBlock(Exit_Objective_Hard);
        Bins.WaveSettings.AddBlock(Exit_Objective_VeryHard);

        // Surge
        Bins.WaveSettings.AddBlock(Surge);

        // Scout
        Bins.WaveSettings.AddBlock(Scout_Easy);
        Bins.WaveSettings.AddBlock(Scout_Normal);
        Bins.WaveSettings.AddBlock(Scout_Hard);
        Bins.WaveSettings.AddBlock(Scout_VeryHard);

        // Reactor
        Bins.WaveSettings.AddBlock(Reactor_Easy);
        Bins.WaveSettings.AddBlock(Reactor_Medium);
        Bins.WaveSettings.AddBlock(Reactor_Hard);
        Bins.WaveSettings.AddBlock(Reactor_VeryHard);

        Bins.WaveSettings.AddBlock(Reactor_Surge_50pts);
        Bins.WaveSettings.AddBlock(Reactor_Surge_60pts);
        Bins.WaveSettings.AddBlock(Reactor_Surge_70pts);

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
        Bins.WaveSettings.AddBlock(Finite_25pts_Normal);

        // Single waves
        Bins.WaveSettings.AddBlock(SingleWave_8pts);
        Bins.WaveSettings.AddBlock(SingleWave_12pts);
        Bins.WaveSettings.AddBlock(SingleWave_16pts);
        Bins.WaveSettings.AddBlock(SingleWave_20pts);
        Bins.WaveSettings.AddBlock(SingleWave_28pts);
        Bins.WaveSettings.AddBlock(SingleWave_35pts);

        SingleWave_MiniBoss_4pts.Persist();
        SingleWave_MiniBoss_6pts.Persist();
        SingleWave_MiniBoss_8pts.Persist();
        SingleWave_MiniBoss_12pts.Persist();
        SingleWave_MiniBoss_16pts.Persist();
        SingleWave_MiniBoss_24pts.Persist();

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
    public static WaveSettings Baseline_Hard = new()
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
    public static WaveSettings Baseline_VeryHard = new()
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
    public static WaveSettings MiniBoss_Hard = new()
    {
        PopulationFilter = { Enemies.EnemyType.Special, Enemies.EnemyType.MiniBoss },
        FilterType = PopulationFilterType.Include,
        PopulationPointsPerWaveStart = 18,
        PopulationPointsPerWaveEnd = 28,
        PopulationRampOverTime = 100,
        Name = "MiniBoss_Hard"
    };
    #endregion

    #region --- Error Alarms ---

    #region Diminished error alarms

    /// <summary>
    /// Intended for A-tier or very easy error alarms. This functions like a normal error alarm
    /// but with a significantly longer delay between waves.
    ///
    /// Wave = 3pts @ 186s
    /// </summary>
    public static readonly WaveSettings Error_Diminished_Easy = new()
    {
        PopulationFilter =
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special
        },
        FilterType = PopulationFilterType.Include,
        PauseBeforeStart = 3.0,
        PauseBetweenGroups = 186.0, // The key property: duration between waves

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

        Name = "Error_Diminished_Easy"
    };

    /// <summary>
    /// Wave = 3pts @ 124s
    /// </summary>
    public static readonly WaveSettings Error_Diminished_Normal = new()
    {
        PopulationFilter =
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special
        },
        FilterType = PopulationFilterType.Include,
        PauseBeforeStart = 3.0,
        PauseBetweenGroups = 124.0, // The key property: duration between waves

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

        Name = "Error_Diminished_Easy"
    };

    #endregion

    #region Standard error alarms
    /// <summary>
    /// Somewhat equavlent to PersistentId=32 "Trickle 3-52 SSpB"
    ///
    /// Quite an easy error alarm.
    /// -> 3pts of enemies every 52 seconds.
    ///
    /// This should be very easy for one player to fully manage with just a hammer by
    /// themselves while the rest of the team continues with their objectives
    ///
    /// Wave = 3pts @ 52s
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
    ///
    /// Wave = 4pts @ 45s
    /// </summary>
    public static readonly WaveSettings Error_Normal = new()
    {
        PopulationFilter = new()
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss,
        },
        FilterType = PopulationFilterType.Include,

        PauseBeforeStart = 3.0,
        PauseBetweenGroups = 45,
        PopulationPointsPerGroupStart = 4.0,
        PopulationPointsPerGroupEnd = 4.0,
        PopulationPointsPerWaveStart = 4.0,
        PopulationPointsPerWaveEnd = 4.0,

        PopulationRampOverTime = 0,
        ChanceToRandomizeSpawnDirectionPerGroup = 0.8,
        ChanceToRandomizeSpawnDirectionPerWave = 1.0,

        WavePauseMin = 1.0,
        WavePauseMax = 20.0,
        WavePauseMin_atCost = 1.0,
        WavePauseMax_atCost = 10.0,

        Name = "Error_Normal"
    };

    /// <summary>
    /// Harder than easy. This shouldn't feel relaxed to deal with
    ///
    /// Wave = 5pts @ 37s
    /// </summary>
    public static readonly WaveSettings Error_Hard = new()
    {
        PopulationFilter = new()
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss,
        },
        FilterType = PopulationFilterType.Include,

        PauseBeforeStart = 3.0,
        PauseBetweenGroups = 37,

        PopulationPointsPerGroupStart = 5.0,
        PopulationPointsPerGroupEnd = 5.0,
        PopulationPointsPerWaveStart = 5.0,
        PopulationPointsPerWaveEnd = 5.0,

        PopulationRampOverTime = 0,
        ChanceToRandomizeSpawnDirectionPerGroup = 0.8,
        ChanceToRandomizeSpawnDirectionPerWave = 1.0,

        WavePauseMin = 1.0,
        WavePauseMax = 20.0,
        WavePauseMin_atCost = 1.0,
        WavePauseMax_atCost = 10.0,

        Name = "Error_Hard"
    };

    /// <summary>
    /// Harder than easy. This shouldn't feel relaxed to deal with
    ///
    /// Wave = 6pts @ 30s
    /// </summary>
    public static readonly WaveSettings Error_VeryHard = new()
    {
        PopulationFilter = new()
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,

        PauseBeforeStart = 3.0,
        PauseBetweenGroups = 30,

        PopulationPointsPerGroupStart = 6.0,
        PopulationPointsPerGroupEnd = 6.0,
        PopulationPointsPerWaveStart = 6.0,
        PopulationPointsPerWaveEnd = 6.0,

        PopulationRampOverTime = 0,
        ChanceToRandomizeSpawnDirectionPerGroup = 0.8,
        ChanceToRandomizeSpawnDirectionPerWave = 1.0,

        WavePauseMin = 1.0,
        WavePauseMax = 20.0,
        WavePauseMin_atCost = 1.0,
        WavePauseMax_atCost = 10.0,

        Name = "Error_VeryHard"
    };
    #endregion

    #region Special error alarms
    /// <summary>
    /// Error alarm for spawning 3x special enemies. This should feel quite hard.
    /// </summary>
    public static readonly WaveSettings Error_Specials_Hard = new()
    {
        PopulationFilter = new()
        {
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,

        PauseBeforeStart = 3.0,
        PauseBetweenGroups = 37,

        PopulationPointsPerGroupStart = 3.6,
        PopulationPointsPerGroupEnd = 3.6,
        PopulationPointsPerWaveStart = 3.6,
        PopulationPointsPerWaveEnd = 3.6,

        PopulationRampOverTime = 1,
        ChanceToRandomizeSpawnDirectionPerGroup = 0.8,
        ChanceToRandomizeSpawnDirectionPerWave = 1.0,

        WavePauseMin = 1.0,
        WavePauseMax = 20.0,
        WavePauseMin_atCost = 1.0,
        WavePauseMax_atCost = 10.0,

        Name = "Error_Specials_Hard"
    };

    /// <summary>
    /// Harder than easy. This shouldn't feel relaxed to deal with
    /// </summary>
    public static readonly WaveSettings Error_Specials_VeryHard = new()
    {
        PopulationFilter = new()
        {
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,

        PauseBeforeStart = 3.0,
        PauseBetweenGroups = 30,

        PopulationPointsPerGroupStart = 4.0,
        PopulationPointsPerGroupEnd = 4.0,
        PopulationPointsPerWaveStart = 4.0,
        PopulationPointsPerWaveEnd = 4.0,

        PopulationRampOverTime = 0,
        ChanceToRandomizeSpawnDirectionPerGroup = 0.8,
        ChanceToRandomizeSpawnDirectionPerWave = 1.0,

        WavePauseMin = 1.0,
        WavePauseMax = 20.0,
        WavePauseMin_atCost = 1.0,
        WavePauseMax_atCost = 10.0,

        Name = "Error_Specials_VeryHard"
    };
    #endregion

    #region Boss error alarms
    /// <summary>
    /// Normal boss error alarm. Modelled after the R4E1 tank error alarm
    ///
    /// 10s before start, 4 minutes between tanks
    ///
    ///
    /// </summary>
    public static readonly WaveSettings Error_Boss_Hard = new()
    {
        PopulationFilter = new List<Enemies.EnemyType> {
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,

        PauseBeforeStart = 10,
        PauseBetweenGroups = 240, // 4 minutes
        WavePauseMin = 1,
        WavePauseMax = 20,
        WavePauseMin_atCost = 1.0,
        WavePauseMax_atCost = 10.0,

        PopulationPointsTotal = -1,

        PopulationPointsMinPerGroup = 1,
        PopulationPointsPerGroupStart = 1,
        PopulationPointsPerGroupEnd = 1,
        PopulationPointsPerWaveStart = 1,
        PopulationPointsPerWaveEnd = 1,

        PopulationRampOverTime = 0,
        ChanceToRandomizeSpawnDirectionPerGroup = 0.1,
        ChanceToRandomizeSpawnDirectionPerWave = 1.0,

        Name = "Error_MiniBoss_Hard"
    };

    public static readonly WaveSettings Error_Boss_VeryHard = new()
    {
        PopulationFilter = new List<Enemies.EnemyType> {
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,

        PauseBeforeStart = 10,
        PauseBetweenGroups = 180, // 3 minutes
        WavePauseMin = 1,
        WavePauseMax = 20,
        WavePauseMin_atCost = 1.0,
        WavePauseMax_atCost = 10.0,

        PopulationPointsTotal = -1,

        PopulationPointsMinPerGroup = 1,
        PopulationPointsPerGroupStart = 1,
        PopulationPointsPerGroupEnd = 1,
        PopulationPointsPerWaveStart = 1,
        PopulationPointsPerWaveEnd = 1,

        PopulationRampOverTime = 0,
        ChanceToRandomizeSpawnDirectionPerGroup = 0.1,
        ChanceToRandomizeSpawnDirectionPerWave = 1.0,

        Name = "Error_MiniBoss_VeryHard"
    };

    public static readonly WaveSettings Error_Boss_Impossible = new()
    {
        PopulationFilter = new List<Enemies.EnemyType> {
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,

        PauseBeforeStart = 1,
        PauseBetweenGroups = 60,
        WavePauseMin = 1,
        WavePauseMax = 20,
        WavePauseMin_atCost = 1.0,
        WavePauseMax_atCost = 10.0,

        PopulationPointsTotal = -1,

        PopulationPointsMinPerGroup = 1,
        PopulationPointsPerGroupStart = 1,
        PopulationPointsPerGroupEnd = 1,
        PopulationPointsPerWaveStart = 1,
        PopulationPointsPerWaveEnd = 1,

        PopulationRampOverTime = 0,
        ChanceToRandomizeSpawnDirectionPerGroup = 0.1,
        ChanceToRandomizeSpawnDirectionPerWave = 1.0,

        Name = "Error_MiniBoss_VeryHard"
    };
    #endregion
    #endregion

    #region Objective/Exit Error Alarms
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
        PopulationRampOverTime = 150,
    };

    public static WaveSettings Exit_Objective_VeryHard = new()
    {
        PopulationFilter = { Enemies.EnemyType.Weakling, Enemies.EnemyType.Boss },
        FilterType = PopulationFilterType.Exclude,

        PauseBeforeStart = 1.0,
        PauseBetweenGroups = 12.0,

        PopulationPointsPerWaveStart = 6.0,
        PopulationPointsPerWaveEnd = 10.0,
        PopulationPointsMinPerGroup = 3,
        PopulationPointsPerGroupStart = 4,
        PopulationPointsPerGroupEnd = 6,
        PopulationRampOverTime = 120,
    };
    #endregion

    #region Finite points value
    public static WaveSettings Finite_25pts_Normal = new()
    {
        PopulationFilter = { Enemies.EnemyType.Weakling },
        FilterType = PopulationFilterType.Exclude,
        PauseBeforeStart = 1.0,

        PopulationPointsPerGroupStart = 7,
        PopulationPointsPerGroupEnd = 7,
        PopulationPointsMinPerGroup = 4,
        PopulationRampOverTime = 45.0,

        PopulationPointsTotal = 25,
        PopulationPointsPerWaveStart = 7,
        PopulationPointsPerWaveEnd = 14,

        ChanceToRandomizeSpawnDirectionPerGroup = 0.8,
        ChanceToRandomizeSpawnDirectionPerWave = 1.0,

        WavePauseMin = 1.0,
        WavePauseMax = 20.0,
        WavePauseMin_atCost = 1.0,

        Name = "Finite_25pts_Normal"
    };

    public static WaveSettings Finite_35pts_Hard = new()
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
    public static WaveSettings Surge = new()
    {
        PopulationFilter =
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
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
        WavePauseMax = 1,

        Name = "Surge"
    };
    #endregion

    #region Scout
    /// <summary>
    /// Equivalent to PersistentId=32 "Trickle 3-52 SSpB"
    ///
    /// Quite an easy error alarm.
    /// </summary>
    public static WaveSettings Scout_Easy = new()
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

        Name = "Scout_Easy"
    };

    public static WaveSettings Scout_Normal = new()
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

        PopulationPointsPerWaveStart = 15,
        PopulationPointsPerWaveEnd = 15,
        PopulationPointsTotal = 15,

        WavePauseMin = 1.0,
        WavePauseMax = 20.0,
        WavePauseMin_atCost = 1.0,

        Name = "Scout_Easy"
    };

    public static WaveSettings Scout_Hard = new()
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

        PopulationPointsPerWaveStart = 18,
        PopulationPointsPerWaveEnd = 18,
        PopulationPointsTotal = 18,

        WavePauseMin = 1.0,
        WavePauseMax = 20.0,
        WavePauseMin_atCost = 1.0,

        Name = "Scout_Easy"
    };

    public static WaveSettings Scout_VeryHard = new()
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

        PopulationPointsPerWaveStart = 24,
        PopulationPointsPerWaveEnd = 24,
        PopulationPointsTotal = 24,

        WavePauseMin = 1.0,
        WavePauseMax = 20.0,
        WavePauseMin_atCost = 1.0,

        Name = "Scout_VeryHard"
    };
    #endregion

    #region === Reactor Waves ===

    #region Reactor Waves -- General Pop
    /// <summary>
    /// Can be a fairly gentle reactor wave depending on the population selected.
    /// Only spawns standard and specials.
    /// </summary>
    public static WaveSettings Reactor_Easy = new()
    {
        PopulationFilter =
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
        },

        PopulationPointsTotal = 50,
        PopulationPointsPerWaveStart = 25,
        PopulationPointsPerWaveEnd = 25,
        PopulationPointsMinPerGroup = 5,
        PopulationPointsPerGroupStart = 5,
        PopulationPointsPerGroupEnd = 10,
        WavePauseMin = 16,
        WavePauseMax = 16,
        PopulationRampOverTime = 20
    };

    public static WaveSettings Reactor_Medium = new()
    {
        PopulationFilter =
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },

        PopulationPointsTotal = 60,
        PopulationPointsPerWaveStart = 25,
        PopulationPointsPerWaveEnd = 25,
        PopulationPointsMinPerGroup = 5,
        PopulationPointsPerGroupStart = 5,
        PopulationPointsPerGroupEnd = 15,
        WavePauseMin = 12,
        WavePauseMax = 12,
        PopulationRampOverTime = 15
    };

    public static WaveSettings Reactor_Hard = new()
    {
        PopulationFilter =
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },

        PopulationPointsTotal = 70,
        PopulationPointsPerWaveStart = 25,
        PopulationPointsPerWaveEnd = 25,
        PopulationPointsMinPerGroup = 5,
        PopulationPointsPerGroupStart = 5,
        PopulationPointsPerGroupEnd = 20,
        WavePauseMin = 9,
        WavePauseMax = 9,
        PopulationRampOverTime = 10
    };

    public static WaveSettings Reactor_VeryHard = new()
    {
        PopulationFilter =
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },

        PopulationPointsTotal = 80,
        PopulationPointsPerWaveStart = 30,
        PopulationPointsPerWaveEnd = 35,
        PopulationPointsMinPerGroup = 5,
        PopulationPointsPerGroupStart = 5,
        PopulationPointsPerGroupEnd = 20,
        WavePauseMin = 6,
        WavePauseMax = 6,
        PopulationRampOverTime = 5
    };
    #endregion

    #region Reactor Waves - Surge wave

    public static WaveSettings Reactor_Surge_50pts = new()
    {
        PopulationFilter =
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,

        PopulationPointsTotal = 50,

        PauseBeforeStart = 1.0,
        PauseBetweenGroups = 3.0,
        PopulationPointsPerWaveStart = 10_000,
        PopulationPointsPerWaveEnd = 10_000,
        PopulationPointsMinPerGroup = 2,
        PopulationPointsPerGroupStart = 4,
        PopulationPointsPerGroupEnd = 7,
        PopulationRampOverTime = 0,
        WavePauseMax = 1,

        Name = "Surge"
    };

    public static WaveSettings Reactor_Surge_60pts = new()
    {
        PopulationFilter =
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,

        PopulationPointsTotal = 60,

        PauseBeforeStart = 1.0,
        PauseBetweenGroups = 3.0,
        PopulationPointsPerWaveStart = 10_000,
        PopulationPointsPerWaveEnd = 10_000,
        PopulationPointsMinPerGroup = 2,
        PopulationPointsPerGroupStart = 4,
        PopulationPointsPerGroupEnd = 7,
        PopulationRampOverTime = 0,
        WavePauseMax = 1,

        Name = "Surge"
    };

    public static WaveSettings Reactor_Surge_70pts = new()
    {
        PopulationFilter =
        {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,

        PopulationPointsTotal = 70,

        PauseBeforeStart = 1.0,
        PauseBetweenGroups = 3.0,
        PopulationPointsPerWaveStart = 10_000,
        PopulationPointsPerWaveEnd = 10_000,
        PopulationPointsMinPerGroup = 2,
        PopulationPointsPerGroupStart = 4,
        PopulationPointsPerGroupEnd = 7,
        PopulationRampOverTime = 0,
        WavePauseMax = 1,

        Name = "Surge"
    };

    #endregion

    #region Reactor Waves -- Hybrids
    public static WaveSettings ReactorHybrids_Medium = new()
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

    public static WaveSettings ReactorHybrids_Group = new()
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
    public static WaveSettings ReactorPoints_Special_16pts = new()
    {
        PopulationFilter = { Enemies.EnemyType.Special },

        PopulationPointsTotal = 16,
        PopulationPointsPerWaveStart = 16,
        PopulationPointsPerWaveEnd = 16,
        PopulationPointsMinPerGroup = 16,
        PopulationPointsPerGroupStart = 16,
        PopulationPointsPerGroupEnd = 16,
        PopulationRampOverTime = 10,
        WavePauseMin = 1,
        WavePauseMax = 1,
    };
    #endregion

    #region Reactor Waves -- Chargers
    public static WaveSettings ReactorChargers_Easy = new()
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

    public static WaveSettings ReactorChargers_Hard = new()
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
    public static WaveSettings ReactorShadows_Easy = new()
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

    public static WaveSettings ReactorShadows_Hard = new()
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
    };
    #endregion

    #region Single wave spawns
    #region Miniboss only single waves
    /// <summary>
    /// 2 minibosses
    /// </summary>
    public static WaveSettings SingleWave_MiniBoss_4pts = new()
    {
        PopulationFilter = { Enemies.EnemyType.MiniBoss },
        FilterType = PopulationFilterType.Include,

        PopulationPointsTotal = 4,
    };

    /// <summary>
    /// 3 minibosses
    /// </summary>
    public static WaveSettings SingleWave_MiniBoss_6pts = new()
    {
        PopulationFilter = { Enemies.EnemyType.MiniBoss },
        FilterType = PopulationFilterType.Include,

        PopulationPointsTotal = 6,
    };

    /// <summary>
    /// 4 minibosses
    /// </summary>
    public static WaveSettings SingleWave_MiniBoss_8pts = new()
    {
        PopulationFilter = { Enemies.EnemyType.MiniBoss },
        FilterType = PopulationFilterType.Include,

        PopulationPointsTotal = 8,
    };

    /// <summary>
    /// 6 minibosses
    /// </summary>
    public static WaveSettings SingleWave_MiniBoss_12pts = new()
    {
        PopulationFilter = { Enemies.EnemyType.MiniBoss },
        FilterType = PopulationFilterType.Include,

        PopulationPointsTotal = 12,
    };

    /// <summary>
    /// 8 minibosses
    /// </summary>
    public static WaveSettings SingleWave_MiniBoss_16pts = new()
    {
        PopulationFilter = { Enemies.EnemyType.MiniBoss },
        FilterType = PopulationFilterType.Include,

        PopulationPointsTotal = 16,
    };

    /// <summary>
    /// 12 minibosses
    /// </summary>
    public static WaveSettings SingleWave_MiniBoss_24pts = new()
    {
        PopulationFilter = { Enemies.EnemyType.MiniBoss },
        FilterType = PopulationFilterType.Include,

        PopulationPointsTotal = 24,
    };
    #endregion

    public static WaveSettings SingleWave_8pts = new()
    {
        PopulationFilter = {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,
        WavePauseMin = 1,
        WavePauseMax = 1,

        PopulationPointsTotal = 8,
    };

    public static WaveSettings SingleWave_12pts = new()
    {
        PopulationFilter = {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,
        WavePauseMin = 1,
        WavePauseMax = 1,

        PopulationPointsTotal = 12,
    };

    public static WaveSettings SingleWave_16pts = new()
    {
        PopulationFilter = {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,
        WavePauseMin = 1,
        WavePauseMax = 1,

        PopulationPointsTotal = 16,
    };

    public static WaveSettings SingleWave_20pts = new()
    {
        PopulationFilter = {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,
        WavePauseMin = 1,
        WavePauseMax = 1,

        PopulationPointsTotal = 20,
    };

    public static WaveSettings SingleWave_28pts = new()
    {
        PopulationFilter = {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,
        WavePauseMin = 1,
        WavePauseMax = 1,

        PopulationPointsTotal = 28,
    };

    public static WaveSettings SingleWave_35pts = new()
    {
        PopulationFilter = {
            Enemies.EnemyType.Standard,
            Enemies.EnemyType.Special,
            Enemies.EnemyType.MiniBoss
        },
        FilterType = PopulationFilterType.Include,
        WavePauseMin = 1,
        WavePauseMax = 1,

        PopulationPointsTotal = 35,
    };
    #endregion

    #region Single enemy spawns
    public static WaveSettings SingleMiniBoss = new()
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
