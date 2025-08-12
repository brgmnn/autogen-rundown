using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

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
    public void BuildLayout_GatherSmallItems(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;

        var last = new ZoneNode();
        var lastZone = new Zone(level);

        switch (level.Tier, director.Bulkhead, objective.GatherRequiredCount)
        {
            #region Tier: A
            case ("A", Bulkhead.Main, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Double dead end
                    (0.30, () =>
                    {
                        startZone.GenHubGeomorph(level.Complex);

                        var (end1, end1Zone) = AddZone(start);
                        end1Zone.GenDeadEndGeomorph(level.Complex);

                        var (end2, end2Zone) = AddZone(start);
                        end2Zone.GenDeadEndGeomorph(level.Complex);

                        objective.Gather_PlacementNodes.Add(end1);
                        objective.Gather_PlacementNodes.Add(end2);
                    }),

                    // Single generator
                    (0.28, () =>
                    {
                        (last, lastZone) = BuildChallenge_GeneratorCellInSide(start);

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;
                    }),

                    // Single keycard
                    (0.34, () =>
                    {
                        (last, lastZone) = BuildChallenge_KeycardInSide(start);

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;
                    }),

                    // Single boss fight
                    (0.08, () =>
                    {
                        (last, lastZone) = BuildChallenge_BossFight(start);

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;
                    }),
                });
                break;
            }

            case ("A", Bulkhead.Extreme, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Just fetch from the first zone
                    (0.2, () =>
                    {
                        objective.Gather_PlacementNodes.Add(start);
                        startZone.Coverage = CoverageMinMax.Medium;
                    }),

                    // Fetch from the second room, but it's a dead end
                    (0.2, () =>
                    {
                        (last, lastZone) = AddZone(start);
                        lastZone.GenDeadEndGeomorph(level.Complex);
                        objective.Gather_PlacementNodes.Add(last);
                    }),
                });
                break;
            }

            case ("A", Bulkhead.Overload, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Fetch from the second room, but it's a dead end
                    (0.3, () =>
                    {
                        (last, lastZone) = AddZone(start);
                        lastZone.GenDeadEndGeomorph(level.Complex);
                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Tiny prelude
                    (0.5, () =>
                    {
                        (last, lastZone) = AddZone(start);
                        startZone.Coverage = new CoverageMinMax { Min = 10, Max = 10 };
                        lastZone.Coverage = CoverageMinMax.Medium;
                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Scout room
                    (0.2, () =>
                    {
                        last = AddScoutRoom(start);
                        objective.Gather_PlacementNodes.Add(last);
                    })
                });
                break;
            }
            #endregion

            #region Tier: B
            case ("B", Bulkhead.Main, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub to dead ends
                    (0.30, () =>
                    {
                        if (objective.GatherRequiredCount <= 7)
                        {
                            startZone.GenHubGeomorph(level.Complex);

                            var (end1, end1Zone) = AddZone(start);
                            end1Zone.GenDeadEndGeomorph(level.Complex);

                            var (end2, end2Zone) = AddZone(start);
                            end2Zone.GenDeadEndGeomorph(level.Complex);

                            objective.Gather_PlacementNodes.Add(end1);
                            objective.Gather_PlacementNodes.Add(end2);
                        }
                        else
                        {
                            startZone.GenCorridorGeomorph(level.Complex);

                            var (hub, hubZone) = AddZone(start);
                            hubZone.GenHubGeomorph(level.Complex);

                            var (end1, end1Zone) = AddZone(hub);
                            end1Zone.GenDeadEndGeomorph(level.Complex);

                            var (end2, end2Zone) = AddZone(hub);
                            end2Zone.GenDeadEndGeomorph(level.Complex);

                            var (end3, end3Zone) = AddZone(hub);
                            end3Zone.GenDeadEndGeomorph(level.Complex);

                            objective.Gather_PlacementNodes.Add(end1);
                            objective.Gather_PlacementNodes.Add(end2);
                            objective.Gather_PlacementNodes.Add(end3);
                        }
                    }),

                    // Single generator
                    // Items distributed between first zone and the locked zone
                    (0.28, () =>
                    {
                        (last, lastZone) = BuildChallenge_GeneratorCellInSide(start, 2);

                        objective.Gather_PlacementNodes.Add(start);
                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Single keycard, double end zone items
                    (0.15, () =>
                    {
                        var (mid, _) = BuildChallenge_KeycardInSide(start);
                        (last, _) = AddZone(mid);

                        objective.Gather_PlacementNodes.Add(mid);
                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Single keycard, split items
                    (0.19, () =>
                    {
                        (last, lastZone) = BuildChallenge_KeycardInSide(start);

                        var keycard = planner.GetLastNode(director.Bulkhead, "keycard");
                        var (second, _) = AddZone(keycard);

                        objective.Gather_PlacementNodes.Add(last);
                        objective.Gather_PlacementNodes.Add(keycard);
                    }),

                    // Single boss fight
                    (0.08, () =>
                    {
                        (last, lastZone) = BuildChallenge_BossFight(start);
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;

                        objective.Gather_PlacementNodes.Add(last);
                    }),
                });
                break;
            }

            case ("B", Bulkhead.Extreme, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Just fetch from the first zone
                    (0.2, () =>
                    {
                        startZone.Coverage = CoverageMinMax.Huge;

                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Fetch from the second room, but it's a dead end
                    (0.2, () =>
                    {
                        (last, lastZone) = AddZone(start);
                        lastZone.GenDeadEndGeomorph(level.Complex);

                        objective.Gather_PlacementNodes.Add(last);
                    }),
                });
                break;
            }

            case ("B", Bulkhead.Overload, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Tiny prelude
                    (0.40, () =>
                    {
                        (last, lastZone) = AddZone(start);
                        startZone.Coverage = new CoverageMinMax { Min = 10, Max = 10 };
                        lastZone.Coverage = CoverageMinMax.Medium;

                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // stealth big hub full of infected enemies
                    (0.20, () =>
                    {
                        startZone.GenHubGeomorph(level.Complex);
                        start = AddStealth_Infested(start);

                        objective.Gather_PlacementNodes.Add(start);
                    }),

                    // Big Apex alarm to enter
                    (0.20, () =>
                    {
                        var (lockedApex, _) = AddZone(start);

                        objective.Gather_PlacementNodes.Add(lockedApex);

                        // Add some extra resources
                        startZone.HealthPacks += 5;
                        startZone.ToolPacks += 4;
                        startZone.AmmoPacks += 5;

                        // Configure the wave population
                        var population = WavePopulation.Baseline_Hybrids;

                        // Chargers first, then flyers
                        if (level.Settings.HasChargers())
                            population = WavePopulation.Baseline_Chargers;

                        // Add the apex alarm
                        AddApexAlarm(lockedApex, population);
                    }),

                    // Scout room
                    (0.20, () =>
                    {
                        last = AddScoutRoom(start);

                        objective.Gather_PlacementNodes.Add(last);
                    })
                });
                break;
            }
            #endregion

            #region Tier: C
            case ("C", Bulkhead.Main, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.2, () =>
                    {
                        AddBranch(start, Generator.Between(1, 2), "find_items", (node, _) =>
                        {
                            objective.Gather_PlacementNodes.Add(node);
                        });
                    })
                });
                break;
            }
            #endregion

            #region Tier: D
            case ("D", Bulkhead.Main, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.2, () =>
                    {
                        AddBranch(start, Generator.Between(1, 2), "find_items", (node, _) =>
                        {
                            objective.Gather_PlacementNodes.Add(node);
                        });
                    })
                });
                break;
            }
            #endregion

            #region Tier: E
            case ("E", Bulkhead.Main, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.2, () =>
                    {
                        AddBranch(start, Generator.Between(1, 2), "find_items", (node, _) =>
                        {
                            objective.Gather_PlacementNodes.Add(node);
                        });
                    })
                });
                break;
            }
            #endregion

            default:
            {
                AddBranch(start, Generator.Between(1, 2), "find_items", (node, _) =>
                {
                    objective.Gather_PlacementNodes.Add(node);
                });
                break;
            }
        }
    }
}
