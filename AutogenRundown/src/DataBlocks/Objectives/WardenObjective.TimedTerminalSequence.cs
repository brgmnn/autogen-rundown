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

        MainObjective = $"Find [ITEM_SERIAL] and initiate timed sequence protocol.";
        // FindLocationInfo = "";

        TimedTerminalSequence_TimeForConfirmation = 10.0;

        // Initialize the lists to the correct size. Hopefully we can refactor this
        TimedTerminalSequence_EventsOnSequenceStart = Enumerable.Repeat(
            new List<WardenObjectiveEvent>(),
            TimedTerminalSequence_NumberOfRounds).ToList();
        TimedTerminalSequence_EventsOnSequenceDone = Enumerable.Repeat(
            new List<WardenObjectiveEvent>(),
            TimedTerminalSequence_NumberOfRounds).ToList();

        TimedTerminalSequence_TimePerRound = 60.0;

        // Calculate round time based on max time to clear a zone
        for (int i = 0; i < TimedTerminalSequence_NumberOfTerminals; i++)
        {
            var nodes = level.Planner.GetZones(director.Bulkhead, $"timed_terminal_{i}");
            var total = 0.0;

            foreach (var node in nodes)
            {
                var zone = level.Planner.GetZone(node)!;
                total += zone.GetClearTimeEstimate();
            }

            // We will just set the time per round to the max time we would need to clear the
            // hardest zone
            TimedTerminalSequence_TimePerRound = Math.Max(TimedTerminalSequence_TimePerRound, total);
        }
    }
}
