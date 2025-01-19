using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Items;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks
{
    public enum SizeFactor
    {
        Small,
        Medium,
        Large
    }

    public partial record LevelLayout : DataBlock
    {
        #region hidden data
        [JsonIgnore]
        private Level level;

        [JsonIgnore]
        private BuildDirector director;

        [JsonIgnore]
        private RelativeDirection direction;

        [JsonIgnore]
        private LayoutPlanner planner;

        [JsonIgnore]
        private LevelSettings settings;
        #endregion

        public int ZoneAliasStart { get; set; }

        public List<Zone> Zones { get; set; } = new List<Zone>();

        public LevelLayout(Level level, BuildDirector director, LevelSettings settings, LayoutPlanner planner)
        {
            this.director = director;
            this.level = level;
            this.planner = planner;
            this.settings = settings;
        }

        /// <summary>
        /// Roll for door alarms
        /// </summary>
        public void RollAlarms(
            ICollection<(double, int, ChainedPuzzle)> puzzlePack,
            ICollection<(double, int, WavePopulation)> wavePopulationPack,
            ICollection<(double, int, WaveSettings)> waveSettingsPack)
        {
            foreach (var zone in Zones)
                zone.RollAlarms(level, this, puzzlePack, wavePopulationPack, waveSettingsPack);
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
        ///
        /// TODO: we might want to use an enemies pack for building this
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

                        // Zoomers
                        EnemySpawningData.ScoutZoomer with { Points = 5 },
                        EnemySpawningData.ScoutZoomer with { Points = 5 },
                        EnemySpawningData.ScoutZoomer with { Points = 5 },
                        EnemySpawningData.ScoutZoomer with { Points = 10 },
                        EnemySpawningData.ScoutZoomer with { Points = 10 },

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

                        // Nightmare
                        EnemySpawningData.ScoutNightmare with { Points = 5 },
                        EnemySpawningData.ScoutNightmare with { Points = 5 },
                        EnemySpawningData.ScoutNightmare with { Points = 5 },
                        EnemySpawningData.ScoutNightmare with { Points = 10 },
                    }),

                "E" => (0.3, -1, new List<EnemySpawningData>
                    {
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 5 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },

                        // Zoomers
                        EnemySpawningData.ScoutZoomer with { Points = 5 },
                        EnemySpawningData.ScoutZoomer with { Points = 5 },
                        EnemySpawningData.ScoutZoomer with { Points = 5 },
                        EnemySpawningData.ScoutZoomer with { Points = 10 },
                        EnemySpawningData.ScoutZoomer with { Points = 10 },

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

                        // Nightmare
                        EnemySpawningData.ScoutNightmare with { Points = 5 },
                        EnemySpawningData.ScoutNightmare with { Points = 5 },
                        EnemySpawningData.ScoutNightmare with { Points = 5 },
                        EnemySpawningData.ScoutNightmare with { Points = 10 },
                    }),

                _ => (0.0, 0, new List<EnemySpawningData>())
            };
            var bossChance = director.Tier switch
            {
                "C" => 0.2,
                "D" => 0.2,
                "E" => 0.3,
                _ => 0.0
            };

            var scoutCount = 0;
            var zoneNodes = planner.GetZones(director.Bulkhead, null);

            foreach (var node in zoneNodes)
            {
                var zone = planner.GetZone(node);

                if (zone == null)
                {
                    Plugin.Logger.LogWarning($"No zone found for ZoneNode: {node}");
                    continue;
                }

                // Skip adding any enemies to the reactor area
                // TODO: we may want to add a chance for some enemies here
                if (node.Tags.Contains("reactor"))
                    continue;

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
                    chargerChance = 0.2;

                if (settings.Modifiers.Contains(LevelModifiers.ManyChargers))
                    chargerChance = 0.5;
                #endregion

                #region Shadows roll check
                var shadowChance = 0.0;

                if (settings.Modifiers.Contains(LevelModifiers.Shadows))
                    shadowChance = 0.15;

                if (settings.Modifiers.Contains(LevelModifiers.ManyShadows))
                    shadowChance = 0.5;
                #endregion

                #region Hybrid roll check
                var hybridChance = 0.0;

                if (settings.Modifiers.Contains(LevelModifiers.Hybrids))
                    hybridChance = 0.2;
                #endregion

                #region Nightmares roll check
                var nightmaresChance = 0.0;

                if (settings.Modifiers.Contains(LevelModifiers.Nightmares))
                    nightmaresChance = 0.2;

                if (settings.Modifiers.Contains(LevelModifiers.ManyNightmares))
                    nightmaresChance = 0.5;
                #endregion

                // Boss settings
                // TODO: don't have totally independent of zone points
                if (Generator.Flip(bossChance) && settings.EnemyBossPack.Any())
                {
                    var boss = Generator.Draw(settings.EnemyBossPack);

                    if (boss != null)
                    {
                        zone.EnemySpawningInZone.Add(boss);
                        Plugin.Logger.LogDebug($"{Name} -- Zone {zone.LocalIndex} rolled a boss!");
                    }
                }

                var groupChoices = new List<(double, List<AutogenDifficulty>)>
                {
                    (1.0, new List<AutogenDifficulty> { AutogenDifficulty.Base }),

                    // Chargers
                    (chargerChance, new List<AutogenDifficulty> { AutogenDifficulty.Chargers }),
                    (chargerChance, new List<AutogenDifficulty>
                    {
                        AutogenDifficulty.Base,
                        AutogenDifficulty.Chargers
                    }),

                    // Shadows
                    (shadowChance, new List<AutogenDifficulty> { AutogenDifficulty.Shadows }),
                    (shadowChance, new List<AutogenDifficulty>
                    {
                        AutogenDifficulty.Base,
                        AutogenDifficulty.Shadows
                    }),

                    // Hybrid is always mixed
                    (hybridChance, new List<AutogenDifficulty>
                    {
                        AutogenDifficulty.Base,
                        AutogenDifficulty.Hybrids
                    }),

                    // Nightmares
                    (nightmaresChance, new List<AutogenDifficulty> { AutogenDifficulty.Nightmares }),
                    (nightmaresChance, new List<AutogenDifficulty>
                    {
                        AutogenDifficulty.Base,
                        AutogenDifficulty.Nightmares
                    }),
                };

                if (chargerChance > 0 && shadowChance > 0)
                    groupChoices.Add(
                        (0.2, new List<AutogenDifficulty>
                            {
                                AutogenDifficulty.Base,
                                AutogenDifficulty.Chargers,
                                AutogenDifficulty.Shadows
                            }));

                if (chargerChance > 0 && nightmaresChance > 0)
                    groupChoices.Add(
                        (0.2, new List<AutogenDifficulty>
                        {
                            AutogenDifficulty.Base,
                            AutogenDifficulty.Chargers,
                            AutogenDifficulty.Nightmares
                        }));

                if (shadowChance > 0 && nightmaresChance > 0)
                    groupChoices.Add(
                        (0.2, new List<AutogenDifficulty>
                        {
                            AutogenDifficulty.Base,
                            AutogenDifficulty.Shadows,
                            AutogenDifficulty.Nightmares
                        }));

                if (chargerChance > 0 && shadowChance > 0 && hybridChance > 0)
                    groupChoices.Add(
                        (0.1, new List<AutogenDifficulty>
                            {
                                AutogenDifficulty.Chargers,
                                AutogenDifficulty.Shadows,
                                AutogenDifficulty.Hybrids
                            }));

                if (chargerChance > 0 && nightmaresChance > 0 && hybridChance > 0)
                    groupChoices.Add(
                        (0.1, new List<AutogenDifficulty>
                        {
                            AutogenDifficulty.Chargers,
                            AutogenDifficulty.Nightmares,
                            AutogenDifficulty.Hybrids
                        }));

                if (shadowChance > 0 && nightmaresChance > 0 && hybridChance > 0)
                    groupChoices.Add(
                        (0.1, new List<AutogenDifficulty>
                        {
                            AutogenDifficulty.Shadows,
                            AutogenDifficulty.Nightmares,
                            AutogenDifficulty.Hybrids
                        }));

                // TODO: TBD if we like having a room with literally everything in it
                if (chargerChance > 0 && shadowChance > 0 && nightmaresChance > 0 && hybridChance > 0)
                    groupChoices.Add(
                        (0.1, new List<AutogenDifficulty>
                        {
                            AutogenDifficulty.Chargers,
                            AutogenDifficulty.Shadows,
                            AutogenDifficulty.Nightmares,
                            AutogenDifficulty.Hybrids
                        }));


                var groups = Generator.Select(groupChoices);

                Plugin.Logger.LogDebug($"{Name} -- Zone {zone.LocalIndex} has {points}pts for enemies. Groups: {string.Join(", ", groups)}");

                // By default we will just let the spawning data allocate out groups. If there
                // are multiple groups we just spawn equal numbers of them and let the game
                // divide that up into portions.
                foreach (var group in groups)
                    zone.EnemySpawningInZone.Add(
                        new EnemySpawningData
                        {
                            GroupType = EnemyGroupType.Hibernate,
                            Difficulty = director.Tier switch
                            {
                                "A" => (uint)(AutogenDifficulty.TierA | group),
                                "B" => (uint)(AutogenDifficulty.TierB | group),
                                "C" => (uint)(AutogenDifficulty.TierC | group),
                                "D" => (uint)(AutogenDifficulty.TierD | group),
                                "E" => (uint)(AutogenDifficulty.TierE | group),
                                _ => (uint)(AutogenDifficulty.TierC | group)
                            },
                            Points = points / groups.Count()
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
                var candidates = Zones.Where(z => z.LocalIndex != 0 && z.Alarm == ChainedPuzzle.None && z.LocalIndex != Zones.Count - 1);

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

                zone.Alarm = ChainedPuzzle.FindOrPersist(puzzle);

                // Give a flat chance of being able to turn off the alarm.
                if (Generator.Flip(0.5))
                {
                    zone.TurnOffAlarmOnTerminal = true;

                    var node = planner.GetZoneNode(zone.LocalIndex);
                    var branchOpenZones = planner.GetOpenZones(director.Bulkhead, node.Branch);

                    // Fallback if there's no open zones in this branch. This will be _hard_.
                    if (branchOpenZones.Count < 1)
                        branchOpenZones = planner.GetOpenZones(director.Bulkhead);

                    var baseNode = Generator.Pick(branchOpenZones);

                    var turnOff = new ZoneNode(director.Bulkhead, planner.NextIndex(director.Bulkhead), $"error_off_{i}");

                    planner.Connect(baseNode, turnOff);
                    planner.AddZone(
                        turnOff,
                        new Zone
                        {
                            Coverage = new CoverageMinMax(Generator.NextDouble(40, 80)),
                            LightSettings = Lights.GenRandomLight(),
                        });

                    var turnOffZone = planner.GetZone(turnOff)!;

                    // Unlock the turn-off zone door when the alarm door has opened.
                    zone.EventsOnDoorScanDone.AddUnlockDoor(director.Bulkhead, turnOff.ZoneNumber);

                    turnOffZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

                    Plugin.Logger.LogDebug($"{Name} -- Zone {zone.LocalIndex} error alarm can be disable in: Zone {turnOff.ZoneNumber}");

                    // For now set the alarm to be in the next zone.
                    zone.TerminalPuzzleZone.LocalIndex = turnOff.ZoneNumber;

                    // TODO: remove when we move roll alarms to use planner entirely.
                    Zones.Add(turnOffZone);
                }
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
        /// Generates a number to be used for level layout generation based on size factors for the inputs
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="bulkhead"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        int GenNumZones(string tier, Bulkhead bulkhead, SizeFactor size)
            => (tier, bulkhead, size) switch
            {
                ("A", Bulkhead.Main, _) => 1,
                ("A", Bulkhead.Extreme, _) => 1,
                ("A", Bulkhead.Overload, _) => 1,

                ("B", Bulkhead.Main, _) => 1,
                ("B", Bulkhead.Extreme, _) => 1,
                ("B", Bulkhead.Overload, _) => 1,

                ("C", Bulkhead.Main, _) => 1,
                ("C", Bulkhead.Extreme, _) => 1,
                ("C", Bulkhead.Overload, _) => 1,

                ("D", Bulkhead.Main, _) => 1,
                ("D", Bulkhead.Extreme, _) => 1,
                ("D", Bulkhead.Overload, _) => 1,

                ("E", Bulkhead.Main, _) => 1,
                ("E", Bulkhead.Extreme, _) => 1,
                ("E", Bulkhead.Overload, _) => 1,

                (_, _, _) => 1
            };

        /// <summary>
        /// Builds a branch, connecting zones and returning the last zone.
        /// </summary>
        /// <param name="baseNode"></param>
        /// <param name="zoneCount"></param>
        /// <param name="branch"></param>
        /// <returns>The last zone node in the branch</returns>
        ZoneNode BuildBranch(ZoneNode baseNode, int zoneCount, string branch = "primary")
        {
            var prev = baseNode;

            if (zoneCount < 1)
                return prev;

            // Generate the zones for this branch
            for (var i = 0; i < zoneCount; i++)
            {
                var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                var next = new ZoneNode(director.Bulkhead, zoneIndex, branch);
                var nextZone = new Zone
                {
                    Coverage = CoverageMinMax.GenNormalSize(),
                    LightSettings = Lights.GenRandomLight(),
                };
                nextZone.RollFog(level);

                level.Planner.Connect(prev, next);
                level.Planner.AddZone(next, nextZone);

                prev = next;
            }

            return prev;
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
        /// <param name="direction">What direction we should build this level layout for</param>
        /// <returns></returns>
        public static LevelLayout Build(
            Level level,
            BuildDirector director,
            WardenObjective objective,
            RelativeDirection direction)
        {
            var layout = new LevelLayout(level, director, level.Settings, level.Planner)
            {
                Name = $"{level.Tier}{level.Index} {level.Name} {director.Bulkhead}",
                ZoneAliasStart = level.GetZoneAliasStart(director.Bulkhead),
                direction = direction
            };

            director.GenZones();

            var puzzlePack = ChainedPuzzle.BuildPack(level.Tier, level.Settings);
            var wavePopulationPack = WavePopulation.BuildPack(level.Tier, level.Settings);
            var waveSettingsPack = WaveSettings.BuildPack(level.Tier);

            Plugin.Logger.LogDebug($"Building layout ({layout.Name}), Objective = {objective.Type}");

            if (objective.Type == WardenObjectiveType.RetrieveBigItems)
                Plugin.Logger.LogDebug($" -- Retrieve Item(s) = {objective.RetrieveItems.First()}");

            BuildStartingArea(level, director);

            switch (director.Objective)
            {
                /**
                 * Reactor startup has quite a complicated layout construction for the fetch codes version
                 * */
                case WardenObjectiveType.ReactorStartup:
                    {
                        var entrance = level.Planner.GetExactZones(director.Bulkhead).First();

                        if (objective.ReactorStartupGetCodes == true)
                            layout.BuildLayout_ReactorStartup_FetchCodes(director, objective, entrance);
                        else
                            layout.BuildLayout_ReactorStartup_Simple(director, objective, entrance);

                        break;
                    }

                /**
                 * In some ways similar to the ReactorStartup mission but much shorter/simpler
                 * */
                case WardenObjectiveType.ReactorShutdown:
                {
                    var start = level.Planner.GetLastZone(director.Bulkhead);

                    layout.BuildLayout_ReactorShutdown(director, objective, start);
                    break;
                }

                /**
                 * Clear Path expeditions should generally be a lot of fun and tough to clear through
                 * */
                case WardenObjectiveType.ClearPath:
                    {
                        var prev = level.Planner.GetExactZones(director.Bulkhead).First();

                        for (int i = 1; i < director.ZoneCount; i++)
                        {
                            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                            var next = new ZoneNode(director.Bulkhead, zoneIndex);
                            var nextZone = new Zone
                            {
                                Coverage = CoverageMinMax.GenNormalSize(),
                                LightSettings = Lights.GenRandomLight(),
                            };

                            nextZone.RollFog(level);

                            level.Planner.Connect(prev, next);
                            level.Planner.AddZone(next, nextZone);

                            prev = next;
                        }

                        var last = prev;
                        var lastZone = level.Planner.GetZone(last)!;

                        var secondLast = (ZoneNode)level.Planner.GetBuildFrom(last)!;
                        var secondLastZone = level.Planner.GetZone(secondLast)!;

                        var subcomplex = GenSubComplex(level.Complex);

                        // Some adjustments to try and reduce the chance of the exit geo not
                        // spawning due to being trapped by a small penultimate zone
                        secondLastZone.ZoneExpansion = ZoneExpansion.Expansional;
                        secondLastZone.Coverage = new CoverageMinMax { Min = 35, Max = 45 };
                        lastZone.StartPosition = ZoneEntranceBuildFrom.Furthest;

                        // The final zone is the extraction zone
                        lastZone.Coverage = new CoverageMinMax { Min = 50, Max = 50 };
                        lastZone.SubComplex = subcomplex;
                        lastZone.GenExitGeomorph(level.Complex);

                        break;
                    }

                /**
                 *
                 */
                case WardenObjectiveType.SpecialTerminalCommand:
                {
                    var start = level.Planner.GetLastZone(director.Bulkhead);

                    layout.BuildLayout_SpecialTerminalCommand(director, objective, start);
                    break;
                }

                /**
                 * Big items are often single, but we can spawn multiple big items (up to 4 for
                 * E levels). Custom logic for interesting geo's should be added here.
                 * */
                case WardenObjectiveType.RetrieveBigItems:
                    {
                        var start = level.Planner.GetLastZone(director.Bulkhead);

                        layout.BuildLayout_RetrieveBigItems(director, objective, start);
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
                        entranceZone.RollFog(level);
                        entrance.MaxConnections = 1;
                        level.Planner.UpdateNode(entrance);

                        // Zone 2 is a hub zone for branches where generators live
                        var hubIndex = level.Planner.NextIndex(director.Bulkhead);
                        var hub = new ZoneNode(director.Bulkhead, hubIndex);
                        hub.MaxConnections = 3;

                        var zone = new Zone { LightSettings = Lights.GenRandomLight() };
                        zone.GenHubGeomorph(director.Complex);
                        zone.RollFog(level);

                        level.Planner.Connect(entrance, hub);
                        level.Planner.AddZone(hub, zone);

                        // Builds a branch with a generator in it. Reused for a lot of places.
                        void BuildGeneratorBranch(ZoneNode baseNode, string branch)
                        {
                            var objectiveLayerData = level.GetObjectiveLayerData(director.Bulkhead);
                            var branchZoneCount = Generator.Random.Next(2, 3);
                            var prev = baseNode;

                            // Generate the zones for this generators branch
                            for (int i = 0; i < branchZoneCount; i++)
                            {
                                var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                                var next = new ZoneNode(director.Bulkhead, zoneIndex, branch);
                                var nextZone = new Zone
                                {
                                    Coverage = CoverageMinMax.GenNormalSize(),
                                    LightSettings = Lights.GenRandomLight(),
                                };
                                nextZone.RollFog(level);

                                level.Planner.Connect(prev, next);
                                level.Planner.AddZone(next, nextZone);

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

                        // Create base branches for each generator
                        for (int g = 0; g < Math.Min(objective.PowerCellsToDistribute, 3); g++)
                        {
                            BuildGeneratorBranch(hub, $"generator_{g}");
                        }

                        if (objective.PowerCellsToDistribute > 3)
                        {
                            var secondHubBase = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead, "generator_2")!;

                            // Special case where we need one more zone. Just add another generator branch.
                            if (objective.PowerCellsToDistribute == 4)
                            {
                                BuildGeneratorBranch(secondHubBase, "generator_3");
                            }
                            else if (objective.PowerCellsToDistribute == 5)
                            {
                                // Case where we want two more zones. Add a hub with two more cells
                                var hub2 = new ZoneNode(director.Bulkhead, level.Planner.NextIndex(director.Bulkhead));
                                hub2.MaxConnections = 3;

                                var zoneHub2 = new Zone { LightSettings = Lights.GenRandomLight() };
                                zoneHub2.GenHubGeomorph(director.Complex);
                                zoneHub2.RollFog(level);

                                level.Planner.Connect(secondHubBase, hub2);
                                level.Planner.AddZone(hub2, zoneHub2);

                                for (int g = 3; g < 5; g++)
                                {
                                    BuildGeneratorBranch(hub2, $"generator_{g}");
                                }
                            }
                        }

                        break;
                    }

                /**
                 * --- DO NOT USE ---
                 * This objective is completely bugged
                 * */
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
                            SpawnData = new List<ItemSpawn>
                                {
                                    new ItemSpawn { Item = Items.Item.PowerCell },
                                    new ItemSpawn { Item = Items.Item.PowerCell }
                                }
                        };
                        Bins.BigPickupDistributions.AddBlock(pickup);

                        var last = prev;
                        var lastZone = (Zone)level.Planner.GetZone(last)!;

                        lastZone.BigPickupDistributionInZone = pickup.PersistentId;

                        break;
                    }

                /**
                 * Survival is a very custom objective
                 */
                case WardenObjectiveType.Survival:
                {
                    var start = level.Planner.GetLastZone(director.Bulkhead);

                    layout.BuildLayout_Survival(director, objective, start);
                    break;
                }

                /**
                 * Terminal Uplink
                 */
                case WardenObjectiveType.TerminalUplink:
                {
                    var start = level.Planner.GetLastZone(director.Bulkhead);

                    layout.BuildLayout_TerminalUplink(director, objective, start);
                    break;
                }

                /**
                 * Survival is a very custom objective
                 */
                case WardenObjectiveType.TimedTerminalSequence:
                {
                    var start = level.Planner.GetLastZone(director.Bulkhead)!;

                    layout.BuildLayout_TimedTerminalSequence(director, objective, (ZoneNode)start);
                    break;
                }

                /**
                 * For level generation these objectives follow a more generic pattern of creating zones for those
                 * areas without too much extra going in to it for now
                 */
                case WardenObjectiveType.HsuFindSample:
                case WardenObjectiveType.GatherSmallItems:
                default:
                {
                    var start = level.Planner.GetLastZone(director.Bulkhead);

                    if (start == null)
                    {
                        Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
                        throw new Exception("No zone node returned");
                    }

                    layout.BuildBranch((ZoneNode)start, director.ZoneCount, "find_items");

                    break;
                }
            }

            //layout.RollKeyedDoors();

            // Attempt to reduce the chance of generation locking where zones cannot be placed
            level.Planner.PlanBulkheadPlacements(director.Bulkhead, direction);

            var numberOfFogZones = level.Planner.GetZones(director.Bulkhead, null)
                .Select(node => level.Planner.GetZone(node))
                .Where(zone => zone != null ? zone.InFog : false)
                .Count();

            Plugin.Logger.LogDebug($"{layout.Name} -- Number of in-fog zones: {numberOfFogZones}");

            // Write the zones
            foreach (var node in level.Planner.GetZones(director.Bulkhead, null))
            {
                var zone = level.Planner.GetZone(node);

                if (zone != null)
                {
                    // Crude way to force the direction of zones for now
                    // TODO: This should live in the zone node building process. We should be able to set the direction
                    //       of these more dynamically
                    if (node.Branch == "primary")
                        zone.ZoneExpansion = direction.Forward;

                    // if (node.Bulkhead.HasFlag(Bulkhead.StartingArea))
                    //     zone.StartPosition = ZoneEntranceBuildFrom.Furthest;

                    layout.Zones.Add(zone);

                    Plugin.Logger.LogDebug(
                        $"{layout.Name} -- Zone_{zone.LocalIndex} " +
                        $"number={layout.ZoneAliasStart + zone.LocalIndex} " +
                        $"pid={zone.PersistentId} -- " +
                        $"branch={node.Branch} -- " +
                        $"Lights={zone.LightSettings}, InFog={zone.InFog}, Tags={node.Tags}");
                }
            }

            // TODO: most or all of these need to be moved
            layout.RollAlarms(puzzlePack, wavePopulationPack, waveSettingsPack);
            layout.RollBloodDoors();
            layout.RollEnemies(director);
            layout.RollErrorAlarm();

            Bins.LevelLayouts.AddBlock(layout);

            return layout;
        }
    }
}
