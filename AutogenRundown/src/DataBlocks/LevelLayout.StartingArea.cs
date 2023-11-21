using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks
{
    public partial record class LevelLayout : DataBlock
    {
        public const double StartArea_TripleBulkheadChance = 0.2;

        /// <summary>
        /// Connects the first bulkhead zone to a zone in this layout
        /// </summary>
        /// <param name="level"></param>
        /// <param name="bulkhead"></param>
        /// <param name="from"></param>
        public static void InitializeBulkheadArea(
            Level level,
            Bulkhead bulkhead,
            ZoneNode from,
            Zone? zone = null)
        {
            var bulkheadZone = new ZoneNode(bulkhead, level.Planner.NextIndex(bulkhead));
            level.Planner.Connect(from, bulkheadZone);

            level.Planner.AddZone(
                bulkheadZone,
                zone ?? new Zone
                {
                    Coverage = CoverageMinMax.GenNormalSize(),
                    LightSettings = Lights.GenRandomLight(),
                });

            // Determine the correct from layer type. This is an int corresponding to the from
            // zone bulkhead type
            var layerType = from.Bulkhead switch
            {
                Bulkhead.Extreme => 1,
                Bulkhead.Overload => 2,
                _ => 0,
            };

            // Ensure a Bulkhead DC is placed in the from zone.
            var layerData = level.GetObjectiveLayerData(from.Bulkhead);

            if (layerData.BulkheadDoorControllerPlacements
                .Where(dc => dc.ZoneIndex == from.ZoneNumber)
                .Count() == 0)
            {
                layerData.BulkheadDoorControllerPlacements.Add(
                    new BulkheadDoorPlacementData()
                    {
                        ZoneIndex = from.ZoneNumber,
                        PlacementWeights = ZonePlacementWeights.NotAtStart
                    });
            }

            // Mark the correct zones as bulkhead zone for main, as well as setting the right build
            // from parameter.
            if (bulkhead.HasFlag(Bulkhead.Extreme))
            {
                level.BuildSecondaryFrom = new BuildFrom { LayerType = layerType, Zone = from.ZoneNumber };
                Plugin.Logger.LogInfo($"Level={level.Tier}{level.Index} - Extreme Bulkhead Entrance = {from}");
            }
            else if (bulkhead.HasFlag(Bulkhead.Overload))
            {
                level.BuildThirdFrom = new BuildFrom { LayerType = layerType, Zone = from.ZoneNumber };
                Plugin.Logger.LogInfo($"Level={level.Tier}{level.Index} - Overload Bulkhead Entrance = {from}");
            }
            else
            {
                level.MainBulkheadZone = bulkheadZone.ZoneNumber;
                level.MainLayerData.ZonesWithBulkheadEntrance.Add(bulkheadZone.ZoneNumber);
                Plugin.Logger.LogInfo($"Level={level.Tier}{level.Index} - Main Bulkhead Entrance = {from}");
            }
        }

        /// <summary>
        /// Applicable for Main bulkhead, build the main level.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="director"></param>
        public static void BuildStartingArea(Level level, BuildDirector director)
        {
            // Return early if we should not be building this.
            if (!director.Bulkhead.HasFlag(Bulkhead.Main))
                return;

            // Add the first zone.
            var elevatorDrop = new ZoneNode(
                Bulkhead.Main | Bulkhead.StartingArea,
                level.Planner.NextIndex(Bulkhead.Main));
            var elevatorDropZone = new Zone
            {
                Coverage = new CoverageMinMax { Min = 25, Max = 35 },
                LightSettings = Lights.GenRandomLight(),
            };

            // Add fog repellers in the first zone if there's fog in the level.
            if (level.Settings.Modifiers.Contains(LevelModifiers.Fog) ||
                level.Settings.Modifiers.Contains(LevelModifiers.HeavyFog))
                elevatorDropZone.ConsumableDistributionInZone
                    = ConsumableDistribution.Baseline_FogRepellers.PersistentId;

            level.Planner.Connect(elevatorDrop);
            level.Planner.AddZone(elevatorDrop, elevatorDropZone);

            var minimumZones = level.Settings.Bulkheads switch
            {
                Bulkhead.Main => 0,
                Bulkhead.Main | Bulkhead.Extreme => 1,
                Bulkhead.Main | Bulkhead.Overload => 1,
                Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload => 2,
                _ => 1
            };

            var toPlace = level.Settings.Bulkheads switch
            {
                Bulkhead.Main => new List<Bulkhead> { Bulkhead.Main },
                Bulkhead.Main | Bulkhead.Extreme => new List<Bulkhead>
                {
                    Bulkhead.Main,
                    Bulkhead.Extreme
                },
                Bulkhead.Main | Bulkhead.Overload => new List<Bulkhead>
                {
                    Bulkhead.Main,
                    Bulkhead.Overload
                },
                Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload => new List<Bulkhead>
                {
                    Bulkhead.Main,
                    Bulkhead.Extreme,
                    Bulkhead.Overload
                },

                _ => new List<Bulkhead>()
            };

            var prev = elevatorDrop;
            Zone nextZone = elevatorDropZone;

            if (level.Settings.Bulkheads.HasFlag(Bulkhead.Main) &&
                level.Settings.Bulkheads.HasFlag(Bulkhead.Extreme) &&
                level.Settings.Bulkheads.HasFlag(Bulkhead.Overload) &&
                Generator.Flip(StartArea_TripleBulkheadChance))
            {
                var zoneIndex = level.Planner.NextIndex(Bulkhead.Main);
                var hubsf = new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, zoneIndex);
                nextZone = new Zone
                {
                    LightSettings = Lights.GenRandomLight(),
                    SubComplex = SubComplex.DigSite,
                    CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_SF_01.prefab",
                    Coverage = new CoverageMinMax { Min = 20, Max = 35 },
                };
                nextZone.SetOutOfFog(level);

                level.Planner.Connect(prev, hubsf);
                nextZone = level.Planner.AddZone(hubsf, nextZone);

                // Place all three build from bulkhead zones from the hub sf tile. There should
                // always be 3 to draw from.
                InitializeBulkheadArea(level, Generator.Draw(toPlace), hubsf);
                InitializeBulkheadArea(level, Generator.Draw(toPlace), hubsf);
                InitializeBulkheadArea(level, Generator.Draw(toPlace), hubsf);
            }
            else
            {
                for (int i = 0; i < minimumZones; i++)
                {
                    var zoneIndex = level.Planner.NextIndex(Bulkhead.Main);
                    var next = new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, zoneIndex);
                    nextZone = new Zone
                    {
                        Coverage = CoverageMinMax.GenNormalSize(),
                        LightSettings = Lights.GenRandomLight(),
                    };
                    nextZone.RollFog(level);

                    level.Planner.Connect(prev, next);
                    nextZone = level.Planner.AddZone(next, nextZone);

                    // Place the first zones of the connecting bulkhead zones, so we can build from
                    // them later.
                    InitializeBulkheadArea(level, Generator.Draw(toPlace), next);

                    prev = next;
                }

                // The final area also needs to be placed
                InitializeBulkheadArea(level, Generator.Draw(toPlace), prev);
            }

            // Add a fog turbine to the last start area if we have fog
            if (level.Settings.Modifiers.Contains(LevelModifiers.Fog) ||
                level.Settings.Modifiers.Contains(LevelModifiers.HeavyFog))
                nextZone.BigPickupDistributionInZone = BigPickupDistribution.FogTurbine.PersistentId;
        }
    }
}
