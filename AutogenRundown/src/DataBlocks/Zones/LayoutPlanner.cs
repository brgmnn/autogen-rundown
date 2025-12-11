using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.Utils;

namespace AutogenRundown.DataBlocks.Zones;

public record struct ZoneNode(
    Bulkhead Bulkhead,
    int ZoneNumber,
    string Branch = "primary",
    int MaxConnections = 2)
{
    /// <summary>
    /// Freeform tags field
    /// </summary>
    public Tags Tags { get; set; } = new();

    public ZoneNode() : this(Bulkhead.Main, 0, "primary", 2) { }

    /// <summary>
    /// Two zones are equal if they are in the same bulkhead and have the same zone number.
    /// All other properties are extra and we want to consider them equal with them.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public readonly bool Equals(ZoneNode other)
    {
        return Bulkhead == other.Bulkhead && ZoneNumber == other.ZoneNumber;
    }

    public override int GetHashCode()
        => Bulkhead.GetHashCode() ^ ZoneNumber.GetHashCode();

    public static string ListToString(IEnumerable<ZoneNode> nodes, string separator = ", ")
        => string.Join(separator, nodes.Select(node => node.ToString()));
}

/// <summary>
/// Relative direction records are used to encode the global directions that will be used to
/// map to the relative directions. Good for easier, more portable code when writing zone
/// layouts.
/// </summary>
/// <param name="Forward"></param>
/// <param name="Left"></param>
/// <param name="Right"></param>
/// <param name="Backward"></param>
public record struct RelativeDirection(
    ZoneExpansion Forward,
    ZoneExpansion Left,
    ZoneExpansion Right,
    ZoneExpansion Backward)
{
    public static RelativeDirection Global_Forward = new RelativeDirection(
        ZoneExpansion.Forward,
        ZoneExpansion.Left,
        ZoneExpansion.Right,
        ZoneExpansion.Backward);
    public static RelativeDirection Global_Left = new RelativeDirection(
        ZoneExpansion.Left,
        ZoneExpansion.Backward,
        ZoneExpansion.Forward,
        ZoneExpansion.Right);
    public static RelativeDirection Global_Right = new RelativeDirection(
        ZoneExpansion.Right,
        ZoneExpansion.Forward,
        ZoneExpansion.Backward,
        ZoneExpansion.Left);
    public static RelativeDirection Global_Backward = new RelativeDirection(
        ZoneExpansion.Backward,
        ZoneExpansion.Right,
        ZoneExpansion.Left,
        ZoneExpansion.Forward);
}

public enum Direction
{
    Left,
    Right,
}

/// <summary>
/// Building the zone layout requires some planning. The game can fail to generate a level if
/// there's too much crowding in a particular space. For instance attempting to build too many
/// zones off one zone will crash on load. The planner is to help plan out where to place
/// zones.
/// </summary>
public class LayoutPlanner
{
    private int indexMain = 0;
    private int indexExtreme = 0;
    private int indexOverload = 0;

    private readonly Dictionary<ZoneNode, List<ZoneNode>> graph = new();

    private readonly Dictionary<ZoneNode, Zone> blocks = new();

    private IEnumerable<KeyValuePair<ZoneNode, List<ZoneNode>>> GetSubgraph(
        Bulkhead bulkhead = Bulkhead.All,
        string? branch = "primary")
        => graph.Where(node => (node.Key.Bulkhead & bulkhead) != 0 && (branch == null || node.Key.Branch == branch));

    public override string ToString()
    {
        var debug = string.Join("\n", graph.Select(n =>
        {
            var children = n.Value.Count switch
            {
                0 => "[]",
                1 => ZoneNode.ListToString(n.Value, ",\n    "),
                _ => $"[\n    {ZoneNode.ListToString(n.Value, ",\n    ")}]"
            };

            var zone = GetZone(n.Key);

            return $"  {n.Key} (door={zone?.StartExpansion}, expand={zone?.ZoneExpansion}) => {children}";
        }));
        return $"Graph:\n{debug}";
    }

    /// <summary>
    /// Connects two zones unidirectionally. If the second zone is not specified then the
    /// first zone is just added as an open zone.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void Connect(ZoneNode from, ZoneNode? to = null)
    {
        if (to != null && from != to)
        {
            if (graph.ContainsKey(from))
                graph[from].Add((ZoneNode)to);
            else
                graph.Add(from, new List<ZoneNode> { (ZoneNode)to });

            if (!graph.ContainsKey((ZoneNode)to))
                graph.Add((ZoneNode)to, new List<ZoneNode>());
        }
        else if (!graph.ContainsKey(from))
            graph.Add(from, new List<ZoneNode>());
    }

