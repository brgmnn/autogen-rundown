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
        WaveSettings = Alarms.WaveSettings.Reactor_Easy.PersistentId,
        WavePopulation = Alarms.WavePopulation.Baseline.PersistentId,
        Duration = 40
    };

    public static ReactorEnemyWave Baseline_Medium = new()
    {
        WaveSettings = Alarms.WaveSettings.Reactor_Medium.PersistentId,
        WavePopulation = Alarms.WavePopulation.Baseline.PersistentId,
        Duration = 50
    };

    public static ReactorEnemyWave Baseline_MediumMixed = new()
    {
        WaveSettings = Alarms.WaveSettings.Reactor_Medium.PersistentId,
        WavePopulation = Alarms.WavePopulation.Baseline.PersistentId,
        Duration = 60
    };

    public static ReactorEnemyWave Baseline_Hard = new()
    {
        WaveSettings = Alarms.WaveSettings.Reactor_Hard.PersistentId,
        WavePopulation = Alarms.WavePopulation.Baseline_Hybrids.PersistentId,
        Duration = 60
    };

    public static ReactorEnemyWave BaselineWithChargers_Hard = new()
    {
        WaveSettings = Alarms.WaveSettings.Reactor_Hard.PersistentId,
        WavePopulation = Alarms.WavePopulation.Baseline_Chargers.PersistentId,
        Duration = 55
    };

    public static ReactorEnemyWave BaselineWithNightmare_Hard = new()
    {
        WaveSettings = Alarms.WaveSettings.Reactor_Hard.PersistentId,
        WavePopulation = Alarms.WavePopulation.Baseline_Nightmare.PersistentId,
        Duration = 60
    };

    #region Giant waves
    public static ReactorEnemyWave Giants_16pts = new()
    {
        WaveSettings = Alarms.WaveSettings.ReactorPoints_Special_16pts.PersistentId,
        WavePopulation = Alarms.WavePopulation.Special_StrikerGiants.PersistentId,
        Duration = 20
    };
    #endregion

    #region Hybrid waves
    public static ReactorEnemyWave OnlyHybrids_Medium = new()
    {
        WaveSettings = Alarms.WaveSettings.ReactorHybrids_Medium.PersistentId,
        WavePopulation = Alarms.WavePopulation.OnlyHybrids.PersistentId,
        Duration = 50
    };
    #endregion

    #region Charger waves
    public static ReactorEnemyWave OnlyChargers_Easy = new()
    {
        WaveSettings = Alarms.WaveSettings.ReactorChargers_Easy.PersistentId,
        WavePopulation = Alarms.WavePopulation.OnlyChargers.PersistentId,
        Duration = 30
    };

    public static ReactorEnemyWave OnlyChargers_Hard = new()
    {
        WaveSettings = Alarms.WaveSettings.ReactorChargers_Hard.PersistentId,
        WavePopulation = Alarms.WavePopulation.OnlyChargers.PersistentId,
        Duration = 50
    };
    #endregion

    #region Shadow waves
    public static ReactorEnemyWave OnlyShadows_Easy = new()
    {
        WaveSettings = Alarms.WaveSettings.ReactorShadows_Easy.PersistentId,
        WavePopulation = Alarms.WavePopulation.OnlyShadows.PersistentId,
        Duration = 45
    };

    public static ReactorEnemyWave OnlyShadows_Hard = new()
    {
        WaveSettings = Alarms.WaveSettings.ReactorShadows_Hard.PersistentId,
        WavePopulation = Alarms.WavePopulation.OnlyShadows.PersistentId,
        Duration = 80
    };
    #endregion

    #region Boss waves
    public static ReactorEnemyWave SingleMother = new()
    {
        WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
        WavePopulation = Alarms.WavePopulation.SingleEnemy_Mother.PersistentId,
        Duration = 90
    };

    public static ReactorEnemyWave SingleTank = new()
    {
        WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
        WavePopulation = Alarms.WavePopulation.SingleEnemy_Tank.PersistentId,
        Duration = 90
    };

    public static ReactorEnemyWave SingleTankPotato = new()
    {
        WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
        WavePopulation = Alarms.WavePopulation.SingleEnemy_TankPotato.PersistentId,
        Duration = 50
    };

    public static ReactorEnemyWave SinglePouncer = new()
    {
        WaveSettings = Alarms.WaveSettings.SingleMiniBoss.PersistentId,
        WavePopulation = Alarms.WavePopulation.SingleEnemy_Pouncer.PersistentId,
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

    public uint WaveSettings { get; set; } = 0;

    public uint WavePopulation { get; set; } = 0;

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
