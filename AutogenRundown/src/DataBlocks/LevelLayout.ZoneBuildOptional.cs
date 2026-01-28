using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

/*
 * --- General layout building blocks ---
 */
public partial record LevelLayout
{
    /// <summary>
    /// Adds a side room which is a med bay
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildOptional_MedicalBay(ZoneNode start)
    {
        var (med, medZone) = AddZone(start, new ZoneNode
        {
            Bulkhead = Bulkhead.Main,
            Branch = "medical_bay",
            MaxConnections = 0
        });

        medZone.AliasPrefix = "MedBay, ZONE";

        // Select tile
        switch (level.Complex)
        {
            case Complex.Mining:
                medZone.SubComplex = SubComplex.Refinery;
                medZone.CustomGeomorph =
                    "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_dead_end_HA_01_R8B3.prefab";
                medZone.Coverage = new CoverageMinMax { Min = 5, Max = 10 };
                break;

            case Complex.Tech:
                (medZone.SubComplex, medZone.CustomGeomorph, medZone.Coverage) = Generator.Pick(
                    new List<(SubComplex, string, CoverageMinMax)>
                    {
                        (SubComplex.DataCenter, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_data_center_dead_end_HA_01.prefab", new CoverageMinMax { Min = 5, Max = 10 }),
                        (SubComplex.Lab, "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_lab_dead_end_HA_03.prefab", new CoverageMinMax { Min = 5, Max = 10 }),

                        // --- MOD Geomorphs ---
                        // Floweria
                        (SubComplex.Lab, "Assets/Prefabs/Geomorph/Tech/geo_lab_FA_dead_end_01.prefab", new CoverageMinMax { Min = 5, Max = 10 }),
                    });
                break;

            case Complex.Service:
                (medZone.SubComplex, medZone.CustomGeomorph, medZone.Coverage) = Generator.Pick(
                    new List<(SubComplex, string, CoverageMinMax)>
                    {
                        (SubComplex.Floodways, "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_dead_end_HA_02.prefab", new CoverageMinMax { Min = 5, Max = 10 }),

                        // // --- MOD Geomorphs ---
                        // SamDB v2
                        (SubComplex.Floodways, "Assets/GameObject/Floodways_Reactor_Cooling_Spawns.prefab", new CoverageMinMax { Min = 5, Max = 10 }),
                    });
                break;
        }

        medZone.LightSettings = Lights.Light.RedToWhite_1;

        // Entry
        medZone.Alarm = ChainedPuzzle.FindOrPersist(ChainedPuzzle.TeamScan);

        switch (level.Tier)
        {
            case "A":
            case "B":
            case "C":
            case "D":
            case "E":
                break;
        }

        // Resources
        medZone.ConsumableDistributionInZone = ConsumableDistribution.MedicalBay_Consumables.PersistentId;

        medZone.HealthPacks = 6;
        medZone.DisinfectPacks = 2;
        medZone.AmmoPacks = 0;
        medZone.ToolPacks = 0;

        return (med, medZone);
    }
}
