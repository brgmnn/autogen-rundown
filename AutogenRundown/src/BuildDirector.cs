using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown
{
    public enum Complexity
    {
        Low,
        Medium,
        High
    }

    public enum MissionSize
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// Director for building what each level (bulkhead) should be
    /// </summary>
    public class BuildDirector
    {
        public int Points { get; set; } = 0;

        public Complexity Complexity { get; set; } = Complexity.Medium;

        public string Tier { get; set; } = "A";

        public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

        public Complex Complex { get; set; }

        public LevelSettings Settings { get; set; } = new LevelSettings();

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
        public void GenObjective(IEnumerable<WardenObjectiveType> exclude)
        {
            var objectives = new List<WardenObjectiveType>
            {
                WardenObjectiveType.HsuFindSample,
                WardenObjectiveType.ReactorShutdown,
                WardenObjectiveType.GatherSmallItems,
                WardenObjectiveType.ClearPath,
                WardenObjectiveType.SpecialTerminalCommand,
                WardenObjectiveType.RetrieveBigItems,
                WardenObjectiveType.PowerCellDistribution,
                WardenObjectiveType.TerminalUplink,

                // TODO: Would love to enable this, but central generator cluster spawning is just
                // too broken. Probably we wait for either a fix from 10Chambers or we have to
                // investigate modding the game to spawn it.
                //WardenObjectiveType.CentralGeneratorCluster
            };

            // Remove any objectives that are in the exclude list.
            objectives.RemoveAll(o => exclude.Contains(o));

            // These objectives are incompatible with non-Main bulkheads.
            if (!Bulkhead.HasFlag(Bulkhead.Main))
            {
                objectives.Remove(WardenObjectiveType.ClearPath);
                objectives.Remove(WardenObjectiveType.PowerCellDistribution);
            }

            // These objectives are really intended as side quests.
            if (Bulkhead.HasFlag(Bulkhead.Main))
            {
                objectives.Remove(WardenObjectiveType.SpecialTerminalCommand);
            }

            // Only Mining and Tech complexes have geomorphs for these objectives
            if (Complex != Complex.Mining || Complex != Complex.Tech)
            {
                objectives.Remove(WardenObjectiveType.ReactorShutdown);
                objectives.Remove(WardenObjectiveType.CentralGeneratorCluster);
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

                ("A", Bulkhead.Extreme) => 400, // Adjust points down a bit
                ("B", Bulkhead.Extreme) => 500,
                ("C", Bulkhead.Extreme) => 800,
                ("D", Bulkhead.Extreme) => 800,
                ("E", Bulkhead.Extreme) => 800,

                ("A", Bulkhead.Overload) => 400, // Adjust points down a bit
                ("B", Bulkhead.Overload) => 500,
                ("C", Bulkhead.Overload) => 800,
                ("D", Bulkhead.Overload) => 800,
                ("E", Bulkhead.Overload) => 800,

                (_, _) => 600,
            };
        }

        /// <summary>
        /// Number of zones only corresponds to that bulkhead. IT does not include the starting
        /// area. This doesn't matter for Extreme/Overload but it does matter for Main, which is
        /// all loaded into a single level layout.
        ///
        /// Max seems to be 20 zones for one layout.
        /// </summary>
        public void GenZones()
        {
            ZoneCount = (Tier, Bulkhead) switch
            {
                ("A", Bulkhead.Main) => Generator.Random.Next(4, 5),
                ("B", Bulkhead.Main) => Generator.Random.Next(5, 6),
                ("C", Bulkhead.Main) => Generator.Random.Next(6, 7),
                ("D", Bulkhead.Main) => Generator.Random.Next(6, 7),
                ("E", Bulkhead.Main) => Generator.Random.Next(7, 9),

                ("A", _) => Generator.Random.Next(2, 3),
                ("B", _) => Generator.Random.Next(2, 3),
                ("C", _) => Generator.Random.Next(2, 4),
                ("D", _) => Generator.Random.Next(3, 5),
                ("E", _) => Generator.Random.Next(3, 5),

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
            // 20-35 minmax for 1.3 dist => 32.5pts
            //
            // Larger zones get more points. The denominator determins the 1.0 benchmark.
            var factor = ((zone.Coverage.Min + zone.Coverage.Max) / 2) / 30.0;

            // Baseline points will be around 25pts / zone for the baseline size.
            return (int)(factor * (Tier switch
            {
                "A" => Generator.Random.Next(15, 25),
                "B" => Generator.Random.Next(20, 25),
                "C" => Generator.Random.Next(20, 30),
                "D" => Generator.Random.Next(30, 35),
                "E" => Generator.Random.Next(35, 40),

                _ => 25,
            }));
        }
    }
}
