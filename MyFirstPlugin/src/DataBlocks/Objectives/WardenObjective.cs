using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static MyFirstPlugin.DataBlocks.Level;

namespace MyFirstPlugin.DataBlocks
{
    /// <summary>
    /// Which objective bulkhead are we in.
    /// </summary>
    enum Bulkhead { Main, Extreme, Overload }

    enum WardenObjectiveType
    {
        HsuFindSample = 0,
        ReactorStartup = 1,
        ReactorShutdown = 2,
        GatherSmallItems = 3,
        ClearPath = 4,
        SpecialTerminalCommand = 5,
        RetrieveBigItems = 6,
        PowerCellDistribution = 7,
        TerminalUplink = 8,
        CentralGeneratorCluster = 9,
        HsuActivateSmall = 10,
        Survival = 11,
        GatherTerminal = 12,
        CorruptedTerminalUplink = 13,
        Empty = 14,
        TimedTerminalSequence = 15
    }

    enum DistributionStrategy
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

    internal class WardenObjective : DataBlock
    {
        /*
        {
            "Type": 3,
            "Header": "Gather Personnel IDs",
            "MainObjective": "Gather [COUNT_REQUIRED] Personnel IDs and return the data to be processed.",
            "WardenObjectiveSpecialUpdateType": 0,
            "GenericItemFromStart": 0,
            "DoNotMarkPickupItemsAsWardenObjectives": false,
            "OverrideNoRequiredItemsForExit": false,
            "FindLocationInfo": "Look for personnel IDs in the complex",
            "FindLocationInfoHelp": "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]",
            "GoToZone": 0,
            "GoToZoneHelp": 0,
            "InZoneFindItem": 0,
            "InZoneFindItemHelp": 0,
            "SolveItem": 0,
            "SolveItemHelp": 0,
            "GoToWinCondition_Elevator": "Return to the point of entrance in [EXTRACTION_ZONE]",
            "GoToWinConditionHelp_Elevator": 0,
            "GoToWinCondition_CustomGeo": "Go to the forward exit point in [EXTRACTION_ZONE]",
            "GoToWinConditionHelp_CustomGeo": 0,
            "GoToWinCondition_ToMainLayer": "Go back to the main objective and complete the expedition.",
            "GoToWinConditionHelp_ToMainLayer": 0,
            "ShowHelpDelay": 180.0,
            "WavesOnElevatorLand": [],
            "EventsOnElevatorLand": [],
            "WaveOnElevatorWardenIntel": 0,
            "FogTransitionDataOnElevatorLand": 0,
            "FogTransitionDurationOnElevatorLand": 0.0,
            "OnActivateOnSolveItem": false,
            "WavesOnActivate": [],
            "EventsOnActivate": [],
            "StopAllWavesBeforeGotoWin": false,
            "WavesOnGotoWin": [],
            "WaveOnGotoWinTrigger": 0,
            "EventsOnGotoWin": [],
            "EventsOnGotoWinTrigger": 0,
            "FogTransitionDataOnGotoWin": 0,
            "FogTransitionDurationOnGotoWin": 0.0,
            "ChainedPuzzleToActive": 0,
            "ChainedPuzzleMidObjective": 0,
            "ChainedPuzzleAtExit": 11,
            "ChainedPuzzleAtExitScanSpeedMultiplier": 2.0,
            "Gather_RequiredCount": 4,
            "Gather_ItemId": 128,
            "Gather_SpawnCount": 6,
            "Gather_MaxPerZone": 3,
            "Retrieve_Items": [],
            "ReactorWaves": [],
            "LightsOnFromBeginning": false,
            "LightsOnDuringIntro": false,
            "LightsOnWhenStartupComplete": false,
            "DoNotSolveObjectiveOnReactorComplete": false,
            "SpecialTerminalCommand": "",
            "SpecialTerminalCommandDesc": "",
            "PostCommandOutput": [],
            "SpecialCommandRule": 0,
            "PowerCellsToDistribute": 0,
            "Uplink_NumberOfVerificationRounds": 0,
            "Uplink_NumberOfTerminals": 1,
            "Uplink_WaveSpawnType": 1,
            "CentralPowerGenClustser_NumberOfGenerators": 0,
            "CentralPowerGenClustser_NumberOfPowerCells": 4,
            "CentralPowerGenClustser_FogDataSteps": [],
            "ActivateHSU_ItemFromStart": 0,
            "ActivateHSU_ItemAfterActivation": 0,
            "ActivateHSU_BringItemInElevator": true,
            "ActivateHSU_MarkItemInElevatorAsWardenObjective": false,
            "ActivateHSU_StopEnemyWavesOnActivation": false,
            "ActivateHSU_ObjectiveCompleteAfterInsertion": false,
            "ActivateHSU_RequireItemAfterActivationInExitScan": false,
            "ActivateHSU_Events": [],
            "Survival_TimeToActivate": 0.0,
            "Survival_TimeToSurvive": 0.0,
            "Survival_TimerTitle": 0,
            "Survival_TimerToActivateTitle": 0,
            "GatherTerminal_SpawnCount": 0,
            "GatherTerminal_RequiredCount": 0,
            "GatherTerminal_Command": "",
            "GatherTerminal_CommandHelp": "",
            "GatherTerminal_DownloadingText": "",
            "GatherTerminal_DownloadCompleteText": "",
            "GatherTerminal_DownloadTime": -1.0,
            "TimedTerminalSequence_NumberOfRounds": 3,
            "TimedTerminalSequence_NumberOfTerminals": 1,
            "TimedTerminalSequence_TimePerRound": 90.0,
            "TimedTerminalSequence_TimeForConfirmation": 10.0,
            "TimedTerminalSequence_UseFilterForSourceTerminalPicking": false,
            "TimedTerminalSequence_SourceTerminalWorldEventObjectFilter": "",
            "TimedTerminalSequence_EventsOnSequenceStart": [],
            "TimedTerminalSequence_EventsOnSequenceDone": [],
            "TimedTerminalSequence_EventsOnSequenceFail": [],
            "name": "Gather_Personnel_IDs",
            "internalEnabled": true,
            "persistentID": 8
        }
        */

