namespace AutogenRundown.Events;

public struct ReadLogEvent
{
    public PluginRundown Rundown { get; set; }

    public uint MainId { get; set; }

    public string LogFileName { get; set; }
}
