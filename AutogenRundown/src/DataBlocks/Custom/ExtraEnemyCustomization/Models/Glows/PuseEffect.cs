namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Glows;

public record PuseEffect
{
    /// <summary>
    /// AgentTarget: Which Mode you want to select?
    /// </summary>
    public string Target { get; set; } = "Hibernate";

    /// <summary>
    /// Total duration of Pulse Effect
    /// </summary>
    public double Duration { get; set; } = 1.75;

    /// <summary>
    /// Pattern String for Effect
    ///
    /// 0~9 and f means Intensity
    /// 0 = 0.0
    /// 1 = 0.1
    /// ...
    /// 9 = 0.9
    /// f = 1.0
    /// + means extend, it extend the previous intensity setting duration
    /// - means delay, it does not extend previous intensity setting and just wait for next pattern
    /// So "f+++0" on Duration 1.0 means...
    /// Make it to full intensity for 0.8 seconds duration, and make it to zero for only 0.2 seconds.
    /// And repeat
    /// </summary>
    public string GlowPattern { get; set; } = "04";

    /// <summary>
    /// Pulse Effect Color
    /// </summary>
    public string GlowColor { get; set; } = "#86F8BD * 10.0";

    /// <summary>
    /// Should Time for each enemy be randomized? (useful for hibernating enemies having different glow timing)
    /// </summary>
    public bool RandomizeTime { get; set; } = true;

    /// <summary>
    /// Should Effect continue even if enemy has died?
    /// </summary>
    public bool KeepOnDead { get; set; } = false;

    /// <summary>
    /// Preventing Glowing Effects (Striker Tentacle, Shooter Firing, Scout Screaming, etc) temporary stop the pulse effect?
    /// </summary>
    public bool AlwaysPulse { get; set; } = false;
}
