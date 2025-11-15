using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Abilities;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public class Ability
{
    #region Properties
    #region Used properties

    /// <summary>
    /// Mother birthing abilities customization
    /// </summary>
    [JsonProperty("BirthingCustom")]
    public List<Birthing> Birthings { get; set; } = new();

    /// <summary>
    /// Customizing attacks to deal infection
    /// </summary>
    [JsonProperty("InfectionAttackCustom")]
    public List<InfectionAttack> InfectionAttacks { get; set; } = new();

    #endregion

    #region Unused properties
    /*
     * These properties are still copied by the SupportedMod framework
     */

    [JsonProperty("FogSphereCustom")]
    public List<JObject> FogSphere { get; set; } = new();

    [JsonProperty("HealthRegenCustom")]
    public List<JObject> HealthRegen { get; set; } = new();

    [JsonProperty("ExplosiveAttackCustom")]
    public List<JObject> ExplosiveAttacks { get; set; } = new();

    [JsonProperty("KnockbackAttackCustom")]
    public List<JObject> KnockbackAttacks { get; set; } = new();

    [JsonProperty("BleedAttackCustom")]
    public List<JObject> BleedAttacks { get; set; } = new();

    [JsonProperty("DrainStaminaAttackCustom")]
    public List<JObject> DrainStaminaAttacks { get; set; } = new();

    [JsonProperty("DoorBreakerCustom")]
    public List<JObject> DoorBreaker { get; set; } = new();

    [JsonProperty("ScoutScreamingCustom")]
    public List<JObject> ScoutScreaming { get; set; } = new();

    [JsonProperty("PouncerCustom")]
    public List<JObject> Pouncer { get; set; } = new();

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
