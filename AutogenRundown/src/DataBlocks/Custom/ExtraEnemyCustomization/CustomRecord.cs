using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public record CustomRecord
{
    public readonly bool Enabled = true;

    [JsonProperty("DebugName")]
    public string Name { get; set; } = "";

    /// <summary>
    ///
    /// </summary>
    public Target Target { get; set; } = new();
}
