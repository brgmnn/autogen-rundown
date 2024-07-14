namespace AutogenRundown.DataBlocks;

/**
 * Objective: ClearPath
 *
 *
 * Fairly straight forward objective, get to the end zone. Some additional enemies
 * at the end make this a more interesting experience.
 *
 * This objective can only be for Main given it ends the level on completion
 */
public partial record class WardenObjective : DataBlock
{
    public void PreBuild_Survival(BuildDirector director, Level level) { }

    public void Build_Survival(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = "Find a way to stay alive during Warden Protocol E.v1, and make your way to [EXTRACTION_ZONE] for extraction";

        Survival_TimerTitle = "Time until allowed extraction:";
        Survival_TimerToActivateTitle = "<color=red>WARNING!</color> Warden Protocol <color=orange>E.v1</color> will commence in: ";

        Survival_TimeToActivate = 30.0;
        Survival_TimeToSurvive = 20.0;
    }
}
