using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Custom.AutogenRundown;
using AutogenRundown.Events;
using AutogenRundown.Serialization;
using CellMenu;
using GameData;
using GTFO.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vector3 = UnityEngine.Vector3;

namespace AutogenRundown.Managers;

public static class LogArchivistManager
{
    private static List<LevelLogArchives> archives { get; set; } = new();

    private static Dictionary<PluginRundown, HashSet<uint>> rundownMainIds { get; set; } = new();

    private static Dictionary<uint, LevelLogArchives> archivesByLevel { get; set; } = new();

    private static Dictionary<uint, List<ReadLogRecord>> readRecordsByLevel { get; set; } = new();

    private static Dictionary<uint, CM_ExpeditionIcon_New> icons = new();

    private static RundownLogRecord WeeklyLogRecord { get; set; } = new();

    private static RundownLogRecord MonthlyLogRecord { get; set; } = new();

    private static RundownLogRecord SeasonalLogRecord { get; set; } = new();

    private const string eventName = "autogen_archivist_read_log";

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
            {
                AddRundownMainIds(PluginRundown.Weekly, rundown);
                WeeklyLogRecord = Load(rundown.name) ?? new RundownLogRecord { Name = rundown.name };
            }

            if (rundown.persistentID == (uint)PluginRundown.Monthly)
            {
                AddRundownMainIds(PluginRundown.Monthly, rundown);
                MonthlyLogRecord = Load(rundown.name) ?? new RundownLogRecord { Name = rundown.name };
            }

