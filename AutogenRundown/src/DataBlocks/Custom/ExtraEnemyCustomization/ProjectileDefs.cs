using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Projectiles;
using BepInEx;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public class ProjectileDefs
{
    #region Properties

    /// <summary>
    /// Shadow (invisible) enemy customization
    /// </summary>
    [JsonProperty("ShooterFireCustom")]
    public ICollection<ShooterFire> ShooterFires { get; set; } = new List<ShooterFire>();

    /// <summary>
    /// Allows configuring enemy materials
    /// </summary>
    [JsonProperty("ProjectileDefinitions")]
    public ICollection<Projectiles.Projectile> Projectiles { get; set; } = new List<Projectiles.Projectile>();

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
        var path = Path.Combine(dir, "Projectile.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
    #endregion
}
