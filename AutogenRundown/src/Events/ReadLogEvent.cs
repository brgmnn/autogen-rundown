using System.Runtime.InteropServices;

namespace AutogenRundown.Events;

[StructLayout(LayoutKind.Sequential)]
public struct ReadLogEvent
{
    public uint Rundown { get; set; }

    public uint MainId { get; set; }

    [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 240)]
    public string LogFileName { get; set; }
}
