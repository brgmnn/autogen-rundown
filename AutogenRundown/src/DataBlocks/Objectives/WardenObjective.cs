using AutogenRundown.DataBlocks.Alarms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
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

    enum WardenObjectiveItem : UInt32
    {
        PersonnelId = 128,
        PartialDecoder = 129,

        Harddrive = 147,
        Glp_1 = 149,
        Glp_2 = 169,
        Osip = 150,
        PlantSample = 153,
        MemoryStick = 171,

        DataCube = 165,
        DataCubes = 179,
        DataCubeBackup = 178,
        DataCubeTampered = 168
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
                    {
                        // Pick a random zone that isn't the first zone unless there's only one
                        var targetZone = Math.Clamp(
                            Generator.Random.Next(1, layout.Zones.Count),
                            0,
                            layout.Zones.Count - 1);

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
                    }

                case DistributionStrategy.EvenlyAcrossZones:
                    {
                        var placement = new List<ZonePlacementData>();

                        foreach (var zone in layout.Zones)
                        {
                            placement.Add(new ZonePlacementData
                            {
                                LocalIndex = zone.LocalIndex,
                                Weights = ZonePlacementWeights.EvenlyDistributed
                            });
                        }

                        data.ObjectiveData.ZonePlacementDatas.Add(placement);

                        break;
                    }

                case DistributionStrategy.Random:
                    {
                        var placement = new List<ZonePlacementData>();

                        void Place(Zone zone)
                        {
                            placement.Add(new ZonePlacementData
                            {
                                LocalIndex = zone.LocalIndex,
                                Weights = ZonePlacementWeights.GenRandom()
                            });
                        }

                        foreach (var zone in layout.Zones)
                        {
                            if (Generator.Flip())
                                continue;

                            Place(zone);
                        }

                        // If none of the zones were picked, force pick one of the zones. Otherwise
                        // no items will be placed
                        if (placement.Count == 0)
                        {
                            var index = Math.Clamp(
                                Generator.Random.Next(1, layout.Zones.Count),
                                0,
                                layout.Zones.Count - 1);

                            var zone = layout.Zones[index];
                            Place(zone);
                        }

                        data.ObjectiveData.ZonePlacementDatas.Add(placement);

                        break;
                    }
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

        public static string GenLevelDescription(WardenObjectiveType type, WardenObjectiveItem item = WardenObjectiveItem.PersonnelId)
            => type switch
            {
                WardenObjectiveType.ClearPath => Generator.Pick(new List<string>
                    {
                        "Unknown hostile lifeform readings in subjacent quadrant. Expendable prisoners sent to survey threat severity."
                    }) ?? "",
                WardenObjectiveType.GatherSmallItems => item switch
                {
                    WardenObjectiveItem.Glp_1 => "Conduit genetic code compromised. Prisoners to collect DNA sample from HSU facility.",
                    WardenObjectiveItem.Glp_2 => "Conduit genetic code compromised. Prisoners to collect DNA sample from HSU facility.",
                    _ => "Prisoners to collect items from storage facility. High asset fatality rate expected."
                },
                _ => "",
            };

        public static List<(WardenObjectiveItem, string, string)> BuildSmallPickupPack(string tier)
            => new List<(WardenObjectiveItem, string, string)>
            {
                (WardenObjectiveItem.PersonnelId, "Personnel ID", "Gather [COUNT_REQUIRED] Personnel IDs and return the data to be processed."),
                (WardenObjectiveItem.PartialDecoder, "Partial Decoder", "Gather [COUNT_REQUIRED] Partial Decoders and return the data to be processed."),
                (WardenObjectiveItem.Harddrive, "Hard drive", "Gather [COUNT_REQUIRED] Hard Drives and return the drives for data archival."),
                (WardenObjectiveItem.Glp_1, "GLP-1 canister", "Gather [COUNT_REQUIRED] GLP-1 canisters and return the canisters for genome sequencing."),
                (WardenObjectiveItem.Glp_2, "GLP-2 canister", "Gather [COUNT_REQUIRED] GLP-2 canisters and return the canisters for genome sequencing."),
                (WardenObjectiveItem.Osip, "OSIP vial", "Gather [COUNT_REQUIRED] OSIP vials and return the vials for chemical analysis."),
                (WardenObjectiveItem.PlantSample, "Plant sample", "Gather [COUNT_REQUIRED] Plant samples and return the samples for analysis."),
                (WardenObjectiveItem.MemoryStick, "Memory stick", "Gather [COUNT_REQUIRED] Memory sticks and return the memory sticks for analysis."),
                (WardenObjectiveItem.DataCube, "Data cube", "Gather [COUNT_REQUIRED] Data cubes and return the cubes for data extraction."),
                (WardenObjectiveItem.DataCubeBackup, "Backup data cube", "Gather [COUNT_REQUIRED] Backup Data cubes and return the cubes for data archival."),
                (WardenObjectiveItem.DataCubeTampered, "Tampered data cube", "Gather [COUNT_REQUIRED] Data cubes and return the cubes for inspection.")
            };

        public static WardenObjective Build(BuildDirector director, Level level)
        {
            var objective = new WardenObjective
            {
                Type = director.Objective,
            };

            ObjectiveLayerData dataLayer = level.GetObjectiveLayerData(director.Bulkhead);

            objective.GoToWinCondition_Elevator = "Return to the point of entrance in [EXTRACTION_ZONE]";
            objective.GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";
            objective.GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

            switch (director.Objective)
            {
                case WardenObjectiveType.GatherSmallItems:
                    {
                        var (itemId, name, description) = Generator.Pick(BuildSmallPickupPack(level.Tier));
                        var strategy = Generator.Pick(new List<DistributionStrategy>
                        {
                            DistributionStrategy.Random,
                            DistributionStrategy.SingleZone,
                            DistributionStrategy.EvenlyAcrossZones
                        });

                        objective.Name = $"Gather_{name.Replace(" ", "_")}";
                        objective.MainObjective = description;
                        objective.FindLocationInfo = $"Look for {name}s in the complex";
                        objective.FindLocationInfoHelp = "Current progress: [COUNT_CURRENT] / [COUNT_REQUIRED]";

                        if (director.Bulkhead == Bulkhead.Main)
                            level.Description = GenLevelDescription(director.Objective, itemId);

                        objective.GatherRequiredCount = level.Tier switch
                        {
                            "A" => Generator.Random.Next(4, 8),
                            "B" => Generator.Random.Next(6, 10),
                            "C" => Generator.Random.Next(7, 12),
                            "D" => Generator.Random.Next(8, 13),
                            "E" => Generator.Random.Next(12, 20),
                            _ => 1,
                        };

                        objective.GatherItemId = (UInt32)itemId;
                        objective.GatherSpawnCount = Generator.Random.Next(
                            objective.GatherRequiredCount,
                            objective.GatherRequiredCount + 6);

                        objective.DistributeObjectiveItems(level, director.Bulkhead, strategy);

                        var zoneSpawns = dataLayer.ObjectiveData.ZonePlacementDatas[0].Count;

                        objective.GatherMaxPerZone = objective.GatherSpawnCount / zoneSpawns + objective.GatherSpawnCount % zoneSpawns;

                        break;
                    }

                case WardenObjectiveType.ClearPath:
                    {
                        objective.Name = "Clear_path";
                        objective.MainObjective = "Clear a path to the exit point in [EXTRACTION_ZONE]";
                        objective.GoToWinCondition_Elevator = "Go to the forward exit point in [EXTRACTION_ZONE]";
                        objective.GoToWinCondition_CustomGeo = "Go to the forward exit point in [EXTRACTION_ZONE]";

                        if (director.Bulkhead == Bulkhead.Main)
                            level.Description = GenLevelDescription(director.Objective);

                        objective.EventsOnGotoWin.Add(new JObject
                        {
                            ["WaveSettings"] = (int)VanillaWaveSettings.Apex,
                            ["WavePopulation"] = (int)WavePopulation.Baseline,
                            ["AreaDistance"] = 2,
                            ["SpawnDelay"] = 0.0,
                            ["TriggerAlarm"] = true,
                            ["IntelMessage"] = 0
                        });

                        break;
                    }
            }

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
        public uint GatherItemId { get; set; } = 128;

        [JsonProperty("Gather_SpawnCount")]
        public int GatherSpawnCount { get; set; } = 6;

        [JsonProperty("Gather_MaxPerZone")]
        public int GatherMaxPerZone { get; set; } = 3;
        #endregion

        #region Type=4: Clear a path
        #endregion

        #region Expedition exit
        public uint ChainedPuzzleAtExit { get; set; } = ChainedPuzzle.ExitAlarm.PersistentId;
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
