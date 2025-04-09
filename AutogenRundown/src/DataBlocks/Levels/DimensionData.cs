using AutogenRundown.DataBlocks.Enums;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Levels;

public class DimensionData
{
    public DimensionIndex Dimension { get; set; } = DimensionIndex.Reality;

    [JsonProperty("DimensionData")]
    public uint DataPid { get; set; } = 0;

    public bool Enabled { get; set; } = true;
}
