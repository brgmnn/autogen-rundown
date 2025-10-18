using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Logs;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Terminals;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    private void BuildLayout_GatherTerminal_AlphaSix(
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
        var layerData = level.GetObjectiveLayerData(director.Bulkhead);

        var (portal, portalZone) = AddZone_Forward(start);

        // portalZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_portal_HA_01.prefab";
        // portalZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_portal_HA_01.prefab";
        portalZone.GenPortalGeomorph();

        var (mwp, mwpZone) = BuildChallenge_Small(portal);

        mwpZone.GenMatterWaveProjectorGeomorph(level.Complex);
        mwpZone.BigPickupDistributionInZone = BigPickupDistribution.MatterWaveProjector.PersistentId;

        mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer);

        // portalZone.EventsOnPortalWarp.AddSpawnWave(
        //     new GenericWave
        //     {
        //         Population = WavePopulation.OnlyNightmareGiants,
        //         Settings = WaveSettings.Error_Hard
        //     },
        //     delay: 10.0);

        portalZone.EventsOnPortalWarp.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SpawnEnemyWave,
                Delay = 10.0,
                SoundId = Sound.Enemies_DistantLowRoar,
                EnemyWaveData = new GenericWave
                {
                    Population = WavePopulation.OnlyNightmareGiants,
                    Settings = WaveSettings.Error_Hard
                },
                Dimension = DimensionIndex.Dimension1
            });

        var dimension = new Dimension
        {
            Data = Dimensions.DimensionData.AlphaOne with
            {
                StaticTerminalPlacements = new List<TerminalPlacement>
                {
                    new()
                    {
                        LogFiles = new List<LogFile>
                        {
                            DLockDecipherer.R1B1_Z40
                        }
                    }
                }
            }
        };
        dimension.FindOrPersist();

        level.DimensionDatas.Add(new Levels.DimensionData
        {
            Dimension = DimensionIndex.Dimension1,
            Data = dimension
        });

        objective.Gather_PlacementNodes.Add(start);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="_"></param>
    /// <param name="startish"></param>
    /// <exception cref="Exception"></exception>
    public void BuildLayout_GatherSmallItems(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;

        var last = new ZoneNode();
        var lastZone = new Zone(level, this);

        switch (level.Tier, director.Bulkhead)
        {
            #region Tier: A
            case ("A", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Double dead end
                    (0.12, () =>
                    {
                        startZone.GenHubGeomorph(level.Complex);

                        var (end1, end1Zone) = AddZone(start);
                        end1Zone.GenDeadEndGeomorph(level.Complex);

                        var (end2, end2Zone) = AddZone(start);
                        end2Zone.GenDeadEndGeomorph(level.Complex);

                        objective.Gather_PlacementNodes.Add(end1);
                        objective.Gather_PlacementNodes.Add(end2);
                    }),

                    // Single generator
                    (0.28, () =>
                    {
                        (last, lastZone) = BuildChallenge_GeneratorCellInSide(start);

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;
                    }),

                    // Single keycard
                    (0.34, () =>
                    {
                        (last, lastZone) = BuildChallenge_KeycardInSide(start);

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;
                    }),

                    // Locked door, side terminal
                    (0.20, () =>
                    {
                        (last, lastZone) = BuildChallenge_LockedTerminalDoor(
                            start,
                            level.Settings.Bulkheads == Bulkhead.Main ? 2 : 1);

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = CoverageMinMax.Medium;
                    }),

                    // Single boss fight
                    (0.06, () =>
                    {
                        (last, lastZone) = BuildChallenge_BossFight(start);

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;
                    }),
                });
                break;
            }

            case ("A", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Just fetch from the first zone
                    (0.2, () =>
                    {
                        objective.Gather_PlacementNodes.Add(start);
                        startZone.Coverage = CoverageMinMax.Medium;
                    }),

                    // Fetch from the second room, but it's a dead end
                    (0.2, () =>
                    {
                        (last, lastZone) = AddZone(start);
                        lastZone.GenDeadEndGeomorph(level.Complex);
                        objective.Gather_PlacementNodes.Add(last);
                    }),
                });
                break;
            }

            case ("A", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Fetch from the second room, but it's a dead end
                    (0.3, () =>
                    {
                        (last, lastZone) = AddZone(start);
                        lastZone.GenDeadEndGeomorph(level.Complex);
                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Tiny prelude
                    (0.5, () =>
                    {
                        (last, lastZone) = AddZone(start);
                        startZone.Coverage = new CoverageMinMax { Min = 10, Max = 10 };
                        lastZone.Coverage = CoverageMinMax.Medium;
                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Scout room
                    (0.2, () =>
                    {
                        last = AddScoutRoom(start);
                        objective.Gather_PlacementNodes.Add(last);
                    })
                });
                break;
            }
            #endregion

            #region Tier: B
            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub to dead ends
                    (0.20, () =>
                    {
                        if (objective.GatherRequiredCount <= 7)
                        {
                            startZone.GenHubGeomorph(level.Complex);

                            var (end1, end1Zone) = AddZone(start);
                            end1Zone.GenDeadEndGeomorph(level.Complex);

                            var (end2, end2Zone) = AddZone(start);
                            end2Zone.GenDeadEndGeomorph(level.Complex);

                            objective.Gather_PlacementNodes.Add(end1);
                            objective.Gather_PlacementNodes.Add(end2);
                        }
                        else
                        {
                            startZone.GenCorridorGeomorph(level.Complex);

                            var (hub, hubZone) = AddZone(start);
                            hubZone.GenHubGeomorph(level.Complex);

                            var (end1, end1Zone) = AddZone(hub);
                            end1Zone.GenDeadEndGeomorph(level.Complex);

                            var (end2, end2Zone) = AddZone(hub);
                            end2Zone.GenDeadEndGeomorph(level.Complex);

                            var (end3, end3Zone) = AddZone(hub);
                            end3Zone.GenDeadEndGeomorph(level.Complex);

                            objective.Gather_PlacementNodes.Add(end1);
                            objective.Gather_PlacementNodes.Add(end2);
                            objective.Gather_PlacementNodes.Add(end3);
                        }
                    }),

                    // Single generator
                    // Items distributed between first zone and the locked zone
                    (0.18, () =>
                    {
                        (last, lastZone) = BuildChallenge_GeneratorCellInSide(start, 2);

                        objective.Gather_PlacementNodes.Add(start);
                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Single keycard, double end zone items
                    (0.15, () =>
                    {
                        var (mid, _) = BuildChallenge_KeycardInSide(start);
                        (last, _) = AddZone(mid);

                        objective.Gather_PlacementNodes.Add(mid);
                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Single keycard, split items
                    (0.19, () =>
                    {
                        (last, lastZone) = BuildChallenge_KeycardInSide(start);

                        var keycard = planner.GetLastNode(director.Bulkhead, "keycard");
                        var (second, _) = AddZone(keycard);

                        objective.Gather_PlacementNodes.Add(last);
                        objective.Gather_PlacementNodes.Add(keycard);
                    }),

                    // Terminal locked door, terminal password locked
                    (0.20, () =>
                    {
                        (last, lastZone) = BuildChallenge_LockedTerminalPasswordInSide(
                            start,
                            (node) =>
                            {
                                var (password, passwordZone) = AddZone(node, new ZoneNode { Branch = "terminal_password" });

                                objective.Gather_PlacementNodes.Add(password);

                                return (password, passwordZone);
                            });
                        objective.Gather_PlacementNodes.Add(last);

                        if (Generator.Flip(0.4))
                            lastZone.GenDeadEndGeomorph(level.Complex);
                    }),

                    // Single boss fight
                    (0.08, () =>
                    {
                        (last, lastZone) = BuildChallenge_BossFight(start);
                        lastZone.Coverage = objective.GatherRequiredCount > 5
                            ? CoverageMinMax.Huge
                            : CoverageMinMax.Large;

                        objective.Gather_PlacementNodes.Add(last);
                    }),
                });
                break;
            }

            case ("B", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Just fetch from the first zone
                    (0.5, () =>
                    {
                        startZone.Coverage = CoverageMinMax.Huge;

                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Terminal locked
                    (0.3, () =>
                    {
                        (last, lastZone) = BuildChallenge_LockedTerminalDoor(start);
                        objective.Gather_PlacementNodes.Add(last);

                        if (Generator.Flip(0.4))
                            lastZone.GenDeadEndGeomorph(level.Complex);
                    }),

                    // Fetch from the second room, but it's a dead end
                    (0.2, () =>
                    {
                        (last, lastZone) = AddZone(start);
                        lastZone.GenDeadEndGeomorph(level.Complex);

                        objective.Gather_PlacementNodes.Add(last);
                    }),
                });
                break;
            }

            case ("B", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Tiny prelude
                    (0.40, () =>
                    {
                        (last, lastZone) = AddZone(start);
                        startZone.Coverage = new CoverageMinMax { Min = 10, Max = 10 };
                        lastZone.Coverage = CoverageMinMax.Medium;

                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // stealth big hub full of infected enemies
                    (0.20, () =>
                    {
                        startZone.GenHubGeomorph(level.Complex);
                        start = AddStealth_Infested(start);

                        objective.Gather_PlacementNodes.Add(start);
                    }),

                    // Big Apex alarm to enter
                    (0.20, () =>
                    {
                        var (lockedApex, _) = AddZone(start);

                        objective.Gather_PlacementNodes.Add(lockedApex);

                        // Add some extra resources
                        startZone.HealthPacks += 5;
                        startZone.ToolPacks += 4;
                        startZone.AmmoPacks += 5;

                        // Configure the wave population
                        var population = WavePopulation.Baseline_Hybrids;

                        // Chargers first, then flyers
                        if (level.Settings.HasChargers())
                            population = WavePopulation.Baseline_Chargers;

                        // Add the apex alarm
                        AddApexAlarm(lockedApex, population);
                    }),

                    // Scout room
                    (0.20, () =>
                    {
                        last = AddScoutRoom(start);

                        objective.Gather_PlacementNodes.Add(last);
                    })
                });
                break;
            }
            #endregion

            #region Tier: C
            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub to dead ends
                    (0.30, () =>
                    {
                        if (objective.GatherRequiredCount < 10)
                        {
                            startZone.GenHubGeomorph(level.Complex);

                            var (end1, end1Zone) = AddZone(start);
                            end1Zone.GenDeadEndGeomorph(level.Complex);

                            var (end2, end2Zone) = AddZone(start);
                            end2Zone.GenDeadEndGeomorph(level.Complex);

                            objective.Gather_PlacementNodes.Add(end1);
                            objective.Gather_PlacementNodes.Add(end2);
                        }
                        else
                        {
                            startZone.GenCorridorGeomorph(level.Complex);

                            var (hub, hubZone) = AddZone(start);
                            hubZone.GenHubGeomorph(level.Complex);

                            var (end1, end1Zone) = AddZone(hub);
                            end1Zone.GenDeadEndGeomorph(level.Complex);

                            var (end2, end2Zone) = AddZone(hub);
                            end2Zone.GenDeadEndGeomorph(level.Complex);

                            var (end3, end3Zone) = AddZone(hub);
                            end3Zone.GenDeadEndGeomorph(level.Complex);

                            objective.Gather_PlacementNodes.Add(end1);
                            objective.Gather_PlacementNodes.Add(end2);
                            objective.Gather_PlacementNodes.Add(end3);
                        }
                    }),

                    // Single terminal locked zone
                    (0.34, () =>
                    {
                        (last, lastZone) = BuildChallenge_KeycardInSide(
                            start,
                            level.Settings.Bulkheads switch
                            {
                                Bulkhead.Main => 2,
                                Bulkhead.PrisonerEfficiency => 1,
                                _ => Generator.Between(1, 2)
                            });

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;
                    }),

                    // Single generator
                    // Items distributed between first zone and the locked zone
                    (0.28, () =>
                    {
                        startZone.Coverage = CoverageMinMax.Medium;
                        objective.Gather_PlacementNodes.Add(start);

                        if (objective.GatherRequiredCount >= 10)
                        {
                            (start, startZone) = AddZone(start);

                            startZone.Coverage = CoverageMinMax.Medium;
                            objective.Gather_PlacementNodes.Add(start);
                        }

                        (last, lastZone) = BuildChallenge_GeneratorCellInSide(start, Generator.Between(1, 2));

                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Single keycard, double end zone items
                    (0.15, () =>
                    {
                        var (mid, _) = BuildChallenge_KeycardInSide(start);
                        (last, _) = AddZone(mid);

                        objective.Gather_PlacementNodes.Add(mid);
                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Single keycard, split items
                    (0.19, () =>
                    {
                        (last, lastZone) = BuildChallenge_KeycardInSide(start, 3);

                        var keycard = planner.GetLastNode(director.Bulkhead, "keycard");
                        var (second, _) = AddZone(keycard);

                        objective.Gather_PlacementNodes.Add(last);
                        objective.Gather_PlacementNodes.Add(keycard);
                    }),

                    // Single boss fight
                    (0.08, () =>
                    {
                        (last, lastZone) = BuildChallenge_BossFight(start);
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;

                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Double generator
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());

                        objective.Gather_PlacementNodes.Add(mid);

                        var nodes2 = AddBranch_Forward(mid, 1);
                        (last, _) = BuildChallenge_GeneratorCellInSide(nodes2.Last());

                        objective.Gather_PlacementNodes.Add(last);
                    }),
                });
                break;
            }

            case ("C", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Simple straight fetch with mid-point locked on in zone
                    (0.25, () =>
                    {
                        var nodes = AddBranch(start, 2, "primary", (node, _) =>
                        {
                            objective.Gather_PlacementNodes.Add(node);
                        });

                        AddTerminalUnlockPuzzle(nodes.First(), start);
                    }),

                    // End zone locked on terminal in side zone
                    (0.25, () =>
                    {
                        (last, lastZone) = BuildChallenge_LockedTerminalDoor(start, 1);

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = CoverageMinMax.Large;
                    }),

                    // Single generator
                    // Items distributed between first zone and the locked zone
                    (0.25, () =>
                    {
                        (last, lastZone) = BuildChallenge_GeneratorCellInSide(start);

                        objective.Gather_PlacementNodes.Add(start);
                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Single keycard
                    // Items distributed between first zone and the locked zone
                    (0.25, () =>
                    {
                        (last, lastZone) = BuildChallenge_KeycardInZone(start);

                        if (level.Settings.Bulkheads != Bulkhead.PrisonerEfficiency)
                        {
                            objective.Gather_PlacementNodes.Add(last);

                            (last, lastZone) = AddZone(last);

                            lastZone.Coverage = CoverageMinMax.Nano;
                        }

                        objective.Gather_PlacementNodes.Add(start);
                        objective.Gather_PlacementNodes.Add(last);
                    }),
                });
                break;
            }

            case ("C", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.50, () =>
                    {
                        AddBranch(start, Generator.Between(1, 2), "find_items", (node, _) =>
                        {
                            objective.Gather_PlacementNodes.Add(node);
                        });
                    }),

                    // stealth big hub full of infected enemies
                    (0.30, () =>
                    {
                        startZone.GenHubGeomorph(level.Complex);
                        start = AddStealth_Infested(start);

                        objective.Gather_PlacementNodes.Add(start);
                    }),

                    // Agro boss in second zone
                    (0.20, () =>
                    {
                        // Add extra zone
                        (start, startZone) = AddZone(start);
                        last = AddAlignedBoss_WakeOnOpen(start);

                        startZone.Coverage = CoverageMinMax.Small_16;

                        objective.Gather_PlacementNodes.Add(last);
                    })
                });
                break;
            }
            #endregion

            #region Tier: D
            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Boss fight to Apex
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, Generator.Between(1, 2));
                        var (mid, midZone) = BuildChallenge_BossFight(nodes.Last());

                        objective.Gather_PlacementNodes.Add(nodes.Last());

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid, new ZoneNode { Branch = "primary" });
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;

                        objective.Gather_PlacementNodes.Add(mid2);

                        var (last, lastZone) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);

                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Locked end zone, terminal password, password keycard
                    (0.25, () =>
                    {
                        (last, lastZone) = BuildChallenge_LockedTerminalPasswordInSide(
                            start,
                            (passwordStart) =>
                            {
                                var midNodes = AddBranch_Forward(passwordStart, 2);
                                var corridor = planner.GetZone(midNodes.First())!;
                                corridor.GenCorridorGeomorph(level.Complex);

                                var (password, passwordZone) = BuildChallenge_KeycardInSide(midNodes.Last());
                                objective.Gather_PlacementNodes.Add(password);

                                return (password, passwordZone);
                            });

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.GenDeadEndGeomorph(level.Complex);
                    }),

                    // Keycard to Apex alarm
                    (0.25, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (mid, midZone) = BuildChallenge_KeycardInSide(nodes.Last(), Generator.Between(1, 2));

                        objective.Gather_PlacementNodes.Add(nodes.Last());

                        midZone.GenCorridorGeomorph(level.Complex);

                        var (mid2, mid2Zone) = AddZone(mid, new ZoneNode { Branch = "primary" });
                        mid2Zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward;
                        // mid2Zone.SetStartExpansionFromExpansion();

                        objective.Gather_PlacementNodes.Add(mid2);

                        var (last, _) = BuildChallenge_ApexAlarm(
                            mid2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);

                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Error with off + cell carry
                    (0.20, () =>
                    {
                        var (mid, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start,
                            Generator.Between(2, 4),
                            1);

                        (last, _) = AddZone(mid);
                        objective.Gather_PlacementNodes.Add(last);
                    }),
                });
                break;
            }

            case ("D", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Easy grab items
                    (0.20, () =>
                    {
                        AddBranch(start, Generator.Between(1, 2), "find_items", (node, _) =>
                        {
                            objective.Gather_PlacementNodes.Add(node);
                        });
                    }),

                    // End zone locked on terminal in side zone
                    (0.25, () =>
                    {
                        (last, lastZone) = BuildChallenge_LockedTerminalDoor(start, 1);

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = CoverageMinMax.Large;
                    }),

                    // Single generator
                    // Items distributed between first zone and the locked zone
                    (0.20, () =>
                    {
                        (last, lastZone) = BuildChallenge_GeneratorCellInSide(
                            start,
                            level.Settings.Bulkheads == Bulkhead.PrisonerEfficiency ? 1 : 2);

                        objective.Gather_PlacementNodes.Add(start);
                        objective.Gather_PlacementNodes.Add(last);
                    }),
                });
                break;
            }

            case ("D", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // stealth big hub full of infected enemies
                    (0.30, () =>
                    {
                        startZone.GenHubGeomorph(level.Complex);
                        start = AddStealth_Infested(start);

                        objective.Gather_PlacementNodes.Add(start);
                    }),

                    // Fill level with infectious fog slowly over time
                    (!level.Settings.HasFog() ? 0.0 : 0.30, () =>
                    {
                        // Make the zone size small
                        startZone.Coverage = CoverageMinMax.Small_10;

                        // Fog duration in seconds. It takes about 2/3'rds of the time to be over
                        // the head of players at mid-height level. So even though these are at
                        // 60mins+ players will be in fog by 40 mins or so
                        var fogRiseDuration = 60.0 * level.Settings.Bulkheads switch
                        {
                            Bulkhead.PrisonerEfficiency => Generator.Between(75, 85),
                            _ => Generator.Between(60, 75)
                        };

                        // Ensure there's a fog turbine
                        startZone.BigPickupDistributionInZone = BigPickupDistribution.FogTurbine.PersistentId;
                        startZone.EventsOnOpenDoor
                            .AddSetFog(Fog.HeavyFullFog_Infectious, 2.0, fogRiseDuration, null)
                            .AddSound(Sound.Environment_DistantFan, 6.0);

                        // Add resources to deal with the infectious fog
                        startZone.ConsumableDistributionInZone
                            = ConsumableDistribution.Baseline_FogRepellers.PersistentId;
                        startZone.DisinfectPacks += 5.0;

                        objective.Gather_PlacementNodes.Add(start);
                    }),

                    // Agro boss in first zone
                    (0.10, () =>
                    {
                        // Add extra zone
                        (start, startZone) = AddZone(start);
                        last = AddAlignedBoss_WakeOnOpen(start);

                        objective.Gather_PlacementNodes.Add(last);
                    }),
                });
                break;
            }
            #endregion

            #region Tier: E
            case ("E", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error alarm and keycard. Zone layout is as follows:
                    //   start -> node2 -> [0-1] -> end -> items   -> error
                    //                                  -> keycard
                    (0.15, () =>
                    {
                        var (mid, midZone) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(2, 3),
                            1,
                            Generator.Between(1, 2)
                        );

                        midZone.Coverage = CoverageMinMax.Medium;

                        objective.Gather_PlacementNodes.Add(mid);
                        objective.Gather_PlacementNodes.Add(
                            planner.GetZones(director.Bulkhead, null)
                                .Where(node => node != mid)
                                .PickRandom());
                    }),

                    // Error alarm with generator lock
                    //      start (cell) -> node2 -> [0-1] -> end (generator) -> items -> error_turnoff
                    (0.20, () =>
                    {
                        var (mid, midZone) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start,
                            Generator.Between(2, 3),
                            1);

                        midZone.Coverage = CoverageMinMax.Medium;

                        objective.Gather_PlacementNodes.Add(mid);
                        objective.Gather_PlacementNodes.Add(
                            planner.GetZones(director.Bulkhead, null)
                                .Where(node => node != mid)
                                .PickRandom());
                    }),

                    // 1 generator lock
                    (0.13, () =>
                    {
                        var (prelude, preludeZone) = AddZone(start, new ZoneNode
                        {
                            Branch = "primary",
                            MaxConnections = 3
                        });
                        preludeZone.GenTGeomorph(level.Complex);

                        var (locked, _) = AddZone(prelude);
                        var cellZones = AddBranch_Side(prelude, 3, "power_cell");

                        // Lock the first zone
                        AddGeneratorPuzzle(locked, cellZones.Last());

                        objective.Gather_PlacementNodes.Add(cellZones.Take(2).PickRandom());
                        objective.Gather_PlacementNodes.Add(locked);
                    }),

                    // 2 keycards
                    (0.40, () =>
                    {
                        // Update number of connections for hub zone
                        planner.UpdateNode(start with { MaxConnections = 3 });
                        startZone.GenHubGeomorph(level.Complex);

                        // Second area, with also a locked zone
                        var (items1, items1Zone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        items1Zone.GenHubGeomorph(level.Complex);

                        var keycard1 = AddBranch(start, 1, "keycard_1").Last();

                        // Lock the first zone
                        AddKeycardPuzzle(items1, keycard1);

                        // Build the second keycard zone and
                        var keycard2 = AddBranch(items1, 1, "keycard_2").Last();
                        var (items2, items2Zone) = AddZone(items1);

                        items2Zone.Coverage = CoverageMinMax.Medium;

                        // Lock the first zone
                        AddKeycardPuzzle(items2, keycard2);

                        objective.Gather_PlacementNodes.Add(items1);
                        objective.Gather_PlacementNodes.Add(items2);
                        objective.Gather_PlacementNodes.Add(keycard2);
                    }),

                    // Level-wide error alarm
                    (0.12, () =>
                    {
                        var population = WavePopulation.Baseline;

                        // First set shadows if we have them
                        if (level.Settings.HasShadows())
                            population = Generator.Flip(0.6) ? WavePopulation.OnlyShadows : WavePopulation
                                .Baseline_Shadows;

                        // Next check and set chargers first, then flyers
                        if (level.Settings.HasChargers())
                            population = WavePopulation.Baseline_Chargers;
                        else if (level.Settings.HasFlyers())
                            population = WavePopulation.Baseline_Flyers;
                        else if (level.Settings.HasNightmares())
                            population = WavePopulation.Baseline_Nightmare;

                        objective.WavesOnElevatorLand.Add(GenericWave.ErrorAlarm_Hard with
                        {
                            Population = population
                        });
                        level.MarkAsErrorAlarm();

                        AddBranch(start, 3, "primary", (node, _) =>
                            {
                                objective.Gather_PlacementNodes.Add(node);
                            });
                    })
                });
                break;
            }

            case ("E", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // End zone locked on terminal in side zone
                    (0.30, () =>
                    {
                        (last, lastZone) = BuildChallenge_LockedTerminalDoor(start, 1);

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = CoverageMinMax.Large;
                    }),

                    // End zone keycard
                    (0.30, () =>
                    {
                        (last, lastZone) = BuildChallenge_KeycardInSide(start, 1);

                        objective.Gather_PlacementNodes.Add(last);
                        lastZone.Coverage = CoverageMinMax.Large;
                    }),

                    // Single generator
                    // Items distributed between first zone and the locked zone
                    (0.20, () =>
                    {
                        (last, lastZone) = BuildChallenge_GeneratorCellInSide(
                            start,
                            level.Settings.Bulkheads == Bulkhead.PrisonerEfficiency ? 1 : 2);

                        objective.Gather_PlacementNodes.Add(start);
                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Boss fight
                    (0.20, () =>
                    {
                        var nodes = AddBranch_Forward(start, 1);
                        var (end, _) = BuildChallenge_BossFight(nodes.Last());

                        objective.Gather_PlacementNodes.Add(end);
                    }),
                });
                break;
            }

            case ("E", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex alarm (infested)
                    (0.20, () =>
                    {
                        (last, lastZone) = BuildChallenge_ApexAlarm(start,
                            WavePopulation.Baseline_Infested,
                            WaveSettings.Baseline_VeryHard);

                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Apex alarm (chargers)
                    (0.10, () =>
                    {
                        (last, lastZone) = BuildChallenge_ApexAlarm(start,
                            WavePopulation.OnlyChargers,
                            WaveSettings.Baseline_Hard);

                        objective.Gather_PlacementNodes.Add(last);
                    }),

                    // Fill level with infectious fog slowly over time
                    (!level.Settings.HasFog() ? 0.0 : 0.50, () =>
                    {
                        // Make the zone size small
                        startZone.Coverage = CoverageMinMax.Small_10;

                        // Fog duration in seconds. It takes about 2/3'rds of the time to be over
                        // the head of players at mid-height level. So even though these are at
                        // 60mins+ players will be in fog by 40 mins or so
                        var fogRiseDuration = 60.0 * level.Settings.Bulkheads switch
                        {
                            Bulkhead.PrisonerEfficiency => Generator.Between(75, 85),
                            _ => Generator.Between(60, 75)
                        };

                        // Ensure there's a fog turbine
                        startZone.BigPickupDistributionInZone = BigPickupDistribution.FogTurbine.PersistentId;
                        startZone.EventsOnOpenDoor
                            .AddSetFog(Fog.HeavyFullFog_Infectious, 2.0, fogRiseDuration, null)
                            .AddSound(Sound.Environment_DistantFan, 6.0);

                        // Add resources to deal with the infectious fog
                        startZone.ConsumableDistributionInZone
                            = ConsumableDistribution.Baseline_FogRepellers.PersistentId;
                        startZone.DisinfectPacks += 2.0;

                        objective.Gather_PlacementNodes.Add(start);
                    }),

                    // Agro boss in first zone
                    (0.10, () =>
                    {
                        // Add extra zone
                        (start, startZone) = AddZone(start);
                        last = AddAlignedBoss_WakeOnOpen(start);

                        objective.Gather_PlacementNodes.Add(last);
                    })
                });
                break;
            }
            #endregion

            default:
            {
                AddBranch(start, Generator.Between(1, 2), "find_items", (node, _) =>
                {
                    objective.Gather_PlacementNodes.Add(node);
                });
                break;
            }
        }
    }
}
