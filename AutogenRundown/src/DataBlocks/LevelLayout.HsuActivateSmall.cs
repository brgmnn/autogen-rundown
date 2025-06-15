using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Utils;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    public void BuildLayout_HsuActivateSmall(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var layerData = level.GetObjectiveLayerData(director.Bulkhead);

        var end = new ZoneNode();
        var endZone = new Zone(level);

        #region Level zone layout
        // Level generation
        switch (level.Tier)
        {
            case "A":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight shot
                    (0.20, () =>
                    {
                        var nodes = AddBranch(start, Generator.Between(2, 4));

                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Single generator
                    (0.35, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                    }),

                    // Single keycard mid
                    (0.25, () =>
                    {
                        var next = AddBranch_Forward(start, 1).Last();
                        (next, _) = BuildChallenge_KeycardInSide(next);
                        var nodes = AddBranch_Forward(next, Generator.Between(1, 2));

                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Single keycard end
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        (end, endZone) = BuildChallenge_KeycardInSide(nodes.Last());
                    })
                });
                break;
            }

            case "B":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex Alarm end
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(3, 4));
                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;

                        var hub = nodes.ElementAt(nodes.Count - 2);
                        var hubZone = planner.GetZone(hub)!;

                        hubZone.AmmoPacks += 3.0;
                        hubZone.ToolPacks += 2.0;

                        var population = WavePopulation.Baseline;
                        var settings = WaveSettings.Baseline_Normal;

                        AddApexAlarm(end, population, settings);
                    }),

                    // Straight shot
                    (0.10, () =>
                    {
                        var nodes = AddBranch(start, Generator.Between(2, 4));

                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Single generator
                    (0.30, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(nodes.Last());
                    }),

                    // Single keycard mid
                    (0.15, () =>
                    {
                        var next = AddBranch_Forward(start, 1).Last();
                        (next, _) = BuildChallenge_KeycardInSide(next);
                        var nodes = AddBranch_Forward(next, Generator.Between(1, 2));

                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    }),

                    // Single keycard end
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                        (end, endZone) = BuildChallenge_KeycardInSide(nodes.Last());

                    })
                });
                break;
            }

            case "C":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard to Apex alarm
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, midZone) = BuildChallenge_KeycardInSide(nodes.Last());

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Normal);
                    }),

                    // Double generator
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());

                        var nodes2 = AddBranch_Forward(mid, 1);
                        (end, endZone) = BuildChallenge_GeneratorCellInSide(nodes2.Last());
                    }),

                    // Generator to boss
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());

                        var nodes2 = AddBranch_Forward(mid, 1);
                        (end, endZone) = BuildChallenge_BossFight(nodes2.Last());
                    }),

                    // Error with off + key card
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(2, 3),
                            1,
                            1);

                        (end, endZone) = AddZone(mid);
                    }),

                    // Straight to Machine... with error alarm active
                    // No turning it off
                    (0.05, () =>
                    {
                        objective.WavesOnElevatorLand.Add(GenericWave.ErrorAlarm_Normal);
                        level.MarkAsErrorAlarm();

                        var nodes = AddBranch_Forward(start, 3);

                        end = nodes.Last();
                        endZone = planner.GetZone(end)!;
                    })
                });
                break;
            }

            case "D":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard to Apex alarm
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, midZone) = BuildChallenge_KeycardInSide(nodes.Last(), Generator.Between(1, 2));

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                    }),

                    // Boss fight to Apex
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));

                        var (mid, midZone) = BuildChallenge_BossFight(nodes.Last());

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                    }),

                    // Error with off + cell carry
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start,
                            Generator.Between(2, 4),
                            1);

                        (end, endZone) = AddZone(mid);
                    }),

                    // Error with off + key card
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(1, 3),
                            Generator.Between(1, 2),
                            1);

                        (end, endZone) = AddZone(mid);
                    }),

                    // Boss Fight: Mega Mother
                    // We also do some interesting prelude zones to get to megamom
                    (0.20, () =>
                    {
                        // var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var bossStart = start;

                        // Have some choices on arriving at the mega mom
                        Generator.SelectRun(new List<(double, Action)>
                        {
                            // Generator access
                            (0.35, () =>
                            {
                                var nodes = AddBranch_Forward(start, 2);
                                (bossStart, _) = BuildChallenge_GeneratorCellInSide(
                                    nodes.Last(),
                                    Generator.Between(1, 2));
                            }),

                            // Keycard access
                            (0.45, () =>
                            {
                                var nodes = AddBranch_Forward(start, 2);
                                (bossStart, _) = BuildChallenge_KeycardInSide(
                                    nodes.Last(),
                                    Generator.Between(1, 2));
                            }),

                            // Apex alarm
                            (0.20, () =>
                            {
                                var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                                (bossStart, _) = BuildChallenge_ApexAlarm(
                                    nodes.Last(),
                                    WavePopulation.Baseline_Hybrids,
                                    WaveSettings.Baseline_Hard);
                            })
                        });

                        var (boss, bossZone) = AddZone(
                            bossStart,
                            new ZoneNode
                            {
                                Branch = "boss_fight",
                                Tags = new Tags("no_blood_door")
                            });
                        (end, endZone) = AddZone(boss);

                        endZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

                        var bossClearEvents = new List<WardenObjectiveEvent>()
                            .AddUnlockDoor(director.Bulkhead, end.ZoneNumber);

                        AddAlignedBossFight_MegaMom(boss, bossClearEvents);
                    })
                });
                break;
            }

            case "E":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Keycard to Apex alarm
                    (0.10, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, midZone) = BuildChallenge_KeycardInSide(nodes.Last(), Generator.Between(1, 2));

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                    }),

                    // Boss fight to Apex
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(2, 3));

                        var (mid, midZone) = BuildChallenge_BossFight(nodes.Last());

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid);
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        mid2Zone.SetStartExpansionFromExpansion();

                        (end, endZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                    }),

                    // Error with off + cell carry
                    (0.25, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start,
                            Generator.Between(2, 4),
                            1);

                        (end, endZone) = AddZone(mid);
                    }),

                    // Error with off + key card
                    (0.15, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(1, 3),
                            Generator.Between(1, 2),
                            1);

                        (end, endZone) = AddZone(mid);
                    }),

                    // Boss Fight: Mega Mother
                    // We also do some interesting prelude zones to get to megamom
                    (0.25, () =>
                    {
                        // var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var bossStart = start;

                        // Have some choices on arriving at the mega mom
                        Generator.SelectRun(new List<(double, Action)>
                        {
                            // Generator access
                            (0.35, () =>
                            {
                                var nodes = AddBranch_Forward(start, 2);
                                (bossStart, _) = BuildChallenge_GeneratorCellInSide(
                                    nodes.Last(),
                                    Generator.Between(1, 2));
                            }),

                            // Keycard access
                            (0.45, () =>
                            {
                                var nodes = AddBranch_Forward(start, 2);
                                (bossStart, _) = BuildChallenge_KeycardInSide(
                                    nodes.Last(),
                                    Generator.Between(1, 2));
                            }),

                            // Apex alarm
                            (0.20, () =>
                            {
                                var nodes = AddBranch_Forward(start, Generator.Between(2, 3));
                                (bossStart, _) = BuildChallenge_ApexAlarm(
                                    nodes.Last(),
                                    WavePopulation.Baseline_Hybrids,
                                    WaveSettings.Baseline_VeryHard);
                            })
                        });

                        var (boss, bossZone) = AddZone(
                            bossStart,
                            new ZoneNode
                            {
                                Branch = "boss_fight",
                                Tags = new Tags("no_blood_door")
                            });
                        (end, endZone) = AddZone(boss);

                        endZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

                        var bossClearEvents = new List<WardenObjectiveEvent>()
                            .AddUnlockDoor(director.Bulkhead, end.ZoneNumber);

                        AddAlignedBossFight_MegaMom(boss, bossClearEvents);
                    })
                });
                break;
            }
        }
        #endregion

        #region End zone machine setup
        // Set up the end zone
        switch (level.Complex)
        {
            case Complex.Mining:
            {
                // Works with:
                //  - Data Sphere

                // Machine -> UNS = Unsealer Machine
                endZone.Coverage = new CoverageMinMax { Min = 16.0, Max = 32.0 };
                endZone.CustomGeomorph =
                    "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_dead_end_HA_01.prefab";

                objective.HsuActivateSmall_MachineName = "Unsealer Machine";
                break;
            }

            case Complex.Tech:
            {
                // transformZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_Lab_dead_end_room_02_R8C2.prefab"; // Doesn't work?
                // transformZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_Lab_dead_end_room_02.prefab"; // Neonate I think, works with Data Sphere too but weird

                // Works with:
                //  - Data Sphere

                // Machine -> NCR = Neurogenic Cardiac Resuscitator
                endZone.Coverage = new CoverageMinMax { Min = 16.0, Max = 32.0 };
                endZone.CustomGeomorph =
                    "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_Lab_dead_end_room_01.prefab";


                // We give it a more generic name that will work for the Data Sphere
                objective.HsuActivateSmall_MachineName = "Neural Command Relay";
                break;
            }

            case Complex.Service:
            {
                // Works with:
                //  -

                endZone.Coverage = new CoverageMinMax { Min = 16.0, Max = 32.0 };
                endZone.CustomGeomorph =
                    "Assets/AssetPrefabs/Complex/Service/Geomorphs/Gardens/geo_64x64_service_gardens_I_01.prefab";

                objective.HsuActivateSmall_MachineName = "Unsealer Machine";
                break;
            }
        }

        layerData.ObjectiveData.ZonePlacementDatas.Add(new List<ZonePlacementData>
        {
            new()
            {
                DimensionIndex = 0,
                LocalIndex = end.ZoneNumber
            }
        });
        #endregion
    }
}
