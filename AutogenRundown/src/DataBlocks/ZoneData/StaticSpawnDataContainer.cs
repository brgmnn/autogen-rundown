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
    /// </summary>
    public int DistributionWeightType { get; set; } = 0;

    /// <summary>
    ///
    /// </summary>
    public double DistributionWeight { get; set; } = 0.0;

    /// <summary>
    ///
    /// </summary>
    public double DistributionRandomBlend { get; set; } = 0.0;

    /// <summary>
    ///
    /// </summary>
    public double DistributionResultPow { get; set; } = 0.0;

    /// <summary>
    /// Which unit to place. Check the comments in StaticSpawnUnit for more details
    /// </summary>
    [JsonProperty("StaticSpawnDataId")]
    public StaticSpawnUnit Unit { get; set; } = 0u;

    /// <summary>
    /// Not totally sure what this does. May be similar to SubSeed
    /// </summary>
    public int FixedSeed { get; set; } = 1;
}
