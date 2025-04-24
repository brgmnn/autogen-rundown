using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Bones;

public class Prefab
{
    public string Bone { get; set; } = "Head";

    [JsonProperty("Prefab")]
    public string Path { get; set; } = "Assets/PrefabToAttach";

    public string Scale { get; set; } = "1, 1, 1";

    public string Position { get; set; } = "0, 0, 0";

    public string Rotation { get; set; } = "0, 0, 0";
}
