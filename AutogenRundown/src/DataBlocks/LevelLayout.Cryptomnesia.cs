using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    /// <summary>
    /// Builds the layout for the Cryptomnesia objective.
    ///
    /// Places 1 data cube in Reality and 1 in each of (GatherRequiredCount - 1) dimensions.
    /// All dimensions share the same complex and resource set as Reality.
    /// A single elevator geomorph is randomly selected from the resource set and used as
    /// the origin tile for each dimension (and the only elevator available in Reality).
    ///
    /// When Cryptomnesia_MatchLayout is true, each dimension mirrors the Reality zone
    /// structure (same zone count, coverage, geomorphs, build directions, and subseeds)
    /// but with independently rolled enemies, alarms, and blood doors.
    /// </summary>
    public void BuildLayout_Cryptomnesia(BuildDirector director, WardenObjective objective, ZoneNode start)
    {
        var startZone = level.Planner.GetZone(start)!;

        // Clone the resource set once -- shared across Reality and all dimensions
        var resourceSet = (level.Complex switch
        {
            Complex.Mining => ComplexResourceSet.Mining,
            Complex.Tech => ComplexResourceSet.Tech,
            Complex.Service => ComplexResourceSet.Service,
            _ => ComplexResourceSet.Mining
        }).Duplicate();

        // Pick one elevator geomorph and strip all others so Reality and dimensions share it
        var elevator = Generator.Pick(resourceSet.ElevatorShafts_1x1);
        var elevatorGeo = elevator.Asset;

        resourceSet.ElevatorShafts_1x1.Clear();
        resourceSet.ElevatorShafts_1x1.Add(elevator);
        resourceSet.CustomGeomorphs.Add(elevator);

        // Set the level's resource set to the pruned clone
        level.ResourceSet = resourceSet;

        #region Dimension = Reality

        // --- Reality: build zones and place 1 data cube ---
        var realityNodes = AddBranch(start, Generator.Between(2, 4), "find_items", (node, zone) =>
        {
            zone.Coverage = CoverageMinMax.Medium;
        });

        // Place the data cube in the last reality zone
        objective.Gather_PlacementNodes.Add(realityNodes.Last());

        #endregion

        #region Dimension = 1+

        // --- Dimensions: build (count - 1) dimensions, each with 1 data cube ---
        var dimensionCount = objective.GatherRequiredCount - 1;

        for (var i = 0; i < dimensionCount; i++)
        {
            var dimensionIndex = (DimensionIndex)(i + 1); // Dimension1, Dimension2, Dimension3

            // Register the dimension on the level with shared resource set and elevator geo
            var dimension = new Dimension
            {
                Data = new Dimensions.DimensionData
                {
                    DimensionGeomorph = elevatorGeo,
                    ResourceSet = resourceSet
                }
            };
            dimension.FindOrPersist();

            level.DimensionDatas.Add(new Levels.DimensionData
            {
                Dimension = dimensionIndex,
                Data = dimension
            });

            // Initialize the dimension layout with a starting zone
            var (dimLayout, dimStart) = LevelLayout.BuildDimension(
                level, director, objective, dimensionIndex, level.Complex);

            // Link the layout to the dimension
            dimension.Data.Layout = dimLayout;

            if (objective.Cryptomnesia_MatchLayout)
            {
                // Mirror Reality zone structure into this dimension
                CopyRealityLayout(dimLayout, dimStart, realityNodes, start, dimensionIndex);
            }
            else
            {
                // Build independent zones
                dimLayout.AddBranch(dimStart, Generator.Between(1, 3), "find_items", (node, zone) =>
                {
                    zone.Coverage = CoverageMinMax.Medium;
                });
            }

            // Place a data cube in the last find_items zone
            var lastDimNode = level.Planner.GetLastZone(
                director.Bulkhead, "find_items", dimensionIndex);
            if (lastDimNode != null)
                objective.Gather_PlacementNodes.Add((ZoneNode)lastDimNode);

            // Finalize: write zones to layout, roll alarms/enemies, persist
            dimLayout.FinalizeLayout();

            Plugin.Logger.LogDebug(
                $"Cryptomnesia: Built dimension {dimensionIndex} with {dimLayout.Zones.Count} zones" +
                $"{(objective.Cryptomnesia_MatchLayout ? " (matched layout)" : "")}");
        }

        #endregion
    }

    /// <summary>
    /// Copies the spatial structure of Reality zones into a dimension layout.
    /// For each Reality zone in the branch, creates a matching dimension zone with the
    /// same coverage, geomorph, build directions, altitude, and subseeds. Enemies,
    /// alarms, blood doors, and lights are left for FinalizeLayout to roll independently.
    /// </summary>
    private void CopyRealityLayout(
        LevelLayout dimLayout,
        ZoneNode dimStart,
        List<ZoneNode> realityNodes,
        ZoneNode realityStart,
        DimensionIndex dimensionIndex)
    {
        // Copy the starting zone's spatial properties from Reality
        var realityStartZone = level.Planner.GetZone(realityStart)!;
        var dimStartZone = level.Planner.GetZone(dimStart)!;
        CopyZoneSpatialProperties(realityStartZone, dimStartZone);

        // Build matching zones for each Reality branch zone
        var prevDimNode = dimStart;

        foreach (var realityNode in realityNodes)
        {
            var realityZone = level.Planner.GetZone(realityNode)!;

            // Create a new zone in the dimension with fresh enemy/alarm state
            var (dimNode, dimZone) = dimLayout.AddZone(prevDimNode, new ZoneNode
            {
                Branch = realityNode.Branch,
                MaxConnections = realityNode.MaxConnections
            });

            // Copy the spatial properties from the Reality zone
            CopyZoneSpatialProperties(realityZone, dimZone);

            prevDimNode = dimNode;
        }
    }

    /// <summary>
    /// Copies spatial/structural properties from a source zone to a target zone.
    /// This makes the target generate the same physical layout as the source.
    /// Does NOT copy enemies, alarms, lights, fog, terminals, or progression puzzles.
    /// </summary>
    private static void CopyZoneSpatialProperties(Zone source, Zone target)
    {
        target.Coverage = source.Coverage;
        target.SubComplex = source.SubComplex;
        target.CustomGeomorph = source.CustomGeomorph;
        target.IgnoreRandomGeomorphRotation = source.IgnoreRandomGeomorphRotation;
        target.StartPosition = source.StartPosition;
        target.StartExpansion = source.StartExpansion;
        target.ZoneExpansion = source.ZoneExpansion;
        target.Altitude = source.Altitude;

        // Match subseeds so the game generates identical tile placement
        target.SubSeed = source.SubSeed;
        target.MarkerSubSeed = source.MarkerSubSeed;
    }
}
