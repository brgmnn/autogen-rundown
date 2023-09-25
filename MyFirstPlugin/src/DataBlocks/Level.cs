using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static GameData.GD;

namespace MyFirstPlugin.DataBlocks
{
    internal class Level : DataBlock
    {
        #region Filler settings that won't change
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
        /// Level Tier, roughly difficulty
        /// </summary>
        [JsonIgnore]
        public string Tier { get; set; } = "A";

        /// <summary>
        /// Level depth in meters
        /// </summary>
        [JsonIgnore]
        public int Depth { get; set; } = 1;

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

        public JObject Expedition = JObject.FromObject(new
        {
            ComplexResourceData = 1,
            MLSLevelKit = 0,
            LightSettings = 0,
            FogSettings = 90,
            EnemyPopulation = 1,
            ExpeditionBalance = 1,
            ScoutWaveSettings = 3,
            ScoutWavePopulation = 1,
            EnvironmentWetness = 0.0,
            DustColor = new Color { Alpha = 1.0, Red = 0.5, Green = 0.5, Blue = 0.5 },
            DustTurbulence = 1.0
        });

        public JObject VanityItemsDropData = new JObject
        {
            ["Groups"] = new JArray()
        };

        /// <summary>
        /// Match this to the persistent ID of the Level Layout
        /// </summary>
        public int LevelLayoutData { get; set; } = 0;

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

        public int SecondaryLayout = 0;

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

        public int ThirdLayout = 0;

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
    }
}
