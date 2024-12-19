using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record WardenObjective
{
    public void Build_RetrieveBigItems(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);
        var item = RetrieveItems.First();

        MainObjective = "Find [ALL_ITEMS] and bring it to the extraction scan in [EXTRACTION_ZONE]";
        FindLocationInfo = "Gather information about the location of [ALL_ITEMS]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";

        if (RetrieveItems.Count == 1)
        {
            if (dataLayer.ObjectiveData.ZonePlacementDatas[0].Count == 1)
            {
                var zone = Intel.Zone(dataLayer.ObjectiveData.ZonePlacementDatas[0][0].LocalIndex +
                                      layout.ZoneAliasStart);

                GoToZone = $"Navigate to {zone} and find [ALL_ITEMS]";
                GoToZoneHelp = $"Use information in the environment to find {zone}";
            }
            else
            {
                var zones = string.Join(", ",
                    dataLayer.ObjectiveData.ZonePlacementDatas[0].Select(placement =>
                        Intel.Zone(placement.LocalIndex + layout.ZoneAliasStart)));

                GoToZone = $"Navigate to and find [ALL_ITEMS] in one of zones {zones}";
                GoToZoneHelp = $"Use information in the environment to find {zones}";
            }
        }
        else
        {
            GoToZone = "Navigate to and find [ALL_ITEMS]";
            GoToZoneHelp = "Use information in the environment to find each item zone";
        }

        InZoneFindItem = "Find [ALL_ITEMS] somewhere inside [ITEM_ZONE]";
        InZoneFindItemHelp = "Use maintenance terminal command PING to find [ALL_ITEMS]";

        // TODO: rename this
        SolveItem = "WARNING - Hisec Cargo misplaced - ENGAGING SECURITY PROTOCOLS";

        if (RetrieveItems.Count() > 1)
        {
            GoToWinCondition_Elevator = "Return [ALL_ITEMS] to the extraction point in [EXTRACTION_ZONE]";
            GoToWinCondition_CustomGeo = "Return [ALL_ITEMS] to the extraction point in [EXTRACTION_ZONE]";
        }
        else
        {
            GoToWinCondition_Elevator = "Return the [ALL_ITEMS] to the extraction point in [EXTRACTION_ZONE]";
            GoToWinCondition_CustomGeo = "Return the [ALL_ITEMS] to the extraction point in [EXTRACTION_ZONE]";
        }

        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        if (item == WardenObjectiveItem.MatterWaveProjector)
        {
            var zoneIndex = dataLayer.ObjectiveData.ZonePlacementDatas[0][0].LocalIndex;

            WavesOnGotoWin.Add(GenericWave.ExitTrickle);

            // Manually set the zones as the inbuilt ITEM_ZONE doesn't seem to
            // work correctly for MWP
            GoToZone = $"Navigate to [ZONE_{zoneIndex}] and find [ALL_ITEMS]";
            GoToZoneHelp = $"Use information in the environment to find [ZONE_{zoneIndex}]";
            InZoneFindItem = $"Find [ALL_ITEMS] somewhere inside [ZONE_{zoneIndex}]";

            SolveItem = "WARNING - Matter Wave Projector misplaced - ENGAGING SECURITY PROTOCOLS";
        }

    }
}
