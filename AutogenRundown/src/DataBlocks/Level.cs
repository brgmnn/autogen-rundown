using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutogenRundown.GeneratorData;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;
using UnityEngine.Experimental.AI;

namespace AutogenRundown.DataBlocks
{
    public class BuildFrom
    {
        public int LayerType { get; set; } = 0;
        public int Zone { get; set; } = 0;
    }

    internal class Level
    {
        #region Filler settings that won't change
        public bool Enabled = true;
        public bool IsSinglePlayer = false;
        public bool SkipLobby = false;
        public bool PutIconAboveTier = false;
        public bool DisablePlayerVoicelines = false;
        public bool ExcludeFromProgression = false;
        public bool HideOnLocked = false;
        public bool HasExternalStyle = false;
        public bool HasStoryStyle = false;
        public bool UseGearPicker = false;
        public JArray DimensionDatas = new JArray();
        public int SoundEventOnWarpToReality = 0;
        #endregion

        /// <summary>
        /// Level name
        /// </summary>
        [JsonIgnore]
        public string Name { get; set; } = "";

        /// <summary>
        /// Level Tier, roughly difficulty
        /// </summary>
        [JsonIgnore]
        public string Tier { get; set; } = "A";

        /// <summary>
        /// Level index (A1, A2, A3, etc)
        /// </summary>
        [JsonIgnore]
        public int Index { get; set; } = 1;

        /// <summary>
        /// Level depth in meters
        /// </summary>
        [JsonIgnore]
        public int Depth { get; set; } = 1;

        /// <summary>
        /// Which complex type to use.
        /// 
        /// By default set to a random value from the available complexes. Weight more towards
        /// mining and tech.
        /// </summary>
        [JsonIgnore]
        public Complex Complex { get; set; } = Generator.Pick(
            new List<Complex>
            {
                Complex.Mining,
                Complex.Mining,
                Complex.Tech,
                Complex.Tech,
                Complex.Service
            });

        [JsonIgnore]
        public BuildDirector? MainDirector { get; set; }

        [JsonIgnore]
        public BuildDirector? SecondaryDirector { get; set; }

        [JsonIgnore]
        public BuildDirector? OverloadDirector { get; set; }

        [JsonIgnore]
        public LayoutPlanner Planner { get; set; } = new LayoutPlanner();

        /// <summary>
        /// Level description
        /// </summary>
        [JsonIgnore]
        public string Description { get; set; } = "";

        /// <summary>
        /// Flags the level as a test level
        /// </summary>
        [JsonIgnore]
        public bool IsTest { get; set; } = false;

        public JObject Descriptive
        {
            get => new JObject
            {
                ["Prefix"] = IsTest ? "TEST" : Tier,
                ["PublicName"] = Name,
                ["IsExtraExpedition"] = false,
                ["SkipExpNumberInName"] = IsTest,
                ["UseCustomMatchmakingTier"] = false,
                ["CustomMatchmakingTier"] = 1,
                ["ProgressionVisualStyle"] = IsTest ? 1 : 0,
                ["ExpeditionDepth"] = Depth,
                ["EstimatedDuration"] = 930,

                // Description shows up in menu
                ["ExpeditionDescription"] = Description,

                // Warden intel displays during drop
                ["RoleplayedWardenIntel"] = 1426
            };
        }

        public JObject Seeds = JObject.FromObject(new
        {
            BuildSeed = Generator.Random.Next(2000),
            FunctionMarkerOffset = 1,
            StandardMarkerOffset = 0,
            LightJobSeedOffset = 0
        });

        public JObject Expedition
        {
            get => new JObject
            {
                ["ComplexResourceData"] = (int)Complex,
                ["MLSLevelKit"] = 0,
                ["LightSettings"] = 36,
                ["FogSettings"] = 139,
                ["EnemyPopulation"] = 1,
                ["ExpeditionBalance"] = 1,
                ["ScoutWaveSettings"] = 3,
                ["ScoutWavePopulation"] = 1,
                ["EnvironmentWetness"] = 0.0,
                ["DustColor"] = JObject.FromObject(
                    new Color { Alpha = 1.0, Red = 0.5, Green = 0.5, Blue = 0.5 }),
                ["DustTurbulence"] = 1.0
            };
        }

        public JObject VanityItemsDropData = new JObject
        {
            ["Groups"] = new JArray()
        };

        /// <summary>
        /// Match this to the persistent ID of the Level Layout
        /// </summary>
        public UInt32 LevelLayoutData { get; set; } = 0;

