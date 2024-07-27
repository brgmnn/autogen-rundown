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
    public Height AllowedZoneAltitude { get; set; } = Height.OnlyMid;

    public double ChanceToChange { get; set; } = 0.5;
}
