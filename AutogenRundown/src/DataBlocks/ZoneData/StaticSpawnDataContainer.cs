using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.ZoneData;

public class StaticSpawnDataContainer
{
    /// <summary>
    /// How many to spawn
    /// </summary>
    public int Count { get; set; } = 0;

    /// <summary>
    /// Defines random distribution type.
    /// Weight_is_zeroToOne_startToEnd - scales DistributionWeight from zero to one by area.
    /// Weight_is_exact_node_index - clamps DistributionWeight to area index.
    ///
    /// Default 0
    /// </summary>
    public int DistributionWeightType { get; set; } = 0;

    /// <summary>
    /// Default 1.0
    /// </summary>
    public double DistributionWeight { get; set; } = 1.0;

    /// <summary>
    /// Default 0.5
    /// </summary>
    public double DistributionRandomBlend { get; set; } = 0.5;

    /// <summary>
    /// Default 2.0
    /// </summary>
    public double DistributionResultPow { get; set; } = 2.0;

    /// <summary>
    /// Which unit to place. Check the comments in StaticSpawnUnit for more details
    /// </summary>
    [JsonProperty("StaticSpawnDataId")]
    public StaticSpawnUnit Unit { get; set; } = 0u;

    /// <summary>
    /// Not totally sure what this does. May be similar to SubSeed
    /// </summary>
    public int FixedSeed { get; set; } = Generator.Between(2, 150);
}
