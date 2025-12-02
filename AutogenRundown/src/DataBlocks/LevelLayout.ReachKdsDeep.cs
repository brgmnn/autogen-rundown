using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Terminals;
using AutogenRundown.DataBlocks.WorldEvents;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;
using AutogenRundown.Utils;

namespace AutogenRundown.DataBlocks;

// Ideas:
//
//  * ADMIN_TEMP_OVERRIDE command styled in R8E2
//  * Really hard error alarm chasing you
//
// Door Intel message on approach for halfway point check:
//      <color=red>://ERROR: Door in temporary lockdown. Admin clearance required to operate.</color>
//
// Intel when door becomes unlocked:
//      <color=red>ERROR:</color> Connection to <color=yellow>ZONE 521</color> lost. Door unlocked.
//
// R8E1 has 19 zones in it (main)
public partial record LevelLayout
{
    /// <summary>
    /// A mad dash to the exit. Adds +3 zones
    ///
    /// R8E1 has:
    ///     * 2x keycard in zone challenges
    ///     * Doors mostly long team scans or None puzzles
    ///     * halfway R2E1 hub is a class 1 stealth scan with 4 individual scans
    ///
    /// * Checkpoint near the very end
    /// * Last stand at the door before KDS deep crater
    /// * Smoke after the explosion in the corridor
    /// * Smoking crater?
    ///
    /// </summary>
    /// <param name="start"></param>
    public void AddKdsDeep_R8E1Exit(ZoneNode start)
    {
        // Level settings
        level.FogSettings = Fog.DefaultFog;
        level.CustomSuccessScreen = SuccessScreen.ResourcesExpended;


        // ------ Snatcher scan corridor ------
        var (corridor1, corridor1Zone) = AddZone_Forward(start, new ZoneNode
        {
            MaxConnections = 1,
            Tags = new Tags("no_enemies", "no_blood_door")
        });

        {
            corridor1Zone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_03.prefab";
            corridor1Zone.Coverage = CoverageMinMax.Small_10;
            corridor1Zone.Altitude = Altitude.OnlyHigh;
            corridor1Zone.LightSettings = Lights.Light.RedToYellow_1;
        }


        // ------ Penultimate corridor ------
        var (corridor2, corridor2Zone) = AddZone_Forward(corridor1, new ZoneNode
        {
            MaxConnections = 1,
            Tags = new Tags("no_enemies", "no_blood_door")
        });

        {
            corridor2Zone.CustomGeomorph =
                "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_06.prefab";
            corridor2Zone.Coverage = CoverageMinMax.Tiny_3;
            corridor2Zone.SecurityGateToEnter = SecurityGate.Apex;
            corridor2Zone.AliasPrefix = "KDS Deep, ZONE";
            corridor2Zone.AliasPrefixShort = "KDS";
            corridor2Zone.Altitude = Altitude.OnlyHigh;
            corridor2Zone.LightSettings = Lights.Light.RedToWhite_1_R5A2_L1;

            corridor2Zone.EnemySpawningInZone.Add(level.Tier switch
            {
                "D" => EnemySpawningData.ChargerGiant with { Points = 8 },
                "E" => EnemySpawningData.NightmareGiant with { Points = 8 },
                _ => EnemySpawningData.Charger with { Points = 6 }
            });
            corridor2Zone.EventsOnOpenDoor.AddAlertEnemies(corridor2.Bulkhead, corridor2.ZoneNumber, 2.0);

            var puzzle = Generator.Select(level.Tier switch
            {
                "D" => new List<(double, ChainedPuzzle)>
                {
                    (1.0, ChainedPuzzle.AlarmClass4_Surge)
                },
                "E" => new List<(double, ChainedPuzzle)>
                {
                    (1.0, ChainedPuzzle.AlarmClass4_Surge)
                },
                _ => new List<(double, ChainedPuzzle)>
                {
                    (1.0, ChainedPuzzle.AlarmClass4_Team)
                }
            });

            corridor2Zone.UseStaticBioscanPointsInZone = true;
            corridor2Zone.Alarm = ChainedPuzzle.FindOrPersist(puzzle);

            var delay = Generator.Between(1, 4);
            var explosionDelay = delay + 17;
            var auxLightsDelay = explosionDelay + 4;

            // Events to simulate the reactor blowing
            corridor2Zone.EventsOnDoorScanDone
                .AddSound(Sound.MachineryBlow, delay)
                .AddScreenShake(1.0, explosionDelay)
                .AddSetZoneLights(corridor1.ZoneNumber, 0, new SetZoneLight
                {
                    LightSettings = Light.LightSettings.LightsOff,
                    Duration = 0.1,
                    Seed = 1,
                }, 0.1, explosionDelay + 0.5)
                .AddSetZoneLights(corridor2.ZoneNumber, 0, new SetZoneLight
                {
                    LightSettings = Light.LightSettings.LightsOff,
                    Duration = 0.1,
                    Seed = 1,
                }, 0.1, explosionDelay + 0.7)
                .AddSound(Sound.Environment_PowerdownFailure, delay: explosionDelay + 1.0)
                .AddSetZoneLights(corridor1.ZoneNumber, 0, new SetZoneLight
                {
                    LightSettings = Light.LightSettings.AuxiliaryPower,
                    Duration = 0.1,
                    Seed = 1,
                }, 0.1, auxLightsDelay + 0.5)
                .AddSetZoneLights(corridor2.ZoneNumber, 0, new SetZoneLight
                {
                    LightSettings = Light.LightSettings.AuxiliaryPower,
                    Duration = 0.1,
                    Seed = 1,
                }, 0.1, auxLightsDelay + 0.5)
                .AddSound(Sound.LightsOn_Vol3, auxLightsDelay);
        }

        // ------ KDS Deep HSU Exit tile ------
        var (exit, exitZone) = AddZone_Forward(
            corridor2,
            new ZoneNode
            {
                MaxConnections = 0,
                Tags = new Tags("no_enemies", "no_blood_door")
            });

        {
            exitZone.LightSettings = (Lights.Light)Light.LightSettings.AuxiliaryPower.PersistentId;
            exitZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_HSU_exit_R8E1.prefab";
            exitZone.AliasPrefix = "KDS Deep, ZONE";
            exitZone.AliasPrefixShort = "KDS";
            exitZone.Altitude = Altitude.OnlyHigh;
            exitZone.LightSettings = Lights.Light.Reactor_blue_to_red_all_on_1;

            exitZone.Alarm = ChainedPuzzle.TeamScan;

            exitZone.EventsOnOpenDoor
                .AddActivateChainedPuzzle("CustomSpawnExit", 1.0)
                .AddSetNavMarker("WE_R8E1_Center", 0.5);

            // Plays the dramatic tension when they see the destruction
            exitZone.EventsOnTrigger.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.PlaySound,
                    Trigger = WardenObjectiveEventTrigger.None,
                    TriggerFilter = "Evt_TriggerVoice_R8E1",
                    SoundId = Sound.DramaticTension
                });

