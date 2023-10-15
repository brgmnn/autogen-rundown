namespace AutogenRundown.DataBlocks.Objectives
{
    /// <summary>
    /// Which objective bulkhead are we in.
    /// </summary>
    [Flags]
    public enum Bulkhead
    {
        None = 0,

        Main = 0x01,
        Extreme = 0x02,
        Overload = 0x04,

        All = Main | Extreme | Overload,
    }
}
