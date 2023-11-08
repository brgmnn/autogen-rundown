using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using Newtonsoft.Json;
using static DefaultCharacterLayouts;
using static UnityEngine.Rendering.PostProcessing.BloomRenderer;

namespace AutogenRundown.DataBlocks
{
    public record class Hibernating : Generator.ISelectable
    {
        public double Weight { get; set; } = 1.0;

        public uint Enemy { get; set; }
    }

    public record class LevelLayout : DataBlock
    {
        #region hidden data
        [JsonIgnore]
        private BuildDirector director;

        [JsonIgnore]
        private LevelSettings settings;
        #endregion

        public int ZoneAliasStart { get; set; }

        public List<Zone> Zones { get; set; } = new List<Zone>();

        public LevelLayout(BuildDirector director, LevelSettings settings)
        {
            this.director = director;
            this.settings = settings;
        }

        /// <summary>
        /// Ensures that the value is within the range of the number of zones in this level layout.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int ClampToZones(int value) => Math.Clamp(value, 0, Zones.Count - 1);

        /// <summary>
        /// Roll for door alarms
        /// </summary>
        public void RollAlarms(List<ChainedPuzzle> puzzlePack)
        {
            foreach (var zone in Zones)
            {
                zone.RollAlarms(puzzlePack);
            }
        }

        /// <summary>
        /// Roll for blood doors
        /// </summary>
        public void RollBloodDoors()
        {
            var count = 0;
            var (max, chance, inAreaChance) = director.Tier switch
            {
                // No blood doors for A
                "A" => (0, 0.0, 0.0),
                "B" => (1, 0.2, 0.3),
                "C" => (2, 0.15, 0.5),
                "D" => (3, 0.15, 0.5),
                _ => (-1, 0.2, 0.7)
            };

            // Ensure that there are at least as many groups as 2x the max number of blood doors
            // that can spawn. For unlimited cap tiers (D and E) this is 2x the number of zones.
            // Door pack is used to select enemies that spawn behind the door.
            var doorPack = director.Tier switch
            {
                "B" => new List<VanillaEnemyGroup>
                {
                    VanillaEnemyGroup.BloodDoor_Easy,
                    VanillaEnemyGroup.BloodDoor_Easy,
                    VanillaEnemyGroup.BloodDoor_Medium
                },

                "C" => new List<VanillaEnemyGroup>
                {
                    VanillaEnemyGroup.BloodDoor_Easy,
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Bigs,
                    VanillaEnemyGroup.BloodDoor_Hybrids_Easy,
                    VanillaEnemyGroup.BloodDoor_Hybrids_Medium,
                },

                "D" => new List<VanillaEnemyGroup>
                {
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Bigs,
                    VanillaEnemyGroup.BloodDoor_Bigs,
                    VanillaEnemyGroup.BloodDoor_Chargers_Easy,
                    VanillaEnemyGroup.BloodDoor_ChargersGiant_Easy,
                    VanillaEnemyGroup.BloodDoor_Hybrids_Medium,
                    VanillaEnemyGroup.BloodDoor_Hybrids_Medium,
                    VanillaEnemyGroup.BloodDoor_Shadows_Easy,
                    VanillaEnemyGroup.BloodDoor_BossMother
                },

                "E" => new List<VanillaEnemyGroup>
                {
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Bigs,
                    VanillaEnemyGroup.BloodDoor_Bigs,
                    VanillaEnemyGroup.BloodDoor_Chargers_Easy,
                    VanillaEnemyGroup.BloodDoor_ChargersGiant_Easy,
                    VanillaEnemyGroup.BloodDoor_Hybrids_Medium,
                    VanillaEnemyGroup.BloodDoor_Hybrids_Medium,
                    VanillaEnemyGroup.BloodDoor_Shadows_Easy,
                    VanillaEnemyGroup.BloodDoor_BossMother,
                    VanillaEnemyGroup.BloodDoor_BossMother
                },

                _ => new List<VanillaEnemyGroup>()
            };

            // Area pack picks enemies to spawn further back, if we successfully roll to add them.
            var areaPack = director.Tier switch
            {
                "B" => new List<VanillaEnemyGroup>
                {
                    VanillaEnemyGroup.BloodDoor_Easy,
                    VanillaEnemyGroup.BloodDoor_Medium
                },

                "C" => new List<VanillaEnemyGroup>
                {
                    VanillaEnemyGroup.BloodDoor_Easy,
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Bigs,
                    VanillaEnemyGroup.BloodDoor_Chargers_Easy,
                    VanillaEnemyGroup.BloodDoor_Hybrids_Easy,
                    VanillaEnemyGroup.BloodDoor_Shadows_Easy,
                    VanillaEnemyGroup.BloodDoor_Pouncers
                },

                "D" => new List<VanillaEnemyGroup>
                {
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Bigs,
                    VanillaEnemyGroup.BloodDoor_Bigs,
                    VanillaEnemyGroup.BloodDoor_Chargers_Easy,
                    VanillaEnemyGroup.BloodDoor_ChargersGiant_Easy,
                    VanillaEnemyGroup.BloodDoor_Hybrids_Medium,
                    VanillaEnemyGroup.BloodDoor_Hybrids_Medium,
                    VanillaEnemyGroup.BloodDoor_Shadows_Easy,
                    VanillaEnemyGroup.BloodDoor_BossMother,
                    VanillaEnemyGroup.BloodDoor_Pouncers
                },

                "E" => new List<VanillaEnemyGroup>
                {
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Medium,
                    VanillaEnemyGroup.BloodDoor_Bigs,
                    VanillaEnemyGroup.BloodDoor_Bigs,
                    VanillaEnemyGroup.BloodDoor_Chargers_Easy,
                    VanillaEnemyGroup.BloodDoor_ChargersGiant_Easy,
                    VanillaEnemyGroup.BloodDoor_Hybrids_Medium,
                    VanillaEnemyGroup.BloodDoor_Hybrids_Medium,
                    VanillaEnemyGroup.BloodDoor_Shadows_Easy,
                    VanillaEnemyGroup.BloodDoor_BossMother,
                    VanillaEnemyGroup.BloodDoor_Pouncers
                },

                _ => new List<VanillaEnemyGroup>()
            };

            // Do not add blood doors to Zone 0, these are always either the elevator or bulkhead doors.
            foreach (var zone in Zones)
                if (zone.LocalIndex > 0 && Generator.Flip(chance) && (count++ < max || max == -1))
                {
                    var withArea = Generator.Flip(inAreaChance);

                    zone.BloodDoor = new BloodDoor
                    {
                        EnemyGroupInfrontOfDoor = (uint)Generator.Draw(doorPack),
                        EnemyGroupInArea = withArea ? (uint)Generator.Draw(areaPack) : 0,
                        EnemyGroupsInArea = withArea ? 1 : 0,
                    };
                }
        }

