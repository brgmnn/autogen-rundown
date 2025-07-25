namespace AutogenRundown.DataBlocks.Zones;

public enum SeedType
{
    None = 0,
    Session = 1,
    Build = 2,
    Static = 3
}

/// <summary>
/// Used to select existing terminals.
/// </summary>
public class TerminalZoneSelection
{
    /// <summary>
    /// Zone of the terminal selection.
    /// </summary>
    public int LocalIndex { get; set; } = 0;

    /// <summary>
    /// Which seed to use when picking.
    ///
    /// Session should randomize between level attempts, Build should produce the same result
    /// always. Static is same as Build except the seed is set separately.
    ///
    /// None might be broken.
    /// </summary>
    public SeedType SeedType { get; set; } = SeedType.Session;

    /// <summary>
    /// Might be unused.
    /// </summary>
    public int TerminalIndex { get; set; } = 0;

    /// <summary>
    /// Seed to use when SeedType is set to static.
    /// </summary>
    public int StaticSeed { get; set; } = 0;
}
