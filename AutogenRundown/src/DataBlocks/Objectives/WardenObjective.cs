using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Objectives.Reactor;
using AutogenRundown.DataBlocks.Zones;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    public enum DistributionStrategy
    {
        /// <summary>
        /// Randomly placed across all zones in random locations.
        /// </summary>
        Random,

        /// <summary>
        /// All items in a single zone (randomly)
        /// </summary>
        SingleZone,

        /// <summary>
        /// Evenly distributed across all zones
        /// </summary>
        EvenlyAcrossZones
    }

    public record class WardenObjective : DataBlock
    {
        /// <summary>
        /// Places objective items in the level as needed
        /// </summary>
        /// <param name="level"></param>
        /// <param name="bulkhead"></param>
        /// <param name="strategy"></param>
        public void DistributeObjectiveItems(
            Level level,
            Bulkhead bulkhead,
            DistributionStrategy strategy)
        {
            var data = level.GetObjectiveLayerData(bulkhead);
            var layout = level.GetLevelLayout(bulkhead);

            if (layout == null)
            {
                Plugin.Logger.LogError($"Missing level layout: {level.Tier}{level.Index}, Bulkhead={bulkhead}");
                return;
            }

            switch (strategy)
            {
                case DistributionStrategy.SingleZone:
                    {
                        // Pick a random zone that isn't the first zone unless there's only one
                        var targetZone = Math.Clamp(
                            Generator.Random.Next(1, layout.Zones.Count),
                            0,
                            layout.Zones.Count - 1);

                        data.ObjectiveData.ZonePlacementDatas.Add(
                            new List<ZonePlacementData>()
                            {
                                new ZonePlacementData
                                {
                                    LocalIndex = targetZone,
                                    Weights = ZonePlacementWeights.EvenlyDistributed
                                }
                            });

                        break;
                    }

                case DistributionStrategy.EvenlyAcrossZones:
                    {
                        var placement = new List<ZonePlacementData>();

                        foreach (var zone in layout.Zones)
                        {
                            placement.Add(new ZonePlacementData
                            {
                                LocalIndex = zone.LocalIndex,
                                Weights = ZonePlacementWeights.EvenlyDistributed
                            });
                        }

                        data.ObjectiveData.ZonePlacementDatas.Add(placement);

                        break;
                    }

                case DistributionStrategy.Random:
                    {
                        var placement = new List<ZonePlacementData>();

                        void Place(Zone zone)
                        {
                            placement.Add(new ZonePlacementData
                            {
                                LocalIndex = zone.LocalIndex,
                                Weights = ZonePlacementWeights.GenRandom()
                            });
                        }

                        foreach (var zone in layout.Zones)
                        {
                            if (Generator.Flip())
                                continue;

                            Place(zone);
                        }

                        // If none of the zones were picked, force pick one of the zones. Otherwise
                        // no items will be placed
                        if (placement.Count == 0)
                        {
                            var index = Math.Clamp(
                                Generator.Random.Next(1, layout.Zones.Count),
                                0,
                                layout.Zones.Count - 1);

                            var zone = layout.Zones[index];
                            Place(zone);
                        }

                        data.ObjectiveData.ZonePlacementDatas.Add(placement);

                        break;
                    }
            }
        }

        public static string GenLevelDescription(WardenObjectiveType type, WardenObjectiveItem item = WardenObjectiveItem.PersonnelId)
            => type switch
            {
                WardenObjectiveType.ClearPath => Generator.Pick(new List<string>
                    {
                        "Unknown hostile lifeform readings in subjacent quadrant. Expendable prisoners sent to survey threat severity."
                    }) ?? "",
                WardenObjectiveType.GatherSmallItems => item switch
                {
                    WardenObjectiveItem.Glp_1 => "Conduit genetic code compromised. Prisoners to collect DNA sample from HSU facility.",
                    WardenObjectiveItem.Glp_2 => "Conduit genetic code compromised. Prisoners to collect DNA sample from HSU facility.",
                    _ => "Prisoners to collect items from storage facility. High asset fatality rate expected."
                },
                _ => "",
            };

        public static List<(WardenObjectiveItem, string, string)> BuildSmallPickupPack(string tier)
            => new List<(WardenObjectiveItem, string, string)>
            {
                // Currently disabled items.
                //  * MemoryStick: The model is quite small and hard to see especially in boxes.
                //    Removed until some other pickup spot can be used

                //(WardenObjectiveItem.MemoryStick, "Memory stick", "Gather [COUNT_REQUIRED] Memory sticks and return the memory sticks for analysis."),

                (WardenObjectiveItem.PersonnelId, "Personnel ID", "Gather [COUNT_REQUIRED] Personnel IDs and return the data to be processed."),
                (WardenObjectiveItem.PartialDecoder, "Partial Decoder", "Gather [COUNT_REQUIRED] Partial Decoders and return the data to be processed."),
                (WardenObjectiveItem.Harddrive, "Hard drive", "Gather [COUNT_REQUIRED] Hard Drives and return the drives for data archival."),
                (WardenObjectiveItem.Glp_1, "GLP-1 canister", "Gather [COUNT_REQUIRED] GLP-1 canisters and return the canisters for genome sequencing."),
                (WardenObjectiveItem.Glp_2, "GLP-2 canister", "Gather [COUNT_REQUIRED] GLP-2 canisters and return the canisters for genome sequencing."),
                (WardenObjectiveItem.Osip, "OSIP vial", "Gather [COUNT_REQUIRED] OSIP vials and return the vials for chemical analysis."),
                (WardenObjectiveItem.PlantSample, "Plant sample", "Gather [COUNT_REQUIRED] Plant samples and return the samples for analysis."),
                (WardenObjectiveItem.DataCube, "Data cube", "Gather [COUNT_REQUIRED] Data cubes and return the cubes for data extraction."),
                (WardenObjectiveItem.DataCubeBackup, "Backup data cube", "Gather [COUNT_REQUIRED] Backup Data cubes and return the cubes for data archival."),
                (WardenObjectiveItem.DataCubeTampered, "Tampered data cube", "Gather [COUNT_REQUIRED] Data cubes and return the cubes for inspection.")
            };

        /// <summary>
        /// Calculates what multiplier should be used to give an exit scan time of "seconds"
        /// seconds. It seems the default exit time is 20 seconds.
        /// </summary>
        /// <param name="seconds">How many seconds the exit scan should take</param>
        /// <returns>The ChainedPuzzleAtExitScanSpeedMultiplier value to set</returns>
        public static double CalculateExitScanSpeedMultiplier(double seconds) => 20.0 / seconds;

        /// <summary>
        /// Randomly picks an exit time speed between min and max seconds inclusive. Returns a
        /// double that should be used to set ChainedPuzzleAtExitScanSpeedMultiplier
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double GenExitScanTime(int min, int max)
            => CalculateExitScanSpeedMultiplier(Generator.Random.Next(min, max + 1));

        /// <summary>
        /// Some settings from the objective are needed for level generation. However plenty of
        /// layout information is needed for the objective. Objective building is split into two
        /// phases. PreBuild() is called first to generate the objective and then Build() is called
        /// after level layout has been done.
        /// </summary>
        /// <param name="director"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static WardenObjective PreBuild(BuildDirector director, Level level)
        {
            var objective = new WardenObjective
            {
                Type = director.Objective,
            };

            switch (objective.Type)
            {
                case WardenObjectiveType.RetrieveBigItems:
                    {
                        var choices = new List<(double, WardenObjectiveItem)>
                        {
                            (1.0, WardenObjectiveItem.DataSphere),
                            (1.0, WardenObjectiveItem.CargoCrate),
                            (1.0, WardenObjectiveItem.CargoCrateHighSecurity),
                            (1.0, WardenObjectiveItem.CryoCase),
                        };

                        // These would be main objective items only
                        if (director.Bulkhead.HasFlag(Bulkhead.Main))
                        {
                            choices.Add((1.0, WardenObjectiveItem.NeonateHsu));
                            choices.Add((1.0, WardenObjectiveItem.MatterWaveProjector));
                        }

                        var item = Generator.Select(choices);

                        /**
                         * Some interesting options here for how many items we should spawn. We
                         * want to reduce the number of items for non Main objectives and also 
                         * want to increase the number of items for deeper levels.
                         * */
                        var count = (item, director.Tier, director.Bulkhead & Bulkhead.Objectives) switch
                        {
                            (WardenObjectiveItem.CryoCase, "A", Bulkhead.Main) => Generator.Random.Next(1, 2),
                            (WardenObjectiveItem.CryoCase, "B", Bulkhead.Main) => Generator.Random.Next(1, 2),
                            (WardenObjectiveItem.CryoCase, "C", Bulkhead.Main) => Generator.Random.Next(1, 2),
                            (WardenObjectiveItem.CryoCase, "D", Bulkhead.Main) => Generator.Random.Next(2, 3),
                            (WardenObjectiveItem.CryoCase, "E", Bulkhead.Main) => Generator.Random.Next(2, 4),
                            (WardenObjectiveItem.CryoCase, "D", _) => Generator.Random.Next(1, 2),
                            (WardenObjectiveItem.CryoCase, "E", _) => 2,

                            (WardenObjectiveItem.CargoCrateHighSecurity, "D", Bulkhead.Main) => Generator.Random.Next(1, 2),
                            (WardenObjectiveItem.CargoCrateHighSecurity, "E", Bulkhead.Main) => 2,

                            (_, _, _) => 1

                        };

                        for (var i = 0; i < count; ++i)
                            objective.RetrieveItems.Add(item);

                        break;
                    }

                case WardenObjectiveType.PowerCellDistribution:
                    {
                        objective.PowerCellsToDistribute = director.Tier switch
                        {
                            "A" => Generator.Random.Next(1, 2),
                            "B" => Generator.Random.Next(1, 2),
                            "C" => Generator.Random.Next(2, 3),
                            "D" => Generator.Random.Next(3, 4),
                            "E" => Generator.Random.Next(3, 5),
                            _ => 2
                        };

                        break;
                    }
            }

            return objective;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="director"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public void Build(BuildDirector director, Level level)
        {
            var dataLayer = level.GetObjectiveLayerData(director.Bulkhead);

            if (dataLayer is null)
            {
                Plugin.Logger.LogError($"WardenObjective.Build(): Missing level data layer: " +
                    $"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead}");
                throw new Exception("Missing level data layer");
            }

            var layout = level.GetLevelLayout(director.Bulkhead);

            if (layout is null)
            {
                Plugin.Logger.LogError($"WardenObjective.Build(): Missing level layout: " +
                    $"{level.Tier}{level.Index}, Bulkhead={director.Bulkhead}");
                throw new Exception("Missing level layout");
            }

            GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
            GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
            GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition";

            // Set the exit scan speed multiplier. Generally we want easier levels to be faster.
            // For some objectives this will be overridden.
            ChainedPuzzleAtExitScanSpeedMultiplier = director.Tier switch
            {
                "A" => GenExitScanTime(20, 30),
                "B" => GenExitScanTime(30, 45),
                "C" => GenExitScanTime(45, 80),
                "D" => GenExitScanTime(90, 120),
                "E" => GenExitScanTime(100, 140),
                _ => 1.0,
            };

            switch (director.Objective)
            {
                /**
                 * Collect the HSU from within a storage zone
                 */
                case WardenObjectiveType.HsuFindSample:
                    {
                        MainObjective = "Find <color=orange>[ITEM_SERIAL]</color> somewhere inside HSU Storage Zone";

                        ActivateHSU_BringItemInElevator = true;
                        GatherItemId = (uint)WardenObjectiveItem.HSU;
                        ChainedPuzzleToActive = ChainedPuzzle.TeamScan.PersistentId;

                        // Place HSU's within the objective zone
                        var zn = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead)!;
                        var zoneIndex = zn.ZoneNumber;

                        dataLayer.ObjectiveData.ZonePlacementDatas.Add(
                            new List<ZonePlacementData>()
                            {
                                new ZonePlacementData
                                {
                                    LocalIndex = zoneIndex,
                                    Weights = ZonePlacementWeights.NotAtStart
                                }
                            });

                        var zone = layout.Zones[zoneIndex];
                        zone.HSUsInZone = level.Tier switch
                        {
                            "B" => DistributionAmount.SomeMore,
                            "C" => DistributionAmount.Many,
                            "D" => DistributionAmount.Alot,
                            "E" => DistributionAmount.Tons,
                            _ => DistributionAmount.Some
                        };
                        zone.Coverage = level.Tier switch
                        {
                            "A" => new CoverageMinMax { Min = 20, Max = 25 },
                            "B" => new CoverageMinMax { Min = 25, Max = 30 },
                            "C" => new CoverageMinMax { Min = 30, Max = 50 },
                            "D" => new CoverageMinMax { Min = 50, Max = 75 },
                            "E" => new CoverageMinMax { Min = 75, Max = 100 },
                            _ => zone.Coverage
                        };
                        zone.HSUClustersInZone = level.Tier switch
                        {
                            "C" => 2,
                            "D" => 3,
                            "E" => 3,
                            _ => 1
                        };

                        // Add enemies on Goto Win
                        // TODO: do we want this for all bulkheads?
                        if (director.Bulkhead.HasFlag(Bulkhead.Main) || director.Tier != "A")
                            WavesOnGotoWin.Add(GenericWave.ExitTrickle);

                        break;
                    }

                /**
                 * Reactor shutdown will result in the lights being off for the remainder of the
                 * level. Factor that as a difficulty modifier.
                 * */
                case WardenObjectiveType.ReactorShutdown:
                    {
                        MainObjective = "Find the main reactor and shut it down";
                        FindLocationInfo = "Gather information about the location of the Reactor";
                        GoToZone = "Navigate to [ITEM_ZONE] and initiate the shutdown process";
                        SolveItem = "Make sure the Reactor is fully shut down before leaving";
                        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
                        GoToWinConditionHelp_ToMainLayer = "Go back to the main objective and complete the expedition.";

                        LightsOnFromBeginning = true;
                        LightsOnDuringIntro = true;
                        LightsOnWhenStartupComplete = false;

                        ChainedPuzzleToActive = ChainedPuzzle.TeamScan.PersistentId;

                        var midScan = Generator.Pick(ChainedPuzzle.BuildReactorShutdownPack(director.Tier));
                        ChainedPuzzleMidObjective = midScan?.PersistentId ?? ChainedPuzzle.AlarmClass5.PersistentId;
                        Bins.ChainedPuzzles.AddBlock(midScan);

                        // Seems we set these as empty?
                        // TODO: can we remove these?
                        ReactorWaves = new List<ReactorWave>
                        {
                            new ReactorWave
                            {
                                Warmup = 90.0,
                                WarmupFail = 20.0,
                                Wave = 60.0,
                                Verify = 0.0,
                                VerifyFail = 45.0
                            },
                            new ReactorWave
                            {
                                Warmup = 90.0,
                                WarmupFail = 20.0,
                                Wave = 60.0,
                                Verify = 0.0,
                                VerifyFail = 45.0
                            },
                            new ReactorWave
                            {
                                Warmup = 90.0,
                                WarmupFail = 20.0,
                                Wave = 60.0,
                                Verify = 0.0,
                                VerifyFail = 45.0
                            }
                        };

                        break;
                    }

                /**
                 * Gather small items from around the level. This is a fairly simple objective
                 * that can be completed in a variety of ways.
                 * */
                case WardenObjectiveType.GatherSmallItems:
                    {
                        var (itemId, name, description) = Generator.Pick(BuildSmallPickupPack(level.Tier));
                        var strategy = Generator.Pick(new List<DistributionStrategy>
                        {
                            DistributionStrategy.Random,
                            DistributionStrategy.SingleZone,
                            DistributionStrategy.EvenlyAcrossZones
                        });

                        MainObjective = description;
                        FindLocationInfo = $"Look for {name}s in the complex";
                        FindLocationInfoHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";

                        if (director.Bulkhead.HasFlag(Bulkhead.Main))
                            level.Description = GenLevelDescription(director.Objective, itemId);

                        GatherRequiredCount = level.Tier switch
                        {
                            "A" => Generator.Random.Next(4, 8),
                            "B" => Generator.Random.Next(6, 10),
                            "C" => Generator.Random.Next(7, 12),
                            "D" => Generator.Random.Next(8, 13),
                            "E" => Generator.Random.Next(9, 16),
                            _ => 1,
                        };

                        GatherItemId = (uint)itemId;
                        GatherSpawnCount = Generator.Random.Next(
                            GatherRequiredCount,
                            GatherRequiredCount + 6);

                        DistributeObjectiveItems(level, director.Bulkhead, strategy);

                        var zoneSpawns = dataLayer.ObjectiveData.ZonePlacementDatas[0].Count;

                        GatherMaxPerZone = GatherSpawnCount / zoneSpawns + GatherSpawnCount % zoneSpawns;

                        break;
                    }

                /**
                 * Fairly straight forward objective, get to the end zone. Some additional enemies
                 * at the end make this a more interesting experience.
                 *
                 * This objective can only be for Main given it ends the level on completion
                 * */
                case WardenObjectiveType.ClearPath:
                    {
                        // TODO: For some reason "[EXTRACTION_ZONE]" is not registering the exit zone correctly.
                        // For now we manually find the exit zone number.
                        var exitZone = layout.Zones.Find(z => z.CustomGeomorph != null && z.CustomGeomorph.Contains("exit_01"));
                        var exitIndex = layout.ZoneAliasStart + exitZone?.LocalIndex;
                        var exitZoneString = $"<color=orange>ZONE {exitIndex}</color>";

                        MainObjective = $"Clear a path to the exit point in {exitZoneString}";
                        GoToWinCondition_Elevator = "";
                        GoToWinCondition_CustomGeo = $"Go to the forward exit point in {exitZoneString}";

                        level.Description = GenLevelDescription(director.Objective);

                        // Ensure there's a nice spicy hoard at the end
                        exitZone?.EnemySpawningInZone.Add(
                            // These will be predominately strikers / shooters
                            new EnemySpawningData()
                            {
                                GroupType = EnemyGroupType.Hibernate,
                                Difficulty = (uint)EnemyRoleDifficulty.Easy,
                                Points = 75, // 25pts is 1.0 distribution, this is quite a lot
                            });

                        break;
                    }

                /**
                 * TODO: It would be nice to add special commands other than just lights off that do other modifiers.
                 *       Such as fog, error alarm, etc.
                 */
                case WardenObjectiveType.SpecialTerminalCommand:
                    {
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

                        // Special Command: Lights Off
                        SpecialTerminalCommand = "REROUTE_POWER";
                        SpecialTerminalCommandDesc = "Reroute power coupling to sector that has been powered down.";
                        EventBuilder.AddLightsOff(EventsOnActivate, 9.0);

                        // Add scans
                        ChainedPuzzleToActive = ChainedPuzzle.TeamScan.PersistentId;
                        ChainedPuzzleAtExit = ChainedPuzzle.ExitAlarm.PersistentId;

                        // Add exit wave if this is the main bulkhead
                        if (director.Bulkhead.HasFlag(Bulkhead.Main))
                            WavesOnGotoWin.Add(GenericWave.ExitTrickle); // TODO: not this, something else

                        var zn = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead)!;
                        var zoneIndex = zn.ZoneNumber;

                        dataLayer.ObjectiveData.ZonePlacementDatas.Add(
                            new List<ZonePlacementData>()
                            {
                                new ZonePlacementData
                                {
                                    LocalIndex = zoneIndex,
                                    Weights = ZonePlacementWeights.NotAtStart
                                }
                            });

                        break;
                    }

                /**
                 * Retrieve an item from within the complex.
                 * */
                case WardenObjectiveType.RetrieveBigItems:
                    {
                        MainObjective = "Find [ALL_ITEMS] and bring it to the extraction scan in [EXTRACTION_ZONE]";
                        FindLocationInfo = "Gather information about the location of [ALL_ITEMS]";
                        FindLocationInfoHelp = "Access more data in the terminal maintenance system";

                        // TODO: change the zone number
                        GoToZone = "Navigate to <color=orange>ZONE 20</color> and find [ALL_ITEMS]";
                        GoToZoneHelp = "Use information in the environment to find <color=orange>ZONE 20</color> ";
                        InZoneFindItem = "Find [ALL_ITEMS] somewhere inside <color=orange>ZONE 20</color>";

                        InZoneFindItemHelp = "Use maintenance terminal command PING to find [ALL_ITEMS]";
                        // TODO: rename this
                        SolveItem = "WARNING - Hisec Cargo misplaced - ENGAGING SECURITY PROTOCOLS";
                        GoToWinCondition_Elevator = "Return the <b>hisec crate</b> to the extraction point in [EXTRACTION_ZONE]";
                        GoToWinCondition_CustomGeo = "Return the <b>hisec crate</b> to the extraction point in [EXTRACTION_ZONE]";

                        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

                        break;
                    }

                /**
                 * Drop in with power cells and distribute them to generators in various zones.
                 *
                 * The power cells set with PowerCellsToDistribute are dropped in with you
                 * automatically.
                 * */
                case WardenObjectiveType.PowerCellDistribution:
                    {
                        MainObjective = "Distribute Power Cells from the elevator cargo container to [ALL_ITEMS]";
                        FindLocationInfo = "Locate the Generators and bring the Power Cells to them";
                        FindLocationInfoHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";
                        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
                        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
                        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
                        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
                        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

                        break;
                    }

                /**
                 * Sets up a terminal uplink objective. Randomizes the number of terminals, number
                 * of uplink words, etc.
                 */
                case WardenObjectiveType.TerminalUplink:
                    {
                        MainObjective = "Find the <u>Uplink Terminals</u> [ALL_ITEMS] and establish an external uplink from each terminal";
                        FindLocationInfo = "Gather information about the location of [ALL_ITEMS]";
                        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
                        SolveItem = "Use [ITEM_SERIAL] to create an uplink to [UPLINK_ADDRESS]";
                        SolveItemHelp = "Use the UPLINK_CONNECT command to establish the connection";

                        GoToWinCondition_Elevator = "Neural Imprinting Protocols retrieved. Return to the point of entrance in [EXTRACTION_ZONE]";
                        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
                        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
                        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
                        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

                        Uplink_NumberOfTerminals = (level.Tier, director.Bulkhead) switch
                        {
                            ("A", _) => 1,

                            ("B", _) => Generator.Random.Next(1, 2),

                            ("C", Bulkhead.Main) => Generator.Random.Next(1, 3),
                            ("C", _) => Generator.Random.Next(1, 2),

                            ("D", Bulkhead.Main) => Generator.Random.Next(1, 3),
                            ("D", _) => Generator.Random.Next(1, 3),

                            ("E", Bulkhead.Main) => Generator.Random.Next(2, 4),
                            ("E", _) => Generator.Random.Next(1, 3),

                            (_, _) => 1
                        };
                        Uplink_NumberOfVerificationRounds = (level.Tier, Uplink_NumberOfTerminals) switch
                        {
                            ("A", _) => 3,

                            ("B", _) => Generator.Random.Next(3, 4),

                            ("C", 1) => Generator.Random.Next(4, 6),
                            ("C", 2) => Generator.Random.Next(4, 5),
                            ("C", 3) => Generator.Random.Next(3, 4),

                            ("D", 1) => Generator.Random.Next(5, 6),
                            ("D", 2) => Generator.Random.Next(4, 6),
                            ("D", 3) => 4,

                            ("E", 1) => Generator.Random.Next(8, 12),
                            ("E", 2) => Generator.Random.Next(5, 6),
                            ("E", 3) => 5,
                            ("E", 4) => 5,

                            (_, _) => 1,
                        };
                        Uplink_WaveSpawnType = SurvivalWaveSpawnType.InSuppliedCourseNodeZone;

                        var wave = level.Tier switch
                        {
                            "A" => GenericWave.Uplink_Easy,
                            "B" => GenericWave.Uplink_Easy,
                            _ => GenericWave.Uplink_Medium,
                        };

                        WavesOnActivate.Add(wave);

                        var placements = new List<ZonePlacementData>();
                        var zones = level.Planner.GetZones(director.Bulkhead)
                                                 .TakeLast(Uplink_NumberOfTerminals);

                        foreach (var zone in zones)
                        {
                            placements.Add(new ZonePlacementData
                            {
                                LocalIndex = zone.ZoneNumber,
                                Weights = ZonePlacementWeights.NotAtStart
                            });
                        }

                        // TODO: Seems it picks randomly from the inner list? Let's split it a bit
                        dataLayer.ObjectiveData.ZonePlacementDatas.Add(placements);

                        break;
                    }

                /**
                 * Central generator cluster.
                 * TODO: Broken! Currently spawning the geomorph doesn't reliably get a generator
                 *       cluster to appear.
                 *
                 * Discord says getting the generator cluster to spawn can be tricky and require
                 * re-rolls with the zone seed. Still waiting on seeing if this is a problem.
                 */
                case WardenObjectiveType.CentralGeneratorCluster:
                    {
                        MainObjective = "Find [COUNT_REQUIRED] Power Cells and bring them to the Central Generator Cluster in [ITEM_ZONE]";
                        FindLocationInfo = "Locate the Power Cells and use them to power up the Generator Cluster";
                        FindLocationInfoHelp = "Generators Online: [COUNT_CURRENT] / [COUNT_REQUIRED]";
                        GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
                        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
                        GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
                        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
                        GoToWinCondition_ToMainLayer = "Malfunction in air purification system. Make your way for the forward emergency exit.";

                        ChainedPuzzleMidObjective = ChainedPuzzle.AlarmClass1.PersistentId;
                        //"ChainedPuzzleAtExit": 11,

                        PowerCellsToDistribute = 3;
                        CentralPowerGenClustser_NumberOfGenerators = 2;
                        CentralPowerGenClustser_NumberOfPowerCells = 2;

                        break;
                    }
            }

            dataLayer.ObjectiveData.DataBlockId = PersistentId;
        }

        public WardenObjectiveType Type { get; set; }

        #region Information and display strings
        public string Header { get; set; } = "";
        public string MainObjective { get; set; } = "";
        public string FindLocationInfo { get; set; } = "";
        public string FindLocationInfoHelp { get; set; } = "Access more data in the terminal maintenance system";
        public string GoToZone { get; set; } = "";
        public string GoToZoneHelp { get; set; } = "";
        public string InZoneFindItem { get; set; } = "";
        public string InZoneFindItemHelp { get; set; } = "";
        public string SolveItem { get; set; } = "";
        public string SolveItemHelp { get; set; } = "";
        public string GoToWinCondition_Elevator { get; set; } = "";
        public string GoToWinConditionHelp_Elevator { get; set; } = "";
        public string GoToWinCondition_CustomGeo { get; set; } = "";
        public string GoToWinConditionHelp_CustomGeo { get; set; } = "";
        public string GoToWinCondition_ToMainLayer { get; set; } = "";
        public string GoToWinConditionHelp_ToMainLayer { get; set; } = "";
        public string WaveOnElevatorWardenIntel { get; set; } = "";
        public string Survival_TimerTitle { get; set; } = "";
        public string Survival_TimerToActivateTitle { get; set; } = "";
        public string GatherTerminal_CommandHelp { get; set; } = "";
        public string GatherTerminal_DownloadingText { get; set; } = "";
        public string GatherTerminal_DownloadCompleteText { get; set; } = "";
        public double ShowHelpDelay { get; set; } = 180.0;
        #endregion

        #region Events
        public List<WardenObjectiveEvent> EventsOnActivate { get; set; } = new List<WardenObjectiveEvent>();

        public List<WardenObjectiveEvent> EventsOnElevatorLand { get; set; } = new List<WardenObjectiveEvent>();

        /// <summary>
        /// Waves to spawn on returning to win. This seems to only be for the main objective.
        /// </summary>
        public List<GenericWave> WavesOnGotoWin { get; set; } = new List<GenericWave>();

        /// <summary>
        /// Enemy waves to spawn on activating the objective.
        /// </summary>
        public List<GenericWave> WavesOnActivate { get; set; } = new List<GenericWave>();
        #endregion

        #region Type=?: Chained puzzles
        public uint ChainedPuzzleToActive { get; set; } = 0;

        public uint ChainedPuzzleMidObjective { get; set; } = 0;
        #endregion

        #region Type=0: Find HSU sample
        public bool ActivateHSU_BringItemInElevator { get; set; } = true;

        public int ActivateHSU_ItemFromStart = 0;
        public int ActivateHSU_ItemAfterActivation = 0;
        public bool ActivateHSU_MarkItemInElevatorAsWardenObjective = false;
        public bool ActivateHSU_StopEnemyWavesOnActivation = false;
        public bool ActivateHSU_ObjectiveCompleteAfterInsertion = false;
        public bool ActivateHSU_RequireItemAfterActivationInExitScan = false;
        public JArray ActivateHSU_Events = new JArray();
        #endregion

        #region Type=1 & 2: Reactor startup/shutdown
        public List<ReactorWave> ReactorWaves { get; set; } = new List<ReactorWave>();
        #endregion

        #region Type=3: Gather small items
        [JsonProperty("Gather_RequiredCount")]
        public int GatherRequiredCount { get; set; } = -1;

        [JsonProperty("Gather_ItemId")]
        public uint GatherItemId { get; set; } = 0;

        [JsonProperty("Gather_SpawnCount")]
        public int GatherSpawnCount { get; set; } = 0;

        [JsonProperty("Gather_MaxPerZone")]
        public int GatherMaxPerZone { get; set; } = 0;
        #endregion

        #region Type=4: Clear a path
        #endregion

        #region Type=5: Special terminal command
        /// <summary>
        /// The Special terminal command players have to enter
        /// </summary>
        public string SpecialTerminalCommand { get; set; } = "";

        /// <summary>
        /// Description displayed in the terminal COMMANDs listing
        /// </summary>
        public string SpecialTerminalCommandDesc { get; set; } = "";
        #endregion

        #region Type=6: Retrieve big items
        /// <summary>
        /// Specifies which items are to be retrieved for this objective
        /// </summary>
        [JsonProperty("Retrieve_Items")]
        public List<WardenObjectiveItem> RetrieveItems { get; set; } = new List<WardenObjectiveItem>();
        #endregion

        #region Type=7: Power cell distribution
        #endregion

        #region Type=8: Uplink terminal
        /// <summary>
        ///
        /// </summary>
        public int Uplink_NumberOfVerificationRounds { get; set; } = 0;

        /// <summary>
        ///
        /// </summary>
        public int Uplink_NumberOfTerminals { get; set; } = 1;

        /// <summary>
        ///
        /// </summary>
        public SurvivalWaveSpawnType Uplink_WaveSpawnType { get; set; } = SurvivalWaveSpawnType.InSuppliedCourseNodeZone;
        #endregion

        #region Type=9: Central generator cluster
        public int PowerCellsToDistribute { get; set; } = 0;

        public int CentralPowerGenClustser_NumberOfGenerators { get; set; } = 0;

        public int CentralPowerGenClustser_NumberOfPowerCells { get; set; } = 4;

        public JArray CentralPowerGenClustser_FogDataSteps = new JArray();
        #endregion

        #region Type=15: Timed terminal sequence
        public int TimedTerminalSequence_NumberOfRounds { get; set; } = 3;

        public int TimedTerminalSequence_NumberOfTerminals = 1;

        public double TimedTerminalSequence_TimePerRound = 90.0;

        public double TimedTerminalSequence_TimeForConfirmation = 10.0;

        public bool TimedTerminalSequence_UseFilterForSourceTerminalPicking = false;

        public string TimedTerminalSequence_SourceTerminalWorldEventObjectFilter = "";

        public JArray TimedTerminalSequence_EventsOnSequenceStart = new JArray();

        public JArray TimedTerminalSequence_EventsOnSequenceDone = new JArray();

        public JArray TimedTerminalSequence_EventsOnSequenceFail = new JArray();
        #endregion

        #region Expedition exit
        /// <summary>
        /// What exit scan to use at the exit
        /// </summary>
        public uint ChainedPuzzleAtExit { get; set; } = ChainedPuzzle.ExitAlarm.PersistentId;

        /// <summary>
        /// Multiplier to use for the exit scan speed. This is calculated from the exit scan time
        /// which by default is 20 seconds
        /// </summary>
        public double ChainedPuzzleAtExitScanSpeedMultiplier { get; set; } = 1.0;
        #endregion

        #region Fields not yet implemented
        public int WardenObjectiveSpecialUpdateType = 0;
        public int GenericItemFromStart = 0;
        public bool DoNotMarkPickupItemsAsWardenObjectives = false;
        public bool OverrideNoRequiredItemsForExit = false;
        public JArray WavesOnElevatorLand = new JArray();
        public int FogTransitionDataOnElevatorLand = 0;
        public double FogTransitionDurationOnElevatorLand = 0.0;
        public bool OnActivateOnSolveItem = false;
        public bool StopAllWavesBeforeGotoWin = false;
        public int WaveOnGotoWinTrigger = 0;
        public JArray EventsOnGotoWin = new JArray();
        public int EventsOnGotoWinTrigger = 0;
        public int FogTransitionDataOnGotoWin = 0;
        public double FogTransitionDurationOnGotoWin = 0.0;
        public bool LightsOnFromBeginning = false;
        public bool LightsOnDuringIntro = false;
        public bool LightsOnWhenStartupComplete = false;
        public bool DoNotSolveObjectiveOnReactorComplete = false;
        public JArray PostCommandOutput = new JArray();
        public int SpecialCommandRule = 0;
        public double Survival_TimeToActivate = 0.0;
        public double Survival_TimeToSurvive = 0.0;
        public int GatherTerminal_SpawnCount = 0;
        public int GatherTerminal_RequiredCount = 0;
        public string GatherTerminal_Command = "";
        public double GatherTerminal_DownloadTime = -1.0;
        #endregion
    }
}
