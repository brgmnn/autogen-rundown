using AutogenRundown.Json;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Abilities;

public record InfectionAttackData
{
    /// <summary>
    /// How much infection to deal per attack.
    ///
    /// BasedValue: This Infection value will be applied to player,
    /// You can also use negative value for disinfection effect
    ///
    /// 0 = 0%
    /// 0.45 = +45% infection
    /// 1.0 = +100% infection
    /// -0.1 = -10% infection
    /// </summary>
    [JsonConverter(typeof(PercentageConverter))]
    public double Infection { get; set; } = 0.0;

    /// <summary>
    /// SoundID to play
    ///
    /// Docs have Sound ID = 676801566 (drinking a disinfect)
    /// </summary>
    [JsonProperty("SoundEventID")]
    public Sound SoundId { get; set; } = Sound.None;

    /// <summary>
    /// Should we use spitter/disinfection effect?
    /// </summary>
    public bool UseEffect { get; set; } = false;

    /// <summary>
    /// How far does effect should spread?
    /// </summary>
    public double ScreenLiquidRange { get; set; } = 0.0;
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public record InfectionAttack : CustomRecord
{
    public InfectionAttackData? MeleeData { get; set; }

    public InfectionAttackData? TentacleData { get; set; }

    public InfectionAttackData? ProjectileData { get; set; }
}
