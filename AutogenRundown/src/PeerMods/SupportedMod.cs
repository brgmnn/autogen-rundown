using BepInEx;

namespace AutogenRundown.PeerMods;

public class SupportedMod
{
    protected string ModName = "";

    protected string PluginFolder => Path.Combine(Paths.BepInExRootPath, "plugins", ModName);

    protected string GameDataFolder => Path.Combine(
        Paths.BepInExRootPath,
        "GameData",
        $"{CellBuildData.GetRevision()}");

    /// <summary>
    /// Wholesale copies any found GameData DataBlock files from the mod folder into the game
    /// data folder.
    ///
    /// This is used for mods which add their own DataBlock files which do not collide with any
    /// of the DataBlocks that we write.
    /// </summary>
    protected void CopyGameDataJson()
    {
        var pluginFolder = Path.Combine(Paths.BepInExRootPath, "plugins", ModName);
        var gameDataFiles = Directory.GetFiles(
            pluginFolder,
            "GameData_*DataBlock_bin.json",
            SearchOption.AllDirectories).ToList();

        gameDataFiles.ForEach(path =>
        {
            var filename = Path.GetFileName(path);

            File.Copy(
                path,
                Path.Combine(GameDataFolder, filename),
                overwrite: true);

            Plugin.Logger.LogDebug($"{ModName}: Copied datablock -> {filename}");
        });
    }

    /// <summary>
    /// Copies the GearPartTransform.json file to the GameData folder.
    ///
    /// Note: if we need more than one mod that can do this we will have to combine them somehow.
    /// </summary>
    protected void CopyCustomMccad00()
    {
        var gearPartTransform = Path.Combine("Custom", "mccad00", "GearPartTransform.json");
        var pluginPath = Path.Combine(PluginFolder, gearPartTransform);
        var gameDataPath = Path.Combine(GameDataFolder, gearPartTransform);

        if (!File.Exists(pluginPath))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(gameDataPath)!);

        File.Copy(pluginPath, gameDataPath, overwrite: true);

        Plugin.Logger.LogDebug($"{ModName}: Copied -> {gearPartTransform}");
    }

    /// <summary>
    /// Copies the GearPartTransform.json file to the GameData folder.
    ///
    /// Note: if we need more than one mod that can do this we will have to combine them somehow.
    /// </summary>
    protected void CopyGearPartTransform()
    {
        var gearPartTransform = "GearPartTransform.json";
        var pluginPath = Path.Combine(PluginFolder, "GearPartTransform.json");
        var gameDataPath = Path.Combine(GameDataFolder, "GearPartTransform.json");

        if (!File.Exists(pluginPath))
            return;

        File.Copy(pluginPath, gameDataPath, overwrite: true);

        Plugin.Logger.LogDebug($"{ModName}: Copied -> {gearPartTransform}");
    }
}
