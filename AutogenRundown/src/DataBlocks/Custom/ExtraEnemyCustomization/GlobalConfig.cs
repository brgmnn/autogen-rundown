using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public class GlobalConfig
{
    private static readonly JObject Config = new()
    {
        // Using Flyer Stuck Check? (Kill stuck flyer after few seconds)
        ["Flyer.StuckCheck.Enabled"] = true,

        // Check Retry Count
        ["Flyer.StuckCheck.RetryCount"] = 5,

        // Check Interval
        ["Flyer.StuckCheck.RetryInterval"] = 1.5,

        // Cache Every Material in-game (For Material Custom)
        // Better leave it as false unless you know what you are doing.
        ["Material.CacheAll"] = false,

        // Using Medipack can stop bleeding effects?
        ["Bleeding.UseMediToStop"] = false,

        // If true, vanilla ids for the unused wave roars will be internally added to DistantRoarCustom (can still be overridden)
        ["WaveRoarFix.AutoAddUnused"] = true
    };

    /// <summary>
    /// We only have one instance of the file, so we will use a singleton pattern for this class
    /// </summary>
    public static void Save()
    {
        var serializer = new JsonSerializer { Formatting = Formatting.Indented };

        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}", "Custom", "ExtraEnemyCustomization");
        var path = Path.Combine(dir, "Global.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, Config);
        stream.Flush();
    }
}
