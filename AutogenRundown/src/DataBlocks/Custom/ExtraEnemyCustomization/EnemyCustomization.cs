using BepInEx;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public static class EnemyCustomization
{
    /// <summary>
    ///
    /// </summary>
    public static Ability Ability { get; private set; } = new();

    /// <summary>
    ///
    /// </summary>
    public static EnemyAbility EnemyAbility { get; private set; } = new();

    /// <summary>
    ///
    /// </summary>
    public static Model Model { get; private set; } = new();

    /// <summary>
    ///
    /// </summary>
    public static ProjectileDefs Projectile { get; private set; } = new();

    /// <summary>
    /// Cleans the EEC directory
    /// </summary>
    public static void Setup()
    {
        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();

        try
        {
            Directory.Delete(
                Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}", "Custom", "ExtraEnemyCustomization"),
                recursive: true);
        }
        catch (DirectoryNotFoundException)
        {
            Plugin.Logger.LogWarning("LayoutDefinitions.Setup(): Could not delete ExtraEnemyCustomization dir.");
        }
    }
}
