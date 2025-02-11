using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Utils;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// Adds a new zone onto the source zone node
    /// </summary>
    /// <param name="source"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) AddZone(ZoneNode source, ZoneNode node)
    {
        var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
        var next = node with { Bulkhead = director.Bulkhead, ZoneNumber = zoneIndex };

        if (next.Tags == null)
            next.Tags = new Tags();

        var nextZone = new Zone
        {
            Coverage = CoverageMinMax.GenNormalSize(),
            LightSettings = Lights.GenRandomLight(),
        };
        nextZone.RollFog(level);

        level.Planner.Connect(source, next);
        var zone = level.Planner.AddZone(next, nextZone);

        return (next, zone);
    }

    /// <summary>
    /// Builds a branch, connecting zones and returning the last zone.
    /// </summary>
    /// <param name="baseNode"></param>
    /// <param name="zoneCount"></param>
    /// <param name="branch"></param>
    /// <returns>The last zone node in the branch</returns>
    ZoneNode BuildBranch(ZoneNode baseNode, int zoneCount, string branch = "primary")
    {
        var prev = baseNode;

        if (zoneCount < 1)
            return prev;

        // Generate the zones for this branch
        for (var i = 0; i < zoneCount; i++)
        {
            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
            var next = new ZoneNode(director.Bulkhead, zoneIndex, branch);
            var nextZone = new Zone
            {
                Coverage = CoverageMinMax.GenNormalSize(),
                LightSettings = Lights.GenRandomLight(),
            };
            nextZone.RollFog(level);

            level.Planner.Connect(prev, next);
            level.Planner.AddZone(next, nextZone);

            prev = next;
        }

        return prev;
    }
}