    /// <summary>
    /// If we ever need to update a ZoneNode after it's been inserted, this will go through
    /// and update all the copies of that zone node in the graph and blocks list
    ///
    /// TODO: Add unit tests for this
    /// </summary>
    /// <param name="node"></param>
    public ZoneNode UpdateNode(ZoneNode node)
    {
        // Update graph[node] key
        if (graph.ContainsKey(node))
        {
            var children = graph[node];
            graph.Remove(node);
            graph.Add(node, children);
        }

        // Update graph[*].list
        foreach (var key in graph.Keys.ToList())
        {
            var children = graph[key]!;

            // Iterate through and update any children that match the new node
            for (int i=0; i<children.Count; i++)
                if (children[i] == node)
                    children[i] = node;
        }

        // Update blocks
        if (blocks.ContainsKey(node))
        {
            var zone = blocks[node];
            blocks.Remove(node);
            blocks.Add(node, zone);
        }

        return node;
    }

    public ZoneNode AddTags(ZoneNode node, params string[] tags)
        => UpdateNode(node with { Tags = node.Tags.Extend(tags) });

    /// <summary>
    /// Handles assigning the right local index and build from index.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="zone"></param>
    public Zone AddZone(ZoneNode node, Zone zone)
    {
        var updatedZone = zone with
        {
            LocalIndex = node.ZoneNumber,
            BuildFromLocalIndex = GetBuildFrom(node)?.ZoneNumber ?? 0,
        };

        try
        {
            blocks.Add(node, updatedZone);
        }
        catch (ArgumentException _)
        {
            Plugin.Logger.LogWarning($"Planner.AddZone() did not add zone as it already exists: node = ${node}");
        }

        return updatedZone;
    }

    /// <summary>
    /// Get's the underlying zone for a given node.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public Zone? GetZone(ZoneNode node)
    {
        if (blocks.TryGetValue(node, out var zone))
            return zone;

        return null;
    }

