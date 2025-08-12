using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
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
        var lastZone = new Zone(level);

        switch (level.Tier, director.Bulkhead, objective.GatherRequiredCount)
        {
            #region Tier: A
            case ("A", Bulkhead.Main, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Double dead end
                    (0.30, () =>
                    {
                        startZone.GenHubGeomorph(level.Complex);

                        var (_, end1Zone) = AddZone(start, new ZoneNode { Branch = "find_items" });
                        end1Zone.GenDeadEndGeomorph(level.Complex);

                        var (_, end2Zone) = AddZone(start, new ZoneNode { Branch = "find_items" });
                        end2Zone.GenDeadEndGeomorph(level.Complex);
                    }),

                    // Single generator
                    (0.25, () =>
                    {
                        (last, lastZone) = BuildChallenge_GeneratorCellInSide(start);

                        last = planner.UpdateNode(last with { Branch = "find_items" });
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;
                    }),

                    // Single keycard
                    (0.30, () =>
                    {
                        (last, lastZone) = BuildChallenge_KeycardInSide(start);

                        last = planner.UpdateNode(last with { Branch = "find_items" });
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;
                    }),

                    // Single boss fight
                    (0.15, () =>
                    {
                        (last, lastZone) = BuildChallenge_BossFight(start);

                        last = planner.UpdateNode(last with { Branch = "find_items" });
                        lastZone.Coverage = objective.GatherRequiredCount > 5 ? CoverageMinMax.Huge : CoverageMinMax.Large;
                    }),
                });
                break;
            }

            case ("A", Bulkhead.Extreme, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Just fetch from the first zone
                    (0.2, () =>
                    {
                        last = planner.UpdateNode(start with { Branch = "find_items" });
                        lastZone = planner.GetZone(last)!;

                        lastZone.Coverage = CoverageMinMax.Medium;
                    }),

                    // Fetch from the second room, but it's a dead end
                    (0.2, () =>
                    {
                        (last, lastZone) = AddZone(start, new ZoneNode { Branch = "find_items" });
                        lastZone.GenDeadEndGeomorph(level.Complex);
                    }),
                });
                break;
            }

            case ("A", Bulkhead.Overload, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // Fetch from the second room, but it's a dead end
                    (0.3, () =>
                    {
                        (last, lastZone) = AddZone(start, new ZoneNode { Branch = "find_items" });
                        lastZone.GenDeadEndGeomorph(level.Complex);
                    }),

                    // Tiny prelude
                    (0.5, () =>
                    {
                        (last, lastZone) = AddZone(start, new ZoneNode { Branch = "find_items" });
                        startZone.Coverage = new CoverageMinMax { Min = 10, Max = 10 };
                        lastZone.Coverage = CoverageMinMax.Medium;
                    }),

                    // Scout room
                    (0.2, () =>
                    {
                        last = AddScoutRoom_Normal(start);
                        last = planner.UpdateNode(last with { Branch = "find_items" });
                    })
                });
                break;
            }
            #endregion

            #region Tier: B
            case ("B", Bulkhead.Main, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.2, () =>
                    {
                        AddBranch(start, Generator.Between(1, 2), "find_items");
                    })
                });
                break;
            }
            #endregion

            #region Tier: C
            case ("C", Bulkhead.Main, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.2, () =>
                    {
                        AddBranch(start, Generator.Between(1, 2), "find_items");
                    })
                });
                break;
            }
            #endregion

            #region Tier: D
            case ("D", Bulkhead.Main, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.2, () =>
                    {
                        AddBranch(start, Generator.Between(1, 2), "find_items");
                    })
                });
                break;
            }
            #endregion

            #region Tier: E
            case ("E", Bulkhead.Main, _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (0.2, () =>
                    {
                        AddBranch(start, Generator.Between(1, 2), "find_items");
                    })
                });
                break;
            }
            #endregion

            default:
            {
                AddBranch(start, Generator.Between(1, 2), "find_items");
                break;
            }
        }
    }
}
