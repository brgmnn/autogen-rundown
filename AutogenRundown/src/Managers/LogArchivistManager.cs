using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using AutogenRundown.Serialization;
using BepInEx;
using CellMenu;
using Dissonance;
using GameData;
using LevelGeneration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace AutogenRundown.Managers;

public static class LogArchivistManager
{
    public static List<LevelLogArchives> archives { get; set; } = new();

    public static Dictionary<uint, LevelLogArchives> archivesByLevel { get; set; } = new();

    public static Dictionary<uint, List<ReadLogRecord>> readRecordsByLevel { get; set; } = new();

    private static RundownLogRecord WeeklyLogRecord { get; set; } = new();

    private static RundownLogRecord MonthlyLogRecord { get; set; } = new();

    private static RundownLogRecord SeasonalLogRecord { get; set; } = new();

    private static Dictionary<uint, CM_ExpeditionIcon_New> icons = new();

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

    public static void RegisterIcon(CM_ExpeditionIcon_New icon)
    {
        icons[icon.DataBlock.LevelLayoutData] = icon;

        UpdateIcon(icon.DataBlock.LevelLayoutData);
    }

    public static void UpdateIcon(uint mainId)
    {
        if (!icons.ContainsKey(mainId))
            return;

        var icon = icons[mainId];

        if (archivesByLevel.TryGetValue(mainId, out var logs))
        {
            if (logs.Logs.Count == 0)
            {
                icon.m_artifactHeatText.SetText("<color=#777777>No logs</color>");
                return;
            }

            var completed = 0;

            if (readRecordsByLevel.TryGetValue(mainId, out var readLogs))
                completed = Math.Min(readLogs.Count, logs.Logs.Count);

            icon.m_artifactHeatText.SetText($"Logs: <color=orange>{completed}</color>/" +
                                                  $"<color=orange>{logs.Logs.Count}</color>");
        }
        else
        {
            // If we can't find the mainId for this level, it means it's part of a rundown with
            // no logs

            // "Hides" the artifact heat text by moving it off-screen
            icon.m_artifactHeatText.gameObject.transform.localPosition += Vector3.down * 10_000;

            // Shifts up the level completion text
            icon.m_statusText.transform.localPosition += new Vector3(0f, 25.0f, 0f);
        }
    }

    public static (int, int) GetLogsRead(uint mainId)
    {
        if (!archivesByLevel.TryGetValue(mainId, out var logs))
            return (-1, -1);

        var completed = 0;

        if (readRecordsByLevel.TryGetValue(mainId, out var readLogs))
            completed = Math.Min(readLogs.Count, logs.Logs.Count);

        return (completed, logs.Logs.Count);

    }

    /// <summary>
    /// Records a log read attempt. Will save to disk as soon as method is called for a valid log file
    /// </summary>
    /// <param name="rundown"></param>
    /// <param name="mainLayout"></param>
    /// <param name="logName"></param>
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

            UpdateIcon(mainLayout);
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
