using MyFirstPlugin.DataBlocks.Alarms;
using MyFirstPlugin.DataBlocks.ZoneData;

namespace MyFirstPlugin.DataBlocks
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

        public static LevelLayout Build(Level level, Bulkhead variant)
        {
            var layout = new LevelLayout
            {
                Name = $"{level.Tier}{level.Index} {level.Name}",
                ZoneAliasStart = GenZoneAliasStart(level.Tier)
            };

            int numZones = GenNumZones(level, variant);

            var puzzlePack = ChainedPuzzle.BuildPack(level.Tier);

            for (int i = 0; i < numZones; i++)
            {
                var zone = new Zone
                {
                    LocalIndex = i,
                    SubComplex = GenSubComplex(level.Complex),
                    Coverage = CoverageMinMax.GenCoverage(),
                    LightSetting = Lights.Light.RedToCyan_1,
                };

                zone.EnemySpawningInZone.Add(new EnemySpawningData());
                zone.TerminalPlacements.Add(new TerminalPlacement());

                layout.Zones.Add(zone);

                if (i == 0)
                    continue;

                // Grab a random puzzle from the puzzle pack
                var puzzle = Generator.Draw(puzzlePack);

                zone.ChainedPuzzleToEnter = puzzle.PersistentId;

                if (puzzle.PersistentId != 0)
                    Bins.ChainedPuzzles.AddBlock(puzzle);
            }

            Bins.LevelLayouts.AddBlock(layout);

            return layout;
        }
    }
}
