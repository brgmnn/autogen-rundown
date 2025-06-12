using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

public class ZoneSelectionData
{
    public string DimensionIndex { get; set; } = "Reality";

    // TODO: check they work in all bulkheads
    public string LayerType
    {
        get => Bulkhead switch
        {
            Bulkhead.Main => "MainLayer",
            Bulkhead.Extreme => "SecondaryLayer",
            Bulkhead.Overload => "ThirdLayer",
            _ => "MainLayer",
        };
        private set { }
    }

    [JsonIgnore]
    public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

    [JsonIgnore]
    public int ZoneNumber { get; set; } = 0;

    public string LocalIndex
    {
        get => $"Zone_{ZoneNumber}";
        private set { }
    }

    public string SeedType { get; set; } = "SessionSeed";

    public int TerminalIndex { get; set; } = 0;

    public int StaticSeed { get; set; } = 0;
}
