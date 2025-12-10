using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: PowerCellDistribution
 *
 *
 * Prisoners have to go and enter a command on a terminal somewhere in the zone. The objective
 * itself is quite simple, but the real fun will come from what the effects are of the terminal
 * command. Base game has lights off as an effect.
 *
 ***************************************************************************************************
 *      TODO List
 *
 * TODO: It would be nice to add special commands other than just lights off that do other modifiers.
 *       Such as fog, error alarm, etc.
 *
 *       Ideas:
 *          1. Spawn boss
 *          2. Flood with fog
 *              a. Flood with fog slowly
 *              b. Instantly flood
 *          3. Trigger error alarm
 *          4. Trigger unit wave
 *          5. Trigger survival of the rest of the level with spawning mega wave at the end
 */
public partial record WardenObjective
{
    /// <summary>
    /// We need to select the type of special terminal command that will be used here
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void PreBuild_SpecialTerminalCommand(BuildDirector director, Level level)
    {
        SpecialTerminalCommand_Type = director.Tier switch
        {
            "A" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.50, SpecialCommand.LightsOff),
                (0.07, SpecialCommand.FillWithFog),
                (0.43, SpecialCommand.KingOfTheHill)
            }),
            "B" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.30, SpecialCommand.LightsOff),
                (0.10, SpecialCommand.FillWithFog),
                (0.10, SpecialCommand.ErrorAlarm),
                (0.50, SpecialCommand.KingOfTheHill)
            }),
            "C" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.30, SpecialCommand.LightsOff),
                (0.10, SpecialCommand.FillWithFog),
                (0.15, SpecialCommand.ErrorAlarm),
                (0.45, SpecialCommand.KingOfTheHill)
            }),
            "D" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.25, SpecialCommand.LightsOff),
                (0.15, SpecialCommand.FillWithFog),
                (0.20, SpecialCommand.ErrorAlarm),
                (0.40, SpecialCommand.KingOfTheHill)
            }),
            "E" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.05, SpecialCommand.LightsOff),
                (0.15, SpecialCommand.FillWithFog),
                (0.30, SpecialCommand.ErrorAlarm),
                (0.50, SpecialCommand.KingOfTheHill)
            }),
        };
    }

    public void Build_SpecialTerminalCommand(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = new Text("Find Computer terminal [ITEM_SERIAL] and input the backdoor command [SPECIAL_COMMAND]");
        FindLocationInfo = "Gather information about the location of [ITEM_SERIAL]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = new Text("Navigate to [ITEM_ZONE] and find [ITEM_SERIAL]");
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Find [ITEM_SERIAL] somewhere inside [ITEM_ZONE]";
        InZoneFindItemHelp = "Use maintenance terminal command PING to find [ITEM_SERIAL]";
        SolveItem = "Proceed to input the backdoor command [SPECIAL_COMMAND] in [ITEM_SERIAL]";

        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        // Place the terminal in the last zone
        var node = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead, "special_terminal")!;
        var zoneIndex = node.ZoneNumber;

        dataLayer.ObjectiveData.ZonePlacementDatas.Add(
            new List<ZonePlacementData>()
            {
                new()
                {
                    LocalIndex = zoneIndex,
                    Weights = ZonePlacementWeights.NotAtStart
                }
            });

        /**
         * Base game commands:
         *  - REROUTE_POWER
         *  - OVERRIDE_LOCKDOWN
         *  - DISABLE_LIFE_SUPPORT
         *  - FLOODGATE_OPEN_S156F
         *  - SCCS_LOCAL_TEMP_SET_294_GRAD_INV_FALSE
         *  - DEACTIVATE_FILTERS_S65T
         *  - SPFC_REACTOR_A42_LINE_7_TRUE
         *  - OPEN_SECURITY_DOORS
         *
         *  - RECYCLE_GROUP_7A42
         *  - DRAIN_COOLING_TANKS
         *  - LOWER_RODS
         */
        switch (SpecialTerminalCommand_Type)
        {
            ///
            /// This is quite an easy command. We just turn the lights off.
            ///
            case SpecialCommand.LightsOff:
            {
                SpecialTerminalCommand = "REROUTE_POWER";
                SpecialTerminalCommandDesc = "Reroute power coupling to sector that has been powered down.";

                EventsOnActivate.AddLightsOff(9.0);

                break;
            }

            ///
            /// Fairly annoying command to get. Fills the level with non-infectious fog.
            ///
            case SpecialCommand.FillWithFog:
            {
                SpecialTerminalCommand = "FLUSH_VENTS";
                SpecialTerminalCommandDesc = "Divert atmospheric control system to initiate environmental maintenance procedures.";

                // Add a fog turbine in the first zone.
                var firstNode = level.Planner.GetZones(director.Bulkhead, null).First()!;
                var firstZone = level.Planner.GetZone(firstNode)!;
                firstZone.BigPickupDistributionInZone = BigPickupDistribution.FogTurbine.PersistentId;

                EventsOnActivate.AddFillFog(11.0);

                break;
            }

            //
            // Triggers an error alarm (except on some easier levels) on the activation of the alarm
            //
            case SpecialCommand.ErrorAlarm:
            {
                (SpecialTerminalCommand, SpecialTerminalCommandDesc) = Generator.Pick(new List<(string, string)>
                {
                    ("ALARM_TEST_PROTOCOL", "Execute system-wide alarm diagnostics and test protocol."),
                    ("OPEN_LOCKDOWN_AREA", "Lift access restrictions on sealed zones, existing biomass containment will be compromised."),
                    ("OVERRIDE_LOCKDOWN", "Bypass emergency containment protocols, initiating a security alert."),
                    ("RESTART_SECURITY_SYSTEM", "Reinitialize all security functions, broadcasting a hostile threat alert."),
                });

                switch (level.Tier, director.Bulkhead, level.Settings.Bulkheads == Bulkhead.PrisonerEfficiency)
                {
                    case ("A", Bulkhead.Main, _):
                    case ("B", Bulkhead.Main, _):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = WavePopulation.Baseline,
                            Settings = WaveSettings.Exit_Objective_Easy,
                            TriggerAlarm = true
                        }, 2.0);
                        break;
                    }

                    // This one isn't an error alarm
                    case ("A", Bulkhead.Extreme, true):
                    {
                        EventsOnActivate
                            .AddSpawnWave(new GenericWave
                            {
                                Population = WavePopulation.Baseline,
                                Settings = WaveSettings.Finite_25pts_Normal
                            }, 2.0)
                            .AddSpawnWave(new GenericWave
                            {
                                Population = WavePopulation.OnlyGiantStrikers,
                                Settings = WaveSettings.SingleWave_MiniBoss_12pts,
                                TriggerAlarm = true
                            }, 45.0);
                        break;
                    }

                    case ("A", Bulkhead.Extreme, _):
                    case ("A", Bulkhead.Overload, _):
                    case ("B", Bulkhead.Extreme, false):
                    case ("B", Bulkhead.Overload, true):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = WavePopulation.Baseline,
                            Settings = WaveSettings.Error_Diminished_Easy,
                            TriggerAlarm = true
                        }, 4.0);
                        break;
                    }

                    // Technically not an error alarm either
                    case ("B", Bulkhead.Extreme, _):
                    {
                        EventsOnActivate
                            .AddSpawnWave(new GenericWave
                            {
                                Population = WavePopulation.Baseline,
                                Settings = WaveSettings.Finite_35pts_Hard
                            }, 2.0)
                            .AddSpawnWave(new GenericWave
                            {
                                Population = WavePopulation.OnlyGiantShooters,
                                Settings = WaveSettings.SingleWave_MiniBoss_16pts,
                                TriggerAlarm = true
                            }, 45.0);
                        break;
                    }

                    case ("B", Bulkhead.Overload, false):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = WavePopulation.Baseline,
                            Settings = WaveSettings.Error_Diminished_Normal,
                            TriggerAlarm = true
                        }, 4.0);
                        break;
                    }

                    case ("C", Bulkhead.Main, _):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = WavePopulation.Baseline,
                            Settings = WaveSettings.Exit_Objective_Medium,
                            TriggerAlarm = true
                        }, 5.0);
                        break;
                    }

                    case ("C", Bulkhead.Extreme, _):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = WavePopulation.Baseline,
                            Settings = WaveSettings.Error_Diminished_Normal,
                            TriggerAlarm = true
                        }, 4.0);
                        break;
                    }

                    case ("C", Bulkhead.Overload, _):
                    case ("D", Bulkhead.Extreme, _):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = WavePopulation.Baseline,
                            Settings = WaveSettings.Error_Easy,
                            TriggerAlarm = true
                        }, 4.0);
                        break;
                    }

                    case ("D", Bulkhead.Main, _):
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = WavePopulation.Baseline_Hybrids,
                            Settings = WaveSettings.Exit_Objective_Hard,
                            TriggerAlarm = true
                        }, 6.0);
                        break;

                    case ("D", Bulkhead.Overload, false):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = (level.Settings.HasChargers(), level.Settings.HasShadows()) switch
                            {
                                (true, _) => WavePopulation.OnlyChargers,
                                (_, true) => WavePopulation.OnlyShadows,
                                (_, _) => WavePopulation.Baseline
                            },
                            Settings = WaveSettings.Error_Hard,
                            TriggerAlarm = true
                        }, 7.0);
                        break;
                    }

                    case ("D", Bulkhead.Overload, true):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = WavePopulation.Baseline,
                            Settings = WaveSettings.Error_Normal,
                            TriggerAlarm = true
                        }, 4.0);
                        break;
                    }

                    case ("E", Bulkhead.Main, _):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = WavePopulation.Baseline_Hybrids,
                            Settings = WaveSettings.Exit_Objective_VeryHard,
                            TriggerAlarm = true
                        }, 4.0);
                        break;
                    }

                    case ("E", Bulkhead.Extreme, _):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = WavePopulation.Baseline,
                            Settings = WaveSettings.Exit_Objective_Medium,
                            TriggerAlarm = true
                        }, 10.0);
                        break;
                    }

                    case ("E", Bulkhead.Overload, false):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = (level.Settings.HasChargers(), level.Settings.HasShadows()) switch
                            {
                                (true, _) => WavePopulation.OnlyChargers,
                                (_, true) => WavePopulation.OnlyShadows,
                                (_, _) => WavePopulation.Baseline
                            },
                            Settings = level.Settings.HasChargers() ?
                                WaveSettings.Exit_Objective_Medium :
                                WaveSettings.Exit_Objective_Hard,
                            TriggerAlarm = true
                        }, 10.0);
                        break;
                    }

                    case ("E", Bulkhead.Overload, true):
                    {
                        EventsOnActivate.AddSpawnWave(new GenericWave
                        {
                            Population = WavePopulation.Baseline,
                            Settings = WaveSettings.Error_VeryHard,
                            TriggerAlarm = true
                        }, 10.0);
                        break;
                    }

                    // Default case is not meant to be too hard
                    default:
                        EventsOnActivate.AddSpawnWave(new()
                        {
                            Population = WavePopulation.Baseline,
                            Settings = WaveSettings.Error_Normal,
                            TriggerAlarm = true
                        }, 12.0);
                        break;
                }

                break;
            }

            //
            // Defend against waves in the center of a big tile
            //
            case SpecialCommand.KingOfTheHill:
            {
                (SpecialTerminalCommand, SpecialTerminalCommandDesc) = Generator.Pick(new List<(string, string)>
                {
                    ("OPEN_SECURITY_DOORS", "Opens all security doors"),
                    ("RELEASE_CONTAINMENT", "Open containment chambers"),
                });
                break;
            }
        }

        // Always have the team scan to verify everyone's here to start the objective
        StartPuzzle = ChainedPuzzle.TeamScan;

        // King of the hill has extra scan
        if (SpecialTerminalCommand_Type == SpecialCommand.KingOfTheHill)
        {
            var population = WavePopulation.Baseline;

            if (level.Settings.HasShadows())
                population = WavePopulation.Baseline_Shadows;
            else if (level.Settings.HasChargers())
                population = WavePopulation.Baseline_Chargers;
            else if (level.Settings.HasNightmares())
                population = WavePopulation.Baseline_Nightmare;

            var settings = director.Tier switch
            {
                "A" => WaveSettings.Baseline_Easy,
                "B" => WaveSettings.Baseline_Normal,
                "C" => WaveSettings.Baseline_Hard,
                "D" => WaveSettings.Baseline_VeryHard,
                "E" => WaveSettings.Baseline_VeryHard
            };

            // TODO: The Zone Sustained scan is also centered on the terminal and so doesn't work great
            MidPuzzle = ChainedPuzzle.FindOrPersist(
                ChainedPuzzle.AlarmClass1_Sustained_MegaHuge with
                {
                    Population = population,
                    Settings = settings
                });

            // Extra boss waves?
            var events = new List<WardenObjectiveEvent>();

            if (director.Tier == "C")
            {
                if (Generator.Flip(0.6))
                    events.AddSpawnWave(GenericWave.SinglePouncer, Generator.Between(90, 120));
            }
            else if (director.Tier == "D")
            {
                if (Generator.Flip(0.9))
                    events.AddSpawnWave(GenericWave.SingleTankPotato, Generator.Between(20, 180));

                if (Generator.Flip(0.2))
                    events.AddSpawnWave(GenericWave.SingleTankPotato, Generator.Between(120, 180));
            }
            else if (director.Tier == "E")
            {
                if (Generator.Flip(0.5))
                    events.AddSpawnWave(GenericWave.SingleMother, Generator.Between(120, 240));

                if (Generator.Flip(0.5))
                    events.AddSpawnWave(GenericWave.SingleTank, Generator.Between(20, 180));

                if (Generator.Flip(0.9))
                    events.AddSpawnWave(GenericWave.SingleTankPotato, Generator.Between(20, 180));

                if (Generator.Flip(0.2))
                    events.AddSpawnWave(GenericWave.SingleTankPotato, Generator.Between(120, 180));
            }

            EventsOnActivate.AddRange(events);
        }

        ChainedPuzzleAtExit = ChainedPuzzle.ExitAlarm.PersistentId;

        // Add exit wave if this is the main bulkhead and it's not already an error alarm command
        if (director.Bulkhead.HasFlag(Bulkhead.Main) && SpecialTerminalCommand_Type != SpecialCommand.ErrorAlarm)
            WavesOnGotoWin.Add(new GenericWave
            {
                Population = WavePopulation.Baseline,
                Settings = WaveSettings.Exit_Objective_Easy,
                TriggerAlarm = true,
                SpawnDelay = 4.0
            });

        #region Warden Intel Messages

        // Generic special terminal command intel
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
        {
            $">... Found the terminal.\r\n>... Command is {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>What does it do?!</color></size>",
            ">... [typing]\r\n>... Backdoor command ready.\r\n>... <size=200%><color=red>Execute now!</color></size>",
            $">... {SpecialTerminalCommand} entered.\r\n>... <size=200%><color=red>Here we go!</color></size>\r\n>... [mechanical sounds]",
            ">... Terminal located.\r\n>... <size=200%><color=red>What's the command?!</color></size>\r\n>... Reading now!",
            $">... [static]\r\n>... Command: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executing!</color></size>",
            ">... Backdoor command found.\r\n>... Ready to execute?\r\n>... <size=200%><color=red>Do it!</color></size>",
            $">... {SpecialTerminalCommand}...\r\n>... <size=200%><color=red>Are we sure?!</color></size>\r\n>... Too late now!",
            ">... [frantic typing]\r\n>... <size=200%><color=red>Command's in!</color></size>\r\n>... [alarm sounds]",
            ">... Terminal's here!\r\n>... Execute the backdoor command.\r\n>... <size=200%><color=red>Now!</color></size>",
            $">... Command is {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Entering now!</color></size>\r\n>... [typing rapidly]",
            ">... [whispering] Terminal found.\r\n>... What's the command?\r\n>... <size=200%><color=red>Check the objective!</color></size>",
            $">... [typing]\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executed!</color></size>",
            ">... Backdoor command ready.\r\n>... <size=200%><color=red>What happens now?!</color></size>\r\n>... Find out!",
            $">... Found it!\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executing!</color></size>",
            ">... [panting]\r\n>... Got the terminal.\r\n>... <size=200%><color=red>Enter the command!</color></size>",
            $">... {SpecialTerminalCommand}?\r\n>... That's the one.\r\n>... <size=200%><color=red>Do it!</color></size>",
            ">... Terminal's locked!\r\n>... <size=200%><color=red>Hacking now!</color></size>\r\n>... [typing]",
            $">... Command ready: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Everyone ready?!</color></size>\r\n>... Execute!",
            ">... [static]\r\n>... Terminal accessed.\r\n>... <size=200%><color=red>Execute command!</color></size>",
            $">... Typing {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Almost there!</color></size>\r\n>... Done!",
            ">... Backdoor command executing.\r\n>... <size=200%><color=red>What's it doing?!</color></size>\r\n>... [mechanical humming]",
            $">... [typing frantically]\r\n>... {SpecialTerminalCommand}!\r\n>... <size=200%><color=red>Entered!</color></size>",
            ">... Terminal interface up.\r\n>... Execute backdoor command.\r\n>... <size=200%><color=red>Now!</color></size>",
            $">... Command: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Consequences unknown!</color></size>\r\n>... Execute anyway!",
            ">... [whispering] Found the terminal.\r\n>... <size=200%><color=red>Enter the command!</color></size>\r\n>... Doing it!",
            $">... {SpecialTerminalCommand} entered.\r\n>... [mechanical sounds]\r\n>... <size=200%><color=red>Something's happening!</color></size>",
            ">... Backdoor access granted.\r\n>... <size=200%><color=red>Command ready!</color></size>\r\n>... Execute!",
            $">... [typing]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Done!",
            ">... Terminal's here!\r\n>... What's the command?\r\n>... <size=200%><color=red>Check your HUD!</color></size>",
            $">... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executing now!</color></size>\r\n>... [alarm blaring]",
            ">... [panting]\r\n>... Terminal located.\r\n>... <size=200%><color=red>Execute!</color></size>",
            $">... Command is {SpecialTerminalCommand}.\r\n>... What does that do?\r\n>... <size=200%><color=red>One way to find out!</color></size>",
            ">... [typing rapidly]\r\n>... Backdoor command in.\r\n>... <size=200%><color=red>Here we go!</color></size>",
            $">... {SpecialTerminalCommand}?\r\n>... <size=200%><color=red>That's the command!</color></size>\r\n>... Entering!",
            ">... Terminal access confirmed.\r\n>... Execute now?\r\n>... <size=200%><color=red>Do it!</color></size>",
            $">... [static]\r\n>... {SpecialTerminalCommand} ready.\r\n>... <size=200%><color=red>Executing!</color></size>",
            ">... Backdoor command located.\r\n>... <size=200%><color=red>Type it in!</color></size>\r\n>... [typing]",
            $">... {SpecialTerminalCommand} executing.\r\n>... <size=200%><color=red>No turning back!</color></size>\r\n>... [mechanical sounds]",
            ">... [whispering] Terminal's right there.\r\n>... Enter the command.\r\n>... <size=200%><color=red>Carefully!</color></size>",
            $">... Command ready: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Execute!</color></size>\r\n>... Done!",
            ">... [typing]\r\n>... Backdoor command entered.\r\n>... <size=200%><color=red>Processing!</color></size>",
            $">... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>What's it going to do?!</color></size>\r\n>... Executing!",
            ">... Terminal interface active.\r\n>... Execute the command.\r\n>... <size=200%><color=red>Now!</color></size>",
            $">... [frantic typing]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Executed!",
            ">... Backdoor access confirmed.\r\n>... <size=200%><color=red>Ready!</color></size>\r\n>... Execute!",
            $">... {SpecialTerminalCommand} entered.\r\n>... [mechanical humming]\r\n>... <size=200%><color=red>It's working!</color></size>",
            ">... [panting]\r\n>... Got the terminal.\r\n>... <size=200%><color=red>Execute command!</color></size>",
            $">... Command: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Entering!</color></size>\r\n>... [typing]",
            ">... Terminal located!\r\n>... Backdoor command ready.\r\n>... <size=200%><color=red>Execute now!</color></size>",
            $">... [typing rapidly]\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Done!</color></size>",
            ">... [static]\r\n>... Terminal accessed.\r\n>... <size=200%><color=red>Execute!</color></size>",
            $">... {SpecialTerminalCommand}?\r\n>... <size=200%><color=red>Yes, execute it!</color></size>\r\n>... [typing]",
            ">... Backdoor command executing.\r\n>... <size=200%><color=red>What happens?!</color></size>\r\n>... [mechanical sounds]",
            $">... [whispering]\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Entering now!</color></size>",
            ">... Terminal's here.\r\n>... Execute backdoor command.\r\n>... <size=200%><color=red>Do it!</color></size>",
            $">... Command is {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Ready!</color></size>\r\n>... Execute!",
            ">... [typing frantically]\r\n>... <size=200%><color=red>Command entered!</color></size>\r\n>... [alarm blaring]",
            $">... {SpecialTerminalCommand} ready.\r\n>... <size=200%><color=red>Execute now!</color></size>\r\n>... [typing]",
            ">... Terminal interface up.\r\n>... Backdoor command ready.\r\n>... <size=200%><color=red>Go!</color></size>",
            $">... [static]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Executing!",
            ">... [panting]\r\n>... Terminal found.\r\n>... <size=200%><color=red>Execute command!</color></size>",
            $">... {SpecialTerminalCommand}.\r\n>... What will it do?\r\n>... <size=200%><color=red>Find out!</color></size>",
            ">... Backdoor command located.\r\n>... <size=200%><color=red>Entering!</color></size>\r\n>... [typing]",
            $">... [typing rapidly]\r\n>... {SpecialTerminalCommand}!\r\n>... <size=200%><color=red>Executed!</color></size>",
            ">... Terminal access granted.\r\n>... Execute now.\r\n>... <size=200%><color=red>Do it!</color></size>",
            $">... Command: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executing!</color></size>\r\n>... [mechanical sounds]",
            ">... [whispering] Terminal's there.\r\n>... <size=200%><color=red>Enter the command!</color></size>\r\n>... On it!",
            $">... {SpecialTerminalCommand} entered.\r\n>... <size=200%><color=red>Here we go!</color></size>\r\n>... [alarm sounds]",
            ">... Backdoor command ready.\r\n>... Execute?\r\n>... <size=200%><color=red>Yes!</color></size>",
            $">... [typing]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Done!",
            ">... Terminal interface active.\r\n>... <size=200%><color=red>Execute command!</color></size>\r\n>... Now!",
            $">... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Ready to execute!</color></size>\r\n>... Go!",
            ">... [frantic typing]\r\n>... Command entered.\r\n>... <size=200%><color=red>Processing!</color></size>",
            $">... [static]\r\n>... {SpecialTerminalCommand} ready.\r\n>... <size=200%><color=red>Execute!</color></size>",
            ">... Backdoor access confirmed.\r\n>... <size=200%><color=red>Command ready!</color></size>\r\n>... Execute!",
            $">... {SpecialTerminalCommand} executing.\r\n>... <size=200%><color=red>No turning back!</color></size>\r\n>... [mechanical humming]",
            ">... [panting]\r\n>... Terminal located.\r\n>... <size=200%><color=red>Execute!</color></size>",
            $">... Command is {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Consequences unknown!</color></size>\r\n>... Execute!",
            ">... [typing rapidly]\r\n>... <size=200%><color=red>Backdoor command in!</color></size>\r\n>... [alarm blaring]",
            $">... {SpecialTerminalCommand}?\r\n>... <size=200%><color=red>That's it!</color></size>\r\n>... Executing!",
            ">... Terminal's here!\r\n>... Execute the command.\r\n>... <size=200%><color=red>Now!</color></size>",
            $">... [typing]\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Entered!</color></size>",
            ">... Backdoor command executing.\r\n>... <size=200%><color=red>What's happening?!</color></size>\r\n>... [mechanical sounds]",
            $">... [whispering]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Executing!",
            ">... Terminal interface up.\r\n>... Execute backdoor command.\r\n>... <size=200%><color=red>Do it!</color></size>",
            $">... Command ready: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Execute now!</color></size>\r\n>... [typing]",
            ">... [frantic typing]\r\n>... Command entered!\r\n>... <size=200%><color=red>Processing!</color></size>",
            $">... {SpecialTerminalCommand} ready.\r\n>... <size=200%><color=red>Executing!</color></size>\r\n>... Done!",
            ">... [static]\r\n>... Terminal accessed.\r\n>... <size=200%><color=red>Execute!</color></size>",
            $">... [panting]\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Entering!</color></size>",
            ">... Backdoor command located.\r\n>... <size=200%><color=red>Type it!</color></size>\r\n>... [typing]",
            $">... {SpecialTerminalCommand} entered.\r\n>... <size=200%><color=red>Here we go!</color></size>\r\n>... [mechanical humming]",
            ">... Terminal's here.\r\n>... Execute the command.\r\n>... <size=200%><color=red>Now!</color></size>",
            $">... [typing rapidly]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Executed!",
            ">... Backdoor access granted.\r\n>... <size=200%><color=red>Ready to execute!</color></size>\r\n>... Go!",
            $">... Command: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executing!</color></size>\r\n>... [alarm sounds]",
            ">... [whispering] Terminal located.\r\n>... <size=200%><color=red>Enter command!</color></size>\r\n>... On it!",
            $">... {SpecialTerminalCommand}.\r\n>... What does it do?\r\n>... <size=200%><color=red>Execute!</color></size>",
            ">... Terminal interface active.\r\n>... Backdoor command ready.\r\n>... <size=200%><color=red>Execute!</color></size>",
            $">... [typing]\r\n>... {SpecialTerminalCommand} entered.\r\n>... <size=200%><color=red>Done!</color></size>",
            ">... [frantic typing]\r\n>... <size=200%><color=red>Command in!</color></size>\r\n>... [mechanical sounds]",
            $">... [static]\r\n>... {SpecialTerminalCommand} ready.\r\n>... <size=200%><color=red>Go!</color></size>",
            ">... Backdoor command executing.\r\n>... <size=200%><color=red>Processing!</color></size>\r\n>... [mechanical humming]",
            $">... {SpecialTerminalCommand}?\r\n>... <size=200%><color=red>Execute it!</color></size>\r\n>... [typing]",
            ">... [panting]\r\n>... Terminal found.\r\n>... <size=200%><color=red>Execute!</color></size>",
            $">... Command is {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Ready!</color></size>\r\n>... Execute!",
            ">... [typing rapidly]\r\n>... Backdoor command entered.\r\n>... <size=200%><color=red>Here we go!</color></size>",
            $">... {SpecialTerminalCommand} executing.\r\n>... <size=200%><color=red>What happens?!</color></size>\r\n>... [alarm blaring]",
            ">... Terminal's here!\r\n>... <size=200%><color=red>Execute command!</color></size>\r\n>... Now!",
            $">... [typing]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Executed!",
            ">... Backdoor access confirmed.\r\n>... Execute the command.\r\n>... <size=200%><color=red>Do it!</color></size>",
            $">... [whispering]\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executing!</color></size>",
            ">... Terminal interface up.\r\n>... <size=200%><color=red>Execute!</color></size>\r\n>... [typing]",
            $">... Command ready: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Entering now!</color></size>\r\n>... Done!",
            ">... [frantic typing]\r\n>... Command entered!\r\n>... <size=200%><color=red>Processing!</color></size>",
            $">... {SpecialTerminalCommand} ready.\r\n>... <size=200%><color=red>Execute!</color></size>\r\n>... [mechanical sounds]",
            ">... [static]\r\n>... Terminal accessed.\r\n>... <size=200%><color=red>Go!</color></size>",
            $">... [panting]\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Entering!</color></size>",
            ">... Backdoor command located.\r\n>... <size=200%><color=red>Type it in!</color></size>\r\n>... [typing]",
            $">... {SpecialTerminalCommand} entered.\r\n>... <size=200%><color=red>Here we go!</color></size>\r\n>... [mechanical humming]",
            ">... Terminal's here.\r\n>... Execute backdoor command.\r\n>... <size=200%><color=red>Now!</color></size>",
            $">... [typing rapidly]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Done!",
            ">... Backdoor access granted.\r\n>... <size=200%><color=red>Ready!</color></size>\r\n>... Execute!",
            $">... Command: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executing!</color></size>\r\n>... [alarm sounds]",
            ">... [whispering] Terminal located.\r\n>... <size=200%><color=red>Enter the command!</color></size>\r\n>... On it!",
            $">... {SpecialTerminalCommand}.\r\n>... What will it do?\r\n>... <size=200%><color=red>Execute!</color></size>",
            ">... Terminal interface active.\r\n>... Backdoor command ready.\r\n>... <size=200%><color=red>Go!</color></size>",
            $">... [typing]\r\n>... {SpecialTerminalCommand} entered.\r\n>... <size=200%><color=red>Executed!</color></size>",
            ">... [frantic typing]\r\n>... <size=200%><color=red>Command in!</color></size>\r\n>... [mechanical sounds]",
            $">... [static]\r\n>... {SpecialTerminalCommand} ready.\r\n>... <size=200%><color=red>Execute!</color></size>",
            ">... Backdoor command executing.\r\n>... <size=200%><color=red>Processing!</color></size>\r\n>... [mechanical humming]",
            $">... {SpecialTerminalCommand}?\r\n>... <size=200%><color=red>That's the command!</color></size>\r\n>... [typing]",
            ">... [panting]\r\n>... Terminal found.\r\n>... <size=200%><color=red>Execute now!</color></size>",
            $">... Command is {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Ready to execute!</color></size>\r\n>... Go!",
            ">... [typing rapidly]\r\n>... Backdoor command entered.\r\n>... <size=200%><color=red>Here we go!</color></size>",
            $">... {SpecialTerminalCommand} executing.\r\n>... <size=200%><color=red>No stopping now!</color></size>\r\n>... [alarm blaring]",
            ">... Terminal's here!\r\n>... <size=200%><color=red>Execute the command!</color></size>\r\n>... Now!",
            $">... [typing]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Done!",
            ">... Backdoor access confirmed.\r\n>... Execute command.\r\n>... <size=200%><color=red>Do it!</color></size>",
            $">... [whispering]\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executing now!</color></size>",
            ">... Terminal interface up.\r\n>... <size=200%><color=red>Execute!</color></size>\r\n>... [typing]",
            $">... Command ready: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Entering!</color></size>\r\n>... [mechanical sounds]",
            ">... [frantic typing]\r\n>... Command entered!\r\n>... <size=200%><color=red>Processing!</color></size>",
            $">... {SpecialTerminalCommand} ready.\r\n>... <size=200%><color=red>Execute now!</color></size>\r\n>... Done!",
            ">... [static]\r\n>... Terminal accessed.\r\n>... <size=200%><color=red>Execute!</color></size>",
            $">... [panting]\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Entering!</color></size>",
            ">... Backdoor command located.\r\n>... <size=200%><color=red>Type it!</color></size>\r\n>... [typing]",
            $">... {SpecialTerminalCommand} entered.\r\n>... <size=200%><color=red>Here we go!</color></size>\r\n>... [mechanical humming]",
            ">... Terminal's here.\r\n>... Execute the command.\r\n>... <size=200%><color=red>Now!</color></size>",
            $">... [typing rapidly]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Executed!",
            ">... Backdoor access granted.\r\n>... <size=200%><color=red>Ready to execute!</color></size>\r\n>... Go!",
            $">... Command: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executing!</color></size>\r\n>... [alarm sounds]",
            ">... [whispering] Terminal located.\r\n>... <size=200%><color=red>Enter command!</color></size>\r\n>... On it!",
            $">... {SpecialTerminalCommand}.\r\n>... Unknown consequences.\r\n>... <size=200%><color=red>Execute anyway!</color></size>",
            ">... Terminal interface active.\r\n>... Backdoor command ready.\r\n>... <size=200%><color=red>Execute!</color></size>",
            $">... [typing]\r\n>... {SpecialTerminalCommand} entered.\r\n>... <size=200%><color=red>Done!</color></size>",
            ">... [frantic typing]\r\n>... <size=200%><color=red>Command in!</color></size>\r\n>... [mechanical sounds]",
            $">... [static]\r\n>... {SpecialTerminalCommand} ready.\r\n>... <size=200%><color=red>Go!</color></size>",
            ">... Backdoor command executing.\r\n>... <size=200%><color=red>Processing!</color></size>\r\n>... [mechanical humming]",
            $">... {SpecialTerminalCommand}?\r\n>... <size=200%><color=red>Execute it!</color></size>\r\n>... [typing]",
            ">... [panting]\r\n>... Terminal found.\r\n>... <size=200%><color=red>Execute!</color></size>",
            $">... Command is {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Ready!</color></size>\r\n>... Execute!",
            ">... [typing rapidly]\r\n>... Backdoor command entered.\r\n>... <size=200%><color=red>Here we go!</color></size>",
            $">... {SpecialTerminalCommand} executing.\r\n>... <size=200%><color=red>What's it do?!</color></size>\r\n>... [alarm blaring]",
            ">... Terminal's here!\r\n>... <size=200%><color=red>Execute command!</color></size>\r\n>... Now!",
            $">... [typing]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Executed!",
            ">... Backdoor access confirmed.\r\n>... Execute the command.\r\n>... <size=200%><color=red>Do it!</color></size>",
            $">... [whispering]\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executing!</color></size>",
            ">... Terminal interface up.\r\n>... <size=200%><color=red>Execute!</color></size>\r\n>... [typing]",
            $">... Command ready: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Entering now!</color></size>\r\n>... Done!",
            ">... [frantic typing]\r\n>... Command entered!\r\n>... <size=200%><color=red>Processing!</color></size>",
            $">... {SpecialTerminalCommand} ready.\r\n>... <size=200%><color=red>Execute!</color></size>\r\n>... [mechanical sounds]",
            ">... [static]\r\n>... Terminal accessed.\r\n>... <size=200%><color=red>Go!</color></size>",
            $">... [panting]\r\n>... {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Entering!</color></size>",
            ">... Backdoor command located.\r\n>... <size=200%><color=red>Type it in!</color></size>\r\n>... [typing]",
            $">... {SpecialTerminalCommand} entered.\r\n>... <size=200%><color=red>Here we go!</color></size>\r\n>... [mechanical humming]",
            ">... Terminal's here.\r\n>... Execute backdoor command.\r\n>... <size=200%><color=red>Now!</color></size>",
            $">... [typing rapidly]\r\n>... <size=200%><color=red>{SpecialTerminalCommand}!</color></size>\r\n>... Done!",
            ">... Backdoor access granted.\r\n>... <size=200%><color=red>Ready!</color></size>\r\n>... Execute!",
            $">... Command: {SpecialTerminalCommand}.\r\n>... <size=200%><color=red>Executing!</color></size>\r\n>... [alarm sounds]",
            ">... [whispering] Terminal located.\r\n>... <size=200%><color=red>Enter the command!</color></size>\r\n>... On it!",
            $">... {SpecialTerminalCommand}.\r\n>... What will happen?\r\n>... <size=200%><color=red>Execute!</color></size>",
            ">... Terminal interface active.\r\n>... Backdoor command ready.\r\n>... <size=200%><color=red>Go!</color></size>",
        }))!);

        if (SpecialTerminalCommand_Type == SpecialCommand.LightsOff)
        {
            // Intel for terminal command that shuts off all the lights/power in the level
            level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
            {
                ">... Lights are going out!\r\n>... <size=200%><color=red>Flashlights on!</color></size>\r\n>... [electrical humming]",
                ">... Can't see anything!\r\n>... Everything's dark!\r\n>... <size=200%><color=red>Stay together!</color></size>",
                ">... Power's gone.\r\n>... <size=200%><color=red>All the lights!</color></size>\r\n>... [darkness]",
                ">... [static]\r\n>... <size=200%><color=red>Lights out!</color></size>\r\n>... Complete darkness!",
                ">... Flashlight!\r\n>... Turn it on!\r\n>... <size=200%><color=red>Can't see!</color></size>",
                ">... Everything went dark!\r\n>... <size=200%><color=red>Power's severed!</color></size>\r\n>... [electrical crackling]",
                ">... [panting]\r\n>... Can't see them!\r\n>... <size=200%><color=red>Where are they?!</color></size>",
                ">... Lights are out!\r\n>... <size=200%><color=red>Flashlights!</color></size>\r\n>... Now!",
                ">... Complete darkness.\r\n>... Turn on your lights.\r\n>... <size=200%><color=red>Stay close!</color></size>",
                ">... [whispering]\r\n>... It's so dark.\r\n>... <size=200%><color=red>Can barely see!</color></size>",
                ">... Power failure!\r\n>... <size=200%><color=red>All lights down!</color></size>\r\n>... [electrical hum stops]",
                ">... Flashlight beam's all we have.\r\n>... <size=200%><color=red>Don't lose sight!</color></size>\r\n>... Stay together!",
                ">... [darkness]\r\n>... <size=200%><color=red>Turn on your light!</color></size>\r\n>... Can't see!",
                ">... Everything's black!\r\n>... Power's gone!\r\n>... <size=200%><color=red>Flashlights!</color></size>",
                ">... Can't see the path!\r\n>... <size=200%><color=red>Use your flashlight!</color></size>\r\n>... [panting]",
                ">... [static]\r\n>... Lights severed.\r\n>... <size=200%><color=red>Darkness everywhere!</color></size>",
                ">... No visibility!\r\n>... <size=200%><color=red>Flashlights on!</color></size>\r\n>... Now!",
                ">... Power cut!\r\n>... Everything's dark!\r\n>... <size=200%><color=red>Stay together!</color></size>",
                ">... [whispering] Can't see anything.\r\n>... Turn on lights.\r\n>... <size=200%><color=red>Careful!</color></size>",
                ">... Lights are gone!\r\n>... <size=200%><color=red>Complete darkness!</color></size>\r\n>... [electrical crackling]",
                ">... [panting]\r\n>... It's so dark!\r\n>... <size=200%><color=red>Flashlights!</color></size>",
                ">... Can't navigate!\r\n>... Too dark!\r\n>... <size=200%><color=red>Use your light!</color></size>",
                ">... Flashlight beam only.\r\n>... <size=200%><color=red>That's all we have!</color></size>\r\n>... Stay close!",
                ">... [darkness]\r\n>... Power's severed.\r\n>... <size=200%><color=red>Lights gone!</color></size>",
                ">... Everything went black!\r\n>... <size=200%><color=red>Turn on flashlights!</color></size>\r\n>... [static]",
                ">... Can't see them in the dark!\r\n>... <size=200%><color=red>Where are they?!</color></size>\r\n>... [whispering]",
                ">... Lights out everywhere!\r\n>... <size=200%><color=red>Flashlights!</color></size>\r\n>... Now!",
                ">... Power failure.\r\n>... Complete darkness.\r\n>... <size=200%><color=red>Stay together!</color></size>",
                ">... [static]\r\n>... <size=200%><color=red>All lights down!</color></size>\r\n>... [electrical humming]",
                ">... Can barely see!\r\n>... Flashlight's dim!\r\n>... <size=200%><color=red>Stay close!</color></size>",
                ">... Everything's black!\r\n>... <size=200%><color=red>Power's gone!</color></size>\r\n>... Use lights!",
                ">... [panting]\r\n>... Too dark to see!\r\n>... <size=200%><color=red>Flashlights on!</color></size>",
                ">... Lights severed!\r\n>... Complete blackout!\r\n>... <size=200%><color=red>Stay together!</color></size>",
                ">... [whispering] It's so dark.\r\n>... <size=200%><color=red>Can't see!</color></size>\r\n>... Use your light!",
                ">... Power's cut!\r\n>... <size=200%><color=red>All lights gone!</color></size>\r\n>... [electrical crackling]",
                ">... Flashlight!\r\n>... That's all we have!\r\n>... <size=200%><color=red>Stay close!</color></size>",
                ">... [darkness]\r\n>... <size=200%><color=red>Can't see anything!</color></size>\r\n>... Turn on lights!",
                ">... Everything went dark!\r\n>... Power failure!\r\n>... <size=200%><color=red>Flashlights!</color></size>",
                ">... Can't see the way!\r\n>... <size=200%><color=red>Too dark!</color></size>\r\n>... [panting]",
                ">... [static]\r\n>... Lights are out.\r\n>... <size=200%><color=red>Darkness!</color></size>",
                ">... No visibility!\r\n>... <size=200%><color=red>Use flashlights!</color></size>\r\n>... Stay together!",
                ">... Power severed!\r\n>... Everything's black!\r\n>... <size=200%><color=red>Lights on!</color></size>",
                ">... [whispering] Can't see.\r\n>... Flashlight's all we have.\r\n>... <size=200%><color=red>Stay close!</color></size>",
                ">... Lights gone!\r\n>... <size=200%><color=red>Complete darkness!</color></size>\r\n>... [electrical hum stops]",
                ">... [panting]\r\n>... So dark!\r\n>... <size=200%><color=red>Turn on lights!</color></size>",
                ">... Can't navigate in this!\r\n>... Too dark!\r\n>... <size=200%><color=red>Flashlights!</color></size>",
                ">... Flashlight beam only.\r\n>... <size=200%><color=red>Stay together!</color></size>\r\n>... Don't separate!",
                ">... [darkness]\r\n>... Power failure.\r\n>... <size=200%><color=red>Lights out!</color></size>",
                ">... Everything's black!\r\n>... <size=200%><color=red>Flashlights on!</color></size>\r\n>... [static]",
                ">... Can't see them!\r\n>... Dark everywhere!\r\n>... <size=200%><color=red>Where?!</color></size>",
                ">... Lights out!\r\n>... <size=200%><color=red>All of them!</color></size>\r\n>... Use flashlights!",
                ">... Power cut.\r\n>... Darkness.\r\n>... <size=200%><color=red>Stay together!</color></size>",
                ">... [static]\r\n>... <size=200%><color=red>Lights down!</color></size>\r\n>... [electrical crackling]",
                ">... Barely see!\r\n>... Flashlight's weak!\r\n>... <size=200%><color=red>Stay close!</color></size>",
                ">... Everything dark!\r\n>... <size=200%><color=red>Power gone!</color></size>\r\n>... Lights!",
                ">... [panting]\r\n>... Can't see!\r\n>... <size=200%><color=red>Flashlights!</color></size>",
                ">... Lights severed!\r\n>... Blackout!\r\n>... <size=200%><color=red>Together!</color></size>",
                ">... [whispering] Dark.\r\n>... <size=200%><color=red>Can't see!</color></size>\r\n>... Light!",
                ">... Power failure!\r\n>... <size=200%><color=red>Lights gone!</color></size>\r\n>... [electrical hum]",
                ">... Flashlight only!\r\n>... That's it!\r\n>... <size=200%><color=red>Stay close!</color></size>",
                ">... [darkness]\r\n>... <size=200%><color=red>Can't see!</color></size>\r\n>... Lights!",
                ">... Went dark!\r\n>... Power cut!\r\n>... <size=200%><color=red>Flashlights!</color></size>",
                ">... Can't see path!\r\n>... <size=200%><color=red>Too dark!</color></size>\r\n>... [panting]",
                ">... [static]\r\n>... Lights out.\r\n>... <size=200%><color=red>Darkness!</color></size>",
                ">... No light!\r\n>... <size=200%><color=red>Flashlights!</color></size>\r\n>... Together!",
                ">... Power severed!\r\n>... Black!\r\n>... <size=200%><color=red>Lights!</color></size>",
                ">... [whispering] Dark.\r\n>... Flashlight.\r\n>... <size=200%><color=red>Close!</color></size>",
                ">... Lights gone!\r\n>... <size=200%><color=red>Darkness!</color></size>\r\n>... [electrical stops]",
                ">... [panting]\r\n>... Dark!\r\n>... <size=200%><color=red>Lights!</color></size>",
                ">... Can't navigate!\r\n>... Dark!\r\n>... <size=200%><color=red>Flashlights!</color></size>",
                ">... Flashlight beam.\r\n>... <size=200%><color=red>Together!</color></size>\r\n>... Don't separate!",
                ">... [darkness]\r\n>... Power cut.\r\n>... <size=200%><color=red>Out!</color></size>",
                ">... Black!\r\n>... <size=200%><color=red>Flashlights!</color></size>\r\n>... [static]",
                ">... Can't see!\r\n>... Dark!\r\n>... <size=200%><color=red>Where?!</color></size>",
                ">... Lights out!\r\n>... <size=200%><color=red>All!</color></size>\r\n>... Flashlights!",
                ">... Power off.\r\n>... Dark.\r\n>... <size=200%><color=red>Together!</color></size>",
                ">... [static]\r\n>... <size=200%><color=red>Down!</color></size>\r\n>... [electrical]",
                ">... Dim!\r\n>... Weak!\r\n>... <size=200%><color=red>Close!</color></size>",
                ">... Dark!\r\n>... <size=200%><color=red>Gone!</color></size>\r\n>... Light!",
                ">... [panting]\r\n>... Dark!\r\n>... <size=200%><color=red>On!</color></size>",
                ">... Severed!\r\n>... Out!\r\n>... <size=200%><color=red>Together!</color></size>",
                ">... Pitch black everywhere!\r\n>... <size=200%><color=red>Flashlights now!</color></size>\r\n>... Can't see anything!",
                ">... Lost all visibility!\r\n>... Power's completely gone!\r\n>... <size=200%><color=red>Stay together!</color></size>",
                ">... [darkness descending]\r\n>... Lights failing!\r\n>... <size=200%><color=red>Turn on flashlights!</color></size>",
                ">... Can't see my hand!\r\n>... <size=200%><color=red>Complete blackout!</color></size>\r\n>... [whispering]",
                ">... Navigation impossible!\r\n>... Too dark!\r\n>... <size=200%><color=red>Use your lights!</color></size>",
                ">... [electrical failure]\r\n>... <size=200%><color=red>All power gone!</color></size>\r\n>... Darkness everywhere!",
                ">... Flashlights our only hope!\r\n>... <size=200%><color=red>Don't lose each other!</color></size>\r\n>... Stay close!",
                ">... Lights extinguished!\r\n>... Complete darkness!\r\n>... <size=200%><color=red>Flashlights!</color></size>",
                ">... [panting heavily]\r\n>... Can't see the floor!\r\n>... <size=200%><color=red>Slow down!</color></size>",
                ">... Power grid severed!\r\n>... <size=200%><color=red>Everything's dark!</color></size>\r\n>... [electrical crackling]",
                ">... Visibility zero!\r\n>... Flashlights only!\r\n>... <size=200%><color=red>Stay together!</color></size>",
                ">... [whispering frantically]\r\n>... Lost in darkness!\r\n>... <size=200%><color=red>Where are you?!</color></size>",
                ">... All illumination gone!\r\n>... <size=200%><color=red>Flashlights on!</color></size>\r\n>... [darkness]",
                ">... Can't see enemies!\r\n>... Dark everywhere!\r\n>... <size=200%><color=red>Listen for them!</color></size>",
                ">... [electrical hum dies]\r\n>... <size=200%><color=red>Power's dead!</color></size>\r\n>... Complete blackout!",
                ">... Flashlight batteries!\r\n>... How long do we have?!\r\n>... <size=200%><color=red>Conserve light!</color></size>",
                ">... Lost visual contact!\r\n>... <size=200%><color=red>Stay together!</color></size>\r\n>... [panting]",
                ">... Darkness enveloping everything!\r\n>... Power's gone!\r\n>... <size=200%><color=red>Lights on!</color></size>",
                ">... [static]\r\n>... Can't see navigation markers!\r\n>... <size=200%><color=red>We're blind!</color></size>",
                ">... Flashlight beams crossing!\r\n>... <size=200%><color=red>Don't separate!</color></size>\r\n>... Stay close!",
                ">... Complete power failure!\r\n>... <size=200%><color=red>Lights out everywhere!</color></size>\r\n>... [electrical crackling]",
                ">... [whispering]\r\n>... Too dark to navigate.\r\n>... <size=200%><color=red>Move slowly!</color></size>",
                ">... All lights extinguished!\r\n>... Flashlights!\r\n>... <size=200%><color=red>Now!</color></size>",
                ">... [panting]\r\n>... Pitch black!\r\n>... <size=200%><color=red>Can't see anything!</color></size>",
                ">... Power completely severed!\r\n>... <size=200%><color=red>Darkness!</color></size>\r\n>... Use flashlights!",
                ">... Visibility eliminated!\r\n>... Everything's black!\r\n>... <size=200%><color=red>Stay together!</color></size>",
                ">... [electrical failure]\r\n>... <size=200%><color=red>All lights down!</color></size>\r\n>... [darkness]",
                ">... Flashlight's all we got!\r\n>... <size=200%><color=red>Don't lose it!</color></size>\r\n>... Critical!",
                ">... Lost all visual!\r\n>... Dark everywhere!\r\n>... <size=200%><color=red>Lights on!</color></size>",
                ">... [whispering frantically]\r\n>... Can't see the path!\r\n>... <size=200%><color=red>Slow down!</color></size>",
                ">... Power grid failure!\r\n>... <size=200%><color=red>Everything dark!</color></size>\r\n>... [electrical crackling]",
                ">... Complete blackout!\r\n>... Flashlights!\r\n>... <size=200%><color=red>Stay together!</color></size>",
                ">... [static]\r\n>... Lost in darkness!\r\n>... <size=200%><color=red>Where's the team?!</color></size>",
                ">... All light gone!\r\n>... <size=200%><color=red>Flashlights now!</color></size>\r\n>... [darkness descending]",
                ">... Can't see threats!\r\n>... Too dark!\r\n>... <size=200%><color=red>Listen!</color></size>",
                ">... [electrical dies]\r\n>... <size=200%><color=red>Power's out!</color></size>\r\n>... Blackout!",
                ">... Flashlight failing!\r\n>... Battery low!\r\n>... <size=200%><color=red>Conserve!</color></size>",
                ">... Lost visual!\r\n>... <size=200%><color=red>Stay together!</color></size>\r\n>... [panting]",
                ">... Darkness consuming everything!\r\n>... Gone!\r\n>... <size=200%><color=red>Lights!</color></size>",
                ">... [static]\r\n>... Can't see markers!\r\n>... <size=200%><color=red>Blind!</color></size>",
                ">... Beams crossing!\r\n>... <size=200%><color=red>Don't separate!</color></size>\r\n>... Close!",
                ">... Power failure!\r\n>... <size=200%><color=red>Everywhere!</color></size>\r\n>... [crackling]",
                ">... [whispering]\r\n>... Dark.\r\n>... <size=200%><color=red>Slow!</color></size>",
                ">... Extinguished!\r\n>... Lights!\r\n>... <size=200%><color=red>Now!</color></size>",
                ">... [panting]\r\n>... Black!\r\n>... <size=200%><color=red>See nothing!</color></size>",
                ">... Severed!\r\n>... <size=200%><color=red>Dark!</color></size>\r\n>... Flashlights!",
                ">... Eliminated!\r\n>... Black!\r\n>... <size=200%><color=red>Together!</color></size>",
                ">... [failure]\r\n>... <size=200%><color=red>Down!</color></size>\r\n>... [darkness]",
                ">... All!\r\n>... <size=200%><color=red>Lost!</color></size>\r\n>... Critical!",
                ">... Visual gone!\r\n>... Dark!\r\n>... <size=200%><color=red>On!</color></size>",
                ">... [whispering]\r\n>... Path!\r\n>... <size=200%><color=red>Slow!</color></size>",
                ">... Grid!\r\n>... <size=200%><color=red>Dark!</color></size>\r\n>... [crackling]",
            }))!);
        }

        if (SpecialTerminalCommand_Type == SpecialCommand.FillWithFog)
        {
            // Intel for terminal command that shuts off the ventilation and slowly fills the level with fog
            // TODO: separate infectious ones only if level has infectious fog
            level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
            {
                ">... [coughing]\r\n>... Fog's filling the zones!\r\n>... <size=200%><color=red>Ventilation's down!</color></size>",
                ">... Can't breathe!\r\n>... <size=200%><color=red>Infection rising!</color></size>\r\n>... Need disinfection!",
                ">... Fog everywhere!\r\n>... Can't see them!\r\n>... <size=200%><color=red>Where are they?!</color></size>",
                ">... [coughing heavily]\r\n>... <size=200%><color=red>Ventilation offline!</color></size>\r\n>... Fog filling!",
                ">... Infection meter rising!\r\n>... Fog's spreading!\r\n>... <size=200%><color=red>Get to disinfection!</color></size>",
                ">... [wheezing]\r\n>... Can't see through this!\r\n>... <size=200%><color=red>Fog's too thick!</color></size>",
                ">... Fog rolling in!\r\n>... <size=200%><color=red>Ventilation's failed!</color></size>\r\n>... [coughing]",
                ">... Infection at fifty percent!\r\n>... <size=200%><color=red>Need disinfection station!</color></size>\r\n>... Where?!",
                ">... [static]\r\n>... Fog spreading!\r\n>... <size=200%><color=red>Vents are down!</color></size>",
                ">... Can barely breathe!\r\n>... Fog's everywhere!\r\n>... <size=200%><color=red>Get out!</color></size>",
                ">... Ventilation system offline!\r\n>... <size=200%><color=red>Fog filling zones!</color></size>\r\n>... [coughing]",
                ">... [wheezing heavily]\r\n>... Infection rising fast!\r\n>... <size=200%><color=red>Disinfect!</color></size>",
                ">... Fog's thick!\r\n>... Can't see!\r\n>... <size=200%><color=red>Stay together!</color></size>",
                ">... [coughing]\r\n>... <size=200%><color=red>Ventilation failed!</color></size>\r\n>... Fog spreading!",
                ">... Infection at seventy percent!\r\n>... <size=200%><color=red>Find disinfection!</color></size>\r\n>... Hurry!",
                ">... Fog repellers!\r\n>... Need them!\r\n>... <size=200%><color=red>Where?!</color></size>",
                ">... [wheezing]\r\n>... Fog's filling everything!\r\n>... <size=200%><color=red>Can't breathe!</color></size>",
                ">... Ventilation's gone!\r\n>... <size=200%><color=red>Fog everywhere!</color></size>\r\n>... [coughing]",
                ">... Infection meter critical!\r\n>... <size=200%><color=red>Disinfection now!</color></size>\r\n>... [panting]",
                ">... [static]\r\n>... Fog spreading fast!\r\n>... <size=200%><color=red>Vents failed!</color></size>",
                ">... Can't see through fog!\r\n>... They're in here!\r\n>... <size=200%><color=red>Where?!</color></size>",
                ">... Fog's toxic!\r\n>... <size=200%><color=red>Infection rising!</color></size>\r\n>... [coughing]",
                ">... [wheezing]\r\n>... Need fog repeller!\r\n>... <size=200%><color=red>Deploy it!</color></size>",
                ">... Ventilation offline!\r\n>... Fog filling!\r\n>... <size=200%><color=red>Get out!</color></size>",
                ">... Infection at ninety percent!\r\n>... <size=200%><color=red>Disinfect!</color></size>\r\n>... Now!",
                ">... [coughing]\r\n>... Fog's everywhere!\r\n>... <size=200%><color=red>Can't breathe!</color></size>",
                ">... Fog rolling through zones!\r\n>... <size=200%><color=red>Vents are down!</color></size>\r\n>... [wheezing]",
                ">... Can barely see!\r\n>... Fog's thick!\r\n>... <size=200%><color=red>Stay close!</color></size>",
                ">... [static]\r\n>... <size=200%><color=red>Ventilation failed!</color></size>\r\n>... Fog spreading!",
                ">... Infection rising!\r\n>... Need station!\r\n>... <size=200%><color=red>Where?!</color></size>",
                ">... Fog repeller active!\r\n>... <size=200%><color=red>Stay near it!</color></size>\r\n>... Don't leave!",
                ">... [coughing heavily]\r\n>... Fog's toxic!\r\n>... <size=200%><color=red>Get out!</color></size>",
                ">... Ventilation's dead!\r\n>... <size=200%><color=red>Fog filling!</color></size>\r\n>... [wheezing]",
                ">... Infection critical!\r\n>... <size=200%><color=red>Disinfection!</color></size>\r\n>... [panting]",
                ">... [static]\r\n>... Fog everywhere!\r\n>... <size=200%><color=red>Vents down!</color></size>",
                ">... Can't see them!\r\n>... Fog!\r\n>... <size=200%><color=red>Where?!</color></size>",
                ">... Fog spreading!\r\n>... <size=200%><color=red>Infection rising!</color></size>\r\n>... [coughing]",
                ">... [wheezing]\r\n>... Repeller!\r\n>... <size=200%><color=red>Deploy!</color></size>",
                ">... Vents offline!\r\n>... Fog!\r\n>... <size=200%><color=red>Out!</color></size>",
                ">... Infection high!\r\n>... <size=200%><color=red>Disinfect!</color></size>\r\n>... Now!",
                ">... [coughing]\r\n>... Fog!\r\n>... <size=200%><color=red>Breathe!</color></size>",
                ">... Fog zones!\r\n>... <size=200%><color=red>Vents down!</color></size>\r\n>... [wheezing]",
                ">... Thick!\r\n>... Fog!\r\n>... <size=200%><color=red>Close!</color></size>",
                ">... Fog creeping in!\r\n>... Ventilation failing!\r\n>... <size=200%><color=red>Move fast!</color></size>",
                ">... [coughing]\r\n>... Infection meter spiking!\r\n>... <size=200%><color=red>Need disinfection!</color></size>",
                ">... Fog turbine needed!\r\n>... <size=200%><color=red>Find one!</color></size>\r\n>... [wheezing]",
                ">... Can't see path!\r\n>... Fog's too dense!\r\n>... <size=200%><color=red>Stay together!</color></size>",
                ">... [static]\r\n>... Ventilation system down!\r\n>... <size=200%><color=red>Fog filling!</color></size>",
                ">... Breathing's difficult!\r\n>... <size=200%><color=red>Infection rising!</color></size>\r\n>... [coughing]",
                ">... Fog bank ahead!\r\n>... Push through!\r\n>... <size=200%><color=red>Hold breath!</color></size>",
                ">... [wheezing]\r\n>... Fog's suffocating!\r\n>... <size=200%><color=red>Get clear!</color></size>",
                ">... Vents shut down!\r\n>... <size=200%><color=red>Fog spreading!</color></size>\r\n>... [coughing]",
                ">... Infection at threshold!\r\n>... Disinfect!\r\n>... <size=200%><color=red>Now!</color></size>",
                ">... [panting]\r\n>... Fog's consuming zones!\r\n>... <size=200%><color=red>Move!</color></size>",
                ">... Repeller's working!\r\n>... <size=200%><color=red>Stay in range!</color></size>\r\n>... Don't wander!",
                ">... [coughing]\r\n>... Can't breathe in this!\r\n>... <size=200%><color=red>Fog's thick!</color></size>",
                ">... Ventilation offline!\r\n>... Fog's spreading!\r\n>... <size=200%><color=red>Evacuate!</color></size>",
                ">... Infection rising steadily!\r\n>... <size=200%><color=red>Find station!</color></size>\r\n>... [wheezing]",
                ">... [static]\r\n>... Fog rolling through!\r\n>... <size=200%><color=red>Vents failed!</color></size>",
                ">... Lost visual in fog!\r\n>... <size=200%><color=red>Where's the team?!</color></size>\r\n>... [coughing]",
                ">... Fog's poisonous!\r\n>... <size=200%><color=red>Infection meter!</color></size>\r\n>... Critical!",
                ">... [wheezing heavily]\r\n>... Need turbine!\r\n>... <size=200%><color=red>Deploy!</color></size>",
                ">... Ventilation dead!\r\n>... Fog!\r\n>... <size=200%><color=red>Out!</color></size>",
                ">... Infection maxing!\r\n>... <size=200%><color=red>Disinfect!</color></size>\r\n>... Quick!",
                ">... [coughing]\r\n>... Fog!\r\n>... <size=200%><color=red>Air!</color></size>",
                ">... Fog zones expanding!\r\n>... <size=200%><color=red>Vents down!</color></size>\r\n>... [wheezing]",
                ">... Dense fog!\r\n>... <size=200%><color=red>Together!</color></size>\r\n>... Stay!",
                ">... Fog bank approaching!\r\n>... Get turbine!\r\n>... <size=200%><color=red>Fast!</color></size>",
                ">... [coughing]\r\n>... Infection climbing!\r\n>... <size=200%><color=red>Station!</color></size>",
                ">... Fog repeller deployed!\r\n>... <size=200%><color=red>Stay near!</color></size>\r\n>... Safe zone!",
                ">... Can't see anything!\r\n>... Fog's blinding!\r\n>... <size=200%><color=red>Where?!</color></size>",
                ">... [static]\r\n>... Ventilation failed!\r\n>... <size=200%><color=red>Fog!</color></size>",
                ">... Breathing hard!\r\n>... <size=200%><color=red>Infection!</color></size>\r\n>... [coughing]",
                ">... Fog ahead!\r\n>... Push!\r\n>... <size=200%><color=red>Hold!</color></size>",
                ">... [wheezing]\r\n>... Suffocating!\r\n>... <size=200%><color=red>Clear!</color></size>",
                ">... Vents down!\r\n>... <size=200%><color=red>Fog!</color></size>\r\n>... [coughing]",
                ">... Infection threshold!\r\n>... Disinfect!\r\n>... <size=200%><color=red>Now!</color></size>",
                ">... [panting]\r\n>... Consuming!\r\n>... <size=200%><color=red>Move!</color></size>",
                ">... Repeller working!\r\n>... <size=200%><color=red>Range!</color></size>\r\n>... Stay!",
                ">... [coughing]\r\n>... Thick!\r\n>... <size=200%><color=red>Fog!</color></size>",
                ">... Offline!\r\n>... Spreading!\r\n>... <size=200%><color=red>Out!</color></size>",
                ">... Rising!\r\n>... <size=200%><color=red>Station!</color></size>\r\n>... [wheezing]",
                ">... [static]\r\n>... Rolling!\r\n>... <size=200%><color=red>Failed!</color></size>",
                ">... Visual lost!\r\n>... <size=200%><color=red>Team?!</color></size>\r\n>... [coughing]",
                ">... Poisonous!\r\n>... <size=200%><color=red>Meter!</color></size>\r\n>... Critical!",
                ">... [wheezing]\r\n>... Turbine!\r\n>... <size=200%><color=red>Deploy!</color></size>",
                ">... Dead!\r\n>... Fog!\r\n>... <size=200%><color=red>Out!</color></size>",
                ">... Maxing!\r\n>... <size=200%><color=red>Disinfect!</color></size>\r\n>... Quick!",
                ">... Fog swallowing zones!\r\n>... Vents gone!\r\n>... <size=200%><color=red>Move through fast!</color></size>",
                ">... [coughing]\r\n>... Infection at eighty percent!\r\n>... <size=200%><color=red>Need disinfection now!</color></size>",
                ">... Fog turbine found!\r\n>... Deploy it!\r\n>... <size=200%><color=red>Clear the area!</color></size>",
                ">... Can't navigate!\r\n>... Fog's blinding!\r\n>... <size=200%><color=red>Follow voices!</color></size>",
                ">... [static]\r\n>... Ventilation compromised!\r\n>... <size=200%><color=red>Fog spreading!</color></size>",
                ">... Air quality failing!\r\n>... <size=200%><color=red>Infection rising!</color></size>\r\n>... [wheezing]",
                ">... Fog wall ahead!\r\n>... Quick dash!\r\n>... <size=200%><color=red>Don't stop!</color></size>",
                ">... [wheezing heavily]\r\n>... Fog's overwhelming!\r\n>... <size=200%><color=red>Get clear!</color></size>",
                ">... Ventilation offline!\r\n>... <size=200%><color=red>Fog filling fast!</color></size>\r\n>... [coughing]",
                ">... Infection nearing limit!\r\n>... Station ahead!\r\n>... <size=200%><color=red>Run!</color></size>",
                ">... [panting]\r\n>... Fog's consuming everything!\r\n>... <size=200%><color=red>Keep moving!</color></size>",
                ">... Repeller range limited!\r\n>... <size=200%><color=red>Stay inside!</color></size>\r\n>... Don't wander!",
                ">... [coughing]\r\n>... Can't take much more!\r\n>... <size=200%><color=red>Fog's toxic!</color></size>",
                ">... Vents completely down!\r\n>... Fog spreading!\r\n>... <size=200%><color=red>Evacuate zones!</color></size>",
                ">... Infection climbing fast!\r\n>... <size=200%><color=red>Find station quick!</color></size>\r\n>... [wheezing]",
                ">... [static]\r\n>... Fog engulfing zones!\r\n>... <size=200%><color=red>System failed!</color></size>",
                ">... Lost in fog bank!\r\n>... <size=200%><color=red>Where's extraction?!</color></size>\r\n>... [coughing]",
                ">... Fog's deadly!\r\n>... <size=200%><color=red>Infection critical!</color></size>\r\n>... Disinfect!",
                ">... [wheezing]\r\n>... Need turbine now!\r\n>... <size=200%><color=red>Deploy fast!</color></size>",
                ">... Ventilation gone!\r\n>... Fog!\r\n>... <size=200%><color=red>Move!</color></size>",
                ">... Infection maxed!\r\n>... <size=200%><color=red>Disinfect immediately!</color></size>\r\n>... Critical!",
                ">... [coughing]\r\n>... Fog!\r\n>... <size=200%><color=red>Clear!</color></size>",
                ">... Zones foggy!\r\n>... <size=200%><color=red>Down!</color></size>\r\n>... [wheezing]",
                ">... Dense!\r\n>... <size=200%><color=red>Stay!</color></size>\r\n>... Together!",
                ">... Fog bank!\r\n>... Turbine!\r\n>... <size=200%><color=red>Fast!</color></size>",
                ">... [coughing]\r\n>... Climbing!\r\n>... <size=200%><color=red>Station!</color></size>",
                ">... Deployed!\r\n>... <size=200%><color=red>Near!</color></size>\r\n>... Zone!",
                ">... Nothing!\r\n>... Blinding!\r\n>... <size=200%><color=red>Where?!</color></size>",
                ">... [static]\r\n>... Failed!\r\n>... <size=200%><color=red>Fog!</color></size>",
                ">... Hard!\r\n>... <size=200%><color=red>Infection!</color></size>\r\n>... [coughing]",
                ">... Ahead!\r\n>... Push!\r\n>... <size=200%><color=red>Hold!</color></size>",
                ">... [wheezing]\r\n>... Suffocate!\r\n>... <size=200%><color=red>Clear!</color></size>",
                ">... Down!\r\n>... <size=200%><color=red>Fog!</color></size>\r\n>... [coughing]",
                ">... Threshold!\r\n>... Disinfect!\r\n>... <size=200%><color=red>Now!</color></size>",
                ">... [panting]\r\n>... Consume!\r\n>... <size=200%><color=red>Move!</color></size>",
                ">... Working!\r\n>... <size=200%><color=red>Range!</color></size>\r\n>... Stay!",
                ">... [coughing]\r\n>... Thick!\r\n>... <size=200%><color=red>Fog!</color></size>",
                ">... Off!\r\n>... Spread!\r\n>... <size=200%><color=red>Out!</color></size>",
                ">... Rise!\r\n>... <size=200%><color=red>Station!</color></size>\r\n>... [wheezing]",
                ">... [static]\r\n>... Roll!\r\n>... <size=200%><color=red>Fail!</color></size>",
                ">... Lost!\r\n>... <size=200%><color=red>Team?!</color></size>\r\n>... [coughing]",
                ">... Poison!\r\n>... <size=200%><color=red>Meter!</color></size>\r\n>... Critical!",
                ">... [wheezing]\r\n>... Turbine!\r\n>... <size=200%><color=red>Deploy!</color></size>",
                ">... Dead!\r\n>... Fog!\r\n>... <size=200%><color=red>Out!</color></size>",
                ">... Max!\r\n>... <size=200%><color=red>Disinfect!</color></size>\r\n>... Quick!",
            }))!);
        }

        if (SpecialTerminalCommand_Type == SpecialCommand.ErrorAlarm)
        {
            // Intel for terminal command that causes an error alarm to persist for the rest of the level
            level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
            {
                ">... [alarm wailing]\r\n>... <size=200%><color=red>Can't shut it off!</color></size>\r\n>... It won't stop!",
                ">... They keep spawning!\r\n>... Alarm's stuck!\r\n>... <size=200%><color=red>No end!</color></size>",
                ">... Security system's broken!\r\n>... <size=200%><color=red>Error alarm!</color></size>\r\n>... [klaxon blaring]",
                ">... [alarm blaring]\r\n>... Won't deactivate!\r\n>... <size=200%><color=red>It's persistent!</color></size>",
                ">... They won't stop coming!\r\n>... <size=200%><color=red>Alarm's stuck!</color></size>\r\n>... [gunfire]",
                ">... Error alarm active!\r\n>... Can't turn it off!\r\n>... <size=200%><color=red>Continuous spawning!</color></size>",
                ">... [klaxon]\r\n>... <size=200%><color=red>System malfunction!</color></size>\r\n>... Alarm won't stop!",
                ">... They keep coming!\r\n>... No breaks!\r\n>... <size=200%><color=red>Endless!</color></size>",
                ">... Alarm's malfunctioning!\r\n>... <size=200%><color=red>Can't deactivate!</color></size>\r\n>... [alarm wailing]",
                ">... [static]\r\n>... Error alarm!\r\n>... <size=200%><color=red>Won't stop!</color></size>",
                ">... Continuous spawns!\r\n>... Alarm's stuck!\r\n>... <size=200%><color=red>No end!</color></size>",
                ">... [alarm blaring]\r\n>... <size=200%><color=red>Can't shut it down!</color></size>\r\n>... It's stuck!",
                ">... They're spawning continuously!\r\n>... <size=200%><color=red>Alarm won't stop!</color></size>\r\n>... [gunfire]",
                ">... Error alarm!\r\n>... Can't deactivate!\r\n>... <size=200%><color=red>It's stuck!</color></size>",
                ">... [klaxon blaring]\r\n>... System malfunction!\r\n>... <size=200%><color=red>Won't stop!</color></size>",
                ">... They keep coming!\r\n>... <size=200%><color=red>No breaks!</color></size>\r\n>... Endless!",
                ">... Alarm's broken!\r\n>... <size=200%><color=red>Can't turn it off!</color></size>\r\n>... [alarm wailing]",
                ">... [static]\r\n>... Error!\r\n>... <size=200%><color=red>Won't stop!</color></size>",
                ">... Spawning continuously!\r\n>... Stuck!\r\n>... <size=200%><color=red>No end!</color></size>",
                ">... [alarm]\r\n>... <size=200%><color=red>Shut it down!</color></size>\r\n>... Can't!",
                ">... Spawning!\r\n>... <size=200%><color=red>Won't stop!</color></size>\r\n>... [gunfire]",
                ">... Error!\r\n>... Deactivate!\r\n>... <size=200%><color=red>Stuck!</color></size>",
                ">... [klaxon]\r\n>... Malfunction!\r\n>... <size=200%><color=red>Stop!</color></size>",
                ">... Coming!\r\n>... <size=200%><color=red>Breaks!</color></size>\r\n>... Endless!",
                ">... Broken!\r\n>... <size=200%><color=red>Off!</color></size>\r\n>... [alarm]",
                ">... [static]\r\n>... Error!\r\n>... <size=200%><color=red>Stop!</color></size>",
                ">... Security alarm error!\r\n>... <size=200%><color=red>Won't deactivate!</color></size>\r\n>... [klaxon]",
                ">... Endless wave spawning!\r\n>... Alarm's stuck!\r\n>... <size=200%><color=red>Can't stop it!</color></size>",
                ">... [alarm wailing]\r\n>... System error!\r\n>... <size=200%><color=red>Alarm persistent!</color></size>",
                ">... They never stop!\r\n>... <size=200%><color=red>Error alarm!</color></size>\r\n>... [gunfire]",
                ">... Alarm malfunction!\r\n>... Can't shut down!\r\n>... <size=200%><color=red>It's broken!</color></size>",
                ">... [klaxon blaring]\r\n>... <size=200%><color=red>Alarm won't stop!</color></size>\r\n>... Error!",
                ">... Continuous enemy waves!\r\n>... <size=200%><color=red>Alarm's stuck!</color></size>\r\n>... No end!",
                ">... Error alarm active!\r\n>... <size=200%><color=red>Can't deactivate!</color></size>\r\n>... [alarm]",
                ">... [static]\r\n>... System error!\r\n>... <size=200%><color=red>Won't stop!</color></size>",
                ">... They keep spawning!\r\n>... Alarm error!\r\n>... <size=200%><color=red>Endless!</color></size>",
                ">... [alarm blaring]\r\n>... <size=200%><color=red>Can't turn it off!</color></size>\r\n>... Stuck!",
                ">... Wave after wave!\r\n>... <size=200%><color=red>Alarm won't stop!</color></size>\r\n>... [gunfire]",
                ">... Error alarm!\r\n>... Deactivation failed!\r\n>... <size=200%><color=red>It's stuck!</color></size>",
                ">... [klaxon]\r\n>... Malfunction!\r\n>... <size=200%><color=red>Won't stop!</color></size>",
                ">... They don't stop!\r\n>... <size=200%><color=red>No breaks!</color></size>\r\n>... Endless!",
                ">... Alarm broken!\r\n>... <size=200%><color=red>Can't shut it down!</color></size>\r\n>... [alarm]",
                ">... [static]\r\n>... Error!\r\n>... <size=200%><color=red>Stuck!</color></size>",
                ">... Spawning endlessly!\r\n>... Alarm!\r\n>... <size=200%><color=red>No end!</color></size>",
                ">... [alarm]\r\n>... <size=200%><color=red>Turn it off!</color></size>\r\n>... Can't!",
                ">... Waves!\r\n>... <size=200%><color=red>Stop!</color></size>\r\n>... [gunfire]",
                ">... Error!\r\n>... Off!\r\n>... <size=200%><color=red>Stuck!</color></size>",
                ">... [klaxon]\r\n>... Error!\r\n>... <size=200%><color=red>Stop!</color></size>",
                ">... Coming!\r\n>... <size=200%><color=red>End!</color></size>\r\n>... Never!",
                ">... Alarm malfunctioning!\r\n>... <size=200%><color=red>Won't deactivate!</color></size>\r\n>... [wailing]",
                ">... No way to stop it!\r\n>... Alarm's stuck!\r\n>... <size=200%><color=red>Error!</color></size>",
                ">... [alarm blaring]\r\n>... System's broken!\r\n>... <size=200%><color=red>Can't stop!</color></size>",
                ">... They're relentless!\r\n>... <size=200%><color=red>Error alarm!</color></size>\r\n>... [gunfire]",
                ">... Alarm won't shut down!\r\n>... Malfunction!\r\n>... <size=200%><color=red>It's broken!</color></size>",
                ">... [klaxon]\r\n>... <size=200%><color=red>Won't stop!</color></size>\r\n>... Error!",
                ">... Endless spawning!\r\n>... <size=200%><color=red>Alarm stuck!</color></size>\r\n>... No end!",
                ">... Error alarm!\r\n>... <size=200%><color=red>Can't stop it!</color></size>\r\n>... [alarm]",
                ">... [static]\r\n>... Broken!\r\n>... <size=200%><color=red>Won't stop!</color></size>",
                ">... They spawn continuously!\r\n>... Error!\r\n>... <size=200%><color=red>Endless!</color></size>",
                ">... [alarm]\r\n>... <size=200%><color=red>Shut down!</color></size>\r\n>... Can't!",
                ">... Wave spawning!\r\n>... <size=200%><color=red>Stop!</color></size>\r\n>... [gunfire]",
                ">... Error!\r\n>... Shut!\r\n>... <size=200%><color=red>Stuck!</color></size>",
                ">... [klaxon]\r\n>... System!\r\n>... <size=200%><color=red>Stop!</color></size>",
                ">... Spawns!\r\n>... <size=200%><color=red>Breaks!</color></size>\r\n>... No!",
                ">... Security alarm stuck!\r\n>... <size=200%><color=red>Can't deactivate!</color></size>\r\n>... [wailing]",
                ">... No stopping them!\r\n>... Alarm error!\r\n>... <size=200%><color=red>Continuous!</color></size>",
                ">... [alarm blaring]\r\n>... System malfunction!\r\n>... <size=200%><color=red>Won't stop!</color></size>",
                ">... They're never-ending!\r\n>... <size=200%><color=red>Error alarm!</color></size>\r\n>... [gunfire]",
                ">... Alarm won't deactivate!\r\n>... System error!\r\n>... <size=200%><color=red>It's stuck!</color></size>",
                ">... [klaxon blaring]\r\n>... <size=200%><color=red>Can't stop!</color></size>\r\n>... Broken!",
                ">... Perpetual spawning!\r\n>... <size=200%><color=red>Alarm stuck!</color></size>\r\n>... No end!",
                ">... Error alarm active!\r\n>... <size=200%><color=red>Won't stop!</color></size>\r\n>... [alarm]",
                ">... [static]\r\n>... Malfunction!\r\n>... <size=200%><color=red>Can't stop!</color></size>",
                ">... They keep pouring in!\r\n>... Error!\r\n>... <size=200%><color=red>Endless!</color></size>",
                ">... [alarm]\r\n>... <size=200%><color=red>Deactivate!</color></size>\r\n>... Can't!",
                ">... Constant waves!\r\n>... <size=200%><color=red>Stop!</color></size>\r\n>... [gunfire]",
                ">... Error!\r\n>... Down!\r\n>... <size=200%><color=red>Stuck!</color></size>",
                ">... [klaxon]\r\n>... Broken!\r\n>... <size=200%><color=red>Stop!</color></size>",
                ">... Waves!\r\n>... <size=200%><color=red>End!</color></size>\r\n>... Never!",
                ">... Alarm persisting!\r\n>... <size=200%><color=red>Won't shut down!</color></size>\r\n>... [wailing]",
                ">... Can't stop the alarm!\r\n>... Error!\r\n>... <size=200%><color=red>It's broken!</color></size>",
                ">... [alarm blaring]\r\n>... System's dead!\r\n>... <size=200%><color=red>Won't stop!</color></size>",
                ">... They never quit!\r\n>... <size=200%><color=red>Error alarm!</color></size>\r\n>... [gunfire]",
                ">... Alarm malfunction!\r\n>... Can't shut off!\r\n>... <size=200%><color=red>Stuck!</color></size>",
                ">... [klaxon]\r\n>... <size=200%><color=red>Stop it!</color></size>\r\n>... Error!",
                ">... Never-ending waves!\r\n>... <size=200%><color=red>Alarm stuck!</color></size>\r\n>... No end!",
                ">... Error alarm!\r\n>... <size=200%><color=red>Can't turn off!</color></size>\r\n>... [alarm]",
                ">... [static]\r\n>... Failed!\r\n>... <size=200%><color=red>Won't stop!</color></size>",
                ">... They flood in!\r\n>... Error!\r\n>... <size=200%><color=red>Endless!</color></size>",
                ">... [alarm]\r\n>... <size=200%><color=red>Stop!</color></size>\r\n>... Can't!",
                ">... Spawning!\r\n>... <size=200%><color=red>Stop!</color></size>\r\n>... [gunfire]",
                ">... Error!\r\n>... Off!\r\n>... <size=200%><color=red>No!</color></size>",
                ">... [klaxon]\r\n>... Error!\r\n>... <size=200%><color=red>Stop!</color></size>",
                ">... Waves!\r\n>... <size=200%><color=red>End!</color></size>\r\n>... No!",
                ">... Alarm won't cease!\r\n>... <size=200%><color=red>Can't deactivate!</color></size>\r\n>... [wailing]",
                ">... No stopping it!\r\n>... Alarm!\r\n>... <size=200%><color=red>Continuous!</color></size>",
                ">... [alarm]\r\n>... System!\r\n>... <size=200%><color=red>Won't stop!</color></size>",
                ">... Never stop!\r\n>... <size=200%><color=red>Error!</color></size>\r\n>... [gunfire]",
                ">... Alarm stuck!\r\n>... Error!\r\n>... <size=200%><color=red>Stuck!</color></size>",
                ">... [klaxon]\r\n>... <size=200%><color=red>Stop!</color></size>\r\n>... Broken!",
                ">... Perpetual!\r\n>... <size=200%><color=red>Stuck!</color></size>\r\n>... No end!",
                ">... Error!\r\n>... <size=200%><color=red>Stop!</color></size>\r\n>... [alarm]",
                ">... [static]\r\n>... Malfunction!\r\n>... <size=200%><color=red>Stop!</color></size>",
                ">... Pouring!\r\n>... Error!\r\n>... <size=200%><color=red>Endless!</color></size>",
                ">... [alarm]\r\n>... <size=200%><color=red>Off!</color></size>\r\n>... Can't!",
                ">... Waves!\r\n>... <size=200%><color=red>Stop!</color></size>\r\n>... [gunfire]",
                ">... Error!\r\n>... Down!\r\n>... <size=200%><color=red>No!</color></size>",
                ">... [klaxon]\r\n>... Broken!\r\n>... <size=200%><color=red>Stop!</color></size>",
                ">... Spawns!\r\n>... <size=200%><color=red>End!</color></size>\r\n>... Never!",
                ">... Alarm persists!\r\n>... <size=200%><color=red>Won't shut!</color></size>\r\n>... [wailing]",
                ">... Can't stop!\r\n>... Error!\r\n>... <size=200%><color=red>Broken!</color></size>",
                ">... [alarm]\r\n>... Dead!\r\n>... <size=200%><color=red>Stop!</color></size>",
                ">... Quit!\r\n>... <size=200%><color=red>Error!</color></size>\r\n>... [gunfire]",
                ">... Malfunction!\r\n>... Off!\r\n>... <size=200%><color=red>Stuck!</color></size>",
                ">... [klaxon]\r\n>... <size=200%><color=red>Stop!</color></size>\r\n>... Error!",
                ">... Ending!\r\n>... <size=200%><color=red>Stuck!</color></size>\r\n>... No!",
                ">... Error!\r\n>... <size=200%><color=red>Off!</color></size>\r\n>... [alarm]",
                ">... [static]\r\n>... Failed!\r\n>... <size=200%><color=red>Stop!</color></size>",
                ">... Flood!\r\n>... Error!\r\n>... <size=200%><color=red>Endless!</color></size>",
                ">... [alarm]\r\n>... <size=200%><color=red>Stop!</color></size>\r\n>... No!",
                ">... Spawn!\r\n>... <size=200%><color=red>Stop!</color></size>\r\n>... [gunfire]",
                ">... Error!\r\n>... Off!\r\n>... <size=200%><color=red>No!</color></size>",
                ">... [klaxon]\r\n>... Error!\r\n>... <size=200%><color=red>Stop!</color></size>",
                ">... Wave!\r\n>... <size=200%><color=red>End!</color></size>\r\n>... No!",
            }))!);
        }

        if (SpecialTerminalCommand_Type == SpecialCommand.KingOfTheHill)
        {
            // Intel for terminal command that has a giant room scan in the middle of a big room
            // that players must defend against. King of the hill style
            level.ElevatorDropWardenIntel.Add((Generator.Between(1, 10), Generator.Draw(new List<string>
            {
                ">... Big scan starting!\r\n>... Hold this position!\r\n>... <size=200%><color=red>They're everywhere!</color></size>",
                ">... [heavy footsteps]\r\n>... <size=200%><color=red>Tank incoming!</color></size>\r\n>... Defend the scan!",
                ">... Get in the scan zone!\r\n>... <size=200%><color=red>Hold the line!</color></size>\r\n>... [gunfire intensifies]",
                ">... Scan's starting!\r\n>... <size=200%><color=red>Don't leave the area!</color></size>\r\n>... [bioscan active]",
                ">... They're pushing hard!\r\n>... Hold position!\r\n>... <size=200%><color=red>Don't break!</color></size>",
                ">... [screeching]\r\n>... <size=200%><color=red>Mother spawned!</color></size>\r\n>... Stay in the zone!",
                ">... Scan percentage climbing!\r\n>... <size=200%><color=red>Keep fighting!</color></size>\r\n>... [monsters roaring]",
                ">... Central room!\r\n>... <size=200%><color=red>Totally exposed!</color></size>\r\n>... [footsteps everywhere]",
                ">... Wave after wave!\r\n>... Stay in the scan!\r\n>... <size=200%><color=red>Hold it!</color></size>",
                ">... [growling]\r\n>... <size=200%><color=red>Pouncer!</color></size>\r\n>... Don't get grabbed!",
                ">... They won't stop coming!\r\n>... <size=200%><color=red>Defend the position!</color></size>\r\n>... [gunfire]",
                ">... Scan's at fifty percent!\r\n>... <size=200%><color=red>Halfway there!</color></size>\r\n>... Keep holding!",
                ">... Big room!\r\n>... <size=200%><color=red>No cover!</color></size>\r\n>... Defend the scan!",
                ">... [roaring]\r\n>... Tank's here!\r\n>... <size=200%><color=red>Focus fire!</color></size>",
                ">... Everyone in the zone!\r\n>... <size=200%><color=red>Stay together!</color></size>\r\n>... [scan humming]",
                ">... They're flanking!\r\n>... <size=200%><color=red>Watch all sides!</color></size>\r\n>... [footsteps]",
                ">... Scan won't finish!\r\n>... Someone's out of zone!\r\n>... <size=200%><color=red>Get back in!</color></size>",
                ">... [shrieking]\r\n>... <size=200%><color=red>Shadow!</color></size>\r\n>... Stay in position!",
                ">... Hold this ground!\r\n>... <size=200%><color=red>No retreat!</color></size>\r\n>... [combat sounds]",
                ">... Surrounded!\r\n>... Scan's still going!\r\n>... <size=200%><color=red>Keep fighting!</color></size>",
                ">... [heavy breathing]\r\n>... <size=200%><color=red>More waves!</color></size>\r\n>... Don't leave the zone!",
                ">... Central position!\r\n>... <size=200%><color=red>Completely open!</color></size>\r\n>... [approaching enemies]",
                ">... They're everywhere!\r\n>... Stay in the scan!\r\n>... <size=200%><color=red>Hold formation!</color></size>",
                ">... [biomass sounds]\r\n>... <size=200%><color=red>Mother incoming!</color></size>\r\n>... Defend the position!",
                ">... Scan's climbing!\r\n>... <size=200%><color=red>Don't break now!</color></size>\r\n>... [monsters closing]",
                ">... Giant room!\r\n>... Exposed on all sides!\r\n>... <size=200%><color=red>Hold the line!</color></size>",
                ">... [pounding footsteps]\r\n>... Tank's coming!\r\n>... <size=200%><color=red>Stay in zone!</color></size>",
                ">... Waves keep spawning!\r\n>... <size=200%><color=red>Defend the scan!</color></size>\r\n>... [combat]",
                ">... Scan at seventy percent!\r\n>... <size=200%><color=red>Almost there!</color></size>\r\n>... Keep holding!",
                ">... [growling close]\r\n>... <size=200%><color=red>Pouncer on me!</color></size>\r\n>... Stay in position!",
                ">... They're rushing!\r\n>... Hold the position!\r\n>... <size=200%><color=red>Don't scatter!</color></size>",
                ">... Big open room!\r\n>... <size=200%><color=red>No walls!</color></size>\r\n>... Defend the scan!",
                ">... [roaring]\r\n>... <size=200%><color=red>Giant incoming!</color></size>\r\n>... Stay in the zone!",
                ">... Everyone in position!\r\n>... <size=200%><color=red>Hold the ground!</color></size>\r\n>... [scan active]",
                ">... They're pushing from every angle!\r\n>... <size=200%><color=red>Watch your sectors!</color></size>\r\n>... [footsteps]",
                ">... Scan's progressing!\r\n>... Don't leave!\r\n>... <size=200%><color=red>Stay inside!</color></size>",
                ">... [teleporting sound]\r\n>... Shadow in the zone!\r\n>... <size=200%><color=red>Kill it fast!</color></size>",
                ">... Hold this spot!\r\n>... <size=200%><color=red>No falling back!</color></size>\r\n>... [gunfire]",
                ">... Completely surrounded!\r\n>... <size=200%><color=red>Keep the scan going!</color></size>\r\n>... [roaring]",
                ">... [heavy footsteps]\r\n>... <size=200%><color=red>Tank charging!</color></size>\r\n>... Don't leave the zone!",
                ">... Central room defense!\r\n>... Totally exposed!\r\n>... <size=200%><color=red>Hold position!</color></size>",
                ">... Wave's not stopping!\r\n>... <size=200%><color=red>Stay in the scan!</color></size>\r\n>... [combat sounds]",
                ">... [screeching]\r\n>... <size=200%><color=red>Mother spawning!</color></size>\r\n>... Defend the scan!",
                ">... Scan percentage rising!\r\n>... <size=200%><color=red>Keep it up!</color></size>\r\n>... [monsters approaching]",
                ">... Huge room!\r\n>... No protection!\r\n>... <size=200%><color=red>Hold the line!</color></size>",
                ">... [growling]\r\n>... Pouncer stalking!\r\n>... <size=200%><color=red>Watch out!</color></size>",
                ">... They keep coming!\r\n>... <size=200%><color=red>Defend the position!</color></size>\r\n>... [footsteps]",
                ">... Scan's halfway!\r\n>... <size=200%><color=red>Don't give up!</color></size>\r\n>... Keep holding!",
                ">... Open space!\r\n>... <size=200%><color=red>Everywhere's exposed!</color></size>\r\n>... [approaching]",
                ">... [roaring]\r\n>... <size=200%><color=red>Giant here!</color></size>\r\n>... Stay in zone!",
                ">... All in the scan area!\r\n>... <size=200%><color=red>Hold together!</color></size>\r\n>... [bioscan]",
                ">... Flanked on all sides!\r\n>... <size=200%><color=red>Cover your angles!</color></size>\r\n>... [combat]",
                ">... Scan's continuing!\r\n>... Someone's outside!\r\n>... <size=200%><color=red>Get in the zone!</color></size>",
                ">... [shrieking]\r\n>... <size=200%><color=red>Shadow teleporting!</color></size>\r\n>... Hold position!",
                ">... Defend this ground!\r\n>... <size=200%><color=red>No breaking!</color></size>\r\n>... [gunfire]",
                ">... Enemies everywhere!\r\n>... Scan's active!\r\n>... <size=200%><color=red>Stay inside!</color></size>",
                ">... [heavy breathing]\r\n>... Another wave!\r\n>... <size=200%><color=red>Keep fighting!</color></size>",
                ">... Big central room!\r\n>... <size=200%><color=red>No cover anywhere!</color></size>\r\n>... [footsteps]",
                ">... Surrounded completely!\r\n>... <size=200%><color=red>Hold the scan!</color></size>\r\n>... [roaring]",
                ">... [biomass]\r\n>... Mother's here!\r\n>... <size=200%><color=red>Don't leave!</color></size>",
                ">... Scan climbing!\r\n>... <size=200%><color=red>Almost done!</color></size>\r\n>... Keep holding!",
                ">... Giant room!\r\n>... Open from every side!\r\n>... <size=200%><color=red>Defend!</color></size>",
                ">... [pounding]\r\n>... <size=200%><color=red>Tank incoming!</color></size>\r\n>... Stay in position!",
                ">... Waves won't end!\r\n>... <size=200%><color=red>Hold the line!</color></size>\r\n>... [combat]",
                ">... Scan at eighty percent!\r\n>... <size=200%><color=red>Nearly there!</color></size>\r\n>... Don't break!",
                ">... [growling]\r\n>... <size=200%><color=red>Pouncer!</color></size>\r\n>... Stay in the zone!",
                ">... They're charging!\r\n>... Hold position!\r\n>... <size=200%><color=red>Don't run!</color></size>",
                ">... Massive room!\r\n>... <size=200%><color=red>No walls!</color></size>\r\n>... [approaching]",
                ">... [roaring]\r\n>... Giant's charging!\r\n>... <size=200%><color=red>Hold formation!</color></size>",
                ">... Everyone inside!\r\n>... <size=200%><color=red>Defend the scan!</color></size>\r\n>... [gunfire]",
                ">... Pushing from everywhere!\r\n>... <size=200%><color=red>Cover all sides!</color></size>\r\n>... [footsteps]",
                ">... Scan's going!\r\n>... Don't leave the zone!\r\n>... <size=200%><color=red>Stay close!</color></size>",
                ">... [teleporting]\r\n>... <size=200%><color=red>Shadow in here!</color></size>\r\n>... Defend position!",
                ">... Hold this area!\r\n>... <size=200%><color=red>No retreat!</color></size>\r\n>... [combat sounds]",
                ">... Fully surrounded!\r\n>... Scan's running!\r\n>... <size=200%><color=red>Keep holding!</color></size>",
                ">... [heavy footsteps]\r\n>... <size=200%><color=red>Tank!</color></size>\r\n>... Don't leave!",
                ">... Central position!\r\n>... All exposed!\r\n>... <size=200%><color=red>Hold the ground!</color></size>",
                ">... Wave after wave!\r\n>... <size=200%><color=red>Stay in the scan!</color></size>\r\n>... [roaring]",
                ">... [screeching]\r\n>... Mother's spawning!\r\n>... <size=200%><color=red>Defend!</color></size>",
                ">... Scan percentage up!\r\n>... <size=200%><color=red>Keep it going!</color></size>\r\n>... [monsters]",
                ">... Enormous room!\r\n>... No cover!\r\n>... <size=200%><color=red>Hold the line!</color></size>",
                ">... [growling close]\r\n>... <size=200%><color=red>Pouncer nearby!</color></size>\r\n>... Stay together!",
                ">... Endless waves!\r\n>... <size=200%><color=red>Hold position!</color></size>\r\n>... [combat]",
                ">... Scan's at sixty percent!\r\n>... <size=200%><color=red>Don't stop!</color></size>\r\n>... Keep fighting!",
                ">... Open everywhere!\r\n>... <size=200%><color=red>Totally exposed!</color></size>\r\n>... [footsteps]",
                ">... [roaring]\r\n>... Giant here!\r\n>... <size=200%><color=red>Focus it down!</color></size>",
                ">... All in the zone!\r\n>... <size=200%><color=red>Hold together!</color></size>\r\n>... [scan active]",
                ">... Flanked everywhere!\r\n>... <size=200%><color=red>Watch your back!</color></size>\r\n>... [approaching]",
                ">... Scan continuing!\r\n>... Someone's out!\r\n>... <size=200%><color=red>Get back in!</color></size>",
                ">... [shrieking]\r\n>... Shadow's here!\r\n>... <size=200%><color=red>Kill it!</color></size>",
                ">... Hold the ground!\r\n>... <size=200%><color=red>No breaking formation!</color></size>\r\n>... [gunfire]",
                ">... Enemies from every angle!\r\n>... <size=200%><color=red>Stay in the scan!</color></size>\r\n>... [roaring]",
                ">... [breathing hard]\r\n>... <size=200%><color=red>More coming!</color></size>\r\n>... Don't leave!",
                ">... Big room!\r\n>... <size=200%><color=red>No protection!</color></size>\r\n>... Defend the scan!",
                ">... Completely surrounded!\r\n>... Scan's active!\r\n>... <size=200%><color=red>Hold it!</color></size>",
                ">... [biomass sounds]\r\n>... <size=200%><color=red>Mother!</color></size>\r\n>... Stay in zone!",
                ">... Scan's climbing!\r\n>... <size=200%><color=red>Nearly done!</color></size>\r\n>... Keep holding!",
                ">... Massive room!\r\n>... Exposed everywhere!\r\n>... <size=200%><color=red>Hold the line!</color></size>",
                ">... [pounding footsteps]\r\n>... <size=200%><color=red>Tank charging!</color></size>\r\n>... Stay in position!",
                ">... Waves won't stop!\r\n>... <size=200%><color=red>Defend the scan!</color></size>\r\n>... [combat]",
                ">... Scan at ninety percent!\r\n>... <size=200%><color=red>Almost complete!</color></size>\r\n>... Don't break!",
                ">... [growling]\r\n>... Pouncer's here!\r\n>... <size=200%><color=red>Watch out!</color></size>",
                ">... They're rushing us!\r\n>... <size=200%><color=red>Hold position!</color></size>\r\n>... [footsteps]",
                ">... Huge open space!\r\n>... <size=200%><color=red>No walls!</color></size>\r\n>... Defend!",
                ">... [roaring]\r\n>... <size=200%><color=red>Giant incoming!</color></size>\r\n>... Hold the zone!",
                ">... Everyone in!\r\n>... <size=200%><color=red>Defend the position!</color></size>\r\n>... [scan humming]",
                ">... Attacking from all sides!\r\n>... <size=200%><color=red>Cover every angle!</color></size>\r\n>... [combat]",
                ">... Scan's going!\r\n>... Don't leave!\r\n>... <size=200%><color=red>Stay inside!</color></size>",
                ">... [teleporting sound]\r\n>... Shadow teleported!\r\n>... <size=200%><color=red>Kill it fast!</color></size>",
                ">... Hold this position!\r\n>... <size=200%><color=red>No falling back!</color></size>\r\n>... [gunfire]",
                ">... Surrounded on all sides!\r\n>... <size=200%><color=red>Keep the scan going!</color></size>\r\n>... [roaring]",
                ">... [heavy footsteps]\r\n>... Tank's here!\r\n>... <size=200%><color=red>Don't leave the zone!</color></size>",
                ">... Central room!\r\n>... Totally exposed!\r\n>... <size=200%><color=red>Hold the ground!</color></size>",
                ">... Waves keep spawning!\r\n>... <size=200%><color=red>Stay in the scan!</color></size>\r\n>... [combat sounds]",
                ">... [screeching]\r\n>... <size=200%><color=red>Mother incoming!</color></size>\r\n>... Defend the scan!",
                ">... Scan's rising!\r\n>... <size=200%><color=red>Keep fighting!</color></size>\r\n>... [monsters closing]",
                ">... Giant room!\r\n>... No cover anywhere!\r\n>... <size=200%><color=red>Hold the line!</color></size>",
                ">... [growling]\r\n>... <size=200%><color=red>Pouncer stalking!</color></size>\r\n>... Stay in zone!",
                ">... They won't stop!\r\n>... <size=200%><color=red>Hold position!</color></size>\r\n>... [footsteps]",
                ">... Scan's at forty percent!\r\n>... <size=200%><color=red>Keep holding!</color></size>\r\n>... Don't break!",
                ">... Open from all sides!\r\n>... <size=200%><color=red>Exposed everywhere!</color></size>\r\n>... [approaching]",
                ">... [roaring]\r\n>... <size=200%><color=red>Giant!</color></size>\r\n>... Stay together!",
                ">... All in the scan!\r\n>... <size=200%><color=red>Hold formation!</color></size>\r\n>... [bioscan]",
                ">... Flanked completely!\r\n>... <size=200%><color=red>Watch all angles!</color></size>\r\n>... [combat]",
                ">... Scan progressing!\r\n>... Someone's outside the zone!\r\n>... <size=200%><color=red>Get in!</color></size>",
                ">... [shrieking]\r\n>... Shadow's teleporting!\r\n>... <size=200%><color=red>Hold position!</color></size>",
                ">... Defend this ground!\r\n>... <size=200%><color=red>No retreating!</color></size>\r\n>... [gunfire]",
                ">... Enemies everywhere!\r\n>... Scan's running!\r\n>... <size=200%><color=red>Stay in!</color></size>",
                ">... [heavy breathing]\r\n>... <size=200%><color=red>Another wave!</color></size>\r\n>... Keep fighting!",
                ">... Big central space!\r\n>... <size=200%><color=red>No cover!</color></size>\r\n>... [footsteps]",
                ">... Totally surrounded!\r\n>... <size=200%><color=red>Hold the scan!</color></size>\r\n>... [roaring]",
                ">... [biomass]\r\n>... <size=200%><color=red>Mother's spawning!</color></size>\r\n>... Don't leave the zone!",
                ">... Scan's climbing!\r\n>... <size=200%><color=red>Almost there!</color></size>\r\n>... Keep holding!",
                ">... Massive open room!\r\n>... Exposed from every angle!\r\n>... <size=200%><color=red>Hold the line!</color></size>",
                ">... [pounding]\r\n>... Tank's charging!\r\n>... <size=200%><color=red>Stay in position!</color></size>",
                ">... Endless waves!\r\n>... <size=200%><color=red>Defend the scan!</color></size>\r\n>... [combat]",
                ">... Scan at ninety-five percent!\r\n>... <size=200%><color=red>Nearly complete!</color></size>\r\n>... Don't break now!",
                ">... [growling close]\r\n>... Pouncer on us!\r\n>... <size=200%><color=red>Stay in the zone!</color></size>",
                ">... They're charging hard!\r\n>... <size=200%><color=red>Hold the position!</color></size>\r\n>... [footsteps]",
                ">... Enormous room!\r\n>... <size=200%><color=red>No walls!</color></size>\r\n>... Defend the scan!",
                ">... [roaring]\r\n>... <size=200%><color=red>Giant here!</color></size>\r\n>... Hold formation!",
                ">... Everyone inside the zone!\r\n>... <size=200%><color=red>Defend together!</color></size>\r\n>... [scan active]",
                ">... Pushing from every direction!\r\n>... <size=200%><color=red>Cover all sides!</color></size>\r\n>... [combat sounds]",
                ">... Scan's active!\r\n>... Don't leave the zone!\r\n>... <size=200%><color=red>Stay close!</color></size>",
                ">... [teleporting]\r\n>... Shadow's in the zone!\r\n>... <size=200%><color=red>Kill it!</color></size>",
                ">... Hold this area!\r\n>... <size=200%><color=red>No retreat!</color></size>\r\n>... [gunfire intensifies]",
            }))!);
        }

        #endregion
    }
}
