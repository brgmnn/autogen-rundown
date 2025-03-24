using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

public class Definition
{
    /// <summary>
    /// Setting this value to null will cause it to be omitted from the JSON
    /// response. Useful for some definitions such as EventsOnScoutScream
    ///
    /// Default = Reality
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? DimensionIndex { get; set; } = "Reality";

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? LayerType
    {
        get => Bulkhead switch
        {
            Bulkhead.Main => "MainLayer",
            Bulkhead.Extreme => "SecondaryLayer",
            Bulkhead.Overload => "ThirdLayer",
            _ => null
        };
        private set { }
    }

    /// <summary>
    /// Setting this value to null will cause it to be omitted from the JSON
    /// response. Useful for some definitions such as EventsOnScoutScream
    ///
    /// Default = Bulkhead.Main
    /// </summary>
    [JsonIgnore]
    public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? LocalIndex
    {
        get => ZoneNumber < 0 ? null : ZoneNumber;
        private set { }
    }

    /// <summary>
    /// Setting this value to null will cause it to be omitted from the JSON
    /// response. Useful for some definitions such as EventsOnScoutScream
    ///
    /// Default = 0
    /// </summary>
    [JsonIgnore]
    public int ZoneNumber { get; set; } = 0;
}
