using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

public class ObjectiveLayerData
{
    /// <summary>
    /// Local zone index where the bulkhead entrace will be
    /// </summary>
    public List<int> ZonesWithBulkheadEntrance { get; set; } = new();

    public List<BulkheadDoorPlacementData> BulkheadDoorControllerPlacements { get; set; } = new();

    public List<List<ZonePlacementData>> BulkheadKeyPlacements { get; set; } = new();

    public WardenObjectiveLayerData ObjectiveData { get; set; } = new();

    // TODO: Can we have multiple objectives chained in one zone? :hmm:
    public List<WardenObjectiveLayerData> ChainedObjectiveData { get; set; } = new();

    /// <summary>
    /// We disable artifacts
    /// </summary>
    [JsonProperty("ArtifactData")]
    public static JObject Artifacts => new()
    {
        ["ArtifactAmountMulti"] = 0.0,
        ["ArtifactLayerDistributionDataID"] = 0,
        ["ArtifactZoneDistributions"] = new JArray()
    };
}
