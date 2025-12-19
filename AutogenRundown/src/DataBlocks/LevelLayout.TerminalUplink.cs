using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
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
                        AddBranch(last, 1, "uplink_terminals");
                    }),

                    // Keycard in side branch
                    (0.35, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());

                        // Add terminal zone after keycard challenge
                        AddBranch(end, 1, "uplink_terminals");
                    }),

                    // Generator cell challenge
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (end, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());

                        // Add terminal zone after generator challenge
                        AddBranch(end, 1, "uplink_terminals");
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
                        AddBranch(last, 1, "uplink_terminals");
                    }),

                    // Keycard in same zone
                    (0.50, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInZone(start);

                        // Add terminal zone after keycard
                        AddBranch(end, 1, "uplink_terminals");
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
                        AddBranch(end, 1, "uplink_terminals");
                    }),

                    // Locked terminal door - need to unlock via terminal command
                    (0.50, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalDoor(start, 1);

                        // Add terminal zone after terminal unlock
                        AddBranch(end, 1, "uplink_terminals");
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
                        AddBranch(nodes.Last(), objective.Uplink_NumberOfTerminals, "uplink_terminals");
                    }),

                    // Keycard in side branch
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        AddBranch(end, objective.Uplink_NumberOfTerminals, "uplink_terminals");
                    }),

                    // Generator cell in side
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (end, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        AddBranch(end, objective.Uplink_NumberOfTerminals, "uplink_terminals");
                    }),

                    // Locked terminal door
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (end, _) = BuildChallenge_LockedTerminalDoor(nodes.Last(), 1);
                        AddBranch(end, objective.Uplink_NumberOfTerminals, "uplink_terminals");
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
                        AddBranch(nodes.Last(), objective.Uplink_NumberOfTerminals, "uplink_terminals");
                    }),

                    // Keycard in side
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        AddBranch(end, objective.Uplink_NumberOfTerminals, "uplink_terminals");
                    }),

                    // Generator cell in zone
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (end, _) = BuildChallenge_GeneratorCellInZone(nodes.Last());
                        AddBranch(end, objective.Uplink_NumberOfTerminals, "uplink_terminals");
                    }),
                });
                break;
            }

            case ("B", Bulkhead.Overload):
            {
                // Conditional weight: only allow first-zone uplink if single terminal
                var firstZoneWeight = objective.Uplink_NumberOfTerminals == 1 ? 0.15 : 0.0;

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Generator in side branch
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2);
                        var (end, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                        AddBranch(end, objective.Uplink_NumberOfTerminals, "uplink_terminals");
                    }),

                    // Locked terminal door with keycard in side
                    (0.30, () =>
                    {
                        var (mid, _) = BuildChallenge_KeycardInSide(start);
                        var (end, _) = BuildChallenge_LockedTerminalDoor(mid, 1);
                        AddBranch(end, objective.Uplink_NumberOfTerminals, "uplink_terminals");
                    }),

                    // Double puzzle - keycard then generator
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
                        AddBranch(end, objective.Uplink_NumberOfTerminals, "uplink_terminals");
                    }),

                    // Uplink terminal in first zone (only available for single terminal)
                    // Terminal is in start zone, player must defend while doing uplink
                    (firstZoneWeight, () =>
                    {
                        // Place terminal in start zone
                        AddBranch(start, 1, "uplink_terminals");

                        // Add some challenge zones after for resource gathering
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                    }),
                });
                break;
            }
            #endregion

            #region C-tier
            // TODO: Implement C-tier variants
            // C-tier should have generator puzzles, class alarms
            // Main: 4-7 zones, class II-IV alarms, multiple keycards
            // Extreme: 3-5 zones, class I-II alarms
            // Overload: 3-5 zones, error alarms with turn-off
            case ("C", Bulkhead.Main):
            case ("C", Bulkhead.Extreme):
            case ("C", Bulkhead.Overload):
                goto default;
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
                AddBranch(last, objective.Uplink_NumberOfTerminals, "uplink_terminals");
                break;
            }
        }
    }
}
