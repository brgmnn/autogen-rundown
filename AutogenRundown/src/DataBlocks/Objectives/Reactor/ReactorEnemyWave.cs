using AutogenRundown.DataBlocks.Alarms;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Objectives.Reactor;

public enum ReactorWaveSpawnType
{
    ClosestToReactor = 0,
    InElevatorZone = 1,
}

public record ReactorEnemyWave
{
    #region Preset waves
    public static readonly ReactorEnemyWave Baseline_Easy = new()
    {
        Settings = WaveSettings.Reactor_Easy,
        Population = WavePopulation.Baseline,
        Duration = 50
    };

    public static readonly ReactorEnemyWave Baseline_Medium = new()
    {
        Settings = WaveSettings.Reactor_Medium,
        Population = WavePopulation.Baseline,
        Duration = 60
    };

    public static readonly ReactorEnemyWave Baseline_Hard = new()
    {
        Settings = WaveSettings.Reactor_Hard,
        Population = WavePopulation.Baseline_Hybrids,
        Duration = 70
    };

    public static readonly ReactorEnemyWave Baseline_VeryHard = new()
    {
        Settings = WaveSettings.Reactor_VeryHard,
        Population = WavePopulation.Baseline_Hybrids,
        Duration = 80
    };

    public static readonly ReactorEnemyWave Baseline_SurgeMedium = new()
    {
        Settings = WaveSettings.Reactor_Surge_50pts,
        Population = WavePopulation.Baseline,
        Duration = 50
    };

    public static readonly ReactorEnemyWave Baseline_SurgeHard = new()
    {
        Settings = WaveSettings.Reactor_Surge_60pts,
        Population = WavePopulation.Baseline,
        Duration = 60
    };

    public static readonly ReactorEnemyWave Baseline_SurgeVeryHard = new()
    {
        Settings = WaveSettings.Reactor_Surge_70pts,
        Population = WavePopulation.Baseline,
        Duration = 70
    };

    #region MiniBoss single wave -- needs pop set
    public static readonly ReactorEnemyWave MiniBoss_4pts = new()
    {
        Settings = WaveSettings.SingleWave_MiniBoss_4pts,
        Duration = 10,
    };

    public static readonly ReactorEnemyWave MiniBoss_6pts = new()
    {
        Settings = WaveSettings.SingleWave_MiniBoss_6pts,
        Duration = 10,
    };

    public static readonly ReactorEnemyWave MiniBoss_8pts = new()
    {
        Settings = WaveSettings.SingleWave_MiniBoss_8pts,
        Duration = 15,
    };

    public static readonly ReactorEnemyWave MiniBoss_12pts = new()
    {
        Settings = WaveSettings.SingleWave_MiniBoss_12pts,
        Duration = 20,
    };

    public static readonly ReactorEnemyWave MiniBoss_16pts = new()
    {
        Settings = WaveSettings.SingleWave_MiniBoss_16pts,
        Duration = 25,
    };

    public static readonly ReactorEnemyWave MiniBoss_24pts = new()
    {
        Settings = WaveSettings.SingleWave_MiniBoss_24pts,
        Duration = 35,
    };
    #endregion

    #region Boss waves
    public static readonly ReactorEnemyWave SingleMother = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_Mother,
        Duration = 90
    };

    public static readonly ReactorEnemyWave SingleTank = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_Tank,
        Duration = 90
    };

    public static readonly ReactorEnemyWave SingleTankPotato = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_TankPotato,
        Duration = 50
    };

    public static readonly ReactorEnemyWave SinglePouncer = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_Pouncer,
        Duration = 30
    };

    public static readonly ReactorEnemyWave SingleShadowPouncer = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_PouncerShadow,
        Duration = 30
    };
    #endregion
    #endregion

    /// <summary>
    /// Estimated duration required to complete this wave. Should be roughly equal to the
    /// number of points spawned.
    /// </summary>
    [JsonIgnore]
    public double Duration { get; set; } = 30.0;

    [JsonProperty("WaveSettings")]
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
    [JsonProperty("WavePopulation")]
    public uint SurvivalWavePopulation
    {
        get => Population.PersistentId;
        private set { }
    }

    [JsonIgnore]
    public WavePopulation Population { get; set; } = WavePopulation.Baseline;

    /// <summary>
    /// Room distance, in general this should always be left at 2.
    /// </summary>
    public int AreaDistance { get; set; } = 2;

    /// <summary>
    /// When to spawn the wave relative to the reactor wave starting.
    /// </summary>
    public double SpawnTimeRel { get; set; } = 0.0;

    /// <summary>
    /// Desired starting spawn time for the wave. This value is used to calculate SpawnTimeRel
    /// after the full list of waves have been generated.
    /// </summary>
    [JsonIgnore]
    public double SpawnTime { get; set; } = 0.0;

    public ReactorWaveSpawnType SpawnType { get; set; } = ReactorWaveSpawnType.ClosestToReactor;
}
