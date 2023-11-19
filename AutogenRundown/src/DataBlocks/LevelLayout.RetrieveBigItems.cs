using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks
{
    public partial record class LevelLayout : DataBlock
    {
        /// <summary>
        /// Builds the level layout for the Matter Wave Projector big item retrieval objective.
        /// </summary>
        public static void BuildLayout_MatterWaveProjector(
            Level level,
            BuildDirector director,
            WardenObjective objective,
            ZoneNode start)
        {
            var objectiveLayerData = level.GetObjectiveLayerData(director.Bulkhead);
            var branchZoneCount = Generator.Random.Next(3, 4);
            var prev = start;

            // Generate some zones leading to the objective
            for (int i = 0; i < branchZoneCount; i++)
            {
                var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
                var next = new ZoneNode(director.Bulkhead, zoneIndex);
                var nextZone = new Zone
                {
                    Coverage = CoverageMinMax.GenNormalSize(),
                    LightSettings = Lights.GenRandomLight(),
                };
                nextZone.RollFog(level);

                level.Planner.Connect(prev, next);
                level.Planner.AddZone(next, nextZone);

                prev = next;
            }

            // Penultimate zone is an I-corridor
            var corridorIndex = level.Planner.NextIndex(director.Bulkhead);
            var corridor = new ZoneNode(director.Bulkhead, corridorIndex);
            corridor.MaxConnections = 1;

            var corridorZone = new Zone { LightSettings = Lights.GenRandomLight() };
            corridorZone.GenCorridorGeomorph(director.Complex);
            corridorZone.RollFog(level);

            level.Planner.Connect(prev, corridor);
            level.Planner.AddZone(corridor, corridorZone);

            // Final zone is the matter wave projector
            var mwpIndex = level.Planner.NextIndex(director.Bulkhead);
            var mwp = new ZoneNode(director.Bulkhead, mwpIndex);
            mwp.MaxConnections = 3;

            var mwpZone = new Zone { LightSettings = Lights.GenRandomLight() };
            mwpZone.GenMatterWaveProjectorGeomorph(director.Complex);
            mwpZone.RollFog(level);

            level.Planner.Connect(corridor, mwp);
            level.Planner.AddZone(mwp, mwpZone);

            // Assign the zone placement data for the objective text
            objectiveLayerData.ObjectiveData.ZonePlacementDatas.Add(
                new List<ZonePlacementData>
                {
                    new ZonePlacementData
                    {
                        LocalIndex = mwp.ZoneNumber,
                        Weights = ZonePlacementWeights.NotAtStart
                    }
                });

            // Ensure there's a nice spicy hoard at the end, also include bosses for harder levels
            // You best stealth this zone!
            switch (level.Tier)
            {
                case "A":
                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.TierA with { Points = 10 });
                    break;

                case "B":
                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.TierB with { Points = 20 });
                    break;

                case "C":
                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.TierC with { Points = 30 });
                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Striker with { Points = 20 });

                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Mother with { Points = 10 });
                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Baby with { Points = 10 });
                    break;

                case "D":
                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.TierD with { Points = 30 });
                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Striker with { Points = 30 });

                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Tank with { Points = 10 });
                    break;

                case "E":
                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.TierE with { Points = 40 });
                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Striker with { Points = 30 });

                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.PMother with { Points = 10 });
                    mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Baby with { Points = 10 });
                    break;
            }
        }
    }
}
