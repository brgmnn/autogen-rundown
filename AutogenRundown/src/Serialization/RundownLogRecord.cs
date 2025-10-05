namespace AutogenRundown.Serialization;

public class RundownLogRecord
{
    public string Name { get; set; } = "";

    /// <summary>
    /// A map of level main layout ids to the logs that have been read for that level
    /// </summary>
    public Dictionary<uint, List<ReadLogRecord>> ReadLogs { get; set; } = new();

    /// <summary>
    /// A list of level main id layouts which have all been read
    /// </summary>
    public HashSet<uint> ReadAllLogsLevels { get; set; } = new();
}
