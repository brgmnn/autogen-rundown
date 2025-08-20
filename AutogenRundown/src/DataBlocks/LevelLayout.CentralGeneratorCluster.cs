using AutogenRundown.DataBlocks.Items;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Objectives.CentralGeneratorCluster;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// When building the power cell distribution layout, here we are modelling a hub with offshoot zones.
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    public void BuildLayout_CentralGeneratorCluster(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;


        switch (level.Tier, director.Bulkhead)
        {
            #region Tier: A

            // Always has 3 cells
            // case ("A", Bulkhead.Main):
            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single center with three branches off. Each branch at a different altitude
                    // and we let the fog rise during scan
                    (0.12, () =>
                    {
                        startZone.GenGeneratorClusterGeomorph(director.Complex);
                        startZone.Altitude = Altitude.OnlyMid;

                        var (cell1, cellZone1) = AddZone(start);
                        var (cell2, cellZone2) = AddZone(start);
                        var (cell3, cellZone3) = AddZone(start);

                        cellZone1.Altitude = Altitude.OnlyLow;
                        cellZone2.Altitude = Altitude.OnlyMid;
                        cellZone3.Altitude = Altitude.OnlyHigh;

                        cellZone1.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_1.PersistentId;
                        cellZone2.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_1.PersistentId;
                        cellZone3.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_1.PersistentId;

                        level.FogSettings = Fog.Normal_Altitude_minus4;

                        objective.CentralGeneratorCluster_FogDataSteps = new List<GeneralFogStep>
                        {
                            new() { Fog = Fog.Normal_Altitude_0 },
                            new() { Fog = Fog.Normal_Altitude_4 }
                        };
                    }),
                });
                break;
            }

            case ("A", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // TODO: add
                    (0.12, () => { }),
                });
                break;
            }
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
            // case ("C", Bulkhead.Main):
            // {
            //     Generator.SelectRun(new List<(double, Action)>
            //     {
            //         // TODO: add
            //         (0.12, () => { }),
            //     });
            //     break;
            // }

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
