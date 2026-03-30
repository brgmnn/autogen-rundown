using AutogenRundown.Patches.CustomTerminals;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.AutogenRundown;

public record LevelCustomTerminals
{
    private static string Folder => Path.Combine(
        Paths.BepInExRootPath,
        "GameData",
        $"{CellBuildData.GetRevision()}",
        "Custom",
        "AutogenRundown",
        "CustomTerminals");

    #region Properties
    public string Name { get; set; } = "";

    public uint MainLevelLayout { get; set; } = 0;

    public List<CustomTerminalSpawnRequest> Requests { get; set; } = new();
    #endregion

    public static List<LevelCustomTerminals> LoadAll()
    {
        var results = new List<LevelCustomTerminals>();

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
                var levelCustomTerminals = data.ToObject<LevelCustomTerminals>();

                Plugin.Logger.LogDebug($"Copied datablock -> {filename}");

                if (levelCustomTerminals is not null)
                    results.Add(levelCustomTerminals);
            });
        }
        catch (Exception error)
        {
            Plugin.Logger.LogError($"Unable to load LevelCustomTerminals files: {error.Message}");
        }

        return results;
    }

    public void Save()
    {
        if (!Requests.Any())
            return;

        var serializer = new JsonSerializer { Formatting = Formatting.Indented };

        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(
            Paths.BepInExRootPath,
            "GameData",
            $"{revision}",
            "Custom",
            "AutogenRundown",
            "CustomTerminals");
        var path = Path.Combine(dir, $"{MainLevelLayout}_{Name.Replace(" ", "_")}.json");

        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
}
