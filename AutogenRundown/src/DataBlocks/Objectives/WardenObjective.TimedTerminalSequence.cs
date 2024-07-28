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

        //TimedTerminalSequence_NumberOfRounds = 3;
        //TimedTerminalSequence_NumberOfTerminals = 3;
        TimedTerminalSequence_TimePerRound = 200;
        TimedTerminalSequence_TimeForConfirmation = 10.0;

        // Initialize the lists to the correct size. Hopefully we can refactor this
        TimedTerminalSequence_EventsOnSequenceStart = Enumerable.Repeat(
            new List<WardenObjectiveEvent>(),
            TimedTerminalSequence_NumberOfRounds).ToList();
        TimedTerminalSequence_EventsOnSequenceDone = Enumerable.Repeat(
            new List<WardenObjectiveEvent>(),
            TimedTerminalSequence_NumberOfRounds).ToList();

        var hub = level.Planner.GetZones(director.Bulkhead, "terminal_hub").First();
        var zone = level.Planner.GetZone(hub)!;

        zone.TerminalPlacements = new List<TerminalPlacement>();

        for (var i = 0; i < TimedTerminalSequence_NumberOfTerminals + 1; i++)
            zone.TerminalPlacements.Add(new TerminalPlacement());

        // Place terminals
        for (var i = 0; i < TimedTerminalSequence_NumberOfTerminals; i++)
            dataLayer.ObjectiveData.ZonePlacementDatas.Add(
                new List<ZonePlacementData>
                {
                    new ZonePlacementData()
                    {
                        LocalIndex = hub.ZoneNumber,
                        Weights = ZonePlacementWeights.NotAtStart
                    }
                });
    }
}
