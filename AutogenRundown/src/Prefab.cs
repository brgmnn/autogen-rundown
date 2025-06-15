using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Enums;
using Newtonsoft.Json;

namespace AutogenRundown;

public record Prefab
{
    /// <summary>
    ///
    /// </summary>
    [JsonProperty("Prefab")]
    public string Asset { get; set; } = "";

    /// <summary>
    ///
    /// </summary>
    public SubComplex SubComplex { get; set; } = SubComplex.All;

    /// <summary>
    ///
    /// </summary>
    public int Shard { get; set; } = 1;
}
