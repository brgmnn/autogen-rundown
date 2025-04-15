using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Glows;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models;

public record Glow : CustomRecord
{
    /// <summary>
    /// Color: Color when this enemy aren't glowing at all.
    /// </summary>
    public string DefaultColor { get; set; } = "black";

    /// <summary>
    /// Color: Color for Heartbeating phase of Hibernation
    /// </summary>
    public string HeartbeatColor { get; set; } = "#86F8BD";

    /// <summary>
    /// Color: Color for Detecting phase of Hibernation
    /// </summary>
    public string DetectionColor { get; set; } = "#86F8BD";

    /// <summary>
    /// Color: Color when this enemy has awake from Hibernate by himself (sounds, detection)
    /// </summary>
    public string SelfWakeupColor { get; set; } = "red";

    /// <summary>
    /// Color: Color when enemy has woke up from Propagate
    /// </summary>
    public string PropagateWakeupColor { get; set; } = "red";

    /// <summary>
    /// Color: Striker Tentacle Attack Glow Color
    /// </summary>
    public string TentacleAttackColor { get; set; } = "red";

    /// <summary>
    /// Shooter Firing Glow Color
    /// </summary>
    public string ShooterFireColor { get; set; } = "#86F8BD";

    /// <summary>
    /// Pulsating Effects
    /// </summary>
    public List<PuseEffect> PulseEffects { get; set; } = new();
}
