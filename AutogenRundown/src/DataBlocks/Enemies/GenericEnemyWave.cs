using AutogenRundown.DataBlocks.Alarms;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Enemies;

public record GenericWave
{
    /// <summary>
    /// Mostly so we can assign a none generic wave and then ignore it in places
    /// </summary>
    public static readonly GenericWave None = new();

    #region Objective Exit

    public static readonly GenericWave Exit_Objective_Easy = new()
    {
        Settings = WaveSettings.Exit_Objective_Easy,
        Population = WavePopulation.Baseline
    };

    public static readonly GenericWave Exit_Objective_Medium = new()
    {
        Settings = WaveSettings.Exit_Objective_Medium,
        Population = WavePopulation.Baseline
    };

    public static readonly GenericWave Exit_Objective_Hard = new()
    {
        Settings = WaveSettings.Exit_Objective_Hard,
        Population = WavePopulation.Baseline
    };

    public static readonly GenericWave Exit_Objective_VeryHard = new()
    {
        Settings = WaveSettings.Exit_Objective_VeryHard,
        Population = WavePopulation.Baseline
    };

    #endregion

    #region Error Alarms
    /// <summary>
    /// Base error alarm
    /// </summary>
    public static readonly GenericWave ErrorAlarm_Easy = new()
    {
        Settings = WaveSettings.Error_Easy,
        Population = WavePopulation.Baseline,
        SpawnDelay = 4.0,
        TriggerAlarm = true
    };

    /// <summary>
    /// Base error alarm
    /// </summary>
    public static readonly GenericWave ErrorAlarm_Normal = new()
    {
        Settings = WaveSettings.Error_Normal,
        Population = WavePopulation.Baseline,
        SpawnDelay = 4.0,
        TriggerAlarm = true
    };

    /// <summary>
    /// Base error alarm
    /// </summary>
    public static readonly GenericWave ErrorAlarm_Hard = new()
    {
        Settings = WaveSettings.Error_Hard,
        Population = WavePopulation.Baseline,
        SpawnDelay = 4.0,
        TriggerAlarm = true
    };

    /// <summary>
    /// Base error alarm
    /// </summary>
    public static readonly GenericWave ErrorAlarm_VeryHard = new()
    {
        Settings = WaveSettings.Error_VeryHard,
        Population = WavePopulation.Baseline,
        SpawnDelay = 4.0,
        TriggerAlarm = true
    };

    public static readonly GenericWave ErrorAlarm_Boss_Hard_Tank = new()
    {
        Settings = WaveSettings.Error_Boss_Hard,
        Population = WavePopulation.SingleEnemy_Tank,
        SpawnDelay = 0.0,
        TriggerAlarm = true
    };
    #endregion

    #region Chargers
    public static readonly GenericWave GiantChargers_35pts = new()
    {
        Settings = WaveSettings.SingleWave_35pts,
        Population = WavePopulation.OnlyChargers
    };
    #endregion

    #region Survival objective waves
    public static readonly GenericWave Survival_ErrorAlarm = new()
    {
        Settings = WaveSettings.Error_Easy,
        Population = WavePopulation.Baseline
    };

    public static readonly GenericWave Survival_Impossible_TankPotato = new()
    {
        Settings = WaveSettings.Survival_Impossible_MiniBoss,
        Population = WavePopulation.SingleEnemy_TankPotato
    };
    #endregion

    #region Single enemy waves
    public static readonly GenericWave SingleMother = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_Mother,
    };

    public static readonly GenericWave SinglePMother = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_PMother,
    };

    public static readonly GenericWave SingleTank = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_Tank,
    };

    public static readonly GenericWave SingleTankPotato = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_TankPotato,
    };

    public static readonly GenericWave SinglePouncer = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_Pouncer,
    };

    public static readonly GenericWave SinglePouncerShadow = new()
    {
        Settings = WaveSettings.SingleMiniBoss,
        Population = WavePopulation.SingleEnemy_PouncerShadow,
    };
    #endregion

    #region Sensor waves
    public static readonly GenericWave Sensor_8pts = new()
    {
        Settings = WaveSettings.SingleWave_8pts,
        Population = WavePopulation.Baseline
    };

    public static readonly GenericWave Sensor_12pts = new()
    {
        Settings = WaveSettings.SingleWave_12pts,
        Population = WavePopulation.Baseline
    };

    public static readonly GenericWave Sensor_16pts = new()
    {
        Settings = WaveSettings.SingleWave_16pts,
        Population = WavePopulation.Baseline
    };

    public static readonly GenericWave Sensor_20pts = new()
    {
        Settings = WaveSettings.SingleWave_20pts,
        Population = WavePopulation.Baseline
    };

    public static readonly GenericWave Sensor_Chargers_12pts = new()
    {
        Settings = WaveSettings.SingleWave_12pts,
        Population = WavePopulation.OnlyChargers
    };

    public static readonly GenericWave Sensor_Shadows_8pts = new()
    {
        Settings = WaveSettings.SingleWave_8pts,
        Population = WavePopulation.OnlyShadows
    };

    public static readonly GenericWave Sensor_Nightmares_12pts = new()
    {
        Settings = WaveSettings.SingleWave_12pts,
        Population = WavePopulation.OnlyNightmares
    };

    public static readonly GenericWave Sensor_Hybrids_8pts = new()
    {
        Settings = WaveSettings.SingleWave_8pts,
        Population = WavePopulation.OnlyHybrids
    };
    #endregion


    // Backing fields for deserialized values
    private uint? _survivalWaveSettings;
    private uint? _survivalWavePopulation;

    [JsonProperty("WaveSettings")]
    public uint SurvivalWaveSettings
    {
        get => _survivalWaveSettings ?? Settings.PersistentId;
        set => _survivalWaveSettings = value;
    }

    [JsonIgnore]
    public WaveSettings Settings { get; set; } = WaveSettings.None;

    // public uint WaveSettings { get; set; } = 0;

    [JsonIgnore]
    public WavePopulation Population { get; set; } = WavePopulation.None;

    [JsonProperty("WavePopulation")]
    public uint SurvivalWavePopulation
    {
        get => _survivalWavePopulation ?? Population.PersistentId;
        set => _survivalWavePopulation = value;
    }

    /// <summary>
    /// Room distance, in general this should always be left at 2.
    /// </summary>
    public int AreaDistance { get; set; } = 2;

    /// <summary>
    /// Delay in seconds before spawning the wave
    /// </summary>
    public double SpawnDelay { get; set; } = 0.0;

    /// <summary>
    /// Whether this should trigger an alarm
    /// </summary>
    public bool TriggerAlarm { get; set; } = false;

    /// <summary>
    /// Message to display
    /// </summary>
    public string IntelMessage { get; set; } = "";
}
