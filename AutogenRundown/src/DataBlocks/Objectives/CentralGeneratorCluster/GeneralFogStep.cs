using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Objectives.CentralGeneratorCluster;

/// <summary>
/// Each step represents what the fog level should be at after that many cells have been inserted
/// </summary>
public record GeneralFogStep
{
    /// <summary>
    /// Which fog setting to use
    /// </summary>
    [JsonIgnore]
    public Fog Fog { get; set; } = Fog.None;

    /// <summary>
    /// This is what the datablocks read for the fog id
    /// </summary>
    [JsonProperty("m_fogDataId")]
    public uint FogId => Fog.PersistentId;

    /// <summary>
    /// How long it should take to transition between the fog levels
    ///
    /// Default = 14.0
    /// </summary>
    [JsonProperty("m_transitionToTime")]
    public double TransitionTime { get; set; } = 14.0;
}
