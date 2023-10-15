using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks.Zones
{
    public record struct ZoneNode(
        Bulkhead Bulkhead,
        int ZoneNumber,
        int MaxConnections = 2);

    public class LayoutPlanner
    {
        private Dictionary<ZoneNode, List<ZoneNode>> graph = new Dictionary<ZoneNode, List<ZoneNode>>();

        public void Connect(ZoneNode from, ZoneNode? to = null)
        {
            if (graph.ContainsKey(from))
                if (to != null)
                    graph[from].Add((ZoneNode)to);
            else
                graph.Add(from, to != null ?
                    new List<ZoneNode> { (ZoneNode)to } :
                    new List<ZoneNode>());

            if (to != null && !graph.ContainsKey((ZoneNode)to))
                graph.Add((ZoneNode)to, new List<ZoneNode>());
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

        public List<ZoneNode> GetBulkheadEntranceZones()
            => graph.Where(node => node.Value.Where(to => !to.Bulkhead.HasFlag(node.Key.Bulkhead)).Count() > 0)
                .Select(node => node.Key)
                .OrderBy(zone => zone.ZoneNumber)
                .ToList();

        public int CountOpenSlots(IEnumerable<ZoneNode> nodes)
            => nodes.Sum(node => Math.Max(0, node.MaxConnections - graph[node].Count));
    }
}
