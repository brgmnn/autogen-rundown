using Newtonsoft.Json;
using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks
{
    internal record class Hibernating : Generator.ISelectable
    {
        public double Weight { get; set; } = 1.0;

        public uint Enemy { get; set; }
    }

    internal record class LevelLayout : DataBlock
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
            /*var enemyDistributionOld = director.Tier switch
            {
                "A" => new List<WeightedDifficulty>
                    {
                        new WeightedDifficulty { Weight = 2.0, Difficulties = { EnemyRoleDifficulty.Easy } },
                        new WeightedDifficulty { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Medium } },
                    },

                "B" => new List<WeightedDifficulty>
                    {
                        new WeightedDifficulty { Weight = 2.0, Difficulties = { EnemyRoleDifficulty.Easy } },
                        new WeightedDifficulty { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Medium } },
                        new WeightedDifficulty { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Hard } }
                    },

                "C" => new List<WeightedDifficulty>
                    {
                        new WeightedDifficulty { Weight = 2.0, Difficulties = { EnemyRoleDifficulty.Medium } },
                        new WeightedDifficulty { Weight = 2.0, Difficulties = { EnemyRoleDifficulty.Medium, EnemyRoleDifficulty.Hard } },
                        new WeightedDifficulty { Weight = 1.0, Difficulties = { EnemyRoleDifficulty.Hard } },
                    },

                _ => new List<WeightedDifficulty>()
            };

            var enemyDistribution = director.Tier switch
            {
                "A" => new List<Hibernating>
                    {
                        // In most cases we will leave enemies to the custom difficulty
                        new Hibernating { Weight = 3.0, Enemy = (uint)AutogenDifficulty.TierA },
                        new Hibernating { Weight = 0.6, Enemy = (uint)Enemy.StrikerGiant },
                        new Hibernating { Weight = 0.4, Enemy = (uint)Enemy.ShooterGiant }
                    },

                _ => new List<Hibernating>() { new Hibernating { Enemy = (uint)Enemy.Striker } }
            };*/

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

                /*while (points > 0)
                {
                    var groupPoints = points switch
                    {
                        > 40 => 20,
                        > 24 => 12,
                        > 16 => 10,
                        > 8 => 4,
                        _ => points + 1
                    };

                    var selectedEnemy = Generator.Select(enemyDistribution);
                    points -= groupPoints;

                    Plugin.Logger.LogDebug($"  Spawning {groupPoints}pts on enemy -> {selectedEnemy.Enemy}");

                    zone.EnemySpawningInZone.Add(
                        new EnemySpawningData
                        {
                            GroupType = EnemyGroupType.Hibernate,
                            Difficulty = (uint)selectedEnemy.Enemy,
                            Points = groupPoints
                        });
                }*/

                /*var selected = Generator.Select(enemyDistributionOld);

                foreach (var difficulty in selected.Difficulties)
                {
                    var enemyPoints = points / selected.Difficulties.Count;

                    // If we have a blood door, reduce the number of enemies that spawn in the zone
                    // by 1/3rd.
                    if (zone.BloodDoor.Enabled)
                        enemyPoints = (int)(enemyPoints * 0.66);

                    zone.EnemySpawningInZone.Add(
                        new EnemySpawningData
                        {
                            GroupType = EnemyGroupType.Hibernate,
                            Difficulty = (uint)difficulty,
                            Points = enemyPoints
                        });
                }*/
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

        public static LevelLayout Build(Level level, BuildDirector director)
        {
            var layout = new LevelLayout(director, level.Settings)
            {
                Name = $"{level.Tier}{level.Index} {level.Name} {director.Bulkhead}",
                ZoneAliasStart = GenZoneAliasStart(level.Tier)
            };

            director.GenZones();

            var objectiveLayerData = level.GetObjectiveLayerData(director.Bulkhead);
            var puzzlePack = ChainedPuzzle.BuildPack(level.Tier);

            if (director.Bulkhead.HasFlag(Bulkhead.Main))
                level.Planner.Connect(new ZoneNode(Bulkhead.Main, 0));

            switch (director.Objective)
            {
                case WardenObjectiveType.ReactorShutdown:
                    {
                        // Create the initial zones
                        for (int i = 0; i < director.ZoneCount; i++)
                        {
                            var fromZone = level.Planner.GetLastZone(director.Bulkhead);

                            var zone = new Zone
                            {
                                LocalIndex = i,
                                BuildFromLocalIndex = fromZone?.ZoneNumber ?? 0,
                                Coverage = CoverageMinMax.GenSize(i),
                                LightSettings = Lights.GenRandomLight(),
                            };

                            if (fromZone != null)
                            {
                                level.Planner.Connect((ZoneNode)fromZone, new ZoneNode(director.Bulkhead, i));
                            }

                            zone.RollAlarms(puzzlePack);

                            layout.Zones.Add(zone);
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
                        var corridor = new Zone
                        {
                            LocalIndex = director.ZoneCount,
                            BuildFromLocalIndex = director.ZoneCount - 1,
                            LightSettings = light,
                            StartPosition = ZoneEntranceBuildFrom.Furthest,
                            StartExpansion = startExpansion,
                            ZoneExpansion = zoneExpansion
                        };
                        corridor.GenReactorCorridorGeomorph(director.Complex);

                        // Create the reactor zone
                        var reactor = new Zone
                        {
                            LocalIndex = director.ZoneCount + 1,
                            BuildFromLocalIndex = corridor.LocalIndex,
                            LightSettings = light,
                            StartPosition = ZoneEntranceBuildFrom.Furthest,
                            StartExpansion = startExpansion,
                            ZoneExpansion = zoneExpansion,
                            ForbidTerminalsInZone = true
                        };
                        reactor.GenReactorGeomorph(director.Complex);
                        reactor.TerminalPlacements = new List<TerminalPlacement>();

                        layout.Zones.Add(corridor);
                        layout.Zones.Add(reactor);

                        // Assign door puzzles
                        corridor.RollAlarms(puzzlePack);
                        reactor.RollAlarms(puzzlePack);

                        break;
                    }

                case WardenObjectiveType.ClearPath:
                    {
                        for (int i = 0; i < director.ZoneCount; i++)
                        {
                            var fromZone = level.Planner.GetLastZone(director.Bulkhead);

                            var zone = new Zone
                            {
                                LocalIndex = i,
                                BuildFromLocalIndex = fromZone?.ZoneNumber ?? 0,
                                Coverage = CoverageMinMax.GenSize(i),
                                LightSettings = Lights.GenRandomLight(),
                            };

                            if (fromZone != null)
                            {
                                level.Planner.Connect((ZoneNode)fromZone, new ZoneNode(director.Bulkhead, i));
                            }

                            zone.RollAlarms(puzzlePack);

                            var subcomplex = GenSubComplex(level.Complex);

                            if (i == director.ZoneCount - 1)
                            {
                                // The final zone is the extraction zone
                                zone.Coverage = new CoverageMinMax { Min = 50, Max = 50 };
                                zone.SubComplex = subcomplex;
                                zone.GenExitGeomorph(level.Complex);
                            }

                            layout.Zones.Add(zone);
                        }

                        break;
                    }

                case WardenObjectiveType.SpecialTerminalCommand:
                case WardenObjectiveType.HsuFindSample:
                case WardenObjectiveType.GatherSmallItems:
                default:
                    {
                        for (int i = 0; i < director.ZoneCount; i++)
                        {
                            var fromZone = level.Planner.GetLastZone(director.Bulkhead);

                            var zone = new Zone
                            {
                                LocalIndex = i,
                                BuildFromLocalIndex = fromZone?.ZoneNumber ?? 0,
                                Coverage = CoverageMinMax.GenSize(i),
                                LightSettings = Lights.GenRandomLight(),
                            };

                            if (fromZone != null)
                            {
                                level.Planner.Connect((ZoneNode)fromZone, new ZoneNode(director.Bulkhead, i));
                            }

                            zone.RollAlarms(puzzlePack);

                            layout.Zones.Add(zone);
                        }

                        break;
                    }
            }

            layout.RollBloodDoors();
            layout.RollEnemies(director);
            layout.RollErrorAlarm();

            Bins.LevelLayouts.AddBlock(layout);

            return layout;
        }
    }
}
