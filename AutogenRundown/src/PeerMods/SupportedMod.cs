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
    ///
    /// </summary>
    protected void CopyCustom(string path)
    {
        var customPath = Path.Combine("Custom", path);
        var pluginPath = Path.Combine(PluginFolder, customPath);
        var gameDataPath = Path.Combine(GameDataFolder, customPath);

        if (!Directory.Exists(pluginPath))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(gameDataPath)!);

        Plugin.Logger.LogDebug($"{ModName}: Copying \"Custom/{path}\"");
        CopyDirectory(pluginPath, gameDataPath);
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destinationPath"></param>
    /// <param name="overwrite"></param>
    /// <param name="recursive"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    private void CopyDirectory(
        string sourcePath,
        string destinationPath,
        bool overwrite = true,
        bool recursive = true)
    {
        var dir = new DirectoryInfo(sourcePath);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        var dirs = dir.GetDirectories();

        Directory.CreateDirectory(destinationPath);

        // Get the files in the source directory and copy to the destination directory
        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationPath, file.Name);

            file.CopyTo(targetFilePath, overwrite: overwrite);

            var logFile = GetRelativePathFromCommonRoot(targetFilePath, destinationPath);

            Plugin.Logger.LogDebug($"{ModName}: Copied -> {logFile}");
        }

        if (!recursive)
            return;

        foreach (var subDir in dirs)
        {
            var newDestinationDir = Path.Combine(destinationPath, subDir.Name);

            CopyDirectory(subDir.FullName, newDestinationDir, overwrite, true);
        }
    }

    public static string GetRelativePathFromCommonRoot(string path1, string path2)
    {
        // Normalize the paths
        path1 = Path.GetFullPath(path1);
        path2 = Path.GetFullPath(path2);

        // Split paths into segments
        string[] segments1 = path1.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string[] segments2 = path2.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // Find the common root by comparing segments
        int commonLength = 0;
        int minLength = Math.Min(segments1.Length, segments2.Length);

        for (int i = 0; i < minLength; i++)
        {
            if (string.Equals(segments1[i], segments2[i], StringComparison.OrdinalIgnoreCase))
            {
                commonLength++;
            }
            else
            {
                break;
            }
        }

        // If no common root found
        if (commonLength == 0)
        {
            throw new ArgumentException("No common root found between the paths.");
        }

        // Get the segments after the common root for path1
        string[] remainingSegments = segments1.Skip(commonLength).ToArray();

        // Join them back together
        return string.Join(Path.DirectorySeparatorChar.ToString(), remainingSegments);
    }
}
