using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.ZoneData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    internal class LevelLayout : DataBlock
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
        /// Rolls for whether we should add an error alarm to this level layout.
        /// </summary>
        /// <param name="factor">
        /// Adjustment factor for whether an alarm should be rolled
        /// </param>
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

        /// <summary>
        /// Generates the number of zones
        /// </summary>
        /// <param name="level"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public static int GenNumZones(Level level, Bulkhead variant)
        {
            return (level.Tier, variant) switch
            {
                ("A", Bulkhead.Main) => Generator.Random.Next(3, 6),
                ("B", Bulkhead.Main) => Generator.Random.Next(4, 8),
                ("C", Bulkhead.Main) => Generator.Random.Next(4, 9),
                ("D", Bulkhead.Main) => Generator.Random.Next(5, 11),
                ("E", Bulkhead.Main) => Generator.Random.Next(6, 14),

                ("A", _) => Generator.Random.Next(1, 5),
                ("B", _) => Generator.Random.Next(1, 7),
                ("C", _) => Generator.Random.Next(2, 8),
                ("D", _) => Generator.Random.Next(3, 10),
                ("E", _) => Generator.Random.Next(3, 12),

                (_, _) => 1
            };
        }

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
                case WardenObjectiveType.ClearPath:
                    {
                        for (int i = 0; i < director.ZoneCount; i++)
                        {
                            var subcomplex = GenSubComplex(level.Complex);

                            var zone = new Zone
                            {
                                LocalIndex = i,
                                Coverage = CoverageMinMax.GenCoverage(),
                                LightSetting = Lights.Light.RedToCyan_1,

                                // Chain zones together
                                BuildFromLocalIndex = i == 0 ? 0 : i - 1,
                                ZoneExpansion = ZoneExpansion.Forward
                            };

                            zone.GenEnemies(director);

                            if (i == director.ZoneCount - 1)
                            {
                                // The final zone is the extraction zone
                                zone.Coverage = new CoverageMinMax { X = 50, Y = 50 };
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

                            if (Generator.Flip(0.2))
                                zone.BloodDoor = BloodDoor.Easy;
                        }

                        break;
                    }

                case WardenObjectiveType.GatherSmallItems:
                default:
                    {
                        for (int i = 0; i < director.ZoneCount; i++)
                        {
                            var zone = new Zone
                            {
                                LocalIndex = i,
                                Coverage = CoverageMinMax.GenCoverage(),
                                LightSetting = Lights.Light.RedToCyan_1,
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

                            if (Generator.Flip(0.2))
                                zone.BloodDoor = BloodDoor.Easy;
                        }

                        break;
                    }
            }

            layout.RollErrorAlarm();

            Bins.LevelLayouts.AddBlock(layout);

            return layout;
        }
    }
}
