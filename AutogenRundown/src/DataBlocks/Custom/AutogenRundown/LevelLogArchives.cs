using AutogenRundown.DataBlocks.Custom.AutogenRundown.LogArchives;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.AutogenRundown;

public record LevelLogArchives
{
    private static string Folder => Path.Combine(
        Paths.BepInExRootPath,
        "GameData",
        $"{CellBuildData.GetRevision()}",
        "Custom",
        "AutogenRundown",
        "LogArchives");

    #region Properties
    public string Name { get; set; } = "";

    public uint MainLevelLayout { get; set; } = 0;

    public List<Log> Logs { get; set; } = new();
    #endregion

    /// <summary>
    /// Loads all the level log archives
    /// </summary>
    /// <returns></returns>
    public static List<LevelLogArchives> LoadAll()
    {
        var results = new List<LevelLogArchives>();

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
                var levelLogArchives = data.ToObject<LevelLogArchives>();

                Plugin.Logger.LogDebug($"Copied datablock -> {filename}");

                if (levelLogArchives is not null)
                    results.Add(levelLogArchives);
            });
        }
        catch (Exception error)
        {
            Plugin.Logger.LogError($"Unable to load LevelLogArchives files: {error.Message}");
        }

        return results;
    }

    public void Save()
    {
        if (!Logs.Any())
            return;

        var serializer = new JsonSerializer { Formatting = Formatting.Indented };

        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(
            Paths.BepInExRootPath,
            "GameData",
            $"{revision}",
            "Custom",
            "AutogenRundown",
            "LogArchives");
        var path = Path.Combine(dir, $"{MainLevelLayout}_{Name.Replace(" ", "_")}.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
}
