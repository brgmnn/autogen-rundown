using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Utils;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="_"></param>
    /// <param name="startish"></param>
    /// <exception cref="Exception"></exception>
    public void BuildLayout_CorruptedTerminalUplink(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var end = new ZoneNode();
        var endZone = new Zone(level, this);

        // TODO: flesh out D/E tier a bit more. Especially E-tier with 3 terminals
        switch (level.Tier, objective.Uplink_NumberOfTerminals)
        {
            case ("A", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight shot
                    (0.20, () =>
                    {
                        var nodes = AddBranch(start, Generator.Between(2, 3));

                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Single generator
                    (0.35, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                    }),

                    // Single keycard mid
                    (0.25, () =>
                    {
                        var next = AddBranch_Forward(start, 1).Last();
                        (next, _) = BuildChallenge_KeycardInSide(next);
                        var nodes = AddBranch_Forward(next, Generator.Between(1, 2));

                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                });

                planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                break;
            }

            case ("B", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex Alarm end
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(3, 4));
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;

                        var hub = nodes.ElementAt(nodes.Count - 2);
                        var hubZone = planner.GetZone(hub)!;

                        hubZone.AmmoPacks += 3.0;
                        hubZone.ToolPacks += 2.0;

                        var population = WavePopulation.Baseline;
                        var settings = WaveSettings.Baseline_Normal;

                        AddApexAlarm(end, population, settings);
                    }),

                    // Straight shot
                    (0.10, () =>
                    {
                        var nodes = AddBranch(start, Generator.Between(2, 4));

                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Single generator
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                    }),

                    // Single keycard mid
                    (0.15, () =>
                    {
                        // TODO: had the complaint
                        var next = AddBranch_Forward(start, 1).Last();
                        (next, _) = BuildChallenge_KeycardInSide(next);
                        var nodes = AddBranch_Forward(next, Generator.Between(1, 2));

                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Single keycard end
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        (end, endZone) = BuildChallenge_KeycardInSide(nodes.Last());

                    })
                });

                planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                break;
            }

            case ("C", 2):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard to Apex alarm
                    // Apex alarm zone is the first uplink area
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, midZone) = BuildChallenge_KeycardInSide(nodes.Last());

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Normal);

                        planner.UpdateNode(mid2 with { Tags = mid2.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    }),

                    // Double generator
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());

                        var nodes2 = AddBranch_Forward(mid, 1);
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(nodes2.Last());

                        planner.UpdateNode(mid with { Tags = mid.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    }),

                    // Generator to boss
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());

                        var nodes2 = AddBranch_Forward(mid, 1);
                        (end, endZone) = BuildChallenge_BossFight(nodes2.Last());

                        planner.UpdateNode(mid with { Tags = mid.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    }),

                    // Error with off + key card
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(2, 3),
                            1,
                            1);

                        (end, endZone) = AddZone(mid);

                        var penultimate = planner.GetParent(end);

                        if (penultimate is not null)
                        {
                            var node = (ZoneNode)penultimate;
                            planner.UpdateNode(node with { Tags = node.Tags.Extend("uplink_terminal") });
                        }

                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    })
                });
                break;
            }

            case ("C", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard to Apex alarm
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, midZone) = BuildChallenge_KeycardInSide(nodes.Last());

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Normal);
                    }),

                    // Double generator
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());

                        var nodes2 = AddBranch_Forward(mid, 1);
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(nodes2.Last());
                    }),

                    // Generator to boss
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());

                        var nodes2 = AddBranch_Forward(mid, 1);
                        (end, endZone) = BuildChallenge_BossFight(nodes2.Last());
                    }),

                    // Error with off + key card
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(2, 3),
                            1,
                            1);

                        (end, endZone) = AddZone(mid);
                    })
                });

                planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                break;
            }

            case ("D", 2):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard to Apex alarm
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, midZone) = BuildChallenge_KeycardInSide(nodes.Last(), Generator.Between(1, 2));

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);

                        planner.UpdateNode(mid2 with { Tags = mid2.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    }),

                    // Boss fight to Apex
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));

                        var (mid, midZone) = BuildChallenge_BossFight(nodes.Last());

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);

                        planner.UpdateNode(mid with { Tags = mid.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    }),

                    // Error with off + cell carry
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start,
                            Generator.Between(2, 4),
                            1);

                        (end, endZone) = AddZone(mid);

                        var penultimate = planner.GetParent(mid);
                        if (penultimate is not null)
                        {
                            var node = (ZoneNode)penultimate;
                            planner.UpdateNode(node with { Tags = node.Tags.Extend("uplink_terminal") });
                        }

                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    }),

                    // Error with off + key card
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(1, 3),
                            Generator.Between(1, 2),
                            1);

                        (end, endZone) = AddZone(mid);

                        var penultimate = planner.GetParent(mid);
                        if (penultimate is not null)
                        {
                            var node = (ZoneNode)penultimate;
                            planner.UpdateNode(node with { Tags = node.Tags.Extend("uplink_terminal") });
                        }

                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    })
                });
                break;
            }

            case ("D", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard to Apex alarm
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, midZone) = BuildChallenge_KeycardInSide(nodes.Last(), Generator.Between(1, 2));

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
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

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
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

                        (end, endZone) = AddZone(mid);
                    }),

                    // Error with off + key card
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(1, 3),
                            Generator.Between(1, 2),
                            1);

                        (end, endZone) = AddZone(mid);
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
                        (end, endZone) = AddZone(boss);

                        endZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

                        var bossClearEvents = new List<WardenObjectiveEvent>()
                            .AddUnlockDoor(director.Bulkhead, end.ZoneNumber);

                        AddAlignedBossFight_MegaMom(boss, bossClearEvents);
                    })
                });

                planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                break;
            }

            case ("E", 3):
            {
                // Something more custom than normal
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub with 2 side zones of progressive pain
                    (0.10, () =>
                    {
                        var (corridor, corridorZone) = AddZone(start);
                        corridorZone.GenCorridorGeomorph(level.Complex);

                        var (hub, hubZone) = AddZone(corridor);
                        corridorZone.GenHubGeomorph(level.Complex);
                        planner.UpdateNode(hub with { Tags = hub.Tags.Extend("uplink_terminal") });

                        var (side1, side1Zone) = AddZone(hub);
                        corridorZone.GenDeadEndGeomorph(level.Complex);
                        planner.UpdateNode(hub with { Tags = hub.Tags.Extend("uplink_terminal") });

                        var nodes = AddBranch_Forward(
                            hub,
                            Generator.Between(2, 3),
                            "side_uplink",
                            (node, zone) =>
                            {

                            });

                        // Side 2 is a BIG zone at the end.
                        var side2 = nodes.Last();
                        var side2Zone = level.Planner.GetZone(side2);

                        planner.UpdateNode(side2 with { Tags = side2.Tags.Extend("uplink_terminal") });
                        side2Zone.Coverage = new CoverageMinMax { Min = 100, Max = 150 };
                    }),

                    // Error with off + cell carry
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start,
                            Generator.Between(2, 4),
                            1);

                        (end, endZone) = AddZone(mid);

                        // We just distribute the uplinks throughout the area
                        planner.UpdateNode(start with { Tags = start.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(mid with { Tags = mid.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    }),
                });
                break;
            }

            case ("E", 2):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard to Apex alarm
                    (0.10, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, midZone) = BuildChallenge_KeycardInSide(nodes.Last(), Generator.Between(1, 2));

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);

                        planner.UpdateNode(mid2 with { Tags = mid2.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    }),

                    // Boss fight to Apex
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));

                        var (mid, midZone) = BuildChallenge_BossFight(nodes.Last());

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);

                        planner.UpdateNode(mid2 with { Tags = mid2.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    }),

                    // Error with off + cell carry
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start,
                            Generator.Between(2, 4),
                            1);

                        (end, endZone) = AddZone(mid);

                        planner.UpdateNode(mid with { Tags = mid.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    }),

                    // Error with off + key card
                    (0.15, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(1, 3),
                            Generator.Between(1, 2),
                            1);

                        planner.UpdateNode(mid with { Tags = mid.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });

                        (end, endZone) = AddZone(mid);
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
                                    WaveSettings.Baseline_VeryHard);
                            })
                        });

                        var (boss, bossZone) = AddZone(
                            bossStart,
                            new ZoneNode
                            {
                                Branch = "boss_fight",
                                Tags = new Tags("no_blood_door")
                            });
                        (end, endZone) = AddZone(boss);

                        endZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

                        var bossClearEvents = new List<WardenObjectiveEvent>()
                            .AddUnlockDoor(director.Bulkhead, end.ZoneNumber);

                        AddAlignedBossFight_MegaMom(boss, bossClearEvents);

                        planner.UpdateNode(boss with { Tags = boss.Tags.Extend("uplink_terminal") });
                        planner.UpdateNode(end with { Tags = end.Tags.Extend("uplink_terminal") });
                    })
                });
                break;
            }
        }
    }
}