            var scanDoneEvents = new List<WardenObjectiveEvent>();
            var surviveDuration = level.Tier switch
            {
                "A" => 30.0,
                "B" => 40.0,
                "C" => 50.0,
                "D" => 70.0,
                "E" => 90.0,

                _ => 30.0
            };

            // Warden messages and win on death
            scanDoneEvents
                .AddMessage("SURVIVE", 6.0)
                .AddWinOnDeath(surviveDuration)
                .AddMessage("WARDEN SECURITY SYSTEMS DISABLED", surviveDuration + 2.5);

            // Enemy waves
            scanDoneEvents
                .AddSpawnWave(
                    new GenericWave
                    {
                        Settings = WaveSettings.Error_VeryHard,
                        Population = WavePopulation.Baseline,
                        TriggerAlarm = false
                    }, 8.0)
                .AddSpawnWave(
                    new GenericWave
                    {
                        Settings = WaveSettings.Survival_Impossible_MiniBoss,
                        Population = WavePopulation.SingleEnemy_Tank,
                        TriggerAlarm = false
                    }, surviveDuration + 4.0);

            if (level.Tier == "D")
            {
                scanDoneEvents
                    .AddSpawnWave(
                        new GenericWave
                        {
                            Settings = WaveSettings.SingleMiniBoss,
                            Population = WavePopulation.SingleEnemy_PouncerShadow,
                            TriggerAlarm = false
                        }, surviveDuration * 0.25)
                    .AddSpawnWave(
                        new GenericWave
                        {
                            Settings = WaveSettings.SingleWave_MiniBoss_8pts,
                            Population = WavePopulation.OnlyInfectedHybrids,
                            TriggerAlarm = false
                        }, surviveDuration * 0.66);
            }
            else if (level.Tier == "E")
            {
                scanDoneEvents
                    .AddSpawnWave(
                        new GenericWave
                        {
                            Settings = WaveSettings.SingleWave_MiniBoss_4pts,
                            Population = WavePopulation.SingleEnemy_Pouncer,
                            TriggerAlarm = false
                        }, surviveDuration * 0.20)
                    .AddSpawnWave(
                        new GenericWave
                        {
                            Settings = WaveSettings.Finite_25pts_Normal,
                            Population = WavePopulation.OnlyChargers,
                            TriggerAlarm = false
                        }, surviveDuration * 0.55)
                    .AddSpawnWave(
                        new GenericWave
                        {
                            Settings = WaveSettings.SingleWave_MiniBoss_16pts,
                            Population = WavePopulation.OnlyGiantShooters,
                            TriggerAlarm = false
                        }, surviveDuration * 0.85);
            }

