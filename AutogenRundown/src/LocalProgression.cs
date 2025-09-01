using AutogenRundown.DataBlocks;
using BepInEx;
using Newtonsoft.Json;

namespace AutogenRundown;

public class LocalProgression
{
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

        using StreamWriter stream = new StreamWriter(path);
        // using JsonWriter writer = new JsonTextWriter(stream);

        stream.Write($@"{{
    ""Configs"": [
        {{
            ""RundownID"": {Rundown.R_Daily},
            ""EnableNoBoosterUsedProgressionForRundown"": true,
            ""AlwaysShowIcon"": true,
            ""Expeditions"": []
        }},
        {{
            ""RundownID"": {Rundown.R_Weekly},
            ""EnableNoBoosterUsedProgressionForRundown"": true,
            ""AlwaysShowIcon"": true,
            ""Expeditions"": []
        }},
        {{
            ""RundownID"": {Rundown.R_Monthly},
            ""EnableNoBoosterUsedProgressionForRundown"": true,
            ""AlwaysShowIcon"": true,
            ""Expeditions"": []
        }},
        {{
            ""RundownID"": {Rundown.R_Seasonal},
            ""EnableNoBoosterUsedProgressionForRundown"": true,
            ""AlwaysShowIcon"": true,
            ""Expeditions"": []
        }},
    ]
}}");
        stream.Flush();
    }
}
