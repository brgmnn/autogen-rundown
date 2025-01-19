using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record WardenObjective
{
    public void Build_RetrieveBigItems(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);
        var item = RetrieveItems.First();

        MainObjective = "Find [ALL_ITEMS] and bring it to the extraction scan in [EXTRACTION_ZONE]";
        FindLocationInfo = "Gather information about the location of [ALL_ITEMS]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";

        if (RetrieveItems.Count == 1)
        {
            if (dataLayer.ObjectiveData.ZonePlacementDatas[0].Count == 1)
            {
                var zone = Intel.Zone(dataLayer.ObjectiveData.ZonePlacementDatas[0][0].LocalIndex +
                                      layout.ZoneAliasStart);

                GoToZone = $"Navigate to {zone} and find [ALL_ITEMS]";
                GoToZoneHelp = $"Use information in the environment to find {zone}";
            }
            else
            {
                var zones = string.Join(", ",
                    dataLayer.ObjectiveData.ZonePlacementDatas[0].Select(placement =>
                        Intel.Zone(placement.LocalIndex + layout.ZoneAliasStart)));

                GoToZone = $"Navigate to and find [ALL_ITEMS] in one of zones {zones}";
                GoToZoneHelp = $"Use information in the environment to find {zones}";
            }
        }
        else
        {
            GoToZone = "Navigate to and find [ALL_ITEMS]";
            GoToZoneHelp = "Use information in the environment to find each item zone";
        }

        InZoneFindItem = "Find [ALL_ITEMS] somewhere inside [ITEM_ZONE]";
        InZoneFindItemHelp = "Use maintenance terminal command PING to find [ALL_ITEMS]";
        SolveItem = "WARNING - Hisec Cargo misplaced - ENGAGING SECURITY PROTOCOLS";

        if (RetrieveItems.Count() > 1)
        {
            GoToWinCondition_Elevator = "Return [ALL_ITEMS] to the extraction point in [EXTRACTION_ZONE]";
            GoToWinCondition_CustomGeo = "Return [ALL_ITEMS] to the extraction point in [EXTRACTION_ZONE]";
        }
        else
        {
            GoToWinCondition_Elevator = "Return the [ALL_ITEMS] to the extraction point in [EXTRACTION_ZONE]";
            GoToWinCondition_CustomGeo = "Return the [ALL_ITEMS] to the extraction point in [EXTRACTION_ZONE]";
        }

        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        switch (item)
        {
            case WardenObjectiveItem.CargoCrate:
            case WardenObjectiveItem.CargoCrateHighSecurity:
            case WardenObjectiveItem.DataSphere:
            {
                switch (level.Tier)
                {
                    case "A":
                    {
                        if (director.Bulkhead == Bulkhead.Main)
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = WaveSettings.Exit_Objective_Easy.PersistentId,
                                WavePopulation = WavePopulation.Baseline.PersistentId,
                                SpawnDelay = 8.0,
                                TriggerAlarm = true
                            });
                        else
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = WaveSettings.SingleWave_28pts.PersistentId,
                                WavePopulation = WavePopulation.Baseline.PersistentId,
                                SpawnDelay = 6.0,
                                TriggerAlarm = false
                            });
                        break;
                    }

                    case "B":
                    {
                        WavesOnGotoWin.Add(new GenericWave
                        {
                            WaveSettings = director.Bulkhead == Bulkhead.Main ?
                                WaveSettings.Exit_Objective_Medium.PersistentId :
                                WaveSettings.Exit_Objective_Easy.PersistentId,
                            WavePopulation = WavePopulation.Baseline.PersistentId,
                            SpawnDelay = 8.0,
                            TriggerAlarm = true
                        });

                        // Disable the lights
                        if (Generator.Flip(0.6))
                            EventsOnGotoWin.AddLightsOff(Generator.Between(2, 35));

                        break;
                    }

                    case "C":
                    {
                        if (level.Settings.Modifiers.Contains(LevelModifiers.Chargers) && Generator.Flip(0.1))
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = WaveSettings.Exit_Objective_Easy.PersistentId,
                                WavePopulation = WavePopulation.OnlyChargers.PersistentId,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = WaveSettings.Exit_Objective_Medium.PersistentId,
                                WavePopulation = WavePopulation.Baseline.PersistentId,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });

                        // Small chance for "fun" pouncer to be added
                        if (Generator.Flip(0.05))
                            WavesOnGotoWin.Add(GenericWave.SinglePouncer with
                            {
                                SpawnDelay = Generator.Between(30, 75),
                                TriggerAlarm = false
                            });

                        // Disable the lights
                        if (Generator.Flip(0.35))
                            EventsOnGotoWin.AddLightsOff(Generator.Between(2, 35));

                        break;
                    }

                    case "D":
                    {
                        // Selecting the error wave to be applied
                        if (level.Settings.Modifiers.Contains(LevelModifiers.Shadows) && Generator.Flip(0.3) ||
                            level.Settings.Modifiers.Contains(LevelModifiers.ManyShadows))
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = director.Bulkhead == Bulkhead.Main ?
                                    WaveSettings.Exit_Objective_Hard.PersistentId :
                                    WaveSettings.Exit_Objective_Medium.PersistentId,
                                WavePopulation = WavePopulation.OnlyShadows.PersistentId,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else if (level.Settings.Modifiers.Contains(LevelModifiers.Chargers) && Generator.Flip(0.3) ||
                            level.Settings.Modifiers.Contains(LevelModifiers.ManyChargers))
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = director.Bulkhead == Bulkhead.Main ?
                                    WaveSettings.Exit_Objective_Medium.PersistentId :
                                    WaveSettings.Exit_Objective_Easy.PersistentId,
                                WavePopulation = WavePopulation.OnlyChargers.PersistentId,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else if (level.Settings.Modifiers.Contains(LevelModifiers.Nightmares) && Generator.Flip(0.3) ||
                                 level.Settings.Modifiers.Contains(LevelModifiers.ManyNightmares))
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = WaveSettings.Exit_Objective_Easy.PersistentId,
                                WavePopulation = WavePopulation.OnlyNightmares.PersistentId,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = WaveSettings.Exit_Objective_Hard.PersistentId,
                                WavePopulation = WavePopulation.Baseline.PersistentId,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });

                        if (Generator.Flip(0.08))
                            WavesOnGotoWin.Add(GenericWave.SingleTank with
                            {
                                SpawnDelay = Generator.Between(20, 50),
                                TriggerAlarm = false
                            });
                        else if (Generator.Flip(0.03))
                            WavesOnGotoWin.Add(GenericWave.SingleMother with
                            {
                                SpawnDelay = Generator.Between(50, 90),
                                TriggerAlarm = false
                            });
                        else if (Generator.Flip(0.01))
                            WavesOnGotoWin.Add(GenericWave.SinglePMother with
                            {
                                SpawnDelay = Generator.Between(10, 45),
                                TriggerAlarm = false
                            });

                        if (level.Settings.Modifiers.Contains(LevelModifiers.Fog) ||
                            level.Settings.Modifiers.Contains(LevelModifiers.HeavyFog))
                        {
                            // Flood the level in 30 mins
                            if (Generator.Flip(0.5))
                                EventsOnGotoWin.AddFillFog(10.0, 30 * 60);
                        }
                        // Disable the lights
                        else if (Generator.Flip(0.6))
                            EventsOnGotoWin.AddLightsOff(Generator.Between(2, 35));

                        break;
                    }

                    case "E":
                    {
                        // Selecting the error wave to be applied
                        if (level.Settings.Modifiers.Contains(LevelModifiers.Shadows) && Generator.Flip(0.5) ||
                            level.Settings.Modifiers.Contains(LevelModifiers.ManyShadows))
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = director.Bulkhead == Bulkhead.Main ?
                                    WaveSettings.Exit_Objective_Hard.PersistentId :
                                    WaveSettings.Exit_Objective_Medium.PersistentId,
                                WavePopulation = WavePopulation.OnlyShadows.PersistentId,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else if (level.Settings.Modifiers.Contains(LevelModifiers.Chargers) && Generator.Flip(0.3) ||
                            level.Settings.Modifiers.Contains(LevelModifiers.ManyChargers))
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = director.Bulkhead == Bulkhead.Main ?
                                    WaveSettings.Exit_Objective_Medium.PersistentId :
                                    WaveSettings.Exit_Objective_Easy.PersistentId,
                                WavePopulation = WavePopulation.OnlyChargers.PersistentId,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else if (level.Settings.Modifiers.Contains(LevelModifiers.Nightmares) && Generator.Flip(0.3) ||
                                 level.Settings.Modifiers.Contains(LevelModifiers.ManyNightmares))
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = WaveSettings.Exit_Objective_Easy.PersistentId,
                                WavePopulation = WavePopulation.OnlyNightmares.PersistentId,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                WaveSettings = WaveSettings.Exit_Objective_VeryHard.PersistentId,
                                WavePopulation = WavePopulation.Baseline.PersistentId,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });

                        if (Generator.Flip(0.12))
                            WavesOnGotoWin.Add(GenericWave.SingleTank with
                            {
                                SpawnDelay = Generator.Between(20, 50),
                                TriggerAlarm = false
                            });
                        else if (Generator.Flip(0.09))
                            WavesOnGotoWin.Add(GenericWave.SingleMother with
                            {
                                SpawnDelay = Generator.Between(50, 90),
                                TriggerAlarm = false
                            });
                        else if (Generator.Flip(0.04))
                            WavesOnGotoWin.Add(GenericWave.SinglePMother with
                            {
                                SpawnDelay = Generator.Between(10, 45),
                                TriggerAlarm = false
                            });


                        if (level.Settings.Modifiers.Contains(LevelModifiers.Fog) ||
                            level.Settings.Modifiers.Contains(LevelModifiers.HeavyFog))
                        {
                            // Flood the level in 30 mins
                            if (Generator.Flip(0.5))
                                EventsOnGotoWin.AddFillFog(10.0, 30 * 60);
                        }

                        // Disable the lights
                        if (Generator.Flip(0.4))
                            EventsOnGotoWin.AddLightsOff(Generator.Between(2, 35));

                        break;

                    }
                }
                break;
            }

            case WardenObjectiveItem.MatterWaveProjector:
            {
                var zoneIndex = dataLayer.ObjectiveData.ZonePlacementDatas[0][0].LocalIndex;

                WavesOnGotoWin.Add(GenericWave.ExitTrickle);

                // Manually set the zones as the inbuilt ITEM_ZONE doesn't seem to
                // work correctly for MWP
                GoToZone = $"Navigate to [ZONE_{zoneIndex}] and find [ALL_ITEMS]";
                GoToZoneHelp = $"Use information in the environment to find [ZONE_{zoneIndex}]";
                InZoneFindItem = $"Find [ALL_ITEMS] somewhere inside [ZONE_{zoneIndex}]";

                SolveItem = "WARNING - Matter Wave Projector misplaced - ENGAGING SECURITY PROTOCOLS";

                break;
            }
        }
    }
}
