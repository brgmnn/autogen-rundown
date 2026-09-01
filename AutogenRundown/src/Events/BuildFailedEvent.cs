using System.Runtime.InteropServices;

namespace AutogenRundown.Events;

/// <summary>
/// Broadcast by the host when a level has exhausted its rebuild budget and the drop is being
/// aborted. Every peer uses this to freeze its own factory, persist the failure, and lock the
/// expedition out of the rundown.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct BuildFailedEvent
{
    /// <summary>
    /// Persistent id of the rundown data block the level belongs to.
    /// </summary>
    public uint RundownId { get; set; }

    /// <summary>
    /// eRundownTier value (TierA = 1 ... TierE = 5).
    /// </summary>
    public int Tier { get; set; }

    /// <summary>
    /// Zero based index of the expedition within its tier.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Main layout persistent id. Used to verify the receiving peer generated the same level
    /// before it locks anything out.
    /// </summary>
    public uint MainLevelLayout { get; set; }

    /// <summary>
    /// Rebuilds attempted before the host gave up. Displayed in the popup.
    /// </summary>
    public int Rebuilds { get; set; }
}
