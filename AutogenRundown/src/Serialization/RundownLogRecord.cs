namespace AutogenRundown.Serialization;

public class RundownLogRecord
{
    public string Name { get; set; } = "";

    public Dictionary<uint, List<ReadLogRecord>> ReadLogs { get; set; } = new();
}
