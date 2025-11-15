using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Abilities;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.EnemyAbilities;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public class EnemyAbilityAblities
{
    public List<JObject> Chain { get; set; } = new();

    public List<FogSphereAbility> FogSphere { get; set; } = new();

    public List<JObject> Explosion { get; set; } = new();

    public List<SpawnEnemyAbility> SpawnEnemy { get; set; } = new();

    public List<JObject> SpawnWave { get; set; } = new();

    public List<JObject> SpawnProjectile { get; set; } = new();

    public List<JObject> DoAnim { get; set; } = new();

    public List<JObject> Cloak { get; set; } = new();

    public List<JObject> EMP { get; set; } = new();
}

public class EnemyAbility
{
    #region Properties

    #region Used Properties

    /// <summary>
    /// Mother birthing abilities customization
    /// </summary>
    [JsonProperty("DeathAbilityCustom")]
    public List<DeathAbility> DeathAbilities { get; set; } = new();

    /// <summary>
    ///
    /// </summary>
    public EnemyAbilityAblities Abilities { get; set; } = new();

    #endregion

    #region Unused Properties

    [JsonProperty("BehaviourAbilityCustom")]
    public List<JObject> BehaviourAbilities { get; set; } = new();

    [JsonProperty("LimbDestroyedAbilityCustom")]
    public List<JObject> LimbDestroyedAbilities { get; set; } = new();

    #endregion
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
