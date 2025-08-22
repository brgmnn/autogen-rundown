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

        // We need to check Fog.DensityHeightAltitude to see how high the fog is
        //  -4 = low
        //   0 = mid
        //   4 = high

        // if (level.FogSettings.DensityHeightAltitude)

        switch (level.Tier, director.Bulkhead)
        {
            #region Tier: A

            // Always has 3 cells
            // case ("A", Bulkhead.Main):
            case ("A", _):
            {
                // For now, we just use the single layout on tier A all objectives and reduce the number of generators
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
                            var zone = planner.GetZone(AddBranch(start, longerBranch ? 2 : 1).Last());

                            longerBranch = false;

                            zone.Altitude = Generator.Draw(altitudes)!;
                            zone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_1.PersistentId;
                        }

                        // level.FogSettings = Fog.Normal_Altitude_minus4;
                        level.FogSettings = Fog.Inverted_Altitude_8;
                        objective.CentralGeneratorCluster_FogDataSteps = CentralGeneratorCluster_BuildFogSteps(
                            objective,
                            GeneratorFogShape.Descending,
                            objective.CentralGeneratorCluster_NumberOfGenerators);

                        // objective.FogOnGotoWin = Generator.Flip() ? Fog.Inverted_Altitude_6 : Fog.Normal_Altitude_minus6;
                        objective.FogOnGotoWin = Fog.Normal_Altitude_minus6;
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
            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // TODO: add
                    (0.12, () => { }),
                });
                break;
            }

            case ("B", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // TODO: add
                    (0.12, () => { }),
                });
                break;
            }
            #endregion

            #region Tier: C
            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // TODO: add
                    (0.12, () => { }),
                });
                break;
            }

            case ("C", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // TODO: add
                    (0.12, () => { }),
                });
                break;
            }
            #endregion

            #region Tier: D
            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // TODO: add
                    (0.12, () => { }),
                });
                break;
            }

            case ("D", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // TODO: add
                    (0.12, () => { }),
                });
                break;
            }
            #endregion

            #region Tier: E
            case ("E", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // TODO: add
                    (0.12, () => { }),
                });
                break;
            }

            case ("E", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // TODO: add
                    (0.12, () => { }),
                });
                break;
            }
            #endregion
        }
    }
}
