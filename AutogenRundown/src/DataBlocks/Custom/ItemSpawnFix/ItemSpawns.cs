using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ItemSpawnFix;

public class ItemSpawns
{
    public static void Save()
    {
        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}", "Custom", "ItemSpawnFix");

        var serializer = new JsonSerializer { Formatting = Formatting.Indented };

        var path = Path.Combine(dir, "ItemSpawnFix.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, new JArray
        {
            new JObject
            {
                ["RundownID"] = 0,
                ["Levels"] = new JArray { "A", "B", "C", "D", "E" },
                ["RaiseObjectSpawnPriority"] = true,
                ["AllowRedistributeObjects"] = false
            }
        });
        stream.Flush();
    }
}
