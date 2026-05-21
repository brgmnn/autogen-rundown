using AutogenRundown.DataBlocks.Custom.AutogenRundown.AreaCounts;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.AutogenRundown;

public record LevelAreaCounts
{
    private static string Folder => Path.Combine(
        Paths.BepInExRootPath,
        "GameData",
        $"{CellBuildData.GetRevision()}",
        "Custom",
        "AutogenRundown",
        "AreaCounts");

    #region Properties
    public string Name { get; set; } = "";

    public uint MainLevelLayout { get; set; } = 0;

    public List<AreaCountZone> Zones { get; set; } = new();
    #endregion

    public static List<LevelAreaCounts> LoadAll()
    {
        var results = new List<LevelAreaCounts>();

        try
        {
            var files = Directory.GetFiles(
                Folder,
                "*.json",
                SearchOption.AllDirectories).ToList();

            files.ForEach(path =>
            {
                var filename = Path.GetFileName(path);
                var data = JObject.Parse(File.ReadAllText(path));
                var levelAreaCounts = data.ToObject<LevelAreaCounts>();

                Plugin.Logger.LogDebug($"Copied datablock -> {filename}");

                if (levelAreaCounts is not null)
                    results.Add(levelAreaCounts);
            });
        }
        catch (Exception error)
        {
            Plugin.Logger.LogError($"Unable to load LevelAreaCounts files: {error.Message}");
        }

        return results;
    }

    public void Save()
    {
        if (!Zones.Any())
            return;

        var serializer = new JsonSerializer { Formatting = Formatting.Indented };

        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(
            Paths.BepInExRootPath,
            "GameData",
            $"{revision}",
            "Custom",
            "AutogenRundown",
            "AreaCounts");
        var path = Path.Combine(dir, $"{MainLevelLayout}_{Name.Replace(" ", "_")}.json");

        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
}
