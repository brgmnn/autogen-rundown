using System.Runtime.InteropServices;

namespace AutogenRundown.Events;

/// <summary>
/// NetworkAPI payload broadcasting a spitter's remaining health fraction from
/// the host to all peers, so every player sees the same damage-state glow
/// (see SpitterVisuals). Cosmetic and fire-and-forget: a late joiner sees the
/// default glow until the spitter's next hit (accepted limitation).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SpitterHealthEvent
{
    public ushort SpitterIndex { get; set; }

    /// <summary>Remaining health fraction, 0..1.</summary>
    public float HealthRel { get; set; }
}
