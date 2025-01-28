using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

public class LayoutDefinitions
{
    [JsonIgnore]
    public string Folder { get; set; } = "";

    [JsonIgnore]
    public string Name { get; set; } = "";

    public uint MainLevelLayout { get; set; } = 0u;

    public List<Definition> Definitions { get; set; } = new();

    public void Save()
    {
        var serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;

        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(
            Paths.BepInExRootPath,
            "GameData",
            $"{revision}",
            "Custom",
            "ExtraObjectiveSetup",
            Folder);
        var path = Path.Combine(dir, $"{MainLevelLayout}_{Name}.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using StreamWriter stream = new StreamWriter(path);
        using JsonWriter writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
}
