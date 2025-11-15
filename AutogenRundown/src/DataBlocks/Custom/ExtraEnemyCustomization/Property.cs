using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Properties;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public class Property
{
    #region Properties

    [JsonProperty("SpawnCostCustom")]
    public List<JObject> SpawnCost { get; set; } = new();

    [JsonProperty("EventsCustom")]
    public List<JObject> Events { get; set; } = new();

    /// <summary>
    /// Customize Roars
    /// </summary>
    [JsonProperty("DistantRoarCustom")]
    public List<DistantRoar> DistantRoars { get; set; } = new();

    #endregion

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
        var path = Path.Combine(dir, "Property.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }

    #endregion
}
