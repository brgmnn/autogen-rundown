using AutogenRundown.DataBlocks.Custom.AutogenRundown.SecurityDoors;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.AutogenRundown;

public record LevelSecurityDoors
{
    private static string Folder => Path.Combine(
        Paths.BepInExRootPath,
        "GameData",
        $"{CellBuildData.GetRevision()}",
        "Custom",
        "AutogenRundown",
        "SecurityDoors");

    #region Properties
    public string Name { get; set; } = "";

    public uint MainLevelLayout { get; set; } = 0;

    public List<SecurityDoor> Doors { get; set; } = new();
    #endregion

    /// <summary>
    /// Loads all the
    /// </summary>
    /// <returns></returns>
    public static List<LevelSecurityDoors> LoadAll()
    {
        var results = new List<LevelSecurityDoors>();
        var files = Directory.GetFiles(
            Folder,
            "*.json",
            SearchOption.AllDirectories).ToList();

        files.ForEach(path =>
        {
            var filename = Path.GetFileName(path);
            var data = JObject.Parse(File.ReadAllText(path));
            var levelSecurityDoors = data.ToObject<LevelSecurityDoors>();

            Plugin.Logger.LogDebug($"Copied datablock -> {filename}");

            if (levelSecurityDoors is not null)
                results.Add(levelSecurityDoors);
        });

        return results;
    }

    public void Save()
    {
        if (!Doors.Any())
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
            "SecurityDoors");
        var path = Path.Combine(dir, $"{MainLevelLayout}_{Name}.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
}
