using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Abilities;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.EnemyAbilities;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public class EnemyAbility
{
    #region Properties

    /// <summary>
    /// Mother birthing abilities customization
    /// </summary>
    [JsonProperty("DeathAbilityCustom")]
    public ICollection<DeathAbility> DeathAbilities { get; set; } = new List<DeathAbility>();

    [JsonIgnore]
    public List<FogSphereAbility> FogSphereAbilities { get; set; } = new();

    [JsonIgnore]
    public List<SpawnEnemyAbility> SpawnEnemyAbilities { get; set; } = new();

    /// <summary>
    ///
    /// </summary>
    public JObject Abilities => new JObject
    {
        ["FogSphere"] = JArray.FromObject(FogSphereAbilities),
        ["SpawnEnemy"] = JArray.FromObject(SpawnEnemyAbilities)
    };

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
        var path = Path.Combine(dir, "EnemyAbility.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
    #endregion
}
