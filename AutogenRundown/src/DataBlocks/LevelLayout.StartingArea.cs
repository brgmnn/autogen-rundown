using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks
{
    public partial record LevelLayout
    {
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
            var bulkheadNode = new ZoneNode(bulkhead, level.Planner.NextIndex(bulkhead));
            level.Planner.Connect(from, bulkheadNode);

            level.Planner.AddZone(
                bulkheadNode,
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
                level.MainBulkheadZone = bulkheadNode.ZoneNumber;
                level.MainLayerData.ZonesWithBulkheadEntrance.Add(bulkheadNode.ZoneNumber);
                Plugin.Logger.LogInfo($"Level={level.Tier}{level.Index} - Main Bulkhead Entrance = {from}");
            }
        }

        /// <summary>
        /// Applicable for Main bulkhead, build the main level.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="director"></param>
        public void BuildStartingArea(Level level, BuildDirector director)
        {
            // Return early if we should not be building this.
            if (!director.Bulkhead.HasFlag(Bulkhead.Main))
                return;

            // Bulkheads that need to be placed
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

            // What bulkheads this level has
            var bulkheads = level.Settings.Bulkheads;

            // Options for starting areas
            // TODO: add default in always as an option after we test
            //var options = new List<(double, string)> { (1.0, "default") };
            var options = new List<(double, string)>();

            // 2 bulkhead objectives
            if (bulkheads.HasFlag(Bulkhead.Extreme) ^ bulkheads.HasFlag(Bulkhead.Overload))
                options.Add((2.0, "2x_bulkhead_hub"));
            // All 3 bulkhead objectives
            else if (bulkheads.HasFlag(Bulkhead.PrisonerEfficiency))
                options.Add((2.0, "3x_bulkhead_hub"));
            // Any other option
            else
                options.Add((1.0, "default"));

            var strategy = Generator.Select(options);
            Plugin.Logger.LogDebug($"StartingArea strategy = {strategy}");

            switch (strategy)
            {
                case "2x_bulkhead_hub":
                {
                    BuildStartingArea_2xBulkheadHub(level, director);
                    break;
                }

                case "3x_bulkhead_hub":
                {
                    BuildStartingArea_3xBulkheadHub(level, director);
                    break;
                }

                default:
                {
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

                    var prev = elevatorDrop;
                    Zone nextZone = elevatorDropZone;

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

                    break;
                }
            }

            // Add a fog turbine to the last start area if we have fog
            if (level.Settings.Modifiers.Contains(LevelModifiers.Fog) ||
                level.Settings.Modifiers.Contains(LevelModifiers.HeavyFog))
            {
                var lastNode = level.Planner.GetZones(Bulkhead.StartingArea).Last();
                var lastZone = level.Planner.GetZone(lastNode)!;

                lastZone.BigPickupDistributionInZone = BigPickupDistribution.FogTurbine.PersistentId;
            }
        }

        /**
         * Generates compact starting areas for 2x bulkhead entrances from the same zone.
         */
        private void BuildStartingArea_2xBulkheadHub(Level level, BuildDirector director)
        {
            // Add the first zone.
            var elevatorDrop = new ZoneNode(
                Bulkhead.Main | Bulkhead.StartingArea,
                level.Planner.NextIndex(Bulkhead.Main));
            var elevatorDropZone = new Zone
            {
                Coverage = new CoverageMinMax { Min = 25, Max = 35 },
                LightSettings = Lights.GenRandomLight(),
                EnemyPointsMultiplier = 0.6
            };

            switch (level.Complex)
            {
                case Complex.Mining:
                case Complex.Tech:
                case Complex.Service:
                    elevatorDropZone.GenHubGeomorph(level.Complex);
                    break;
            }

            level.Planner.Connect(elevatorDrop);
            level.Planner.AddZone(elevatorDrop, elevatorDropZone);

            var secondBulkhead = level.Settings.Bulkheads.HasFlag(Bulkhead.Extreme) ?
                Bulkhead.Extreme : Bulkhead.Overload;

            var sensorChance = this.level.Tier switch
            {
                "A" => 0.00,
                "B" => 0.05,
                "C" => 0.15,
                "D" => 0.45,
                "E" => 0.70,
                _ => 0.0
            };

            if (Generator.Flip(sensorChance))
                AddSecuritySensors_SinglePouncer((0, 1));

            // Place both bulkheads in the first zone
            InitializeBulkheadArea(level, Bulkhead.Main, elevatorDrop);
            InitializeBulkheadArea(level, secondBulkhead, elevatorDrop);
        }

        /**
         * Generates compact starting areas for 3x bulkhead entrances from the same zone.
         */
        private void BuildStartingArea_3xBulkheadHub(Level level, BuildDirector director)
        {
            // Add the first zone.
            var elevatorDrop = new ZoneNode(
                Bulkhead.Main | Bulkhead.StartingArea,
                level.Planner.NextIndex(Bulkhead.Main));
            var elevatorDropZone = new Zone
            {
                Coverage = new CoverageMinMax { Min = 25, Max = 35 },
                LightSettings = Lights.GenRandomLight(),
                EnemyPointsMultiplier = 0.5
            };

            switch (level.Complex)
            {
                case Complex.Mining:
                {
                    if (Generator.Flip(0.5))
                    {
                        elevatorDropZone.SubComplex = SubComplex.DigSite;
                        elevatorDropZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_SF_01.prefab";
                        elevatorDropZone.Coverage = new CoverageMinMax { Min = 20, Max = 35 };

                        elevatorDropZone.SetOutOfFog(level);
                    }
                    else
                    {
                        elevatorDropZone.GenHubGeomorph(level.Complex);
                    }

                    break;
                }

                case Complex.Tech:
                case Complex.Service:
                    elevatorDropZone.GenHubGeomorph(level.Complex);
                    break;
            }

            level.Planner.Connect(elevatorDrop);
            level.Planner.AddZone(elevatorDrop, elevatorDropZone);

            var sensorChance = this.level.Tier switch
            {
                "A" => 0.00,
                "B" => 0.05,
                "C" => 0.15,
                "D" => 0.45,
                "E" => 0.70,
                _ => 0.0
            };

            if (Generator.Flip(sensorChance))
                AddSecuritySensors_SinglePouncer((0, 1));

            // Place all bulkheads in the first zone
            InitializeBulkheadArea(level, Bulkhead.Main, elevatorDrop);
            InitializeBulkheadArea(level, Bulkhead.Extreme, elevatorDrop);
            InitializeBulkheadArea(level, Bulkhead.Overload, elevatorDrop);
        }
    }
}
