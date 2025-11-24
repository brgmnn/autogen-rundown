using Newtonsoft.Json;

namespace AutogenRundown.PeerMods;

public class Mod
{
    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("version_number")]
    public string Version { get; set; } = "";

    [JsonIgnore]
    public string Path { get; set; } = "";
}
