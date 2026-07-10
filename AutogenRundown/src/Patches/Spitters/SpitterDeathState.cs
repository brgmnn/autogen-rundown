using System.Runtime.InteropServices;

namespace AutogenRundown.Patches.Spitters;

/// <summary>
/// Replicated dead-spitter bitmask for one shard of the spitter index space.
/// Used with AmorLib StateReplicator for multiplayer sync (host broadcast +
/// late-joiner recall).
///
/// AmorLib state payloads are capped at 256 bytes, so the full index space is
/// split across SpitterKillManager.SHARD_COUNT replicators. Each shard covers
/// SpittersPerShard consecutive spitter indices:
///   shard = spitterIndex / SpittersPerShard
///   local = spitterIndex % SpittersPerShard
///
/// Struct size: 1 (ShardIndex) + 240 (mask) = 241 bytes.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct SpitterDeathState
{
    public const int MaskBytes = 240;

    /// <summary>Spitter indices covered per shard: 240 * 8 = 1920.</summary>
    public const int SpittersPerShard = MaskBytes * 8;

    /// <summary>
    /// Which shard this state belongs to. Set once at replicator creation;
    /// carried in the payload as a debugging cross-check.
    /// </summary>
    public byte ShardIndex;

    private fixed byte _deadBits[MaskBytes];

    /// <summary>Maps a global spitter index to its (shard, local) coordinates.</summary>
    public static (int shard, int local) MapIndex(ushort spitterIndex)
        => (spitterIndex / SpittersPerShard, spitterIndex % SpittersPerShard);

    // Note: not marked readonly — taking a pointer to a fixed buffer is not
    // permitted inside readonly members.
    public bool IsDead(int localIndex)
    {
        if (localIndex < 0 || localIndex >= SpittersPerShard)
            return false;

        fixed (byte* bits = _deadBits)
            return (bits[localIndex >> 3] & (1 << (localIndex & 7))) != 0;
    }

    public void SetDead(int localIndex)
    {
        if (localIndex < 0 || localIndex >= SpittersPerShard)
            return;

        fixed (byte* bits = _deadBits)
            bits[localIndex >> 3] |= (byte)(1 << (localIndex & 7));
    }

    public void ClearDead(int localIndex)
    {
        if (localIndex < 0 || localIndex >= SpittersPerShard)
            return;

        fixed (byte* bits = _deadBits)
            bits[localIndex >> 3] &= (byte)~(1 << (localIndex & 7));
    }

    /// <summary>Raw mask byte access, used to diff old vs new state cheaply.</summary>
    public byte GetMaskByte(int byteIndex)
    {
        if (byteIndex < 0 || byteIndex >= MaskBytes)
            return 0;

        fixed (byte* bits = _deadBits)
            return bits[byteIndex];
    }
}
