using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;
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
        var (corridor1, corridor1Zone) = AddZone_Forward(start, new ZoneNode { MaxConnections = 1 });

        {
            corridor1Zone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_03.prefab";
            corridor1Zone.Coverage = CoverageMinMax.Small_10;
            corridor1Zone.Altitude = Altitude.OnlyHigh;
            corridor1Zone.LightSettings = Lights.Light.RedToYellow_1;
        }


        // ------ Penultimate corridor ------
        var (corridor2, corridor2Zone) = AddZone_Forward(corridor1, new ZoneNode { MaxConnections = 1 });

        {
            corridor2Zone.CustomGeomorph =
                "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_06.prefab";
            corridor2Zone.Coverage = CoverageMinMax.Tiny_3;
            corridor2Zone.SecurityGateToEnter = SecurityGate.Apex;
            corridor2Zone.AliasPrefix = "KDS Deep, ZONE";
            corridor2Zone.AliasPrefixShort = "KDS";
            corridor2Zone.Altitude = Altitude.OnlyHigh;
            corridor2Zone.LightSettings = Lights.Light.RedToWhite_1_R5A2_L1;

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
                    (1.0, ChainedPuzzle.AlarmClass4_Mixed)
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

            scanDoneEvents
                .AddMessage("SURVIVE", 6.0)
                .AddWinOnDeath(surviveDuration)
                .AddMessage("WARDEN SECURITY SYSTEMS DISABLED", surviveDuration + 2.5);

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

        var endStart = elevator;

        PuzzlePack = level.Tier switch
        {
            "D" => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                (0.5, 8, ChainedPuzzle.None),
                (0.5, 12, ChainedPuzzle.TeamScan)
            },

            "E" => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                (0.5, 6, ChainedPuzzle.None),
                (0.5, 14, ChainedPuzzle.TeamScan)
            },

            // A/B/C
            _ => new List<(double chance, int count, ChainedPuzzle puzzle)>
            {
                (0.5, 10, ChainedPuzzle.None),
                (0.5, 10, ChainedPuzzle.TeamScan)
            },
        };

        #region Start challenge
        switch (level.Tier)
        {
            #region Tier: C

            case "C":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (1.0, () =>
                    {
                        var segment1 = AddBranch_Forward(elevator, 3, zoneCallback: (node, zone) =>
                        {
                            planner.AddTags(node, "no_enemies", "no_blood_door");
                            zone.GenCorridorGeomorph(level.Complex);
                        });

                        var (turn1, turn1Zone) = AddZone_Forward(
                            segment1.Last(),
                            new ZoneNode { MaxConnections = 1, Tags = new Tags("no_enemies") });
                        turn1Zone.CustomGeomorph =
                            "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_L_HA_01.prefab";

                        Generator.SelectRun(new List<(double, Action)>
                        {
                            (0.33, () => AddKeycardPuzzle(turn1, segment1[2])),
                            (0.33, () => AddGeneratorPuzzle(turn1, segment1[2])),
                            (0.33, () => AddTerminalUnlockPuzzle(turn1, segment1[2])),
                        });

                        var segment2 = AddBranch_Left(turn1, 2, zoneCallback: (node, zone) =>
                        {
                            planner.AddTags(node, "no_enemies");
                            zone.GenCorridorGeomorph(level.Complex);
                        });

                        endStart = segment2.Last();
                    }),
                });
                break;
            }

            #endregion

            #region Tier: D

            case "D":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (1.0, () => { }),
                });
                break;
            }

            #endregion

            #region Tier: E

            case "E":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (1.0, () => { }),
                });
                break;
            }

            #endregion

            default:
            {
                break;
            }
        }
        #endregion

        #region Mid challenge
        //
        // Builds out a middle challenge item
        //
        Generator.SelectRun(new List<(double, Action)>
        {
            // This is a terminal to disable the KDS deep defenses
            (1.0, () =>
            {
                var (hub, hubZone) = AddZone_Forward(endStart, new ZoneNode
                {
                    Tags = new Tags("no_enemies", "no_blood_door")
                });

                hubZone.CustomGeomorph =
                    "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_SF_01.prefab";

                var (security, securityZone) = AddZone_Side(hub, new ZoneNode
                {
                    MaxConnections = 0,
                    Tags = new Tags("no_enemies")
                });

                securityZone.AliasPrefix = "Security, ZONE";

                var (next, nextZone) = AddZone_Forward(hub, new ZoneNode
                {
                    Tags = new Tags("no_enemies", "no_blood_door")
                });
                nextZone.ProgressionPuzzleToEnter = ProgressionPuzzle.AdminLocked;

                var terminal = securityZone.TerminalPlacements.First();

                terminal.UniqueCommands.Add(new CustomTerminalCommand
                {
                    Command = "KDS-DEEP_DEACTIVATE_DEFENSE",
                    CommandDesc = "Deactivate KDS Deep",
                    CommandEvents = new List<WardenObjectiveEvent>()
                        .AddUnlockDoor(
                            next.Bulkhead,
                            next.ZoneNumber,
                            null,
                            WardenObjectiveEventTrigger.OnStart,
                            10)
                        .AddUpdateSubObjective(
                            header: new Text($"Proceed to KDS Deep"),
                            description: new Text($"Use information in the environment to find KDS Deep"),
                            intel: $"Door unlocked. Proceed to KDS Deep",
                            delay: 13.0)
                        .ToList(),
                    PostCommandOutputs = new List<TerminalOutput>
                    {
                        // 3 for starting command
                        new()
                        {
                            Output = "Connecting to door control plane...",
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
                            Output = "Activating door servo motor systems...",
                            Type = LineType.SpinningWaitDone,
                            Time = 3.0
                        },
                        new()
                        {
                            Output = "Door unlocked.",
                            Type = LineType.Normal,
                            Time = 1.0
                        },
                    }
                });

                objective.EventsOnElevatorLand.AddUpdateSubObjective(
                    header: new Text($"Find {Intel.Terminal(security)} and deactivate KDS Defense"),
                    description: new Text($"Use information in the environment to find {Intel.Terminal(security)}"),
                    intel: $"Find {Intel.Terminal(security)} in the KDS Security zone",
                    delay: 5.0);

                endStart = next;
            }),
        });

        #endregion

        #region End challenge
        switch (level.Tier)
        {
            #region Tier: C

            case "C":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (1.0, () =>
                    {
                        var segment1 = AddBranch_Right(endStart, 2, zoneCallback: (node, zone) =>
                        {
                            planner.AddTags(node, "no_enemies", "no_blood_door");
                            zone.GenCorridorGeomorph(level.Complex);
                        });

                        Generator.SelectRun(new List<(double, Action)>
                        {
                            (0.33, () => AddKeycardPuzzle(segment1[1], segment1[0])),
                            (0.33, () => AddGeneratorPuzzle(segment1[1], segment1[0])),
                            (0.33, () => AddTerminalUnlockPuzzle(segment1[1], segment1[0])),
                        });

                        endStart = segment1.Last();
                    }),
                });
                break;
            }

            #endregion

            #region Tier: D

            case "D":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (1.0, () => { }),
                });
                break;
            }

            #endregion

            #region Tier: E

            case "E":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (1.0, () => { }),
                });
                break;
            }

            #endregion

            default:
            {
                break;
            }
        }
        #endregion

        AddKdsDeep_R8E1Exit(endStart);
        }
}
