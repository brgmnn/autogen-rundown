namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Projectiles;

public record ProjectileChange
{
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Min Multiplier
    /// </summary>
    public double MinMulti { get; set; } = -0.2;

    /// <summary>
    /// Max Multiplier
    /// </summary>
    public double MaxMulti { get; set; } = 0.4;

    /// <summary>
    /// Duration of full effect
    /// </summary>
    public double Duration { get; set; } = 2.0;

    /// <summary>
    /// Stop Changing speed after duration
    /// </summary>
    public bool StopAfterDuration { get; set; }

    /// <summary>
    /// Specify Multiplier
    /// </summary>
    public double StopMulti { get; set; }

    /// <summary>
    /// Easing: type for Min-Max Transition
    /// </summary>
    public string EasingMode { get; set; } = "Linear";

    /// <summary>
    /// Min-Max transition movement
    ///
    /// Clamped = Min -> Max -> Keep exact Max value after Duration [Graph: /￣￣...]
    /// Unclamped = Min -> Max -> Keep increase after Duration [Graph: ↗+...]
    /// PingPong = Min -> Max -> Min -> Max -> ... [Graph: ↗↘↗↘↗↘...]
    /// Repeat = Min -> Max -> go to Min without transition [Graph: //////...]
    /// </summary>
    public string Mode { get; set; } = "PingPong";
}
