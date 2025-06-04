using AutogenRundown.DataBlocks.Custom.AutogenRundown.TerminalPlacements;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.AutogenRundown;

public record LevelTerminalPlacements
{
    private static string Folder => Path.Combine(
        Paths.BepInExRootPath,
        "GameData",
        $"{CellBuildData.GetRevision()}",
        "Custom",
        "AutogenRundown",
        "TerminalPlacements");

    #region Properties
    public string Name { get; set; } = "";

    public uint MainLevelLayout { get; set; } = 0;

    public List<TerminalPosition> Placements { get; set; } = new();
    #endregion

    /// <summary>
    /// Loads all the
    /// </summary>
    /// <returns></returns>
    public static List<LevelTerminalPlacements> LoadAll()
    {
        var results = new List<LevelTerminalPlacements>();

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
                var levelTerminalPlacements = data.ToObject<LevelTerminalPlacements>();

                Plugin.Logger.LogDebug($"Copied datablock -> {filename}");

                if (levelTerminalPlacements is not null)
                    results.Add(levelTerminalPlacements);
            });
        }
        catch (Exception error)
        {
            Plugin.Logger.LogError($"Unable to load LevelTerminalPlacement files: {error.Message}");
        }

        return results;
    }

    public void Save()
    {
        if (!Placements.Any())
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
            "TerminalPlacements");
        var path = Path.Combine(dir, $"{MainLevelLayout}_{Name}.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
}
