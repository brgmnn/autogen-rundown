using AutogenRundown.DataBlocks.Alarms;
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
                        AddErrorAlarm(locked, terminal, ChainedPuzzle.AlarmError_Baseline with
                        {
                            PersistentId = 0,
                            Population = population,
                            Settings = WaveSettings.Error_Easy
                        });

                        startZone.Coverage = CoverageMinMax.Tiny;
                    }),

                    // Big Apex alarm to enter
                    (0.20, () =>
                    {
                        var (lockedApex, _) = AddZone(start, new ZoneNode { Branch = "hsu_sample" });

                        // Add some extra resources
                        startZone.HealthMulti *= 2.0;
                        startZone.ToolAmmoMulti *= 2.0;
                        startZone.WeaponAmmoMulti *= 2.0;

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
            #endregion

            #region Tier: C
            // TODO: Implement
            case ("C", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight to HSU
                    (0.10, () =>
                    {
                        var last = BuildBranch(start, Generator.Between(2, 3));
                        planner.UpdateNode(last with { Branch = "hsu_sample" });
                    })
                });
                break;
            }
            #endregion

            #region Tier: D
            // TODO: Implement
            case ("D", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight to HSU
                    (0.10, () =>
                    {
                        var last = BuildBranch(start, Generator.Between(2, 3));
                        planner.UpdateNode(last with { Branch = "hsu_sample" });
                    })
                });
                break;
            }
            #endregion

            #region Tier: E
            // TODO: Implement
            case ("E", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Straight to HSU
                    (0.10, () =>
                    {
                        var last = BuildBranch(start, Generator.Between(2, 3));
                        planner.UpdateNode(last with { Branch = "hsu_sample" });
                    })
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
