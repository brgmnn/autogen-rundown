using AutogenRundown.DataBlocks.Custom.ZoneSensors;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.AutogenRundown;

public record LevelZoneSensors
{
    private static string Folder => Path.Combine(
        Paths.BepInExRootPath,
        "GameData",
        $"{CellBuildData.GetRevision()}",
        "Custom",
        "AutogenRundown",
        "ZoneSensors");

    #region Properties
    public string Name { get; set; } = "";

    public uint MainLevelLayout { get; set; } = 0;

    public List<ZoneSensorDefinition> Definitions { get; set; } = new();
    #endregion

    /// <summary>
    /// Loads all zone sensor definitions from JSON files.
    /// </summary>
    public static List<LevelZoneSensors> LoadAll()
    {
        var results = new List<LevelZoneSensors>();

        try
        {
            if (!Directory.Exists(Folder))
                return results;

            var files = Directory.GetFiles(
                Folder,
                "*.json",
                SearchOption.AllDirectories).ToList();

            files.ForEach(path =>
            {
                var filename = Path.GetFileName(path);
                var data = JObject.Parse(File.ReadAllText(path));
                var levelZoneSensors = data.ToObject<LevelZoneSensors>();

                Plugin.Logger.LogDebug($"Loaded zone sensors -> {filename}");

                if (levelZoneSensors is not null)
                    results.Add(levelZoneSensors);
            });
        }
        catch (Exception error)
        {
            Plugin.Logger.LogError($"Unable to load LevelZoneSensors files: {error.Message}");
        }

        return results;
    }

    public void Save()
    {
        if (!Definitions.Any())
            return;

        var serializer = new JsonSerializer { Formatting = Formatting.Indented };

        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(
            Paths.BepInExRootPath,
            "GameData",
            $"{revision}",
            "Custom",
            "AutogenRundown",
            "ZoneSensors");
        var path = Path.Combine(dir, $"{MainLevelLayout}_{Name.Replace(" ", "_")}.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
}
