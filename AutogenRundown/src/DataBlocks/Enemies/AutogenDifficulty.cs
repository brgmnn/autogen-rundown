namespace AutogenRundown.DataBlocks.Enemies;

/// <summary>
/// Max value:
///     0xff
///
///     0b_1111_1111
///
/// 100 => 0b110_0100
/// 127 => 0b111_1111
/// We should stick to that as the max as this goes over the network, and it
/// seems like it has to be below a short int type in size.
///
/// Difficulty has a max of 255. 0xff
/// </summary>
[Flags]
public enum AutogenDifficulty : uint
{
    // Specific enemy variations are assigned
    //  0x00xx
    // 0xff is max
    // This gives 256 different enemy variant types.

    // Enemy categories
    Base             = 0x10, // 0001_0000
    Chargers         = 0x20, // 0010_0000
    Shadows          = 0x30, // 0011_0000
    Hybrids          = 0x40, // 0100_0000
    Flyers           = 0x50, // 0101_0000
    Nightmares       = 0x60, // 0110_0000

    // Types of specific spawns
    BloodDoors       = 0x70, // 0111_0000
    BossAlignedSpawn = 0x80, // 1000_0000
    MegaMotherSpawn  = 0x90, // 1001_0000

    // Tiers of difficulty
    TierA            = 0xa0, // 1010_0000
    TierB            = 0xb0, // 1011_0000
    TierC            = 0xc0, // 1100_0000
    TierD            = 0xd0, // 1101_0000
    TierE            = 0xe0, // 1110_0000
}
