using AutogenRundown.DataBlocks;
using Newtonsoft.Json;

namespace AutogenRundown;

public record Prefab
{
    /// <summary>
    ///
    /// </summary>
    [JsonProperty("Prefab")]
    public string Value { get; set; } = "";

    /// <summary>
    ///
    /// </summary>
    public SubComplex SubComplex { get; set; } = SubComplex.All;

    /// <summary>
    ///
    /// </summary>
    public string Shard { get; set; } = "S1";
}
