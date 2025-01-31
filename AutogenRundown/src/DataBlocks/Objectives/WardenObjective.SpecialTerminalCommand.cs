using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

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
public partial record class WardenObjective : DataBlock
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

        MainObjective = "Find Computer terminal [ITEM_SERIAL] and input the backdoor command [SPECIAL_COMMAND]";
        FindLocationInfo = "Gather information about the location of [ITEM_SERIAL]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = "Navigate to [ITEM_ZONE] and find [ITEM_SERIAL]";
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Find [ITEM_SERIAL] somewhere inside [ITEM_ZONE]";
        InZoneFindItemHelp = "Use maintenance terminal command PING to find [ITEM_SERIAL]";
        SolveItem = "Proceed to input the backdoor command [SPECIAL_COMMAND] in [ITEM_SERIAL]";

        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        // Place the terminal in the last zone
        var node = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead, "special_terminal")!;
        var zoneIndex = node.ZoneNumber;

        dataLayer.ObjectiveData.ZonePlacementDatas.Add(
            new List<ZonePlacementData>()
            {
                new ZonePlacementData
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

            ///
            /// Triggers an error alarm on the activation of the alarm
            ///
            case SpecialCommand.ErrorAlarm:
            {
                (SpecialTerminalCommand, SpecialTerminalCommandDesc) = Generator.Pick(new List<(string, string)>
                {
                    ("ALARM_TEST_PROTOCOL", "Execute system-wide alarm diagnostics and test protocol."),
                    ("OPEN_LOCKDOWN_AREA", "Lift access restrictions on sealed zones, existing biomass containment will be compromised."),
                    ("OVERRIDE_LOCKDOWN", "Bypass emergency containment protocols, initiating a continuous security alert."),
                    ("RESTART_SECURITY_SYSTEM", "Reinitialize all security functions, broadcasting a persistent hostile threat alert."),
                });

                switch (level.Tier, director.Bulkhead)
                {
                    case ("D", Bulkhead.Main):
                        EventsOnActivate.AddSpawnWave(new()
                        {
                            WavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
                            WaveSettings = WaveSettings.Exit_Objective_Hard.PersistentId,
                            TriggerAlarm = true
                        }, 6.0);
                        break;
                    case ("D", Bulkhead.Extreme):
                        EventsOnActivate.AddSpawnWave(new()
                        {
                            WavePopulation = WavePopulation.Baseline.PersistentId,
                            WaveSettings = WaveSettings.Exit_Objective_Medium.PersistentId,
                            TriggerAlarm = true
                        }, 10.0);
                        break;
                    case ("D", Bulkhead.Overload):
                        EventsOnActivate.AddSpawnWave(new()
                        {
                            WavePopulation = (level.Settings.HasChargers(), level.Settings.HasShadows()) switch
                            {
                                (true, _) => WavePopulation.OnlyChargers.PersistentId,
                                (_, true) => WavePopulation.OnlyShadows.PersistentId,
                                (_, _) => WavePopulation.Baseline.PersistentId
                            },
                            WaveSettings = level.Settings.HasChargers() ?
                                WaveSettings.Exit_Objective_Medium.PersistentId :
                                WaveSettings.Exit_Objective_Hard.PersistentId,
                            TriggerAlarm = true
                        }, 7.0);
                        break;

                    case ("E", Bulkhead.Main):
                        EventsOnActivate.AddSpawnWave(new()
                        {
                            WavePopulation = WavePopulation.Baseline_Hybrids.PersistentId,
                            WaveSettings = WaveSettings.Exit_Objective_VeryHard.PersistentId,
                            TriggerAlarm = true
                        }, 4.0);
                        break;
                    case ("E", Bulkhead.Extreme):
                        EventsOnActivate.AddSpawnWave(new()
                        {
                            WavePopulation = WavePopulation.Baseline.PersistentId,
                            WaveSettings = WaveSettings.Exit_Objective_Medium.PersistentId,
                            TriggerAlarm = true
                        }, 10.0);
                        break;
                    case ("E", Bulkhead.Overload):
                        EventsOnActivate.AddSpawnWave(new()
                        {
                            WavePopulation = (level.Settings.HasChargers(), level.Settings.HasShadows()) switch
                            {
                                (true, _) => WavePopulation.OnlyChargers.PersistentId,
                                (_, true) => WavePopulation.OnlyShadows.PersistentId,
                                (_, _) => WavePopulation.Baseline.PersistentId
                            },
                            WaveSettings = level.Settings.HasChargers() ?
                                WaveSettings.Exit_Objective_Medium.PersistentId :
                                WaveSettings.Exit_Objective_Hard.PersistentId,
                            TriggerAlarm = true
                        }, 10.0);
                        break;

                    // Default case is not meant to be too hard
                    default:
                        EventsOnActivate.AddSpawnWave(new()
                        {
                            WavePopulation = WavePopulation.Baseline.PersistentId,
                            WaveSettings = WaveSettings.Exit_Objective_Easy.PersistentId,
                            TriggerAlarm = true
                        }, 12.0);
                        break;
                }

                break;
            }

            ///
            ///
            ///
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
                WavePopulation = WavePopulation.Baseline.PersistentId,
                WaveSettings = WaveSettings.Exit_Objective_Easy.PersistentId,
                TriggerAlarm = true,
                SpawnDelay = 4.0
            });
    }
}
