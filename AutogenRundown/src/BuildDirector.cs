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
                ("A", Bulkhead.Main) => 400,
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
                ("E", Bulkhead.Main) => Generator.Random.Next(6, 14),

                ("A", _) => Generator.Random.Next(1, 5),
                ("B", _) => Generator.Random.Next(1, 7),
                ("C", _) => Generator.Random.Next(2, 8),
                ("D", _) => Generator.Random.Next(3, 10),
                ("E", _) => Generator.Random.Next(3, 12),

                (_, _) => 1
            };

            // Assign a random number of points to each zone
            EnemyPointPool = Enumerable.Range(0, ZoneCount)
                .Select(_ => Points / ZoneCount)
                .ToList();
        }
    }
}
