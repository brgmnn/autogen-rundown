using MyFirstPlugin.GeneratorData;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace MyFirstPlugin.DataBlocks
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
        /// Which complex type to use
        /// </summary>
        [JsonIgnore]
        public Complex Complex { get; set; } = Complex.Mining;

        /// <summary>
        /// Level description
        /// </summary>
        [JsonIgnore]
        public string Description { get; set; } = "Conduit genetic code compromised. Prisoners to collect DNA sample from HSU facility.";

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
            BuildSeed = 1996,
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
        public ObjectiveLayerData GetObjectiveLayerData(ObjectiveVariant variant)
        {
            switch (variant)
            {
                case ObjectiveVariant.Main:
                    return MainLayerData;
                case ObjectiveVariant.Extreme:
                    return SecondaryLayerData;
                case ObjectiveVariant.Overload:
                    return ThirdLayerData;
                default:
                    return MainLayerData;
            }
        }

        public LevelLayout? GetLevelLayout(ObjectiveVariant variant)
        {
            switch (variant)
            {
                case ObjectiveVariant.Main:
                    return Bins.LevelLayouts.Find(LevelLayoutData);
                case ObjectiveVariant.Extreme:
                    return Bins.LevelLayouts.Find(SecondaryLayout);
                case ObjectiveVariant.Overload:
                    return Bins.LevelLayouts.Find(ThirdLayout);
                default:
                    return Bins.LevelLayouts.Find(LevelLayoutData);
            }
        }

        public enum DistributionStrategy 
        {
            /// <summary>
            /// Randomly placed across all zones in random locations.
            /// </summary>
            Random, 

            /// <summary>
            /// All items in a single zone (randomly)
            /// </summary>
            SingleZone, 

            /// <summary>
            /// Evenly distributed across all zones
            /// </summary>
            EvenlyAcrossZones 
        }

        public void DistributeObjectiveItems(
            WardenObjective objective,
            ObjectiveVariant variant,
            DistributionStrategy strategy)
        {
            var layerData = GetObjectiveLayerData(variant);
            var layout = GetLevelLayout(variant);

            objective.GatherRequiredCount = Generator.Random.Next(4, 8);
            objective.GatherItemId = 128;
            objective.GatherSpawnCount = Generator.Random.Next(
                objective.GatherRequiredCount,
                objective.GatherRequiredCount + 6);
            objective.GatherMaxPerZone = Generator.Random.Next(3, 8);

            switch (strategy)
            {
                case DistributionStrategy.SingleZone:
                    var targetZone = Generator.Random.Next(0, layout.Zones.Count);

                    layerData.ObjectiveData.ZonePlacementDatas.Add(
                        new List<ZonePlacementData>()
                        {
                            new ZonePlacementData
                            {
                                LocalIndex = targetZone,
                                Weights = ZonePlacementWeights.EvenlyDistributed
                            }
                        });

                    break;
                case DistributionStrategy.EvenlyAcrossZones:
                    break;
                case DistributionStrategy.Random:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public Level Build(BuildDirector director, Level level)
        {
            var name = Generator.Pick(Words.NounsLevel);

            level.Name = name;
            level.GenerateDepth();

            // Main level
            var mainLevelLayout = LevelLayout.Build(level);
            level.LevelLayoutData = mainLevelLayout.PersistentId;

            var mainObjective = WardenObjective.Build(
                WardenObjectiveType.GatherSmallItems,
                level,
                ObjectiveVariant.Main);

            level.MainLayerData.ObjectiveData.DataBlockId = mainObjective.PersistentId;

            // Secondary (Extreme)
            // Tertiary (Overload)

            Bins.WardenObjectives.AddBlock(mainObjective);

            return level;
        }
    }
}
