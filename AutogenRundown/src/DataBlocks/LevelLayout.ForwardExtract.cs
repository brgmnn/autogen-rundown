using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;
using AutogenRundown.Utils;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// Builds a forward extract zone
    /// </summary>
    public void BuildLayout_ForwardExtract(WardenObjective objective, double chance = 0.5)
    {
        // Skip objectives that already include forward extracts
        if (director.Objective is WardenObjectiveType.ClearPath or WardenObjectiveType.Survival)
            return;

        // // Random chance for us to skip doing this all together
        // if (Generator.Flip(1.0 - chance))
        //     return;

        var start = level.ForwardExtractStartCandidates.Any()
            ? Generator.Select(level.ForwardExtractStartCandidates)
            : planner.GetLeafZones(Bulkhead.Main).PickRandom();

        var first = new ZoneNode();
        var exit = new ZoneNode();
        var firstZone = new Zone(level, Bulkhead.Main);
        var exitZone = new Zone(level, Bulkhead.Main);

        switch (level.Tier, level.Settings.Bulkheads)
        {
            case ("A", Bulkhead.Main):
            {
                var nodes = AddBranch(start, 2, "forward_extraction");

                first = nodes.First();
                firstZone = planner.GetZone(first)!;

                exit = nodes.Last();
                exitZone = planner.GetZone(exit)!;

                break;
            }

            case ("B", Bulkhead.Main):
            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Direct path
                    (0.20, () =>
                    {
                        var nodes = AddBranch(start, Generator.Between(1, 2), "forward_extraction");

                        first = nodes.First();
                        firstZone = planner.GetZone(first)!;

                        exit = nodes.Last();
                        exitZone = planner.GetZone(exit)!;
                    }),

                    // Terminal door unlock
                    (0.40, () =>
                    {
                        (first, firstZone) = AddZone(start, new ZoneNode { Branch = "forward_extract" });

                        // Increase size a bit and ensure we have 3 terminals total after adding challenge
                        firstZone.Coverage = CoverageMinMax.Medium_56;
                        firstZone.TerminalPlacements.Add(new TerminalPlacement());

                        (exit, exitZone) = BuildChallenge_LockedTerminalDoor(start, sideZones: 0);
                    }),

                    // Key door unlock
                    (0.40, () =>
                    {
                        (first, firstZone) = AddZone(start, new ZoneNode { Branch = "forward_extract" });

                        // Increase size a bit and ensure we have 3 terminals total after adding challenge
                        firstZone.Coverage = CoverageMinMax.Medium_56;

                        (exit, exitZone) = BuildChallenge_KeycardInZone(start);
                    }),
                });

                break;
            }

            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Direct path
                    (0.15, () =>
                    {
                        var nodes = AddBranch(start, Generator.Between(1, 2), "forward_extraction");

                        first = nodes.First();
                        firstZone = planner.GetZone(first)!;

                        exit = nodes.Last();
                        exitZone = planner.GetZone(exit)!;
                    }),

                    // Terminal door unlock
                    (0.30, () =>
                    {
                        (first, firstZone) = AddZone(start, new ZoneNode { Branch = "forward_extract" });

                        // Increase size a bit and ensure we have 3 terminals total after adding challenge
                        firstZone.Coverage = CoverageMinMax.Medium_56;
                        firstZone.TerminalPlacements.Add(new TerminalPlacement());

                        (exit, exitZone) = BuildChallenge_LockedTerminalDoor(start, sideZones: 0);
                    }),

                    // Key door unlock
                    (0.25, () =>
                    {
                        (first, firstZone) = AddZone(start, new ZoneNode { Branch = "forward_extract" });

                        // Increase size a bit and ensure we have 3 terminals total after adding challenge
                        firstZone.Coverage = CoverageMinMax.Medium_56;

                        (exit, exitZone) = BuildChallenge_KeycardInZone(start);
                    }),

                    // Error alarm, no turn off
                    // Note: this will be on top of the regular extract alarm!
                    (0.30, () =>
                    {
                        (first, firstZone) = AddZone(start, new ZoneNode { Branch = "forward_extract" });

                        AddErrorAlarm(first, null, WaveSettings.Error_Hard, WavePopulation.Baseline);

                        // Error zone is large to be nasty
                        firstZone.Coverage = CoverageMinMax.Large_80;

                        (exit, exitZone) = AddZone(first, new ZoneNode { Branch = "forward_extract" });
                    }),
                });
                break;
            }

            case ("D", Bulkhead.Main | Bulkhead.Extreme):
            case ("D", Bulkhead.Main | Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone
                    (0.15, () =>
                    {
                        (exit, exitZone) = (first, firstZone) = AddZone(start, new ZoneNode { Branch = "forward_extract" });
                    }),

                    // Terminal door unlock
                    (0.30, () =>
                    {
                        (first, firstZone) = AddZone(start, new ZoneNode { Branch = "forward_extract" });

                        // Increase size a bit and ensure we have 3 terminals total after adding challenge
                        firstZone.Coverage = CoverageMinMax.Medium_56;
                        firstZone.TerminalPlacements.Add(new TerminalPlacement());

                        (exit, exitZone) = BuildChallenge_LockedTerminalDoor(start, sideZones: 0);
                    }),

                    // Key door unlock
                    (0.25, () =>
                    {
                        (first, firstZone) = AddZone(start, new ZoneNode { Branch = "forward_extract" });

                        // Increase size a bit and ensure we have 3 terminals total after adding challenge
                        firstZone.Coverage = CoverageMinMax.Medium_56;

                        (exit, exitZone) = BuildChallenge_KeycardInZone(start);
                    })
                });

                break;
            }

            case ("E", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Terminal door with password in side zone
                    (0.20, () =>
                    {
                        (first, firstZone) = AddZone_Forward(start, new ZoneNode { Branch = "forward_extract" });

                        // Increase size a bit and ensure we have 3 terminals total after adding challenge
                        firstZone.Coverage = CoverageMinMax.Medium_56;
                        firstZone.TerminalPlacements.Add(new TerminalPlacement());

                        (exit, exitZone) = BuildChallenge_LockedTerminalPasswordInSide(start);
                    }),

                    // Key door unlock with key in zone
                    (0.10, () =>
                    {
                        (first, firstZone) = AddZone_Forward(start, new ZoneNode { Branch = "forward_extract" });

                        // Increase size a bit and ensure we have 3 terminals total after adding challenge
                        firstZone.Coverage = CoverageMinMax.Medium_56;

                        (exit, exitZone) = BuildChallenge_KeycardInZone(start);
                    }),

                    // Key in side
                    (0.15, () =>
                    {
                        (first, firstZone) = AddZone_Forward(start, new ZoneNode { Branch = "forward_extract" });

                        // Increase size a bit and ensure we have 3 terminals total after adding challenge
                        firstZone.Coverage = CoverageMinMax.Medium_56;

                        (exit, exitZone) = BuildChallenge_KeycardInSide(start, sideKeycardZones: 1);
                    }),

                    // Cell in side
                    (0.15, () =>
                    {
                        (first, firstZone) = AddZone_Forward(start, new ZoneNode { Branch = "forward_extract" });

                        // Increase size a bit and ensure we have 3 terminals total after adding challenge
                        firstZone.Coverage = CoverageMinMax.Medium_56;

                        (exit, exitZone) = BuildChallenge_GeneratorCellInSide(start, sideCellZones: 1);
                    }),

                    // Error alarm, no turn off
                    // Note: this will be on top of the regular extract alarm!
                    (0.25, () =>
                    {
                        (first, firstZone) = AddZone_Forward(start, new ZoneNode { Branch = "forward_extract" });

                        AddErrorAlarm(first, null, WaveSettings.Error_Hard, WavePopulation.Baseline);

                        // Error zone is large to be nasty
                        firstZone.Coverage = CoverageMinMax.Large_80;

                        (exit, exitZone) = AddZone_Forward(first, new ZoneNode { Branch = "forward_extract" });
                    }),

                    // Impossible Boss error alarm, sprint to extract!
                    // No enemies spawn except in extract zone, no blood doors along the way, only team scans
                    // Note: this will be on top of the regular extract alarm!
                    (0.15, () =>
                    {
                        (first, firstZone) = AddZone_Forward(
                            start,
                            new ZoneNode
                            {
                                Branch = "forward_extract",
                                Tags = new Tags("no_blood_door", "no_enemies")
                            });

                        var population = Generator.Select(new List<(double, WavePopulation)>
                        {
                            (0.6, WavePopulation.SingleEnemy_Tank),
                            (0.4, WavePopulation.SingleEnemy_Mother),
                        });

                        AddApexErrorAlarm(
                            first,
                            null,
                            WaveSettings.Error_Boss_Impossible,
                            population,
                            spawnDelay: Generator.Between(30, 45));

                        firstZone.Coverage = CoverageMinMax.Large_80;

                        var (middle, middleZone) = AddZone_Forward(
                            first,
                            new ZoneNode
                            {
                                Branch = "forward_extract",
                                Tags = new Tags("no_blood_door", "no_enemies")
                            });

                        middleZone.Coverage = CoverageMinMax.Large_80;
                        middleZone.Alarm = ChainedPuzzle.TeamScan;

                        (exit, exitZone) = AddZone_Forward(
                            first,
                            new ZoneNode
                            {
                                Branch = "forward_extract",
                                Tags = new Tags("no_blood_door")
                            });

                        exitZone.Alarm = ChainedPuzzle.TeamScan;
                    }),
                });
                break;
            }

            case ("E", Bulkhead.Main | Bulkhead.Extreme):
            case ("E", Bulkhead.Main | Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Terminal door with password in side zone
                    (0.30, () =>
                    {
                        (first, firstZone) = AddZone_Forward(start, new ZoneNode { Branch = "forward_extract" });

                        // Giga zone size
                        firstZone.Coverage = CoverageMinMax.Large_120;

                        (exit, exitZone) = BuildChallenge_LockedTerminalDoor(start);
                    }),

                    // Key door unlock with key in zone
                    (0.25, () =>
                    {
                        (first, firstZone) = AddZone_Forward(start, new ZoneNode { Branch = "forward_extract" });

                        (exit, exitZone) = BuildChallenge_KeycardInZone(start);
                    }),

                    // Cell in zone
                    (0.25, () =>
                    {
                        (first, firstZone) = AddZone_Forward(start, new ZoneNode { Branch = "forward_extract" });

                        (exit, exitZone) = BuildChallenge_GeneratorCellInZone(start);
                    }),

                    // Error alarm, no turn off
                    // Note: this will be on top of the regular extract alarm!
                    (0.12, () =>
                    {
                        (first, firstZone) = AddZone_Forward(start, new ZoneNode { Branch = "forward_extract" });

                        AddErrorAlarm(first, null, WaveSettings.Error_Hard, WavePopulation.Baseline);

                        // Error zone is large to be nasty
                        firstZone.Coverage = CoverageMinMax.Large_80;

                        (exit, exitZone) = AddZone_Forward(first, new ZoneNode { Branch = "forward_extract" });
                    }),

                    // Impossible Boss error alarm, sprint to extract! Lite version of the Main version
                    // No enemies, no blood doors along the way, only team scans
                    // Note: this will be on top of the regular extract alarm!
                    (0.08, () =>
                    {
                        (first, firstZone) = AddZone_Forward(
                            start,
                            new ZoneNode
                            {
                                Branch = "forward_extract",
                                Tags = new Tags("no_blood_door", "no_enemies")
                            });

                        var population = Generator.Select(new List<(double, WavePopulation)>
                        {
                            (0.6, WavePopulation.SingleEnemy_Tank),
                            (0.4, WavePopulation.SingleEnemy_Mother),
                        });

                        AddApexErrorAlarm(
                            first,
                            null,
                            WaveSettings.Error_Boss_Impossible,
                            population,
                            spawnDelay: Generator.Between(30, 45));

                        firstZone.Coverage = CoverageMinMax.Large_80;

                        (exit, exitZone) = AddZone_Forward(
                            first,
                            new ZoneNode
                            {
                                Branch = "forward_extract",
                                Tags = new Tags("no_blood_door", "no_enemies")
                            });

                        exitZone.Alarm = ChainedPuzzle.TeamScan;
                    }),
                });
                break;
            }

            // For A/B tier with side objectives, keep it simple
            case ("A", _):
            case ("B", _):
            case ("C", _):
            // For PE missions we always want to just add a single zone with the extract
            case (_, Bulkhead.PrisonerEfficiency):
            case (_, _):
            {
                (exit, exitZone) = (first, firstZone) = AddZone(start, new ZoneNode { Branch = "forward_extract" });

                break;
            }
        }

        // Locking the way to forward extract on main completion
        var lockedExtractionPath = (level.Tier) switch
        {
            "A" => false,
            "B" => Generator.Flip(0.2),
            "C" => Generator.Flip(0.6),
            "D" => true,
            "E" => true
        };

        if (lockedExtractionPath)
        {
            firstZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;
            objective.EventsOnGotoWin.AddUnlockDoor(first.Bulkhead, first.ZoneNumber);
        }

        level.ExtractionZone = exit;
        exitZone.GenExitGeomorph(level.Complex);
    }
}
