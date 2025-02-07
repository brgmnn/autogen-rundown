namespace AutogenRundown.DataBlocks;

public class ZonePlacementData
{
    public int DimensionIndex { get; set; } = 0;

    /// <summary>
    /// Target zone number for this placement data
    /// </summary>
    public int LocalIndex { get; set; } = 0;

    public ZonePlacementWeights Weights { get; set; } = new();
}
