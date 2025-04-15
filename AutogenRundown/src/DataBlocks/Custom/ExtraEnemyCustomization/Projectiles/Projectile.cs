using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Projectiles;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public record Projectile : CustomRecord
{
    /// <summary>
    /// Do not overlap with existing one (0~12), It also should be between 0~255 (byte type)
    /// </summary>
    [JsonProperty("ID")]
    public uint Id { get; set; } = 50;

    /// <summary>
    /// It should be from existing projectile
    /// </summary>
    public string BaseProjectile { get; set; } = "TargetingSmall";

    /// <summary>
    /// BasedValue (Based on base projectile speed): Speed of Projectile, meter/s I guess
    /// </summary>
    public double Speed { get; set; } = 1.0;

    public ProjectileChange? SpeedChange { get; set; }

    /// <summary>
    /// BasedValue: Distance for checking if players doing Evasive Movement (dodge) - This will disable homing of projectile while doing evasive movement
    /// </summary>
    public double CheckEvasiveDistance { get; set; } = 4.0;

    /// <summary>
    /// BasedValue: Initial Homing before it starts actual homing, It uses lot loosen check for homing the projectile
    /// </summary>
    public double? InitialHomingDuration { get; set; }

    /// <summary>
    /// BasedValue: Initial Homing strength
    /// </summary>
    public double? InitialHomingStrength { get; set; }

    /// <summary>
    /// BasedValue: Delay before it starts second homing after it finished initial homing
    /// </summary>
    public double HomingDelay { get; set; } = 0.0;

    /// <summary>
    /// BasedValue (Based on base projectile strength): How strong it should be Homing to Target?
    /// </summary>
    public double HomingStrength { get; set; } = 1.0;

    public ProjectileChange? HomingStrengthChange { get; set; }

    /// <summary>
    /// BasedValue: Life time until projectile automatically destroyed
    /// </summary>
    public string LifeTime { get; set; } = "100%";

    /// <summary>
    /// Color: Trail effect color
    /// </summary>
    public string TrailColor { get; set; } = "cyan";

    /// <summary>
    /// BasedValue: Trail effect lasting time (determines length of trail effect)
    /// </summary>
    public double TrailTime { get; set; } = 1.0;

    /// <summary>
    /// BasedValue: Trail effect width (how thick?)
    /// </summary>
    public string TrailWidth { get; set; } = "100%";

    /// <summary>
    /// Color: Glow Color
    /// </summary>
    public string GlowColor { get; set; } = "cyan";

    /// <summary>
    /// BasedValue Glow Range
    /// </summary>
    public double GlowRange { get; set; } = 1.0;

    /// <summary>
    /// BasedValue: (Based on PlayerDataBlock.health, Has original Value): Damage to deal
    /// </summary>
    public double Damage { get; set; } = 1.0;

    /// <summary>
    /// BasedValue: (Based on 1.0, Has Original Value): Add Infection On Hit
    /// Editing Infection Will enable the Infection Bomb effect (Includes Particle Effect, Sound Effect),
    /// If you don't want it, use Ability/InfectionAttackCustom instead
    /// </summary>
    public string Infection { get; set; } = "0%";

    /// <summary>
    /// Spawn Projectile when it's collide with world
    /// </summary>
    public SpawnProjectile? SpawnProjectileOnCollideWorld { get; set; }

    /// <summary>
    /// Spawn Projectile when it's collide with players
    /// </summary>
    public SpawnProjectile? SpawnProjectileOnCollidePlayer { get; set; }

    /// <summary>
    /// Spawn Projectile when projectile has destroyed mid-air
    /// </summary>
    public SpawnProjectile? SpawnProjectileOnLifeTimeDone { get; set; }

    public JObject DrainStamina = new JObject
    {
        ["Enabled"] = false
    };
}
