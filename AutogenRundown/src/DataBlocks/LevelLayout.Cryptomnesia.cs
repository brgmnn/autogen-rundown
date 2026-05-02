using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    // Planner tags used by the Cryptomnesia post-setup prune to identify must-keep zones.
    private const string CryptoTag_Cube = "crypto:cube";
    private const string CryptoTag_KeyHolder = "crypto:key_holder";
    private const string CryptoTag_Extraction = "crypto:extraction";

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
        {
            var taggedCube = level.Planner.AddTags((ZoneNode)realityCubeNode, CryptoTag_Cube);
            objective.Gather_PlacementNodes.Add(taggedCube);
        }

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
            {
                var taggedCube = level.Planner.AddTags((ZoneNode)cubeNode, CryptoTag_Cube);
                objective.Gather_PlacementNodes.Add(taggedCube);
            }

            // Apply dimension theme before finalization
            ApplyCryptomnesiaTheme(themes[i + 1], layoutType, dimLayout, level, director, objective);

            // Prune unreachable zones before the dimension is finalized so alarms,
            // blood doors, and enemies aren't rolled on zones players can't visit.
            if (layoutType == CryptomnesiaLayout.HubChain)
                PruneCryptomnesiaDimension(dimLayout, dimensionIndex);

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

        // Pre-place secondary bulkhead FROM zones BEFORE the Reality prune runs so:
        //  (a) the prune's BulkheadDoorControllerPlacements carve-out keeps them reachable
        //      from the elevator, and
        //  (b) we never land on a locked decoy (decoys don't exist in Reality yet).
        // Without this, the secondary director's StartingArea_GetBuildStart picks AFTER
        // the prune has tagged Locked decoys and can attach the secondary bulkhead --
        // and thus the secondary objective -- behind a locked door.
        PrePlaceCryptomnesiaSecondaryBulkheads();

        // Reality prune runs AFTER the dimension loop because CopyRealityLayout reads
        // Reality's planner graph to mirror each dimension -- pruning Reality earlier
        // would strip zones the dimensions still need to copy.
        if (layoutType == CryptomnesiaLayout.HubChain)
            PruneCryptomnesiaDimension(this, DimensionIndex.Reality);
    }

    /// <summary>
    /// For each non-Main bulkhead on the level, picks a Reality main zone and calls
    /// <see cref="InitializeBulkheadArea"/> to attach the secondary bulkhead. Must
    /// run before the Reality prune so the prune's BulkheadDoorControllerPlacements
    /// carve-out keeps the FROM zone reachable from the elevator.
    /// </summary>
    private void PrePlaceCryptomnesiaSecondaryBulkheads()
    {
        var secondaryBulkheads = new[] { Bulkhead.Extreme, Bulkhead.Overload }
            .Where(b => level.Settings.Bulkheads.HasFlag(b))
            .ToList();

        if (secondaryBulkheads.Count == 0)
            return;

        var pool = level.Planner.GetOpenZones(
            Bulkhead.Main, branch: null, dimension: DimensionIndex.Reality);

        foreach (var sb in secondaryBulkheads)
        {
            if (pool.Count == 0)
            {
                Plugin.Logger.LogWarning(
                    $"Cryptomnesia: no Reality zones available to host secondary bulkhead {sb}");
                break;
            }

            var picked = Generator.Pick(pool)!;
            InitializeBulkheadArea(level, sb, picked);

            // Don't stack two bulkhead doors on the same FROM zone if both Extreme
            // and Overload are present.
            pool.Remove(picked);
        }
    }

    #region Cryptomnesia Prune

    /// <summary>
    /// Removes zones from a Cryptomnesia dimension that players cannot reach. Keeps
    /// the cube zone, any key-holder zone, (Reality only) the extraction chain, and
    /// every ancestor of those up to the elevator. Immediate children of any kept
    /// zone are retained as locked decoys so players still see a sealed door one
    /// zone beyond their accessible area. Must be called AFTER SetupHubChain but
    /// BEFORE FinalizeLayout so pruned zones don't get alarms/enemies rolled.
    /// </summary>
    private void PruneCryptomnesiaDimension(LevelLayout dimLayout, DimensionIndex dim)
    {
        var bulkhead = dimLayout.director.Bulkhead;
        var planner = level.Planner;

        var allZones = planner.GetZones(bulkhead, branch: null, dimension: dim);
        if (allZones.Count == 0)
            return;

        // 1. Seed must-traverse with tagged anchor zones for this dimension.
        var mustTraverse = new HashSet<ZoneNode>();

        foreach (var cube in planner.GetZonesByTag(bulkhead, CryptoTag_Cube, branch: null, dimension: dim))
            mustTraverse.Add(cube);

        foreach (var holder in planner.GetZonesByTag(bulkhead, CryptoTag_KeyHolder, branch: null, dimension: dim))
            mustTraverse.Add(holder);

        if (dim == DimensionIndex.Reality)
            foreach (var ext in planner.GetZonesByTag(bulkhead, CryptoTag_Extraction, branch: null, dimension: dim))
                mustTraverse.Add(ext);

        // Bulkhead-DC FROM zones must remain reachable so secondary bulkhead doors
        // stay connected to a traversable origin. Without this, a layout where the
        // DC FROM zone (or its descendants) carries no CryptoTag_* anchor would
        // orphan the bulkhead door or get the FROM zone tagged no_access.
        foreach (var dc in level.GetObjectiveLayerData(bulkhead).BulkheadDoorControllerPlacements)
            foreach (var node in allZones.Where(n => n.ZoneNumber == dc.ZoneIndex))
                mustTraverse.Add(node);

        if (mustTraverse.Count == 0)
        {
            Plugin.Logger.LogWarning(
                $"Cryptomnesia prune: dim={dim} has no tagged anchor zones; skipping prune.");
            return;
        }

        // 2. Expand each anchor back to the elevator.
        var seeds = mustTraverse.ToList();
        foreach (var seed in seeds)
            foreach (var ancestor in planner.TraverseToElevator(seed))
                mustTraverse.Add(ancestor);

        // 3. Collect immediate children of must-traverse that aren't themselves must-traverse.
        var lockedDecoys = new HashSet<ZoneNode>();
        foreach (var node in mustTraverse)
            foreach (var child in planner.GetConnections(node, branch: null))
                if (!mustTraverse.Contains(child))
                    lockedDecoys.Add(child);

        // 4. Apply ProgressionPuzzle.Locked to each decoy (idempotent if already locked),
        //    swap its geomorph to a small dead-end tile so the locked door visually
        //    signals "nothing useful past here", and tag it so downstream placement
        //    (e.g. bulkhead keys) skips it.
        var (decoySubComplex, decoyGeo) = NoAccessDeadEndGeo(dimLayout.Complex);

        foreach (var decoy in lockedDecoys)
        {
            var zone = planner.GetZone(decoy);

            if (zone != null)
            {
                zone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;
                zone.SubComplex = decoySubComplex;
                zone.CustomGeomorph = decoyGeo;
                zone.Coverage = new CoverageMinMax { Min = 5, Max = 10 };

                // Remove all resources
                zone.AmmoPacks = 0;
                zone.DisinfectPacks = 0;
                zone.HealthPacks = 0;
                zone.ToolPacks = 0;
                zone.ConsumableDistributionInZone = 0;

                // Remove terminals // TODO: breaks an objective?
                // zone.TerminalPlacements.Clear();

                // Remove enemies
                zone.EnemySpawningInZone.Clear();
                zone.BloodDoor = BloodDoor.None;
            }

            planner.AddTags(decoy, "no_access");
        }

        // 5. Remove everything else.
        var keep = new HashSet<ZoneNode>(mustTraverse);
        keep.UnionWith(lockedDecoys);

        var pruned = 0;
        foreach (var node in allZones)
        {
            if (keep.Contains(node))
                continue;

            planner.Remove(node);
            pruned++;
        }

        Plugin.Logger.LogDebug(
            $"Cryptomnesia prune: dim={dim} " +
            $"kept={mustTraverse.Count} locked_decoys={lockedDecoys.Count} pruned={pruned} " +
            $"(total was {allZones.Count})");
    }

    /// <summary>
    /// Per-complex small dead-end geomorph used for no_access locked-decoy zones so the
    /// sealed door behind a kept zone reads as "nothing past here" instead of a full
    /// hub/T/corridor tile.
    /// </summary>
    private static (SubComplex, string) NoAccessDeadEndGeo(Complex complex) => complex switch
    {
        Complex.Mining => (
            SubComplex.Storage,
            "Assets/Prefabs/Geomorph/Mining/geo_storage_FA_dead_end_01.prefab"),

        Complex.Tech => (
            SubComplex.Lab,
            "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_dead_end_HA_03.prefab"),

        Complex.Service => (
            SubComplex.Floodways,
            "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_dead_end_HA_01.prefab"),

        _ => (
            SubComplex.Lab,
            "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_dead_end_HA_03.prefab"),
    };

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
