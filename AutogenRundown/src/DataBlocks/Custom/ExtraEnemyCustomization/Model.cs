using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models;
using BepInEx;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public class Model
{
    #region Properties

    /// <summary>
    /// Shadow (invisible) enemy customization
    /// </summary>
    [JsonProperty("ShadowCustom")]
    public ICollection<Shadow> Shadows { get; set; } = new List<Shadow>();

    /// <summary>
    /// Allows configuring enemy materials
    /// </summary>
    [JsonProperty("MaterialCustom")]
    public ICollection<Material> Materials { get; set; } = new List<Material>();

    /// <summary>
    /// Allows configuring enemy materials
    /// </summary>
    [JsonProperty("GlowCustom")]
    public ICollection<Glow> Glows { get; set; } = new List<Glow>();

    /// <summary>
    /// Body part transformation
    /// </summary>
    [JsonProperty("BoneCustom")]
    public ICollection<BoneCustom> Bones { get; set; } = new List<BoneCustom>();

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
