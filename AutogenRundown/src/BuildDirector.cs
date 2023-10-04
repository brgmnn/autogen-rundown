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

        public int Credits { get; set; } = 0;

        public Complexity Complexity { get; set; } = Complexity.Medium;

        public string Tier { get; set; } = "A";

        public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

        public WardenObjectiveType Objective { get; set; } = WardenObjectiveType.GatherSmallItems;

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
    }
}
