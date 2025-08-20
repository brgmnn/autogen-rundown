namespace AutogenRundown.DataBlocks;

/// <summary>
/// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#ewantedzoneheighs
/// </summary>
public enum Height
{
    LowMidHigh = 0,
    OnlyLow = 1,
    OnlyHigh = 2,
    OnlyMid = 3,
    LowMid = 4,
    MidHigh = 5,
    LowHigh = 6,
    Ascending = 7,
    Descending = 8,
    Unchanged = 9
}

public class Altitude
{
    /// <summary>
    /// Allowed altitudes for this zone.
    /// </summary>
    public Height AllowedZoneAltitude { get; set; } = Height.OnlyMid;

    /// <summary>
    /// The chance for the altitude to change in applicable situations (when it is not forced and
    /// can happen, e.g. allowed is "LowMid" and current is "Low").
    /// </summary>
    public double ChanceToChange { get; set; } = 0.5;

    public static readonly Altitude OnlyLow = new() { AllowedZoneAltitude = Height.OnlyLow };

    public static readonly Altitude OnlyMid = new() { AllowedZoneAltitude = Height.OnlyMid };

    public static readonly Altitude OnlyHigh = new() { AllowedZoneAltitude = Height.OnlyHigh };
}
