using System.Runtime.InteropServices;

namespace AutogenRundown.Events;

[StructLayout(LayoutKind.Sequential)]
public struct ReadLogEvent
{
    public PluginRundown Rundown { get; set; }

    public uint MainId { get; set; }

    [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string LogFileName { get; set; }
}
