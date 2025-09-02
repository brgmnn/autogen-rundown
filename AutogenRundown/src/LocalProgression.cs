using AutogenRundown.DataBlocks;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown;

public class LocalProgression
{
    private static readonly JObject Config = new()
    {
        ["Configs"] = new JArray(
            new JObject
            {
                ["RundownID"] = Rundown.R_Daily,
                ["EnableNoBoosterUsedProgressionForRundown"] = true,
                ["AlwaysShowIcon"] = true,
                ["Expeditions"] = new JArray()
            },
            new JObject
            {
                ["RundownID"] = Rundown.R_Weekly,
                ["EnableNoBoosterUsedProgressionForRundown"] = true,
                ["AlwaysShowIcon"] = true,
                ["Expeditions"] = new JArray()
            },
            new JObject
            {
                ["RundownID"] = Rundown.R_Monthly,
                ["EnableNoBoosterUsedProgressionForRundown"] = true,
                ["AlwaysShowIcon"] = true,
                ["Expeditions"] = new JArray()
            },
            new JObject
            {
                ["RundownID"] = Rundown.R_Seasonal,
                ["EnableNoBoosterUsedProgressionForRundown"] = true,
                ["AlwaysShowIcon"] = true,
                ["Expeditions"] = new JArray()
            })
    };

    public static void WriteConfig()
    {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;

        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}", "Custom", "LocalProgressionConfig");
        var path = Path.Combine(dir, "ProgressionConfig.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);
        writer.Formatting = Formatting.Indented;

        Config.WriteTo(writer);

        writer.Flush();
    }
}
