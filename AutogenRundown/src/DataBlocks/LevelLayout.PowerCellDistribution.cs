using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    // --- Helpers ---

    /// <summary>
    /// Places a single power generator in the given zone and registers
    /// ZonePlacementDatas for the objective UI.
    /// </summary>
    private void PlaceGenerator(ZoneNode node)
    {
        var zone = planner.GetZone(node)!;
        var objectiveLayerData = level.GetObjectiveLayerData(director.Bulkhead);

        zone.PowerGeneratorPlacements.Add(
            new FunctionPlacementData
            {
                PlacementWeights = ZonePlacementWeights.NotAtStart
            });

        objectiveLayerData.ObjectiveData.ZonePlacementDatas.Add(
            new List<ZonePlacementData>
            {
                new()
                {
                    LocalIndex = node.ZoneNumber,
                    Weights = ZonePlacementWeights.NotAtStart
                }
            });
    }

    /// <summary>
    /// Places multiple generators in a single zone. For clustered layouts.
    /// </summary>
    private void PlaceGeneratorsInZone(ZoneNode node, int count)
    {
        for (var i = 0; i < count; i++)
            PlaceGenerator(node);
    }

    /// <summary>
    /// Builds a branch of Generator.Between(1, 2) + depth zones and places a generator
    /// at the end. Returns the last node.
    /// </summary>
    private ZoneNode BuildGeneratorBranch(ZoneNode baseNode, string branch, int depth = 0)
    {
        var nodes = AddBranch(baseNode, Generator.Between(1, 2) + depth, branch);
        PlaceGenerator(nodes.Last());
        return nodes.Last();
    }

    // --- Fast version (unchanged) ---

    public void BuildLayout_PowerCellDistribution_Fast(ZoneNode start)
    {
        var startZone = planner.GetZone(start)!;
        var objectiveLayerData = level.GetObjectiveLayerData(director.Bulkhead);

        startZone.Coverage = CoverageMinMax.Small_16;

        for (var g = 0; g < objective.PowerCellsToDistribute; g++)
        {
            startZone.PowerGeneratorPlacements.Add(
                new FunctionPlacementData
                {
                    PlacementWeights = ZonePlacementWeights.NotAtStart
                });

            objectiveLayerData.ObjectiveData.ZonePlacementDatas.Add(
                new List<ZonePlacementData>
                {
                    new()
                    {
                        LocalIndex = start.ZoneNumber,
                        Weights = ZonePlacementWeights.NotAtStart
                    }
                });
        }
    }

    // --- Main entry point ---

    public void BuildLayout_PowerCellDistribution(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;

        // --- Fast version ---
        if (level.MainDirector.Objective is WardenObjectiveType.ReachKdsDeep or WardenObjectiveType.Cryptomnesia)
        {
            BuildLayout_PowerCellDistribution_Fast(start);
            return;
        }

        var numGens = objective.PowerCellsToDistribute;

        switch (level.Tier, director.Bulkhead)
        {
            #region A-tier
            // A-Main: 1-2 gens, 5 variants
            // Simple introductory layouts.
            case ("A", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Linear direct — start → 1-2 zones → gen zone (cluster if 2)
                    (0.25, () =>
                    {
                        var nodes = AddBranch(start, Generator.Between(1, 2), "generator");
                        PlaceGeneratorsInZone(nodes.Last(), numGens);
                        AddForwardExtractStart(nodes.Last());
                    }),

                    // Hub + branches — start → hub → 1 branch per gen
                    (0.25, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        for (var g = 0; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(hub, $"generator_{g}");
                            AddForwardExtractStart(last);
                        }
                    }),

                    // Linear + keycard — start → KeycardInZone → gen zone(s)
                    (0.20, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInZone(start);
                        var nodes = AddBranch(end, 1, "generator");
                        PlaceGeneratorsInZone(nodes.Last(), numGens);
                        AddForwardExtractStart(nodes.Last());
                    }),

                    // Linear + locked terminal — start → LockedTerminalDoor(0) → gen zone(s)
                    (0.15, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalDoor(start, 0);
                        var nodes = AddBranch(end, 1, "generator");
                        PlaceGeneratorsInZone(nodes.Last(), numGens);
                        AddForwardExtractStart(nodes.Last());
                    }),

                    // Hub + keycard gate — hub → KeycardInZone branch + gen branch(es)
                    (0.15, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // First branch gated by keycard
                        var (kcEnd, _) = BuildChallenge_KeycardInZone(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_0");
                        PlaceGenerator(kcNodes.Last());
                        AddForwardExtractStart(kcNodes.Last());

                        // Remaining gens get their own branches
                        for (var g = 1; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(hub, $"generator_{g}");
                            AddForwardExtractStart(last);
                        }
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                        AddForwardExtractStart(genNodes.Last());
                    }),
                });
                break;
            }

            // A-Extreme: 1-2 gens, 3 variants — simpler than A-Main
            case ("A", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Direct — start → gen zone (cluster if 2)
                    (0.40, () =>
                    {
                        var nodes = AddBranch(start, 1, "generator");
                        PlaceGeneratorsInZone(nodes.Last(), numGens);
                    }),

                    // KeycardInZone — start → KeycardInZone → gen zone
                    (0.35, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInZone(start);
                        var nodes = AddBranch(end, 1, "generator");
                        PlaceGeneratorsInZone(nodes.Last(), numGens);
                    }),

                    // Hub + branches — start → hub → 1 branch per gen
                    (0.25, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        for (var g = 0; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                    }),
                });
                break;
            }

            // A-Overload: 1-2 gens, 3 variants — B-ish difficulty
            case ("A", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Locked terminal — start → LockedTerminalDoor(0) → gen zone(s)
                    (0.35, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalDoor(start, 0);
                        var nodes = AddBranch(end, 1, "generator");
                        PlaceGeneratorsInZone(nodes.Last(), numGens);
                    }),

                    // KeycardInSide — start → KeycardInSide → gen zone(s)
                    (0.35, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInSide(start);
                        var nodes = AddBranch(end, 1, "generator");
                        PlaceGeneratorsInZone(nodes.Last(), numGens);
                    }),

                    // Hub + keycard — hub → KeycardInZone branch + gen branch(es)
                    (0.30, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        var (kcEnd, _) = BuildChallenge_KeycardInZone(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_0");
                        PlaceGenerator(kcNodes.Last());

                        for (var g = 1; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                    }),
                });
                break;
            }
            #endregion

            #region B-tier
            // B-Main: 1-3 gens, 5 variants — moderate complexity, sensors first appear
            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub + keycard branch — hub → gen branches, one gated by KeycardInZone
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // First branch gated by keycard
                        var (kcEnd, _) = BuildChallenge_KeycardInZone(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_0");
                        PlaceGenerator(kcNodes.Last());
                        AddForwardExtractStart(kcNodes.Last());

                        for (var g = 1; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(hub, $"generator_{g}");
                            AddForwardExtractStart(last);
                        }
                    }),

                    // Linear + KeycardInSide — start → 1 zone → KeycardInSide → gen zone(s)
                    (0.20, () =>
                    {
                        var nodes = AddBranch(start, 1, "approach");
                        var (end, _) = BuildChallenge_KeycardInSide(nodes.Last());
                        var genNodes = AddBranch(end, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                        AddForwardExtractStart(genNodes.Last());
                    }),

                    // Hub + sensors — hub [with sensors on approach] → gen branches
                    (0.20, () =>
                    {
                        var approach = AddBranch(start, 1, "approach");
                        AddSecuritySensors(approach.Last());

                        var (hub, hubZone) = AddZone(approach.Last(), new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        for (var g = 0; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(hub, $"generator_{g}");
                            AddForwardExtractStart(last);
                        }
                    }),

                    // Linear + locked terminal — start → LockedTerminalDoor(0) → 1 zone → gen zone(s)
                    (0.20, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalDoor(start, 0);
                        var nodes = AddBranch(end, 1, "generator");
                        PlaceGeneratorsInZone(nodes.Last(), numGens);
                        AddForwardExtractStart(nodes.Last());
                    }),

                    // Hub + keycard on one branch — hub → branch1(gen), branch2(KeycardInZone + gen)
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // First branch: plain generator
                        var last0 = BuildGeneratorBranch(hub, "generator_0");
                        AddForwardExtractStart(last0);

                        // Second branch: keycard + gen
                        var (kcEnd, _) = BuildChallenge_KeycardInZone(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_1");
                        PlaceGenerator(kcNodes.Last());
                        AddForwardExtractStart(kcNodes.Last());

                        for (var g = 2; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(hub, $"generator_{g}");
                            AddForwardExtractStart(last);
                        }
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                        AddForwardExtractStart(genNodes.Last());
                    }),
                });
                break;
            }

            // B-Extreme: 1-3 gens, 3 variants — A-Main equivalent
            case ("B", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub + branches — hub → 1-3 gen branches, no challenges
                    (0.40, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        for (var g = 0; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // KeycardInZone — start → KeycardInZone → gen zone(s)
                    (0.30, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInZone(start);
                        var nodes = AddBranch(end, 1, "generator");
                        PlaceGeneratorsInZone(nodes.Last(), numGens);
                    }),

                    // Locked terminal — start → LockedTerminalDoor(0) → gen zone(s)
                    (0.30, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalDoor(start, 0);
                        var nodes = AddBranch(end, 1, "generator");
                        PlaceGeneratorsInZone(nodes.Last(), numGens);
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                    }),
                });
                break;
            }

            // B-Overload: 1-3 gens, 4 variants — C-Main equivalent
            case ("B", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub + KeycardInSide — hub → KeycardInSide → gen branches
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInSide(start);

                        var (hub, hubZone) = AddZone(end, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        for (var g = 0; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // Locked terminal + side — start → LockedTerminalDoor(1) → hub → gen branches
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalDoor(start, 1);

                        var (hub, hubZone) = AddZone(end, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        for (var g = 0; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // Sensors + hub — start → sensors zone → hub → gen branches
                    (0.25, () =>
                    {
                        var approach = AddBranch(start, 1, "approach");
                        AddSecuritySensors(approach.Last());

                        var (hub, hubZone) = AddZone(approach.Last(), new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        for (var g = 0; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // Hub + locked terminal branch — hub → branches, one with LockedTerminalDoor
                    (0.25, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // First branch behind locked terminal
                        var (ltEnd, _) = BuildChallenge_LockedTerminalDoor(hub, 0);
                        var ltNodes = AddBranch(ltEnd, 1, "generator_0");
                        PlaceGenerator(ltNodes.Last());

                        for (var g = 1; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                    }),
                });
                break;
            }
            #endregion

            #region C-tier
            // C-Main: 2-3 gens, 6 variants — significant difficulty, multi-layer challenges
            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub + KeycardInSide — corridor → hub → gen branches, one gated by KeycardInSide
                    (0.20, () =>
                    {
                        var corridor = AddBranch(start, 1, "corridor");

                        var (hub, hubZone) = AddZone(corridor.Last(), new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // First branch gated by KeycardInSide
                        var (kcEnd, _) = BuildChallenge_KeycardInSide(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_0");
                        PlaceGenerator(kcNodes.Last());
                        AddForwardExtractStart(kcNodes.Last());

                        for (var g = 1; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(hub, $"generator_{g}");
                            AddForwardExtractStart(last);
                        }
                    }),

                    // Hub + locked terminal — hub → gen branches, one behind LockedTerminalDoor(1)
                    (0.15, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // First branch behind locked terminal
                        var (ltEnd, _) = BuildChallenge_LockedTerminalDoor(hub, 1);
                        var ltNodes = AddBranch(ltEnd, 1, "generator_0");
                        PlaceGenerator(ltNodes.Last());
                        AddForwardExtractStart(ltNodes.Last());

                        for (var g = 1; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(hub, $"generator_{g}");
                            AddForwardExtractStart(last);
                        }
                    }),

                    // Clustered + sensors — start → 1 zone [sensors] → large zone (2-3 gens clustered)
                    (0.20, () =>
                    {
                        var approach = AddBranch(start, 1, "approach");
                        AddSecuritySensors(approach.Last());

                        var (genZone, genZoneData) = AddZone(approach.Last(), new ZoneNode { Branch = "generator" });
                        genZoneData.Coverage = CoverageMinMax.Large;
                        PlaceGeneratorsInZone(genZone, numGens);
                        AddForwardExtractStart(genZone);
                    }),

                    // Hub + boss — hub → BossFight branch (gen behind boss), other gen branches
                    (0.15, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // Boss branch with gen at end
                        var (bossEnd, _) = BuildChallenge_BossFight(hub);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_0");
                        PlaceGenerator(bossNodes.Last());
                        AddForwardExtractStart(bossNodes.Last());

                        for (var g = 1; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(hub, $"generator_{g}");
                            AddForwardExtractStart(last);
                        }
                    }),

                    // Error + keycard — corridor → ErrorWithOff_KeycardInSide(1,1,1) → gen zone(s)
                    (0.15, () =>
                    {
                        var corridor = AddBranch(start, 1, "corridor");
                        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            corridor.Last(),
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var genNodes = AddBranch(end, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                        AddForwardExtractStart(genNodes.Last());
                    }),

                    // Hub + generator puzzle — hub → one branch has extra cell from side zone,
                    // gen behind generator-locked door. Rare.
                    (0.15, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // Generator puzzle branch: side zone provides extra cell
                        var (genPuzzleEnd, _) = BuildChallenge_GeneratorCellInSide(hub);
                        var gpNodes = AddBranch(genPuzzleEnd, 1, "generator_0");
                        PlaceGenerator(gpNodes.Last());
                        AddForwardExtractStart(gpNodes.Last());

                        for (var g = 1; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(hub, $"generator_{g}");
                            AddForwardExtractStart(last);
                        }
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                        AddForwardExtractStart(genNodes.Last());
                    }),
                });
                break;
            }

            // C-Extreme: 2-3 gens, 4 variants — B-Main equivalent
            case ("C", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub + KeycardInZone — hub → 2-3 gen branches, KeycardInZone on one
                    (0.30, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // First branch with keycard
                        var (kcEnd, _) = BuildChallenge_KeycardInZone(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_0");
                        PlaceGenerator(kcNodes.Last());

                        for (var g = 1; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // KeycardInSide — start → KeycardInSide → hub → gen branches
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_KeycardInSide(start);

                        var (hub, hubZone) = AddZone(end, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        for (var g = 0; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // Locked terminal — start → LockedTerminalDoor(0) → gen zone(s)
                    (0.25, () =>
                    {
                        var (end, _) = BuildChallenge_LockedTerminalDoor(start, 0);
                        var genNodes = AddBranch(end, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                    }),

                    // Sensors — hub → branches, sensors on one
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // First branch with sensors
                        var sensorNodes = AddBranch(hub, Generator.Between(1, 2), "generator_0");
                        AddSecuritySensors(sensorNodes.First());
                        PlaceGenerator(sensorNodes.Last());

                        for (var g = 1; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                    }),
                });
                break;
            }

            // C-Overload: 2-3 gens, 4 variants — D-Main equivalent
            case ("C", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error + keycard — hub → ErrorWithOff_KeycardInSide(1,1,1) → gen branch, other gen branches
                    (0.25, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        var (errEnd, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            hub,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var errNodes = AddBranch(errEnd, 1, "generator_0");
                        PlaceGenerator(errNodes.Last());

                        for (var g = 1; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // Boss + keycard — hub → BossFight branch (gen at end), KeycardInSide branch (gen at end)
                    (0.25, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // Boss branch
                        var (bossEnd, _) = BuildChallenge_BossFight(hub);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_0");
                        PlaceGenerator(bossNodes.Last());

                        // Keycard branch
                        var (kcEnd, _) = BuildChallenge_KeycardInSide(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_1");
                        PlaceGenerator(kcNodes.Last());

                        for (var g = 2; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // Sensors + apex — 1 zone [sensors] → ApexAlarm → large gen zone (clustered)
                    (0.25, () =>
                    {
                        var approach = AddBranch(start, 1, "approach");
                        AddSecuritySensors(approach.Last());

                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            approach.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);

                        var (genZone, genZoneData) = AddZone(apexEnd, new ZoneNode { Branch = "generator" });
                        genZoneData.Coverage = CoverageMinMax.Large;
                        PlaceGeneratorsInZone(genZone, numGens);
                    }),

                    // Locked terminal + sensors — hub → LockedTerminalDoor(1) branch (gen), sensors branch (gen)
                    (0.25, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        // Locked terminal branch
                        var (ltEnd, _) = BuildChallenge_LockedTerminalDoor(hub, 1);
                        var ltNodes = AddBranch(ltEnd, 1, "generator_0");
                        PlaceGenerator(ltNodes.Last());

                        // Sensors branch
                        var sensorNodes = AddBranch(hub, Generator.Between(1, 2), "generator_1");
                        AddSecuritySensors(sensorNodes.First());
                        PlaceGenerator(sensorNodes.Last());

                        for (var g = 2; g < numGens; g++)
                            BuildGeneratorBranch(hub, $"generator_{g}");
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                    }),
                });
                break;
            }
            #endregion

            #region D-tier
            // D-Main: 3-4 gens, 7 variants — hard, deep challenge chains
            // Forward extraction with double registration.
            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub + mixed challenges — corridor → hub → 3 gen branches: KeycardInSide, LockedTerminalDoor(1), sensors
                    (0.15, () =>
                    {
                        var corridor = AddBranch(start, 1, "corridor");

                        var (hub, hubZone) = AddZone(corridor.Last(), new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Branch 1: KeycardInSide
                        var (kcEnd, _) = BuildChallenge_KeycardInSide(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_0");
                        PlaceGenerator(kcNodes.Last());
                        AddForwardExtractStart(kcNodes.Last());

                        // Branch 2: LockedTerminalDoor
                        var (ltEnd, _) = BuildChallenge_LockedTerminalDoor(hub, 1);
                        var ltNodes = AddBranch(ltEnd, 1, "generator_1");
                        PlaceGenerator(ltNodes.Last());

                        // Branch 3: sensors
                        var sensorNodes = AddBranch(hub, Generator.Between(1, 2), "generator_2");
                        AddSecuritySensors(sensorNodes.First());
                        PlaceGenerator(sensorNodes.Last());

                        var overflowBase = sensorNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");

                        AddForwardExtractStart(hub, chance: 0.3);
                    }),

                    // Cluster + boss — 1 zone → large zone (2-3 gens clustered) + side branch (1 gen behind BossFight)
                    (0.15, () =>
                    {
                        var approach = AddBranch(start, 1, "approach");

                        // Clustered gens
                        var clusteredCount = Math.Min(numGens - 1, 3);
                        var (genZone, genZoneData) = AddZone(approach.Last(), new ZoneNode { Branch = "generator_cluster", MaxConnections = 3 });
                        genZoneData.Coverage = CoverageMinMax.Large;
                        genZoneData.GenHubGeomorph(Complex);
                        PlaceGeneratorsInZone(genZone, clusteredCount);
                        AddForwardExtractStart(genZone);

                        // Boss side branch
                        var (bossEnd, _) = BuildChallenge_BossFight(genZone);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_boss");
                        PlaceGenerator(bossNodes.Last());
                        AddForwardExtractStart(bossNodes.Last());

                        AddForwardExtractStart(approach.Last(), chance: 0.3);
                    }),

                    // Error + keycard + sensors — hub → ErrorWithOff_KeycardInSide(1,1,1) branch (gen),
                    // 2 more gen branches with KeycardInZone, sensors
                    (0.15, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Error branch
                        var (errEnd, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            hub,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var errNodes = AddBranch(errEnd, 1, "generator_0");
                        PlaceGenerator(errNodes.Last());
                        AddForwardExtractStart(errNodes.Last());

                        // KeycardInZone branch
                        var (kcEnd, _) = BuildChallenge_KeycardInZone(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_1");
                        PlaceGenerator(kcNodes.Last());

                        // Sensors branch
                        var sensorNodes = AddBranch(hub, Generator.Between(1, 2), "generator_2");
                        AddSecuritySensors(sensorNodes.First());
                        PlaceGenerator(sensorNodes.Last());

                        var overflowBase = sensorNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");

                        AddForwardExtractStart(hub, chance: 0.3);
                    }),

                    // Sensors + keycard + double hub — 1 zone [sensors] → KeycardInSide → 1 zone → hub2 → 2 gen branches
                    (0.15, () =>
                    {
                        var approach = AddBranch(start, 1, "approach");
                        AddSecuritySensors(approach.Last());

                        var (kcEnd, _) = BuildChallenge_KeycardInSide(approach.Last());
                        var mid = AddBranch(kcEnd, 1, "mid");

                        var (hub, hubZone) = AddZone(mid.Last(), new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        ZoneNode overflowBase = hub;
                        for (var g = 0; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                            if (g == 0)
                                AddForwardExtractStart(last);
                            overflowBase = last;
                        }

                        AddForwardExtractStart(hub, chance: 0.3);
                    }),

                    // Apex + cluster — corridor → ApexAlarm(Hard) → large zone (3-4 gens all clustered)
                    (0.15, () =>
                    {
                        var corridor = AddBranch(start, 1, "corridor");

                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            corridor.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);

                        var (genZone, genZoneData) = AddZone(apexEnd, new ZoneNode { Branch = "generator" });
                        genZoneData.Coverage = CoverageMinMax.Large;
                        PlaceGeneratorsInZone(genZone, numGens);
                        AddForwardExtractStart(genZone);

                        AddForwardExtractStart(corridor.Last(), chance: 0.3);
                    }),

                    // Double hub — hub1 → 2 gen branches; hub2 (off branch end) → 1-2 gen branches
                    (0.15, () =>
                    {
                        var (hub1, hub1Zone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hub1Zone.GenHubGeomorph(Complex);

                        // First 2 gens in hub1 branches
                        var last0 = BuildGeneratorBranch(hub1, "generator_0");
                        var last1 = BuildGeneratorBranch(hub1, "generator_1");
                        AddForwardExtractStart(last0);

                        // Hub2 off the end of branch 1
                        var (hub2, hub2Zone) = AddZone(last1, new ZoneNode { MaxConnections = 3 });
                        hub2Zone.GenHubGeomorph(Complex);

                        for (var g = 2; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(hub2, $"generator_{g}");
                            AddForwardExtractStart(last);
                        }

                        AddForwardExtractStart(hub1, chance: 0.3);
                    }),

                    // Hub + generator puzzle — hub → one branch with extra cell from side, gen behind gen-locked door
                    (0.10, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Generator puzzle branch
                        var (genPuzzleEnd, _) = BuildChallenge_GeneratorCellInSide(hub);
                        var gpNodes = AddBranch(genPuzzleEnd, 1, "generator_0");
                        PlaceGenerator(gpNodes.Last());
                        AddForwardExtractStart(gpNodes.Last());

                        var overflowBase = gpNodes.Last();
                        for (var g = 1; g < numGens; g++)
                        {
                            var last = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                            AddForwardExtractStart(last);
                            overflowBase = last;
                        }

                        AddForwardExtractStart(hub, chance: 0.3);
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                        AddForwardExtractStart(genNodes.Last());
                    }),
                });
                break;
            }

            // D-Extreme: 3-4 gens, 5 variants — C-Main equivalent
            case ("D", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub + KeycardInSide — hub → 3 gen branches, KeycardInSide on one
                    (0.25, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // First branch gated by KeycardInSide
                        var (kcEnd, _) = BuildChallenge_KeycardInSide(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_0");
                        PlaceGenerator(kcNodes.Last());

                        var overflowBase = kcNodes.Last();
                        for (var g = 1; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                    }),

                    // Hub + boss — hub → gen branches, one behind BossFight
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Boss branch
                        var (bossEnd, _) = BuildChallenge_BossFight(hub);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_0");
                        PlaceGenerator(bossNodes.Last());

                        var overflowBase = bossNodes.Last();
                        for (var g = 1; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                    }),

                    // Locked terminal + cluster — LockedTerminalDoor(1) → large zone (gens clustered)
                    (0.20, () =>
                    {
                        var (ltEnd, _) = BuildChallenge_LockedTerminalDoor(start, 1);

                        var (genZone, genZoneData) = AddZone(ltEnd, new ZoneNode { Branch = "generator" });
                        genZoneData.Coverage = CoverageMinMax.Large;
                        PlaceGeneratorsInZone(genZone, numGens);
                    }),

                    // Hub + mixed — hub → gen branches, one with sensors, one with LockedTerminalDoor(0)
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Sensors branch
                        var sensorNodes = AddBranch(hub, Generator.Between(1, 2), "generator_0");
                        AddSecuritySensors(sensorNodes.First());
                        PlaceGenerator(sensorNodes.Last());

                        // LockedTerminalDoor branch
                        var (ltEnd, _) = BuildChallenge_LockedTerminalDoor(hub, 0);
                        var ltNodes = AddBranch(ltEnd, 1, "generator_1");
                        PlaceGenerator(ltNodes.Last());

                        var overflowBase = ltNodes.Last();
                        for (var g = 2; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                    }),

                    // Error + keycard — hub → ErrorWithOff_KeycardInSide(1,1,1) → gen zone, other branches
                    (0.15, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        var (errEnd, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            hub,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var errNodes = AddBranch(errEnd, 1, "generator_0");
                        PlaceGenerator(errNodes.Last());

                        var overflowBase = errNodes.Last();
                        for (var g = 1; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                    }),
                });
                break;
            }

            // D-Overload: 3-4 gens, 5 variants — E-Main equivalent
            case ("D", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex + cluster — ApexAlarm(Hard) → large zone (all gens clustered)
                    (0.20, () =>
                    {
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            start,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);

                        var (genZone, genZoneData) = AddZone(apexEnd, new ZoneNode { Branch = "generator" });
                        genZoneData.Coverage = CoverageMinMax.Large;
                        PlaceGeneratorsInZone(genZone, numGens);
                    }),

                    // Error + boss + keycard — hub → ErrorWithOff_KeycardInSide(2,1,1) branch (gen),
                    // BossFight branch (gen), keycard branch (gen)
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Error branch
                        var (errEnd, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            hub,
                            errorZones: 2,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var errNodes = AddBranch(errEnd, 1, "generator_0");
                        PlaceGenerator(errNodes.Last());

                        // Boss branch
                        var (bossEnd, _) = BuildChallenge_BossFight(hub);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_1");
                        PlaceGenerator(bossNodes.Last());

                        // Keycard branch
                        var (kcEnd, _) = BuildChallenge_KeycardInSide(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_2");
                        PlaceGenerator(kcNodes.Last());

                        var overflowBase = kcNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");
                    }),

                    // Apex + boss + sensors — hub → ApexAlarm(Hard) + gen, BossFight + gen, sensors + gen
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Apex branch
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            hub,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var apexNodes = AddBranch(apexEnd, 1, "generator_0");
                        PlaceGenerator(apexNodes.Last());

                        // Boss branch
                        var (bossEnd, _) = BuildChallenge_BossFight(hub);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_1");
                        PlaceGenerator(bossNodes.Last());

                        // Sensors branch
                        var sensorNodes = AddBranch(hub, Generator.Between(1, 2), "generator_2");
                        AddSecuritySensors(sensorNodes.First());
                        PlaceGenerator(sensorNodes.Last());

                        var overflowBase = sensorNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");
                    }),

                    // Boss + sensors + hub — BossFight → 1 zone [sensors] → hub → 2 gen branches + 1 clustered
                    (0.20, () =>
                    {
                        var (bossEnd, _) = BuildChallenge_BossFight(start);
                        var sensorNodes = AddBranch(bossEnd, 1, "approach");
                        AddSecuritySensors(sensorNodes.Last());

                        var (hub, hubZone) = AddZone(sensorNodes.Last(), new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        ZoneNode overflowBase = hub;
                        for (var g = 0; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                    }),

                    // Locked terminal password + apex — hub → LockedTerminalPasswordInSide branch (gen),
                    // ApexAlarm(Hard) branch (gen), other branches
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Locked terminal password branch
                        var (ltEnd, _) = BuildChallenge_LockedTerminalPasswordInSide(hub);
                        var ltNodes = AddBranch(ltEnd, 1, "generator_0");
                        PlaceGenerator(ltNodes.Last());

                        // Apex branch
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            hub,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var apexNodes = AddBranch(apexEnd, 1, "generator_1");
                        PlaceGenerator(apexNodes.Last());

                        var overflowBase = apexNodes.Last();
                        for (var g = 2; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                    }),
                });
                break;
            }
            #endregion

            #region E-tier
            // E-Main: 3-5 gens, 7 variants — maximum difficulty
            // Forward extraction with double registration.
            case ("E", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error + boss + apex — hub → ErrorWithOff_KeycardInSide(2,1,1) + gen,
                    // BossFight + gen, ApexAlarm(VeryHard) + gen, [+more if needed]
                    (0.15, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Error branch
                        var (errEnd, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            hub,
                            errorZones: 2,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var errNodes = AddBranch(errEnd, 1, "generator_0");
                        PlaceGenerator(errNodes.Last());
                        AddForwardExtractStart(errNodes.Last());

                        // Boss branch
                        var (bossEnd, _) = BuildChallenge_BossFight(hub);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_1");
                        PlaceGenerator(bossNodes.Last());

                        // Apex branch
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            hub,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var apexNodes = AddBranch(apexEnd, 1, "generator_2");
                        PlaceGenerator(apexNodes.Last());

                        var overflowBase = apexNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");

                        AddForwardExtractStart(hub, chance: 0.3);
                    }),

                    // Error + apex + cluster — corridor → ErrorWithOff_KeycardInSide(2,1,1)
                    // → ApexAlarm(VeryHard) → large zone (3-5 gens all clustered)
                    (0.15, () =>
                    {
                        var corridor = AddBranch(start, 1, "corridor");

                        var (errEnd, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            corridor.Last(),
                            errorZones: 2,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);

                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            errEnd,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);

                        var (genZone, genZoneData) = AddZone(apexEnd, new ZoneNode { Branch = "generator" });
                        genZoneData.Coverage = CoverageMinMax.Large;
                        PlaceGeneratorsInZone(genZone, numGens);
                        AddForwardExtractStart(genZone);

                        AddForwardExtractStart(corridor.Last(), chance: 0.3);
                    }),

                    // Double hub + layered — hub1 → boss branch (gen), keycard+sensors branch (gen);
                    // hub2 off first → apex branch (gen), error branch (gen)
                    (0.15, () =>
                    {
                        var (hub1, hub1Zone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                        hub1Zone.GenHubGeomorph(Complex);

                        // Boss branch off hub1
                        var (bossEnd, _) = BuildChallenge_BossFight(hub1);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_0");
                        PlaceGenerator(bossNodes.Last());
                        AddForwardExtractStart(bossNodes.Last());

                        // Keycard + sensors branch off hub1, hub2 at end
                        var (kcEnd, _) = BuildChallenge_KeycardInSide(hub1);
                        var sensorNodes = AddBranch(kcEnd, 1, "sensor_approach");
                        AddSecuritySensors(sensorNodes.Last());
                        PlaceGenerator(sensorNodes.Last());

                        // Hub2 off the sensor approach end
                        var (hub2, hub2Zone) = AddZone(sensorNodes.Last(), new ZoneNode { MaxConnections = 3 });
                        hub2Zone.GenHubGeomorph(Complex);

                        // Apex branch off hub2
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            hub2,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var apexNodes = AddBranch(apexEnd, 1, "generator_2");
                        PlaceGenerator(apexNodes.Last());

                        // Remaining gens off hub2
                        var overflowBase = apexNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(g < 3 ? hub2 : overflowBase, $"generator_{g}");

                        AddForwardExtractStart(hub1, chance: 0.3);
                    }),

                    // Generator puzzle + boss + apex — hub → one branch with extra cell + gen-locked door,
                    // other branches with BossFight, ApexAlarm(VeryHard). Rare.
                    (0.15, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Generator puzzle branch
                        var (genPuzzleEnd, _) = BuildChallenge_GeneratorCellInSide(hub);
                        var gpNodes = AddBranch(genPuzzleEnd, 1, "generator_0");
                        PlaceGenerator(gpNodes.Last());
                        AddForwardExtractStart(gpNodes.Last());

                        // Boss branch
                        var (bossEnd, _) = BuildChallenge_BossFight(hub);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_1");
                        PlaceGenerator(bossNodes.Last());

                        // Apex branch
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            hub,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var apexNodes = AddBranch(apexEnd, 1, "generator_2");
                        PlaceGenerator(apexNodes.Last());

                        var overflowBase = apexNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");

                        AddForwardExtractStart(hub, chance: 0.3);
                    }),

                    // Boss + sensors + apex + cluster — BossFight → 1 zone [sensors]
                    // → ApexAlarm(VeryHard) → large zone (all gens clustered)
                    (0.15, () =>
                    {
                        var (bossEnd, _) = BuildChallenge_BossFight(start);
                        var sensorNodes = AddBranch(bossEnd, 1, "sensor_approach");
                        AddSecuritySensors(sensorNodes.Last());

                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            sensorNodes.Last(),
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);

                        var (genZone, genZoneData) = AddZone(apexEnd, new ZoneNode { Branch = "generator" });
                        genZoneData.Coverage = CoverageMinMax.Large;
                        PlaceGeneratorsInZone(genZone, numGens);
                        AddForwardExtractStart(genZone);

                        AddForwardExtractStart(bossEnd, chance: 0.3);
                    }),

                    // Error carry + boss + sensors — hub → ErrorWithOff_GeneratorCellCarry(2,1) branch (gen),
                    // boss branches (gen), sensors branches (gen)
                    (0.15, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Error carry branch
                        var (errEnd, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            hub,
                            errorZones: 2,
                            terminalTurnoffZones: 1);
                        var errNodes = AddBranch(errEnd, 1, "generator_0");
                        PlaceGenerator(errNodes.Last());
                        AddForwardExtractStart(errNodes.Last());

                        // Boss branch
                        var (bossEnd, _) = BuildChallenge_BossFight(hub);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_1");
                        PlaceGenerator(bossNodes.Last());

                        // Sensors branch
                        var sensorNodes = AddBranch(hub, Generator.Between(1, 2), "generator_2");
                        AddSecuritySensors(sensorNodes.First());
                        PlaceGenerator(sensorNodes.Last());

                        var overflowBase = sensorNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");

                        AddForwardExtractStart(hub, chance: 0.3);
                    }),

                    // Triple distinct — hub → KeycardInSide+BossFight chain → gen,
                    // ApexAlarm(VeryHard) → gen, LockedTerminalDoor(1)+sensors → gen
                    (0.10, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Keycard + boss chain
                        var (kcEnd, _) = BuildChallenge_KeycardInSide(hub);
                        var (bossEnd, _) = BuildChallenge_BossFight(kcEnd);
                        var chainNodes = AddBranch(bossEnd, 1, "generator_0");
                        PlaceGenerator(chainNodes.Last());
                        AddForwardExtractStart(chainNodes.Last());

                        // Apex branch
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            hub,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var apexNodes = AddBranch(apexEnd, 1, "generator_1");
                        PlaceGenerator(apexNodes.Last());

                        // Locked terminal + sensors branch
                        var (ltEnd, _) = BuildChallenge_LockedTerminalDoor(hub, 1);
                        var ltSensorNodes = AddBranch(ltEnd, Generator.Between(1, 2), "generator_2");
                        AddSecuritySensors(ltSensorNodes.First());
                        PlaceGenerator(ltSensorNodes.Last());

                        var overflowBase = ltSensorNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");

                        AddForwardExtractStart(hub, chance: 0.3);
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                        AddForwardExtractStart(genNodes.Last());
                    }),
                });
                break;
            }

            // E-Extreme: 3-5 gens, 5 variants — D-Main equivalent
            case ("E", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Hub + mixed — hub → gen branches: KeycardInSide, BossFight, LockedTerminalDoor(1)
                    (0.25, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // KeycardInSide branch
                        var (kcEnd, _) = BuildChallenge_KeycardInSide(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_0");
                        PlaceGenerator(kcNodes.Last());

                        // BossFight branch
                        var (bossEnd, _) = BuildChallenge_BossFight(hub);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_1");
                        PlaceGenerator(bossNodes.Last());

                        // LockedTerminalDoor branch
                        var (ltEnd, _) = BuildChallenge_LockedTerminalDoor(hub, 1);
                        var ltNodes = AddBranch(ltEnd, 1, "generator_2");
                        PlaceGenerator(ltNodes.Last());

                        var overflowBase = ltNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");
                    }),

                    // Boss + cluster — BossFight → large zone (gens clustered)
                    (0.20, () =>
                    {
                        var (bossEnd, _) = BuildChallenge_BossFight(start);

                        var (genZone, genZoneData) = AddZone(bossEnd, new ZoneNode { Branch = "generator" });
                        genZoneData.Coverage = CoverageMinMax.Large;
                        PlaceGeneratorsInZone(genZone, numGens);
                    }),

                    // Error + sensors — hub → ErrorWithOff_KeycardInSide(1,1,1) branch (gen),
                    // sensors branch (gen), normal branch (gen)
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Error branch
                        var (errEnd, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            hub,
                            errorZones: 1,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var errNodes = AddBranch(errEnd, 1, "generator_0");
                        PlaceGenerator(errNodes.Last());

                        // Sensors branch
                        var sensorNodes = AddBranch(hub, Generator.Between(1, 2), "generator_1");
                        AddSecuritySensors(sensorNodes.First());
                        PlaceGenerator(sensorNodes.Last());

                        // Normal branch
                        var overflowBase = sensorNodes.Last();
                        for (var g = 2; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                    }),

                    // Apex + keycard + terminal — hub → ApexAlarm(Hard) branch (gen),
                    // KeycardInSide branch (gen), LockedTerminalDoor(0) branch (gen)
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Apex branch
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            hub,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_Hard);
                        var apexNodes = AddBranch(apexEnd, 1, "generator_0");
                        PlaceGenerator(apexNodes.Last());

                        // KeycardInSide branch
                        var (kcEnd, _) = BuildChallenge_KeycardInSide(hub);
                        var kcNodes = AddBranch(kcEnd, 1, "generator_1");
                        PlaceGenerator(kcNodes.Last());

                        // LockedTerminalDoor branch
                        var (ltEnd, _) = BuildChallenge_LockedTerminalDoor(hub, 0);
                        var ltNodes = AddBranch(ltEnd, 1, "generator_2");
                        PlaceGenerator(ltNodes.Last());

                        var overflowBase = ltNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");
                    }),

                    // Sensors + keycard + hub — 1 zone [sensors] → KeycardInSide → hub → 2-3 gen branches
                    (0.15, () =>
                    {
                        var approach = AddBranch(start, 1, "approach");
                        AddSecuritySensors(approach.Last());

                        var (kcEnd, _) = BuildChallenge_KeycardInSide(approach.Last());

                        var (hub, hubZone) = AddZone(kcEnd, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        ZoneNode overflowBase = hub;
                        for (var g = 0; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                    }),
                });
                break;
            }

            // E-Overload: 3-5 gens, 5 variants — maximum difficulty everywhere
            case ("E", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Apex + boss + cluster — ApexAlarm(VeryHard) → BossFight → large zone (all gens clustered)
                    (0.20, () =>
                    {
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            start,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);

                        var (bossEnd, _) = BuildChallenge_BossFight(apexEnd);

                        var (genZone, genZoneData) = AddZone(bossEnd, new ZoneNode { Branch = "generator" });
                        genZoneData.Coverage = CoverageMinMax.Large;
                        PlaceGeneratorsInZone(genZone, numGens);
                    }),

                    // Error + apex + boss — hub → ErrorWithOff_KeycardInSide(2,1,1) branch (gen),
                    // ApexAlarm(VeryHard) branch (gen), BossFight branch (gen)
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Error branch
                        var (errEnd, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            hub,
                            errorZones: 2,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);
                        var errNodes = AddBranch(errEnd, 1, "generator_0");
                        PlaceGenerator(errNodes.Last());

                        // Apex branch
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            hub,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var apexNodes = AddBranch(apexEnd, 1, "generator_1");
                        PlaceGenerator(apexNodes.Last());

                        // Boss branch
                        var (bossEnd, _) = BuildChallenge_BossFight(hub);
                        var bossNodes = AddBranch(bossEnd, 1, "generator_2");
                        PlaceGenerator(bossNodes.Last());

                        var overflowBase = bossNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");
                    }),

                    // Boss+sensors + error-carry + apex — hub → BossFight+sensors branch (gen),
                    // ErrorWithOff_GeneratorCellCarry(2,1) branch (gen), ApexAlarm(VeryHard) branch (gen)
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Boss + sensors chain
                        var (bossEnd, _) = BuildChallenge_BossFight(hub);
                        var sensorNodes = AddBranch(bossEnd, 1, "generator_0");
                        AddSecuritySensors(sensorNodes.Last());
                        PlaceGenerator(sensorNodes.Last());

                        // Error carry branch
                        var (errEnd, _) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            hub,
                            errorZones: 2,
                            terminalTurnoffZones: 1);
                        var errNodes = AddBranch(errEnd, 1, "generator_1");
                        PlaceGenerator(errNodes.Last());

                        // Apex branch
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            hub,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var apexNodes = AddBranch(apexEnd, 1, "generator_2");
                        PlaceGenerator(apexNodes.Last());

                        var overflowBase = apexNodes.Last();
                        for (var g = 3; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(overflowBase, $"generator_{g}");
                    }),

                    // Error + apex gate hub — ErrorWithOff_KeycardInSide(2,1,1) → ApexAlarm(VeryHard) → hub → gen branches
                    (0.20, () =>
                    {
                        var (errEnd, _) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            errorZones: 2,
                            sideKeycardZones: 1,
                            terminalTurnoffZones: 1);

                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            errEnd,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);

                        var (hub, hubZone) = AddZone(apexEnd, new ZoneNode { MaxConnections = 3 });
                        hubZone.GenHubGeomorph(Complex);

                        ZoneNode overflowBase = hub;
                        for (var g = 0; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                    }),

                    // Generator puzzle + boss + apex — hub → one branch with extra cell + gen-locked door
                    // + BossFight chain, other branches with ApexAlarm(VeryHard)
                    (0.20, () =>
                    {
                        var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 4 });
                        hubZone.GenHubGeomorph(Complex);

                        // Generator puzzle + boss chain
                        var (genPuzzleEnd, _) = BuildChallenge_GeneratorCellInSide(hub);
                        var (bossEnd, _) = BuildChallenge_BossFight(genPuzzleEnd);
                        var chainNodes = AddBranch(bossEnd, 1, "generator_0");
                        PlaceGenerator(chainNodes.Last());

                        // Apex branches
                        var (apexEnd, _) = BuildChallenge_ApexAlarm(
                            hub,
                            WavePopulation.Baseline_Hybrids,
                            WaveSettings.Baseline_VeryHard);
                        var apexNodes = AddBranch(apexEnd, 1, "generator_1");
                        PlaceGenerator(apexNodes.Last());

                        var overflowBase = apexNodes.Last();
                        for (var g = 2; g < numGens; g++)
                            overflowBase = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                    }),

                    // Travel scan gate
                    (0.15, () =>
                    {
                        var (travelEnd, _) = AddTravelScanAlarm(start);
                        var genNodes = AddBranch(travelEnd, 1, "generator");
                        PlaceGeneratorsInZone(genNodes.Last(), numGens);
                    }),
                });
                break;
            }
            #endregion

            // Default fallback: simple hub + branches
            default:
            {
                var (hub, hubZone) = AddZone(start, new ZoneNode { MaxConnections = 3 });
                hubZone.GenHubGeomorph(Complex);

                ZoneNode overflowBase = hub;
                for (var g = 0; g < numGens; g++)
                {
                    var last = BuildGeneratorBranch(g < 3 ? hub : overflowBase, $"generator_{g}");
                    AddForwardExtractStart(last);
                    overflowBase = last;
                }
                break;
            }
        }
    }
}
