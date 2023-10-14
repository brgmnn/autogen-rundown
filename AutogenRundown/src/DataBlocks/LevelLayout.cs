using Newtonsoft.Json;
using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;

namespace AutogenRundown.DataBlocks
{
    internal record class LevelLayout : DataBlock
    {
        #region hidden data
        [JsonIgnore]
        private BuildDirector director;
        #endregion

        public int ZoneAliasStart { get; set; }

        public List<Zone> Zones { get; set; } = new List<Zone>();

        public LevelLayout(BuildDirector director)
        {
            this.director = director;
        }

        /// <summary>
        /// Roll for blood doors
        /// </summary>
        public void RollBloodDoors()
        {
            var count = 0;
            var (max, chance) = director.Tier switch
            {
                "A" => (0, 0.0),
                "B" => (2, 0.3),
                "C" => (4, 0.2),
                _ => (-1, 0.2)
            };

            foreach (var zone in Zones)
                if (Generator.Flip(chance) && (count++ < max || max == -1))
                    zone.BloodDoor = BloodDoor.Easy;
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
        /// Roll for adding scout patrols to each zone
        /// </summary>
        public void RollScouts()
        {
            // All scouts cost 5pts each
            var (chance, max, scoutPacks) = director.Tier switch
            {
                "A" => (0.2, 2, new List<EnemySpawningData>
                    {
                        EnemySpawningData.Scout with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.Scout with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.Scout with { Points = 10 },
                    }),
                "B" => (0.2, 3, new List<EnemySpawningData>
                    {
                        EnemySpawningData.Scout with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.Scout with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },
                    }),
                "C" => (0.2, 5, new List<EnemySpawningData>
                    {
                        EnemySpawningData.Scout with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.Scout with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.Scout with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 20 },

                        // Chargers
                        EnemySpawningData.ScoutCharger with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.ScoutCharger with { Distribution = EnemyZoneDistribution.ForceOne },
                    }),
                "D" => (0.3, -1, new List<EnemySpawningData>
                    {
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 20 },
                        EnemySpawningData.Scout with { Points = 20 },
                        EnemySpawningData.Scout with { Points = 25 },
                        EnemySpawningData.Scout with { Points = 25 },
                        EnemySpawningData.Scout with { Points = 30 },
                        EnemySpawningData.Scout with { Points = 30 },

                        // Chargers
                        EnemySpawningData.ScoutCharger with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.ScoutCharger with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.ScoutCharger with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.ScoutCharger with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.ScoutCharger with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 15 },

                        // Shadows
                        EnemySpawningData.ScoutShadow with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.ScoutShadow with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 15 },
                    }),
                "E" => (0.3, -1, new List<EnemySpawningData>
                    {
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 20 },
                        EnemySpawningData.Scout with { Points = 20 },
                        EnemySpawningData.Scout with { Points = 30 },
                        EnemySpawningData.Scout with { Points = 30 },

                        // Chargers
                        EnemySpawningData.ScoutCharger with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.ScoutCharger with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 20 },
                        EnemySpawningData.Scout with { Points = 30 },

                        // Shadows
                        EnemySpawningData.ScoutShadow with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.ScoutShadow with { Distribution = EnemyZoneDistribution.ForceOne },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 10 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 15 },
                        EnemySpawningData.Scout with { Points = 20 },
                        EnemySpawningData.Scout with { Points = 30 },
                    }),

                _ => (0.0, 0, new List<EnemySpawningData>())
            };

            var count = 0;

            foreach (var zone in Zones)
            {
                if (Generator.Flip(chance) && (count++ < max || max == -1))
                {
                    var scout = Generator.Draw(scoutPacks);

                    if (scout == null)
                        return;

                    zone.EnemySpawningInZone.Add(scout);
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
            var layout = new LevelLayout(director)
            {
                Name = $"{level.Tier}{level.Index} {level.Name}",
                ZoneAliasStart = GenZoneAliasStart(level.Tier)
            };

            director.GenZones();

            var objectiveLayerData = level.GetObjectiveLayerData(director.Bulkhead);
            var puzzlePack = ChainedPuzzle.BuildPack(level.Tier);

            switch (director.Objective)
            {
                case WardenObjectiveType.ReactorShutdown:
                    {
                        // Create the initial zones
                        for (int i = 0; i < director.ZoneCount; i++)
                        {
                            var zone = new Zone
                            {
                                LocalIndex = i,
                                BuildFromLocalIndex = i == 0 ? 0 : i - 1,
                                Coverage = CoverageMinMax.GenNormalSize(),
                                LightSettings = Lights.GenRandomLight(),
                            };

                            zone.GenEnemies(director);

                            layout.Zones.Add(zone);

                            if (i == 0)
                                continue;

                            // Grab a random puzzle from the puzzle pack
                            var puzzle = Generator.Draw(puzzlePack);

                            zone.ChainedPuzzleToEnter = puzzle.PersistentId;

                            if (puzzle.PersistentId != 0)
                                Bins.ChainedPuzzles.AddBlock(puzzle);
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

                        // Assign enemies
                        corridor.GenEnemies(director);
                        reactor.GenEnemies(director);

                        // Assign door puzzles
                        var corridorPuzzle = Generator.Draw(puzzlePack);
                        corridor.ChainedPuzzleToEnter = corridorPuzzle.PersistentId;

                        var reactorPuzzle = Generator.Draw(puzzlePack);
                        reactor.ChainedPuzzleToEnter = reactorPuzzle.PersistentId;

                        Bins.ChainedPuzzles.AddBlock(corridorPuzzle);
                        Bins.ChainedPuzzles.AddBlock(reactorPuzzle);

                        break;
                    }

                case WardenObjectiveType.ClearPath:
                    {
                        for (int i = 0; i < director.ZoneCount; i++)
                        {
                            var subcomplex = GenSubComplex(level.Complex);

                            var zone = new Zone
                            {
                                LocalIndex = i,
                                Coverage = CoverageMinMax.GenNormalSize(),
                                LightSettings = Lights.GenRandomLight(),

                                // Chain zones together
                                BuildFromLocalIndex = i == 0 ? 0 : i - 1,
                                ZoneExpansion = ZoneExpansion.Forward
                            };

                            zone.GenEnemies(director);

                            if (i == director.ZoneCount - 1)
                            {
                                // The final zone is the extraction zone
                                zone.Coverage = new CoverageMinMax { Min = 50, Max = 50 };
                                zone.SubComplex = subcomplex;
                                zone.GenExitGeomorph(level.Complex);
                            }

                            layout.Zones.Add(zone);

                            if (i == 0)
                                continue;

                            // Grab a random puzzle from the puzzle pack
                            var puzzle = Generator.Draw(puzzlePack);

                            zone.ChainedPuzzleToEnter = puzzle.PersistentId;

                            if (puzzle.PersistentId != 0)
                                Bins.ChainedPuzzles.AddBlock(puzzle);
                        }

                        break;
                    }

                case WardenObjectiveType.SpecialTerminalCommand:
                    {
                        for (int i = 0; i < director.ZoneCount; i++)
                        {
                            // We roughly want to chain zones together. 80% chance we build off the
                            // previous zone, 20% chance we build off the zone before that.
                            var buildFrom = Math.Max(0, i - (Generator.Flip(0.8) ? 1 : 2));
                            var zone = new Zone
                            {
                                LocalIndex = i,
                                Coverage = CoverageMinMax.GenNormalSize(),
                                LightSettings = Lights.GenRandomLight(),
                                BuildFromLocalIndex = buildFrom
                            };

                            zone.GenEnemies(director);

                            layout.Zones.Add(zone);

                            if (i == 0)
                                continue;

                            // Grab a random puzzle from the puzzle pack
                            var puzzle = Generator.Draw(puzzlePack);

                            zone.ChainedPuzzleToEnter = puzzle.PersistentId;

                            if (puzzle.PersistentId != 0)
                                Bins.ChainedPuzzles.AddBlock(puzzle);
                        }

                        break;
                    }

                case WardenObjectiveType.HsuFindSample:
                case WardenObjectiveType.GatherSmallItems:
                default:
                    {
                        for (int i = 0; i < director.ZoneCount; i++)
                        {
                            var zone = new Zone
                            {
                                LocalIndex = i,
                                Coverage = CoverageMinMax.GenNormalSize(),
                                LightSettings = Lights.GenRandomLight(),
                            };

                            zone.GenEnemies(director);

                            layout.Zones.Add(zone);

                            if (i == 0)
                                continue;

                            // Grab a random puzzle from the puzzle pack
                            var puzzle = Generator.Draw(puzzlePack);

                            zone.ChainedPuzzleToEnter = puzzle.PersistentId;

                            if (puzzle.PersistentId != 0)
                                Bins.ChainedPuzzles.AddBlock(puzzle);
                        }

                        break;
                    }
            }

            layout.RollBloodDoors();
            layout.RollErrorAlarm();
            layout.RollScouts();

            Bins.LevelLayouts.AddBlock(layout);

            return layout;
        }
    }
}
