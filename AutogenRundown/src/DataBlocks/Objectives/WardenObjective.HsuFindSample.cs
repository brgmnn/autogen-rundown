using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

/**
 * Objective: HsuFindSample
 *
 *
 * Collect the HSU from within a storage zone
 */
public partial record class WardenObjective : DataBlock
{
    public void Build_HsuFindSample(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = "Find <color=orange>[ITEM_SERIAL]</color> somewhere inside HSU Storage Zone";

        ActivateHSU_BringItemInElevator = true;
        GatherItemId = (uint)WardenObjectiveItem.HSU;
        StartPuzzle = ChainedPuzzle.FindOrPersist(ChainedPuzzle.TeamScan);

        // Place HSU's within the objective zone
        var zn = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead, "hsu_sample")!;
        var zoneIndex = zn.ZoneNumber;

        dataLayer.ObjectiveData.ZonePlacementDatas.Add(
            new List<ZonePlacementData>()
            {
                new ZonePlacementData
                {
                    LocalIndex = zoneIndex,
                    Weights = ZonePlacementWeights.NotAtStart
                }
            });

        var zone = layout.Zones[zoneIndex];
        zone.HSUsInZone = level.Tier switch
        {
            "B" => DistributionAmount.SomeMore,
            "C" => DistributionAmount.Many,
            "D" => DistributionAmount.Alot,
            "E" => DistributionAmount.Tons,
            _ => DistributionAmount.Some
        };
        zone.Coverage = level.Tier switch
        {
            "A" => new CoverageMinMax { Min = 20, Max = 25 },
            "B" => new CoverageMinMax { Min = 25, Max = 30 },
            "C" => new CoverageMinMax { Min = 30, Max = 50 },
            "D" => new CoverageMinMax { Min = 50, Max = 75 },
            "E" => new CoverageMinMax { Min = 75, Max = 100 },
            _ => zone.Coverage
        };
        zone.HSUClustersInZone = level.Tier switch
        {
            "C" => 2,
            "D" => 3,
            "E" => 3,
            _ => 1
        };

        // Extraction waves. These are progressively harder
        // Overload is balanced to get harder error waves when getting the sample
        switch (level.Tier, director.Bulkhead)
        {
            case ("A", Bulkhead.Main):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Easy);
                break;
            case ("A", _):
                // No exits for the A-tier side objectives
                break;

            case ("B", Bulkhead.Extreme):
                // Extreme is easier so it doesn't get one
                break;
            case ("B", _):
                // Always get an exit trickle on Main/Overload for B
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Easy);
                break;

            case ("C", Bulkhead.Main):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Medium);
                break;
            case ("C", Bulkhead.Extreme):
                // No exit still on C extreme
                break;
            case ("C", Bulkhead.Overload):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Easy);
                break;

            case ("D", Bulkhead.Main):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Hard);
                break;
            case ("D", Bulkhead.Extreme):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Easy);
                break;
            case ("D", Bulkhead.Overload):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Medium);
                break;

            case ("E", Bulkhead.Main):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_VeryHard);
                break;
            case ("E", Bulkhead.Extreme):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Medium);
                break;
            case ("E", Bulkhead.Overload):
                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Hard);
                break;
        }
    }
}
