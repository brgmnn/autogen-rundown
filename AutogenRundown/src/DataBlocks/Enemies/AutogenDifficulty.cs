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
///
/// Allowed bitmasks
///     * Tier | Category
///     * Blooddoor | Category
/// </summary>
[Flags]
public enum AutogenDifficulty : uint
{
    // Specific enemy variations are assigned
    //  0x00xx
    // 0xff is max
    // This gives 256 different enemy variant types.
    //
    // Enemies up to:
    //     0b_0011_1111

    // striker          24      0001_0000
    // giant            28      0001_1100
    // ---
    // charger          30      0001_1110
    // giant charger    39      0010_0111
    // --
    // shadow           21      0001_0101
    // giant shadow     35      0010_0011
    // --
    // nightmare        53      0011_0101
    // nightmare shoot  52      0011_0100
    // nightmare giant 109      0110_1101
    // --
    // hybrid          33       0010_0001
    // hybrid infect   108      0110_1100

    // Enemy categories
    // Some are the same, because we don't have enough bits
    // The ones that are the same have to use EnemyRole to
    // Pick between the right units
    Base             = 0x00, // 0000_0000 -> Melee/Ranged
    Chargers         = 0x00, // 0000_0000 -> Lurker
    Shadows          = 0x40, // 0100_0000 -> Melee
    Hybrids          = 0x00, // 0000_0000 -> PureSneak
    Flyers           = 0xc0, // 1100_0000 -> Ranged
    Nightmares       = 0x80, // 1000_0000 -> Melee/Ranged

    // Types of specific spawns
    // Fully mask blood door across all base enemies to ensure it doesn't collide
    BloodDoors       = 0x3f, // 0011_1111
    BossAlignedSpawn = 0xa0, // 1010_0000 -> Pure sneak TODO: can we remove?
    MegaMotherSpawn  = 0x90, // 1001_0000

    // Tiers of difficulty
    // Overlaps with specific enemies, but that's fine because we only mask
    // these with the groups that sit in the higher bits
    TierA            = 0x01, // 0000_0001
    TierB            = 0x02, // 0000_0010
    TierC            = 0x03, // 0000_0011
    TierD            = 0x04, // 0000_0100
    TierE            = 0x05, // 0000_0101
}
