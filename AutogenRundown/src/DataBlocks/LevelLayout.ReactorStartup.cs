using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Objectives.Reactor;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    /// <summary>
    /// Creates a simple reactor startup layout. These startups have no code fetching and
    /// thus are simpler and "easier". Note that we can make them harder with the waves
    /// being spawned.
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="start"></param>
    public void BuildLayout_ReactorStartup_Simple(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode start)
    {
        // Place some zones before the reactor
        var preludeCount = level.Tier switch
        {
            "A" => 0,
            "B" => 1,
            "C" => Generator.Between(1, 2),
            "D" => Generator.Between(1, 3),
            "E" => Generator.Between(2, 4),
            _ => 2
        };

        var last = AddBranch(start, preludeCount).Last();
        BuildReactor(last);
    }

    /// <summary>
    /// Creates a fetch codes reactor layout. These are a lot more complex and require
    /// branches with terminals to fetch codes from.
    /// </summary>
    public void BuildLayout_ReactorStartup_FetchCodes(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode start)
    {
        var reactor = BuildReactor(start);

        var fetchCount = objective.ReactorWaves.Count(wave => wave.IsFetchWave);
        var (branchMin, branchMax) = (director.Tier, fetchCount) switch
        {
            ("A", _) => (1, 1),
            ("B", _) => (1, 2),

            ("C", 4) => (1, 2),
            ("C", _) => (1, 3),

            ("D", >= 5) => (1, 2),
            ("D", >= 3) => (1, 3),
            ("D",  < 3) => (2, 3),

            ("E", >= 6) => (1, 2),
            ("E", >= 3) => (1, 3),
            ("E",  < 3) => (2, 3),

            (_, _) => (1, 1)
        };
        var openChance = director.Tier switch
        {
            "A" => 1.0,
            "B" => 0.5,
            "C" => 0.4,
            "D" => 0.3,
            "E" => 0.2,
            _ => 1.0
        };

        objective.ReactorStartup_FetchWaves = fetchCount;
        var fetchWaves = objective.ReactorWaves.Where(wave => wave.IsFetchWave).ToList();

        for (var b = 0; b < fetchCount; b++)
        {
            var branch = $"reactor_code_{b}";
            var baseNode = b < 3 ? reactor : (ZoneNode)level.Planner.GetLastZone(director.Bulkhead, $"reactor_code_{b - 3}")!;

            var branchNodes = AddBranch(
                baseNode,
                Generator.Between(branchMin, branchMax),
                branch,
                (_, zone) =>
                {
                    zone.AmmoPacks += 5;
                    zone.HealthPacks += 4;
                    zone.ToolPacks += 3;
                });
            var last = branchNodes.Last();

            var lastZone = level.Planner.GetZone(last)!;
            var firstZone = level.Planner.GetZone(branchNodes.First())!;

            // Set terminal zone size to be large. This is to help avoid soft locking the level
            // when placing terminals
            lastZone.Coverage = CoverageMinMax.Large;

            // Set the last zone potentially to a garden tile
            if (fetchCount - b - 1 < 3 && Generator.Flip(0.33))
                lastZone.GenGardenGeomorph(level.Complex);

            // Add some extra terminals for confusion. All at the back.
            lastZone.TerminalPlacements = new List<TerminalPlacement>();
            var terminalCount = Generator.Between(2, 3);

            for (var i = 0; i < terminalCount; i++)
                lastZone.TerminalPlacements.Add(
                    new TerminalPlacement
                    {
                        PlacementWeights = ZonePlacementWeights.NotAtStart
                    });

            // Lock the entrance zone
            firstZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

            // Set this zone to have the code for the right fetch wave
            var wave = fetchWaves[b];
            wave.VerifyInOtherZone = true;
            wave.ZoneForVerification = last.ZoneNumber;

            // Add an event to open/unlock the door when the wave defense is over (OnMid trigger)
            if (Generator.Flip(openChance))
            {
                // TODO: The Zone number appears to not work now (E-level)
                EventBuilder.AddOpenDoor(
                    wave.Events,
                    director.Bulkhead,
                    firstZone.LocalIndex,
                    $"Door to [ZONE_{firstZone.LocalIndex}] opened by startup sequence",
                    WardenObjectiveEventTrigger.OnMid,
                    8.0);

                // Do not add an alarm to this zone as the door will be opened for the players.
                firstZone.Alarm = ChainedPuzzle.SkipZone;
            }
            else
                EventBuilder.AddUnlockDoor(
                    wave.Events,
                    director.Bulkhead,
                    firstZone.LocalIndex,
                    $"Door to [ZONE_{firstZone.LocalIndex}] unlocked by startup sequence",
                    WardenObjectiveEventTrigger.OnMid,
                    8.0);
        }
    }
}
