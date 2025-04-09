using AutogenRundown.DataBlocks.Enums;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Levels;

public class DimensionData
{
    public DimensionIndex Dimension { get; set; } = DimensionIndex.Reality;

    [JsonProperty("DimensionData")]
    public uint DataPersistentId
    {
        get => Data.PersistentId;
        private set { }
    }

    [JsonIgnore]
    public Dimension Data { get; set; } = DataBlocks.Dimension.None;

    public bool Enabled { get; set; } = true;
}
