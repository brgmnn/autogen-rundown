using AutogenRundown.DataBlocks.Enums;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public class ZonePlacementData
{
    /// <summary>
    /// What dimension this zone exists in
    ///
    /// Default = Reality (0)
    /// </summary>
    [JsonProperty("DimensionIndex")]
    public DimensionIndex Dimension { get; set; } = DimensionIndex.Reality;

    /// <summary>
    /// Target zone number for this placement data
    /// </summary>
    public int LocalIndex { get; set; } = 0;

    public ZonePlacementWeights Weights { get; set; } = new();
}
