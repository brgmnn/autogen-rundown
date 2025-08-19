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
    [JsonProperty("m_fogDataId")]
    public Fog Fog { get; set; } = Fog.None;

    /// <summary>
    /// How long it should take to transition between the fog levels
    /// </summary>
    [JsonProperty("m_transitionToTime")]
    public double TransitionTime { get; set; } = 5.0;
}
