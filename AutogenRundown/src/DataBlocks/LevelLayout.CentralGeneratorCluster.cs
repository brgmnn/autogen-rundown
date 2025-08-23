using AutogenRundown.DataBlocks.Items;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Objectives.CentralGeneratorCluster;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public enum GeneratorFogShape
{
    Ascending,
    Descending
}

public partial record LevelLayout
{
    /// <summary>
    /// Also sets the level fog settings
    ///
    /// The level gets picked after that cell goes in
    /// </summary>
    /// <param name="objective"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    private List<GeneralFogStep> CentralGeneratorCluster_BuildFogSteps(
        WardenObjective objective,
        GeneratorFogShape shape,
        int numberOfSteps)
    {
        var defaultAltitude = level.FogSettings.DensityHeightAltitude;
        var infectious = level.FogSettings.IsInfectious;
        var generators = objective.CentralGeneratorCluster_NumberOfGenerators;

        return (shape, generators) switch
        {
            // Ascending fog
            (GeneratorFogShape.Ascending, 2) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Normal_Altitude_2 },
                new() { Fog = Fog.Normal_Altitude_8 } // completion scan
            },
            (GeneratorFogShape.Ascending, 3) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Normal_Altitude_0 },
                new() { Fog = Fog.Normal_Altitude_4 },
                new() { Fog = Fog.Normal_Altitude_8 } // completion scan
            },
            (GeneratorFogShape.Ascending, 4) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Normal_Altitude_minus2 },
                new() { Fog = Fog.Normal_Altitude_2 },
                new() { Fog = Fog.Normal_Altitude_6 },
                new() { Fog = Fog.Normal_Altitude_8 } // completion scan
            },
            (GeneratorFogShape.Ascending, 5) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Normal_Altitude_minus2 },
                new() { Fog = Fog.Normal_Altitude_2 },
                new() { Fog = Fog.Normal_Altitude_4 },
                new() { Fog = Fog.Normal_Altitude_6 },
                new() { Fog = Fog.Normal_Altitude_8 } // completion scan
            },

            // Descending fog
            (GeneratorFogShape.Descending, 2) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Inverted_Altitude_2 },
                new() { Fog = Fog.Inverted_Altitude_minus4 } // completion scan
            },
            (GeneratorFogShape.Descending, 3) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Inverted_Altitude_4 },
                new() { Fog = Fog.Inverted_Altitude_0 },
                new() { Fog = Fog.Inverted_Altitude_minus4 } // completion scan
            },
            (GeneratorFogShape.Descending, 4) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Inverted_Altitude_6 },
                new() { Fog = Fog.Inverted_Altitude_2 },
                new() { Fog = Fog.Inverted_Altitude_minus2 },
                new() { Fog = Fog.Inverted_Altitude_minus8 } // completion scan
            },
            (GeneratorFogShape.Descending, 5) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Inverted_Altitude_8 },
                new() { Fog = Fog.Inverted_Altitude_4 },
                new() { Fog = Fog.Inverted_Altitude_0 },
                new() { Fog = Fog.Inverted_Altitude_minus4 },
                new() { Fog = Fog.Inverted_Altitude_minus8 } // completion scan
            },

            _ => new List<GeneralFogStep>()
        };

        // return steps;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="objective"></param>
    /// <returns>The node where the cell is located.</returns>
    private ZoneNode CentralGeneratorCluster_AddCellBranch(ZoneNode start, Altitude? altitude = null)
    {
        var cellNode = new ZoneNode();
        var cellZone = new Zone(level);

        switch (level.Tier, director.Bulkhead)
        {
            #region Tier: A
            case ("A", _):
            {
                var zoneCount = director.Bulkhead == Bulkhead.Main && Generator.Flip(0.2)
                    ? 2
                    : 1;

                cellNode = AddBranch(start, zoneCount).Last();
                cellZone = planner.GetZone(cellNode);

                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell
                    (4.0, () => { (cellNode, cellZone) = AddZone(start); }),

                    // Single zone to cell, guarded by keycard
                    (2.0, () => { (cellNode, cellZone) = BuildChallenge_KeycardInZone(start); }),

                    // Single zone to cell, door locked down in source zone
                    (2.0, () => { (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(start, 0); }),

                    // --- Main only ---
                    // 2 zones
                    (director.Bulkhead == Bulkhead.Main ? 0.5 : 0.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = AddZone(mid);
                    }),

                    // 2 zones, with cell behind keycard
                    (director.Bulkhead == Bulkhead.Main ? 0.5 : 0.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // 2 zones, with locked second door on terminal
                    (director.Bulkhead == Bulkhead.Main ? 0.5 : 0.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 0);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }
            #endregion

            #region Tier: B
            case ("B", _):
            {
                var zoneCount = director.Bulkhead == Bulkhead.Main && Generator.Flip(0.2)
                    ? 2
                    : 1;

                cellNode = AddBranch(start, zoneCount).Last();
                cellZone = planner.GetZone(cellNode);

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }
            #endregion

            #region Tier: C
            case ("C", Bulkhead.Main):
            {
                cellNode = AddBranch(start, Generator.Flip(0.2) ? 2 : 1).Last();
                cellZone = planner.GetZone(cellNode);

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }

            case ("C", _):
            {
                var (node, zone) = AddZone(start);

                if (altitude != null)
                    zone.Altitude = altitude;

                cellNode = node;
                break;
            }
            #endregion

            #region Tier: D
            case ("D", Bulkhead.Main):
            {
                cellNode = AddBranch(start, Generator.Flip(0.2) ? 2 : 1).Last();
                cellZone = planner.GetZone(cellNode);

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }

            case ("D", _):
            {
                var (node, zone) = AddZone(start);

                if (altitude != null)
                    zone.Altitude = altitude;

                cellNode = node;
                break;
            }
            #endregion

            #region Tier: E
            case ("E", Bulkhead.Main):
            {
                cellNode = AddBranch(start, Generator.Flip(0.2) ? 2 : 1).Last();
                cellZone = planner.GetZone(cellNode);

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }

            case ("E", _):
            {
                var (node, zone) = AddZone(start);

                if (altitude != null)
                    zone.Altitude = altitude;

                cellNode = node;
                break;
            }
            #endregion
        }

        cellZone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_1.PersistentId;

        // Add the node to the objective placement list
        objective.PlacementNodes.Add(cellNode);

        return cellNode;
    }

    /// <summary>
    /// Central Generator Cluster objective
    ///
    /// Number of cells to distribute can be between 2 and 5
    ///
    /// I think we want to only allow a single Central Generator Cluster objective per level
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    private void BuildLayout_CentralGeneratorCluster(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;

        if (Generator.Flip(0.7))
            startZone.GenCorridorGeomorph(director.Complex);
        else
            startZone.Coverage = CoverageMinMax.Large_80;

        startZone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
        startZone.StartExpansion = ZoneBuildExpansion.Random;

        (start, startZone) = AddZone_Forward(
            start,
            new ZoneNode
            {
                Branch = "generator_cluster",
                MaxConnections = 3
            });

        // We need to check Fog.DensityHeightAltitude to see how high the fog is
        //  -4 = low
        //   0 = mid
        //   4 = high

        // Always ensure we have a fog turbine in the start zone and repellers
        startZone.BigPickupDistributionInZone = BigPickupDistribution.FogTurbine.PersistentId;
        startZone.ConsumableDistributionInZone = ConsumableDistribution.Baseline_FogRepellers.PersistentId;

        switch (level.Tier, director.Bulkhead)
        {
            #region Tier: A

            // Always has 3 cells
            // case ("A", Bulkhead.Main):
            case ("A", _):
            {
                // For now, we just use the single layout and reduce the number of branches
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single center with three branches off. Each branch at a different altitude,
                    // and we let the fog rise during scan
                    (1.0, () =>
                    {
                        startZone.GenGeneratorClusterGeomorph(director.Complex);
                        startZone.Altitude = Altitude.OnlyMid;

                        var altitudes = new List<Altitude>
                        {
                            Altitude.OnlyLow,
                            Altitude.OnlyMid,
                            Altitude.OnlyHigh
                        };

                        for (var c = 0; c < objective.CentralGeneratorCluster_NumberOfGenerators; c++)
                        {
                            CentralGeneratorCluster_AddCellBranch(start, Generator.Draw(altitudes)!);
                        }

                        var invertedFog = Generator.Flip();

                        level.FogSettings = invertedFog ? Fog.Inverted_Altitude_8 : Fog.Normal_Altitude_minus4;
                        objective.CentralGeneratorCluster_FogDataSteps = CentralGeneratorCluster_BuildFogSteps(
                            objective,
                            invertedFog ? GeneratorFogShape.Descending : GeneratorFogShape.Ascending,
                            objective.CentralGeneratorCluster_NumberOfGenerators);

                        objective.FogOnGotoWin = invertedFog ? Fog.Normal_Altitude_minus6 : Fog.Inverted_Altitude_6;
                        objective.FogTransitionDurationOnGotoWin = 6.0;
                    }),
                });
                break;
            }

            // TODO: when we have more layouts to make for generator cluster
            // case ("A", _):
            // {
            //     Generator.SelectRun(new List<(double, Action)>
            //     {
            //         (1.0, () =>
            //         {
            //         }),
            //     });
            //     break;
            // }
            #endregion

            #region Tier: B
            // case ("B", Bulkhead.Main):
            case ("B", _):
            {
                // For now, we just use the single layout and reduce the number of branches
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single center with three branches off. Each branch at a different altitude,
                    // and we let the fog rise during scan
                    (1.0, () =>
                    {
                        startZone.GenGeneratorClusterGeomorph(director.Complex);
                        startZone.Altitude = Altitude.OnlyMid;

                        var altitudes = new List<Altitude>
                        {
                            Altitude.OnlyLow,
                            Altitude.OnlyMid,
                            Altitude.OnlyHigh
                        };

                        var longerBranch = director.Bulkhead == Bulkhead.Main;

                        for (var c = 0; c < objective.CentralGeneratorCluster_NumberOfGenerators; c++)
                        {
                            CentralGeneratorCluster_AddCellBranch(start, Generator.Draw(altitudes)!);
                        }

                        var invertedFog = Generator.Flip();

                        level.FogSettings = invertedFog ? Fog.Inverted_Altitude_8 : Fog.Normal_Altitude_minus4;
                        objective.CentralGeneratorCluster_FogDataSteps = CentralGeneratorCluster_BuildFogSteps(
                            objective,
                            invertedFog ? GeneratorFogShape.Descending : GeneratorFogShape.Ascending,
                            objective.CentralGeneratorCluster_NumberOfGenerators);

                        objective.FogOnGotoWin = invertedFog ? Fog.Normal_Altitude_minus6 : Fog.Inverted_Altitude_6;
                        objective.FogTransitionDurationOnGotoWin = 6.0;
                    }),
                });
                break;
            }

            // case ("B", _):
            // {
            //     Generator.SelectRun(new List<(double, Action)>
            //     {
            //         // TODO: add
            //         (0.12, () => { }),
            //     });
            //     break;
            // }
            #endregion

            #region Tier: C
            // case ("C", Bulkhead.Main):
            case ("C", _):
            {
                // For now, we just use the single layout and reduce the number of branches
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single center with three branches off. Each branch at a different altitude,
                    // and we let the fog rise during scan
                    (1.0, () =>
                    {
                        startZone.GenGeneratorClusterGeomorph(director.Complex);
                        startZone.Altitude = Altitude.OnlyMid;

                        var altitudes = new List<Altitude>
                        {
                            Altitude.OnlyLow,
                            Altitude.OnlyMid,
                            Altitude.OnlyHigh
                        };

                        var cellEnds = new List<ZoneNode>();

                        // Slightly different approach here, build out 3 branches and any more
                        // branches that we need get built from one of the previous branch ends.
                        // We keep adding more end points after each piece so we can keep drawing
                        // more start points
                        for (var c = 0; c < objective.CentralGeneratorCluster_NumberOfGenerators; c++)
                        {
                            var branchStart = c < 3 ? start : Generator.Draw(cellEnds)!;
                            var node = CentralGeneratorCluster_AddCellBranch(branchStart, Generator.Pick(altitudes)!);

                            cellEnds.Add(node);
                        }

                        var invertedFog = Generator.Flip();

                        level.FogSettings = invertedFog ? Fog.Inverted_Altitude_8 : Fog.Normal_Altitude_minus4;
                        objective.CentralGeneratorCluster_FogDataSteps = CentralGeneratorCluster_BuildFogSteps(
                            objective,
                            invertedFog ? GeneratorFogShape.Descending : GeneratorFogShape.Ascending,
                            objective.CentralGeneratorCluster_NumberOfGenerators);

                        objective.FogOnGotoWin = invertedFog ? Fog.Normal_Altitude_minus6 : Fog.Inverted_Altitude_6;
                        objective.FogTransitionDurationOnGotoWin = 6.0;
                    }),
                });
                break;
            }

            // case ("C", _):
            // {
            //     Generator.SelectRun(new List<(double, Action)>
            //     {
            //         // TODO: add
            //         (0.12, () => { }),
            //     });
            //     break;
            // }
            #endregion

            #region Tier: D
            // case ("D", Bulkhead.Main):
            case ("D", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single center with three branches off. Each branch at a different altitude,
                    // and we let the fog rise during scan
                    (1.0, () =>
                    {
                        startZone.GenGeneratorClusterGeomorph(director.Complex);
                        startZone.Altitude = Altitude.OnlyMid;

                        var altitudes = new List<Altitude>
                        {
                            Altitude.OnlyLow,
                            Altitude.OnlyMid,
                            Altitude.OnlyHigh
                        };

                        var cellEnds = new List<ZoneNode>();

                        // Slightly different approach here, build out 3 branches and any more
                        // branches that we need get built from one of the previous branch ends.
                        // We keep adding more end points after each piece so we can keep drawing
                        // more start points
                        for (var c = 0; c < objective.CentralGeneratorCluster_NumberOfGenerators; c++)
                        {
                            var branchStart = c < 3 ? start : Generator.Draw(cellEnds)!;
                            var node = CentralGeneratorCluster_AddCellBranch(branchStart, Generator.Pick(altitudes)!);

                            cellEnds.Add(node);
                        }

                        var invertedFog = Generator.Flip();

                        level.FogSettings = invertedFog ? Fog.Inverted_Altitude_8 : Fog.Normal_Altitude_minus4;
                        objective.CentralGeneratorCluster_FogDataSteps = CentralGeneratorCluster_BuildFogSteps(
                            objective,
                            invertedFog ? GeneratorFogShape.Descending : GeneratorFogShape.Ascending,
                            objective.CentralGeneratorCluster_NumberOfGenerators);

                        objective.FogOnGotoWin = invertedFog ? Fog.Normal_Altitude_minus6 : Fog.Inverted_Altitude_6;
                        objective.FogTransitionDurationOnGotoWin = 6.0;
                    }),
                });
                break;
            }

            // case ("D", _):
            // {
            //     Generator.SelectRun(new List<(double, Action)>
            //     {
            //         // TODO: add
            //         (0.12, () => { }),
            //     });
            //     break;
            // }
            #endregion

            #region Tier: E
            // case ("E", Bulkhead.Main):
            case ("E", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single center with three branches off. Each branch at a different altitude,
                    // and we let the fog rise during scan
                    (1.0, () =>
                    {
                        startZone.GenGeneratorClusterGeomorph(director.Complex);
                        startZone.Altitude = Altitude.OnlyMid;

                        var altitudes = new List<Altitude>
                        {
                            Altitude.OnlyLow,
                            Altitude.OnlyMid,
                            Altitude.OnlyHigh
                        };

                        var cellEnds = new List<ZoneNode>();

                        // Slightly different approach here, build out 3 branches and any more
                        // branches that we need get built from one of the previous branch ends.
                        // We keep adding more end points after each piece so we can keep drawing
                        // more start points
                        for (var c = 0; c < objective.CentralGeneratorCluster_NumberOfGenerators; c++)
                        {
                            var branchStart = c < 3 ? start : Generator.Draw(cellEnds)!;
                            var node = CentralGeneratorCluster_AddCellBranch(branchStart, Generator.Pick(altitudes)!);

                            cellEnds.Add(node);
                        }

                        var invertedFog = Generator.Flip();

                        level.FogSettings = invertedFog ? Fog.Inverted_Altitude_8 : Fog.Normal_Altitude_minus4;
                        objective.CentralGeneratorCluster_FogDataSteps = CentralGeneratorCluster_BuildFogSteps(
                            objective,
                            invertedFog ? GeneratorFogShape.Descending : GeneratorFogShape.Ascending,
                            objective.CentralGeneratorCluster_NumberOfGenerators);

                        objective.FogOnGotoWin = invertedFog ? Fog.Normal_Altitude_minus6 : Fog.Inverted_Altitude_6;
                        objective.FogTransitionDurationOnGotoWin = 6.0;
                    }),
                });
                break;
            }

            // case ("E", _):
            // {
            //     Generator.SelectRun(new List<(double, Action)>
            //     {
            //         // TODO: add
            //         (0.12, () => { }),
            //     });
            //     break;
            // }
            #endregion
        }
    }
}
