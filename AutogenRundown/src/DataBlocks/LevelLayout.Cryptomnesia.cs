using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    /// <summary>
    /// Builds the layout for the Cryptomnesia objective. For now this is a basic
    /// GatherSmallItems layout with zones in Reality. Later this will be expanded
    /// to build zones across multiple dimensions.
    /// </summary>
    public void BuildLayout_Cryptomnesia(BuildDirector director, WardenObjective objective, ZoneNode start)
    {
        var startZone = level.Planner.GetZone(start)!;

        // Build a short branch for the items
        var nodes = AddBranch(start, Generator.Between(1, 3), "find_items", (node, zone) =>
        {
            zone.Coverage = objective.GatherRequiredCount > 3
                ? CoverageMinMax.Large_100
                : CoverageMinMax.Medium;

            objective.Gather_PlacementNodes.Add(node);
        });
    }
}
