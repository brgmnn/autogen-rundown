using System.Runtime.CompilerServices;
using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Items;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Objectives.CentralGeneratorCluster;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

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

        return (shape, infectious, generators) switch
        {
            // Ascending fog (non-infectious)
            (GeneratorFogShape.Ascending, false, 2) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Normal_Altitude_2 },
                new() { Fog = Fog.Normal_Altitude_8 } // completion scan
            },
            (GeneratorFogShape.Ascending, false, 3) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Normal_Altitude_0 },
                new() { Fog = Fog.Normal_Altitude_4 },
                new() { Fog = Fog.Normal_Altitude_8 } // completion scan
            },
            (GeneratorFogShape.Ascending, false, 4) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Normal_Altitude_minus2 },
                new() { Fog = Fog.Normal_Altitude_2 },
                new() { Fog = Fog.Normal_Altitude_6 },
                new() { Fog = Fog.Normal_Altitude_8 } // completion scan
            },
            (GeneratorFogShape.Ascending, false, 5) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Normal_Altitude_minus2 },
                new() { Fog = Fog.Normal_Altitude_2 },
                new() { Fog = Fog.Normal_Altitude_4 },
                new() { Fog = Fog.Normal_Altitude_6 },
                new() { Fog = Fog.Normal_Altitude_8 } // completion scan
            },

            // Descending fog (non-infectious)
            (GeneratorFogShape.Descending, false, 2) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Inverted_Altitude_2 },
                new() { Fog = Fog.Inverted_Altitude_minus4 } // completion scan
            },
            (GeneratorFogShape.Descending, false, 3) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Inverted_Altitude_4 },
                new() { Fog = Fog.Inverted_Altitude_0 },
                new() { Fog = Fog.Inverted_Altitude_minus4 } // completion scan
            },
            (GeneratorFogShape.Descending, false, 4) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Inverted_Altitude_6 },
                new() { Fog = Fog.Inverted_Altitude_2 },
                new() { Fog = Fog.Inverted_Altitude_minus2 },
                new() { Fog = Fog.Inverted_Altitude_minus8 } // completion scan
            },
            (GeneratorFogShape.Descending, false, 5) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.Inverted_Altitude_8 },
                new() { Fog = Fog.Inverted_Altitude_4 },
                new() { Fog = Fog.Inverted_Altitude_0 },
                new() { Fog = Fog.Inverted_Altitude_minus4 },
                new() { Fog = Fog.Inverted_Altitude_minus8 } // completion scan
            },

            // Ascending fog (infectious)
            (GeneratorFogShape.Ascending, true, 2) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.NormalInfectious_Altitude_2 },
                new() { Fog = Fog.NormalInfectious_Altitude_8 } // completion scan
            },
            (GeneratorFogShape.Ascending, true, 3) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.NormalInfectious_Altitude_0 },
                new() { Fog = Fog.NormalInfectious_Altitude_4 },
                new() { Fog = Fog.NormalInfectious_Altitude_8 } // completion scan
            },
            (GeneratorFogShape.Ascending, true, 4) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.NormalInfectious_Altitude_minus2 },
                new() { Fog = Fog.NormalInfectious_Altitude_2 },
                new() { Fog = Fog.NormalInfectious_Altitude_6 },
                new() { Fog = Fog.NormalInfectious_Altitude_8 } // completion scan
            },
            (GeneratorFogShape.Ascending, true, 5) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.NormalInfectious_Altitude_minus2 },
                new() { Fog = Fog.NormalInfectious_Altitude_2 },
                new() { Fog = Fog.NormalInfectious_Altitude_4 },
                new() { Fog = Fog.NormalInfectious_Altitude_6 },
                new() { Fog = Fog.NormalInfectious_Altitude_8 } // completion scan
            },

            // Descending fog (infectious)
            (GeneratorFogShape.Descending, true, 2) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.InvertedInfectious_Altitude_2 },
                new() { Fog = Fog.InvertedInfectious_Altitude_minus4 } // completion scan
            },
            (GeneratorFogShape.Descending, true, 3) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.InvertedInfectious_Altitude_4 },
                new() { Fog = Fog.InvertedInfectious_Altitude_0 },
                new() { Fog = Fog.InvertedInfectious_Altitude_minus4 } // completion scan
            },
            (GeneratorFogShape.Descending, true, 4) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.InvertedInfectious_Altitude_6 },
                new() { Fog = Fog.InvertedInfectious_Altitude_2 },
                new() { Fog = Fog.InvertedInfectious_Altitude_minus2 },
                new() { Fog = Fog.InvertedInfectious_Altitude_minus8 } // completion scan
            },
            (GeneratorFogShape.Descending, true, 5) => new List<GeneralFogStep>
            {
                new() { Fog = Fog.InvertedInfectious_Altitude_8 },
                new() { Fog = Fog.InvertedInfectious_Altitude_4 },
                new() { Fog = Fog.InvertedInfectious_Altitude_0 },
                new() { Fog = Fog.InvertedInfectious_Altitude_minus4 },
                new() { Fog = Fog.InvertedInfectious_Altitude_minus8 } // completion scan
            },

            _ => new List<GeneralFogStep>()
        };
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="objective"></param>
    /// <returns>The node where the cell is located.</returns>
    private ZoneNode CentralGeneratorCluster_AddCellBranch(ZoneNode start, Altitude? altitude = null)
    {
        var cellNode = new ZoneNode();
        var cellZone = new Zone(level, this);

        var zoneCount = planner.GetZones(Bulkhead.All, null).Count;

        // If we are in this case, we don't want to add any more zones so just stick the cell
        // in the start area
        //
        // For main only levels we allow up to 4 branches, for levels with optionals we cap at 3
        if (level.Settings.Bulkheads == Bulkhead.Main && objective.PlacementNodes.Count >= 4 ||
            level.Settings.Bulkheads != Bulkhead.Main && objective.PlacementNodes.Count >= 3)
        {
            cellZone.BigPickupDistributionInZone = BigPickupDistribution.PowerCell_1.PersistentId;

            cellZone.BigPickupDistributionInZone = cellZone.BigPickupDistributionInZone switch
            {
                BigPickupDistribution.PowerCellId_1 => BigPickupDistribution.PowerCell_2.PersistentId,
                BigPickupDistribution.PowerCellId_2 => BigPickupDistribution.PowerCell_3.PersistentId,
                BigPickupDistribution.PowerCellId_3 => BigPickupDistribution.PowerCell_4.PersistentId,
                BigPickupDistribution.PowerCellId_4 => BigPickupDistribution.PowerCell_5.PersistentId,

                _ => BigPickupDistribution.PowerCell_1.PersistentId,
            };

            // Add the node to the objective placement list, if it doesn't already exist
            if (!objective.PlacementNodes.Exists(node =>
                    node.Bulkhead == cellNode.Bulkhead && node.ZoneNumber == cellNode.ZoneNumber))
                objective.PlacementNodes.Add(cellNode);
        }

        switch (level.Tier, director.Bulkhead)
        {
            #region Tier: A
            case ("A", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell
                    (2.0, () => { (cellNode, cellZone) = AddZone(start); }),

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
            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell
                    (1.0, () => { (cellNode, cellZone) = AddZone(start); }),

                    // Single zone to cell, guarded by keycard
                    (1.0, () => { (cellNode, cellZone) = BuildChallenge_KeycardInZone(start); }),

                    // Single zone to cell, door locked down in source zone
                    (1.0, () => { (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(start, 0); }),

                    // --- Main only ---
                    // 2 zones
                    (0.5, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = AddZone(mid);
                    }),

                    // 2 zones, with boss
                    (0.5, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_BossFight(mid);
                    }),

                    // 2 zones, with cell behind keycard
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // 2 zones, with locked second door on terminal
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 0);
                    }),

                    // 2 zones with sensors
                    (0.3, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = AddZone(mid);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }

            case ("B", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell
                    (2.0, () => { (cellNode, cellZone) = AddZone(start); }),

                    // Single zone to cell, guarded by keycard
                    (2.0, () => { (cellNode, cellZone) = BuildChallenge_KeycardInZone(start); }),

                    // Single zone to cell, door locked down in source zone
                    (2.0, () => { (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(start, 0); }),

                    // 2 zones, with cell behind keycard
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // 2 zones, with locked second door on terminal
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 0);
                    }),

                    // Keycard with sensors
                    (0.3, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }

            case ("B", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell, door locked down in source zone
                    (2.0, () => { (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(start, 0); }),

                    // Single zone to cell, guarded by keycard
                    (2.0, () => { (cellNode, cellZone) = BuildChallenge_KeycardInZone(start); }),

                    // Single zone to cell, apex alarm
                    (1.0, () =>
                    {
                        (cellNode, cellZone) = AddApexAlarmZone(
                            start,
                            WavePopulation.Baseline,
                            WaveSettings.Baseline_Normal);
                    }),

                    // 2 zones, with boss in one
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_BossFight(mid);
                    }),

                    // 2 zones with sensors
                    (0.3, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = AddZone(mid);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }
            #endregion

            #region Tier: C
            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell
                    (0.2, () => { (cellNode, cellZone) = AddZone(start); }),

                    // Single zone to cell, guarded by keycard
                    (1.0, () => { (cellNode, cellZone) = BuildChallenge_KeycardInZone(start); }),

                    // Single zone to cell, door locked down in source zone
                    (1.0, () => { (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(start, 0); }),

                    // 2 zones
                    (0.5, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = AddZone(mid);
                    }),

                    // 2 zones, with boss
                    (0.5, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_BossFight(mid);
                    }),

                    // Password in side zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalPasswordInSide(mid);
                    }),

                    // Password in side zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInSide(mid, 1);
                    }),

                    // 2 zones, with cell behind keycard
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // 2 zones, with locked second door on terminal
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 0);
                    }),

                    // 2 zones with sensors on mid
                    (0.4, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = AddZone(mid);
                    }),

                    // Keycard in side with sensors
                    (0.3, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = BuildChallenge_KeycardInSide(mid, 1);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }

            case ("C", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell
                    (1.0, () => { (cellNode, cellZone) = AddZone(start); }),

                    // Single zone to cell, guarded by keycard
                    (1.0, () => { (cellNode, cellZone) = BuildChallenge_KeycardInZone(start); }),

                    // Single zone to cell, door locked down in source zone
                    (1.0, () => { (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(start, 0); }),

                    // 2 zones, with cell behind keycard
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // 2 zones, with locked second door on terminal
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 0);
                    }),

                    // 2 zones, with cell behind keycard
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // 2 zones, with locked second door on side zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 1);
                    }),

                    // Keycard with sensors
                    (0.3, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }

            case ("C", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell, door locked down in source zone
                    (2.0, () => { (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(start, 0); }),

                    // Single zone to cell, guarded by keycard
                    (2.0, () => { (cellNode, cellZone) = BuildChallenge_KeycardInZone(start); }),

                    // Single zone to cell, apex alarm
                    (1.0, () =>
                    {
                        (cellNode, cellZone) = AddApexAlarmZone(
                            start,
                            WavePopulation.Baseline,
                            WaveSettings.Baseline_Normal);
                    }),

                    // Single zone to cell, apex alarm
                    (1.0, () =>
                    {
                        (cellNode, cellZone) = AddApexAlarmZone(
                            start,
                            WavePopulation.Baseline,
                            WaveSettings.Baseline_Normal);
                    }),

                    // Tough enemy error alarm. 2 zones, error turnoff in cell zone
                    (0.7, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = AddZone(mid);

                        var population = Generator.Select(new List<(double, WavePopulation)>
                        {
                            (2.0, WavePopulation.OnlyInfestedStrikers),
                            (1.0, WavePopulation.Baseline_InfectedHybrids),
                            (1.0, WavePopulation.OnlyChargers),
                        });

                        AddErrorAlarm(mid, cellNode, WaveSettings.Error_Normal, population);
                    }),

                    // 2 zones, with boss in one
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_BossFight(mid);
                    }),

                    // 2 zones with sensors
                    (0.4, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = AddZone(mid);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }
            #endregion

            #region Tier: D
            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell, guarded by keycard
                    (0.7, () => { (cellNode, cellZone) = BuildChallenge_KeycardInZone(start); }),

                    // Single zone to cell, door locked down in source zone
                    (0.7, () => { (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(start, 0); }),

                    // 2 zones
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = AddZone(mid);
                    }),

                    // 2 zones, with boss
                    (0.5, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_BossFight(mid);
                    }),

                    // Locked terminal in side zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalPasswordInSide(mid);
                    }),

                    // Password in side zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInSide(mid, 1);
                    }),

                    // 2 zones, with cell behind keycard
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // Tough enemy error alarm. 2 zones, error turnoff in cell zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = AddZone(mid);

                        var population = Generator.Select(new List<(double, WavePopulation)>
                        {
                            (2.0, WavePopulation.OnlyInfestedStrikers),
                            (1.0, WavePopulation.Baseline_InfectedHybrids),
                            (1.0, WavePopulation.Baseline_Hybrids),
                            (level.Settings.HasChargers() ? 1.0 : 0.0, WavePopulation.OnlyChargers),
                            (level.Settings.HasNightmares() ? 1.0 : 0.0, WavePopulation.OnlyNightmares),
                        });

                        AddErrorAlarm(mid, cellNode, WaveSettings.Error_Normal, population);
                    }),

                    // 2 zones, with locked second door on terminal
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 0);
                    }),

                    // 2 zones with sensors
                    (0.5, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = AddZone(mid);
                    }),

                    // Boss fight with sensors in approach
                    (0.3, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = BuildChallenge_BossFight(mid);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }

            case ("D", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell
                    (1.0, () => { (cellNode, cellZone) = AddZone(start); }),

                    // Single zone to cell, guarded by keycard
                    (1.0, () => { (cellNode, cellZone) = BuildChallenge_KeycardInZone(start); }),

                    // Single zone to cell, door locked down in source zone
                    (1.0, () => { (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(start, 0); }),

                    // 2 zones, with cell behind keycard
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // 2 zones, with locked second door on terminal
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 0);
                    }),

                    // 2 zones, with cell behind keycard
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // 2 zones, with locked second door on side zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 1);
                    }),

                    // Keycard with sensors
                    (0.3, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }

            case ("D", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell, door locked down in source zone
                    (0.7, () => { (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(start, 0); }),

                    // Single zone to cell, guarded by keycard
                    (0.7, () => { (cellNode, cellZone) = BuildChallenge_KeycardInZone(start); }),

                    // Single zone to cell, apex alarm
                    (1.0, () =>
                    {
                        (cellNode, cellZone) = AddApexAlarmZone(
                            start,
                            WavePopulation.Baseline,
                            WaveSettings.Baseline_Normal);
                    }),

                    // Single zone to cell, apex alarm
                    (1.0, () =>
                    {
                        (cellNode, cellZone) = AddApexAlarmZone(
                            start,
                            WavePopulation.Baseline,
                            WaveSettings.Baseline_Normal);
                    }),

                    // Tough enemy error alarm. 2 zones, error turnoff in cell zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = AddZone(mid);

                        var population = Generator.Select(new List<(double, WavePopulation)>
                        {
                            (2.0, WavePopulation.OnlyInfestedStrikers),
                            (1.0, WavePopulation.Baseline_InfectedHybrids),
                            (1.0, WavePopulation.OnlyChargers),
                        });

                        AddErrorAlarm(mid, cellNode, WaveSettings.Error_Hard, population);
                    }),

                    // 2 zones, with boss in one
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_BossFight(mid);
                    }),

                    // 2 zones with sensors
                    (0.4, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = AddZone(mid);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }
            #endregion

            #region Tier: E
            case ("E", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // 2 zones
                    (0.5, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = AddZone(mid);
                    }),

                    // 2 zones, with boss
                    (0.5, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_BossFight(mid);
                    }),

                    // Locked terminal in side zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalPasswordInSide(mid);
                    }),

                    // Password in side zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInSide(mid, 1);
                    }),

                    // 2 zones, with cell behind keycard
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // Tough enemy error alarm. 2 zones, error turnoff in cell zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = AddZone(mid);

                        var population = Generator.Select(new List<(double, WavePopulation)>
                        {
                            (2.0, WavePopulation.OnlyInfestedStrikers),
                            (1.0, WavePopulation.Baseline_InfectedHybrids),
                            (1.0, WavePopulation.Baseline_Hybrids),
                            (level.Settings.HasChargers() ? 1.0 : 0.0, WavePopulation.OnlyChargers),
                            (level.Settings.HasNightmares() ? 1.0 : 0.0, WavePopulation.OnlyNightmares),
                        });

                        AddErrorAlarm(mid, cellNode, WaveSettings.Error_Hard, population);
                    }),

                    // 2 zones, with locked second door on terminal
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 0);
                    }),

                    // 2 zones with sensors
                    (0.6, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = AddZone(mid);
                    }),

                    // Keycard in side with sensors
                    (0.5, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = BuildChallenge_KeycardInSide(mid, 1);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }

            case ("E", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Single zone to cell
                    (1.0, () => { (cellNode, cellZone) = AddZone(start); }),

                    // Single zone to cell, guarded by keycard
                    (1.0, () => { (cellNode, cellZone) = BuildChallenge_KeycardInZone(start); }),

                    // Single zone to cell, door locked down in source zone
                    (1.0, () => { (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(start, 0); }),

                    // 2 zones, with cell behind keycard
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // 2 zones, with locked second door on terminal
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 0);
                    }),

                    // 2 zones, with cell behind keycard
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),

                    // 2 zones, with locked second door on side zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 1);
                    }),

                    // Keycard with sensors
                    (0.3, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = BuildChallenge_KeycardInZone(mid);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

                break;
            }

            case ("E", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // 2 zones, with locked second door on terminal
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_LockedTerminalDoor(mid, 1);
                    }),

                    // Single zone to cell, apex alarm
                    (1.0, () =>
                    {
                        (cellNode, cellZone) = AddApexAlarmZone(
                            start,
                            WavePopulation.Baseline,
                            WaveSettings.Baseline_Normal);
                    }),

                    // Error alarm, no turn off
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = AddZone(mid);

                        var population = Generator.Select(new List<(double, WavePopulation)>
                        {
                            (2.0, WavePopulation.Baseline),
                            (level.Settings.HasShadows() ? 1.0 : 0.0, WavePopulation.OnlyShadows)
                        });

                        AddErrorAlarm(mid, null, WaveSettings.Error_Normal, population);
                    }),

                    // Tough enemy error alarm. 2 zones, error turnoff in cell zone
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = AddZone(mid);

                        var population = Generator.Select(new List<(double, WavePopulation)>
                        {
                            (2.0, WavePopulation.OnlyInfestedStrikers),
                            (1.0, WavePopulation.Baseline_InfectedHybrids),
                            (1.0, WavePopulation.OnlyChargers),
                        });

                        AddErrorAlarm(mid, cellNode, WaveSettings.Error_VeryHard, population);
                    }),

                    // 2 zones, with boss in one
                    (1.0, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);

                        if (altitude != null)
                            midZone.Altitude = altitude;

                        (cellNode, cellZone) = BuildChallenge_BossFight(mid);
                    }),

                    // 2 zones with sensors
                    (0.5, () =>
                    {
                        var (mid, midZone) = AddZone_Forward(start);
                        if (altitude != null) midZone.Altitude = altitude;
                        AddSecuritySensors(mid);
                        (cellNode, cellZone) = AddZone(mid);
                    }),
                });

                if (altitude != null)
                    cellZone.Altitude = altitude;

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

        #region Fog settings for level
        // Generally this affects more than just the central generator cluster mission, so we
        // limit the number of generator missions to just 1 in the level.
        // TODO: this will also affect any flood-the-map with fog objectives

        var invertedFog = Generator.Flip();
        var isInfectious = level.FogSettings.IsInfectious;
        level.FogSettings = (isInfectious, invertedFog) switch
        {
            (false, false) => Fog.Normal_Altitude_minus4,
            (false, true) => Fog.Inverted_Altitude_8,
            (true, false) => Fog.NormalInfectious_Altitude_minus4,
            (true, true) => Fog.InvertedInfectious_Altitude_8,
        };
        objective.FogOnGotoWin = (isInfectious, invertedFog) switch
        {
            (false, false) => Fog.Inverted_Altitude_6,
            (false, true) => Fog.Normal_Altitude_minus6,
            (true, false) => Fog.NormalInfectious_Altitude_6,
            (true, true) => Fog.InvertedInfectious_Altitude_minus6,
        };
        objective.FogTransitionDurationOnGotoWin = 6.0;
        objective.CentralGeneratorCluster_FogDataSteps = CentralGeneratorCluster_BuildFogSteps(
            objective,
            invertedFog ? GeneratorFogShape.Descending : GeneratorFogShape.Ascending,
            objective.CentralGeneratorCluster_NumberOfGenerators);

        #endregion

        // Build layout
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
                        objective.GeneratorClusterNode = start;

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
                        objective.GeneratorClusterNode = start;

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
                        objective.GeneratorClusterNode = start;

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
                        objective.GeneratorClusterNode = start;

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
                        objective.GeneratorClusterNode = start;

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
