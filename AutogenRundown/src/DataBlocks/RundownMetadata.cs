using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public record RundownMetadataEntry
{
    public uint PersistentId { get; set; }
    public string Title { get; set; } = "";
}

public record RundownMetadata
{
    public List<RundownMetadataEntry> Rundowns { get; set; } = new();
    public int WeekNumber { get; set; } = -1;

    public string InputDailySeed { get; set; } = "";
    public string InputWeeklySeed { get; set; } = "";
    public string InputMonthlySeed { get; set; } = "";
    public string SeasonalSeason { get; set; } = "";
    public int SeasonalYear { get; set; } = 1;

    private static string MetadataPath =>
        Path.Combine(Plugin.GameDataPath, "autogen_metadata.json");

    public void Save()
    {
        Directory.CreateDirectory(Plugin.GameDataPath);
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(MetadataPath, json);
    }

    public static RundownMetadata? Load()
    {
        if (!File.Exists(MetadataPath))
            return null;

        try
        {
            var json = File.ReadAllText(MetadataPath);
            return JsonConvert.DeserializeObject<RundownMetadata>(json);
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogWarning($"Failed to load rundown metadata: {ex.Message}");
            return null;
        }
    }
}
