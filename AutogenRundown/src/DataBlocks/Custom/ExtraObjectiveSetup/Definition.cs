using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

public class Definition
{
    public string DimensionIndex { get; set; } = "Reality";

    // TODO: check they work in all bulkheads
    // Checked main and overload
    public string LayerType
    {
        get => Bulkhead switch
        {
            Bulkhead.Main => "MainLayer",
            Bulkhead.Extreme => "SecondaryLayer",
            Bulkhead.Overload => "ThirdLayer",
        };
        private set { }
    }

    [JsonIgnore]
    public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

    public int LocalIndex
    {
        get => ZoneNumber;
        private set { }
    }

    [JsonIgnore]
    public int ZoneNumber { get; set; } = 0;
}