        /// <summary>
        /// Places objective items in the level as needed
        /// </summary>
        /// <param name="level"></param>
        /// <param name="variant"></param>
        /// <param name="strategy"></param>
        public void DistributeObjectiveItems(
            Level level,
            Bulkhead variant, 
            DistributionStrategy strategy)
        {
            var data = level.GetObjectiveLayerData(variant);
            var layout = level.GetLevelLayout(variant);

            switch (strategy)
            {
                case DistributionStrategy.SingleZone:
                    // Pick a random zone that isn't the first zone unless there's only one
                    var targetZone = Generator.Random.Next(
                        Math.Min(1, layout.Zones.Count - 1),
                        layout.Zones.Count);

                    data.ObjectiveData.ZonePlacementDatas.Add(
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
                    // TODO
                    break;
                case DistributionStrategy.Random:
                    // TODO
                    break;
            }
        }

        /// <summary>
        /// Create a new list of objective types. The intention is to Draw() from this "pack" of objectives.
        /// </summary>
        /// <param name="tier"></param>
        /// <returns></returns>
        public static List<WardenObjectiveType> BuildObjectivePack(string tier)
            => new List<WardenObjectiveType>
            {
                WardenObjectiveType.GatherSmallItems,
                WardenObjectiveType.GatherSmallItems,
                WardenObjectiveType.GatherSmallItems,
            };

        public static WardenObjective Build(
            WardenObjectiveType objectiveType,
            Level level,
            Bulkhead variant = Bulkhead.Main)
        {
            var objective = new WardenObjective
            {
                Type = objectiveType,
            };

            ObjectiveLayerData dataLayer = level.GetObjectiveLayerData(variant);

            switch (objectiveType)
            {
                case WardenObjectiveType.GatherSmallItems:
                    {
                        objective.Name = "Gather_Personnel_IDs";
                        objective.MainObjective = "Gather [COUNT_REQUIRED] Personnel IDs and return the data to be processed. DO IT NOW";
                        objective.FindLocationInfo = "Look for personnel IDs in the complex";
                        objective.FindLocationInfoHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";

                        objective.GatherRequiredCount = Generator.Random.Next(4, 8);
                        objective.GatherItemId = 128;
                        objective.GatherSpawnCount = Generator.Random.Next(
                            objective.GatherRequiredCount, 
                            objective.GatherRequiredCount + 6);
                        objective.GatherMaxPerZone = Generator.Random.Next(3, 8);

                        objective.DistributeObjectiveItems(level, variant, DistributionStrategy.SingleZone);
                        break;
                    }
            }

            objective.GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
            objective.GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
            objective.GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

            dataLayer.ObjectiveData.DataBlockId = objective.PersistentId;

            return objective;
        }

        public WardenObjectiveType Type { get; set; }

        #region Information and display strings
        public string Header { get; set; } = "";
        public string MainObjective { get; set; } = "";
        public string FindLocationInfo { get; set; } = "";
        public string FindLocationInfoHelp { get; set; } = "";
        public string GoToZone { get; set; } = "";
        public string GoToZoneHelp { get; set; } = "";
        public string InZoneFindItem { get; set; } = "";
        public string InZoneFindItemHelp { get; set; } = "";
        public string SolveItem { get; set; } = "";
        public string SolveItemHelp { get; set; } = "";
        public string GoToWinCondition_Elevator { get; set; } = "";
        public string GoToWinConditionHelp_Elevator { get; set; } = "";
        public string GoToWinCondition_CustomGeo { get; set; } = "";
        public string GoToWinConditionHelp_CustomGeo { get; set; } = "";
        public string GoToWinCondition_ToMainLayer { get; set; } = "";
        public string GoToWinConditionHelp_ToMainLayer { get; set; } = "";
        public string WaveOnElevatorWardenIntel { get; set; } = "";
        public string SpecialTerminalCommandDesc { get; set; } = "";
        public string Survival_TimerTitle { get; set; } = "";
        public string Survival_TimerToActivateTitle { get; set; } = "";
        public string GatherTerminal_CommandHelp { get; set; } = "";
        public string GatherTerminal_DownloadingText { get; set; } = "";
        public string GatherTerminal_DownloadCompleteText { get; set; } = "";
        #endregion

        public double ShowHelpDelay { get; set; } = 180.0;

        #region Type=3: Gather small items
        [JsonProperty("Gather_RequiredCount")]
        public int GatherRequiredCount { get; set; } = 4;

        [JsonProperty("Gather_ItemId")]
        public int GatherItemId { get; set; } = 128;

        [JsonProperty("Gather_SpawnCount")]
        public int GatherSpawnCount { get; set; } = 6;

        [JsonProperty("Gather_MaxPerZone")]
        public int GatherMaxPerZone { get; set; } = 3;
        #endregion

        #region Fields not yet implemented
        public int WardenObjectiveSpecialUpdateType = 0;
        public int GenericItemFromStart = 0;
        public bool DoNotMarkPickupItemsAsWardenObjectives = false;
        public bool OverrideNoRequiredItemsForExit = false;
        public JArray WavesOnElevatorLand = new JArray();
        public JArray EventsOnElevatorLand = new JArray();
        public int FogTransitionDataOnElevatorLand = 0;
        public double FogTransitionDurationOnElevatorLand = 0.0;
        public bool OnActivateOnSolveItem = false;
        public JArray WavesOnActivate = new JArray();
        public JArray EventsOnActivate = new JArray();
        public bool StopAllWavesBeforeGotoWin = false;
        public JArray WavesOnGotoWin = new JArray();
        public int WaveOnGotoWinTrigger = 0;
        public JArray EventsOnGotoWin = new JArray();
        public int EventsOnGotoWinTrigger = 0;
        public int FogTransitionDataOnGotoWin = 0;
        public double FogTransitionDurationOnGotoWin = 0.0;
        public int ChainedPuzzleToActive = 0;
        public int ChainedPuzzleMidObjective = 0;
        public int ChainedPuzzleAtExit = 11;
        public double ChainedPuzzleAtExitScanSpeedMultiplier = 2.0;
        public JArray Retrieve_Items = new JArray();
        public JArray ReactorWaves = new JArray();
        public bool LightsOnFromBeginning = false;
        public bool LightsOnDuringIntro = false;
        public bool LightsOnWhenStartupComplete = false;
        public bool DoNotSolveObjectiveOnReactorComplete = false;
        public string SpecialTerminalCommand = "";
        public JArray PostCommandOutput = new JArray();
        public int SpecialCommandRule = 0;
        public int PowerCellsToDistribute = 0;
        public int Uplink_NumberOfVerificationRounds = 0;
        public int Uplink_NumberOfTerminals = 1;
        public int Uplink_WaveSpawnType = 1;
        public int CentralPowerGenClustser_NumberOfGenerators = 0;
        public int CentralPowerGenClustser_NumberOfPowerCells = 4;
        public JArray CentralPowerGenClustser_FogDataSteps = new JArray();
        public int ActivateHSU_ItemFromStart = 0;
        public int ActivateHSU_ItemAfterActivation = 0;
        public bool ActivateHSU_BringItemInElevator = true;
        public bool ActivateHSU_MarkItemInElevatorAsWardenObjective = false;
        public bool ActivateHSU_StopEnemyWavesOnActivation = false;
        public bool ActivateHSU_ObjectiveCompleteAfterInsertion = false;
        public bool ActivateHSU_RequireItemAfterActivationInExitScan = false;
        public JArray ActivateHSU_Events = new JArray();
        public double Survival_TimeToActivate = 0.0;
        public double Survival_TimeToSurvive = 0.0;
        public int GatherTerminal_SpawnCount = 0;
        public int GatherTerminal_RequiredCount = 0;
        public string GatherTerminal_Command = "";
        public double GatherTerminal_DownloadTime = -1.0;
        public int TimedTerminalSequence_NumberOfRounds = 3;
        public int TimedTerminalSequence_NumberOfTerminals = 1;
        public double TimedTerminalSequence_TimePerRound = 90.0;
        public double TimedTerminalSequence_TimeForConfirmation = 10.0;
        public bool TimedTerminalSequence_UseFilterForSourceTerminalPicking = false;
        public string TimedTerminalSequence_SourceTerminalWorldEventObjectFilter = "";
        public JArray TimedTerminalSequence_EventsOnSequenceStart = new JArray();
        public JArray TimedTerminalSequence_EventsOnSequenceDone = new JArray();
        public JArray TimedTerminalSequence_EventsOnSequenceFail = new JArray();
        #endregion
    }
}
