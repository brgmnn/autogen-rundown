using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public record Dimension : DataBlock
{
    /// <summary>
    /// For some reason the devs decided to put all the properties under this data key
    /// </summary>
    [JsonProperty("DimensionData")]
    public DimensionData Data { get; init; } = new();
}
