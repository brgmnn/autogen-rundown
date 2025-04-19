using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.Extensions;

public static class ZoneNodeCollections
{
    /// <summary>
    /// Given a list of nodes, returns the nodes with their associated zones.
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="planner"></param>
    /// <returns></returns>
    public static ICollection<(ZoneNode, Zone?)> WithZones(this ICollection<ZoneNode> nodes, LayoutPlanner planner)
        => nodes.Select(node => (node, planner.GetZone(node))).ToList();

    /// <summary>
    ///
    /// </summary>
    /// <param name="nodes"></param>
    /// <returns></returns>
    public static string ToString(this List<ZoneNode> nodes)
        => string.Join(Environment.NewLine, nodes.Select(n => n.ToString()));
}
