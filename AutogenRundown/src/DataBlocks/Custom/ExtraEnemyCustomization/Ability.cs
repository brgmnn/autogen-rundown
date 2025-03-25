using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Abilities;
using BepInEx;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public class Ability
{
    #region Properties

    /// <summary>
    /// Mother birthing abilities customization
    /// </summary>
    [JsonProperty("BirthingCustom")]
    public ICollection<Birthing> Birthings { get; set; } = new List<Birthing>();

    #endregion

    /// <summary>
    ///
    /// </summary>
    public void Setup()
    {

    }

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
        var path = Path.Combine(dir, "Ability.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
    #endregion
}
