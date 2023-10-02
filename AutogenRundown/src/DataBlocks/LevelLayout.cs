using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.ZoneData;
using System.Security.AccessControl;
using UnityEngine.TestTools;

namespace AutogenRundown.DataBlocks
{
    internal class LevelLayout : DataBlock
    {
        public int ZoneAliasStart { get; set; }

        public List<Zone> Zones { get; set; } = new List<Zone>();

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
            var layout = new LevelLayout
            {
                Name = $"{level.Tier}{level.Index} {level.Name}",
                ZoneAliasStart = GenZoneAliasStart(level.Tier)
            };

            var objectiveLayerData = level.GetObjectiveLayerData(director.Bulkhead);
            var puzzlePack = ChainedPuzzle.BuildPack(level.Tier);

            int numZones = GenNumZones(level, director.Bulkhead);

            switch (director.Objective)
            {
                case WardenObjectiveType.ClearPath:
                    {
                        for (int i = 0; i < numZones; i++)
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

                            zone.EnemySpawningInZone.Add(new EnemySpawningData());

                            if (i == numZones - 1)
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
                        for (int i = 0; i < numZones; i++)
                        {
                            var zone = new Zone
                            {
                                LocalIndex = i,
                                Coverage = CoverageMinMax.GenCoverage(),
                                LightSetting = Lights.Light.RedToCyan_1,
                            };

                            zone.EnemySpawningInZone.Add(new EnemySpawningData());

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

            Bins.LevelLayouts.AddBlock(layout);

            return layout;
        }
    }
}