    /// <summary>
    /// Returns a nodes parent node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public ZoneNode? GetParent(ZoneNode node)
    {
        try
        {
            return graph.First(pair => pair.Value.Contains(node)).Key;
        }
        catch (InvalidOperationException)
        {
            // This should only happen for the elevator node, which has no parent
            return null;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="startNode"></param>
    /// <returns></returns>
    public List<ZoneNode> TraverseToElevator(ZoneNode startNode)
    {
        ZoneNode? node = startNode;
        var path = new List<ZoneNode>();

        while (node != null)
        {
            path.Add((ZoneNode)node);
            node = GetParent((ZoneNode)node);
        }

        return path;
    }

    /// <summary>
    /// Gets a zone node by its index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ZoneNode GetZoneNode(int index)
        => graph.Keys.First(node => node.ZoneNumber == index);

    /// <summary>
    /// Gets the ZoneNode for a given Zone
    /// </summary>
    /// <param name="zone"></param>
    /// <returns></returns>
    public ZoneNode GetNode(Zone zone)
        => blocks.First(entry => entry.Value.Equals(zone)).Key;

    /// <summary>
    /// Gets the last ZoneNode in the branch for a given bulkhead / branch
    /// </summary>
    /// <param name="bulkhead"></param>
    /// <param name="branch"></param>
    /// <returns></returns>
    public ZoneNode GetLastNode(Bulkhead bulkhead, string? branch) => GetSubgraph(bulkhead, branch).Last().Key;

    /// <summary>
    ///
    /// </summary>
    /// <param name="bulkhead"></param>
    /// <returns></returns>
    public int NextIndex(Bulkhead bulkhead) => bulkhead switch
    {
        Bulkhead.Main => indexMain++,
        Bulkhead.Extreme => indexExtreme++,
        Bulkhead.Overload => indexOverload++,

        // Starting area is part of main.
        Bulkhead.StartingArea => indexMain++,
        Bulkhead.StartingArea | Bulkhead.Main => indexMain++,

        _ => throw new ArgumentOutOfRangeException(nameof(bulkhead), bulkhead, null)
    };

    /// <summary>
    /// Gets all zones associated with the given bulkhead
    /// </summary>
    /// <param name="bulkhead"></param>
    /// <returns></returns>
    public List<ZoneNode> GetZones(Bulkhead bulkhead = Bulkhead.All, string? branch = "primary")
        => GetSubgraph(bulkhead, branch)
            .Select(node => node.Key)
            .OrderBy(zone => zone.ZoneNumber)
            .ToList();

    /// <summary>
    /// Gets all zones associated with the given bulkhead. Exlusively only return nodes
    /// exactly matching the given bulkhead.
    /// </summary>
    /// <param name="bulkhead"></param>
    /// <returns></returns>
    public List<ZoneNode> GetExactZones(Bulkhead bulkhead, string? branch = "primary")
        => GetSubgraph(bulkhead, branch)
            .Select(node => node.Key)
            .Where(node => node.Bulkhead == bulkhead)
            .OrderBy(zone => zone.ZoneNumber)
            .ToList();

    /// <summary>
    /// Get's all the next zones building off the source `node` zone
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public List<ZoneNode> GetConnections(ZoneNode node, string? branch = "primary")
    {
        List<ZoneNode> connections;

        if (graph.TryGetValue(node, out connections!))
            return connections.Where(node => branch == null || node.Branch == branch).ToList();

        return new List<ZoneNode>();
    }

    /// <summary>
    /// Finds the zone that a given node should be built from.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public ZoneNode? GetBuildFrom(ZoneNode node)
    {
        foreach (var (from, to) in graph)
            if (to.Contains(node))
                return from;

        return null;
    }

    /// <summary>
    /// Find all zones that are leaf nodes, or dead ends. I.e. they only have one connection
    /// from their previous zone.
    /// </summary>
    /// <returns></returns>
    public List<ZoneNode> GetLeafZones(Bulkhead bulkhead = Bulkhead.All, string? branch = "primary")
        => graph.Where(node => node.Value.Count == 0)
            .Where(node => (node.Key.Bulkhead & bulkhead) != 0)
            .Where(node => branch == null || node.Key.Branch == branch)
            .Select(node => node.Key)
            .ToList();

    /// <summary>
    /// Find all zones that could have another zone attached to them
    /// </summary>
    /// <returns></returns>
    public List<ZoneNode> GetOpenZones(Bulkhead bulkhead = Bulkhead.All, string? branch = "primary")
        => graph
            .Where(node => (node.Key.Bulkhead & bulkhead) != 0)
            .Where(node => node.Value.Count < node.Key.MaxConnections)
            .Where(node => branch == null || node.Key.Branch == branch)
            .Select(node => node.Key)
            .OrderBy(zone => zone.ZoneNumber)
            .ToList();

    /// <summary>
    /// Gets all the zones that have the max number of connections made
    /// </summary>
    /// <param name="bulkhead"></param>
    /// <param name="branch"></param>
    /// <returns></returns>
    public List<ZoneNode> GetClosedZones(Bulkhead bulkhead = Bulkhead.All, string? branch = "primary")
        => graph
            .Where(node => (node.Key.Bulkhead & bulkhead) != 0)
            .Where(node => node.Value.Count >= node.Key.MaxConnections)
            .Where(node => branch == null || node.Key.Branch == branch)
            .Select(node => node.Key)
            .OrderBy(zone => zone.ZoneNumber)
            .ToList();

    /// <summary>
    /// Gets the last zone in a branch. Very useful for building the branch chain upwards.
    /// Note that this always returns the last zone in the chain regardless of whether it's
    /// Open/Closed. Care should be used if building new branches from this.
    ///
    /// Consider using GetLastOpenZone() if looking for the furthest place in a branch to build
    /// a new branch from.
    /// </summary>
    /// <param name="bulkhead"></param>
    /// <param name="branchId"></param>
    /// <returns></returns>
    public ZoneNode? GetLastZone(Bulkhead bulkhead = Bulkhead.Main, string? branch = "primary")
    {
        var zones = GetZones(bulkhead, branch);

        return zones.Count == 0 ? null : zones.Last();
    }

    public ZoneNode? GetLastZoneExact(Bulkhead bulkhead = Bulkhead.Main, string? branch = "primary")
    {
        var zones = graph
            .Where(node =>
                node.Key.Bulkhead == bulkhead && (branch == null || node.Key.Branch == branch))
            .Select(node => node.Key)
            .OrderBy(zone => zone.ZoneNumber)
            .ToList();

        return zones.Count == 0 ? null : zones.Last();
    }

    /// <summary>
    /// Gets the last zone in a branch which is open. Generally this will be the same as
    /// GetLastZone() except this will return earlier zones if the final zone in that branch
    /// is not open.
    /// </summary>
    /// <param name="bulkhead"></param>
    /// <param name="branchId"></param>
    /// <returns></returns>
    public ZoneNode? GetLastOpenZone(Bulkhead bulkhead = Bulkhead.Main, string? branch = "primary")
    {
        var openZones = GetOpenZones(bulkhead, branch);

        if (openZones.Count == 0)
            return null;

        return openZones.Last();
    }

    /// <summary>
    /// Returns the list of zones starting from 0 that have "totalOpenSlots" number of open
    /// slots across all of them. Useful for ensuring we place key doors like the main,
    /// extreme, overload bulkhead doors far enough forward that we can still connect other
    /// doors to them.
    /// </summary>
    /// <param name="totalOpenSlots"></param>
    /// <param name="bulkhead"></param>
    /// <param name="branch"></param>
    /// <returns></returns>
    public IEnumerable<ZoneNode> GetZonesWithTotalOpen(
        int fromIndex,
        int totalOpenSlots,
        Bulkhead bulkhead = Bulkhead.Main,
        string? branch = "primary")
    {
        var openZones = GetOpenZones(bulkhead, branch);

        var index = fromIndex;
        while (CountOpenSlots(openZones.Take(index)) < totalOpenSlots)
        {
            index++;
        }

        return openZones.Take(index);
    }

    /// <summary>
    /// Get all zones that have a specified tag
    /// </summary>
    /// <param name="bulkhead"></param>
    /// <param name="tag"></param>
    /// <param name="branch"></param>
    /// <returns></returns>
    public List<ZoneNode> GetZonesByTag(Bulkhead bulkhead, string tag, string? branch = null)
        => graph
            .Where(node => (node.Key.Bulkhead & bulkhead) != 0)
            .Where(node => node.Key.Tags.Contains(tag))
            .Where(node => branch == null || node.Key.Branch == branch)
            .Select(node => node.Key)
            .OrderBy(zone => zone.ZoneNumber)
            .ToList();

    /// <summary>
    /// Returns all ZoneNodes which have connections to other bulkhead zones.
    /// </summary>
    /// <returns></returns>
    public List<ZoneNode> GetBulkheadEntranceZones()
        => graph.Where(node => node.Value.Any(to => !to.Bulkhead.HasFlag(node.Key.Bulkhead)))
            .Select(node => node.Key)
            .OrderBy(zone => zone.ZoneNumber)
            .ToList();

    /// <summary>
    /// Get's the first zone in a bulkhead
    /// </summary>
    /// <param name="bulkhead"></param>
    /// <returns></returns>
    public ZoneNode? GetBulkheadFirstZone(Bulkhead bulkhead)
    {
        var nodes = graph.Where(node => node.Key.Bulkhead.HasFlag(bulkhead))
            .Select(node => node.Key)
            .OrderBy(zone => zone.ZoneNumber);

        return nodes.Any() ? nodes.First() : null;
    }

    /// <summary>
    /// Returns true/false whether the given node is a bulkhead entrance.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool IsBulkheadEntrance(ZoneNode node)
        => graph[node].Any(to => !to.Bulkhead.HasFlag(node.Bulkhead));

    /// <summary>
    /// Counts the number of open slots across all the zones given. Useful for ensuring we
    /// place key doors like the main bulkhead far enough forward that we can still connect
    /// other zones to the previous area.
    /// </summary>
    /// <param name="nodes"></param>
    /// <returns></returns>
    public int CountOpenSlots(IEnumerable<ZoneNode> nodes)
        => nodes.Sum(node => Math.Max(0, node.MaxConnections - graph[node].Count()));

    /// <summary>
    /// This method traverses the tree of zones and attempts to set the properties:
    /// ZoneEntranceBuildFrom, ZoneBuildExpansion, and ZoneExpansion, such that when generating
    /// the level in the game there is minimal chance of a failed generation. Failed
    /// generation errors appear in logs like this:
    ///
    ///     [Error  :     Unity] WARNING : Zone1 (Zone_1 - 544): Failed to find any good StartAreas in zone 0 (543) expansionType:Towards_Random m_buildFromZone.m_areas: 1 scoredCount:0 dim: Reality
    ///
    /// When these occur the game will hang whilst it continually attempts to find a place
    /// to place the next zone, with no success.
    ///
    /// We can have the following zone Bulkhead types:
    ///     * Main
    ///     * Extreme
    ///     * Overload
    ///     * StartingArea (this is part of Main but before the Main bulkhead door)
    ///
    /// We want to ensure we lay each of these out well. We can also have side branches
    /// generated for various things like generators, keys, error alarm turn off codes.
    ///
    /// Variables we can adjust for generation. "Forwards" direction is from the elevator:
    ///
    ///     * ZoneBuildExpansion -- Direction to expand towards
    ///     * ZoneExpansion -- Direction to expand towards
    ///     * ZoneEntranceBuildFrom -- Where in previous zone to make entrance to this zone
    /// </summary>
    public void PlanPlacements(Bulkhead bulkhead)
    {}

    /// <summary>
    /// Specific placement planning on a per bulkhead level
    /// </summary>
    /// <param name="bulkhead"></param>
    public void PlanBulkheadPlacements(Bulkhead bulkhead, RelativeDirection direction)
    {
        // Adjust zone sizes to help reduce locked generation
        var fullZones = GetClosedZones(bulkhead);

        foreach (var node in fullZones)
        {
            var zone = GetZone(node);

            if (zone != null && zone.CustomGeomorph != null)
            {
                // Increase the size for this zone we think
                zone.Coverage.Min += 20.0;
                zone.Coverage.Max += 20.0;
            }
        }
    }
}
