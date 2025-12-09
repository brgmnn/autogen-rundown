using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: PowerCellDistribution
 *
 *
 * Prisoners have to go and enter a command on a terminal somewhere in the zone. The objective
 * itself is quite simple, but the real fun will come from what the effects are of the terminal
 * command. Base game has lights off as an effect.
 *
 */
public partial record WardenObjective
{
    /// <summary>
    /// We need to select the type of special terminal command that will be used here
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void PreBuild_ReachKdsDeep(BuildDirector director, Level level)
    {
        director.DisableStartingArea = true;

        SubType = Generator.Pick(new List<WardenObjectiveSubType>
        {
            WardenObjectiveSubType.ErrorAlarmChase
        });

        KdsDeepUnit = Generator.Between(1, 9);
    }

    public void Build_ReachKdsDeep(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = new Text("Reach and enter KDS Deep");
        // FindLocationInfo = "Gather information about the location of [ITEM_SERIAL]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";

        dataLayer.ObjectiveData.WinCondition = WardenObjectiveWinCondition.GoToExitGeo;

        switch (SubType)
        {
            case WardenObjectiveSubType.ErrorAlarmChase:
            {
                EventsOnElevatorLand
                    .AddSound(Sound.R8E1_ErrorAlarm, 2.0, WardenObjectiveEventTrigger.None)
                    .AddSound(
                        Sound.R8E1_GargantaWarning,
                        subtitle: 442824023,
                        delay: 7.0,
                        trigger: WardenObjectiveEventTrigger.None)
                    .AddSpawnWave(
                        new GenericWave
                        {
                            Settings = (WaveSettings.Error_VeryHard with
                            {
                                FilterType = PopulationFilterType.Include,
                                PopulationFilter = new List<Enemies.EnemyType>
                                {
                                    Enemies.EnemyType.Standard,
                                    Enemies.EnemyType.MiniBoss
                                },
                                OverrideWaveSpawnType = true,
                                SurvivalWaveSpawnType = Enemies.SurvivalWaveSpawnType.FromElevatorDirection,
                            }).FindOrPersist(),
                            Population = WavePopulation.Baseline_Hybrids,
                            SpawnDelay = 0.0,
                            TriggerAlarm = true
                        }, 30.0);
                break;
            }
        }

        if (level.HasOverload)
        {
            var overload = level.Planner.GetBulkheadFirstZone(Bulkhead.Overload);

            if (overload != null)
            {
                var overloadZone = level.Planner.GetZone((ZoneNode)overload);

                if (overloadZone != null)
                {
                    Plugin.Logger.LogDebug($"Cycling fog");
                    overloadZone.EventsOnOpenDoor.AddCyclingFog(
                        Fog.FullFog,
                        level.FogSettings,
                        2);
                }
            }
        }

        // Set type to empty
        Type = WardenObjectiveType.Empty;

        #region Warden Intel Messages
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 5), Generator.Draw(new List<string>
        {
            // TODO: Claude Code: Add 300 intel messages here
        }))!);
        #endregion
    }
}
