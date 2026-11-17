using AutogenRundown.DataBlocks.Enums;
using BepInEx;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.LGTuner;

/// <summary>
/// LGTuner per-layout config, read from Custom/LGTuner/. We only use it to load the asset
/// shards of extra complexes: some custom tiles place another complex's markers (e.g.
/// Lab_I_RLC_02 uses mining markers), and those marker prefabs only resolve if that complex's
/// shards are loaded alongside the level's own complex.
/// </summary>
public record LayoutConfig
{
    [JsonIgnore]
    public string Name { get; set; } = "";

    /// <summary>
    /// LGTuner matches this against the main, secondary, third and dimension layouts.
    /// </summary>
    public uint LevelLayoutID { get; set; }

    [JsonProperty(ItemConverterType = typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public List<Complex> ExtraComplexResourceToLoad { get; set; } = new();

    public void Save()
    {
        var dir = Path.Combine(
            Paths.BepInExRootPath,
            "GameData",
            $"{CellBuildData.GetRevision()}",
            "Custom",
            "LGTuner");
        var path = Path.Combine(dir, $"{LevelLayoutID}_{Name.Replace(" ", "_")}.json");

        Directory.CreateDirectory(dir);

        File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
