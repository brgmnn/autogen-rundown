using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks.Zones
{
    public record struct ZoneNode(
        Bulkhead Bulkhead,
        int ZoneNumber,
        string Branch = "primary",
        int MaxConnections = 2)
    {
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

        private Dictionary<ZoneNode, List<ZoneNode>> graph = new Dictionary<ZoneNode, List<ZoneNode>>();

        private Dictionary<ZoneNode, Zone> blocks = new Dictionary<ZoneNode, Zone>();

        private IEnumerable<KeyValuePair<ZoneNode, List<ZoneNode>>> GetSubgraph(
                Bulkhead bulkhead = Bulkhead.All,
                string? branch = "primary")
            => graph.Where(node => (node.Key.Bulkhead & bulkhead) != 0 && (branch == null || node.Key.Branch == branch));

        public override string ToString()
        {
            var debug = string.Join("\n", graph.Select(n => $"  {n.Key} => [{ZoneNode.ListToString(n.Value, ",\n    ")}]"));
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

            blocks.Add(node, updatedZone);

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
        /// Get's a zone node by it's index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ZoneNode GetZoneNode(int index)
            => graph.Keys.Where(node => node.ZoneNumber == index).First();

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
        /// 
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
        /// Gets the last zone in a branch. Very useful for building the branch chain upwards.
        /// </summary>
        /// <param name="bulkhead"></param>
        /// <param name="branchId"></param>
        /// <returns></returns>
        public ZoneNode? GetLastZone(Bulkhead bulkhead = Bulkhead.Main, string? branch = "primary")
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
        /// Returns all ZoneNodes which have connections to other bulkhead zones.
        /// </summary>
        /// <returns></returns>
        public List<ZoneNode> GetBulkheadEntranceZones()
            => graph.Where(node => node.Value.Where(to => !to.Bulkhead.HasFlag(node.Key.Bulkhead)).Count() > 0)
                .Select(node => node.Key)
                .OrderBy(zone => zone.ZoneNumber)
                .ToList();

        /// <summary>
        /// Returns true/false whether the given node is a bulkhead entrance.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool IsBulkheadEntrance(ZoneNode node)
            => graph[node].Where(to => !to.Bulkhead.HasFlag(node.Bulkhead)).Count() > 0;

        /// <summary>
        /// Counts the number of open slots across all the zones given. Useful for ensuring we
        /// place key doors like the main bulkhead far enough forward that we can still connect
        /// other zones to the previous area.
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public int CountOpenSlots(IEnumerable<ZoneNode> nodes)
            => nodes.Sum(node => Math.Max(0, node.MaxConnections - graph[node].Count()));
    }
}
