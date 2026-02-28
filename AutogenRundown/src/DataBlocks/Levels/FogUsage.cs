namespace AutogenRundown.DataBlocks.Levels;

/// <summary>
/// Tracks how fog is being used in a level to prevent incompatible fog transitions.
///
/// Short-duration fog events (alarm fog floods, CGC fog steps, reactor wave fog)
/// are compatible with each other. Long-duration fog transitions (slowly rising/lowering
/// fog over the entire level) are exclusive — nothing else may change the fog.
/// </summary>
public enum FogUsage
{
    /// No fog modifications planned. Anything is compatible.
    None,

    /// Short-duration fog events are active (alarm floods, CGC steps, etc).
    /// More short-duration events are OK. Long-duration transitions are blocked.
    ShortDuration,

    /// A long-duration fog transition is active (e.g. slowly rising fog over minutes).
    /// No other fog changes are allowed.
    LongDuration
}