        #region Main Objective Data
        /// <summary>
        /// All levels must have a main objective
        /// </summary>
        public ObjectiveLayerData MainLayerData { get; set; } = new ObjectiveLayerData();
        #endregion

        #region Secondary (Extreme) Objective Data
        /// <summary>
        /// Secondary (Extreme) objectives data
        /// </summary>
        public ObjectiveLayerData SecondaryLayerData { get; set; } = new ObjectiveLayerData();

        public bool SecondaryLayerEnabled { get; set; } = false;

        public UInt32 SecondaryLayout = 0;

        public BuildFrom BuildSecondaryFrom = new BuildFrom
        {
            LayerType = 0,
            Zone = 0
        };
        #endregion

        #region Third (Overload) Objective Data
        /// <summary>
        /// Third (Overload) objectives data
        /// </summary>
        public ObjectiveLayerData ThirdLayerData { get; set; } = new ObjectiveLayerData();

        public bool ThirdLayerEnabled { get; set; } = false;

        public UInt32 ThirdLayout = 0;

        public BuildFrom BuildThirdFrom = new BuildFrom
        {
            LayerType = 0,
            Zone = 0
        };
        #endregion

        #region Modifiers
        [JsonIgnore]
        public double StartingInfection { get; set; } = 0.0;

        [JsonIgnore]
        public double StartingHealth { get; set; } = 1.0;

        [JsonIgnore]
        public double StartingMainAmmo { get; set; } = 1.0;

        [JsonIgnore]
        public double StartingSpecialAmmo { get; set; } = 1.0;

        [JsonIgnore]
        public double StartingTool { get; set; } = 1.0;

        /// <summary>
        /// Additional override data JSON encoding
        /// </summary>
        public JObject SpecialOverrideData
        {
            get => new JObject
            {
                ["WeakResourceContainerWithPackChanceForLocked"] = -1.0,
                ["InfectionLevelAtExpeditionStart"] = StartingInfection,
                ["HealthLevelAtExpeditionStart"] = StartingHealth,
                ["StandardAmmoAtExpeditionStart"] = StartingMainAmmo,
                ["SpecialAmmoAtExpeditionStart"] = StartingSpecialAmmo,
                ["ToolAmmoAtExpeditionStart"] = StartingTool
            };
        }
        #endregion

        /// <summary>
        /// Generates a random depth for the level based on the Tier
        /// </summary>
        private void GenerateDepth()
        {
            switch (Tier)
            {
                case "A":
                    Depth = Generator.Random.Next(420, 650);
                    break;
                case "B":
                    Depth = Generator.Random.Next(600, 850);
                    break;
                case "C":
                    Depth = Generator.Random.Next(800, 1000);
                    break;
                case "D":
                    Depth = Generator.Random.Next(900, 1100);
                    break;
                case "E":
                    Depth = Generator.Random.Next(950, 1500);
                    break;
            }
        }

        /// <summary>
        /// Gets the right layer data given the objective being asked for
        /// </summary>
        /// <param name="variant"></param>
        /// <returns></returns>
        public ObjectiveLayerData GetObjectiveLayerData(Bulkhead variant)
        {
            switch (variant)
            {
                case Bulkhead.Main:
                    return MainLayerData;
                case Bulkhead.Extreme:
                    return SecondaryLayerData;
                case Bulkhead.Overload:
                    return ThirdLayerData;
                default:
                    return MainLayerData;
            }
        }

