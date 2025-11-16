using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public class Model
{
    #region Properties

    /// <summary>
    /// Shadow (invisible) enemy customization
    /// </summary>
    [JsonProperty("ShadowCustom")]
    public List<Shadow> Shadows { get; set; } = new();

    /// <summary>
    /// Allows configuring enemy materials
    /// </summary>
    [JsonProperty("MaterialCustom")]
    public List<Material> Materials { get; set; } = new();

    /// <summary>
    /// Allows configuring enemy materials
    /// </summary>
    [JsonProperty("GlowCustom")]
    public List<Glow> Glows { get; set; } = new();

    public List<JObject> BoneCustom { get; set; } = new();

    public List<JObject> LimbCustom { get; set; } = new();

    public List<JObject> AnimHandleCustom { get; set; } = new();

    public List<JObject> ModelRefCustom { get; set; } = new();

    public List<JObject> MarkerCustom { get; set; } = new();

    public List<JObject> ScannerCustom { get; set; } = new();

    public List<JObject> SilhouetteCustom { get; set; } = new();

    #endregion

    /// <summary>
    ///
    /// </summary>
    public void Setup() { }

    #region Filesystem
    /// <summary>
    /// We only have one instance of the file, so we will use a singleton pattern for this class
    /// </summary>
    public void Save()
    {
        var serializer = new JsonSerializer { Formatting = Formatting.Indented };

        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}", "Custom", "ExtraEnemyCustomization");
        var path = Path.Combine(dir, "Model.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
    #endregion
}
