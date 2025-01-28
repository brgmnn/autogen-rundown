using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

public class Definition
{
    public string DimensionIndex { get; set; } = "Reality";

    public string LayerType { get; set; } = "MainLayer";

    public string LocalIndex
    {
        get => $"Zone_{ZoneNumber}";
        private set { }
    }

    [JsonIgnore]
    public int ZoneNumber { get; set; } = 0;
}
