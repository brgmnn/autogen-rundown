using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Objectives;

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
        public int Points { get; set; } = 0;

        public Complexity Complexity { get; set; } = Complexity.Medium;

        public string Tier { get; set; } = "A";

        public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

        public Complex Complex { get; set; }

        public WardenObjectiveType Objective { get; set; } = WardenObjectiveType.GatherSmallItems;

        #region Zones
        public int ZoneCount { get; set; } = 0;

        /// <summary>
        /// Pool of points to draw from for spawning enemies
        /// </summary>
        public List<int> EnemyPointPool { get; set; } = new List<int>();
        #endregion

        /// <summary>
        /// Generates an appropriate objective for this bulkhead level.
        ///
        /// Not all objectives can be selected based on certain criteria. For instance Service
        /// complex has no Reactor geomorph and so cannot have any Reactor objective. ClearPath
        /// only makes sense for Main bulkheads as players must extract to complete the objective.
        /// </summary>
        public void GenObjective()
        {
            var objectives = new List<WardenObjectiveType>
            {
                WardenObjectiveType.ReactorShutdown,
                WardenObjectiveType.GatherSmallItems,
                WardenObjectiveType.ClearPath,
                WardenObjectiveType.HsuActivateSmall,
            };

            if (Bulkhead != Bulkhead.Main)
            {
                objectives.Remove(WardenObjectiveType.ClearPath);
            }

            if (Complex != Complex.Mining || Complex != Complex.Tech)
            {
                objectives.Remove(WardenObjectiveType.ReactorShutdown);
            }

            Objective = Generator.Pick(objectives);
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
                ("A", Bulkhead.Main) => Generator.Random.Next(4, 6),
                ("B", Bulkhead.Main) => Generator.Random.Next(4, 8),
                ("C", Bulkhead.Main) => Generator.Random.Next(5, 9),
                ("D", Bulkhead.Main) => Generator.Random.Next(5, 10), // TODO: playtest
                ("E", Bulkhead.Main) => Generator.Random.Next(6, 11), // TODO: playtest

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
            // Adjust points by a factor of the zone's coverage size. Average the min/max values
            // to get the mean zone size and divide by 25.0 to set a room size of 25 as the
            // baseline.
            //
            // Larger zones get more points.
            var factor = ((zone.Coverage.Min + zone.Coverage.Max) / 2) / 25.0;

            // Baseline points will be around 25pts / zone for the baseline size.
            return (int)(factor * (Tier switch
            {
                "A" => Generator.Random.Next(20, 30),
                "B" => Generator.Random.Next(25, 30),
                "C" => Generator.Random.Next(25, 35),
                "D" => Generator.Random.Next(25, 40),
                "E" => Generator.Random.Next(25, 45), // TODO: is this sane?

                _ => Generator.Random.Next(25, 30),
            }));
        }
    }
}