            if (rundown.persistentID == (uint)PluginRundown.Seasonal)
            {
                AddRundownMainIds(PluginRundown.Seasonal, rundown);
                SeasonalLogRecord = Load(rundown.name) ?? new RundownLogRecord { Name = rundown.name };
            }
        }

        foreach (var layoutId in WeeklyLogRecord.ReadLogs.Keys)
            readRecordsByLevel[layoutId] = WeeklyLogRecord.ReadLogs[layoutId];

        foreach (var layoutId in MonthlyLogRecord.ReadLogs.Keys)
            readRecordsByLevel[layoutId] = MonthlyLogRecord.ReadLogs[layoutId];

        foreach (var layoutId in SeasonalLogRecord.ReadLogs.Keys)
            readRecordsByLevel[layoutId] = SeasonalLogRecord.ReadLogs[layoutId];

        // Ensure we update the icons when finishing a level
        LevelAPI.OnLevelCleanup += OnLevelCleanup;

        NetworkAPI.RegisterEvent<ReadLogEvent>(eventName, OnReadLog);
    }

    private static void AddRundownMainIds(PluginRundown rundown, RundownDataBlock data)
    {
        rundownMainIds[rundown] = new HashSet<uint>();

        foreach (var level in data.TierA)
            rundownMainIds[rundown].Add(level.LevelLayoutData);
        foreach (var level in data.TierB)
            rundownMainIds[rundown].Add(level.LevelLayoutData);
        foreach (var level in data.TierC)
            rundownMainIds[rundown].Add(level.LevelLayoutData);
        foreach (var level in data.TierD)
            rundownMainIds[rundown].Add(level.LevelLayoutData);
        foreach (var level in data.TierE)
            rundownMainIds[rundown].Add(level.LevelLayoutData);
    }

    /// <summary>
    /// Updates the icon for the level on cleanup
    /// </summary>
    private static void OnLevelCleanup()
    {
        try
        {
            var mainId = RundownManager.ActiveExpedition.LevelLayoutData;
            UpdateIcon(mainId);
        }
        catch (NullReferenceException err)
        {
            Plugin.Logger.LogInfo($"Null reference exception: {err.Message}");
        }
    }

    public static void RegisterIcon(CM_ExpeditionIcon_New icon)
    {
        icons[icon.DataBlock.LevelLayoutData] = icon;

        UpdateIcon(icon.DataBlock.LevelLayoutData);
    }

    private static void UpdateIcon(uint mainId)
    {
        if (!icons.TryGetValue(mainId, out var icon))
            return;

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

            var completedString = $"{completed}";

            if (completed == 0)
                completedString = $"<color=red>{completedString}</color>";
            else if (completed < logs.Logs.Count)
                completedString = $"<color=orange>{completedString}</color>";

            if (WeeklyLogRecord.ReadAllLogsLevels.Contains(mainId) ||
                MonthlyLogRecord.ReadAllLogsLevels.Contains(mainId) ||
                SeasonalLogRecord.ReadAllLogsLevels.Contains(mainId))
                completedString = $"{logs.Logs.Count}";

            icon.m_artifactHeatText.SetText($"Logs: {completedString}/{logs.Logs.Count}");
        }
        else
        {
            // If we can't find the mainId for this level, it means it's part of a rundown with
            // no logs

            // "Hides" the artifact heat text by moving it off-screen
            icon.m_artifactHeatText.gameObject.transform.localPosition = Vector3.down * 10_000;

            // Shifts up the level completion text
            // Base game position is (-156.4, -67.8, -1.4)
            // += new Vector3(0f, 25.0f, 0f) is perfect
            icon.m_statusText.transform.localPosition = new Vector3 { x = -156.4f, y = -42.8f, z = -1.4f };
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
    /// Gets the total number of read logs for a rundown
    /// </summary>
    /// <param name="rundown"></param>
    /// <returns></returns>
    public static (int, int) GetLogsRead(PluginRundown rundown)
    {
        RundownLogRecord record;

        switch (rundown)
        {
            case PluginRundown.Weekly:
                record = WeeklyLogRecord;
                break;

            case PluginRundown.Monthly:
                record = MonthlyLogRecord;
                break;

            case PluginRundown.Seasonal:
                record = SeasonalLogRecord;
                break;

            case PluginRundown.None:
            case PluginRundown.Daily:
            default:
                return (0, 0);
        }

        var total = 0;
        var read = 0;

        foreach (var archive in archives)
        {
            var id = archive.MainLevelLayout;

            if (!rundownMainIds.TryGetValue(rundown, out var ids))
                continue;

            if (!ids.Contains(id))
                continue;

            total += archive.Logs.Count;
        }

        foreach (var mainId in record.ReadAllLogsLevels)
            if (archivesByLevel.TryGetValue(mainId, out var logs))
                read += logs.Logs.Count;

        foreach (var data in record.ReadLogs)
        {
            var mainId = data.Key;
            var logs = data.Value;

            if (record.ReadAllLogsLevels.Contains(mainId))
                continue;

            read += logs.Count;
        }

        return (read, total);
    }

    /// <summary>
    /// Records a log read attempt. Will save to disk as soon as method is called for a valid log file
    /// </summary>
    /// <param name="rundown"></param>
    /// <param name="mainLayout"></param>
    /// <param name="logName"></param>
    public static void RecordRead(string rundownKey, uint mainLayout, string? logName)
    {
        if (logName == null)
            return;

        logName = logName.ToUpper();
        PluginRundown rundown;

        switch (rundownKey)
        {
            // Weekly
            case "Local_2":
                rundown = PluginRundown.Weekly;
                break;

            // Monthly
            case "Local_3":
                rundown = PluginRundown.Monthly;
                break;

            // Seasonal
            case "Local_4":
                rundown = PluginRundown.Seasonal;
                break;

            // Daily -> "Local_1"
            default:
                return;
        }

        if (!archivesByLevel.TryGetValue(mainLayout, out var levelLogs))
            return;

        if (!levelLogs.Logs.Exists(log => log.FileName.ToUpper().Equals(logName.ToUpper())))
            return;

        var data = new ReadLogEvent
        {
            Rundown = rundown,
            MainId = mainLayout,
            LogFileName = logName.ToUpper() ?? ""
        };

        NetworkAPI.InvokeEvent(eventName, data);

        OnReadLog(0u, data);
    }

    #region Networking

    /// <summary>
    /// Actually updates the logs. This is received from a network requests so all players
    /// get the log
    /// </summary>
    /// <param name="snetPlayer"></param>
    /// <param name="data"></param>
    private static void OnReadLog(ulong snetPlayer, ReadLogEvent data)
    {
        RundownLogRecord record;
        var logName = data.LogFileName.ToUpper();

        switch (data.Rundown)
        {
            case PluginRundown.Weekly:
                record = WeeklyLogRecord;
                break;

            case PluginRundown.Monthly:
                record = MonthlyLogRecord;
                break;

            case PluginRundown.Seasonal:
                record = SeasonalLogRecord;
                break;

            case PluginRundown.None:
            case PluginRundown.Daily:
            default:
                return;
        }

        if (!archivesByLevel.TryGetValue(data.MainId, out var levelLogs))
            return;

        if (!levelLogs.Logs.Exists(log => log.FileName.ToUpper().Equals(logName)))
            return;

        if (!record.ReadLogs.ContainsKey(data.MainId))
            record.ReadLogs.Add(data.MainId, new List<ReadLogRecord>());

        var logs = record.ReadLogs[data.MainId];

        logs.Add(new ReadLogRecord { FileName = logName });

        // Remove any duplicates
        record.ReadLogs[data.MainId] = logs.Distinct().ToList();

        Save(record.Name, record);
        UpdateIcon(data.MainId);

        Plugin.Logger.LogDebug($"Recorded log read: {logName}");
    }

    #endregion

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
        // Ensure we have all the "ReadAllLogsLevels" written
        foreach (var (layoutId, record) in data.ReadLogs)
        {
            if (!archivesByLevel.TryGetValue(layoutId, out var levelLogs))
                continue;

            if (record.Count >= levelLogs.Logs.Count)
                data.ReadAllLogsLevels.Add(layoutId);
        }

        // --- Actually save the data ---
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
