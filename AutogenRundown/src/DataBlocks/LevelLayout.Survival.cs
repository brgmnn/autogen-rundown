﻿using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Terminals;
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
    /// <param name="objective"></param>
    /// <param name="start"></param>
    public void BuildLayout_Survival(
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
        var arenaNodes = new List<ZoneNode>();

        switch (level.Tier)
        {
            #region Tier: E
            case "E":
            {
                // Boss error with some puzzles in the middle
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.20, () =>
                    {
                        var (mid1, mid1Zone) = AddZone(start, new ZoneNode { Branch = "survival_arena" });

                        var population = Generator.Flip()
                            ? WavePopulation.SingleEnemy_Tank
                            : WavePopulation.SingleEnemy_Mother;

                        AddApexErrorAlarm(mid1, null, WaveSettings.Error_Boss_VeryHard, population);

                        arenaNodes.Add(mid1);

                        // Options for the mid puzzles
                        Generator.SelectRun(new List<(double, Action)>
                        {
                            // 1 generator
                            (0.50, () =>
                            {
                                var nodes = AddBranch_Forward(
                                    mid1,
                                    Generator.Between(2, 3),
                                    "survival_arena",
                                    (_, zone) =>
                                    {
                                        // Add extra resources to survival arena zones
                                        zone.AmmoPacks += 3;
                                        zone.ToolPacks += 2;
                                        zone.HealthPacks += 3;
                                    });

                                var (mid2, _) = BuildChallenge_GeneratorCellInZone(nodes.Last());

                                arenaNodes.AddRange(nodes);
                                arenaNodes.Add(mid2);
                            }),

                            // 1 key
                            (0.50, () =>
                            {
                                var nodes = AddBranch_Forward(
                                    mid1,
                                    Generator.Between(2, 3),
                                    "survival_arena",
                                    (_, zone) =>
                                    {
                                        // Add extra resources to survival arena zones
                                        zone.AmmoPacks += 4;
                                        zone.ToolPacks += 2;
                                        zone.HealthPacks += 3;
                                    });

                                var (mid2, _) = BuildChallenge_KeycardInZone(nodes.Last());

                                arenaNodes.AddRange(nodes);
                                arenaNodes.Add(mid2);
                            }),
                        });
                    })
                });
                break;
            }
            #endregion

            default:
            {
                arenaNodes = AddBranch(
                    start,
                    director.ZoneCount,
                    "survival_arena",
                    (node, zone) =>
                    {
                        zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;

                        // Add extra resources to survival arena zones
                        zone.AmmoPacks += 3;
                        zone.ToolPacks += 2;
                        zone.HealthPacks += 3;
                    }).ToList();
                break;
            }
        }

        var first = arenaNodes.First();
        var last = arenaNodes.Last();

        #region Exit Zone
        // We need an exit zone as prisoners have to run to the exit
        var exitIndex = level.Planner.NextIndex(director.Bulkhead);
        var exitNode = new ZoneNode()
        {
            Bulkhead = director.Bulkhead,
            ZoneNumber = exitIndex,
            Branch = "exit",
            Tags = new Tags("exit_elevator"),
            MaxConnections = 0
        };
        var exitZone = new Zone(level)
        {
            Coverage = CoverageMinMax.Tiny,
            LightSettings = Lights.GenRandomLight()
        };

        exitZone.GenExitGeomorph(director.Complex);

        // Exit scan will be HARD
        exitZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;
        exitZone.Alarm = ChainedPuzzle.SkipZone;

        level.Planner.Connect(last, exitNode);
        level.Planner.AddZone(exitNode, exitZone);
        #endregion

        #region Alarm Skip -- Security Disable Side Zone
        // Adds an optional side zone where players can force open every single door between them
        // and the extract. This in theory is nice because they skip every scan, but it will be
        // hard as now the error alarm can trigger every single sleeping enemy between them and
        // the extract. Also any blood doors are force opened which _WILL_ aggro the entire main
        // zone as they walk to the players.
        //
        // High risk, high reward?
        // TODO: don't make this always spawn
        if (true)
        {
            var securityControlNode = new ZoneNode(
                director.Bulkhead,
                level.Planner.NextIndex(director.Bulkhead),
                "arena_security_control");
            securityControlNode.Tags.Add("no_enemies");

            var securityControlZone = new Zone(level)
            {
                Coverage = CoverageMinMax.Nano,
                LightSettings = Lights.GenRandomLight(),
                AliasPrefix = "Security, ZONE"
            };
            securityControlZone.RollFog(level);

            level.Planner.Connect(first, securityControlNode);
            level.Planner.AddZone(securityControlNode, securityControlZone);

            securityControlZone.EventsOnApproachDoor.AddMessage(
                "SECURITY CONTROL ZONE");

            objective.SecurityControlEventLoopIndex = (int)Generator.GetPersistentId();
            var lightsEventLoop = new EventLoop()
            {
                LoopIndex = objective.SecurityControlEventLoopIndex,
                LoopDelay = 2.0,
                LoopCount = -1
            };

            var securityControlEvents = new List<WardenObjectiveEvent>();

            // Add events to open all the doors in the arena index
            foreach (var node in arenaNodes)
            {
                securityControlEvents.AddOpenDoor(
                    director.Bulkhead,
                    node.ZoneNumber,
                    null,
                    WardenObjectiveEventTrigger.None);

                lightsEventLoop.EventsToActivate.Add(
                    new WardenObjectiveEvent
                    {
                        Type = WardenObjectiveEventType.SetLightDataInZone,
                        LocalIndex = node.ZoneNumber,
                        Layer = 0,
                        Delay = 0.0,
                        Duration = 0.1,
                        SetZoneLight = new SetZoneLight
                        {
                            LightSettings = Light.LightSettings.LightsOff,
                            Duration = 0.1,
                            Seed = 1,
                        }
                    });
                lightsEventLoop.EventsToActivate.Add(
                    new WardenObjectiveEvent
                    {
                        Type = WardenObjectiveEventType.SetLightDataInZone,
                        LocalIndex = node.ZoneNumber,
                        Layer = 0,
                        Delay = 1.0,
                        Duration = 0.1,
                        SetZoneLight = new SetZoneLight
                        {
                            LightSettings = Light.LightSettings.ErrorFlashOn,
                            Duration = 0.1,
                            Seed = 2,
                        }
                    });
            }

            // Also open exit node
            securityControlEvents.AddOpenDoor(
                director.Bulkhead,
                exitNode.ZoneNumber,
                null,
                WardenObjectiveEventTrigger.None);

            // Add lights event loops
            securityControlEvents.Add(
                new WardenObjectiveEvent()
                {
                    Type = WardenObjectiveEventType.StartEventLoop,
                    EventLoop = lightsEventLoop
                });

            // Ramp up the difficulty of the survival waves
            // They will now get hybrids
            securityControlEvents.AddGenericWave(level.Tier switch
            {
                "E" => new GenericWave
                {
                    SpawnDelay = 15.0,
                    Settings = WaveSettings.Error_Specials_VeryHard,
                    Population = WavePopulation.OnlyHybrids
                },

                _ => new GenericWave
                {
                    SpawnDelay = 20.0,
                    Settings = WaveSettings.Error_Specials_Hard,
                    Population = WavePopulation.OnlyHybrids
                }
            });
            securityControlEvents.AddMessage(Lore.UnknownError_Any(), 12.0);

            objective.SecurityControlEvents = securityControlEvents;

            securityControlZone.TerminalPlacements.Clear();
            securityControlZone.TerminalPlacements.Add(new TerminalPlacement
            {
                PlacementWeights = ZonePlacementWeights.AtEnd,
                UniqueCommands = new List<CustomTerminalCommand>
                {
                    new()
                    {
                        Command = "OVERRIDE_LOCKDOWN_PROTOCOL",
                        CommandDesc = "Manual override for security systems on floor, force open all security doors to exit",
                        CommandEvents = objective.SecurityControlEvents
                    }
                }
            });
        }
        #endregion
    }
}
