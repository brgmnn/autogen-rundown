using AutogenRundown.DataBlocks.Alarms;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Enemies;

public record GenericWave
{
    /// <summary>
    /// Exit trickle alarm for running to extraction at the end of the level.
    /// </summary>
    public static readonly GenericWave ExitTrickle = new()
    {
        //WaveSettings = (uint)VanillaWaveSettings.ExitTrickle_38S_Original,
        Settings = WaveSettings.Exit_Baseline,
        Population = WavePopulation.Baseline,
        SpawnDelay = 4.0,
        TriggerAlarm = true,
    };

    /// <summary>
    ///
    /// </summary>
    public static readonly GenericWave Exit_Surge = new()
    {
        Settings = WaveSettings.Exit_Baseline,
    };

    #region Uplink Waves
    public static readonly GenericWave Uplink_Easy = new()
    {
        Settings = WaveSettings.Baseline_Normal,
        Population = WavePopulation.Baseline,
        SpawnDelay = 2.0,
        TriggerAlarm = true,
    };

    public static readonly GenericWave Uplink_Medium = new()
    {
        Settings = WaveSettings.Baseline_Hard,
        Population = WavePopulation.Baseline,
        SpawnDelay = 2.0,
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
    #endregion


    [JsonProperty("WaveSettings")]
    public uint SurvivalWaveSettings
    {
        get => Settings.PersistentId;
        private set { }
    }

    [JsonIgnore]
    public WaveSettings Settings { get; set; } = WaveSettings.None;

    // public uint WaveSettings { get; set; } = 0;

    [JsonIgnore]
    public WavePopulation Population { get; set; } = WavePopulation.None;

    [JsonProperty("WavePopulation")]
    public uint SurvivalWavePopulation
    {
        get => Population.PersistentId;
        private set { }
    }

    // public uint WavePopulation { get; set; } = 0;

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
