using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Terminals;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="start"></param>
    private void BuildLayout_GatherTerminal_Fast(ZoneNode start)
    {
        var startZone = planner.GetZone(start)!;
        startZone.Coverage = CoverageMinMax.Large_100;
        startZone.TerminalPlacements.First().PlacementWeights = ZonePlacementWeights.AtStart;

        for (var t = 0; t < objective.GatherTerminal_SpawnCount; t++)
        {
            startZone.TerminalPlacements.Add(new TerminalPlacement
            {
                PlacementWeights = ZonePlacementWeights.AtEnd
            });

            SetGatherTerminal(start.ZoneNumber, ZonePlacementWeights.NotAtStart);
            objective.PlacementNodes.Add(start);
        }

        AddGatherTerminalInfoLog(startZone);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    /// <exception cref="Exception"></exception>
    private void BuildLayout_GatherTerminal(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;

        // --- Fast version ---
        if (level.MainDirector.Objective is WardenObjectiveType.ReachKdsDeep or WardenObjectiveType.Cryptomnesia)
        {
            BuildLayout_GatherTerminal_Fast(start);

            return;
        }

        // --- Normal version ---
        // Main-only levels build directly from the elevator drop zone (no starting
        // area zones). Add an approach so no objective terminal lands in the elevator
        // zone and the main path gets parity with bulkhead levels.
        if (director.Bulkhead == Bulkhead.Main && level.Settings.Bulkheads == Bulkhead.Main)
            start = AddBranch_Forward(start, Generator.Between(1, 2), "approach").Last();

        switch (level.Tier, director.Bulkhead)
        {
            // These all have 3 spawn count
            case ("B", Bulkhead.Main):
            case ("E", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight line
                    (0.25, () =>
                    {
                        SetGatherTerminal(start.ZoneNumber);
                        objective.PlacementNodes.Add(start);

                        var nodes = AddBranch_Forward(start, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            SetGatherTerminal(node.ZoneNumber);
                            objective.PlacementNodes.Add(node);
                        });

                        AddForwardExtractStart(nodes.Last());
                    }),

                    // start -> Hub -> end1,end2
                    (0.75, () =>
                    {
                        start = level.GenCorridorGeomorph(start);

                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });

                        AddForwardExtractStart(hub);

                        if (Generator.Flip(0.4))
                            hub = level.GenTGeomorph(hub);
                        else
                            hub = level.GenHubGeomorph(hub);

                        SetGatherTerminal(hub.ZoneNumber);
                        objective.PlacementNodes.Add(hub);

                        var (end1, end1Zone) = AddZone(hub);
                        end1 = level.GenDeadEndGeomorph(end1);

                        var (end2, end2Zone) = AddZone(hub);
                        end2 = level.GenDeadEndGeomorph(end2);

                        SetGatherTerminal(end1.ZoneNumber);
                        SetGatherTerminal(end2.ZoneNumber);
                        objective.PlacementNodes.Add(end1);
                        objective.PlacementNodes.Add(end2);
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);

                        SetGatherTerminal(travelEnd.ZoneNumber);
                        objective.PlacementNodes.Add(travelEnd);

                        var nodes = AddBranch_Forward(travelEnd, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            SetGatherTerminal(node.ZoneNumber);
                            objective.PlacementNodes.Add(node);
                        });

                        AddForwardExtractStart(nodes.Last());
                    }),
                });
                break;
            }

            // D-Overload: 3 spawn count with sensor variants
            case ("D", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight line
                    (0.15, () =>
                    {
                        SetGatherTerminal(start.ZoneNumber);
                        objective.PlacementNodes.Add(start);

                        var nodes = AddBranch_Forward(start, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            SetGatherTerminal(node.ZoneNumber);
                            objective.PlacementNodes.Add(node);
                        });

                        AddForwardExtractStart(nodes.Last());
                    }),

                    // start -> Hub -> end1,end2
                    (0.30, () =>
                    {
                        start = level.GenCorridorGeomorph(start);

                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });

                        AddForwardExtractStart(hub);

                        if (Generator.Flip(0.4))
                            hub = level.GenTGeomorph(hub);
                        else
                            hub = level.GenHubGeomorph(hub);

                        SetGatherTerminal(hub.ZoneNumber);
                        objective.PlacementNodes.Add(hub);

                        var (end1, end1Zone) = AddZone(hub);
                        end1 = level.GenDeadEndGeomorph(end1);

                        var (end2, end2Zone) = AddZone(hub);
                        end2 = level.GenDeadEndGeomorph(end2);

                        SetGatherTerminal(end1.ZoneNumber);
                        SetGatherTerminal(end2.ZoneNumber);
                        objective.PlacementNodes.Add(end1);
                        objective.PlacementNodes.Add(end2);
                    }),

                    // Sensor fortress hub - sensors on all zones
                    (0.35, () =>
                    {
                        start = level.GenCorridorGeomorph(start);
                        AddSecuritySensors(start);

                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });

                        AddForwardExtractStart(hub);

                        if (Generator.Flip(0.4))
                            hub = level.GenTGeomorph(hub);
                        else
                            hub = level.GenHubGeomorph(hub);

                        AddSecuritySensors(hub);
                        SetGatherTerminal(hub.ZoneNumber);
                        objective.PlacementNodes.Add(hub);

                        var (end1, end1Zone) = AddZone(hub);
                        end1 = level.GenDeadEndGeomorph(end1);
                        AddSecuritySensors(end1);

                        var (end2, end2Zone) = AddZone(hub);
                        end2 = level.GenDeadEndGeomorph(end2);
                        AddSecuritySensors(end2);

                        SetGatherTerminal(end1.ZoneNumber);
                        SetGatherTerminal(end2.ZoneNumber);
                        objective.PlacementNodes.Add(end1);
                        objective.PlacementNodes.Add(end2);
                    }),

                    // Sensor line gauntlet - linear branch with sensors on all zones
                    (0.20, () =>
                    {
                        AddSecuritySensors(start);
                        SetGatherTerminal(start.ZoneNumber);
                        objective.PlacementNodes.Add(start);

                        var nodes = AddBranch_Forward(start, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            AddSecuritySensors(node);
                            SetGatherTerminal(node.ZoneNumber);
                            objective.PlacementNodes.Add(node);
                        });

                        AddForwardExtractStart(nodes.Last());
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);

                        SetGatherTerminal(travelEnd.ZoneNumber);
                        objective.PlacementNodes.Add(travelEnd);

                        var nodes = AddBranch_Forward(travelEnd, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            SetGatherTerminal(node.ZoneNumber);
                            objective.PlacementNodes.Add(node);
                        });

                        AddForwardExtractStart(nodes.Last());
                    }),
                });
                break;
            }

            // These all have 4 spawn count
            case ("C", Bulkhead.Main):
            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // start -> Hub -> arm1 (2 terminals at end), arm2 (1 terminal at end)
                    (1.0, () =>
                    {
                        start = level.GenCorridorGeomorph(start);

                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hub = level.GenHubGeomorph(hub);

                        SetGatherTerminalGroup(hub, 1);

                        var arm1 = AddBranch(
                            hub,
                            level.Tier == "D" ? 2 : Generator.Between(1, 2),
                            "find_terminal_1");
                        var end1 = level.GenDeadEndGeomorph(arm1.Last());

                        SetGatherTerminalGroup(end1, 2);

                        var arm2 = AddBranch(
                            hub,
                            level.Tier == "D" ? 2 : Generator.Between(1, 2),
                            "find_terminal_2");
                        var end2 = arm2.Last();

                        SetGatherTerminalGroup(end2, 1);
                        AddForwardExtractStart(end2);
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);

                        SetGatherTerminal(travelEnd.ZoneNumber);
                        objective.PlacementNodes.Add(travelEnd);

                        var nodes = AddBranch_Forward(travelEnd, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            SetGatherTerminal(node.ZoneNumber);
                            objective.PlacementNodes.Add(node);
                        });

                        AddForwardExtractStart(nodes.Last());
                    }),
                });
                break;
            }

            // E-Overload: 4 spawn count with sensor variants
            case ("E", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // start -> Hub -> end1,end2,end3
                    (0.35, () =>
                    {
                        start = level.GenCorridorGeomorph(start);

                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hub = level.GenHubGeomorph(hub);

                        SetGatherTerminal(hub.ZoneNumber);
                        objective.PlacementNodes.Add(hub);

                        var (end1, end1Zone) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_1" });
                        end1 = level.GenDeadEndGeomorph(end1);

                        var (end2, end2Zone) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_2" });
                        end2 = level.GenDeadEndGeomorph(end2);

                        var (end3, _) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_3" });
                        AddForwardExtractStart(end3);

                        SetGatherTerminal(end1.ZoneNumber);
                        SetGatherTerminal(end2.ZoneNumber);
                        SetGatherTerminal(end3.ZoneNumber);

                        objective.PlacementNodes.Add(end1);
                        objective.PlacementNodes.Add(end2);
                        objective.PlacementNodes.Add(end3);
                    }),

                    // Sensor fortress hub - sensors on all zones (4 spawn)
                    (0.40, () =>
                    {
                        start = level.GenCorridorGeomorph(start);
                        AddSecuritySensors(start);

                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hub = level.GenHubGeomorph(hub);
                        AddSecuritySensors(hub);

                        SetGatherTerminal(hub.ZoneNumber);
                        objective.PlacementNodes.Add(hub);

                        var (end1, end1Zone) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_1" });
                        end1 = level.GenDeadEndGeomorph(end1);
                        AddSecuritySensors(end1);

                        var (end2, end2Zone) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_2" });
                        end2 = level.GenDeadEndGeomorph(end2);
                        AddSecuritySensors(end2);

                        var (end3, _) =
                            AddZone(hub, new ZoneNode { MaxConnections = 0, Branch = "find_terminal_3" });
                        AddForwardExtractStart(end3);
                        AddSecuritySensors(end3);

                        SetGatherTerminal(end1.ZoneNumber);
                        SetGatherTerminal(end2.ZoneNumber);
                        SetGatherTerminal(end3.ZoneNumber);

                        objective.PlacementNodes.Add(end1);
                        objective.PlacementNodes.Add(end2);
                        objective.PlacementNodes.Add(end3);
                    }),

                    // Sensor line gauntlet - linear 4-zone branch, all sensored
                    (0.25, () =>
                    {
                        AddSecuritySensors(start);
                        SetGatherTerminal(start.ZoneNumber);
                        objective.PlacementNodes.Add(start);

                        var nodes = AddBranch_Forward(start, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            AddSecuritySensors(node);
                            SetGatherTerminal(node.ZoneNumber);
                            objective.PlacementNodes.Add(node);
                        });

                        AddForwardExtractStart(nodes.Last());
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);

                        SetGatherTerminal(travelEnd.ZoneNumber);
                        objective.PlacementNodes.Add(travelEnd);

                        var nodes = AddBranch_Forward(travelEnd, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            SetGatherTerminal(node.ZoneNumber);
                            objective.PlacementNodes.Add(node);
                        });

                        AddForwardExtractStart(nodes.Last());
                    }),
                });
                break;
            }

            // Spawns 6
            case ("E", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Twin hubs — start(corridor) -> hub1 (2 terminals) + dead end (1 terminal)
                    // -> link(corridor) -> hub2 (2 terminals) + dead end (1 terminal)
                    (0.35, () =>
                    {
                        start = level.GenCorridorGeomorph(start);

                        var (hub1, hub1Zone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hub1 = level.GenHubGeomorph(hub1);

                        SetGatherTerminalGroup(hub1, 2);

                        var (end1, end1Zone) = AddZone(hub1, new ZoneNode { Branch = "find_terminal_1" });
                        end1 = level.GenDeadEndGeomorph(end1);

                        SetGatherTerminalGroup(end1, 1);

                        var (link, linkZone) = AddZone(hub1, new ZoneNode { Branch = "hub_link" });
                        link = level.GenCorridorGeomorph(link);

                        var (hub2, hub2Zone) = AddZone(link, new ZoneNode { MaxConnections = 3 });
                        hub2 = level.GenHubGeomorph(hub2);

                        SetGatherTerminalGroup(hub2, 2);
                        AddForwardExtractStart(hub2);

                        var (end2, end2Zone) = AddZone(hub2, new ZoneNode { Branch = "find_terminal_2" });
                        end2 = level.GenDeadEndGeomorph(end2);

                        SetGatherTerminalGroup(end2, 1);
                    }),

                    // Locked route — start -> 2 zones -> 3 terminals, locked terminal
                    // door -> 3 terminals
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 2, "primary");
                        var stopA = nodes.Last();

                        SetGatherTerminalGroup(stopA, 3);

                        var (stopB, stopBZone) = BuildChallenge_LockedTerminalDoor(stopA, 1);

                        SetGatherTerminalGroup(stopB, 3);
                        AddForwardExtractStart(stopB);
                    }),

                    // Deep arm hub — start(corridor) -> hub -> 3 arms x 2 zones,
                    // 2 terminals at each arm end
                    (0.25, () =>
                    {
                        start = level.GenCorridorGeomorph(start);

                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hub = level.GenHubGeomorph(hub);

                        for (var t = 0; t < 3; t++)
                        {
                            var arm = AddBranch(hub, 2, $"find_terminal_{t + 1}");
                            var end = arm.Last();

                            if (t < 2)
                                end = level.GenDeadEndGeomorph(end);
                            else
                                AddForwardExtractStart(end);

                            SetGatherTerminalGroup(end, 2);
                        }
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);

                        SetGatherTerminalGroup(travelEnd, 2);

                        var nodes = AddBranch_Forward(travelEnd, 3, "primary");

                        foreach (var node in nodes.Skip(1))
                            SetGatherTerminalGroup(node, 2);

                        AddForwardExtractStart(nodes.Last());
                    }),
                });
                break;
            }

            // Most of the smaller levels will use this default linear branch
            default:
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Standard linear branch
                    (0.85, () =>
                    {
                        SetGatherTerminal(start.ZoneNumber);
                        objective.PlacementNodes.Add(start);

                        var nodes = AddBranch_Forward(start, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            SetGatherTerminal(node.ZoneNumber);
                            objective.PlacementNodes.Add(node);
                        });

                        AddForwardExtractStart(nodes.Last());
                    }),

                    // Travel scan prelude
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);

                        SetGatherTerminal(travelEnd.ZoneNumber);
                        objective.PlacementNodes.Add(travelEnd);

                        var nodes = AddBranch_Forward(travelEnd, objective.GatherTerminal_SpawnCount - 1, "primary", (node, zone) =>
                        {
                            SetGatherTerminal(node.ZoneNumber);
                            objective.PlacementNodes.Add(node);
                        });

                        AddForwardExtractStart(nodes.Last());
                    }),
                });
                break;
            }
        }

        AddGatherTerminalInfoLog(startZone);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="zone"></param>
    private void AddGatherTerminalInfoLog(Zone zone)
    {
        zone.TerminalPlacements.First().LogFiles.Add(new LogFile
        {
            FileName = $"DEC_KEY_INVENTORY-{Generator.ShortHexHash()}",
            FileContent = new Text(() =>
            {
                var zones = string.Join(
                    ",\n  ",
                    objective.PlacementNodes
                        .Select(node => Intel.Zone(node, planner, underscore: true))
                        .Chunk(4)
                        .Select(group => string.Join(", ", group)));

                return $"-------------------------------------------\n" +
                       $"          Data redundancy system          \n\n" +
                       $"Backup decryption keys stored in mirror\n" +
                       $"terminal array. Terminal storage zones:\n\n" +
                       $"  {zones}\n\n" +
                       $"-------------------------------------------";
            })
        });
    }

    /// <summary>
    /// Places `count` gather terminals grouped in a single zone, adding extra terminal
    /// placements so each gather item gets its own terminal.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="count"></param>
    private void SetGatherTerminalGroup(ZoneNode node, int count)
    {
        var zone = planner.GetZone(node)!;

        for (var t = 0; t < count; t++)
        {
            if (t > 0)
                zone.TerminalPlacements.Add(new TerminalPlacement());

            SetGatherTerminal(node.ZoneNumber);
        }

        objective.PlacementNodes.Add(node);
    }

    /// <summary>
    /// Helper function to wrap adding the zone placement data
    /// </summary>
    /// <param name="zoneNumber"></param>
    /// <param name="distribution"></param>
    private void SetGatherTerminal(
        int zoneNumber,
        ZonePlacementWeights? distribution = null,
        DimensionIndex dimension = DimensionIndex.Reality)
    {
        var dataLayer = level.GetObjectiveLayerData(director.Bulkhead);

        dataLayer.ObjectiveData.ZonePlacementDatas.Add(new List<ZonePlacementData>
        {
            new()
            {
                Dimension = dimension,
                LocalIndex = zoneNumber,
                Weights = distribution ?? ZonePlacementWeights.EvenlyDistributed
            }
        });
    }
}
