using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.GeneratorData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    public class BuildFrom
    {
        public int LayerType { get; set; } = 0;
        public int Zone { get; set; } = 0;
    }

    public class Level
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

        #region Internal settings for us
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
        public LayoutPlanner Planner { get; set; } = new LayoutPlanner();

        [JsonIgnore]
        public LevelSettings Settings { get; set; }

        [JsonIgnore]
        public List<RelativeDirection> RelativeDirections { get; set; } = new List<RelativeDirection>
            {
                RelativeDirection.Global_Forward,
                RelativeDirection.Global_Left,
                RelativeDirection.Global_Right,
                RelativeDirection.Global_Backward
            };

        /// <summary>
        /// What zone does Main start with
        /// </summary>
        [JsonIgnore]
        public int ZoneAliasStart_Main { get; set; } = 0;

        /// <summary>
        /// What zone does Extreme start with
        /// </summary>
        [JsonIgnore]
        public int ZoneAliasStart_Extreme { get; set; } = 0;

        /// <summary>
        /// What zone does Overload start with
        /// </summary>
        [JsonIgnore]
        public int ZoneAliasStart_Overload { get; set; } = 0;

        public int GetZoneAliasStart(Bulkhead bulkhead)
            => bulkhead switch
            {
                Bulkhead.Main => ZoneAliasStart_Main,
                Bulkhead.Extreme => ZoneAliasStart_Extreme,
                Bulkhead.Overload => ZoneAliasStart_Overload,
                _ => 0,
            };
        #endregion

        #region Build directors
        [JsonIgnore]
        /// <summary>
        /// Allows easy access to the directors without having to switch
        /// </summary>
        public Dictionary<Bulkhead, BuildDirector> Director { get; private set; } = new Dictionary<Bulkhead, BuildDirector>();

        [JsonIgnore]
        public BuildDirector MainDirector
        {
            get => Director[Bulkhead.Main];
            set => Director[Bulkhead.Main] = value;
        }

        [JsonIgnore]
        public BuildDirector SecondaryDirector
        {
            get => Director[Bulkhead.Extreme];
            set => Director[Bulkhead.Extreme] = value;
        }

        [JsonIgnore]
        public BuildDirector OverloadDirector
        {
            get => Director[Bulkhead.Overload];
            set => Director[Bulkhead.Overload] = value;
        }
        #endregion

        #region Layout and layer data
        [JsonIgnore]
        /// <summary>
        /// Allows easy access to the directors without having to switch
        /// </summary>
        public Dictionary<Bulkhead, ObjectiveLayerData> ObjectiveLayer { get; private set; }
            = new Dictionary<Bulkhead, ObjectiveLayerData>
            {
                { Bulkhead.Main, new ObjectiveLayerData() },
                { Bulkhead.Extreme, new ObjectiveLayerData() },
                { Bulkhead.Overload, new ObjectiveLayerData() }
            };

        [JsonIgnore]
        /// <summary>
        /// Allows easy access to the directors without having to switch
        /// </summary>
        public Dictionary<Bulkhead, uint> LayoutRef { get; private set; }
            = new Dictionary<Bulkhead, uint>
            {
                { Bulkhead.Main, 0 },
                { Bulkhead.Extreme, 0 },
                { Bulkhead.Overload, 0 }
            };
        #endregion

        /// <summary>
        /// Which zone the main bulkhead door gates. Often we want objectives to be spawned here
        /// or later.
        /// </summary>
        [JsonIgnore]
        public int MainBulkheadZone { get; set; } = 0;

        /// <summary>
        /// Level description
        /// </summary>
        [JsonIgnore]
        public string Description { get; set; } = "";

        [JsonIgnore]
        public Fog FogSettings { get; set; } = Fog.DefaultFog;

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
                ["FogSettings"] = FogSettings.PersistentId,
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

        public JObject VanityItemsDropData = new JObject { ["Groups"] = new JArray() };

        #region Main Objective Data
        /// <summary>
        /// Match this to the persistent ID of the Level Layout
        /// </summary>
        public uint LevelLayoutData
        {
            get => LayoutRef[Bulkhead.Main];
            set => LayoutRef[Bulkhead.Main] = value;
        }

        /// <summary>
        /// All levels must have a main objective
        /// </summary>
        public ObjectiveLayerData MainLayerData
        {
            get => ObjectiveLayer[Bulkhead.Main];
            set => ObjectiveLayer[Bulkhead.Main] = value;
        }
        #endregion

        #region Secondary (Extreme) Objective Data
        /// <summary>
        /// Secondary (Extreme) objectives data
        /// </summary>
        public ObjectiveLayerData SecondaryLayerData
        {
            get => ObjectiveLayer[Bulkhead.Extreme];
            set => ObjectiveLayer[Bulkhead.Extreme] = value;
        }

        public bool SecondaryLayerEnabled
        {
            get => SecondaryLayout > 0;
            private set { }
        }

        public uint SecondaryLayout
        {
            get => LayoutRef[Bulkhead.Extreme];
            set => LayoutRef[Bulkhead.Extreme] = value;
        }

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
        public ObjectiveLayerData ThirdLayerData
        {
            get => ObjectiveLayer[Bulkhead.Overload];
            set => ObjectiveLayer[Bulkhead.Overload] = value;
        }

        public bool ThirdLayerEnabled
        {
            get => ThirdLayout > 0;
            private set { }
        }

        public uint ThirdLayout
        {
            get => LayoutRef[Bulkhead.Overload];
            set => LayoutRef[Bulkhead.Overload] = value;
        }

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
        /// Generates the zone alias start numbers, tries to ensure there will be no collisions.
        /// </summary>
        private void GenerateZoneAliasStarts()
        {
            var minmax = Tier switch
            {
                "B" => new List<(int, int)>
                {
                    ( 50, 170),  // 250 spread
                    (200, 275),
                    (300, 450),
                },

                "C" => new List<(int, int)>
                {
                    (200, 275), // 400 spread
                    (300, 475),
                    (500, 600),
                },

                "D" => new List<(int, int)>
                {
                    (300, 475), // 550 spread
                    (500, 680),
                    (700, 850),
                },

                "E" => new List<(int, int)>
                {
                    (450, 570), // 500 spread
                    (600, 750),
                    (790, 950),
                },

                // A-Tier: 195 spread
                _ => new List<(int, int)>
                {
                    (  5, 70),
                    ( 95, 135),
                    (160, 200),
                }
            };

            var (min, max) = Generator.Draw(minmax);
            ZoneAliasStart_Main = Generator.Random.Next(min, max);

            (min, max) = Generator.Draw(minmax);
            ZoneAliasStart_Extreme = Generator.Random.Next(min, max);

            (min, max) = Generator.Draw(minmax);
            ZoneAliasStart_Overload = Generator.Random.Next(min, max);
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

        public WardenObjective? GetObjective(Bulkhead variant)
        {
            return variant switch
            {
                Bulkhead.Main => Bins.WardenObjectives.Find(MainLayerData.ObjectiveData.DataBlockId),
                Bulkhead.Extreme => Bins.WardenObjectives.Find(SecondaryLayerData.ObjectiveData.DataBlockId),
                Bulkhead.Overload => Bins.WardenObjectives.Find(ThirdLayerData.ObjectiveData.DataBlockId),
                Bulkhead.None => throw new NotImplementedException(),
                Bulkhead.StartingArea => throw new NotImplementedException(),
                Bulkhead.All => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
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
        /// Builds a specific bulkhead layout
        /// </summary>
        /// <param name="bulkhead"></param>
        public void BuildLayout(Bulkhead bulkhead)
        {
            var existing = new List<WardenObjectiveType>();

            if (Director.ContainsKey(Bulkhead.Main))
                existing.Add(Director[Bulkhead.Main].Objective);
            if (Director.ContainsKey(Bulkhead.Extreme))
                existing.Add(Director[Bulkhead.Extreme].Objective);
            if (Director.ContainsKey(Bulkhead.Overload))
                existing.Add(Director[Bulkhead.Overload].Objective);

            if (!Director.ContainsKey(bulkhead))
            {
                Director[bulkhead] = new BuildDirector
                {
                    Bulkhead = bulkhead,
                    Complex = Complex,
                    Complexity = Complexity.Low,
                    Settings = Settings,
                    Tier = Tier
                };

                Director[bulkhead].GenObjective(existing);
            }

            var director = Director[bulkhead];
            director.GenPoints();

            var objective = WardenObjective.PreBuild(director, this);

            var direction = Generator.Draw(RelativeDirections);
            var layout = LevelLayout.Build(this, director, objective, direction);
            LayoutRef[bulkhead] = layout.PersistentId;

            objective.Build(director, this);

            var layerData = ObjectiveLayer[bulkhead];
            layerData.ObjectiveData.DataBlockId = objective.PersistentId;

            // TODO: can this be moved somewhere else?
            if (director.Objective == WardenObjectiveType.ClearPath)
                layerData.ObjectiveData.WinCondition = WardenObjectiveWinCondition.GoToElevator;

            Bins.WardenObjectives.AddBlock(objective);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        static public Level Build(Level level)
        {
            if (level.Name == "")
                level.Name = Generator.Pick(Words.NounsLevel) ?? "";

            var logLevelId = $"Level={level.Tier}{level.Index}";

            level.GenerateDepth();
            level.GenerateZoneAliasStarts();

            if (level.Settings == null)
                level.Settings = new LevelSettings(level.Tier);

            // Randomly select which bulkheads to use
            var selectedBulkheads = Generator.Select(new List<(double, Bulkhead)>
            {
                (0.25, Bulkhead.Main),
                (0.4, Bulkhead.Main | Bulkhead.Extreme),
                (0.1, Bulkhead.Main | Bulkhead.Overload),
                (0.25, Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload)
            });

            // Assign bulkheads
            level.Settings.Bulkheads = selectedBulkheads;

            // Set low fog if we have fog
            if (level.Settings.Modifiers.Contains(LevelModifiers.Fog))
                level.FogSettings = Fog.LowFog;

            // For heavy fog we can also roll low mid fog
            if (level.Settings.Modifiers.Contains(LevelModifiers.HeavyFog))
                level.FogSettings = Generator.Flip(0.75) ? Fog.LowFog : Fog.LowMidFog;

            Plugin.Logger.LogDebug($"{logLevelId} - Modifiers: {level.Settings.Modifiers}, Fog: {level.FogSettings.Name}");

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
                (1.0, "chained"),

                // TODO: implement these
                // (1.0, "all"),
                // (1.0, "choice"), // TODO: implement
            });

            #region Layout generation
            level.BuildLayout(Bulkhead.Main);

            if (selectedBulkheads.HasFlag(Bulkhead.Extreme))
                level.BuildLayout(Bulkhead.Extreme);

            if (selectedBulkheads.HasFlag(Bulkhead.Overload))
                level.BuildLayout(Bulkhead.Overload);
            #endregion

            #region Bulkhead Keys
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

            Plugin.Logger.LogDebug($"Level={level.Tier}{level.Index} level plan: {level.Planner}");

            return level;
        }
    }
}
