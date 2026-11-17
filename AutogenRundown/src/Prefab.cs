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

    /// <summary>
    /// Other complexes whose asset shards this tile needs loaded, e.g. a Tech tile that places
    /// Mining markers. Levels using the tile export these to LGTuner. Not part of the datablock.
    /// </summary>
    [JsonIgnore]
    public List<Complex> ExtraComplexes { get; set; } = new();
}
