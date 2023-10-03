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
        public int Credits { get; set; } = 0;

        public Complexity Complexity { get; set; } = Complexity.Medium;

        public string Tier { get; set; } = "A";

        public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

        public WardenObjectiveType Objective { get; set; } = WardenObjectiveType.GatherSmallItems;
    }
}
