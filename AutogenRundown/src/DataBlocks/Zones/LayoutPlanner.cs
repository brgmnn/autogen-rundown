using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks.Zones
{
    public record struct ZoneNode(
        Bulkhead Bulkhead,
        int ZoneNumber,
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

        public static string ListToString(IEnumerable<ZoneNode> nodes)
            => string.Join(", ", nodes.Select(node => node.ToString()));
    }

    public class LayoutPlanner
    {
        private Dictionary<ZoneNode, List<ZoneNode>> graph = new Dictionary<ZoneNode, List<ZoneNode>>();

        private IEnumerable<KeyValuePair<ZoneNode, List<ZoneNode>>> GetSubgraph(Bulkhead bulkhead = Bulkhead.All)
            => graph.Where(node => (node.Key.Bulkhead & bulkhead) != 0);

        public override string ToString()
        {
            var debug = string.Join("\n", graph.Select(n => $"  {n.Key} => [{ZoneNode.ListToString(n.Value)}]"));
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
        /// Gets all zones associated with the given bulkhead
        /// </summary>
        /// <param name="bulkhead"></param>
        /// <returns></returns>
        public List<ZoneNode> GetZones(Bulkhead bulkhead = Bulkhead.All)
            => GetSubgraph(bulkhead).Select(node => node.Key).ToList();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<ZoneNode> GetConnections(ZoneNode node)
        {
            List<ZoneNode> connections;

            if (graph.TryGetValue(node, out connections!))
                return connections;

            return new List<ZoneNode>();
        }

        /// <summary>
        /// Find all zones that are leaf nodes, or dead ends. I.e. they only have one connection
        /// from their previous zone.
        /// </summary>
        /// <returns></returns>
        public List<ZoneNode> GetLeafZones()
            => graph.Where(node => node.Value.Count == 0)
                .Select(node => node.Key)
                .ToList();

        /// <summary>
        /// Find all zones that could have another zone attached to them
        /// </summary>
        /// <returns></returns>
        public List<ZoneNode> GetOpenZones(Bulkhead bulkhead = Bulkhead.All)
            => graph
                .Where(node => (node.Key.Bulkhead & bulkhead) != 0)
                .Where(node => node.Value.Count < node.Key.MaxConnections)
                .Select(node => node.Key)
                .OrderBy(zone => zone.ZoneNumber)
                .ToList();

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
