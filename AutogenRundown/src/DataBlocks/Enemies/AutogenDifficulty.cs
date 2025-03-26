namespace AutogenRundown.DataBlocks.Enemies
{
    [Flags]
    public enum AutogenDifficulty : uint
    {
        // Enemy variants
        Base       = 0x0000,
        Chargers   = 0x0001,
        Shadows    = 0x0002,
        Hybrids    = 0x0004,
        Flyers     = 0x0008,
        Nightmares = 0x0010,

        // Boss aligned spawned bosses
        BossAlignedSpawn = 0x0020,

        MegaMotherSpawn = 0x0040,

        // Tiers of difficulty
        TierA = 0x0100,
        TierB = 0x0200,
        TierC = 0x0400,
        TierD = 0x0800,
        TierE = 0x1000,
    }
}
