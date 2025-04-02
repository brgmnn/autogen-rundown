using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Utils;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    #region Basic layout
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
        Zone zone = level.Planner.AddZone(next, nextZone)!;

        return (next, zone);
    }

    /// <summary>
    /// Basic function to add a new zone onto a base zone node. Returns the
    /// newly created ZoneNode and Zone as a tuple for further use
    /// </summary>
    /// <param name="baseNode"></param>
    /// <param name="branch"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) AddZone(ZoneNode baseNode, string branch = "primary")
    {
        var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
        var next = new ZoneNode(director.Bulkhead, zoneIndex, branch);
        var nextZone = new Zone
        {
            Coverage = CoverageMinMax.GenNormalSize(),
            LightSettings = Lights.GenRandomLight(),
        };
        nextZone.RollFog(level);

        level.Planner.Connect(baseNode, next);
        nextZone = level.Planner.AddZone(next, nextZone);

        return (next, nextZone);
    }

    /// <summary>
    /// Builds a branch, connecting zones and returning the last zone.
    /// </summary>
    /// <param name="baseNode"></param>
    /// <param name="zoneCount"></param>
    /// <param name="branch"></param>
    /// <returns>The last zone node in the branch</returns>
    [Obsolete("Use and rename AddBranch instead")]
    public ZoneNode BuildBranch(ZoneNode baseNode, int zoneCount, string branch = "primary")
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="baseNode"></param>
    /// <param name="zoneCount"></param>
    /// <param name="branch"></param>
    /// <param name="zoneCallback"></param>
    /// <returns></returns>
    public ICollection<ZoneNode> AddBranch(
        ZoneNode baseNode,
        int zoneCount,
        string branch = "primary",
        Action<ZoneNode, Zone>? zoneCallback = null)
    {
        var prev = baseNode;

        // If we don't have any zones to add, just return the base node.
        // Note that we do not invoke the zone callback in this case.
        if (zoneCount < 1)
            return new List<ZoneNode> { baseNode };

        var insertedNodes = new List<ZoneNode>();

        // Generate the zones for this branch
        for (var i = 0; i < zoneCount; i++)
        {
            var (next, nextZone) = AddZone(prev, branch);

            insertedNodes.Add(next);
            zoneCallback?.Invoke(next, nextZone);

            prev = next;
        }

        return insertedNodes;
    }

    /// <summary>
    /// Wraps AddBranch() with an automatic callback to set the zone expansion
    /// to the bulkheads forward direction.
    /// </summary>
    /// <param name="baseNode"></param>
    /// <param name="zoneCount"></param>
    /// <param name="branch"></param>
    /// <param name="zoneCallback"></param>
    /// <returns></returns>
    public ICollection<ZoneNode> AddBranch_Forward(
        ZoneNode baseNode,
        int zoneCount,
        string branch = "primary",
        Action<ZoneNode, Zone>? zoneCallback = null)
        => AddBranch(baseNode, zoneCount, branch,
            (node, zone) =>
            {
                var direction = level.Settings.GetDirections(director.Bulkhead).Forward;
                zone.ZoneExpansion = direction;
                zone.SetStartExpansionFromExpansion();

                zoneCallback?.Invoke(node, zone);
            });

    /// <summary>
    /// Wraps AddBranch() with an automatic callback to set the zone expansion
    /// to either the left or right relative direction. Nice in conjunction
    /// with AddBranch_Forward() to make side areas
    /// </summary>
    /// <param name="baseNode"></param>
    /// <param name="zoneCount"></param>
    /// <param name="branch"></param>
    /// <param name="zoneCallback"></param>
    /// <returns></returns>
    public ICollection<ZoneNode> AddBranch_Side(
        ZoneNode baseNode,
        int zoneCount,
        string branch = "primary",
        Action<ZoneNode, Zone>? zoneCallback = null)
    {
        var direction = Generator.Flip()
            ? level.Settings.GetDirections(director.Bulkhead).Left
            : level.Settings.GetDirections(director.Bulkhead).Right;

        return AddBranch(baseNode, zoneCount, branch,
            (node, zone) =>
            {
                zone.ZoneExpansion = direction;
                zone.SetStartExpansionFromExpansion();

                zoneCallback?.Invoke(node, zone);
            });
    }
    #endregion
}
