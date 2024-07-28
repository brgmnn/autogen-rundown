using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;

namespace AutogenRundown.DataBlocks;

public partial record WardenObjective : DataBlock
{
    public void PreBuild_TimedTerminalSequence(BuildDirector director, Level level)
    {
        TimedTerminalSequence_NumberOfRounds = 3;
        TimedTerminalSequence_NumberOfTerminals = 3;
    }

    public void Build_TimedTerminalSequence(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        // MainObjective = $"";
        // FindLocationInfo = "";

        TimedTerminalSequence_TimePerRound = 200;
        TimedTerminalSequence_TimeForConfirmation = 10.0;

        // Initialize the lists to the correct size. Hopefully we can refactor this
        TimedTerminalSequence_EventsOnSequenceStart = Enumerable.Repeat(
            new List<WardenObjectiveEvent>(),
            TimedTerminalSequence_NumberOfRounds).ToList();
        TimedTerminalSequence_EventsOnSequenceDone = Enumerable.Repeat(
            new List<WardenObjectiveEvent>(),
            TimedTerminalSequence_NumberOfRounds).ToList();
    }
}
