using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

public partial record WardenObjective : DataBlock
{
    public void Build_TimedTerminalSequence(BuildDirector director, Level level)
    {
        // MainObjective = $"";
        // FindLocationInfo = "";

        TimedTerminalSequence_NumberOfRounds = 3;
        TimedTerminalSequence_NumberOfTerminals = 3;
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
