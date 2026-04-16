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
    /// Each dimension mirrors the Reality zone structure (same zone count, coverage,
    /// geomorphs, build directions, and subseeds) but with independently rolled enemies,
    /// alarms, and blood doors.
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
        level.FogSettings = Fog.Randomized();

        // Select layout type -- shared between Reality and all dimensions
        var layoutType = Generator.Select(new List<(double, CryptomnesiaLayout)>
        {
            (1.0, CryptomnesiaLayout.HubChain),
            // (1.0, CryptomnesiaLayout.ForwardSplit),
        });

        Plugin.Logger.LogDebug($"Cryptomnesia: Selected layout type: {layoutType}");

        // --- Dimensions: build (count - 1) dimensions, each with 1 data cube ---
        var dimensionCount = objective.GatherRequiredCount - 1;

        // Data cube placement branches per dimension (by layout type)
        objective.Cryptomnesia_CubeBranches = layoutType switch
        {
            CryptomnesiaLayout.HubChain => Generator.Pick(
                new List<List<string>>
                {
                    new() { "hub_3", "side_2",  "side_3a", "side_1" },
                    new() { "hub_3", "side_3a", "side_1",  "side_2" },
                    new() { "hub_3", "side_1",  "side_2",  "side_3a" }
                })!,

            // CryptomnesiaLayout.ForwardSplit => new() { "find_items", "find_items", "find_items", "find_items" },

            _ => new() { "find_items", "find_items", "find_items", "find_items" }
        };

        #region Dimension = Reality

        switch (layoutType)
        {
            case CryptomnesiaLayout.HubChain:
                BuildCryptomnesia_HubChain(start);
                break;
            case CryptomnesiaLayout.ForwardSplit:
                BuildCryptomnesia_ForwardSplit(start);
                break;
        }

        // Place a data cube in the dimension-specific branch zone
        var realityCubeNode = level.Planner.GetLastZone(director.Bulkhead, objective.Cryptomnesia_CubeBranches.First());

        if (realityCubeNode != null)
            objective.Gather_PlacementNodes.Add((ZoneNode)realityCubeNode);

        #endregion

        #region Dimension = 1+

        // Select unique themes: 1 for Reality + 1 per dimension
        var themes = SelectCryptomnesiaThemes(dimensionCount + 1);

        Plugin.Logger.LogDebug(
            $"Cryptomnesia: Selected themes: Reality={themes[0]}" +
            string.Concat(themes.Skip(1).Select((t, i) => $", Dimension{i + 1}={t}")));

        // Apply Reality theme before dimension loop
        ApplyCryptomnesiaTheme(themes[0], layoutType, this, level, director, objective);

        for (var i = 0; i < dimensionCount; i++)
        {
            var dimensionIndex = (DimensionIndex)(i + 1); // Dimension1, Dimension2, Dimension3

            // Create the dimension with shared resource set and elevator geo
            var dimension = new Dimension
            {
                Data = new Dimensions.DimensionData
                {
                    DimensionGeomorph = elevatorGeo,
                    ResourceSet = resourceSet,
                    Fog = Fog.Randomized().Persist()
                }
            };

            // Initialize the dimension layout with a starting zone
            var (dimLayout, dimStart) = LevelLayout.BuildDimension(
                level, director, objective, dimensionIndex, level.Complex);

            // Link the layout and dimension to each other
            dimension.Data.Layout = dimLayout;
            dimLayout.LinkedDimension = dimension;

            // Mirror Reality zone structure into this dimension
            CopyRealityLayout(dimLayout, dimStart, start, dimensionIndex);

            // Place a data cube in the dimension-specific branch zone
            var cubeBranch = objective.Cryptomnesia_CubeBranches[Math.Min(i + 1, objective.Cryptomnesia_CubeBranches.Count - 1)];
            var cubeNode = level.Planner.GetLastZone(director.Bulkhead, cubeBranch, dimensionIndex);

            if (cubeNode != null)
                objective.Gather_PlacementNodes.Add((ZoneNode)cubeNode);

            // Apply dimension theme before finalization
            ApplyCryptomnesiaTheme(themes[i + 1], layoutType, dimLayout, level, director, objective);

            // Persist after all configuration so DimensionFogData, Layout, etc. are final
            dimension.FindOrPersist();

            level.DimensionDatas.Add(new Levels.DimensionData
            {
                Dimension = dimensionIndex,
                Data = dimension
            });

            // Finalize: write zones to layout, roll alarms/blood doors, persist
            dimLayout.FinalizeLayout();

            Plugin.Logger.LogDebug(
                $"Cryptomnesia: Built dimension {dimensionIndex} with {dimLayout.Zones.Count} zones" +
                $" theme={themes[i + 1]} layout={layoutType}");
        }

        #endregion
    }

    #region Cryptomnesia Layout Builders

    /// <summary>
    /// ForwardSplit: A central hub with two arms going left and right.
    /// The hub has MaxConnections=3 to support two outgoing branches.
    /// Each arm is 1-3 zones. Data cube placed in a randomly chosen arm's last zone.
    /// </summary>
    /// <returns>A leaf node from a randomly chosen arm (for data cube placement).</returns>
    private ZoneNode BuildCryptomnesia_ForwardSplit(ZoneNode start)
    {
        // Create the central hub
        var (hub, hubZone) = AddZone(start, new ZoneNode
        {
            Branch = "find_items",
            MaxConnections = 3
        });
        hubZone.GenHubGeomorph(Complex);

        // Left arm: 1-3 zones
        var leftArm = AddBranch_Left(hub, Generator.Between(1, 3), "find_items", (node, zone) =>
        {
            zone.Coverage = CoverageMinMax.Medium;
        });

        // Right arm: 1-3 zones
        var rightArm = AddBranch_Right(hub, Generator.Between(1, 3), "find_items", (node, zone) =>
        {
            zone.Coverage = CoverageMinMax.Medium;
        });

        // Place data cube at the end of a randomly chosen arm
        return Generator.Flip() ? leftArm.Last() : rightArm.Last();
    }

    #endregion

    #region Cryptomnesia Layout Copying

    /// <summary>
    /// Copies the spatial structure of Reality zones into a dimension layout.
    /// Recursively walks the planner graph to handle any zone topology (linear,
    /// branching, hub chains, etc.). Enemies, alarms, blood doors, and lights
    /// are left for FinalizeLayout to roll independently.
    /// </summary>
    private void CopyRealityLayout(
        LevelLayout dimLayout,
        ZoneNode dimStart,
        ZoneNode realityStart,
        DimensionIndex dimensionIndex)
    {
        // Copy the starting zone's spatial properties from Reality
        var realityStartZone = level.Planner.GetZone(realityStart)!;
        var dimStartZone = level.Planner.GetZone(dimStart)!;
        CopyZoneSpatialProperties(realityStartZone, dimStartZone);

        // Recursively copy all children
        CopyRealityChildren(dimLayout, dimStart, realityStart, dimensionIndex);
    }

    /// <summary>
    /// Recursively walks the planner graph from a reality node and creates matching
    /// zones in the dimension layout. Handles branching zone graphs.
    /// </summary>
    private void CopyRealityChildren(
        LevelLayout dimLayout,
        ZoneNode dimParent,
        ZoneNode realityParent,
        DimensionIndex dimensionIndex)
    {
        var realityChildren = level.Planner.GetConnections(realityParent, branch: null);

        // If the reality parent has multiple children, ensure the dim parent
        // has enough MaxConnections to accommodate them
        if (realityChildren.Count > 1 && dimParent.MaxConnections < realityChildren.Count + 1)
        {
            dimParent = dimParent with { MaxConnections = realityChildren.Count + 1 };
            level.Planner.UpdateNode(dimParent);
        }

        foreach (var realityChild in realityChildren)
        {
            var realityZone = level.Planner.GetZone(realityChild)!;

            var (dimChild, dimZone) = dimLayout.AddZone(dimParent, new ZoneNode
            {
                Branch = realityChild.Branch,
                MaxConnections = realityChild.MaxConnections
            });

            CopyZoneSpatialProperties(realityZone, dimZone);

            switch (realityChild.Branch)
            {
                case "forward_extract":
                {
                    dimZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

                    break;
                }

                case "extraction_elevator":
                {
                    dimZone.CustomGeomorph = null;
                    dimZone.Coverage = CoverageMinMax.Tiny_3;

                    break;
                }
            }

            // Recurse into children of this node
            CopyRealityChildren(dimLayout, dimChild, realityChild, dimensionIndex);
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

    #endregion
}
