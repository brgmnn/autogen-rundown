using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Utils;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    public void BuildLayout_ClearPath(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;

        var exit = new ZoneNode();
        var exitZone = new Zone();

        // This whole objective can only be done on main
        switch (level.Tier)
        {
            case "A":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight shot
                    (0.2, () =>
                    {
                        var nodes = AddBranch(start, Generator.Between(2, 4));

                        exit = nodes.Last();
                        exitZone = planner.GetZone(exit)!;
                    }),

                    // Single generator
                    (0.35, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        (exit, exitZone) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                    }),

                    // Single keycard mid
                    (0.25, () =>
                    {
                        var next = AddBranch_Forward(start, 1).Last();
                        (next, _) = BuildChallenge_KeycardInSide(next);
                        var nodes = AddBranch_Forward(next, Generator.Between(1, 2));

                        exit = nodes.Last();
                        exitZone = planner.GetZone(exit)!;
                    }),

                    // Single keycard end
                    (0.2, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        (exit, exitZone) = BuildChallenge_KeycardInSide(nodes.Last());
                    })
                });
                break;
            }

            case "B":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex Alarm end
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(3, 4));
                        exit = nodes.Last();
                        exitZone = planner.GetZone(exit)!;

                        var hub = nodes.ElementAt(nodes.Count - 2);
                        var hubZone = planner.GetZone(hub)!;

                        hubZone.AmmoPacks += 3.0;
                        hubZone.ToolPacks += 2.0;

                        var population = WavePopulation.Baseline;
                        var settings = WaveSettings.Baseline_Normal;

                        AddApexAlarm(exit, population, settings);
                    }),

                    // Straight shot
                    (0.10, () =>
                    {
                        var nodes = AddBranch(start, Generator.Between(2, 4));

                        exit = nodes.Last();
                        exitZone = planner.GetZone(exit)!;
                    }),

                    // Single generator
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        (exit, exitZone) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                    }),

                    // Single keycard mid
                    (0.15, () =>
                    {
                        var next = AddBranch_Forward(start, 1).Last();
                        (next, _) = BuildChallenge_KeycardInSide(next);
                        var nodes = AddBranch_Forward(next, Generator.Between(1, 2));

                        exit = nodes.Last();
                        exitZone = planner.GetZone(exit)!;
                    }),

                    // Single keycard end
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        (exit, exitZone) = BuildChallenge_KeycardInSide(nodes.Last());

                    })
                });
                break;
            }

            // TODO
            case "C":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // // Straight shot
                    // (0.4, () =>
                    // {
                    //     var nodes = AddBranch(start, director.ZoneCount, "primary",
                    //         (node, zone) => zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward);
                    //
                    //     exit = nodes.Last();
                    //     exitZone = planner.GetZone(exit)!;
                    // }),

                    // Build keycard locked puzzle
                    (0.4, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(3, 4));
                        exit = nodes.Last();
                        exitZone = planner.GetZone(exit)!;

                        var hub = nodes.ElementAt(nodes.Count - 2);
                        var hubZone = planner.GetZone(hub)!;

                        hubZone.AmmoPacks += 3.0;
                        hubZone.ToolPacks += 2.0;

                        var population = WavePopulation.Baseline;
                        var settings = WaveSettings.Baseline_Normal;

                        AddApexAlarm(exit, population, settings);
                    }),
                });
                break;
            }

            // TODO
            case "D":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard to Apex alarm
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, midZone) = BuildChallenge_KeycardInSide(nodes.Last(), Generator.Between(1, 2));

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid, new ZoneNode { Branch = "primary" });
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (exit, exitZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                    }),

                    // Boss fight to Apex
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));

                        var (mid, midZone) = BuildChallenge_BossFight(nodes.Last());

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid, new ZoneNode { Branch = "primary" });
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (exit, exitZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                    }),

                    // Error with off + cell carry
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start,
                            Generator.Between(2, 4),
                            1);

                        (exit, exitZone) = AddZone(mid, new ZoneNode { Branch = "exit" });
                    }),

                    // Error with off + key card
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(1, 3),
                            Generator.Between(1, 2),
                            1);

                        (exit, exitZone) = AddZone(mid, new ZoneNode { Branch = "exit" });
                    }),

                    // Boss Fight: Mega Mother
                    // We also do some interesting prelude zones to get to megamom
                    (0.20, () =>
                    {
                        // var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var bossStart = start;

                        // Have some choices on arriving at the mega mom
                        Generator.SelectRun(new List<(double, Action)>
                        {
                            // Generator access
                            (0.35, () =>
                            {
                                var nodes = AddBranch_Forward(start, 2);
                                (bossStart, _) = BuildChallenge_GeneratorCellInSide(
                                    nodes.Last(),
                                    Generator.Between(1, 2));
                            }),

                            // Keycard access
                            (0.45, () =>
                            {
                                var nodes = AddBranch_Forward(start, 2);
                                (bossStart, _) = BuildChallenge_KeycardInSide(
                                    nodes.Last(),
                                    Generator.Between(1, 2));
                            }),

                            // Apex alarm
                            (0.20, () =>
                            {
                                var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                                (bossStart, _) = BuildChallenge_ApexAlarm(
                                    nodes.Last(),
                                    WavePopulation.Baseline_Hybrids,
                                    WaveSettings.Baseline_Hard);
                            })
                        });

                        var (boss, bossZone) = AddZone(
                            bossStart,
                            new ZoneNode
                            {
                                Branch = "boss_fight",
                                Tags = new Tags("no_blood_door")
                            });
                        (exit, exitZone) = AddZone(boss, "exit");

                        exitZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

                        var bossClearEvents = new List<WardenObjectiveEvent>()
                            .AddUnlockDoor(director.Bulkhead, exit.ZoneNumber);

                        AddAlignedBossFight_MegaMom(boss, bossClearEvents);
                    })
                });
                break;
            }

            case "E":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard to Apex alarm
                    (0.10, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, midZone) = BuildChallenge_KeycardInSide(nodes.Last(), Generator.Between(1, 2));

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid, new ZoneNode { Branch = "primary" });
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (exit, exitZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                    }),

                    // Boss fight to Apex
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));

                        var (mid, midZone) = BuildChallenge_BossFight(nodes.Last());

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid, new ZoneNode { Branch = "primary" });
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (exit, exitZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                    }),

                    // Error with off + cell carry
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start,
                            Generator.Between(2, 4),
                            1);

                        (exit, exitZone) = AddZone(mid, new ZoneNode { Branch = "exit" });
                    }),

                    // Error with off + key card
                    (0.15, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(1, 3),
                            Generator.Between(1, 2),
                            1);

                        (exit, exitZone) = AddZone(mid, new ZoneNode { Branch = "exit" });
                    }),

                    // Boss Fight: Mega Mother
                    // We also do some interesting prelude zones to get to megamom
                    (0.25, () =>
                    {
                        // var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var bossStart = start;

                        // Have some choices on arriving at the mega mom
                        Generator.SelectRun(new List<(double, Action)>
                        {
                            // Generator access
                            (0.35, () =>
                            {
                                var nodes = AddBranch_Forward(start, 2);
                                (bossStart, _) = BuildChallenge_GeneratorCellInSide(
                                    nodes.Last(),
                                    Generator.Between(1, 2));
                            }),

                            // Keycard access
                            (0.45, () =>
                            {
                                var nodes = AddBranch_Forward(start, 2);
                                (bossStart, _) = BuildChallenge_KeycardInSide(
                                    nodes.Last(),
                                    Generator.Between(1, 2));
                            }),

                            // Apex alarm
                            (0.20, () =>
                            {
                                var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                                (bossStart, _) = BuildChallenge_ApexAlarm(
                                    nodes.Last(),
                                    WavePopulation.Baseline_Hybrids,
                                    WaveSettings.Baseline_Hard);
                            })
                        });

                        var (boss, bossZone) = AddZone(
                            bossStart,
                            new ZoneNode
                            {
                                Branch = "boss_fight",
                                Tags = new Tags("no_blood_door")
                            });
                        (exit, exitZone) = AddZone(boss, "exit");

                        exitZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

                        var bossClearEvents = new List<WardenObjectiveEvent>()
                            .AddUnlockDoor(director.Bulkhead, exit.ZoneNumber);

                        AddAlignedBossFight_MegaMom(boss, bossClearEvents);
                    })
                });
                break;
            }
        }

        // Configure the exit zone
        exit = planner.UpdateNode(exit with { Branch = "exit", Tags = exit.Tags.Extend("exit_elevator") });
        exitZone.GenExitGeomorph(level.Complex);
        exitZone.Coverage = new() { Min = 64, Max = 64 };

        // Ensure there's a nice spicy hoard at the end
        exitZone.EnemySpawningInZone.Add(
            // These will be predominately strikers / shooters
            new EnemySpawningData()
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = (uint)EnemyRoleDifficulty.Easy,
                Points = 75, // 25pts is 1.0 distribution, this is quite a lot
            });
    }
}
