namespace MyFirstPlugin.DataBlocks
{
    internal class LevelLayout : DataBlock
    {
        public int ZoneAliasStart { get; set; }

        public List<Zone> Zones { get; set; } = new List<Zone>();

        public static LevelLayout Build(Level level)
        {
            var layout = new LevelLayout
            {
                Name = $"{level.Tier}{level.Index} {level.Name}",
                ZoneAliasStart = Generator.Random.Next(5, 900)
            };

            int numZones = 1;

            for (int i = 0; i < numZones; i++)
            {
                var zone = new Zone
                {
                    LocalIndex = i,
                    SubComplex = SubComplex.All,
                    Coverage = new CoverageMinMax { X = 50.0, Y = 50.0 }
                };

                zone.EnemySpawningInZone.Add(
                    new EnemySpawningData());

                layout.Zones.Add(zone);
            }

            return layout;
        }
    }
}
