namespace AutogenRundown.DataBlocks.Enemies
{
    [Flags]
    public enum AutogenDifficulty : uint
    {
        // Enemy variants
        Base = 0x0000,
        Chargers = 0x0001,
        Shadows = 0x0002,
        Hybrids = 0x0004,

        // Tiers of difficulty
        TierA = 0x0100,
        TierB = 0x0200,
        TierC = 0x0400,
        TierD = 0x0800,
        TierE = 0x1000,
    }
}
