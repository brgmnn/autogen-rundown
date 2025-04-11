namespace AutogenRundown.DataBlocks;

/*
 * This objective is bringing a large item like Neonate or Data Sphere to a machine and then
 * optionally returning with it.
 *
 * Looks like we have a few options for what we could do with this:
 *  - Bring Item down in elevator -> insert into device
 *  - Bring Item down in elevator -> insert into device -> extract with it
 *  - No item in elevator -> find item -> insert into device
 *  - No item in elevator -> find item -> insert into device -> extract with it
 */
public partial record WardenObjective
{
    public void PreBuild_HsuActivateSmall(BuildDirector director, Level level)
    {
        ActivateHSU_ItemFromStart = Items.Item.DataSphere;
        ActivateHSU_ItemAfterActivation = Items.Item.DataSphere;

        // ActivateHSU_ItemFromStart = Items.Item.NeonateHsu;
        // ActivateHSU_ItemAfterActivation = Items.Item.NeonateHsu;
        ActivateHSU_RequireItemAfterActivationInExitScan = true;
    }

    public void Build_HsuActivateSmall(BuildDirector director, Level level)
    {
        var machine = "Neural Frame";

        if (ActivateHSU_ItemFromStart == Items.Item.DataSphere)
        {
            MainObjective = "Bring the Data sphere to [ITEM_SERIAL] to retrieve its data";
            SolveItem = "Insert Data Sphere into Neural Frame [ITEM_SERIAL]";
            GoToWinCondition_Elevator = "Return the Data Sphere to the point of entrance in [EXTRACTION_ZONE]";
            GoToWinCondition_CustomGeo = "Bring the Data Sphere to the forward exit in [EXTRACTION_ZONE]";
        }
        else if (ActivateHSU_ItemFromStart == Items.Item.NeonateHsu)
        {
            MainObjective = "Bring the Neonate to [ITEM_SERIAL] to resuscitate it";
            SolveItem = "Insert Neonate into Neural Frame [ITEM_SERIAL]";
            GoToWinCondition_Elevator = "Return the Neonate to the point of entrance in [EXTRACTION_ZONE]";
            GoToWinCondition_CustomGeo = "Bring the Neonate to the forward exit in [EXTRACTION_ZONE]";
        }

        FindLocationInfo = "Gather information about the location of [ITEM_SERIAL]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        GoToZone = "Navigate to [ITEM_ZONE] and find [ITEM_SERIAL]";
        GoToZoneHelp = "Use information in the environment to find [ITEM_ZONE]";
        InZoneFindItem = "Find [ITEM_SERIAL] inside [ITEM_ZONE]";
        GoToWinConditionHelp_Elevator = "Use the navigational beacon and the floor map ([KEY_MAP]) to find the way back";
        GoToWinConditionHelp_CustomGeo = "Use the navigational beacon and the information in the surroundings to find the exit point";
        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        ActivateHSU_BringItemInElevator = true;
        ActivateHSU_MarkItemInElevatorAsWardenObjective = false;
        ActivateHSU_StopEnemyWavesOnActivation = false;
        ActivateHSU_ObjectiveCompleteAfterInsertion = false;
        ActivateHSU_RequireItemAfterActivationInExitScan = false;
    }
}
