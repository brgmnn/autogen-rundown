using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks;

public partial record WardenObjective : DataBlock
{
    public void PreBuild_TimedTerminalSequence(BuildDirector director, Level level)
    {
        // TODO: change these?
        TimedTerminalSequence_NumberOfRounds = 3;
        TimedTerminalSequence_NumberOfTerminals = 3;
    }

    /// <summary>
    /// Aparently you can set the terminal it picks using the WorldEventFilter?
    ///
    /// If true, it would mean we could control which terminals are used.
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void Build_TimedTerminalSequence(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = $"Find [ITEM_SERIAL] and initiate timed sequence protocol.";
        FindLocationInfo = "Gather information about the location of [ITEM_SERIAL]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";

        TimedTerminalSequence_TimeForConfirmation = 10.0;
        //TimedTerminalSequence_TimeForConfirmation = 120.0; // DEBUG

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

        ///
        /// Build the event pool of things that can be triggered on sequence events
        ///
        var eventPool = new List<(double, int, ICollection<WardenObjectiveEvent>)>
        {
            // Base error alarm
            (1.0, 1, new List<WardenObjectiveEvent>()
                    .AddMessage($"{Intel.Error}! Alarm Active", 7.0)
                    .AddSpawnWave(new GenericWave
                    {
                        WavePopulation = WavePopulation.Baseline.PersistentId,
                        WaveSettings = WaveSettings.Error_Normal.PersistentId,
                    }, 8.0)),

            // Some padding waves to always have something
            (0.5, 3, new List<WardenObjectiveEvent>()
                    .AddSpawnWave(new GenericWave
                    {
                        WavePopulation = WavePopulation.Baseline.PersistentId,
                        WaveSettings = WaveSettings.Finite_35pts_Hard.PersistentId
                    }, 15.0))
        };

        var roundTwoEvents = new List<(double, int, ICollection<WardenObjectiveEvent>)>
        {
            // Tank single spawn
            (1.0, 2, new List<WardenObjectiveEvent>()
                    .AddMessage($"{Intel.Error} LARGE BIOMASS DETECTED", 12.0)
                    .AddSpawnWave(GenericWave.SingleTank, 11.0)),

            // Single Potato
            (0.5, 2, new List<WardenObjectiveEvent>()
                    .AddMessage($"{Intel.Error} LARGE BIOMASS DETECTED", 12.0)
                    .AddSpawnWave(GenericWave.SingleTankPotato, 18.0)),

            // Single Mother
            (0.1, 3, new List<WardenObjectiveEvent>()
                    .AddMessage($"{Intel.Error} LARGE BIOMASS DETECTED", 12.0)
                    .AddSpawnWave(GenericWave.SingleMother, 22.0))
        };

        var extraEvents = new List<(double, int, ICollection<WardenObjectiveEvent>)>
        {
            // Nothing
            (1.0, 3, new List<WardenObjectiveEvent>()),

            // Lights off
            (0.5, 1, new List<WardenObjectiveEvent>().AddLightsOff(20.0)),

            // Fog flood
            (100.1, 1, new List<WardenObjectiveEvent>().AddFillFog(
                5, 600, $"{Intel.Warning} - VENTILATION SYSTEM ON BACKUP POWER"))
        };

        // Add waves etc. on each round
        for (int round = 0; round < TimedTerminalSequence_NumberOfRounds; round++)
        {
            var onStart = Generator.DrawSelect(eventPool).ToList();
            TimedTerminalSequence_EventsOnSequenceDone.Add(new());
            TimedTerminalSequence_EventsOnSequenceFail.Add(new());

            // Unlock all the terminal zone doors on round start
            if (round == 0)
            {
                for (int t = 0; t < TimedTerminalSequence_NumberOfTerminals; t++)
                {
                    var first = level.Planner.GetZones(director.Bulkhead, $"timed_terminal_{t}").First();
                    EventBuilder.AddUnlockDoor(onStart, first.ZoneNumber);
                }
            }

            // Add some more events for post round 0
            if (round > 0)
                eventPool.AddRange(roundTwoEvents);

            // Potentially add some bonus events
            onStart.AddRange(Generator.DrawSelect(extraEvents));

            TimedTerminalSequence_EventsOnSequenceStart.Add(onStart);
        }
    }
}
