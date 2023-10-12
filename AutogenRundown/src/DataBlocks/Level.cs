using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutogenRundown.GeneratorData;
using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks
{
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
        public BuildDirector MainDirector { get; set; }

        [JsonIgnore]
        public BuildDirector SecondaryDirector { get; set; }

        [JsonIgnore]
        public BuildDirector OverloadDirector { get; set; }

        /// <summary>
        /// Level description
        /// </summary>
        [JsonIgnore]
        public string Description { get; set; } = "";

        public JObject Descriptive
        {
            get => new JObject
            {
                ["Prefix"] = Tier,
                ["PublicName"] = Name,
                ["IsExtraExpedition"] = false,
                ["SkipExpNumberInName"] = false,
                ["UseCustomMatchmakingTier"] = false,
                ["CustomMatchmakingTier"] = 1,
                ["ProgressionVisualStyle"] = 0,
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

        public JObject BuildSecondaryFrom = JObject.FromObject(new
        {
            LayerType = 0,
            Zone = 0
        });
        #endregion

        #region Third (Overload) Objective Data
        /// <summary>
        /// Third (Overload) objectives data
        /// </summary>
        public ObjectiveLayerData ThirdLayerData { get; set; } = new ObjectiveLayerData();

        public bool ThirdLayerEnabled { get; set; } = false;

        public UInt32 ThirdLayout = 0;

        public JObject ThirdSecondaryFrom = JObject.FromObject(new
        {
            LayerType = 0,
            Zone = 0
        });
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

            // ============ Main level ============
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

            // Secondary (Extreme)
            // Tertiary (Overload)

            Bins.WardenObjectives.AddBlock(mainObjective);

            return level;
        }
    }
}
