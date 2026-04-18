using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// ForwardSplit: A central hub with two arms going left and right.
    /// The hub has MaxConnections=3 to support two outgoing branches.
    /// Each arm is 1-3 zones. Data cube placed in a randomly chosen arm's last zone.
    ///
    /// "right_hub", "side_1", "side_4", "side_2"
    ///
    /// Layout map:
    ///
    ///   start (Hub)
    ///   │
    ///   ├── left_corridor
    ///   │   └── left_hub
    ///   │       ├── side_1                    -> Dimension1 Cube
    ///   │       ├── side_2                    -> Dimension3 Cube
    ///   │       └── side_3
    ///   │
    ///   └── right_corridor
    ///       └── right_hub                     -> Reality Cube
    ///           ├── side_4                    -> Dimension2 Cube
    ///           ├── side_5
    ///           └── forward_extract
    ///               └── extraction_elevator
    ///
    /// </summary>
    /// <returns>A leaf node from a randomly chosen arm (for data cube placement).</returns>
    private void BuildCryptomnesia_ForwardSplit(ZoneNode start)
    {
        var hub1 = planner.UpdateNode(start with { MaxConnections = 3 });
        var hub1Zone = planner.GetZone(hub1)!;

        hub1Zone.GenHubGeomorph(Complex);

        #region Left side

        var (leftCorridor, leftCorridorZone) = AddZone_Left(hub1, new ZoneNode
        {
            Branch = "left_corridor",
            MaxConnections = 1
        });
        leftCorridorZone.GenCorridorGeomorph(Complex);

        var (leftHub, leftHubZone) = AddZone_Left(leftCorridor, new ZoneNode
        {
            Branch = "left_hub",
            MaxConnections = 3
        });
        leftHubZone.GenHubGeomorph(Complex);

        AddZone_Backward(leftHub, new ZoneNode { Branch = "side_1" });
        AddZone_Left(leftHub, new ZoneNode { Branch = "side_2" });
        AddZone_Forward(leftHub, new ZoneNode { Branch = "side_3" });

        #endregion

        #region Right side

        var (rightCorridor, rightCorridorZone) = AddZone_Right(hub1, new ZoneNode
        {
            Branch = "right_corridor",
            MaxConnections = 1
        });
        leftCorridorZone.GenCorridorGeomorph(Complex);

        var (rightHub, rightHubZone) = AddZone_Right(rightCorridor, new ZoneNode
        {
            Branch = "right_hub",
            MaxConnections = 3
        });
        leftHubZone.GenHubGeomorph(Complex);

        AddZone_Backward(rightHub, new ZoneNode { Branch = "side_4" });
        AddZone_Right(rightHub, new ZoneNode { Branch = "side_5" });

        #endregion

        #region Forward extract

        var (forwardExtract, forwardExtractZone) = AddZone_Forward(
            rightHub,
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
    }

}
