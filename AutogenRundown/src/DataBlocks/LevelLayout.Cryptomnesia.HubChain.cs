using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// HubChain: A chain of 2-4 hub zones connected in sequence.
    /// Each zone uses GenHubGeomorph for large, open rooms.
    ///
    /// "hub_3", "side_2", "side_3a", "side_1"
    ///
    /// Layout map:
    ///
    ///   start
    ///   ├── side_1                            -> Dimension3 Cube
    ///   └── hub_2
    ///       ├── side_2                        -> Dimension1 Cube
    ///       ├── side_3a                       -> Dimension2 Cube
    ///       │   └── side_3b
    ///       └── hub_3                         -> Reality Cube
    ///           ├── side_4
    ///           └── forward_extract
    ///               └── extraction_elevator
    ///
    /// </summary>
    /// <returns>The last zone node (for data cube placement).</returns>
    private ZoneNode BuildCryptomnesia_HubChain(ZoneNode start)
    {
        #region Phase 1

        var hub1 = planner.UpdateNode(start with { MaxConnections = 3 });
        var hub1Zone = planner.GetZone(hub1)!;

        hub1Zone.GenHubGeomorph(Complex);

        var (side1, side1Zone) = AddZone_Side(hub1, new ZoneNode { Branch = "side_1" });

        side1Zone.Coverage = CoverageMinMax.Large_80;
        side1Zone.ZoneExpansion = ZoneExpansion.Expansional;

        #endregion

        #region Phase 2

        var (hub2, hub2Zone) = AddZone_Forward(hub1, new ZoneNode { Branch = "hub_2", MaxConnections = 3 });
        hub2Zone.GenHubGeomorph(Complex);

        var (side2, side2Zone) = AddZone_Left(hub2, new ZoneNode { Branch = "side_2", MaxConnections = 3 });
        side2Zone.GenHubGeomorph(Complex);

        var (side3a, side3aZone) = AddZone_Right(hub2, new ZoneNode { Branch = "side_3a", MaxConnections = 1 });
        var (side3b, side3bZone) = AddZone_Right(side3a, new ZoneNode { Branch = "side_3b" });

        #endregion

        #region Phase 3

        var (hub3, hub3Zone) = AddZone_Forward(hub2, new ZoneNode { Branch = "hub_3", MaxConnections = 3 });
        hub3Zone.GenHubGeomorph(Complex);

        var (side4, side4Zone) = AddZone_Forward(hub3, new ZoneNode { Branch = "side_4", MaxConnections = 3 });

        #endregion

        #region Forward extract

        var (forwardExtract, forwardExtractZone) = AddZone_Left(
            hub3,
            new ZoneNode { Branch = "forward_extract", MaxConnections = 1 });

        forwardExtractZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

        if (Generator.Flip())
            forwardExtractZone.GenCorridorGeomorph(Complex);
        else
        {
            forwardExtractZone.Coverage = CoverageMinMax.Medium_56;
            forwardExtractZone.ZoneExpansion = ZoneExpansion.Expansional;
        }

        var (extraction, extractionZone) = AddZone_Left(
            forwardExtract,
            new ZoneNode { Branch = "extraction_elevator", MaxConnections = 0 });

        extractionZone.GenExitGeomorph(Complex);

        level.ExtractionZone = extraction;

        #endregion

        return hub3;
    }
}