        public LevelLayout? GetLevelLayout(Bulkhead variant)
        {
            switch (variant)
            {
                case Bulkhead.Main:
                    return Bins.LevelLayouts.Find(LevelLayoutData);
                case Bulkhead.Extreme:
                    return Bins.LevelLayouts.Find(SecondaryLayout);
                case Bulkhead.Overload:
                    return Bins.LevelLayouts.Find(ThirdLayout);
                default:
                    return Bins.LevelLayouts.Find(LevelLayoutData);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public Level Build(Level level)
        {
            if (level.Name == "")
            {
                level.Name = Generator.Pick(Words.NounsLevel) ?? "";
            }

            level.GenerateDepth();

            // Randomly select which bulkheads to use
            var selectedBulkheads = Generator.Select(new List<(double, Bulkhead)>
            {
                (0.25, Bulkhead.Main),
                (0.4, Bulkhead.Main | Bulkhead.Extreme),
                (0.1, Bulkhead.Main | Bulkhead.Overload),
                (0.25, Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload)
            });
            var bulkheadPlacements = Generator.Select(new List<(double, string)>
            {
                (1.0, "random")
            });

            /**
             * Options for bulkhead keys and bulkhead DCs:
             * 
             *  * All unlocked: main, extreme, overload
             *    Each bulkhead is unlocked, no keys required
             *    
             *  * Chained: main -> extreme -> overload -> END
             *    Each bulkhead is locked behind the previous one with one key in each.
             *    Main has extreme DC, extreme has overload DC.
             *    
             *  * Choice: main -> overload -> END
             *                 -> extreme -> overload -> END
             *    Main has extreme/overload DC, extreme has extra key for overload DC.
             */
            var bulkheadKeys = Generator.Select(new List<(double, string)>
            {
                (1.0, "chained")
            });

            // ============ Main level ============
            #region Main
            if (level.MainDirector == null)
            {
                level.MainDirector = new BuildDirector
                {
                    Bulkhead = Bulkhead.Main,
                    Complex = level.Complex,
                    Complexity = Complexity.Low,
                    Tier = level.Tier
                };
                level.MainDirector.GenPoints();
                level.MainDirector.GenObjective();
            }

            var mainLevelLayout = LevelLayout.Build(level, level.MainDirector);
            level.LevelLayoutData = mainLevelLayout.PersistentId;

            var mainObjective = WardenObjective.Build(level.MainDirector, level);
            level.MainLayerData.ObjectiveData.DataBlockId = mainObjective.PersistentId;

            if (level.MainDirector.Objective == WardenObjectiveType.ClearPath)
            {
                level.MainLayerData.ObjectiveData.WinCondition = WardenObjectiveWinCondition.GoToElevator;
            }

            var openZones = level.Planner.GetOpenZones(Bulkhead.Main);
            var min = selectedBulkheads switch
            {
                Bulkhead.Main => 1,
                Bulkhead.Main | Bulkhead.Extreme => 2,
                Bulkhead.Main | Bulkhead.Overload => 2,
                Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload => 3,
                _ => 1
            };

            // Main bulkhead zone cannot spawn in zone 0. Otherwise we would have zero zones to
            // place other bulkheads
            var index = 1;
            while (level.Planner.CountOpenSlots(openZones.Take(index)) < min)
            {
                index++;
            }

            var mainBulkheadZone = Generator.Pick(openZones.TakeLast(openZones.Count - index));

            Plugin.Logger.LogInfo($"Level={level.Tier}{level.Index} - Main Bulkhead Zone = {mainBulkheadZone.ZoneNumber}");

            level.MainLayerData.ZonesWithBulkheadEntrance.Add(mainBulkheadZone.ZoneNumber);
            level.MainLayerData.BulkheadDoorControllerPlacements.Add(
                new BulkheadDoorPlacementData()
                {
                    ZoneIndex = mainBulkheadZone.ZoneNumber - 1,
                    PlacementWeights = ZonePlacementWeights.None,
                });

            Bins.WardenObjectives.AddBlock(mainObjective);
            #endregion

            // Secondary (Extreme)
            #region Secondary
            if (selectedBulkheads.HasFlag(Bulkhead.Extreme))
            {
                if (level.SecondaryDirector == null)
                {
                    level.SecondaryDirector = new BuildDirector
                    {
                        Bulkhead = Bulkhead.Extreme,
                        Complex = level.Complex,
                        Complexity = Complexity.Low,
                        Tier = level.Tier
                    };
                    level.SecondaryDirector.GenPoints();
                    level.SecondaryDirector.GenObjective();
                }

                level.SecondaryLayerEnabled = true;

                var entranceZone = level.Planner.GetOpenZones(Bulkhead.Main)
                    .Where(zone => zone.ZoneNumber < mainBulkheadZone.ZoneNumber)
                    .OrderBy(zone => zone.ZoneNumber)
                    .Last();

                Plugin.Logger.LogInfo($"Level={level.Tier}{level.Index} - Extreme entrance Zone = {entranceZone.ZoneNumber}");

                level.BuildSecondaryFrom = new BuildFrom
                {
                    LayerType = 0,
                    Zone = entranceZone.ZoneNumber
                };
                level.Planner.Connect(entranceZone, new ZoneNode(Bulkhead.Extreme, 0));

                if (level.MainLayerData.BulkheadDoorControllerPlacements
                        .Where(placement => placement.ZoneIndex == entranceZone.ZoneNumber)
                        .Count() == 0)
                {
                    Plugin.Logger.LogInfo($"Level={level.Tier}{level.Index} - Extreme Bulkhead DC added, Zone = {entranceZone.ZoneNumber}");
                    level.MainLayerData.BulkheadDoorControllerPlacements.Add(
                        new BulkheadDoorPlacementData()
                        {
                            ZoneIndex = entranceZone.ZoneNumber,
                            PlacementWeights = ZonePlacementWeights.None,
                        });
                }

                // Actually create the layout
                var extremeLevelLayout = LevelLayout.Build(level, level.SecondaryDirector);
                level.SecondaryLayout = extremeLevelLayout.PersistentId;

                var extremeObjective = WardenObjective.Build(level.SecondaryDirector, level);
                level.SecondaryLayerData.ObjectiveData.DataBlockId = extremeObjective.PersistentId;

                Bins.WardenObjectives.AddBlock(extremeObjective);
            }
            #endregion

            // Tertiary (Overload)
            #region Third
            if (selectedBulkheads.HasFlag(Bulkhead.Overload))
            {
                if (level.OverloadDirector == null)
                {
                    level.OverloadDirector = new BuildDirector
                    {
                        Bulkhead = Bulkhead.Overload,
                        Complex = level.Complex,
                        Complexity = Complexity.Low,
                        Tier = level.Tier
                    };
                    level.OverloadDirector.GenPoints();
                    level.OverloadDirector.GenObjective();
                }

                level.ThirdLayerEnabled = true;

                var entranceZone = level.Planner.GetOpenZones(Bulkhead.Main)
                    .Where(zone => zone.ZoneNumber < mainBulkheadZone.ZoneNumber)
                    .OrderBy(zone => zone.ZoneNumber)
                    .Last();

                Plugin.Logger.LogInfo($"Level={level.Tier}{level.Index} - Overload entrance Zone = {entranceZone.ZoneNumber}");

                level.BuildThirdFrom = new BuildFrom
                {
                    LayerType = 0,
                    Zone = entranceZone.ZoneNumber
                };
                level.Planner.Connect(entranceZone, new ZoneNode(Bulkhead.Overload, 0));

                if (level.MainLayerData.BulkheadDoorControllerPlacements
                        .Where(placement => placement.ZoneIndex == entranceZone.ZoneNumber)
                        .Count() == 0)
                {
                    level.MainLayerData.BulkheadDoorControllerPlacements.Add(
                        new BulkheadDoorPlacementData()
                        {
                            ZoneIndex = entranceZone.ZoneNumber,
                            PlacementWeights = ZonePlacementWeights.None,
                        });
                    Plugin.Logger.LogDebug($"Level={level.Tier}{level.Index} - Overload Bulkhead DC added, Zone={entranceZone.ZoneNumber}");
                }

                // Actually create the layout
                var overloadLevelLayout = LevelLayout.Build(level, level.OverloadDirector);
                level.ThirdLayout = overloadLevelLayout.PersistentId;

                var overloadObjective = WardenObjective.Build(level.OverloadDirector, level);
                level.ThirdLayerData.ObjectiveData.DataBlockId = overloadObjective.PersistentId;

                Bins.WardenObjectives.AddBlock(overloadObjective);
            }
            #endregion

            // Place bulkhead DCs
            #region Bulkhead DCs and Keys
            level.MainLayerData.BulkheadKeyPlacements.Add(
                new List<ZonePlacementData>
                {
                    new ZonePlacementData { LocalIndex = 0, Weights = ZonePlacementWeights.NotAtStart }
                });

            if (selectedBulkheads.HasFlag(Bulkhead.Extreme))
                level.SecondaryLayerData.BulkheadKeyPlacements.Add(
                    new List<ZonePlacementData>
                    {
                        new ZonePlacementData { LocalIndex = 0, Weights = ZonePlacementWeights.NotAtStart }
                    });

            if (selectedBulkheads.HasFlag(Bulkhead.Overload))
                level.ThirdLayerData.BulkheadKeyPlacements.Add(
                    new List<ZonePlacementData>
                    {
                        new ZonePlacementData { LocalIndex = 0, Weights = ZonePlacementWeights.NotAtStart }
                    });
            #endregion

            #region Adjust coverage sizes
            var entrances = level.Planner.GetBulkheadEntranceZones();

            foreach (var node in entrances)
            {
                var zone = level.GetLevelLayout(node.Bulkhead)?.Zones[node.ZoneNumber];

                if (zone == null)
                {
                    Plugin.Logger.LogWarning($"Level={level.Tier}{level.Index} - Cannot resize " +
                        $"bulkhead entrance zone. Zone_{node.ZoneNumber} could not be found.");
                    continue;
                }

                // TODO: rebalance enemies
                // TODO: skip this if it's a custom geomorph
                zone.Coverage = new CoverageMinMax { Min = 80.0, Max = 80.0 };
            }
            #endregion

            return level;
        }
    }
}