        /// <summary>
        /// Roll enemies for each zone.
        /// </summary>
        public void RollEnemies(BuildDirector director)
        {
            // All scouts cost 5pts each
            var (chance, max, scoutPack) = director.Tier switch
            {
                "A" => (0.2, 2, new List<EnemySpawningData>
                    {
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 5 },
                    }),
                "B" => (0.2, 3, new List<EnemySpawningData>
                    {
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 10 },
                    }),
                "C" => (0.2, 5, new List<EnemySpawningData>
                    {
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },

                        // Chargers
                        EnemySpawningData.ScoutCharger with { Points = 5 },
                        EnemySpawningData.ScoutCharger with { Points = 5 },
                    }),

                "D" => (0.3, -1, new List<EnemySpawningData>
                    {
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },

                        // Chargers
                        EnemySpawningData.ScoutCharger with { Points = 5 },
                        EnemySpawningData.ScoutCharger with { Points = 5 },
                        EnemySpawningData.ScoutCharger with { Points = 5 },
                        EnemySpawningData.ScoutCharger with { Points = 10 },

                        // Shadows
                        EnemySpawningData.ScoutShadow with { Points = 5 },
                        EnemySpawningData.ScoutShadow with { Points = 5 },
                        EnemySpawningData.ScoutShadow with { Points = 5 },
                        EnemySpawningData.ScoutShadow with { Points = 10 },
                    }),

                "E" => (0.3, -1, new List<EnemySpawningData>
                    {
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },

                        // Chargers
                        EnemySpawningData.ScoutCharger with { Points = 5 },
                        EnemySpawningData.ScoutCharger with { Points = 5 },
                        EnemySpawningData.ScoutCharger with { Points = 5 },
                        EnemySpawningData.ScoutCharger with { Points = 10 },

                        // Shadows
                        EnemySpawningData.ScoutShadow with { Points = 5 },
                        EnemySpawningData.ScoutShadow with { Points = 5 },
                        EnemySpawningData.ScoutShadow with { Points = 5 },
                        EnemySpawningData.ScoutShadow with { Points = 10 },
                    }),

                _ => (0.0, 0, new List<EnemySpawningData>())
            };

            var scoutCount = 0;

            foreach (var zone in Zones)
            {
                var points = director.GetPoints(zone);

                // Reduce the chance of scouts spawning in the zone if there's a blood door to enter.
                var scoutRollModifier = zone.BloodDoor.Enabled ? 0.5 : 1.0;

                // Roll for adding scouts
                if (Generator.Flip(chance * scoutRollModifier) && (scoutCount++ < max || max == -1))
                {
                    var scout = Generator.Draw(scoutPack);

                    // Add scouts with force one, this is to guarantee we get exactly the right
                    // number of scouts.
                    if (scout != null)
                    {
                        points = scout.Points;

                        for (int i = 0; i < scout.Points / 5; i++)
                            zone.EnemySpawningInZone.Add(
                                scout with { Distribution = EnemyZoneDistribution.ForceOne, Points = 25 });
                    }
                }

                // If we have run out of points, skip adding enemies.
                if (points < 3)
                    continue;


                // If we have a blood door, reduce the number of enemies that spawn in the zone
                // by 1/3rd.
                if (zone.BloodDoor.Enabled)
                    points = (int)(points * 0.66);


                #region Charger roll check
                var chargerChance = 0.0;

                if (settings.Modifiers.Contains(LevelModifiers.Chargers))
                    chargerChance = 0.15;

                if (settings.Modifiers.Contains(LevelModifiers.ManyChargers))
                    chargerChance = 0.5;

                // Roll for a Charger room
                if (!settings.Modifiers.Contains(LevelModifiers.NoChargers) &&
                    (settings.Modifiers.Contains(LevelModifiers.OnlyChargers) || Generator.Flip(chargerChance)))
                {
                    var chargerGiantRatio = director.Tier switch
                    {
                        "D" => Generator.Flip(0.3) ? 0.25 : 0,
                        "E" => Generator.Flip(0.5) ? 0.6 : 0,
                        _ => 0.0
                    };

                    var chargerGiantPoints = (int)(points * chargerGiantRatio);
                    var chargerPoints = points - chargerGiantPoints;

                    Plugin.Logger.LogDebug($"Zone {zone.LocalIndex} rolled: Chargers {points}pts");

                    zone.EnemySpawningInZone.Add(
                        new EnemySpawningData
                        {
                            GroupType = EnemyGroupType.Hibernate,
                            Difficulty = (uint)Enemy.Charger,
                            Points = chargerPoints
                        });

                    if (chargerGiantPoints > 0)
                    {
                        Plugin.Logger.LogDebug($"Zone {zone.LocalIndex} rolled: Giant Chargers {points}pts");

                        zone.EnemySpawningInZone.Add(
                            new EnemySpawningData
                            {
                                GroupType = EnemyGroupType.Hibernate,
                                Difficulty = (uint)Enemy.ChargerGiant,
                                Points = chargerGiantPoints
                            });
                    }
                    continue;
                }
                #endregion

                #region Shadows roll check
                var shadowChance = 0.0;

                if (settings.Modifiers.Contains(LevelModifiers.Shadows))
                    shadowChance = 0.15;

                if (settings.Modifiers.Contains(LevelModifiers.ManyShadows))
                    shadowChance = 0.5;

                // Roll for a Shadow room
                if (!settings.Modifiers.Contains(LevelModifiers.NoShadows) &&
                    (settings.Modifiers.Contains(LevelModifiers.OnlyShadows) || Generator.Flip(shadowChance)))
                {
                    var shadowGiantRatio = director.Tier switch
                    {
                        "D" => Generator.Flip(0.2) ? 0.25 : 0,
                        "E" => Generator.Flip(0.5) ? 0.6 : 0,
                        _ => 0.0
                    };

                    var shadowGiantPoints = (int)(points * shadowGiantRatio);
                    var shadowPoints = points - shadowGiantPoints;

                    Plugin.Logger.LogDebug($"Zone {zone.LocalIndex} rolled: Shadows {shadowPoints}pts");

                    zone.EnemySpawningInZone.Add(
                        new EnemySpawningData
                        {
                            GroupType = EnemyGroupType.Hibernate,
                            Difficulty = (uint)Enemy.Shadow,
                            Points = shadowPoints
                        });

                    if (shadowGiantPoints > 0)
                    {
                        Plugin.Logger.LogDebug($"Zone {zone.LocalIndex} rolled: Shadow Giants {shadowGiantPoints}pts");

                        zone.EnemySpawningInZone.Add(
                            new EnemySpawningData
                            {
                                GroupType = EnemyGroupType.Hibernate,
                                Difficulty = (uint)Enemy.ShadowGiant,
                                Points = shadowGiantPoints
                            });
                    }
                    continue;
                }
                #endregion

                Plugin.Logger.LogDebug($"Zone has {points}pts for enemies");

                // By default we will just let the spawning data allocate out groups.
                zone.EnemySpawningInZone.Add(
                    new EnemySpawningData
                    {
                        GroupType = EnemyGroupType.Hibernate,
                        Difficulty = director.Tier switch
                        {
                            "A" => (uint)AutogenDifficulty.TierA,
                            "B" => (uint)AutogenDifficulty.TierB,
                            "C" => (uint)AutogenDifficulty.TierC,
                            "D" => (uint)AutogenDifficulty.TierD,
                            "E" => (uint)AutogenDifficulty.TierE,
                            _ => (uint)AutogenDifficulty.TierC
                        },
                        Points = points
                    });
            }
        }

        /// <summary>
        /// Rolls for whether we should add an error alarm to this level layout.
        /// </summary>
        public void RollErrorAlarm()
        {
            // Rolls for alarms to add. Each successful roll adds an error alarm and rolls again
            // with reduced chance of adding another alarm.
            int Roll(double chance)
            {
                if (Generator.Flip(chance))
                    return 1 + Roll(chance - 0.4);

                return 0;
            }

            var alarmCount = director.Tier switch
            {
                // No error alarms for A/B
                "A" => 0,
                "B" => 0,
                "C" => Roll(0.1),
                "D" => Roll(0.3),

                // E-tier has a 85% chance of having one error alarm. 38% chance of having two error alarms,
                // and a 2% chance of having three error alarms.
                "E" => Roll(0.85),
                _ => 0
            };

            // No need to process further if we have no error alarm
            if (alarmCount < 1)
                return;

            for (int i = 0; i < alarmCount; i++)
            {
                var puzzle = ChainedPuzzle.AlarmError_Baseline;

                // First try and find a zone in the middle without an alarm already.
                var candidates = Zones.Where(z => z.LocalIndex != 0 && z.ChainedPuzzleToEnter == 0 && z.LocalIndex != Zones.Count - 1);

                // If no candidates, search for any zone in the middle (we will overwrite the alarm)
                if (candidates.Count() == 0)
                    candidates = Zones.Where(z => z.LocalIndex != 0 && z.LocalIndex != Zones.Count - 1);

                // If there's still no candidates, include the last zone. Note this probably never
                // gets called as all levels have at least 3 zones.
                if (candidates.Count() == 0)
                    candidates = Zones.Where(z => z.LocalIndex != 0);

                // Pick from all zones without alarms already that aren't the first zone
                var zone = Generator.Pick(candidates);

                if (zone == null)
                {
                    // Something's gone wrong if this is the case and there were no zones to pick from.
                    return;
                }

                zone.ChainedPuzzleToEnter = puzzle.PersistentId;

                Bins.ChainedPuzzles.AddBlock(puzzle);

                // Give a flat chance of being able to turn off the alarm.
                if (Generator.Flip(0.2))
                {
                    zone.TurnOffAlarmOnTerminal = true;
                }
            }
        }

        /// <summary>
        /// Add a door that requires a key to be used.
        /// </summary>
        public void RollKeyedDoors(BuildDirector director, LayoutPlanner planner, ICollection<ChainedPuzzle> puzzlePack)
        {
            // Chance not to add any keyed doors
            if (Generator.Flip(0.6))
                return;

            var openZones = planner.GetZonesWithTotalOpen(0, 2, director.Bulkhead, "primary");
            var branchFrom = Generator.Pick(openZones);
            var connections = planner.GetConnections(branchFrom, "primary");
            var lockedZone = Generator.Pick(connections);
            var trunk = Zones[branchFrom.ZoneNumber];

            // Add the branch zones
            var branch = "key1";
            var branchLength = director.Tier switch
            {
                "A" => 1,
                "B" => 1,
                "C" => Generator.Select(new List<WeightedValue<int>>
                {
                    new WeightedValue<int> { Value = 1, Weight = 0.66 },
                    new WeightedValue<int> { Value = 2, Weight = 0.33 },
                }).Value,
                "D" => Generator.Select(new List<WeightedValue<int>>
                {
                    new WeightedValue<int> { Value = 1, Weight = 0.66 },
                    new WeightedValue<int> { Value = 2, Weight = 0.33 },
                }).Value,
                "E" => Generator.Select(new List<WeightedValue<int>>
                {
                    new WeightedValue<int> { Value = 1, Weight = 0.66 },
                    new WeightedValue<int> { Value = 2, Weight = 0.33 },
                }).Value,
                _ => 1
            };

            for (int i = 0; i < branchLength; i++)
            {
                var fromZone = planner.GetLastZone(director.Bulkhead, branch) ??
                    new ZoneNode(director.Bulkhead, trunk.LocalIndex, "primary");

                var zone = new Zone
                {
                    LocalIndex = planner.NextIndex(director.Bulkhead),
                    BuildFromLocalIndex = fromZone.ZoneNumber,
                    Coverage = CoverageMinMax.GenSize(i),
                    LightSettings = Lights.GenRandomLight(),
                };
                planner.Connect(fromZone, new ZoneNode(director.Bulkhead, zone.LocalIndex, branch));

                Plugin.Logger.LogDebug($"Adding zones: {fromZone} -> {new ZoneNode(director.Bulkhead, zone.LocalIndex, branch)}");

                zone.RollAlarms(puzzlePack);

                Zones.Add(zone);
            }

            var locked = Zones.Find(z => z.LocalIndex == lockedZone.ZoneNumber);

            locked.ProgressionPuzzleToEnter = new ProgressionPuzzle
            {
                PuzzleType = ProgressionPuzzleType.Keycard,
                ZonePlacementData = new List<ZonePlacementData>
                {
                    new ZonePlacementData
                    {
                        LocalIndex = planner.GetZones(director.Bulkhead, branch).Last().ZoneNumber,
                    }
                }
            };
        }

        /// <summary>
        /// Generates a Zone Alias start. In general the deeper the level the higher the zone numbers
        /// </summary>
        /// <param name="tier"></param>
        /// <returns></returns>
        public static int GenZoneAliasStart(string tier)
        {
            switch (tier)
            {
                case "B":
                    return Generator.Random.Next(50, 400);
                case "C":
                    return Generator.Random.Next(200, 600);
                case "D":
                    return Generator.Random.Next(300, 850);
                case "E":
                    return Generator.Random.Next(450, 950);

                case "A":
                default:
                    return Generator.Random.Next(5, 200);
            }
        }

        public static SubComplex GenSubComplex(Complex complex)
            => complex switch
            {
                Complex.Mining => Generator.Pick(new List<SubComplex>
                {
                    SubComplex.DigSite,
                    SubComplex.Refinery,
                    SubComplex.Storage
                }),
                Complex.Tech => Generator.Pick(new List<SubComplex>
                {
                    SubComplex.DataCenter,
                    SubComplex.Lab
                }),
                Complex.Service => Generator.Pick(new List<SubComplex>
                {
                    SubComplex.Floodways,
                    SubComplex.Gardens
                }),

                _ => SubComplex.All
            };

        /// <summary>
        /// Connects the first bulkhead zone to a zone in this layout
        /// </summary>
        /// <param name="level"></param>
        /// <param name="bulkhead"></param>
        /// <param name="from"></param>
        public static void InitializeBulkheadArea(
            Level level,
            Bulkhead bulkhead,
            ZoneNode from,
            Zone? zone = null)
        {
            var bulkheadZone = new ZoneNode(bulkhead, level.Planner.NextIndex(bulkhead));
            level.Planner.Connect(from, bulkheadZone);

            level.Planner.AddZone(
                bulkheadZone,
                zone ?? new Zone
                {
                    Coverage = CoverageMinMax.GenNormalSize(),
                    LightSettings = Lights.GenRandomLight(),
                });

            // Determine the correct from layer type. This is an int corresponding to the from
            // zone bulkhead type
            var layerType = from.Bulkhead switch
            {
                Bulkhead.Extreme => 1,
                Bulkhead.Overload => 2,
                _ => 0,
            };

            // Ensure a Bulkhead DC is placed in the from zone.
            var layerData = level.GetObjectiveLayerData(from.Bulkhead);

            if (layerData.BulkheadDoorControllerPlacements
                .Where(dc => dc.ZoneIndex == from.ZoneNumber)
                .Count() == 0)
            {
                layerData.BulkheadDoorControllerPlacements.Add(
                    new BulkheadDoorPlacementData()
                    {
                        ZoneIndex = from.ZoneNumber,
                        PlacementWeights = ZonePlacementWeights.NotAtStart
                    });
            }

            // Mark the correct zones as bulkhead zone for main, as well as setting the right build
            // from parameter.
            if (bulkhead.HasFlag(Bulkhead.Extreme))
            {
                level.BuildSecondaryFrom = new BuildFrom { LayerType = layerType, Zone = from.ZoneNumber };
                Plugin.Logger.LogInfo($"Level={level.Tier}{level.Index} - Extreme Bulkhead Entrance = {from}");
            }
            else if (bulkhead.HasFlag(Bulkhead.Overload))
            {
                level.BuildThirdFrom = new BuildFrom { LayerType = layerType, Zone = from.ZoneNumber };
                Plugin.Logger.LogInfo($"Level={level.Tier}{level.Index} - Overload Bulkhead Entrance = {from}");
            }
            else
            {
                level.MainBulkheadZone = bulkheadZone.ZoneNumber;
                level.MainLayerData.ZonesWithBulkheadEntrance.Add(bulkheadZone.ZoneNumber);
                Plugin.Logger.LogInfo($"Level={level.Tier}{level.Index} - Main Bulkhead Entrance = {from}");
            }
        }

        /// <summary>
        /// Applicable for Main bulkhead, build the main level.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="director"></param>
        public static void BuildStartingArea(Level level, BuildDirector director)
        {
            // Return early if we should not be building this.
            if (!director.Bulkhead.HasFlag(Bulkhead.Main))
                return;

            // Add the first zone.
            var elevatorDrop = new ZoneNode(
                Bulkhead.Main | Bulkhead.StartingArea,
                level.Planner.NextIndex(Bulkhead.Main));

            level.Planner.Connect(elevatorDrop);
            level.Planner.AddZone(
                elevatorDrop,
                new Zone
                {
                    Coverage = new CoverageMinMax { Min = 25, Max = 35 },
                    LightSettings = Lights.GenRandomLight(),
                });

            var minimumZones = level.Settings.Bulkheads switch
            {
                Bulkhead.Main => 0,
                Bulkhead.Main | Bulkhead.Extreme => 1,
                Bulkhead.Main | Bulkhead.Overload => 1,
                Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload => 2,
                _ => 1
            };

            var toPlace = level.Settings.Bulkheads switch
            {
                Bulkhead.Main => new List<Bulkhead>{ Bulkhead.Main },
                Bulkhead.Main | Bulkhead.Extreme => new List<Bulkhead>
                {
                    Bulkhead.Main,
                    Bulkhead.Extreme
                },
                Bulkhead.Main | Bulkhead.Overload => new List<Bulkhead>
                {
                    Bulkhead.Main,
                    Bulkhead.Overload
                },
                Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload => new List<Bulkhead>
                {
                    Bulkhead.Main,
                    Bulkhead.Extreme,
                    Bulkhead.Overload
                },

                _ => new List<Bulkhead>()
            };

            var prev = elevatorDrop;
            for (int i = 0; i < minimumZones; i++)
            {
                var zoneIndex = level.Planner.NextIndex(Bulkhead.Main);
                var next = new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, zoneIndex);

                level.Planner.Connect(prev, next);
                level.Planner.AddZone(
                    next,
                    new Zone
                    {
                        Coverage = CoverageMinMax.GenNormalSize(),
                        LightSettings = Lights.GenRandomLight(),
                    });

                // Place the first zones of the connecting bulkhead zones, so we can build from
                // them later.
                InitializeBulkheadArea(level, Generator.Draw(toPlace), next);

                prev = next;
            }

            // The final area also needs to be placed
            InitializeBulkheadArea(level, Generator.Draw(toPlace), prev);
        }

        /// <summary>
        /// Builds the main level
        ///
        /// Objective is not a fully initialized objective, it is a pre-built objective with just
        /// the basics needed for level generation
        /// </summary>
        /// <param name="level"></param>
        /// <param name="director"></param>
        /// <param name="objective"></param>
        /// <returns></returns>
        public static LevelLayout Build(Level level, BuildDirector director, WardenObjective objective)
        {
            var layout = new LevelLayout(director, level.Settings)
            {
                Name = $"{level.Tier}{level.Index} {level.Name} {director.Bulkhead}",
                ZoneAliasStart = GenZoneAliasStart(level.Tier)
            };

            director.GenZones();

            var objectiveLayerData = level.GetObjectiveLayerData(director.Bulkhead);
            var puzzlePack = ChainedPuzzle.BuildPack(level.Tier);

            BuildStartingArea(level, director);

            switch (director.Objective)
            {
                case WardenObjectiveType.ReactorShutdown:
                    {
                        // Create some initial zones
                        var prev = level.Planner.GetExactZones(director.Bulkhead).First();

                        // Don't create quite all the initial zones
                        var preludeZoneCount = Generator.Random.Next(0, director.ZoneCount);

                        for (int i = 0; i < preludeZoneCount; i++)
                        {
                            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                            var next = new ZoneNode(director.Bulkhead, zoneIndex);

                            level.Planner.Connect(prev, next);
                            level.Planner.AddZone(
                                next,
                                new Zone
                                {
                                    Coverage = CoverageMinMax.GenNormalSize(),
                                    LightSettings = Lights.GenRandomLight(),
                                });

                            prev = next;
                        }

                        // Pick a random direction to expand the reactor
                        var (startExpansion, zoneExpansion) = Generator.Pick(
                            new List<(ZoneBuildExpansion, ZoneExpansion)>
                            {
                                (ZoneBuildExpansion.Left, ZoneExpansion.Left),
                                (ZoneBuildExpansion.Right, ZoneExpansion.Right),
                                (ZoneBuildExpansion.Forward, ZoneExpansion.Forward),
                                (ZoneBuildExpansion.Backward, ZoneExpansion.Backward)
                            });
                        // Use the same light for both corridor and reactor
                        var light = Lights.GenReactorLight();

                        // Always generate a corridor of some kind (currently fixed) for the reactor zones.
                        var corridorNode = new ZoneNode(director.Bulkhead, level.Planner.NextIndex(director.Bulkhead));
                        var corridor = new Zone
                        {
                            LightSettings = light,
                            StartPosition = ZoneEntranceBuildFrom.Furthest,
                            StartExpansion = startExpansion,
                            ZoneExpansion = zoneExpansion
                        };
                        corridor.GenReactorCorridorGeomorph(director.Complex);

                        level.Planner.Connect(prev, corridorNode);
                        level.Planner.AddZone(corridorNode, corridor);

                        // Create the reactor zone
                        var reactorNode = new ZoneNode(director.Bulkhead, level.Planner.NextIndex(director.Bulkhead));
                        var reactor = new Zone
                        {
                            LightSettings = light,
                            StartPosition = ZoneEntranceBuildFrom.Furthest,
                            StartExpansion = startExpansion,
                            ZoneExpansion = zoneExpansion,
                            ForbidTerminalsInZone = true
                        };
                        reactor.GenReactorGeomorph(director.Complex);
                        reactor.TerminalPlacements = new List<TerminalPlacement>();

                        level.Planner.Connect(corridorNode, reactorNode);
                        level.Planner.AddZone(reactorNode, reactor);

                        break;
                    }

                case WardenObjectiveType.ClearPath:
                    {
                        var prev = level.Planner.GetExactZones(director.Bulkhead).First();

                        for (int i = 1; i < director.ZoneCount; i++)
                        {
                            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                            var next = new ZoneNode(director.Bulkhead, zoneIndex);

                            level.Planner.Connect(prev, next);
                            level.Planner.AddZone(
                                next,
                                new Zone
                                {
                                    Coverage = CoverageMinMax.GenNormalSize(),
                                    LightSettings = Lights.GenRandomLight(),
                                });

                            prev = next;
                        }

                        var last = prev;
                        var lastZone = (Zone)level.Planner.GetZone(last)!;

                        var subcomplex = GenSubComplex(level.Complex);

                        // The final zone is the extraction zone
                        lastZone.Coverage = new CoverageMinMax { Min = 50, Max = 50 };
                        lastZone.SubComplex = subcomplex;
                        lastZone.GenExitGeomorph(level.Complex);

                        break;
                    }

                /**
                 * When building the power cell distribution layout, here we are modelling a hub with offshoot zones.
                 * */
                case WardenObjectiveType.PowerCellDistribution:
                    {
                        // Zone 1 is an entrance I-geo
                        var entrance = level.Planner.GetExactZones(director.Bulkhead).First();

                        var entranceZone = level.Planner.GetZone(entrance)!;
                        entranceZone.GenCorridorGeomorph(director.Complex);
                        entrance.MaxConnections = 1;

                        // Zone 2 is a hub zone for branches where generators live
                        var hubIndex = level.Planner.NextIndex(director.Bulkhead);
                        var hub = new ZoneNode(director.Bulkhead, hubIndex);
                        hub.MaxConnections = 3;

                        var zone = new Zone { LightSettings = Lights.GenRandomLight() };
                        zone.GenHubGeomorph(director.Complex);

                        level.Planner.Connect(entrance, hub);
                        level.Planner.AddZone(hub, zone);

                        // Create base branches for each generator
                        for (int g = 0; g < Math.Min(objective.PowerCellsToDistribute, 3); g++)
                        {
                            var branch = $"generator_{g}";
                            var branchZoneCount = Generator.Random.Next(2, 3);
                            var prev = hub;

                            // Generate the zones for this generators branch
                            for (int i = 0; i < branchZoneCount; i++)
                            {
                                var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                                var next = new ZoneNode(director.Bulkhead, zoneIndex, branch);

                                level.Planner.Connect(prev, next);
                                level.Planner.AddZone(
                                    next,
                                    new Zone
                                    {
                                        Coverage = CoverageMinMax.GenNormalSize(),
                                        LightSettings = Lights.GenRandomLight(),
                                    });

                                prev = next;
                            }

                            // Place the generator in the last zone of the branch
                            var lastZone = level.Planner.GetZone(prev)!;
                            lastZone.PowerGeneratorPlacements.Add(
                                new FunctionPlacementData()
                                {
                                    PlacementWeights = ZonePlacementWeights.NotAtStart
                                });

                            // Assign the zone placement data for the objective text
                            objectiveLayerData.ObjectiveData.ZonePlacementDatas.Add(
                                new List<ZonePlacementData>
                                {
                                    new ZonePlacementData
                                    {
                                        LocalIndex = prev.ZoneNumber,
                                        Weights = ZonePlacementWeights.NotAtStart
                                    }
                                });
                        }

                        // Special case where we need one more zone.
                        if (objective.PowerCellsToDistribute == 4)
                        {
                            var branch = "generator_3";

                            var baseNode = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead, "generator_2")!;

                            var branchZoneCount = Generator.Random.Next(2, 3);
                            var prev = baseNode;

                            // Generate the zones for this generators branch
                            for (int i = 0; i < branchZoneCount; i++)
                            {
                                var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                                var next = new ZoneNode(director.Bulkhead, zoneIndex, branch);

                                level.Planner.Connect(prev, next);
                                level.Planner.AddZone(
                                    next,
                                    new Zone
                                    {
                                        Coverage = CoverageMinMax.GenNormalSize(),
                                        LightSettings = Lights.GenRandomLight(),
                                    });

                                prev = next;
                            }

                            // Place the generator in the last zone of the branch
                            var lastZone = level.Planner.GetZone(prev)!;
                            lastZone.PowerGeneratorPlacements.Add(
                                new FunctionPlacementData()
                                {
                                    PlacementWeights = ZonePlacementWeights.NotAtStart
                                });

                            // Assign the zone placement data for the objective text
                            objectiveLayerData.ObjectiveData.ZonePlacementDatas.Add(
                                new List<ZonePlacementData>
                                {
                                    new ZonePlacementData
                                    {
                                        LocalIndex = prev.ZoneNumber,
                                        Weights = ZonePlacementWeights.NotAtStart
                                    }
                                });
                        }

                        break;
                    }

                case WardenObjectiveType.CentralGeneratorCluster:
                    {
                        var prev = level.Planner.GetExactZones(director.Bulkhead).First();

                        // Place the generator cluster in the first zone
                        var firstZone = level.Planner.GetZone(prev)!;
                        firstZone.GenGeneratorClusterGeomorph(director.Complex);

                        // Place out some cell zones
                        for (int i = 1; i < director.ZoneCount; i++)
                        {
                            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                            var next = new ZoneNode(director.Bulkhead, zoneIndex);

                            level.Planner.Connect(prev, next);
                            level.Planner.AddZone(
                                next,
                                new Zone
                                {
                                    Coverage = CoverageMinMax.GenNormalSize(),
                                    LightSettings = Lights.GenRandomLight(),
                                });

                            prev = next;
                        }

                        // Distribute cells
                        var pickup = new BigPickupDistribution
                        {
                            SpawnsPerZone = 2,
                            SpawnData = new List<BigPickupSpawnData>
                                    {
                                        new BigPickupSpawnData { Item = WardenObjectiveItem.PowerCell },
                                        new BigPickupSpawnData { Item = WardenObjectiveItem.PowerCell }
                                    }
                        };
                        Bins.BigPickupDistributions.AddBlock(pickup);

                        var last = prev;
                        var lastZone = (Zone)level.Planner.GetZone(last)!;

                        lastZone.BigPickupDistributionInZone = pickup.PersistentId;

                        break;
                    }

                case WardenObjectiveType.SpecialTerminalCommand:
                case WardenObjectiveType.HsuFindSample:
                case WardenObjectiveType.GatherSmallItems:
                default:
                    {
                        var prev = level.Planner.GetExactZones(director.Bulkhead).First();

                        for (int i = 1; i < director.ZoneCount; i++)
                        {
                            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                            var next = new ZoneNode(director.Bulkhead, zoneIndex);

                            level.Planner.Connect(prev, next);
                            level.Planner.AddZone(
                                next,
                                new Zone
                                {
                                    Coverage = CoverageMinMax.GenNormalSize(),
                                    LightSettings = Lights.GenRandomLight(),
                                });

                            prev = next;
                        }

                        break;
                    }
            }

            // Write the zones
            foreach (var node in level.Planner.GetZones(director.Bulkhead, null))
            {
                var zone = level.Planner.GetZone(node);

                if (zone != null)
                    layout.Zones.Add(zone);
            }

            //if (director.Bulkhead.HasFlag(Bulkhead.Main))
            //    layout.RollKeyedDoors(director, level.Planner, puzzlePack);

            layout.RollAlarms(puzzlePack);
            layout.RollBloodDoors();
            layout.RollEnemies(director);
            layout.RollErrorAlarm();

            Bins.LevelLayouts.AddBlock(layout);

            return layout;
        }
    }
}
