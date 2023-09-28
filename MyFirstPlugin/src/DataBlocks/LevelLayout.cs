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
        {
            // TODO: Add variety
            switch (complex)
            {
                case Complex.Tech:
                    return SubComplex.DataCenter;
                case Complex.Service:
                    return SubComplex.Floodways;
                case Complex.Mining:
                    return SubComplex.Refinery;

                default:
                    return SubComplex.All;
            }
        }

        public static LevelLayout Build(Level level)
        {
            var layout = new LevelLayout
            {
                Name = $"{level.Tier}{level.Index} {level.Name}",
                ZoneAliasStart = GenZoneAliasStart(level.Tier)
            };

            int numZones = 3;

            for (int i = 0; i < numZones; i++)
            {
                var zone = new Zone
                {
                    LocalIndex = i,
                    SubComplex = GenSubComplex(level.Complex),
                    Coverage = new CoverageMinMax { X = 50.0, Y = 50.0 }
                };

                zone.EnemySpawningInZone.Add(
                    new EnemySpawningData());

                layout.Zones.Add(zone);
            }

            Bins.LevelLayouts.AddBlock(layout);

            return layout;
        }
    }
}
