using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    public void BuildLayout_ClearPath(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;

        var exit = new ZoneNode();
        var exitZone = new Zone();

        // This whole objective can only be done on main
        switch (level.Tier)
        {
            #region Tier: A, B, C
            // case "A":
            // case "B":
            case "C":
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    // // Straight shot
                    // (0.4, () => { }),

                    // Boss Fight: Mega Mother
                    (0.4, () =>
                    {
                        var nodes = AddBranch(start, Generator.Between(1, 2), "primary",
                            (node, zone) => zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward);

                        var (boss, bossZone) = AddZone(nodes.Last(), "boss_fight");
                        (exit, exitZone) = AddZone(boss, "exit");

                        exitZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

                        var bossClearEvents = new List<WardenObjectiveEvent>()
                            .AddUnlockDoor(director.Bulkhead, exit.ZoneNumber);

                        AddAlignedBossFight_MegaMom(boss, bossClearEvents);
                    })
                });
                break;
            }

            case "A":
            case "B":
            case "D":
            case "E":
            {
                var nodes = AddBranch(start, director.ZoneCount, "primary",
                    (node, zone) => zone.ZoneExpansion = level.Settings.GetDirections(director.Bulkhead).Forward);

                exit = nodes.Last();
                exitZone = planner.GetZone(exit)!;
                break;
            }
            #endregion

            // #region Tier: D
            // case "D":
            // {
            //     Generator.SelectRun(new List<(double, Action)>
            //     {
            //         // Straight shot
            //         (0.4, () => { }),
            //
            //         // Mega Mom
            //         (0.4, () => { })
            //     });
            //     break;
            // }
            // #endregion
            //
            // #region Tier: E
            // case "E":
            // {
            //     break;
            // }
            // #endregion
        }

        // Configure the exit zone
        exit = planner.UpdateNode(exit with { Tags = exit.Tags.Extend("exit_elevator") });
        exitZone.GenExitGeomorph(level.Complex);
        exitZone.Coverage = new() { Min = 64, Max = 64 };

        // Ensure there's a nice spicy hoard at the end
        exitZone.EnemySpawningInZone.Add(
            // These will be predominately strikers / shooters
            new EnemySpawningData()
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = (uint)EnemyRoleDifficulty.Easy,
                Points = 75, // 25pts is 1.0 distribution, this is quite a lot
            });

        // var prev = start;
        //
        // for (var i = 1; i < director.ZoneCount; i++)
        // {
        //     var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
        //     var next = new ZoneNode(director.Bulkhead, zoneIndex);
        //     var nextZone = new Zone
        //     {
        //         Coverage = CoverageMinMax.GenNormalSize(),
        //         LightSettings = Lights.GenRandomLight(),
        //     };
        //
        //     nextZone.RollFog(level);
        //
        //     // This means it is the last zone
        //     if (i == director.ZoneCount - 1)
        //         next.Tags.Add("exit_elevator");
        //
        //     level.Planner.Connect(prev, next);
        //     level.Planner.AddZone(next, nextZone);
        //
        //     prev = next;
        // }
        //
        // var last = prev;
        // var lastZone = level.Planner.GetZone(last)!;
        //
        // var secondLast = (ZoneNode)level.Planner.GetBuildFrom(last)!;
        // var secondLastZone = level.Planner.GetZone(secondLast)!;
        //
        // var subcomplex = GenSubComplex(level.Complex);
        //
        // // Some adjustments to try and reduce the chance of the exit geo not
        // // spawning due to being trapped by a small penultimate zone
        // secondLastZone.ZoneExpansion = ZoneExpansion.Expansional;
        // secondLastZone.Coverage = new CoverageMinMax { Min = 35, Max = 45 };
        // lastZone.StartPosition = ZoneEntranceBuildFrom.Furthest;
        //
        // // The final zone is the extraction zone
        // lastZone.Coverage = new CoverageMinMax { Min = 50, Max = 50 };
        // lastZone.SubComplex = subcomplex;
        // lastZone.GenExitGeomorph(level.Complex);
    }
}
