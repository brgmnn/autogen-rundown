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
            case ("A", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight shot - simple forward path to terminal
                    (0.40, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var last = nodes.Last();

                        // Add terminal zone at end
                        AddUplinkTerminalZones(last, objective);
                    }),

                    // Keycard in side branch
                    (0.35, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());

                        // Add terminal zone after keycard challenge
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator cell challenge
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

            case ("A", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Simple path - short and easy
                    (0.50, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var last = nodes.Last();

                        // Add terminal zone at end
                        AddUplinkTerminalZones(last, objective);
                    }),

                    // Keycard in same zone
                    (0.50, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInZone(start);

                        // Add terminal zone after keycard
                        AddUplinkTerminalZones(end, objective);
                    }),
                });
                break;
            }

            case ("A", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Generator cell - carry cell through
                    (0.50, () =>
                    {
                        var (end, _) = BuildChallenge_GeneratorCellInSide(start, 1);

                        // Add terminal zone after generator challenge
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Locked terminal door - need to unlock via terminal command
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
            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight shot - longer path
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(3, 4));
                        AddUplinkTerminalZones(nodes.Last(), objective);
                    }),

                    // Keycard in side branch
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator cell in side
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (end, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Locked terminal door
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (end, _) = BuildChallenge_LockedTerminalDoor(nodes.Last(), 1);
                        AddUplinkTerminalZones(end, objective);
                    }),
                });
                break;
            }

            case ("B", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Simple forward path
                    (0.40, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        AddUplinkTerminalZones(nodes.Last(), objective);
                    }),

                    // Keycard in side
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator cell in zone
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (end, _) = BuildChallenge_GeneratorCellInZone(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),
                });
                break;
            }

            case ("B", Bulkhead.Overload):
            {
                var options = new List<(double, Action)>
                {
                    // Generator in side branch
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (end, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Locked terminal door with keycard in side
                    (0.30, () =>
                    {
                        var (mid, _) = BuildChallenge_KeycardInSide(start);
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid, 1);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Double puzzle - keycard then generator
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),
                };

                // Uplink terminal in first zone (only available for single terminal)
                // Terminal is in start zone, player must defend while doing uplink
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
            case ("C", Bulkhead.Main):
            {
                var options = new List<(double, Action)>
                {
                    // Double keycard - keycard then another keycard
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (end, _) = BuildChallenge_KeycardInSide(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Keycard + generator
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInSide(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator + terminal unlock
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid, 1);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Long path + keycard
                    (0.15, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(3, 4));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),
                };

                // Hub style - terminals in end zones off central hub (2-3 terminals only)
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

            case ("C", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Forward + keycard
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Forward + generator
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (end, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Double puzzle - keycard then generator
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Long path
                    (0.15, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        AddUplinkTerminalZones(nodes.Last(), objective);
                    }),
                });
                break;
            }

            case ("C", Bulkhead.Overload):
            {
                var options = new List<(double, Action)>
                {
                    // Error + keycard
                    (0.35, () =>
                    {
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Generator + terminal unlock
                    (0.35, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid, 1);
                        AddUplinkTerminalZones(end, objective);
                    }),

                    // Double generator
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        AddUplinkTerminalZones(end, objective);
                    }),
                };

                // First zone uplink (single terminal only)
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
            // TODO: Implement D-tier variants
            // D-tier should have error alarms, terminal puzzles
            // Main: 5-9 zones, error alarms, boss fights possible
            // Extreme: 4-6 zones, apex alarms
            // Overload: 4-5 zones, error + boss
            case ("D", Bulkhead.Main):
            case ("D", Bulkhead.Extreme):
            case ("D", Bulkhead.Overload):
                goto default;
            #endregion

            #region E-tier
            // TODO: Implement E-tier variants
            // E-tier should have apex alarms, boss fights, complex puzzles
            // Main: 6-12 zones, mega mom, double apex, multi-terminal setups
            // Extreme: 5-8 zones, apex + boss
            // Overload: 4-6 zones, mega mom or brutal apex
            case ("E", Bulkhead.Main):
            case ("E", Bulkhead.Extreme):
            case ("E", Bulkhead.Overload):
                goto default;
            #endregion

            default:
            {
                // Fallback layout - simple branch with terminal zones
                // Uses terminal count to determine how many terminal zones to add
                var zoneCount = objective.Uplink_NumberOfTerminals switch
                {
                    1 => Generator.Between(2, 4),
                    2 => Generator.Between(2, 3),
                    3 => Generator.Between(1, 2),
                    4 => Generator.Between(1, 2),
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
