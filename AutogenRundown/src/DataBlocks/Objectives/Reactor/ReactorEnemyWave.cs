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
    public static ReactorEnemyWave Baseline_Easy = new()
    {
        Settings = WaveSettings.Reactor_Easy,
        Population = WavePopulation.Baseline,
        Duration = 40
    };

    public static ReactorEnemyWave Baseline_Medium = new()
    {
        Settings = WaveSettings.Reactor_Medium,
        Population = WavePopulation.Baseline,
        Duration = 50
    };

    public static ReactorEnemyWave Baseline_MediumMixed = new()
    {
        Settings = WaveSettings.Reactor_Medium,
        Population = WavePopulation.Baseline,
        Duration = 60
    };

    public static ReactorEnemyWave Baseline_Hard = new()
    {
        Settings = WaveSettings.Reactor_Hard,
        Population = WavePopulation.Baseline_Hybrids,
        Duration = 60
    };

    public static ReactorEnemyWave BaselineWithChargers_Hard = new()
    {
        Settings = WaveSettings.Reactor_Hard,
        Population = WavePopulation.Baseline_Chargers,
        Duration = 55
    };

    public static ReactorEnemyWave BaselineWithNightmare_Hard = new()
    {
        Settings = WaveSettings.Reactor_Hard,
        Population = WavePopulation.Baseline_Nightmare,
        Duration = 60
    };

    #region Giant waves
    public static ReactorEnemyWave Giants_16pts = new()
    {
        Settings = WaveSettings.ReactorPoints_Special_16pts,
        Population = WavePopulation.Special_StrikerGiants,
        Duration = 20
    };
    #endregion

    #region Hybrid waves
    public static ReactorEnemyWave OnlyHybrids_Medium = new()
    {
        Settings = WaveSettings.ReactorHybrids_Medium,
        Population = WavePopulation.OnlyHybrids,
        Duration = 50
    };
    #endregion

    #region Charger waves
    public static ReactorEnemyWave OnlyChargers_Easy = new()
    {
        Settings = WaveSettings.ReactorChargers_Easy,
        Population = WavePopulation.OnlyChargers,
        Duration = 30
    };

    public static ReactorEnemyWave OnlyChargers_Hard = new()
    {
        Settings = WaveSettings.ReactorChargers_Hard,
        Population = WavePopulation.OnlyChargers,
        Duration = 50
    };
    #endregion

    #region Shadow waves
    public static ReactorEnemyWave OnlyShadows_Easy = new()
    {
        Settings = WaveSettings.ReactorShadows_Easy,
        Population = WavePopulation.OnlyShadows,
        Duration = 45
    };

    public static ReactorEnemyWave OnlyShadows_Hard = new()
    {
        Settings = WaveSettings.ReactorShadows_Hard,
        Population = WavePopulation.OnlyShadows,
        Duration = 80
    };
    #endregion

    #region Nightmare waves
    #endregion

    #region Boss waves
    public static ReactorEnemyWave SingleMother = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_Mother,
        Duration = 90
    };

    public static ReactorEnemyWave SingleTank = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_Tank,
        Duration = 90
    };

    public static ReactorEnemyWave SingleTankPotato = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_TankPotato,
        Duration = 50
    };

    public static ReactorEnemyWave SinglePouncer = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_Pouncer,
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
    public WavePopulation Population { get; set; } = WavePopulation.None;

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
