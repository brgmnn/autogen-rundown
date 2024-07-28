using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout : DataBlock
{
    public void BuildLayout_TimedTerminalSequence(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode? start)
    {
    }
}
