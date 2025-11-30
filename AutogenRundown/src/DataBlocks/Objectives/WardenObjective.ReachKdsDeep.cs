using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
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

        MainObjective = new Text("Find Computer terminal [ITEM_SERIAL] and input the backdoor command [SPECIAL_COMMAND]");
        FindLocationInfo = "Gather information about the location of [ITEM_SERIAL]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = new Text("Navigate to [ITEM_ZONE] and find [ITEM_SERIAL]");
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Find [ITEM_SERIAL] somewhere inside [ITEM_ZONE]";
        InZoneFindItemHelp = "Use maintenance terminal command PING to find [ITEM_SERIAL]";
        SolveItem = "Proceed to input the backdoor command [SPECIAL_COMMAND] in [ITEM_SERIAL]";

        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        dataLayer.ObjectiveData.WinCondition = WardenObjectiveWinCondition.GoToExitGeo;

        switch (SubType)
        {
            case WardenObjectiveSubType.ErrorAlarmChase:
            {
                EventsOnElevatorLand
                    .AddSound(Sound.R8E1_ErrorAlarm, 2.0, WardenObjectiveEventTrigger.None)
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
                            SpawnDelay = 5.0,
                            TriggerAlarm = true
                        }, 2.0);
                break;
            }
        }

        // Set type to empty
        Type = WardenObjectiveType.Empty;
    }
}
