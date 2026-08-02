namespace AutogenRundown.DataBlocks.Levels;

/// <summary>
/// A level signature is a single defining mechanic for a level, at most one per level.
/// Currently only rolled in the E-tier branch of LevelSettings.Generate().
///
/// See docs/dev/e-tier-difficulty.md, Group C.
/// </summary>
public enum LevelSignature
{
    None,

    /// <summary>
    /// Recurring stalker: a scripted pseudo-error alarm sends a single shadow pouncer every
    /// 4-6 minutes with a heartbeat tell. Finite, never escalates, no combat music.
    /// </summary>
    Stalker,

    /// <summary>
    /// R4E1-style boss error alarm: a real TriggerAlarm error wave streams a boss
    /// (Tank/TankPotato/Mother) from level start until the Main objective completes, then
    /// all waves stop (StopAllWavesBeforeGotoWin).
    /// </summary>
    BossAlarm,
}
