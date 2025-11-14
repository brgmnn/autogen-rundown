using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Alarms;

public record GlobalWaveSettings : DataBlock<GlobalWaveSettings>
{
    #region Properties

    /// <summary>
    /// Maximum total units cost that can be in play at once
    ///
    /// Soft spawn cap. All aggressive enemies count towards this. The value of
    /// each enemy is set by EnemyTypeCostsTowardsCap
    /// </summary>
    [JsonProperty("AllowedTotalCost")]
    public uint MaxCost { get; set; } = 25;

    /// <summary>
    /// Max heat value used for weight calculations. The closer a type's heat is to max, the lower its spawn chance.
    /// </summary>
    public uint MaxHeat { get; set; } = 200;

    /// <summary>
    /// How much to decrease heat of all types (that are not filtered out) each time an enemy is picked.
    ///
    /// This value is actually 1 right now in base game, which causes some bugs. It's intended to be 25, which is the default for the mod.
    /// </summary>
    public uint HeatCooldownSpeed { get; set; } = 25;

    /// <summary>
    /// Array of 5 floats. Base chance to spawn each type.
    /// </summary>
    public List<double> BaseWeights { get; set; } = new()
    {
        1.0,
        1.0,
        0.5,
        0.15,
        0.10
    };

    /// <summary>
    ///
    /// </summary>
    public List<int> HeatsOnSelect { get; set; } = new()
    {
        2,
        3,
        15,
        150,
        100
    };

    /// <summary>
    ///
    /// </summary>
    public List<double> HeatsAtStart { get; set; } = new()
    {
        0.2,
        0.3,
        1.5,
        15.0,
        10.0
    };

    /// <summary>
    ///
    /// </summary>
    public List<double> EnemyTypeCostsForWave { get; set; } = new()
    {
        0.75,
        1.0,
        1.0,
        2.0,
        2.0
    };

    /// <summary>
    ///
    /// </summary>
    public List<double> EnemyTypeCostsTowardsCap { get; set; } = new()
    {
        0.75,
        1.0,
        1.0,
        2.0,
        2.0
    };

    /// <summary>
    ///
    /// </summary>
    public List<int> WaveEnemyTypeLimits { get; set; } = new()
    {
        1,
        1,
        1,
        1,
        1
    };

    /// <summary>
    /// When set to true, prevents heat of enemy types not present in the wave settings of
    /// current wave from cooling down.
    ///
    /// Behind the scenes, there's a global grab count number for how many times total any enemy
    /// was picked. Each enemy heat type holds its own grab count, and when cooling down,
    /// multiplies the cooldown by the grab count difference. When set to true, the grab count for
    /// all types is always adjusted so the cooldown isn't multiplied.
    /// </summary>
    public bool AdjustHeatGrabCount { get; set; } = false;

    #endregion

    /// <summary>
    /// These should match the base games settings
    /// </summary>
    public static readonly GlobalWaveSettings Default = new() { PersistentId = 1 };

    public static readonly GlobalWaveSettings HighCap_30pts = new()
    {
        MaxCost = 30,
        PersistentId = 2
    };

    public static readonly GlobalWaveSettings HighCap_35pts = new()
    {
        MaxCost = 35,
        PersistentId = 3
    };

    public new static void SaveStatic()
    {
        Bins.GlobalWaveSettings.AddBlock(Default);

        Bins.GlobalWaveSettings.AddBlock(HighCap_30pts);
        Bins.GlobalWaveSettings.AddBlock(HighCap_35pts);
    }
}
