using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// Ideas for layout
    ///
    /// * Straight go and get from zones (current implementation)
    /// * Go get with 1 locked zone
    /// * Go get with 1 -> 2 locked zones
    /// * Go get with one of two zones to go through (random from game pick)
    /// * Go get through class 10 alarm
    /// * Go get through error alarm door
    /// * Go get through error alarm -> locked door
    /// * Go get through error alarm -> locked -> locked
    ///
    /// TODO: Add resources for the error alarm zones. They may be quite hard
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    /// <exception cref="Exception"></exception>
    public void BuildLayout_HsuFindSample(BuildDirector director, WardenObjective objective, ZoneNode? startish)
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
            // Main has a few options to build
            case ("A", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Build generator locked puzzle
                    (0.4, () =>
                    {
                        planner.UpdateNode(start with { MaxConnections = 3 });

                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });
                        var cell = BuildBranch(start, 2, "power_cell");

                        // Lock the first zone
                        AddGeneratorPuzzle(locked, cell);

                        startZone.GenTGeomorph(level.Complex);
                    }),

                    // Build keycard locked puzzle
                    (0.4, () =>
                    {
                        // Possibly add an extra zone to go throuh
                        if (Generator.Flip(0.7) || true)
                            (start, startZone) = AddZone(start, new ZoneNode { Branch = "primary" });

                        // Update number of connections for hub zone
                        planner.UpdateNode(start with { MaxConnections = 3 });
                        startZone.GenHubGeomorph(level.Complex);

                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });
                        var keycard = BuildBranch(start, 1, "keycard");

                        // Lock the first zone
                        AddKeycardPuzzle(locked, keycard);
                    }),

                    // Just a straight shot to the HSU
                    (0.2, () =>
                    {
                        var last = BuildBranch(start, 2);
                        planner.UpdateNode(last with { Branch = "hsu_sample" });
                    })
                });

                break;
            }

            // Extreme/Overload are the same here. Pretty simple and short
            case ("A", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Simple keycard puzzle: two zones. First one is the locked zone which also
                    // has the key. Second has the HSU
                    (1.0, () =>
                    {
                        // Set the key zone to be big
                        startZone.Coverage = CoverageMinMax.Medium;

                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });

                        // Lock the HSU zone
                        AddKeycardPuzzle(locked, start);
                    }),

                    // Just a straight shot to the HSU, variable number of zones. In some cases just one zone
                    (2.0, () =>
                    {
                        var last = BuildBranch(start, Generator.Between(0, 1));
                        planner.UpdateNode(last with { Branch = "hsu_sample" });
                    })
                });
                break;
            }
            #endregion

            #region Tier: B
            // We want: Generator, keycard, king of the hill alarm
            case ("B", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Basic error alarm run with turnoff zone deep at the end
                    (0.05, () =>
                    {
                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "primary" });
                        var end = BuildBranch(locked, 1);
                        var (hsu, _) = AddZone(end, new ZoneNode { Branch = "hsu_sample" });

                        // Any side objectives and we allow disabling the alarm
                        var allowTurnoff = level.Settings.Bulkheads != Bulkhead.Main;

                        ZoneNode? terminal = allowTurnoff ? BuildBranch(hsu, 1, "error_turnoff") : null;

                        // Optionally add shadows if the level has shadows in it
                        var population = level.Settings.HasShadows()
                            ? WavePopulation.OnlyShadows
                            : WavePopulation.Baseline;

                        // Lock the first zone
                        AddErrorAlarm(locked, terminal, WaveSettings.Error_Easy, population);

                        startZone.Coverage = CoverageMinMax.Tiny;
                    }),

                    // Big Apex alarm to enter
                    (0.20, () =>
                    {
                        var (lockedApex, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });

                        // Add some extra resources
                        startZone.HealthPacks += 4;
                        startZone.ToolPacks += 4;
                        startZone.AmmoPacks += 5;

                        // Configure the wave population
                        var population = WavePopulation.Baseline;

                        if (Generator.Flip(0.2))
                            population = WavePopulation.Baseline_Hybrids;

                        if (level.Settings.HasShadows())
                            population = Generator.Flip(0.4) ? WavePopulation.OnlyShadows : WavePopulation
                                .Baseline_Shadows;

                        // Add the apex alarm
                        AddApexAlarm(lockedApex, population);
                    }),

                    // Generator locked forward progression
                    (0.35, () =>
                    {
                        planner.UpdateNode(start with { MaxConnections = 3 });

                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });
                        var cell = BuildBranch(start, 2, "power_cell");

                        // Lock the first zone
                        AddGeneratorPuzzle(locked, cell);

                        startZone.GenHubGeomorph(level.Complex);
                    }),

                    // Build keycard locked puzzle
                    (0.30, () =>
                    {
                        // Possibly add an extra zone to go throuh
                        start = BuildBranch(start, Generator.Between(0, 2));
                        startZone = planner.GetZone(start)!;

                        // Update number of connections for hub zone
                        planner.UpdateNode(start with { MaxConnections = 3 });
                        startZone.GenHubGeomorph(level.Complex);

                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });
                        var keycard = BuildBranch(start, 1, "keycard");

                        // Lock the first zone
                        AddKeycardPuzzle(locked, keycard);
                    }),

                    // Just a straight shot to the HSU
                    (0.10, () =>
                    {
                        var last = BuildBranch(start, Generator.Between(2, 3));
                        planner.UpdateNode(last with { Branch = "hsu_sample" });
                    })
                });
                break;
            }

            // No difference really here between Extreme/Overload
            case ("B", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Generator locked forward progression
                    (0.35, () =>
                    {
                        planner.UpdateNode(start with { MaxConnections = 3 });

                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });
                        var cell = BuildBranch(start, 2, "power_cell");

                        // Lock the first zone
                        AddGeneratorPuzzle(locked, cell);

                        startZone.GenHubGeomorph(level.Complex);
                    }),

                    // Build keycard locked puzzle
                    (0.30, () =>
                    {
                        // Possibly add an extra zone to go throuh
                        start = BuildBranch(start, Generator.Between(0, 2));
                        startZone = planner.GetZone(start)!;

                        // Update number of connections for hub zone
                        planner.UpdateNode(start with { MaxConnections = 3 });
                        startZone.GenHubGeomorph(level.Complex);

                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });
                        var keycard = BuildBranch(start, 1, "keycard");

                        // Lock the first zone
                        AddKeycardPuzzle(locked, keycard);
                    }),

                    // Just a straight shot to the HSU
                    (0.10, () =>
                    {
                        var last = BuildBranch(start, Generator.Between(2, 3));
                        planner.UpdateNode(last with { Branch = "hsu_sample" });
                    })
                });
                break;
            }
            #endregion

            #region Tier: C
            // Main only
            case ("C", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Basic error alarm run with turnoff zone deep at the end
                    (0.20, () =>
                    {
                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "primary" });
                        var end = BuildBranch(locked, 1);
                        var (hsu, _) = AddZone(end, new ZoneNode { Branch = "hsu_sample" });

                        // Any side objectives and we allow disabling the alarm
                        var allowTurnoff = level.Settings.Bulkheads != Bulkhead.Main;

                        ZoneNode? terminal = allowTurnoff
                            ? BuildBranch(hsu, Generator.Between(1, 2), "error_turnoff")
                            : null;

                        // Optionally add shadows if the level has shadows in it
                        var population = level.Settings.HasShadows()
                            ? WavePopulation.OnlyShadows
                            : WavePopulation.Baseline;

                        // Lock the first zone
                        AddErrorAlarm(locked, terminal, WaveSettings.Error_Normal, population);

                        startZone.Coverage = CoverageMinMax.Tiny;
                    }),

                    // Big Apex alarm to enter
                    (0.15, () =>
                    {
                        var (lockedApex, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });

                        // Add some extra resources
                        startZone.HealthPacks += 5;
                        startZone.ToolPacks += 4;
                        startZone.AmmoPacks += 5;

                        // Configure the wave population
                        var population = WavePopulation.Baseline_Hybrids;

                        if (Generator.Flip(0.2))
                            population = WavePopulation.Baseline_Hybrids;

                        if (level.Settings.HasShadows())
                            population = Generator.Flip(0.4) ? WavePopulation.OnlyShadows : WavePopulation
                                .Baseline_Shadows;

                        // Chargers first, then flyers
                        if (level.Settings.HasChargers())
                            population = WavePopulation.Baseline_Chargers;
                        else if (level.Settings.HasFlyers())
                            population = WavePopulation.Baseline_Flyers;

                        // Add the apex alarm
                        AddApexAlarm(lockedApex, population);
                    }),

                    // 1 generator lock
                    (0.15, () =>
                    {
                        var (prelude, preludeZone) = AddZone(start, new ZoneNode
                        {
                            Branch = "primary",
                            MaxConnections = 3
                        });
                        preludeZone.GenTGeomorph(level.Complex);

                        var (locked, _) = AddZone(prelude, new ZoneNode { Branch = "hsu_sample" });
                        var cell = BuildBranch(prelude, 2, "power_cell");

                        // Lock the first zone
                        AddGeneratorPuzzle(locked, cell);
                    }),

                    // Build 1 keycard lock
                    (0.05, () =>
                    {
                        // Possibly add an extra zone to go throuh
                        start = BuildBranch(start, Generator.Between(1, 2));
                        startZone = planner.GetZone(start)!;

                        // Update number of connections for hub zone
                        planner.UpdateNode(start with { MaxConnections = 3 });
                        startZone.GenHubGeomorph(level.Complex);

                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });
                        var keycard = BuildBranch(start, Generator.Between(1, 2), "keycard");

                        // Lock the first zone
                        AddKeycardPuzzle(locked, keycard);
                    }),

                    // 2 keycards
                    (0.25, () =>
                    {
                        // Update number of connections for hub zone
                        planner.UpdateNode(start with { MaxConnections = 3 });
                        startZone.GenHubGeomorph(level.Complex);

                        // Second area, with also a locked zone
                        var (node2, zone2) = AddZone(start, new ZoneNode { Branch = "primary", MaxConnections = 3 });
                        zone2.GenHubGeomorph(level.Complex);

                        var keycard1 = BuildBranch(start, 1, "keycard_1");

                        // Lock the first zone
                        AddKeycardPuzzle(node2, keycard1);

                        // Build the second keycard zone and
                        var keycard2 = BuildBranch(node2, 1, "keycard_2");
                        var (hsu, hsuZone) = AddZone(node2, new ZoneNode { Branch = "hsu_sample" });

                        hsuZone.Coverage = CoverageMinMax.Medium;

                        // Lock the first zone
                        AddKeycardPuzzle(hsu, keycard2);
                    }),

                    // 1 error alarm and keycard. Zone layout is as follows:
                    //   start -> node2 -> end -> hsu -> error
                    //                                -> keycard
                    (0.15, () =>
                    {
                        // TODO: This didn't seem to work at all in C2
                        var (hsu, hsuZone) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            2,
                            1,
                            1
                        );

                        planner.UpdateNode(hsu with { Branch = "hsu_sample" });

                        hsuZone.Coverage = CoverageMinMax.Medium;
                    }),

                    // Straight to HSU... with error alarm active
                    // No turning it off
                    (0.05, () =>
                    {
                        objective.WavesOnElevatorLand.Add(GenericWave.ErrorAlarm_Normal);
                        level.MarkAsErrorAlarm();

                        var last = BuildBranch(start, Generator.Between(2, 3));
                        planner.UpdateNode(last with { Branch = "hsu_sample" });
                    })
                });
                break;
            }

            // Shorter and a bit easier secondary objective
            case ("C", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // 1 generator lock
                    (0.35, () =>
                    {
                        var (prelude, preludeZone) = AddZone(start, new ZoneNode
                        {
                            Branch = "primary",
                            MaxConnections = 3
                        });
                        preludeZone.GenTGeomorph(level.Complex);

                        var (locked, _) = AddZone(prelude, new ZoneNode { Branch = "hsu_sample" });
                        var cell = BuildBranch(prelude, 2, "power_cell");

                        // Lock the first zone
                        AddGeneratorPuzzle(locked, cell);
                    }),

                    // Build 1 keycard lock
                    (0.30, () =>
                    {
                        // Possibly add an extra zone to go throuh
                        start = BuildBranch(start, Generator.Between(1, 2));
                        startZone = planner.GetZone(start)!;

                        // Update number of connections for hub zone
                        planner.UpdateNode(start with { MaxConnections = 3 });
                        startZone.GenHubGeomorph(level.Complex);

                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });
                        var keycard = BuildBranch(start, Generator.Between(1, 2), "keycard");

                        // Lock the first zone
                        AddKeycardPuzzle(locked, keycard);
                    }),

                    // Straight to HSU
                    (0.10, () =>
                    {
                        var last = BuildBranch(start, Generator.Between(1, 2));
                        planner.UpdateNode(last with { Branch = "hsu_sample" });
                    })
                });
                break;
            }

            // Overload should be harder than extreme
            case ("C", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Basic error alarm run with turnoff zone deep at the end
                    (0.33, () =>
                    {
                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "primary" });
                        var end = BuildBranch(locked, 1);
                        var (hsu, _) = AddZone(end, new ZoneNode { Branch = "hsu_sample" });

                        // Any side objectives and we allow disabling the alarm
                        var allowTurnoff = level.Settings.Bulkheads != Bulkhead.Main;

                        ZoneNode? terminal = allowTurnoff
                            ? BuildBranch(hsu, Generator.Between(1, 2), "error_turnoff")
                            : null;

                        // Optionally add shadows if the level has shadows in it
                        var population = level.Settings.HasShadows()
                            ? WavePopulation.OnlyShadows
                            : WavePopulation.Baseline;

                        // Lock the first zone
                        AddErrorAlarm(locked, terminal, WaveSettings.Error_Normal, population);

                        startZone.Coverage = CoverageMinMax.Tiny;
                    }),

                    // Big Apex alarm to enter
                    (0.66, () =>
                    {
                        var (lockedApex, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });

                        // Add some extra resources
                        startZone.HealthPacks += 5;
                        startZone.ToolPacks += 5;
                        startZone.AmmoPacks += 6;

                        // Configure the wave population
                        var population = WavePopulation.Baseline_Hybrids;

                        if (level.Settings.HasShadows())
                            population = Generator.Flip(0.4) ? WavePopulation.OnlyShadows : WavePopulation
                                .Baseline_Shadows;

                        // Chargers first, then flyers
                        if (level.Settings.HasChargers())
                            population = WavePopulation.Baseline_Chargers;
                        else if (level.Settings.HasFlyers())
                            population = WavePopulation.Baseline_Flyers;

                        // Add the apex alarm
                        AddApexAlarm(lockedApex, population, WaveSettings.Baseline_Normal);
                    }),
                });
                break;
            }
            #endregion

            #region Tier: D
            case ("D", Bulkhead.Main):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Error alarm and keycard
                    (0.15, () =>
                    {
                        var (hsu, hsuZone) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(2, 3),
                            1,
                            1
                        );

                        planner.UpdateNode(hsu with { Branch = "hsu_sample" });

                        hsuZone.Coverage = CoverageMinMax.Medium;
                    }),

                    // Error alarm with generator lock
                    (0.20, () =>
                    {
                        var (hsu, hsuZone) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start,
                            Generator.Between(2, 3),
                            1);

                        planner.UpdateNode(hsu with { Branch = "hsu_sample" });

                        hsuZone.Coverage = CoverageMinMax.Medium;
                    }),

                    // 1 generator lock
                    (0.20, () =>
                    {
                        var (prelude, preludeZone) = AddZone(start, new ZoneNode
                        {
                            Branch = "primary",
                            MaxConnections = 3
                        });
                        preludeZone.GenTGeomorph(level.Complex);

                        var (hsu, hsuZone) = AddZone(prelude, new ZoneNode { Branch = "hsu_sample" });
                        hsuZone.Coverage = CoverageMinMax.Medium;

                        var cell = BuildBranch(prelude, Generator.Between(2, 3), "power_cell");

                        // Lock the first zone
                        AddGeneratorPuzzle(hsu, cell);
                    }),

                    // 2 keycards
                    (0.40, () =>
                    {
                        // Update number of connections for hub zone
                        planner.UpdateNode(start with { MaxConnections = 3 });
                        startZone.GenHubGeomorph(level.Complex);

                        // Second area, with also a locked zone
                        var (node2, zone2) = AddZone(start, new ZoneNode { Branch = "primary", MaxConnections = 3 });
                        zone2.GenHubGeomorph(level.Complex);

                        var keycard1 = BuildBranch(start, 1, "keycard_1");

                        // Lock the first zone
                        AddKeycardPuzzle(node2, keycard1);

                        // Build the second keycard zone and
                        var keycard2 = BuildBranch(node2, 1, "keycard_2");
                        var (hsu, hsuZone) = AddZone(node2, new ZoneNode { Branch = "hsu_sample" });

                        hsuZone.Coverage = CoverageMinMax.Medium;

                        // Lock the first zone
                        AddKeycardPuzzle(hsu, keycard2);
                    }),

                    // Straight to HSU... with error alarm active
                    // No turning it off
                    (0.05, () =>
                    {
                        var population = WavePopulation.Baseline;

                        // First set shadows if we have them
                        if (level.Settings.HasShadows())
                            population = Generator.Flip(0.4) ? WavePopulation.OnlyShadows : WavePopulation
                                .Baseline_Shadows;

                        // Next check and set chargers first, then flyers
                        if (level.Settings.HasChargers())
                            population = WavePopulation.Baseline_Chargers;
                        else if (level.Settings.HasFlyers())
                            population = WavePopulation.Baseline_Flyers;

                        objective.WavesOnElevatorLand.Add(GenericWave.ErrorAlarm_Hard with
                        {
                            Population = population
                        });
                        level.MarkAsErrorAlarm();

                        var last = BuildBranch(start, Generator.Between(2, 3));
                        planner.UpdateNode(last with { Branch = "hsu_sample" });
                    })
                });
                break;
            }

            // Shorter and a bit easier secondary objective
            case ("D", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // 1 generator lock
                    (0.50, () =>
                    {
                        var (prelude, preludeZone) = AddZone(start, new ZoneNode
                        {
                            Branch = "primary",
                            MaxConnections = 3
                        });
                        preludeZone.GenTGeomorph(level.Complex);

                        var (locked, _) = AddZone(prelude, new ZoneNode { Branch = "hsu_sample" });
                        var cell = BuildBranch(prelude, 2, "power_cell");

                        // Lock the first zone
                        AddGeneratorPuzzle(locked, cell);
                    }),

                    // Build 1 keycard lock
                    (0.50, () =>
                    {
                        // Possibly add an extra zone to go throuh
                        start = BuildBranch(start, Generator.Between(1, 2));
                        startZone = planner.GetZone(start)!;

                        // Update number of connections for hub zone
                        planner.UpdateNode(start with { MaxConnections = 3 });
                        startZone.GenHubGeomorph(level.Complex);

                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });
                        var keycard = BuildBranch(start, Generator.Between(1, 2), "keycard");

                        // Lock the first zone
                        AddKeycardPuzzle(locked, keycard);
                    }),
                });
                break;
            }

            // Overload should be harder than extreme
            case ("D", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Basic error alarm run with turnoff zone deep at the end
                    (0.33, () =>
                    {
                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "primary" });
                        var end = BuildBranch(locked, 1);
                        var (hsu, _) = AddZone(end, new ZoneNode { Branch = "hsu_sample" });

                        // Any side objectives and we allow disabling the alarm
                        var allowTurnoff = level.Settings.Bulkheads != Bulkhead.Main;

                        ZoneNode? terminal = allowTurnoff
                            ? BuildBranch(hsu, Generator.Between(1, 2), "error_turnoff")
                            : null;

                        // Error alarm population
                        var population = WavePopulation.Baseline;

                        // First set shadows if we have them
                        if (level.Settings.HasShadows())
                            population = Generator.Flip(0.4) ? WavePopulation.OnlyShadows : WavePopulation
                                .Baseline_Shadows;

                        // Next check and set chargers first, then flyers
                        if (level.Settings.HasChargers())
                            population = WavePopulation.Baseline_Chargers;
                        else if (level.Settings.HasFlyers())
                            population = WavePopulation.Baseline_Flyers;

                        // Lock the first zone
                        AddErrorAlarm(locked, terminal, WaveSettings.Error_Hard, population);

                        startZone.Coverage = CoverageMinMax.Tiny;
                    }),

                    // Big Apex alarm to enter
                    (0.66, () =>
                    {
                        var (lockedApex, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });

                        // Add some extra resources
                        startZone.HealthPacks += 4;
                        startZone.ToolPacks += 4;
                        startZone.AmmoPacks += 6;

                        // Configure the wave population
                        var population = WavePopulation.Baseline_Hybrids;

                        if (level.Settings.HasShadows())
                            population = Generator.Flip(0.4) ? WavePopulation.OnlyShadows : WavePopulation
                                .Baseline_Shadows;

                        // Chargers first, then flyers
                        if (level.Settings.HasChargers())
                            population = WavePopulation.Baseline_Chargers;
                        else if (level.Settings.HasFlyers())
                            population = WavePopulation.Baseline_Flyers;

                        // Add the apex alarm
                        AddApexAlarm(lockedApex, population, WaveSettings.Baseline_Normal);
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
                    //   start -> node2 -> [0-1] -> end -> hsu     -> error
                    //                                  -> keycard
                    (0.15, () =>
                    {
                        var (hsu, hsuZone) = BuildChallenge_ErrorWithOff_KeycardInSide(
                            start,
                            Generator.Between(2, 3),
                            1,
                            Generator.Between(1, 2)
                        );

                        planner.UpdateNode(hsu with { Branch = "hsu_sample" });

                        hsuZone.Coverage = CoverageMinMax.Medium;
                    }),

                    // Error alarm with generator lock
                    //      start (cell) -> node2 -> [0-1] -> end (generator) -> hsu -> error_turnoff
                    (0.20, () =>
                    {
                        var (hsu, hsuZone) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
                            start,
                            Generator.Between(2, 3),
                            1);

                        planner.UpdateNode(hsu with { Branch = "hsu_sample" });

                        hsuZone.Coverage = CoverageMinMax.Medium;
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

                        var (locked, _) = AddZone(prelude, new ZoneNode { Branch = "hsu_sample" });
                        var cell = BuildBranch(prelude, 3, "power_cell");

                        // Lock the first zone
                        AddGeneratorPuzzle(locked, cell);
                    }),

                    // 2 keycards
                    (0.40, () =>
                    {
                        // Update number of connections for hub zone
                        planner.UpdateNode(start with { MaxConnections = 3 });
                        startZone.GenHubGeomorph(level.Complex);

                        // Second area, with also a locked zone
                        var (node2, zone2) = AddZone(start, new ZoneNode { Branch = "primary", MaxConnections = 3 });
                        zone2.GenHubGeomorph(level.Complex);

                        var keycard1 = BuildBranch(start, 1, "keycard_1");

                        // Lock the first zone
                        AddKeycardPuzzle(node2, keycard1);

                        // Build the second keycard zone and
                        var keycard2 = BuildBranch(node2, 1, "keycard_2");
                        var (hsu, hsuZone) = AddZone(node2, new ZoneNode { Branch = "hsu_sample" });

                        hsuZone.Coverage = CoverageMinMax.Medium;

                        // Lock the first zone
                        AddKeycardPuzzle(hsu, keycard2);
                    }),

                    // Straight to HSU... with error alarm active
                    // No turning it off
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

                        var last = BuildBranch(start, Generator.Between(2, 3));
                        planner.UpdateNode(last with { Branch = "hsu_sample" });
                    })
                });
                break;
            }

            // Shorter and a bit easier secondary objective
            case ("E", Bulkhead.Extreme):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // 1 generator lock
                    (0.50, () =>
                    {
                        var (prelude, preludeZone) = AddZone(start, new ZoneNode
                        {
                            Branch = "primary",
                            MaxConnections = 3
                        });
                        preludeZone.GenTGeomorph(level.Complex);

                        var (locked, _) = AddZone(prelude, new ZoneNode { Branch = "hsu_sample" });
                        var cell = BuildBranch(prelude, 2, "power_cell");

                        // Lock the first zone
                        AddGeneratorPuzzle(locked, cell);
                    }),

                    // Build 1 keycard lock
                    (0.50, () =>
                    {
                        // Possibly add an extra zone to go throuh
                        start = BuildBranch(start, Generator.Between(1, 2));
                        startZone = planner.GetZone(start)!;

                        // Update number of connections for hub zone
                        planner.UpdateNode(start with { MaxConnections = 3 });
                        startZone.GenHubGeomorph(level.Complex);

                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });
                        var keycard = BuildBranch(start, Generator.Between(1, 2), "keycard");

                        // Lock the first zone
                        AddKeycardPuzzle(locked, keycard);
                    }),
                });
                break;
            }

            // Overload should be harder than extreme
            case ("E", Bulkhead.Overload):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Basic error alarm run with turnoff zone deep at the end
                    (0.50, () =>
                    {
                        var (locked, _) = AddZone(start, new ZoneNode { Branch = "primary" });
                        var end = BuildBranch(locked, 1);
                        var (hsu, _) = AddZone(end, new ZoneNode { Branch = "hsu_sample" });

                        // Any side objectives and we allow disabling the alarm
                        var allowTurnoff = level.Settings.Bulkheads != Bulkhead.Main;

                        ZoneNode? terminal = allowTurnoff
                            ? BuildBranch(hsu, Generator.Between(1, 2), "error_turnoff")
                            : null;

                        // Error alarm population
                        var population = WavePopulation.Baseline;

                        // First set shadows if we have them
                        if (level.Settings.HasShadows())
                            population = Generator.Flip(0.4) ? WavePopulation.OnlyShadows : WavePopulation
                                .Baseline_Shadows;

                        // Next check and set chargers first, then flyers
                        if (level.Settings.HasChargers())
                            population = WavePopulation.Baseline_Chargers;
                        else if (level.Settings.HasFlyers())
                            population = WavePopulation.Baseline_Flyers;
                        else if (level.Settings.HasNightmares())
                            population = WavePopulation.Baseline_Nightmare;

                        // Lock the first zone
                        AddErrorAlarm(locked, terminal, WaveSettings.Error_VeryHard, population);

                        startZone.Coverage = CoverageMinMax.Tiny;
                    }),

                    // Big Apex alarm to enter
                    (0.50, () =>
                    {
                        var (lockedApex, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });

                        // Add some extra resources
                        startZone.HealthPacks += 2;
                        startZone.ToolPacks += 6;
                        startZone.AmmoPacks += 8;

                        // Configure the wave population
                        var population = WavePopulation.Baseline_Hybrids;

                        if (level.Settings.HasShadows())
                            population = Generator.Flip(0.4) ? WavePopulation.OnlyShadows : WavePopulation
                                .Baseline_Shadows;

                        // Chargers first, then flyers
                        if (level.Settings.HasChargers())
                            population = WavePopulation.Baseline_Chargers;
                        else if (level.Settings.HasFlyers())
                            population = WavePopulation.Baseline_Flyers;

                        // Add the apex alarm
                        AddApexAlarm(lockedApex, population, WaveSettings.Baseline_Normal);
                    }),
                });
                break;
            }
            #endregion

            default:
            {
                // This is the old way we generated HSU missions. Should still work while we
                // implement the remaining tiers
                var last = BuildBranch(start, director.ZoneCount);
                planner.UpdateNode(last with { Branch = "hsu_sample" });
                break;
            }
        }
    }
}