            exitZone.WorldEventChainedPuzzleData.Add(new WorldEventChainedPuzzle
            {
                Puzzle = ChainedPuzzle.TeamScan,
                WorldEventObjectFilter = "CustomSpawnExit",
                EventsOnScanDone = scanDoneEvents
            });
        }
    }

    /// <summary>
    /// Only allow this level to be built on C-tier and below on Mining complex
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    /// <exception cref="Exception"></exception>
    public void BuildLayout_ReachKdsDeep(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode? startish)
    {
        // There's a problem if we have no start zone
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var elevator = (ZoneNode)startish;
        planner.AddTags(elevator, "no_enemies");

        var elevatorZone = planner.GetZone(elevator)!;
        elevatorZone.Coverage = CoverageMinMax.Small_10;
        elevatorZone.ConsumableDistributionInZone = ConsumableDistribution.Baseline_LockMelters.PersistentId;

        var endStart = elevator;

        PuzzlePack = level.Tier switch
        {
            "D" => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                (0.1,  8, ChainedPuzzle.None),
                (0.1, 12, ChainedPuzzle.TeamScan),
                (0.4,  2, ChainedPuzzle.StealthScan1),
                (0.4,  1, ChainedPuzzle.StealthScan2)
            },

            "E" => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                (0.10,  6, ChainedPuzzle.None),
                (0.10, 14, ChainedPuzzle.TeamScan),
                (0.35,  2, ChainedPuzzle.StealthScan1),
                (0.45,  1, ChainedPuzzle.StealthScan2)
            },

            // A/B/C
            _ => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                (0.2, 10, ChainedPuzzle.None),
                (0.2, 10, ChainedPuzzle.TeamScan),
                (0.8,  1, ChainedPuzzle.StealthScan1)
            },
        };

        #region Start challenge
        //
        // All tiers will pick from the same category of options
        //
        Generator.SelectRun(new List<(double, Action)>
        {
            (1.0, () =>
            {
                var segment1 = AddBranch_Forward(elevator, 3, zoneCallback: (node, zone) =>
                {
                    node = planner.UpdateNode(node with { MaxConnections = 1 });
                    planner.AddTags(node, "no_enemies", "no_blood_door");
                    zone.GenCorridorGeomorph(level.Complex);
                    zone.HealthPacks = 0.0;
                    zone.ToolPacks = 0.0;
                    zone.AmmoPacks = 1.0;
                    zone.ConsumableDistributionInZone = 0;
                });

                var (turn1, turn1Zone) = AddZone_Forward(
                    segment1.Last(),
                    new ZoneNode { MaxConnections = 3, Tags = new Tags("no_enemies") });
                turn1Zone.GenHubGeomorph(level.Complex);
                turn1Zone.HealthPacks = 1.0;
                turn1Zone.ToolPacks = 1.0;
                turn1Zone.AmmoPacks = 3.0;
                turn1Zone.ConsumableDistributionInZone = 0;

                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.33, () => AddKeycardPuzzle(turn1, segment1[2])),
                    (0.33, () => AddGeneratorPuzzle(turn1, segment1[2])),
                    (0.33, () => AddTerminalUnlockPuzzle(turn1, segment1[2])),
                });

                var segment2 = AddBranch_Side(turn1, 2, zoneCallback: (node, zone) =>
                {
                    node = planner.UpdateNode(node with { MaxConnections = 1 });
                    planner.AddTags(node, "no_enemies");
                    zone.GenCorridorGeomorph(level.Complex);
                    zone.HealthPacks = 1.0;
                    zone.ToolPacks = 0.0;
                    zone.AmmoPacks = 1.0;
                    zone.ConsumableDistributionInZone = 0;
                });

                endStart = segment2.Last();
            }),
        });

        var toMidClearTime = planner
            .TraverseToElevator(endStart)
            .Select(node => planner.GetZone(node)?.GetClearTimeEstimate() ?? 0.0)
            .Sum();

        objective.EventsOnElevatorLand
            .AddSpawnWave(
                new GenericWave
                {
                    Settings = WaveSettings.SingleWave_20pts,
                    Population = WavePopulation.OnlyShadows,
                    TriggerAlarm = false
                }, toMidClearTime * 0.4)
            .AddSpawnWave(
                new GenericWave
                {
                    Settings = WaveSettings.SingleMiniBoss,
                    Population = WavePopulation.SingleEnemy_Pouncer,
                    TriggerAlarm = false
                }, toMidClearTime * 0.25)
            .AddSpawnWave(
                new GenericWave
                {
                    Settings = level.Tier switch
                    {
                        "D" => WaveSettings.SingleWave_MiniBoss_8pts,
                        "E" => WaveSettings.SingleWave_MiniBoss_12pts,
                        _ => WaveSettings.SingleWave_MiniBoss_6pts,
                    },
                    Population = WavePopulation.OnlyGiantShooters,
                    TriggerAlarm = false
                }, toMidClearTime * 0.85);

        if (level.Tier != "C")
            objective.EventsOnElevatorLand
                .AddSpawnWave(
                    new GenericWave
                    {
                        Settings = WaveSettings.SingleMiniBoss,
                        Population = WavePopulation.SingleEnemy_PouncerShadow,
                        TriggerAlarm = false
                    }, toMidClearTime * 0.90);

        #endregion

        #region Mid challenge
        //
        // Builds out a middle challenge item
        //
        Generator.SelectRun(new List<(double, Action)>
        {
            //
            // --- Terminal Unlock: KDS Defense Grid ---
            //
            // This is a terminal to disable the KDS deep defenses
            //
            (1.0, () =>
            {
                var (hub, hubZone) = AddZone_Forward(endStart, new ZoneNode
                {
                    MaxConnections = 3,
                    Tags = new Tags("no_enemies", "no_blood_door")
                });
                hubZone.CustomGeomorph =
                    "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_SF_01.prefab";
                hubZone.HealthPacks = 4.0;
                hubZone.ToolPacks = 4.0;
                hubZone.AmmoPacks = 8.0;
                hubZone.ConsumableDistributionInZone = ConsumableDistribution.Baseline_LockMelters.PersistentId;

                var (security, securityZone) = AddZone_Side(hub, new ZoneNode
                {
                    MaxConnections = 0,
                    Tags = new Tags("no_enemies")
                });
                securityZone.AliasPrefix = "Security, ZONE";
                securityZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;
                securityZone.CustomGeomorph = Generator.Select(new List<(double, string)>
                {
                    (1.0, "Assets/geo_64x64_dig_site_dead_end_dak_01.prefab"),
                    (1.0, "Assets/Prefabs/Geomorph/Mining/geo_storage_FA_dead_end_01.prefab"),
                    (1.0, "Assets/SamdownGeos/Storage dead end spawn/DeadEnd_Storage.prefab"),
                });
                hubZone.HealthPacks = 4.0;
                hubZone.ToolPacks = 4.0;
                hubZone.AmmoPacks = 8.0;
                hubZone.ConsumableDistributionInZone = 0;

                var (next, nextZone) = AddZone_Forward(hub, new ZoneNode
                {
                    MaxConnections = 2,
                    Tags = new Tags("no_enemies", "no_blood_door")
                });
                nextZone.ProgressionPuzzleToEnter = ProgressionPuzzle.AdminLocked;

                var terminal = securityZone.TerminalPlacements.First();

                terminal.UniqueCommands.Add(new CustomTerminalCommand
                {
                    Command = "KDS-DEEP_DEACTIVATE_DEFENSE",
                    CommandDesc = "Deactivate KDS Deep defense grid",
                    CommandEvents = new List<WardenObjectiveEvent>()
                        .AddUnlockDoor(
                            next.Bulkhead,
                            next.ZoneNumber,
                            null,
                            WardenObjectiveEventTrigger.OnStart,
                            11)
                        .AddUpdateSubObjective(
                            header: new Text($"Proceed to KDS Deep"),
                            description: new Text($"Use information in the environment to find KDS Deep"),
                            intel: $"Door unlocked. Proceed to KDS Deep",
                            delay: 13.0)
                        .AddSpawnWave(
                            new GenericWave
                            {
                                Settings = level.Tier == "C" ? WaveSettings.Error_Easy : WaveSettings.Error_Normal,
                                Population = WavePopulation.OnlyShadows,
                                TriggerAlarm = false
                            }, 25.0)
                        .ToList(),
                    PostCommandOutputs = new List<TerminalOutput>
                    {
                        // 3 for starting command
                        new()
                        {
                            Output = "Authenticating with BIOCOM...",
                            Type = LineType.SpinningWaitNoDone,
                            Time = 2.5
                        },
                        new()
                        {
                            Output = "Confirming valid terminal ID",
                            Type = LineType.Normal,
                            Time = 1.5
                        },
                        new()
                        {
                            Output = "Confirming operative credentials",
                            Type = LineType.Normal,
                            Time = 1.5
                        },
                        new()
                        {
                            Output = "Deactivating defense grid...",
                            Type = LineType.SpinningWaitDone,
                            Time = 4.0
                        },
                        new()
                        {
                            Output = "KDS Defense system <color=red>inactive</color>",
                            Type = LineType.Normal,
                            Time = 1.0
                        },
                    }
                });

                objective.EventsOnElevatorLand
                    .AddUpdateSubObjective(
                        header: new Text($"Find {Intel.Terminal(security)} and deactivate KDS Defense"),
                        description: new Text($"Use information in the environment to find {Intel.Terminal(security)}"),
                        intel: $"Find {Intel.Terminal(security)} in the KDS Security zone",
                        delay: 5.0)
                    .AddCountdown(
                        countdown: new WardenObjectiveEventCountdown
                        {
                            TitleText = "Security zone time lock lifts in:",
                            TimerColor = "orange",
                            EventsOnDone = new List<WardenObjectiveEvent>()
                                .AddUnlockDoor(security.Bulkhead, security.ZoneNumber, "Door Unlocked")
                                .ToList()
                        },
                        duration: toMidClearTime + hubZone.GetClearTimeEstimate(),
                        delay: 5.0);

                endStart = next;
            }),
        });

        #endregion

        #region End challenge
        //
        // Second act challenge
        //
        Generator.SelectRun(new List<(double, Action)>
        {
            (1.0, () =>
            {
                var segment1 = AddBranch_Side(endStart, 2, zoneCallback: (node, zone) =>
                {
                    node = planner.UpdateNode(node with { MaxConnections = 1 });
                    planner.AddTags(node, "no_enemies", "no_blood_door");
                    zone.GenCorridorGeomorph(level.Complex);
                    zone.ConsumableDistributionInZone = 0;
                    zone.HealthPacks = 0.0;
                    zone.ToolPacks = 0.0;
                    zone.AmmoPacks = 2.0;
                });

                var segment1Zone1 = planner.GetZone(segment1.First())!;
                segment1Zone1.EventsOnOpenDoor
                    .AddSpawnWave(
                        new GenericWave
                        {
                            Settings = WaveSettings.SingleMiniBoss,
                            Population = WavePopulation.SingleEnemy_TankPotato,
                            TriggerAlarm = false
                        }, 20)
                    .AddSpawnWave(
                        new GenericWave
                        {
                            Settings = level.Tier switch
                            {
                                "D" or "E" => WaveSettings.SingleWave_MiniBoss_8pts,
                                _ => WaveSettings.SingleWave_MiniBoss_6pts
                            },
                            Population = WavePopulation.OnlyGiantShooters,
                            TriggerAlarm = false
                        }, 120)
                    .AddSpawnWave(
                        new GenericWave
                        {
                            Settings = WaveSettings.SingleMiniBoss,
                            Population = WavePopulation.SingleEnemy_PouncerShadow,
                            TriggerAlarm = false
                        }, 180);

                var (hub, hubZone) = AddZone(segment1.Last(), new ZoneNode
                {
                    MaxConnections = 3,
                    Tags = new Tags("no_enemies", "no_blood_door")

                });
                hubZone.GenHubGeomorph(level.Complex);
                hubZone.HealthPacks = 4.0;
                hubZone.ToolPacks = 8.0;
                hubZone.AmmoPacks = 8.0;
                hubZone.ConsumableDistributionInZone = 0;

                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.33, () => AddKeycardPuzzle(segment1[1], segment1[0])),
                    (0.33, () => AddGeneratorPuzzle(segment1[1], segment1[0])),
                    (0.33, () => AddTerminalUnlockPuzzle(segment1[1], segment1[0])),

                    (0.33, () => AddKeycardPuzzle(hub, segment1[1])),
                    (0.33, () => AddGeneratorPuzzle(hub, segment1[1])),
                    (0.33, () => AddTerminalUnlockPuzzle(hub, segment1[1])),
                });

                endStart = hub;
            }),
        });
        #endregion

        AddKdsDeep_R8E1Exit(endStart);
        }
}
