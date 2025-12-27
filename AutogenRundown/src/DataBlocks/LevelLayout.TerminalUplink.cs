using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    /// <summary>
    /// Adds terminal zones for the uplink objective and registers them in PlacementNodes
    /// </summary>
    private void AddUplinkTerminalZones(ZoneNode parent, WardenObjective objective)
    {
        var nodes = AddBranch(parent, objective.Uplink_NumberOfTerminals, "uplink_terminals");
        foreach (var node in nodes)
            objective.PlacementNodes.Add(node);
    }

    /// <summary>
    /// Builds an uplink terminal objective layout
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    public void BuildLayout_TerminalUplink(
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

        switch (level.Tier, director.Bulkhead)
        {
            #region A-tier
            // A-Main: 2-3+N zones (straight), 3-4+N zones (keycard), 3+N zones (generator)
            // 3 layouts
            case ("A", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight shot - simple forward path to terminal (2-3+N zones)
                    (0.40, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var last = nodes.Last();

                        // Add terminal zone at end
                        AddUplinkTerminalZones(last, objective);
                    }),

                    // Keycard in side branch (3-4+N zones)
                    (0.35, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());

                        // Add terminal zone after keycard challenge
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator cell challenge (3+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (end, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());

                        // Add terminal zone after generator challenge
                        AddUplinkTerminalZones(end, objective);
                    }),
                });
                break;
            }

            // A-Extreme: 2+N zones (simple), 1+N zones (keycard)
            // 2 layouts
            case ("A", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Simple path - short and easy (2+N zones)
                    (0.50, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var last = nodes.Last();

                        // Add terminal zone at end
                        AddUplinkTerminalZones(last, objective);
                    }),

                    // Keycard in same zone (1+N zones)
                    (0.50, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInZone(start);

                        // Add terminal zone after keycard
                        AddUplinkTerminalZones(end, objective);
                    }),
                });
                break;
            }

            // A-Overload: 2+N zones (generator), 2+N zones (locked terminal)
            // 2 layouts
            case ("A", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Generator cell - carry cell through (2+N zones)
                    (0.50, () =>
                    {
                        var (end, _) = BuildChallenge_GeneratorCellInSide(start, 1);

                        // Add terminal zone after generator challenge
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Locked terminal door - need to unlock via terminal command (2+N zones)
                    (0.50, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalDoor(start, 1);

                        // Add terminal zone after terminal unlock
                        AddUplinkTerminalZones(end, objective);
                    }),
                });
                break;
            }
            #endregion

            #region B-tier
            // B-Main: 3-4+N zones (straight), 4-5+N zones (keycard), 4+N zones (generator), 4+N zones (locked)
            // 4 layouts
            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight shot - longer path (3-4+N zones)
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(3, 4));
                        AddUplinkTerminalZones(nodes.Last(), objective);
                    }),

                    // Keycard in side branch (4-5+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator cell in side (4+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (end, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Locked terminal door (4+N zones)
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (end, _) = BuildChallenge_LockedTerminalDoor(nodes.Last(), 1);
                        AddUplinkTerminalZones(end, objective);
                    }),
                });
                break;
            }

            // B-Extreme: 2-3+N zones (forward), 3-4+N zones (keycard), 2+N zones (generator)
            // 3 layouts
            case ("B", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Simple forward path (2-3+N zones)
                    (0.40, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        AddUplinkTerminalZones(nodes.Last(), objective);
                    }),

                    // Keycard in side (3-4+N zones)
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator cell in zone (2+N zones)
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (end, _) = BuildChallenge_GeneratorCellInZone(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),
                });
                break;
            }

            // B-Overload: 4+N zones (generator), 4+N zones (locked+keycard), 4+N zones (keycard+generator), 0+N zones (first zone, N=1)
            // 4 layouts (3 always, 1 conditional)
            case ("B", Bulkhead.Overload):
            {
                var options = new List<(double, Action)>
                {
                    // Generator in side branch (4+N zones)
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (end, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Locked terminal door with keycard in side (4+N zones)
                    (0.30, () =>
                    {
                        var (mid, _) = BuildChallenge_KeycardInSide(start);
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid, 1);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Double puzzle - keycard then generator (4+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),
                };

                // Uplink terminal in first zone (only available for single terminal)
                // Terminal is in start zone, player must defend while doing uplink (0 zones)
                if (objective.Uplink_NumberOfTerminals == 1)
                    options.Add((0.15, () =>
                    {
                        objective.PlacementNodes.Add(start);
                    }));

                Generator.SelectRun(options);
                break;
            }
            #endregion

            #region C-tier
            // C-Main: 6-7+N zones (double keycard), 6-7+N zones (keycard+generator), 6+N zones (generator+terminal),
            //         5-6+N zones (long+keycard), 1-2+N zones (hub, N>=2)
            // 5 layouts (4 always, 1 conditional)
            case ("C", Bulkhead.Main):
            {
                var options = new List<(double, Action)>
                {
                    // Double keycard - keycard then another keycard (6-7+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (end, _) = BuildChallenge_KeycardInSide(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Keycard + generator (6-7+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInSide(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator + terminal unlock (6+N zones)
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid, 1);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Long path + keycard (5-6+N zones)
                    (0.15, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(3, 4));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),
                };

                // Hub style - terminals in end zones off central hub (2-3 terminals only) (1-2+N zones)
                if (objective.Uplink_NumberOfTerminals >= 2)
                    options.Add((0.15, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var hub = nodes.Last();
                        hub = planner.UpdateNode(hub with { MaxConnections = objective.Uplink_NumberOfTerminals + 1 });
                        planner.GetZone(hub)!.GenHubGeomorph(level.Complex);

                        // Add each terminal in its own end zone
                        for (var i = 0; i < objective.Uplink_NumberOfTerminals; i++)
                        {
                            var (end, _) = AddZone(hub);
                            objective.PlacementNodes.Add(end);
                        }
                    }));

                Generator.SelectRun(options);
                break;
            }

            // C-Extreme: 3-4+N zones (forward+keycard), 3+N zones (forward+generator), 4+N zones (keycard+generator), 2-3+N zones (long path)
            // 4 layouts
            case ("C", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward + keycard (3-4+N zones)
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Forward + generator (3+N zones)
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (end, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Double puzzle - keycard then generator (4+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Long path (2-3+N zones)
                    (0.15, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        AddUplinkTerminalZones(nodes.Last(), objective);
                    }),
                });
                break;
            }

            // C-Overload: 4+N zones (error+keycard), 6+N zones (generator+terminal), 4+N zones (double generator), 3-4+N zones (first zone, N=1)
            // 4 layouts (3 always, 1 conditional)
            case ("C", Bulkhead.Overload):
            {
                var options = new List<(double, Action)>
                {
                    // Error + keycard (4+N zones)
                    (0.35, () =>
                    {
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator + terminal unlock (6+N zones)
                    (0.35, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid, 1);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Double generator (4+N zones)
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),
                };

                // First zone uplink (single terminal only) (3-4 zones, terminal in start)
                if (objective.Uplink_NumberOfTerminals == 1)
                    options.Add((0.10, () =>
                    {
                        objective.PlacementNodes.Add(start);

                        // Add challenge zones after for resources
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                    }));

                Generator.SelectRun(options);
                break;
            }
            #endregion

            #region D-tier
            // D-Main: 7-9+N zones (error+keycard), 5-7+N zones (error+generator), 5-6+N zones (boss+keycard),
            //         8+N zones (triple puzzle), 4-5+N zones (long+boss)
            // 5 layouts
            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error + keycard (7-9+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            nodes.Last(),
                            errorZones: Generator.Between(2, 3),
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Error + generator carry (5-7+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            nodes.Last(),
                            errorZones: Generator.Between(2, 3),
                            terminalTurnoffZones: 1);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Boss + keycard (5-6+N zones)
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var (mid, _) = BuildChallenge_BossFight(nodes.Last());
                        var (end, _) = BuildChallenge_KeycardInSide(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Triple puzzle (8+N zones)
                    (0.15, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (mid1, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (mid2, _) = BuildChallenge_GeneratorCellInSide(mid1);
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid2, 1);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Long path + boss (4-5+N zones)
                    (0.15, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(3, 4));
                        var (end, _) = BuildChallenge_BossFight(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),
                });
                break;
            }

            // D-Extreme: 3-4+N zones (forward+boss), 3-4+N zones (keycard+boss), 3-4+N zones (generator+apex), 4-5+N zones (long+boss)
            // 4 layouts
            case ("D", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward + boss (3-4+N zones)
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var (end, _) = BuildChallenge_BossFight(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Keycard zone + boss (3-4+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (mid, _) = BuildChallenge_KeycardInZone(nodes.Last());
                        var (end, _) = BuildChallenge_BossFight(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator zone + apex (3-4+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (mid, _) = BuildChallenge_GeneratorCellInZone(nodes.Last());
                        var (end, _) = BuildChallenge_ApexAlarm(
                            mid,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Long path + boss (4-5+N zones)
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(3, 4));
                        var (end, _) = BuildChallenge_BossFight(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),
                });
                break;
            }

            // D-Overload: 3-4+N zones (forward+apex), 3-4+N zones (generator+apex), 3+N zones (boss+keycard), 5+N zones (error+keycard, N=1)
            // 4 layouts (3 always, 1 conditional)
            case ("D", Bulkhead.Overload):
            {
                var options = new List<(double, Action)>
                {
                    // Forward + apex (3-4+N zones)
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var (end, _) = BuildChallenge_ApexAlarm(
                            nodes.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator zone + apex (3-4+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (mid, _) = BuildChallenge_GeneratorCellInZone(nodes.Last());
                        var (end, _) = BuildChallenge_ApexAlarm(
                            mid,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Boss + keycard zone (3+N zones)
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_BossFight(nodes.Last());
                        var (end, _) = BuildChallenge_KeycardInZone(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),
                };

                // Error + keycard zone (single terminal only - larger layout) (5+N zones)
                if (objective.Uplink_NumberOfTerminals == 1)
                    options.Add((0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var (end, _) = BuildChallenge_KeycardInZone(mid);
                        AddUplinkTerminalZones(end, objective);
                    }));

                Generator.SelectRun(options);
                break;
            }
            #endregion

            #region E-tier
            // E-Main: 2+N zones (apex+keycard), 6-7+N zones (error+boss), 4-5+N zones (boss+forward+apex),
            //         6+N zones (error+generator), 2-3+N zones (hub, N>=3)
            // 5 layouts (4 always, 1 conditional)
            case ("E", Bulkhead.Main):
            {
                var options = new List<(double, Action)>
                {
                    // Apex + keycard - apex immediately (2+N zones)
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_ApexAlarm(
                            start,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var (end, _) = BuildChallenge_KeycardInZone(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Error + boss - error first (6-7+N zones)
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            errorZones: Generator.Between(2, 3),
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var (end, _) = BuildChallenge_BossFight(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Boss + apex - boss first, then forward, then apex (4-5+N zones)
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_BossFight(start);
                        var nodes = AddBranch_Forward(mid, Generator.Between(2, 3));
                        var (end, _) = BuildChallenge_ApexAlarm(
                            nodes.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Error + generator - error immediately (6+N zones)
                    (0.15, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            errorZones: 2,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),
                };

                // Hub multi-terminal (3+ terminals only) - apex first (2-3+N zones)
                if (objective.Uplink_NumberOfTerminals >= 3)
                    options.Add((0.15, () =>
                    {
                        var (mid, _) = BuildChallenge_ApexAlarm(
                            start,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var nodes = AddBranch_Forward(mid, Generator.Between(1, 2));
                        var hub = nodes.Last();
                        hub = planner.UpdateNode(hub with { MaxConnections = objective.Uplink_NumberOfTerminals + 1 });
                        planner.GetZone(hub)!.GenHubGeomorph(level.Complex);

                        for (var i = 0; i < objective.Uplink_NumberOfTerminals; i++)
                        {
                            var (end, _) = AddZone(hub);
                            objective.PlacementNodes.Add(end);
                        }
                    }));

                Generator.SelectRun(options);
                break;
            }

            // E-Extreme: 2+N zones (apex+boss), 3-4+N zones (boss+forward+apex), 3-4+N zones (double boss), 3+N zones (gen+apex+keycard, N=1)
            // 4 layouts (3 always, 1 conditional)
            case ("E", Bulkhead.Extreme):
            {
                var options = new List<(double, Action)>
                {
                    // Apex + boss - apex immediately (2+N zones)
                    (0.30, () =>
                    {
                        var (mid, _) = BuildChallenge_ApexAlarm(
                            start,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var (end, _) = BuildChallenge_BossFight(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Boss + apex - boss first, forward, then apex (3-4+N zones)
                    (0.30, () =>
                    {
                        var (mid, _) = BuildChallenge_BossFight(start);
                        var nodes = AddBranch_Forward(mid, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_ApexAlarm(
                            nodes.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Double boss - forward in middle (3-4+N zones)
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_BossFight(start);
                        var nodes = AddBranch_Forward(mid, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_BossFight(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),
                };

                // Generator + apex + keycard (single terminal only) (3+N zones)
                if (objective.Uplink_NumberOfTerminals == 1)
                    options.Add((0.15, () =>
                    {
                        var (mid1, _) = BuildChallenge_GeneratorCellInZone(start);
                        var (mid2, _) = BuildChallenge_ApexAlarm(
                            mid1,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var (end, _) = BuildChallenge_KeycardInZone(mid2);
                        AddUplinkTerminalZones(end, objective);
                    }));

                Generator.SelectRun(options);
                break;
            }

            // E-Overload: 1+N zones (brutal apex), 2+N zones (boss+apex), 3-4+N zones (generator+apex), 2+N zones (apex+boss, N=1)
            // 4 layouts (3 always, 1 conditional)
            case ("E", Bulkhead.Overload):
            {
                var options = new List<(double, Action)>
                {
                    // Brutal apex - start immediately (1+N zones)
                    (0.35, () =>
                    {
                        var (end, _) = BuildChallenge_ApexAlarm(
                            start,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Boss + apex - boss immediately (2+N zones)
                    (0.30, () =>
                    {
                        var (mid, _) = BuildChallenge_BossFight(start);
                        var (end, _) = BuildChallenge_ApexAlarm(
                            mid,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator + apex - forward then challenges (3-4+N zones)
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (mid, _) = BuildChallenge_GeneratorCellInZone(nodes.Last());
                        var (end, _) = BuildChallenge_ApexAlarm(
                            mid,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        AddUplinkTerminalZones(end, objective);
                    }),
                };

                // Apex + boss (single terminal only) (2+N zones)
                if (objective.Uplink_NumberOfTerminals == 1)
                    options.Add((0.15, () =>
                    {
                        var (mid, _) = BuildChallenge_ApexAlarm(
                            start,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var (end, _) = BuildChallenge_BossFight(mid);
                        AddUplinkTerminalZones(end, objective);
                    }));

                Generator.SelectRun(options);
                break;
            }
            #endregion

            // Default: 2-4+N zones (N=1), 2-3+N zones (N=2), 1-2+N zones (N=3,4)
            // Fallback for unhandled tier/bulkhead combinations
            default:
            {
                // Fallback layout - simple branch with terminal zones
                // Uses terminal count to determine how many terminal zones to add
                var zoneCount = objective.Uplink_NumberOfTerminals switch
                {
                    1 => Generator.Between(2, 4),  // 3-5 total zones
                    2 => Generator.Between(2, 3),  // 4-5 total zones
                    3 => Generator.Between(1, 2),  // 4-5 total zones
                    4 => Generator.Between(1, 2),  // 5-6 total zones
                    _ => Generator.Between(2, 4)
                };

                var nodes = AddBranch_Forward(start, zoneCount);
                var last = nodes.Last();

                // Add terminal zones at the end
                AddUplinkTerminalZones(last, objective);
                break;
            }
        }
    }
}
