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
                (1.0, SpecialCommand.KingOfTheHill)
                // (0.80, SpecialCommand.LightsOff),
                // (0.15, SpecialCommand.FillWithFog),
                // (0.05, SpecialCommand.ErrorAlarm)
            }),
            "B" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.60, SpecialCommand.LightsOff),
                (0.15, SpecialCommand.FillWithFog),
                (0.15, SpecialCommand.ErrorAlarm)
            }),
            "C" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.40, SpecialCommand.LightsOff),
                (0.30, SpecialCommand.FillWithFog),
                (0.30, SpecialCommand.ErrorAlarm)
            }),
            "D" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.33, SpecialCommand.LightsOff),
                (0.33, SpecialCommand.FillWithFog),
                (0.34, SpecialCommand.ErrorAlarm)
            }),
            "E" => Generator.Select(new List<(double, SpecialCommand)>
            {
                (0.10, SpecialCommand.LightsOff),
                (0.40, SpecialCommand.FillWithFog),
                (0.50, SpecialCommand.ErrorAlarm)
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
                SpecialTerminalCommand = "OPEN_SECURITY_DOORS";
                SpecialTerminalCommandDesc = "Opens all security doors";
                break;
            }
        }

        // Always have the team scan to verify everyone's here to start the objective
        StartPuzzle = ChainedPuzzle.TeamScan;

        // King of the hill has extra scan
        if (SpecialTerminalCommand_Type == SpecialCommand.KingOfTheHill)
        {
            MidPuzzle = ChainedPuzzle.FindOrPersist(ChainedPuzzle.AlarmClass1_Sustained_MegaHuge);
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
