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

    #region Terminal positions

    /// <summary>
    ///
    /// We want to have at least 4 candidates per geomorph
    ///
    /// This will let us have some kind of interesting multi-terminal challenge
    /// </summary>
    /// <param name="geomorph"></param>
    /// <returns></returns>
    public static List<(Vector3 Position, Vector3 Rotation)> GetCandidates(string geomorph)
    {
        // Position terminal
        // X -> +left/-right
        // Y -> +up/-down
        // Z -> +backward/-forward
        // Rotate Y -> -right/+left

        return geomorph switch
        {
            // AlphaOne
            "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Boss_Arena.prefab" => new List<(Vector3 Position, Vector3 Rotation)>
            {
                // Central level to the left
                (new Vector3 { X = 37.45, Y = 7.4, Z = 16.5 }, new Vector3 { Y = 190 }),

                // Top, middle of a container
                (new Vector3 { X = 30.05, Y = 18.0, Z = -3.5 }, new Vector3 { Y = -100 }),

                // Far left corner, lower level
                (new Vector3 { X = 56.14, Y = -2.7, Z = 32.2 }, new Vector3 { Y = 180 }),

                // Central, the vanilla location
                (new Vector3 { X = 33.9, Y = 6.1, Z = -0.92 }, new Vector3 { Y = -90 }),
            },

            // AlphaThree_Top
            "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_R6A2.prefab" => new List<(Vector3, Vector3)>
            {
                // Pretty central outside with backplate (the vanilla location)
                (new Vector3 { X = 11.4, Y = 747.4, Z = 11.7 }, new Vector3()),

                // By the balcony
                (new Vector3 { X =  8.8, Y = 747.5, Z = 27.8 }, new Vector3 { Y = -90 }),

                // Inside the smaller building by the stairs
                (new Vector3 { X = -8.6, Y = 745.2, Z = -7.6 }, new Vector3 { Y = 200 }),

                // Up on the roof by the two ladders
                (new Vector3 { X = -9.4, Y = 749.3, Z = -5.8 }, new Vector3 { Y = 90 }),
            },

            _ => new List<(Vector3, Vector3)> { (Vector3.Zero(), Vector3.Zero()) }
        };
    }

    #endregion

    #region Filesystem

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

    #endregion
}
