namespace AutogenRundown.DataBlocks.Objectives
{
    /// <summary>
    /// Which objective bulkhead are we in.
    /// </summary>
    [Flags]
    public enum Bulkhead
    {
        None = 0,

        // The three main vanilla dropzones
        Main = 0x01,
        Extreme = 0x02,
        Overload = 0x04,

        /// <summary>
        /// Starting area is part of Main in the data blocks but for our purpose it's the zones
        /// before any of the bulkheads, including the drop zone.
        /// </summary>
        StartingArea = 0x08,

        All = Main | Extreme | Overload | StartingArea,
    }
}
