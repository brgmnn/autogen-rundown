using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    public void BuildLayout_SpecialTerminalCommand_Fast(ZoneNode start)
    {
        // Mark the first zone to be where the terminal will spawn
        planner.UpdateNode(start with { Branch = "special_terminal" });

        // Largeish zone size
        var zone = planner.GetZone(start)!;
        zone.Coverage = new CoverageMinMax { Min = 32, Max = 64 };
    }

    /// <summary>
    /// Dispatcher for special terminal command layouts. Delegates to per-command-type methods.
    /// </summary>
    public void BuildLayout_SpecialTerminalCommand(
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

        // Fast path for Survival/ReachKdsDeep secondary objectives
        if (level.MainDirector.Objective
            is WardenObjectiveType.Survival
            or WardenObjectiveType.ReachKdsDeep)
        {
            BuildLayout_SpecialTerminalCommand_Fast(start);
            return;
        }

        switch (objective.SpecialTerminalCommand_Type)
        {
            case SpecialCommand.LightsOff:
                BuildLayout_SpecialTerminalCommand_LightsOff(start);
                break;

            case SpecialCommand.FillWithFog:
                BuildLayout_SpecialTerminalCommand_FillWithFog(start);
                break;

            case SpecialCommand.ErrorAlarm:
                BuildLayout_SpecialTerminalCommand_ErrorAlarm(start);
                break;

            case SpecialCommand.KingOfTheHill:
                BuildLayout_SpecialTerminalCommand_KingOfTheHill(start, objective);
                break;
        }
    }

    #region LightsOff

    /// <summary>
    /// LightsOff: Power infrastructure paths. The real challenge comes after the command
    /// kills the lights during extraction — approach paths are straightforward.
    /// </summary>
    private void BuildLayout_SpecialTerminalCommand_LightsOff(ZoneNode start)
    {
        var threeBulkheads = level.HasExtreme && level.HasOverload;

        var end = new ZoneNode();
        var endZone = new Zone(level, this);

        switch (level.Tier, director.Bulkhead)
        {
            case ("A", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward 2 zones
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Keycard in side
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        (end, endZone) = BuildChallenge_KeycardInSide(nodes.Last());
                    }),

                    // Single zone
                    (0.25, () =>
                    {
                        var nodes = AddBranch(start, 1, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Generator cell in zone
                    (0.20, () =>
                    {
                        (end, endZone) = BuildChallenge_GeneratorCellInZone(start);
                    }),
                });
                break;
            }

            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward 2-3 + keyed puzzle
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3), "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                        AddKeyedPuzzle(end, "special_terminal", 2);
                    }),

                    // Keycard in side
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        (end, endZone) = BuildChallenge_KeycardInSide(nodes.Last());
                    }),

                    // Generator cell in side + 1 zone
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(start);
                        var nodes = AddBranch(mid, 1, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Locked terminal door
                    (0.20, () =>
                    {
                        (end, endZone) = BuildChallenge_LockedTerminalDoor(start, 1);
                    }),
                });
                break;
            }

            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard mid + generator end
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(mid);
                    }),

                    // Sensors + keycard
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        AddSecuritySensors(nodes.First());
                        (end, endZone) = BuildChallenge_KeycardInSide(nodes.Last());
                    }),

                    // Forward 2 + keyed puzzle
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                        AddKeyedPuzzle(end, "special_terminal", 2);
                    }),

                    // Travel scan + keycard
                    (0.20, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        (end, endZone) = BuildChallenge_KeycardInSide(travelEnd);
                    }),
                });
                break;
            }

            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard + sensors + generator
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        AddSecuritySensors(mid);
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(mid);
                    }),

                    // Locked terminal password in side
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        (end, endZone) = BuildChallenge_LockedTerminalPasswordInSide(nodes.Last());
                    }),

                    // Boss fight + 1
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (mid, _) = BuildChallenge_BossFight(nodes.Last());
                        var endNodes = AddBranch(mid, 1, "special_terminal");
                        end = endNodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Error with off + keycard
                    (0.20, () =>
                    {
                        (end, endZone) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start, Generator.Between(2, 3), 1, 1);
                    }),
                });
                break;
            }

            case ("E", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Boss + generator
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (mid, _) = BuildChallenge_BossFight(nodes.Last());
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(mid);
                    }),

                    // Error with off + cell carry
                    (0.25, () =>
                    {
                        (end, endZone) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start, Generator.Between(2, 4), 1);
                    }),

                    // Sensors + keycard + apex alarm
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        AddSecuritySensors(nodes.Last());
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last(), Generator.Between(1, 2));
                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid, WavePopulation.Baseline_Hybrids, WaveSettings.Baseline_VeryHard);
                    }),

                    // Travel scan + boss
                    (0.20, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        (end, endZone) = BuildChallenge_BossFight(travelEnd);
                    }),
                });
                break;
            }

            // Extreme — short approach
            case ("A" or "B", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.50, () =>
                    {
                        var nodes = AddBranch_Forward(start, threeBulkheads ? 1 : Generator.Between(1, 2), "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                    (0.50, () =>
                    {
                        (end, endZone) = BuildChallenge_KeycardInZone(start);
                    }),
                });
                break;
            }

            case (_, Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.50, () =>
                    {
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(start);
                    }),
                    (0.50, () =>
                    {
                        (end, endZone) = BuildChallenge_KeycardInSide(start);
                    }),
                });
                break;
            }

            // Overload — minimal
            case ("A" or "B", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.60, () =>
                    {
                        var nodes = AddBranch(start, 1, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                    (0.40, () =>
                    {
                        (end, endZone) = BuildChallenge_KeycardInZone(start);
                    }),
                });
                break;
            }

            case (_, Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.60, () =>
                    {
                        (end, endZone) = BuildChallenge_GeneratorCellInZone(start);
                    }),
                    (0.40, () =>
                    {
                        (end, endZone) = BuildChallenge_LockedTerminalDoor(start, 0);
                    }),
                });
                break;
            }
        }

        // Ensure terminal zone is marked
        end = planner.UpdateNode(end with { Branch = "special_terminal" });

        if (director.Bulkhead == Bulkhead.Main)
            AddForwardExtractStart(end, chance: 0.3);

        if (director.Bulkhead != Bulkhead.Main)
            planner.UpdateNode(end with { Tags = end.Tags.Extend("bulkhead_candidate") });
    }

    #endregion

    #region FillWithFog

    /// <summary>
    /// FillWithFog: Linear forward paths — players retreat through rising fog.
    /// Avoids side-branch challenges (_InSide) that become disorienting in fog.
    /// Uses _InZone variants and straight paths. Sensors are thematic (detection in fog).
    /// </summary>
    private void BuildLayout_SpecialTerminalCommand_FillWithFog(ZoneNode start)
    {
        var threeBulkheads = level.HasExtreme && level.HasOverload;

        var end = new ZoneNode();
        var endZone = new Zone(level, this);

        switch (level.Tier, director.Bulkhead)
        {
            case ("A", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward 2
                    (0.40, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Forward 1 + keycard in zone
                    (0.35, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        (end, endZone) = BuildChallenge_KeycardInZone(nodes.Last());
                    }),

                    // Single zone
                    (0.25, () =>
                    {
                        var nodes = AddBranch(start, 1, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                });
                break;
            }

            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward 2-3
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3), "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Forward 1-2 + keycard in zone
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        (end, endZone) = BuildChallenge_KeycardInZone(nodes.Last());
                    }),

                    // Forward 2 + keyed puzzle
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                        AddKeyedPuzzle(end, "special_terminal", 2);
                    }),

                    // Travel scan + forward 1
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var nodes = AddBranch(travelEnd, 1, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                });
                break;
            }

            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward 2 + generator in zone
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        (end, endZone) = BuildChallenge_GeneratorCellInZone(nodes.Last());
                    }),

                    // Forward 3 + keyed puzzle
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 3, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                        AddKeyedPuzzle(end, "special_terminal", 2);
                    }),

                    // Keycard in zone mid + forward 1
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_KeycardInZone(nodes.Last());
                        var endNodes = AddBranch_Forward(mid, 1, "special_terminal");
                        end = endNodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Travel scan + forward 2
                    (0.20, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var nodes = AddBranch_Forward(travelEnd, 2, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                });
                break;
            }

            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward 3 + generator in zone
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 3);
                        (end, endZone) = BuildChallenge_GeneratorCellInZone(nodes.Last());
                    }),

                    // Forward 2 + keycard in zone + forward 1
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (mid, _) = BuildChallenge_KeycardInZone(nodes.Last());
                        var endNodes = AddBranch_Forward(mid, 1, "special_terminal");
                        end = endNodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Sensors + forward 3
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 3, "special_terminal");
                        AddSecuritySensors(nodes.First());
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Forward 2 + locked terminal door
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        (end, endZone) = BuildChallenge_LockedTerminalDoor(nodes.Last(), 0);
                    }),
                });
                break;
            }

            case ("E", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward 3 + sensors + generator in zone
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 3);
                        AddSecuritySensors(nodes.ElementAt(1));
                        (end, endZone) = BuildChallenge_GeneratorCellInZone(nodes.Last());
                    }),

                    // Boss fight + forward 1
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (mid, _) = BuildChallenge_BossFight(nodes.Last());
                        var endNodes = AddBranch_Forward(mid, 1, "special_terminal");
                        end = endNodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Forward 2 + keycard in zone + generator in zone
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (mid, _) = BuildChallenge_KeycardInZone(nodes.Last());
                        (end, endZone) = BuildChallenge_GeneratorCellInZone(mid);
                    }),

                    // Travel scan + forward 2 + keyed puzzle
                    (0.20, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var nodes = AddBranch_Forward(travelEnd, 2, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                        AddKeyedPuzzle(end, "special_terminal", 2);
                    }),
                });
                break;
            }

            // Extreme — short linear
            case (_, Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.60, () =>
                    {
                        var nodes = AddBranch_Forward(start, threeBulkheads ? 1 : Generator.Between(1, 2), "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                    (0.40, () =>
                    {
                        (end, endZone) = BuildChallenge_KeycardInZone(start);
                    }),
                });
                break;
            }

            // Overload — minimal
            case (_, Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.70, () =>
                    {
                        var nodes = AddBranch(start, 1, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                    (0.30, () =>
                    {
                        (end, endZone) = BuildChallenge_GeneratorCellInZone(start);
                    }),
                });
                break;
            }
        }

        // Ensure terminal zone is marked
        end = planner.UpdateNode(end with { Branch = "special_terminal" });

        if (director.Bulkhead == Bulkhead.Main)
            AddForwardExtractStart(end, chance: 0.25);

        if (director.Bulkhead != Bulkhead.Main)
            planner.UpdateNode(end with { Tags = end.Tags.Extend("bulkhead_candidate") });
    }

    #endregion

    #region ErrorAlarm

    /// <summary>
    /// ErrorAlarm: Security breach. Command triggers enemy waves (handled by objective).
    /// Short, defensible approach — extra resources in terminal zone.
    /// Note: ErrorAlarm never appears in A-tier.
    /// </summary>
    private void BuildLayout_SpecialTerminalCommand_ErrorAlarm(ZoneNode start)
    {
        var threeBulkheads = level.HasExtreme && level.HasOverload;

        var end = new ZoneNode();
        var endZone = new Zone(level, this);

        switch (level.Tier, director.Bulkhead)
        {
            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward 2 straight
                    (0.40, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Keycard in side
                    (0.35, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        (end, endZone) = BuildChallenge_KeycardInSide(nodes.Last());
                    }),

                    // Generator in zone + 1
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_GeneratorCellInZone(start);
                        var nodes = AddBranch(mid, 1, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                });
                break;
            }

            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard in side + forward 1
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var endNodes = AddBranch_Forward(mid, 1, "special_terminal");
                        end = endNodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Generator in side + forward 1
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(start);
                        var endNodes = AddBranch_Forward(mid, 1, "special_terminal");
                        end = endNodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Forward 2 + keyed puzzle
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                        AddKeyedPuzzle(end, "special_terminal", 2);
                    }),

                    // Sensors + straight 2
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2, "special_terminal");
                        AddSecuritySensors(nodes.First());
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                });
                break;
            }

            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard + generator
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(mid);
                    }),

                    // Sensors + keycard in side
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        AddSecuritySensors(nodes.First());
                        (end, endZone) = BuildChallenge_KeycardInSide(nodes.Last());
                    }),

                    // Boss + terminal
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        (end, endZone) = BuildChallenge_BossFight(nodes.Last());
                    }),

                    // Travel scan + generator
                    (0.20, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(travelEnd);
                    }),
                });
                break;
            }

            case ("E", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Boss + generator
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (mid, _) = BuildChallenge_BossFight(nodes.Last());
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(mid);
                    }),

                    // Sensors + keycard + apex alarm
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        AddSecuritySensors(nodes.Last());
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last(), Generator.Between(1, 2));
                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid, WavePopulation.Baseline_Hybrids, WaveSettings.Baseline_VeryHard);
                    }),

                    // Forward 3 + keyed puzzle
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 3, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                        AddKeyedPuzzle(end, "special_terminal", 2);
                    }),

                    // Travel scan + boss
                    (0.20, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        (end, endZone) = BuildChallenge_BossFight(travelEnd);
                    }),
                });
                break;
            }

            // Extreme — very short
            case (_, Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.60, () =>
                    {
                        var nodes = AddBranch(start, threeBulkheads ? 1 : Generator.Between(1, 2), "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                    (0.40, () =>
                    {
                        (end, endZone) = BuildChallenge_KeycardInZone(start);
                    }),
                });
                break;
            }

            // Overload — minimal
            case (_, Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.60, () =>
                    {
                        var nodes = AddBranch(start, 1, "special_terminal");
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),
                    (0.40, () =>
                    {
                        (end, endZone) = BuildChallenge_GeneratorCellInZone(start);
                    }),
                });
                break;
            }

            // Fallback for A-tier (ErrorAlarm shouldn't appear in A, but handle gracefully)
            default:
            {
                var nodes = AddBranch_Forward(start, 2, "special_terminal");
                end = nodes.Last();
                endZone = planner.GetZone(end)!;
                break;
            }
        }

        // Ensure terminal zone is marked
        end = planner.UpdateNode(end with { Branch = "special_terminal" });

        // Extra resources for the error alarm fight
        endZone.AmmoPacks += 3.0;
        endZone.ToolPacks += 2.0;

        if (director.Bulkhead == Bulkhead.Main)
            AddForwardExtractStart(end, chance: 0.4);

        if (director.Bulkhead != Bulkhead.Main)
            planner.UpdateNode(end with { Tags = end.Tags.Extend("bulkhead_candidate") });
    }

    #endregion

    #region KingOfTheHill

    /// <summary>
    /// KingOfTheHill: Sustained defense. SelectRun varies the prelude (path to the hill zone).
    /// The hill zone + spawn zones are always appended after the prelude.
    /// </summary>
    private void BuildLayout_SpecialTerminalCommand_KingOfTheHill(ZoneNode start, WardenObjective objective)
    {
        var threeBulkheads = level.HasExtreme && level.HasOverload;

        var hillStart = start;

        // Build the prelude path to the hill
        switch (level.Tier, director.Bulkhead)
        {
            case ("A", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Direct — no prelude
                    (0.50, () => { }),

                    // Forward 1
                    (0.30, () =>
                    {
                        hillStart = AddBranch_Forward(start, 1).Last();
                    }),

                    // Keycard in zone
                    (0.20, () =>
                    {
                        (hillStart, _) = BuildChallenge_KeycardInZone(start);
                    }),
                });
                break;
            }

            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward 1
                    (0.35, () =>
                    {
                        hillStart = AddBranch_Forward(start, 1).Last();
                    }),

                    // Keycard in side
                    (0.35, () =>
                    {
                        (hillStart, _) = BuildChallenge_KeycardInSide(start);
                    }),

                    // Generator in zone
                    (0.30, () =>
                    {
                        (hillStart, _) = BuildChallenge_GeneratorCellInZone(start);
                    }),
                });
                break;
            }

            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Generator in side
                    (0.30, () =>
                    {
                        (hillStart, _) = BuildChallenge_GeneratorCellInSide(start);
                    }),

                    // Keycard in side + forward 1
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_KeycardInSide(start);
                        hillStart = AddBranch_Forward(mid, 1).Last();
                    }),

                    // Forward 1 + sensors
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        AddSecuritySensors(nodes.Last());
                        hillStart = nodes.Last();
                    }),

                    // Travel scan
                    (0.20, () =>
                    {
                        (hillStart, _) = AddTravelScanAlarm(start);
                    }),
                });
                break;
            }

            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard in side + generator in zone
                    (0.30, () =>
                    {
                        var (mid, _) = BuildChallenge_KeycardInSide(start);
                        (hillStart, _) = BuildChallenge_GeneratorCellInZone(mid);
                    }),

                    // Forward 2 + sensors
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        AddSecuritySensors(nodes.Last());
                        hillStart = nodes.Last();
                    }),

                    // Boss fight
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        (hillStart, _) = BuildChallenge_BossFight(nodes.Last());
                    }),

                    // Error with off + keycard
                    (0.20, () =>
                    {
                        (hillStart, _) = BuildChallenge_ErrorWithOff_KeycardInSide(start, 1, 1, 1);
                    }),
                });
                break;
            }

            case ("E", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Boss + generator
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_BossFight(nodes.Last());
                        (hillStart, _) = BuildChallenge_GeneratorCellInZone(mid);
                    }),

                    // Apex alarm
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        (hillStart, _) = BuildChallenge_ApexAlarm(
                            nodes.Last(), WavePopulation.Baseline_Hybrids, WaveSettings.Baseline_VeryHard);
                    }),

                    // Error with off + cell carry
                    (0.25, () =>
                    {
                        (hillStart, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(start, 2, 1);
                    }),

                    // Sensors + keycard in side
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        AddSecuritySensors(nodes.Last());
                        (hillStart, _) = BuildChallenge_KeycardInSide(nodes.Last(), 2);
                    }),
                });
                break;
            }

            // Extreme — light prelude
            case ("A" or "B", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.60, () => { }),
                    (0.40, () =>
                    {
                        (hillStart, _) = BuildChallenge_KeycardInZone(start);
                    }),
                });
                break;
            }

            case (_, Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.50, () =>
                    {
                        (hillStart, _) = BuildChallenge_GeneratorCellInZone(start);
                    }),
                    (0.50, () =>
                    {
                        (hillStart, _) = BuildChallenge_KeycardInZone(start);
                    }),
                });
                break;
            }

            // Overload — minimal prelude
            case (_, Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.60, () => { }),
                    (0.40, () =>
                    {
                        (hillStart, _) = BuildChallenge_GeneratorCellInZone(start);
                    }),
                });
                break;
            }
        }

        // Build the hill zone — shorter for secondary bulkheads
        var hillForwardCount = director.Bulkhead == Bulkhead.Main ? 2 : 1;
        var hillNodes = AddBranch_Forward(hillStart, hillForwardCount, "special_terminal");
        var hill = hillNodes.Last();
        var hillZone = planner.GetZone(hill)!;

        AddForwardExtractStart(hillNodes.First());

        hillZone.GenKingOfTheHillGeomorph(PersistentId, director);
        hillZone.TerminalPlacements = new List<TerminalPlacement>
        {
            new() { PlacementWeights = ZonePlacementWeights.AtEnd }
        };

        // Spawn zones — scale by tier, reduce for secondary bulkheads
        var spawnZoneCount = (level.Tier, director.Bulkhead) switch
        {
            ("A", Bulkhead.Main) => 2,
            ("B", Bulkhead.Main) => 3,
            (_, Bulkhead.Main) => 3,
            ("D" or "E", _) => 3,
            _ => 2,
        };

        for (var num = 0; num < spawnZoneCount; num++)
        {
            const string branch = "hill_spawn";
            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
            var node = new ZoneNode(director.Bulkhead, zoneIndex, branch, 0);
            node.Tags.Add("no_enemies");

            var zone = new Zone(level, this)
            {
                LightSettings = Lights.GenRandomLight(),
            };
            zone.RollFog(level);
            zone.GenDeadEndGeomorph(director.Complex);

            // No terminals needed in the spawn zones
            zone.TerminalPlacements = new List<TerminalPlacement>();

            // No alarm needed on the door and have it be locked
            zone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;
            zone.Alarm = ChainedPuzzle.SkipZone;

            // Add event to open the door on activation
            objective.EventsOnActivate.AddOpenDoor(director.Bulkhead, node.ZoneNumber);

            level.Planner.Connect(hill, node);
            level.Planner.AddZone(node, zone);
        }
    }

    #endregion
}
