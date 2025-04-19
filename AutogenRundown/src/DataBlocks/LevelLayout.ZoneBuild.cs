using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
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
    /// <param name="newNode"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) AddZone(ZoneNode source, ZoneNode? newNode = null)
    {
        if (newNode is null)
            newNode = new ZoneNode();

        var node = (ZoneNode)newNode;
        var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
        var next = node with { Bulkhead = director.Bulkhead, ZoneNumber = zoneIndex };

        if (next.Tags == null)
            next.Tags = new Tags();

        var nextZone = new Zone(level.Tier)
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
            var nextZone = new Zone(level.Tier)
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
            var (next, nextZone) = AddZone(prev, new ZoneNode { Branch = branch });

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

    /// <summary>
    /// Creates the very first zone in the level, which is the elevator zone. This zone doesn't
    /// lead from anywhere, so there's no source.
    /// </summary>
    /// <param name="newNode"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) CreateElevatorZone(ZoneNode? newNode = null)
    {
        if (newNode is null)
            newNode = new ZoneNode
            {
                Bulkhead = Bulkhead.Main | Bulkhead.StartingArea,
                Branch = "primary",
                MaxConnections = 2
            };

        var node = (ZoneNode)newNode;
        var zoneIndex = level.Planner.NextIndex(Bulkhead.Main);

        if (zoneIndex != 0)
        {
            Plugin.Logger.LogError($"Tried to create elevator node when one already exists! zoneIndex = {zoneIndex}");
            throw new Exception("Tried to create elevator node when one already exists!");
        }

        var elevator = node with
        {
            Bulkhead = Bulkhead.Main | Bulkhead.StartingArea,
            ZoneNumber = zoneIndex
        };
        elevator.Tags ??= new Tags();

        var elevatorZone = new Zone(level.Tier)
        {
            Coverage = new CoverageMinMax { Min = 25, Max = 35 },
            LightSettings = Lights.GenRandomLight(),
            EnemyPointsMultiplier = 0.6
        };
        elevatorZone.RollFog(level);

        level.Planner.Connect(elevator);
        Zone zone = level.Planner.AddZone(elevator, elevatorZone)!;

        return (elevator, zone);
    }
    #endregion
}
