using AutogenRundown.DataBlocks;

namespace AutogenRundown
{
    enum Complexity
    {
        Low,
        Medium,
        High
    }

    enum MissionSize
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// Director for building what each level (bulkhead) should be
    /// </summary>
    internal class BuildDirector
    {
        /// <summary>
        /// Main objective types
        /// </summary>
        private static WardenObjectiveType[] mainObjectives = new WardenObjectiveType[]
        {
            WardenObjectiveType.ClearPath,
            WardenObjectiveType.GatherSmallItems,
        };

        /// <summary>
        /// Extreme / Overload objective types. Some objectives for main aren't as appropriate for
        /// non-main objectives. Examples are ClearPath.
        /// </summary>
        private static WardenObjectiveType[] nonMainObjectives = new WardenObjectiveType[]
        {
            WardenObjectiveType.GatherSmallItems,
            WardenObjectiveType.HsuActivateSmall,
        };

        public int Points { get; set; } = 0;

        public Complexity Complexity { get; set; } = Complexity.Medium;

        public string Tier { get; set; } = "A";

        public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

        public WardenObjectiveType Objective { get; set; } = WardenObjectiveType.GatherSmallItems;

        #region Zones
        public int ZoneCount { get; set; } = 0;

        /// <summary>
        /// Pool of points to draw from for spawning enemies
        /// </summary>
        public List<int> EnemyPointPool { get; set; } = new List<int>();
        #endregion

        public void GenObjective()
        {
            Objective = Bulkhead switch
            {
                Bulkhead.Main => Generator.Pick(mainObjectives),
                Bulkhead.Extreme => Generator.Pick(mainObjectives),
                Bulkhead.Overload => Generator.Pick(mainObjectives),
                _ => Generator.Pick(mainObjectives),
            };
        }

        public void GenPoints()
        {
            Points = (Tier, Bulkhead) switch
            {
                ("A", Bulkhead.Main) => 400, // Adjust points down a bit
                ("B", Bulkhead.Main) => 500,
                ("C", Bulkhead.Main) => 800,
                ("D", Bulkhead.Main) => 800,
                ("E", Bulkhead.Main) => 800,

                (_, _) => 600,
            };
        }

        public void GenZones()
        {
            ZoneCount = (Tier, Bulkhead) switch
            {
                ("A", Bulkhead.Main) => Generator.Random.Next(3, 6),
                ("B", Bulkhead.Main) => Generator.Random.Next(4, 8),
                ("C", Bulkhead.Main) => Generator.Random.Next(4, 9),
                ("D", Bulkhead.Main) => Generator.Random.Next(5, 11),
                ("E", Bulkhead.Main) => Generator.Random.Next(6, 14), // TODO: this is too much

                ("A", _) => Generator.Random.Next(1, 5),
                ("B", _) => Generator.Random.Next(1, 7),
                ("C", _) => Generator.Random.Next(2, 8),
                ("D", _) => Generator.Random.Next(3, 10),
                ("E", _) => Generator.Random.Next(3, 12), // TODO: this is also too much

                (_, _) => 1
            };

            // Assign a random number of points to each zone
            EnemyPointPool = Enumerable.Range(0, ZoneCount)
                .Select(_ => Points / ZoneCount)
                .ToList();
        }

        /// <summary>
        /// Randomly generate a point value for hibernating enemies in each zone.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public int GetPoints(Zone zone)
        {
            // Adjust points by a factor of the zone's coverage size. This helps keep large zones
            // from feeling too empty and small zones from feeling too packed.
            // Magnitude =  ~32 for a 20 x 25 size zone, which is relatively small.
            //             ~175 for a 125 x 125 room
            //             ~200 for a 200 x 25 room
            //
            // The sqrt of the magnitude is used to scale the points down a bit for larger zones
            // without penalizing small zones. The final 12.5 is an empirical adjustment value.
            var factor = Math.Sqrt(zone.Coverage.Magnitude) / 12.5;

            return (int)(factor * (Tier switch
            {
                "A" => Generator.Random.Next(50, 80),
                "B" => Generator.Random.Next(60, 90),
                "C" => Generator.Random.Next(70, 100),
                "D" => Generator.Random.Next(80, 110),
                "E" => Generator.Random.Next(80, 120),

                _ => Generator.Random.Next(50, 100),
            }));
        }
    }
}
