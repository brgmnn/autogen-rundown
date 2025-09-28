using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using AutogenRundown.Serialization;
using BepInEx;
using Dissonance;
using GameData;
using LevelGeneration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.Managers;

public static class LogArchivistManager
{
    public static List<LevelLogArchives> archives { get; set; } = new();

    public static Dictionary<uint, LevelLogArchives> archivesByLevel { get; set; } = new();

    public static Dictionary<uint, List<ReadLogRecord>> readRecordsByLevel { get; set; } = new();

    public static RundownLogRecord WeeklyLogRecord { get; set; } = new();

    public static RundownLogRecord MonthlyLogRecord { get; set; } = new();

    public static RundownLogRecord SeasonalLogRecord { get; set; } = new();

    private static readonly string dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GTFO-Modding",
        "AutogenRundown");

    public static void Setup()
    {
        archives = LevelLogArchives.LoadAll();

        foreach (var archive in archives)
        {
            archivesByLevel.Add(archive.MainLevelLayout, archive);
            readRecordsByLevel.Add(archive.MainLevelLayout, new List<ReadLogRecord>());
        }

        var blocks = RundownDataBlock.GetAllBlocks();

        foreach (var rundown in blocks)
        {
            if (rundown.persistentID == (uint)PluginRundown.Weekly)
                WeeklyLogRecord = Load(rundown.name) ?? new RundownLogRecord { Name = rundown.name };
            if (rundown.persistentID == (uint)PluginRundown.Monthly)
                MonthlyLogRecord = Load(rundown.name) ?? new RundownLogRecord { Name = rundown.name };
            if (rundown.persistentID == (uint)PluginRundown.Seasonal)
                SeasonalLogRecord = Load(rundown.name) ?? new RundownLogRecord { Name = rundown.name };
        }

        foreach (var layoutId in WeeklyLogRecord.ReadLogs.Keys)
            readRecordsByLevel[layoutId] = WeeklyLogRecord.ReadLogs[layoutId];

        foreach (var layoutId in MonthlyLogRecord.ReadLogs.Keys)
            readRecordsByLevel[layoutId] = MonthlyLogRecord.ReadLogs[layoutId];

        foreach (var layoutId in SeasonalLogRecord.ReadLogs.Keys)
            readRecordsByLevel[layoutId] = SeasonalLogRecord.ReadLogs[layoutId];

        GTFO.API.LevelAPI.OnLevelCleanup += () =>
        {
            Plugin.Logger.LogDebug($"Got the level cleanup event");
        };
    }

    public static void RecordRead(string rundown, uint mainLayout, string logName)
    {
        RundownLogRecord record;
        logName = logName.ToUpper();

        switch (rundown)
        {
            // Weekly
            case "Local_2":
                record = WeeklyLogRecord;
                break;

            // Monthly
            case "Local_3":
                record = MonthlyLogRecord;
                break;

            // Seasonal
            case "Local_4":
                record = SeasonalLogRecord;
                break;

            // Daily -> "Local_1"
            default:
                return;
        }

        if (archivesByLevel.TryGetValue(mainLayout, out var archives))
        {
            if (!archives.Logs.Exists(log => log.FileName.ToUpper().Equals(logName.ToUpper())))
                return;

            if (!record.ReadLogs.ContainsKey(mainLayout))
                record.ReadLogs.Add(mainLayout, new List<ReadLogRecord>());

            var logs = record.ReadLogs[mainLayout];

            logs.Add(new ReadLogRecord
            {
                FileName = logName.ToUpper()
            });

            Save(record.Name, record);

            Plugin.Logger.LogDebug($"Recorded log read: {logName}");
        }
    }

    #region Filesystem

    private static RundownLogRecord? Load(string rundownName)
    {
        try
        {
            var path = RundownFile(rundownName);

            var data = JObject.Parse(File.ReadAllText(path));
            var rundownLogRecord = data.ToObject<RundownLogRecord>();

            Plugin.Logger.LogDebug($"Copied datablock -> {path}");

            if (rundownLogRecord is not null)
                return rundownLogRecord;
        }
        catch (Exception error)
        {
            Plugin.Logger.LogInfo($"Unable to load LevelLogArchives files: {error.Message}");
        }

        return null;
    }

    private static void Save(string rundownName, RundownLogRecord data)
    {
        var serializer = new JsonSerializer { Formatting = Formatting.Indented };

        var path = RundownFile(rundownName);

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using var stream = new StreamWriter(path);
        using var writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, data);
        stream.Flush();
    }

    private static string RundownFile(string rundownName)
    {
        var invalidChars = Path.GetInvalidPathChars();

        var filename = invalidChars.Aggregate(rundownName, (current, c) => current.Replace(c, '_'));

        return Path.Combine(dir, $"{filename}.json");
    }

    #endregion
}
