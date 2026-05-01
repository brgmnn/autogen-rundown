using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

/// <summary>
/// Higher-level wrappers around the <see cref="Zone"/> Gen*Geomorph helpers.
///
/// These take a <see cref="ZoneNode"/>, look the zone up via the planner, apply the geomorph,
/// then update the node's MaxConnections to match the topology of the geomorph being placed
/// and persist the change via <see cref="LayoutPlanner.UpdateNode"/>.
///
/// Each wrapper returns the updated <see cref="ZoneNode"/> so callers can rebind any local
/// variable they were holding for the node.
///
/// Always prefer these wrappers over calling the Zone-side helpers directly — using them
/// means the topology constraint cannot drift out of sync with the geomorph shape.
/// </summary>
public partial class Level
{
    /// <summary>
    /// Exit tile — final zone, no outgoing children.
    /// </summary>
    public ZoneNode GenExitGeomorph(ZoneNode node, int maxConnections = 0)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenExitGeomorph(Complex);

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }

    /// <summary>
    /// Hub tile — multi-way intersection. Default 3 outgoing connections.
    /// </summary>
    public ZoneNode GenHubGeomorph(ZoneNode node, int maxConnections = 3)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenHubGeomorph(Complex);

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }

    /// <summary>
    /// T-tile — three-way junction. Default 2 outgoing connections.
    /// </summary>
    public ZoneNode GenTGeomorph(ZoneNode node, int maxConnections = 2)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenTGeomorph(Complex);

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }

    /// <summary>
    /// I-tile / corridor — straight pass-through. Default 1 outgoing connection.
    /// </summary>
    public ZoneNode GenCorridorGeomorph(ZoneNode node, int maxConnections = 1)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenCorridorGeomorph(Complex);

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }

    /// <summary>
    /// Matter wave projector tile — final zone for gather/big-item objectives. Tech variant
    /// is a dead-end room. Default 0 outgoing connections.
    /// </summary>
    public ZoneNode GenMatterWaveProjectorGeomorph(ZoneNode node, int maxConnections = 0)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenMatterWaveProjectorGeomorph(Complex);

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }

    /// <summary>
    /// Reactor hall — hub-equivalent tile with multiple expanders. Default 3 outgoing
    /// connections.
    /// </summary>
    public ZoneNode GenReactorGeomorph(ZoneNode node, int maxConnections = 3)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenReactorGeomorph(Complex);

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }

    /// <summary>
    /// Reactor approach corridor. Default 1 outgoing connection.
    /// </summary>
    public ZoneNode GenReactorCorridorGeomorph(ZoneNode node, int maxConnections = 1)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenReactorCorridorGeomorph(Complex);

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }

    /// <summary>
    /// Generator cluster — X-tile with four expanders. Default 3 outgoing connections
    /// (one expander reserved for the entrance).
    /// </summary>
    public ZoneNode GenGeneratorClusterGeomorph(ZoneNode node, int maxConnections = 3)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenGeneratorClusterGeomorph(Complex);

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }

    /// <summary>
    /// Dead-end side room — single-expander tile, no outgoing children.
    /// </summary>
    public ZoneNode GenDeadEndGeomorph(ZoneNode node, int maxConnections = 0)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenDeadEndGeomorph(Complex);

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }

    /// <summary>
    /// Boss arena — hub-style tile. Default 3 outgoing connections.
    /// </summary>
    public ZoneNode GenBossGeomorph(ZoneNode node, int maxConnections = 3)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenBossGeomorph(Complex);

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }

    /// <summary>
    /// Garden tile. Default 1 outgoing connection.
    /// </summary>
    public ZoneNode GenGardenGeomorph(ZoneNode node, int maxConnections = 1)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenGardenGeomorph(Complex);

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }

    /// <summary>
    /// Portal tile. Tech variant is a dead end so the default is 0 outgoing connections —
    /// callers using Mining (which has a forward expander) should pass 1.
    /// </summary>
    public ZoneNode GenPortalGeomorph(ZoneNode node, int maxConnections = 0)
    {
        var zone = Planner.GetZone(node)!;
        zone.GenPortalGeomorph();

        node = node with { MaxConnections = maxConnections };
        Planner.UpdateNode(node);

        return node;
    }
}
