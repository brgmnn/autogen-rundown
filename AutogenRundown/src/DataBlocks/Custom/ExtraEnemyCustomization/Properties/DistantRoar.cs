using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Properties;

public record DistantRoar : CustomRecord
{
    /// <summary>
    /// Which sound ID to use
    /// </summary>
    [JsonProperty("SoundID")]
    public Sound Sound { get; set; } = (Sound)3912366267;

    /// <summary>
    /// Minimum Interval between this distant roar sound plays
    /// </summary>
    public double Interval { get; set; } = 5.0;

    /// <summary>
    /// Override other WaveSpawning Roar sound
    /// </summary>
    public bool IsGlobal { get; set; } = false;

    /// <summary>
    /// Does this distant roar sound only plays when it's survival wave spawned enemies
    /// </summary>
    public bool OnlyForSurvivalWave { get; set; } = true;

    /// <summary>
    /// Only use WaveRoarFix's implementation, ignore legacy method
    /// </summary>
    public bool OnlyUseOverrides { get; set; } = true;

    /// <summary>
    /// Accepted enums:
    ///     Striker,
    ///     Shooter,
    ///     Birther,
    ///     Shadow,
    ///     Tank,
    ///     Flyer,
    ///     Immortal,
    ///     Bullrush,
    ///     Pouncer,
    ///     Striker_Berserk,
    ///     Shooter_Spread,
    ///     None,
    ///     OldDistantRoar,
    ///     Custom
    /// </summary>
    public string RoarSound { get; set; } = "Striker";

    /// <summary>
    /// Accepted enums: Unchanged, Small, Medium, Big
    /// </summary>
    public string RoarSize { get; set; } = "Unchanged";

    /// <summary>
    /// BoolBase: "Unchanged", true, false
    /// </summary>
    public string IsOutside { get; set; } = "Unchanged";
}
