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
    /// <summary>
    /// Collect the HSU from within a storage zone
    ///
    /// WardenObjectiveType.HsuFindSample
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    public void Build_HsuFindSample(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);

        MainObjective = "Find <color=orange>[ITEM_SERIAL]</color> somewhere inside HSU Storage Zone";

        ActivateHSU_BringItemInElevator = true;
        GatherItemId = (uint)WardenObjectiveItem.HSU;
        ChainedPuzzleToActive = ChainedPuzzle.TeamScan.PersistentId;

        // Place HSU's within the objective zone
        var zn = (ZoneNode)level.Planner.GetLastZone(director.Bulkhead)!;
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

        // Add enemies on Goto Win
        // TODO: do we want this for all bulkheads?
        if (director.Bulkhead.HasFlag(Bulkhead.Main) || director.Tier != "A")
            WavesOnGotoWin.Add(GenericWave.ExitTrickle);
    }
}
