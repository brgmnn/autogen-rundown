using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Light;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Terminals;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;
using AutogenRundown.Utils;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    #region Private: Zone Mood

    /// <summary>
    ///
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="eggSacks"></param>
    private static void SetMotherVibe(Zone zone, int eggSacks = 200)
    {
        // Pick some mother like lights
        zone.LightSettings = Generator.Pick(new List<Lights.Light>
        {
            Lights.Light.Monochrome_Red,
            Lights.Light.RedToYellow_1,
            Lights.Light.OrangeToBrown_1,
            Lights.Light.OrangeToYellow_1,
            Lights.Light.YellowToOrange_1,
            Lights.Light.Monochrome_Orange_Flickering,
            Lights.Light.WashedOutRed_1
        });

        // Add mother egg sacks to the zone
        zone.StaticSpawnDataContainers.Add(
            new StaticSpawnDataContainer
            {
                Count = eggSacks,
                DistributionWeightType = 0,
                DistributionWeight = 1.0,
                DistributionRandomBlend = 0.0,
                DistributionResultPow = 2.0,
                Unit = StaticSpawnUnit.EggSack,
                FixedSeed = 121
            });
    }

    private static void SetInfectionVibe(Zone zone, int spitters = 100)
    {
        // Pick some mother like lights
        zone.LightSettings = Generator.Pick(new List<Lights.Light>
        {
            Lights.Light.Monochrome_Green,
            Lights.Light.Monochrome_YellowToGreen,
            Lights.Light.DarkGreenToRed_1,
            Lights.Light.camo_green_R4E1,
            Lights.Light.BlueToGreen_1
        });

        // Add mother egg sacks to the zone
        zone.StaticSpawnDataContainers.Add(
            new StaticSpawnDataContainer
            {
                Count = spitters,
                DistributionWeightType = 0,
                DistributionWeight = 1.0,
                DistributionRandomBlend = 0.0,
                DistributionResultPow = 2.0,
                Unit = StaticSpawnUnit.Spitter,
                FixedSeed = Generator.Between(10, 150)
            });
    }

    private static void SetInfestedVibe(Zone zone)
    {
        zone.LightSettings = Generator.Pick(new List<Lights.Light>
        {
            Lights.Light.OrangeToBrown_1,
            Lights.Light.OrangeToYellow_1,
            Lights.Light.YellowToOrange_1,
            Lights.Light.Monochrome_Orange_Flickering,
        });
    }

    #endregion

    #region Apex Alarms
    /// <summary>
    /// Add a BIG alarm in a big room that will be challenging to beat.
    ///
    /// It may be desirable to increase the resources that go into the zone for higher tier
    /// alarms. Especially for harder enemies such as Nightmare enemies that require a lot
    /// of ammo and tool to clear. Resourcing is left entirely up to the calling site.
    /// </summary>
    /// <param name="lockedNode"></param>
    /// <param name="population"></param>
    /// <param name="settings"></param>
    public void AddApexAlarm(
        ZoneNode lockedNode,
        WavePopulation? population = null,
        WaveSettings? settings = null)
    {
        var lockedZone = planner.GetZone(lockedNode);
        var setupish = planner.GetBuildFrom(lockedNode);

        if (setupish == null)
        {
            Plugin.Logger.LogWarning($"AddGeneratorPuzzle() returned early due to missing zones: locked={lockedNode}");
            return;
        }

        var setupNode = (ZoneNode)setupish;
        var setupZone = planner.GetZone(setupNode);

        if (lockedZone == null || setupZone == null)
        {
            Plugin.Logger.LogWarning($"AddGeneratorPuzzle() returned early due to missing zones: locked={lockedZone}");
            return;
        }

        planner.UpdateNode(setupNode with { MaxConnections = 3 });
        setupZone.GenKingOfTheHillGeomorph(level, level.Director[setupNode.Bulkhead]);

        // We want a side spawn room to make it basically impossible to C-foam hold this alarm
        // NOTE: the side spawn room CAN have blood doors roll on it!
        var (sideSpawn, sideSpawnZone) = AddZone(
            setupNode,
            new ZoneNode
            {
                Branch = "apex_spawn",
                MaxConnections = 0,
                Tags = new Tags("no_enemies")
            });

        sideSpawnZone.GenDeadEndGeomorph(level.Complex);
        sideSpawnZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

        var puzzle = director.Tier switch
        {
            "A" => ChainedPuzzle.AlarmClass8,
            "B" => ChainedPuzzle.AlarmClass9,
            "C" => ChainedPuzzle.AlarmClass10,
            "D" => ChainedPuzzle.AlarmClass11,
            "E" => ChainedPuzzle.AlarmClass12,

            _ => ChainedPuzzle.AlarmClass10,
        };

        lockedZone.SecurityGateToEnter = SecurityGate.Apex;
        lockedZone.Alarm = ChainedPuzzle.FindOrPersist(puzzle with
        {
            Population = population ?? puzzle.Population,
            Settings = settings ?? puzzle.Settings
        });

        // Force open the side room
        lockedZone.EventsOnDoorScanStart.AddOpenDoor(director.Bulkhead, sideSpawn.ZoneNumber);
    }

    /// <summary>
    /// Works somewhat like adding an Apex alarm except here we look to add a pretty difficult
    /// alarm from the scan list
    /// </summary>
    public void AddHardAlarm()
    {

    }
    #endregion

    #region Extract forward

    /// <summary>
    /// Adds a branch going forward with the last zone being a forward extract
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public ZoneNode AddForwardExtract(ZoneNode start, int preludeZones = 0)
    {
        var nodes = AddBranch_Forward(start, 1 + preludeZones);

        var exit = nodes.Last();
        var exitZone = planner.GetZone(exit)!;

        exitZone.GenExitGeomorph(level.Complex);

        return exit;
    }

    #endregion

    #region Boss Zone
    /// <summary>
    /// Adds a progressively harder boss fight to a room
    ///
    /// Note: This excludes the MegaMom boss fight. There's a separate method if you want mega mom
    /// </summary>
    /// <param name="bossNode"></param>
    public ZoneNode AddAlignedBoss(ZoneNode bossNode)
    {
        var zone = planner.GetZone(bossNode);

        if (zone == null)
        {
            Plugin.Logger.LogDebug($"Skipping adding boss zone as zone is null: {bossNode}");
            return bossNode;
        }

        zone.GenBossGeomorph(level.Complex);

        // Disable any scouts on anything except E-tier
        if (level.Tier != "E")
            bossNode = planner.UpdateNode(bossNode with { Tags = bossNode.Tags.Extend("no_scouts") });

        // Disable blood doors
        if (level.Tier != "D" && level.Tier != "E")
            bossNode = planner.UpdateNode(bossNode with { Tags = bossNode.Tags.Extend("no_blood_door") });

        Plugin.Logger.LogInfo($"Added aligned boss fight, Zone = {bossNode}");

        switch (level.Tier)
        {
            case "A":
            {
                // TODO: still need to add more or just balance more
                bossNode = planner.UpdateNode(bossNode with { Tags = bossNode.Tags.Extend("no_enemies") });

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single mother
                    (0.4, () =>
                    {
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Mother_AlignedSpawn with { Points = 10 });

                        SetMotherVibe(zone, 150);
                    }),

                    // Double pouncer
                    (0.6, () =>
                    {
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer_AlignedSpawn with { Points = 4 });
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer with { Points = 4 });
                    }),
                });
                break;
            }

            case "B":
            {
                // TODO: still need to add more
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single mother
                    (0.6, () =>
                    {
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Mother_AlignedSpawn with { Points = 10 });

                        SetMotherVibe(zone);
                    }),

                    // Triple pouncer
                    (0.4, () =>
                    {
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer_AlignedSpawn with { Points = 4 });
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer with { Points = 4 });
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer with { Points = 4 });
                    }),
                });
                break;
            }

            case "C":
            {
                // TODO: still need to add more
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single mother
                    (0.4, () =>
                    {
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Mother with { Points = 10 });

                        SetMotherVibe(zone);
                    }),

                    // // Mother and Pouncer
                    // (0.2, () =>
                    // {
                    //     zone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer with { Points = 4 });
                    //     zone.EnemySpawningInZone.Add(EnemySpawningData.Mother_AlignedSpawn with { Points = 10 });
                    //
                    //     SetMotherVibe(zone);
                    // }),

                    // // Tank + pouncer with lots of spitters
                    // (0.4, () =>
                    // {
                    //     zone.EnemySpawningInZone.Add(EnemySpawningData.Tank_AlignedSpawn with { Points = 10 });
                    //     zone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer with { Points = 4 });
                    //
                    //     SetInfectionVibe(zone, 200);
                    // }),
                });
                break;
            }

            case "D":
            {
                // TODO: still need to add more
                Generator.SelectRun(new List<(double, Action)>
                {
                    // PMother
                    (0.4, () =>
                    {
                        zone.EnemySpawningInZone.Add(EnemySpawningData.PMother_AlignedSpawn with { Points = 10 });

                        SetMotherVibe(zone);
                    }),
                });
                break;
            }

            case "E":
            {
                // Done for now
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Tank + pouncer with lots of spitters
                    (0.35, () =>
                    {
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Tank_AlignedSpawn with { Points = 10 });
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer with { Points = 4 });

                        SetInfectionVibe(zone, 200);
                    }),

                    // Triple Potato + Tank
                    // Also probably very hard
                    (0.25, () =>
                    {
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Tank_AlignedSpawn with { Points = 10 });
                        zone.EnemySpawningInZone.Add(EnemySpawningData.TankPotato with { Points = 30 });

                        zone.LightSettings = Lights.Light.Pitch_black_1;

                        zone.AmmoPacks += 4;

                        zone.StaticSpawnDataContainers.Add(
                            new StaticSpawnDataContainer { Count = 50, Unit = StaticSpawnUnit.Corpses });
                    }),

                    // Tank & PMother
                    (0.25, () =>
                    {
                        // Spawn align the mother, and default spawn the tank
                        zone.EnemySpawningInZone.Add(EnemySpawningData.PMother_AlignedSpawn with { Points = 10 });
                        zone.EnemySpawningInZone.Add(EnemySpawningData.Tank with { Points = 10 });

                        SetMotherVibe(zone, 250);

                        zone.AmmoPacks += 6;
                        zone.ToolPacks += 2;
                    })
                });
                break;
            }
        }

        return bossNode;
    }

    /// <summary>
    /// Higher level method for AddAlignedBoss(). This sets up the fight for being hibernation
    /// </summary>
    /// <param name="bossNode"></param>
    /// <returns></returns>
    public ZoneNode AddAlignedBoss_Hibernate(ZoneNode bossNode)
    {
        bossNode = AddAlignedBoss(bossNode);

        var zone = planner.GetZone(bossNode)!;

        zone.EventsOnOpenDoor.AddSound(Sound.TenseRevelation, 2.0);

        return bossNode;
    }

    /// <summary>
    /// This wakes up the room on opening the door
    /// </summary>
    /// <param name="bossNode"></param>
    /// <returns></returns>
    public ZoneNode AddAlignedBoss_WakeOnOpen(ZoneNode bossNode)
    {
        bossNode = AddAlignedBoss(bossNode);

        var zone = planner.GetZone(bossNode)!;

        zone.EventsOnOpenDoor
            .AddAlertEnemies(bossNode.Bulkhead, bossNode.ZoneNumber, 1.0)
            .AddSound(Sound.TankRoar, 2.0);

        return bossNode;
    }

    /// <summary>
    /// Adds a MegaMom sleeping boss fight to the target bossNode.
    ///
    /// The MegaMom boss fight is probably the hardest boss fight in the game. The number of
    /// enemies MegaMom can spawn can be _a lot_ and she also has a huge health pool.
    /// </summary>
    /// <param name="bossNode"></param>
    public void AddAlignedBossFight_MegaMom(
        ZoneNode bossNode,
        ICollection<WardenObjectiveEvent>? onDeathEvents = null)
    {
        var zone = planner.GetZone(bossNode);

        if (zone == null)
        {
            Plugin.Logger.LogDebug($"Skipping adding boss zone as zone is null: {bossNode}");
            return;
        }

        // We pick the specific tiles we want ourselves
        (zone.SubComplex, zone.CustomGeomorph, zone.Coverage) = level.Complex switch
        {
            Complex.Mining => (SubComplex.Refinery, "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_X_HA_06.prefab", new CoverageMinMax { Min = 30, Max = 70 }),
            Complex.Tech => (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_R3D1.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
            Complex.Service => Generator.Pick(new List<(SubComplex, string, CoverageMinMax)>
            {
                (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_HA_03_V2.prefab", new CoverageMinMax { Min = 50, Max = 75 }),

                // --- MOD Geomorphs ---
                // donan3967
                (SubComplex.Floodways, "Assets/geo_64x64_service_floodways_boss_hub_DS_01.prefab", new CoverageMinMax { Min = 30, Max = 40 }),
            }),
        };

        // Only allow connecting forwards
        bossNode = planner.UpdateNode(bossNode with { MaxConnections = 1 });

        zone.EventsOnOpenDoor
            .AddSound(Sound.TenseRevelation, 2.0)
            .AddAlertEnemies(bossNode.Bulkhead, bossNode.ZoneNumber, Generator.Between(10, 12));

        // Disable any scouts on anything except E-tier
        if (level.Tier != "E")
            bossNode = planner.UpdateNode(bossNode with { Tags = bossNode.Tags.Extend("no_scouts") });

        // Disable blood doors and don't roll enemies
        // We still add some enemies later
        bossNode = planner.UpdateNode(bossNode with
        {
            Tags = bossNode.Tags.Extend("no_blood_door", "no_enemies", "boss_megamom")
        });

        zone.EnemySpawningInZone.Add(EnemySpawningData.MegaMother_AlignedSpawn with { Points = 40 });

        // Some small enemies
        zone.EnemySpawningInZone.Add(EnemySpawningData.Baby with { Points = 10 });
        zone.EnemySpawningInZone.Add(EnemySpawningData.Striker with { Points = 5 });
        zone.EnemySpawningInZone.Add(EnemySpawningData.Shooter with { Points = 5 });

        SetMotherVibe(zone, 300);

        // We need more resources
        zone.AmmoPacks += 12;
        zone.ToolPacks += 4;

        // if (onDeathEvents is not null)
        if (onDeathEvents is not null && onDeathEvents.Any())
            level.EOS_EventsOnBossDeath.Definitions.Add(
                new EventsOnBossDeath
                {
                    ZoneNumber = bossNode.ZoneNumber,
                    Bulkhead = bossNode.Bulkhead,
                    PersistentIds = new()
                    {
                        Enemy_New.MegaMother.PersistentId
                    },
                    Events = onDeathEvents.ToList()
                });

        Plugin.Logger.LogInfo($"Added megamom fight, Zone = {bossNode}");

        #region Warden Intel Messages
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 5), Generator.Draw(new List<string>
        {
            ">... A nest?\r\n>... We have no choice. <color=red><size=200%>She's waiting.</size></color>\n>... Why... why?"
        }))!);
        #endregion
    }
    #endregion

    #region Sleeping Enemy Zone

    /// <summary>
    /// Adds a tough infested sleeper hub zone
    /// </summary>
    /// <param name="node"></param>
    public ZoneNode AddStealth_Infested(ZoneNode node)
    {
        var zone = planner.GetZone(node);

        if (zone == null)
        {
            Plugin.Logger.LogDebug($"Skipping adding infested sleeper zone as zone is null: {node}");
            return node;
        }

        node = planner.UpdateNode(node with { Tags = node.Tags.Extend("no_enemies")});

        SetInfestedVibe(zone);

        zone.EnemySpawningInZone.Add(EnemySpawningData.StrikerInfested with
        {
            Points = level.Tier switch
            {
                "A" => 15,
                "B" => 20,
                "C" => 25,
                "D" => 30,
                "E" => 35,

                _ => 15
            }
        });

        return node;
    }

    #endregion

    #region Scout Zones

    /// <summary>
    /// Sets up a zone to spawn a bunch of scouts that must be cleared
    /// </summary>
    /// <param name="zone"></param>
    public ZoneNode AddScoutRoom(ZoneNode node)
    {
        var zone = planner.GetZone(node);

        if (zone == null)
        {
            Plugin.Logger.LogDebug($"Skipping adding boss zone as zone is null: {node}");
            return node;
        }

        // Prevent normal enemy spawning
        node = planner.UpdateNode(node with { Tags = node.Tags.Extend("no_enemies", "no_scouts") });

        // Set a big open geomorph
        zone.GenHubGeomorph(level.Complex);

        zone.EnemySpawningInZone.Add(EnemySpawningData.Scout with
        {
            Points = level.Tier switch
            {
                "A" => 15,
                "B" => 20,
                "C" => 25,
                "D" => 30,
                "E" => 40
            }
        });

        return node;
    }

    #endregion

    #region Error alarms
    /// <summary>
    /// Adds an error alarm (or desired otherwise alarm) to the locked node and also sets up
    /// the turn-off alarms terminal if it's passed in
    ///
    /// This is now a manually set up process
    ///
    /// Error alarm door text should read:
    /// ```
    /// START SECURITY SCAN SEQUENCE <color=red>[WARNING:CLASS ://ERROR! ALARM DETECTED] DEACTIVATE ALARM ON <color=orange>TERMINAL_123</color></color>
    /// ```
    ///
    /// Command to turn off alarms:
    /// ```
    /// DEACTIVATE_ALARMS    Terminates any active alarms linked to this terminal.
    /// ```
    /// </summary>
    /// <param name="lockedNode"></param>
    /// <param name="terminalNode"></param>
    /// /// <param name="puzzle"></param>
    public void AddErrorAlarm(
        ZoneNode lockedNode,
        ZoneNode? terminalish,
        WaveSettings? settings,
        WavePopulation? population)
    {
        var lockedZone = planner.GetZone(lockedNode);
        var setupish = planner.GetBuildFrom(lockedNode);

        if (setupish == null)
        {
            Plugin.Logger.LogWarning($"AddGeneratorPuzzle() returned early due to missing zones: locked={lockedNode} terminal={terminalish}");
            return;
        }

        var setupNode = (ZoneNode)setupish;
        var setupZone = planner.GetZone(setupNode);

        if (lockedZone == null || setupZone == null)
        {
            Plugin.Logger.LogWarning($"AddGeneratorPuzzle() returned early due to missing zones: locked={lockedZone} terminal={terminalish}");
            return;
        }

        // We use a template error alarm to provide the correct scan, but it doesn't trigger any waves
        lockedZone.Alarm = ChainedPuzzle.AlarmError_Template;
        lockedZone.SecurityGateToEnter = SecurityGate.Security;
        level.Settings.ErrorAlarmZones.Add(lockedNode);

        lockedZone.EventsOnDoorScanStart
            .AddSound(Sound.Alarms_Error_AmbientLoop, 1.0)
            .AddSpawnWave(new GenericWave
            {
                Settings = settings ?? WaveSettings.Error_Easy,
                Population = population ?? WavePopulation.Baseline,
                TriggerAlarm = true
            }, 1.0, "error_alarms");

        // Custom door message
        var customDoor = new Custom.AutogenRundown.SecurityDoors.SecurityDoor()
        {
            Bulkhead = director.Bulkhead,
            ZoneNumber = lockedNode.ZoneNumber,
            InteractionMessage = "START SECURITY SCAN SEQUENCE <color=red>[WARNING:CLASS ://ERROR! ALARM DETECTED]</color>"
        };

        Plugin.Logger.LogDebug($"Overriding the normal error alarm logic for this special zone: {lockedNode}");

        // Set the turnoff code (if we have it)
        if (terminalish == null)
        {
            level.SecurityDoors.Doors.Add(customDoor);
            return;
        }

        var terminalNode = (ZoneNode)terminalish;
        var terminalZone = planner.GetZone(terminalNode)!;
        var terminalPlacement = new TerminalPlacement { PlacementWeights = ZonePlacementWeights.NotAtStart };

        if (terminalZone.TerminalPlacements.Any())
            terminalPlacement = terminalZone.TerminalPlacements.First();

        var commandEvents = new List<WardenObjectiveEvent>()
            .AddTurnOffAlarms(9.5, "error_alarms")
            .AddSound(Sound.Alarms_Error_AmbientStop, 10)
            .RemoveCustomHudText(10)
            .ToList();

        terminalPlacement.UniqueCommands.Add(
            new CustomTerminalCommand
            {
                Command = "DEACTIVATE_ALARMS",
                CommandDesc = "Terminates any active error alarms linked to this terminal",
                CommandEvents = commandEvents,
                PostCommandOutputs = new List<TerminalOutput>
                {
                    new()
                    {
                        Output = "Executing alarm-shutdown protocol...",
                        Type = LineType.SpinningWaitNoDone,
                        Time = 2.5
                    },
                    new()
                    {
                        Output = "Connecting to active error alarm systems",
                        Type = LineType.Normal,
                        Time = 2.0
                    },
                    new()
                    {
                        Output = "Confirming valid terminal ID",
                        Type = LineType.Normal,
                        Time = 1.5
                    },
                    new()
                    {
                        Output = "Shutting down alarms",
                        Type = LineType.SpinningWaitDone,
                        Time = 3.0
                    },
                    new()
                    {
                        Output = "Alarms shut down.",
                        Type = LineType.Normal,
                        Time = 1.0
                    },
                },
            });

        // TODO: support other dimensions
        var terminalSerial = Lore.TerminalSerial("Reality", terminalNode.Bulkhead, terminalNode.ZoneNumber);

        customDoor.InteractionMessage += $"<color=red> DEACTIVATE ALARM ON {terminalSerial}</color>";
        lockedZone.EventsOnDoorScanStart.AddCustomHudText($"<color=red>Deactivate alarm on {terminalSerial}</color>");

        level.SecurityDoors.Doors.Add(customDoor);

        // Unlock the turn-off zone door when the alarm door has opened.
        lockedZone.EventsOnDoorScanDone.AddUnlockDoor(director.Bulkhead, terminalNode.ZoneNumber);
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="lockedNode"></param>
    /// <param name="terminalish"></param>
    /// <param name="settings"></param>
    /// <param name="population"></param>
    public void AddApexErrorAlarm(ZoneNode lockedNode,
        ZoneNode? terminalish,
        WaveSettings? settings,
        WavePopulation? population)
    {
        var lockedZone = planner.GetZone(lockedNode);
        var setupish = planner.GetBuildFrom(lockedNode);

        if (setupish == null)
        {
            Plugin.Logger.LogWarning($"AddGeneratorPuzzle() returned early due to missing zones: locked={lockedNode} terminal={terminalish}");
            return;
        }

        var setupNode = (ZoneNode)setupish;
        var setupZone = planner.GetZone(setupNode);

        if (lockedZone == null || setupZone == null)
        {
            Plugin.Logger.LogWarning($"AddGeneratorPuzzle() returned early due to missing zones: locked={lockedZone} terminal={terminalish}");
            return;
        }

        // We use a template error alarm to provide the correct scan, but it doesn't trigger any waves
        lockedZone.Alarm = ChainedPuzzle.AlarmError_Template;
        lockedZone.SecurityGateToEnter = SecurityGate.Apex;
        level.Settings.ErrorAlarmZones.Add(lockedNode);

        lockedZone.EventsOnDoorScanStart
            .AddSound(Sound.Alarms_Error_AmbientLoop, 1.0)
            .AddSpawnWave(new GenericWave
            {
                Settings = settings ?? WaveSettings.Error_Boss_Hard,
                Population = population ?? WavePopulation.SingleEnemy_Tank,
                TriggerAlarm = true
            }, 1.0, "apex_error_alarms");

        // Custom door message
        var customDoor = new Custom.AutogenRundown.SecurityDoors.SecurityDoor
        {
            Bulkhead = director.Bulkhead,
            ZoneNumber = lockedNode.ZoneNumber,
            // <color=purple>[</color><color=#ff00ffff>APEX</color><color=purple>]</color>
            InteractionMessage = "START SECURITY SCAN SEQUENCE <color=red>[WARNING:<color=#ff00ffff>[APEX]</color> ://ERROR! ALARM DETECTED]</color>"
        };

        level.MarkAsBossErrorAlarm();

        Plugin.Logger.LogDebug($"Adding Apex Error alarm for: {lockedNode} (terminal = {terminalish})");

        // Set the turnoff code (if we have it)
        if (terminalish == null)
        {
            level.SecurityDoors.Doors.Add(customDoor);
            return;
        }

        var terminalNode = (ZoneNode)terminalish;
        var terminalZone = planner.GetZone(terminalNode);
        var terminalPlacement = new TerminalPlacement { PlacementWeights = ZonePlacementWeights.NotAtStart };

        if (terminalZone.TerminalPlacements.Any())
            terminalPlacement = terminalZone.TerminalPlacements.First();

        var commandEvents = new List<WardenObjectiveEvent>()
            .AddTurnOffAlarms(9.5, "apex_error_alarms")
            .AddTurnOffAlarms(9.5, "error_alarms")
            .AddSound(Sound.Alarms_Error_AmbientStop, 10)
            .RemoveCustomHudText(10)
            .ToList();

        terminalPlacement.UniqueCommands.Add(
            new CustomTerminalCommand
            {
                Command = "FORCE_DEACTIVATE_ALARMS",
                CommandDesc = "Admin override for deactivating all error/apex alarms",
                CommandEvents = commandEvents,
                PostCommandOutputs = new List<TerminalOutput>
                {
                    new()
                    {
                        Output = "Executing alarm-shutdown protocol...",
                        Type = LineType.SpinningWaitNoDone,
                        Time = 2.5
                    },
                    new()
                    {
                        Output = "Connecting to active error alarm systems",
                        Type = LineType.Normal,
                        Time = 2.0
                    },
                    new()
                    {
                        Output = "Confirming valid terminal ID",
                        Type = LineType.Normal,
                        Time = 1.5
                    },
                    new()
                    {
                        Output = "Shutting down alarms",
                        Type = LineType.SpinningWaitDone,
                        Time = 3.0
                    },
                    new()
                    {
                        Output = "Alarms shut down.",
                        Type = LineType.Normal,
                        Time = 1.0
                    },
                },
            });

        // TODO: support other dimensions
        var terminalSerial = Lore.TerminalSerial("Reality", terminalNode.Bulkhead, terminalNode.ZoneNumber);

        customDoor.InteractionMessage += $"<color=red> DEACTIVATE ALARM ON {terminalSerial}</color>";
        lockedZone.EventsOnDoorScanStart.AddCustomHudText($"<color=red>Deactivate alarm on {terminalSerial}</color>");

        level.SecurityDoors.Doors.Add(customDoor);

        // Unlock the turn-off zone door when the alarm door has opened.
        lockedZone.EventsOnDoorScanDone.AddUnlockDoor(director.Bulkhead, terminalNode.ZoneNumber);
    }

    #endregion

    #region Generator Powered Door
    /// <summary>
    ///
    /// </summary>
    /// <param name="lockedNode">
    ///     The ZoneNode who's entrance is to be locked behind the power generator
    /// </param>
    /// <param name="cellNode">The ZoneNode to place the power cell</param>
    public void AddGeneratorPuzzle(ZoneNode lockedNode, ZoneNode cellNode)
    {
        var lockedZone = planner.GetZone(lockedNode);
        var cellZone = planner.GetZone(cellNode);
        var powerOffish = planner.GetBuildFrom(lockedNode);

        if (powerOffish == null)
        {
            Plugin.Logger.LogWarning($"AddGeneratorPuzzle() returned early due to no parent node: locked={lockedZone} cell={cellZone}");
            return;
        }

        var powerOff = (ZoneNode)powerOffish;
        var powerOffZone = planner.GetZone(powerOff);

        if (lockedZone == null || cellZone == null || powerOffZone == null)
        {
            Plugin.Logger.LogWarning($"AddGeneratorPuzzle() returned early due to missing zones: locked={lockedZone} cell={cellZone}");
            return;
        }

        // Place the key for the locked zone
        lockedZone.ProgressionPuzzleToEnter = new ProgressionPuzzle
        {
            PuzzleType = ProgressionPuzzleType.Generator,
            PlacementCount = 0,
        };

        cellZone.ForceBigPickupsAllocation = true;
        // TODO: Change this so we can dynamically set distributions
        // For instance adding additional cells without needing to know what they are
        cellZone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_1.PersistentId;

        // Turn off the lights in the zone that the locked zone builds from. We will turn on
        // the emergency lights when the cell is plugged in.
        powerOffZone.LightSettings = Lights.Light.Pitch_black_1;

        // Turn on the Auxiliary power lights when inserting the cell
        var powerGenerator = new IndividualPowerGenerator()
        {
            Bulkhead = powerOff.Bulkhead,
            ZoneNumber = powerOff.ZoneNumber,
            EventsOnInsertCell = new()
            {
                new WardenObjectiveEvent()
                {
                    Type = WardenObjectiveEventType.SetLightDataInZone,
                    Trigger = WardenObjectiveEventTrigger.OnStart,
                    LocalIndex = powerOff.ZoneNumber,
                    Layer = 0,
                    Delay = 2.5,
                    Duration = 0.1,
                    SetZoneLight = new()
                    {
                        LightSettings = LightSettings.AuxiliaryPower,
                        Duration = 0.1,
                        Seed = 1,
                    }
                }
            }
        };
        powerGenerator.EventsOnInsertCell.AddSound(Sound.LightsOn_Vol1, 2.0);

        level.EOS_IndividualGenerator.Definitions.Add(powerGenerator);
    }
    #endregion

    #region Keyed Puzzles
    /// <summary>
    /// Adds a keycard lock on the locked zone and places the keycard in keycardNode.
    /// </summary>
    /// <param name="lockedNode"></param>
    /// <param name="keycardNode"></param>
    public void AddKeycardPuzzle(ZoneNode lockedNode, ZoneNode keycardNode)
    {
        var lockedZone = level.Planner.GetZone(lockedNode);
        var keycardZone = level.Planner.GetZone(keycardNode);

        if (lockedZone == null || keycardZone == null)
        {
            Plugin.Logger.LogWarning($"AddKeycardPuzzle() returned early due to missing zones: locked={lockedZone} keycard={keycardZone}");
            return;
        }

        // Place the key for the locked zone
        lockedZone.ProgressionPuzzleToEnter = new ProgressionPuzzle
        {
            PuzzleType = ProgressionPuzzleType.Keycard,
            ZonePlacementData = new List<ZonePlacementData>
            {
                new() { LocalIndex = keycardZone.LocalIndex }
            }
        };
    }

    /// <summary>
    /// Adds a key puzzle to enter the zone we have selected
    ///
    /// Generally this should be added manually depending on the objective.
    /// </summary>
    public void AddKeyedPuzzle(
        ZoneNode lockedNode,
        string? searchBranch = null,
        int keyBranchLength = -1)
    {
        var lockedZone = level.Planner.GetZone(lockedNode);

        if (lockedZone == null)
            return;

        searchBranch ??= lockedNode.Branch;

        var openZones = level.Planner
            .GetOpenZones(director.Bulkhead, searchBranch)
            .Where(node => node.ZoneNumber < lockedNode.ZoneNumber).ToList();
        var keyZoneNumber = 0;

        if (openZones.Any())
        {
            // We are able to construct a branch to store the key

            // Determin the key branch length
            var branchLength = keyBranchLength > 0 ? keyBranchLength : director.Tier switch
            {
                "A" => 1,
                "B" => 1,
                "C" => 1,
                "D" => Generator.Select(new List<(double, int)>
                {
                    (0.75, 1),
                    (0.25, 2)
                }),
                "E" => Generator.Select(new List<(double, int)>
                {
                    (0.60, 1),
                    (0.35, 2),
                    (0.05, 3)
                }),

                _ => 1
            };

            var branchBase = Generator.Pick(openZones);
            var branchIndex = 1;

            // Find a valid branch name
            while (level.Planner.GetZones(Bulkhead.All, $"key_{branchIndex}").Any())
                branchIndex++;

            var branch = $"key_{branchIndex}";
            var last = BuildBranch(branchBase, branchLength, branch);

            var branchFirstNode = level.Planner.GetZones(director.Bulkhead, branch).First();
            var firstZone = level.Planner.GetZone(branchFirstNode)!;
            var branchBaseZone = level.Planner.GetZone(branchBase)!;

            // Try and set the first zone branch to actually pick a direction that can work
            firstZone.SetExpansionAsBranchOfZone(branchBaseZone,
                Generator.Pick(new List<Direction>
                    { Direction.Left, Direction.Right }));

            keyZoneNumber = last.ZoneNumber;
        }
        else
        {
            // Bad luck, this key is a freebie and we just place in one of the preceeding zones
            // Pick one of the zones before the door
            keyZoneNumber = Generator.Pick(
                level.Planner
                    .GetZones(director.Bulkhead, lockedNode.Branch)
                    .Where(node => node.ZoneNumber < lockedNode.ZoneNumber)
            ).ZoneNumber;
        }

        // Place the key for the locked zone
        lockedZone.ProgressionPuzzleToEnter = new ProgressionPuzzle
        {
            PuzzleType = ProgressionPuzzleType.Keycard,
            ZonePlacementData = new List<ZonePlacementData>
            {
                new() { LocalIndex = keyZoneNumber }
            }
        };
    }
    #endregion

    #region Resourcing
    /// <summary>
    /// TODO: use
    /// </summary>
    /// <param name="start"></param>
    public void AddResourceZone(ZoneNode start)
    {
        var (node, zone) = AddZone(start, new ZoneNode { MaxConnections = 0 });

        switch (level.Complex)
        {
            case Complex.Mining:
                break;

            case Complex.Tech:
                break;

            case Complex.Service:
                break;
        }

        zone.AliasPrefix = "Armory, ZONE";

        zone.AmmoPacks += 10;
        zone.HealthPacks += 10;
        zone.ToolPacks += 6;
    }

    /// <summary>
    /// Add a small side zone which contains a disinfection station in it
    /// </summary>
    /// <param name="start">ZoneNode to build the side disinfection station from</param>
    public void AddDisinfectionZone(ZoneNode start)
    {
        var (_, zone) = AddZone(start, new ZoneNode { MaxConnections = 0 });

        switch (level.Complex)
        {
            case Complex.Mining:
                zone.SubComplex = SubComplex.Storage;
                zone.CustomGeomorph = "Assets/Prefabs/Geomorph/Mining/geo_storage_FA_dead_end_01.prefab";
                zone.Coverage = new CoverageMinMax { Min = 5, Max = 10 };
                break;

            case Complex.Tech:
                zone.SubComplex = SubComplex.Lab;
                zone.CustomGeomorph = "Assets/Prefabs/Geomorph/Tech/geo_lab_FA_dead_end_01.prefab";
                zone.Coverage = new CoverageMinMax { Min = 5, Max = 10 };
                break;

            case Complex.Service:
                zone.SubComplex = SubComplex.Floodways;
                zone.CustomGeomorph = "Assets/geo_64x64_service_floodways_armory_DS_01.prefab";
                zone.Coverage = new CoverageMinMax { Min = 5, Max = 10 };
                break;
        }

        // Also add some disinfection packs
        zone.DisinfectPacks += 6;

        zone.DisinfectionStationPlacements.Add(new FunctionPlacementData
        {
            PlacementWeights = ZonePlacementWeights.NotAtStart
        });
    }
    #endregion
}
