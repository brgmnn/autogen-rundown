using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

public partial record WardenObjective
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="director"></param>
    /// <param name="level"></param>
    private void PreBuild_RetrieveBigItems(BuildDirector director, Level level)
    {
        var choices = new List<(double, WardenObjectiveItem)>
        {
            (1.0, WardenObjectiveItem.DataSphere),
            (1.0, WardenObjectiveItem.CargoCrate),
            (1.0, WardenObjectiveItem.CargoCrateHighSecurity),
            (1.0, WardenObjectiveItem.CryoCase),
        };

        // These would be main objective items only
        if (director.Bulkhead.HasFlag(Bulkhead.Main))
        {
            //choices.Add((1.0, WardenObjectiveItem.NeonateHsu));
            choices.Add((1.0, WardenObjectiveItem.MatterWaveProjector));
        }

        var item = Generator.Select(choices);

        /*
         * Some interesting options here for how many items we should spawn. We
         * want to reduce the number of items for non Main objectives and also
         * want to increase the number of items for deeper levels.
         * */
        var count = (item, director.Tier, director.Bulkhead & Bulkhead.Objectives) switch
        {
            (WardenObjectiveItem.CryoCase, "A", Bulkhead.Main) => Generator.Between(1, 2),
            (WardenObjectiveItem.CryoCase, "B", Bulkhead.Main) => Generator.Between(1, 2),
            (WardenObjectiveItem.CryoCase, "C", Bulkhead.Main) => Generator.Between(1, 2),
            (WardenObjectiveItem.CryoCase, "D", Bulkhead.Main) => Generator.Between(2, 3),
            (WardenObjectiveItem.CryoCase, "E", Bulkhead.Main) => Generator.Between(2, 4),
            (WardenObjectiveItem.CryoCase, "D", _) => Generator.Between(1, 2),
            (WardenObjectiveItem.CryoCase, "E", _) => 2,

            (WardenObjectiveItem.CargoCrateHighSecurity, "D", Bulkhead.Main) => Generator.Between(1, 2),
            (WardenObjectiveItem.CargoCrateHighSecurity, "E", Bulkhead.Main) => 2,

            (_, _, _) => 1

        };

        for (var i = 0; i < count; ++i)
            RetrieveItems.Add(item);
    }

    public void Build_RetrieveBigItems(BuildDirector director, Level level)
    {
        var (dataLayer, layout) = GetObjectiveLayerAndLayout(director, level);
        var item = RetrieveItems.First();

        MainObjective = new Text(() => $"Find [ALL_ITEMS] and bring it to the extraction scan in {Intel.Zone(level.ExtractionZone, level.Planner)}");
        FindLocationInfo = "Gather information about the location of [ALL_ITEMS]";
        FindLocationInfoHelp = "Access more data in the terminal maintenance system";
        InZoneFindItem = "Find [ALL_ITEMS] somewhere inside [ITEM_ZONE]";

        if (RetrieveItems.Count == 1)
        {
            if (dataLayer.ObjectiveData.ZonePlacementDatas[0].Count == 1)
            {
                var zone = Intel.Zone(dataLayer.ObjectiveData.ZonePlacementDatas[0][0].LocalIndex +
                                      layout.ZoneAliasStart);
                var zoneNumber = dataLayer.ObjectiveData.ZonePlacementDatas[0][0].LocalIndex;

                GoToZone = new Text(() => $"Navigate to {Intel.Zone(layout.ZoneAliasStart + zoneNumber)} and find [ALL_ITEMS]");
                GoToZoneHelp = $"Use information in the environment to find {zone}";
                InZoneFindItem = $"Find [ALL_ITEMS] somewhere inside {zone}";
            }
            else
            {
                var zones = string.Join(", ",
                    dataLayer.ObjectiveData.ZonePlacementDatas[0].Select(placement =>
                        Intel.Zone(placement.LocalIndex + layout.ZoneAliasStart)));

                GoToZone = new Text(() =>
                {
                    var zones = string.Join(", ",
                        dataLayer.ObjectiveData.ZonePlacementDatas[0].Select(placement =>
                            Intel.Zone(placement.LocalIndex + layout.ZoneAliasStart)));

                    return $"Navigate to and find [ALL_ITEMS] in one of zones {zones}";
                });
                GoToZoneHelp = $"Use information in the environment to find {zones}";
            }
        }
        else
        {
            GoToZone = new Text("Navigate to and find [ALL_ITEMS]");
            GoToZoneHelp = "Use information in the environment to find each item zone";
        }

        SolveItem = "WARNING - Hisec Cargo misplaced - ENGAGING SECURITY PROTOCOLS";
        InZoneFindItemHelp = "Use maintenance terminal command PING to find [ALL_ITEMS]";

        if (RetrieveItems.Count() > 1)
        {
            GoToWinCondition_Elevator = new Text(() =>
                $"Return [ALL_ITEMS] to the extraction point in {Intel.Zone(level.ExtractionZone, level.Planner)}");
            GoToWinCondition_CustomGeo = new Text(() =>
                $"Bring [ALL_ITEMS] to the forward exit point in {Intel.Zone(level.ExtractionZone, level.Planner)}");
        }
        else
        {
            GoToWinCondition_Elevator = new Text(() =>
                $"Return the [ALL_ITEMS] to the extraction point in {Intel.Zone(level.ExtractionZone, level.Planner)}");
            GoToWinCondition_CustomGeo = new Text(() =>
                $"Bring the [ALL_ITEMS] to the forward exit point in {Intel.Zone(level.ExtractionZone, level.Planner)}");
        }

        GoToWinCondition_ToMainLayer = "Go back to the main objective and complete the expedition.";

        // TODO: Switch and combine with the generic exit waves objective
        switch (item)
        {
            case WardenObjectiveItem.CryoCase:
            {
                if (RetrieveItems.Count == 1)
                    AddCompletedObjectiveWaves(level, director);

                break;
            }

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
                                Settings = WaveSettings.Exit_Objective_Easy,
                                Population = WavePopulation.Baseline,
                                SpawnDelay = 8.0,
                                TriggerAlarm = true
                            });
                        else
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                Settings = WaveSettings.SingleWave_28pts,
                                Population = WavePopulation.Baseline,
                                SpawnDelay = 6.0,
                                TriggerAlarm = false
                            });
                        break;
                    }

                    case "B":
                    {
                        WavesOnGotoWin.Add(new GenericWave
                        {
                            Settings = director.Bulkhead == Bulkhead.Main ?
                                WaveSettings.Exit_Objective_Medium :
                                WaveSettings.Exit_Objective_Easy,
                            Population = WavePopulation.Baseline,
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
                                Settings = WaveSettings.Exit_Objective_Easy,
                                Population = WavePopulation.OnlyChargers,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                Settings = WaveSettings.Exit_Objective_Medium,
                                Population = WavePopulation.Baseline,
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
                                Settings = director.Bulkhead == Bulkhead.Main ?
                                    WaveSettings.Exit_Objective_Hard :
                                    WaveSettings.Exit_Objective_Medium,
                                Population = WavePopulation.OnlyShadows,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else if (level.Settings.Modifiers.Contains(LevelModifiers.Chargers) && Generator.Flip(0.3) ||
                            level.Settings.Modifiers.Contains(LevelModifiers.ManyChargers))
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                Settings = director.Bulkhead == Bulkhead.Main ?
                                    WaveSettings.Exit_Objective_Medium :
                                    WaveSettings.Exit_Objective_Easy,
                                Population = WavePopulation.OnlyChargers,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else if (level.Settings.Modifiers.Contains(LevelModifiers.Nightmares) && Generator.Flip(0.3) ||
                                 level.Settings.Modifiers.Contains(LevelModifiers.ManyNightmares))
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                Settings = WaveSettings.Exit_Objective_Easy,
                                Population = WavePopulation.OnlyNightmares,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                Settings = WaveSettings.Exit_Objective_Hard,
                                Population = WavePopulation.Baseline,
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
                            // TODO: maybe this should be faster for main missions
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
                                Settings = director.Bulkhead == Bulkhead.Main ?
                                    WaveSettings.Exit_Objective_Hard :
                                    WaveSettings.Exit_Objective_Medium,
                                Population = WavePopulation.OnlyShadows,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else if (level.Settings.Modifiers.Contains(LevelModifiers.Chargers) && Generator.Flip(0.3) ||
                            level.Settings.Modifiers.Contains(LevelModifiers.ManyChargers))
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                Settings = director.Bulkhead == Bulkhead.Main ?
                                    WaveSettings.Exit_Objective_Medium :
                                    WaveSettings.Exit_Objective_Easy,
                                Population = WavePopulation.OnlyChargers,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else if (level.Settings.Modifiers.Contains(LevelModifiers.Nightmares) && Generator.Flip(0.3) ||
                                 level.Settings.Modifiers.Contains(LevelModifiers.ManyNightmares))
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                Settings = WaveSettings.Exit_Objective_Easy,
                                Population = WavePopulation.OnlyNightmares,
                                SpawnDelay = 5.0,
                                TriggerAlarm = true
                            });
                        else
                            WavesOnGotoWin.Add(new GenericWave
                            {
                                Settings = WaveSettings.Exit_Objective_VeryHard,
                                Population = WavePopulation.Baseline,
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

                WavesOnGotoWin.Add(GenericWave.Exit_Objective_Easy);

                // Manually set the zones as the inbuilt ITEM_ZONE doesn't seem to
                // work correctly for MWP
                GoToZone = new Text(() => $"Navigate to {Intel.Zone(layout.ZoneAliasStart + zoneIndex)} and find [ALL_ITEMS]");
                GoToZoneHelp = $"Use information in the environment to find [ZONE_{zoneIndex}]";
                InZoneFindItem = $"Find [ALL_ITEMS] somewhere inside [ZONE_{zoneIndex}]";

                SolveItem = "WARNING - Matter Wave Projector misplaced - ENGAGING SECURITY PROTOCOLS";

                break;
            }
        }

        #region Warden Intel Messages

        var itemName = item switch
        {
            WardenObjectiveItem.CryoCase => "Cryo Case",
            WardenObjectiveItem.CargoCrate => "Cargo Crate",
            WardenObjectiveItem.CargoCrateHighSecurity => "HiSec Cargo Crate",
            WardenObjectiveItem.DataSphere => "Data Sphere",
            WardenObjectiveItem.MatterWaveProjector => "Matter Wave Projector",
            _ => "big item"
        };

        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 5), Generator.Draw(new List<string>
        {
            // Theme 1: Carrier Restrictions & Vulnerability (80 messages: 30 with itemName, 50 generic)
            $">... Got the {itemName}!\r\n>... <size=200%><color=red>Can't use my weapon!</color></size>\r\n>... Cover me!",
            ">... I'm carrying it!\r\n>... Can't sprint!\r\n>... <size=200%><color=red>I'm defenseless!</color></size>",
            $">... Carrying the {itemName}!\r\n>... <size=200%><color=red>No weapons!</color></size>\r\n>... Protect me!",
            ">... Can't run with this!\r\n>... Walking speed only!\r\n>... <size=200%><color=red>I need cover!</color></size>",
            $">... {itemName} is heavy!\r\n>... Can't defend myself!\r\n>... <size=200%><color=red>Stay close!</color></size>",
            ">... I'm vulnerable!\r\n>... <size=200%><color=red>Can't shoot!</color></size>\r\n>... Keep them off me!",
            $">... I have the {itemName}!\r\n>... <size=200%><color=red>Completely exposed!</color></size>\r\n>... [breathing hard]",
            ">... This thing's restricting me!\r\n>... Can't use gear!\r\n>... <size=200%><color=red>I need backup!</color></size>",
            $">... Moving with the {itemName}!\r\n>... No weapon access!\r\n>... <size=200%><color=red>Cover me!</color></size>",
            ">... Can't sprint!\r\n>... <size=200%><color=red>I'm slow!</color></size>\r\n>... Defend me!",
            $">... Holding the {itemName}!\r\n>... Can't fight!\r\n>... <size=200%><color=red>Protect me!</color></size>",
            ">... I'm unarmed now!\r\n>... Carrying the item!\r\n>... <size=200%><color=red>Watch my back!</color></size>",
            $">... {itemName} secured!\r\n>... <size=200%><color=red>No weapons!</color></size>\r\n>... I need escort!",
            ">... Can't use my gun!\r\n>... <size=200%><color=red>Hands are full!</color></size>\r\n>... Cover me!",
            $">... Got the {itemName}!\r\n>... Walking speed!\r\n>... <size=200%><color=red>Defenseless!</color></size>",
            ">... I'm exposed!\r\n>... Can't fight back!\r\n>... <size=200%><color=red>Need protection!</color></size>",
            $">... Carrying the {itemName}!\r\n>... <size=200%><color=red>Can't run!</color></size>\r\n>... Stay with me!",
            ">... This is heavy!\r\n>... No combat ability!\r\n>... <size=200%><color=red>Keep them away!</color></size>",
            $">... {itemName} in hand!\r\n>... Can't access weapons!\r\n>... <size=200%><color=red>I'm vulnerable!</color></size>",
            ">... Moving slow!\r\n>... <size=200%><color=red>Can't defend!</color></size>\r\n>... Protect me!",
            $">... I have the {itemName}!\r\n>... No gear access!\r\n>... <size=200%><color=red>Cover me!</color></size>",
            ">... Can't use anything!\r\n>... Just my flashlight!\r\n>... <size=200%><color=red>I need backup!</color></size>",
            $">... {itemName} is restricting me!\r\n>... <size=200%><color=red>Can't fight!</color></size>\r\n>... Stay close!",
            ">... I'm a sitting duck!\r\n>... Can't shoot!\r\n>... <size=200%><color=red>Guard me!</color></size>",
            $">... Holding the {itemName}!\r\n>... <size=200%><color=red>Completely vulnerable!</color></size>\r\n>... [grunting]",
            ">... No weapon!\r\n>... Can't sprint!\r\n>... <size=200%><color=red>I'm exposed!</color></size>",
            $">... Moving the {itemName}!\r\n>... Can't fight back!\r\n>... <size=200%><color=red>Defend me!</color></size>",
            ">... This is slowing me down!\r\n>... <size=200%><color=red>Unarmed!</color></size>\r\n>... Need cover!",
            $">... {itemName} secured!\r\n>... Can't use weapons!\r\n>... <size=200%><color=red>Protect me!</color></size>",
            ">... I'm helpless!\r\n>... Carrying the item!\r\n>... <size=200%><color=red>Keep them off!</color></size>",
            ">... Can't run!\r\n>... <size=200%><color=red>Walking only!</color></size>\r\n>... I need escort!",
            ">... Hands full!\r\n>... No combat!\r\n>... <size=200%><color=red>Watch my back!</color></size>",
            ">... I'm defenseless!\r\n>... <size=200%><color=red>Can't shoot!</color></size>\r\n>... Cover me!",
            ">... Moving slow!\r\n>... Can't use gear!\r\n>... <size=200%><color=red>I need protection!</color></size>",
            ">... This thing's heavy!\r\n>... <size=200%><color=red>No weapons!</color></size>\r\n>... Defend me!",
            ">... I'm exposed!\r\n>... Can't access anything!\r\n>... <size=200%><color=red>Stay close!</color></size>",
            ">... No gun!\r\n>... Can't sprint!\r\n>... <size=200%><color=red>I'm vulnerable!</color></size>",
            ">... Completely unarmed!\r\n>... <size=200%><color=red>Carrying it!</color></size>\r\n>... Need backup!",
            ">... I can't fight!\r\n>... Just walking!\r\n>... <size=200%><color=red>Guard me!</color></size>",
            ">... Too slow!\r\n>... <size=200%><color=red>Can't run!</color></size>\r\n>... Protect me!",
            ">... No defensive options!\r\n>... Carrying the item!\r\n>... <size=200%><color=red>Keep them away!</color></size>",
            ">... I'm helpless here!\r\n>... Can't use weapons!\r\n>... <size=200%><color=red>Cover me!</color></size>",
            ">... Walking speed!\r\n>... <size=200%><color=red>No combat!</color></size>\r\n>... I need help!",
            ">... This restricts everything!\r\n>... Can't fight!\r\n>... <size=200%><color=red>Defend me!</color></size>",
            ">... I'm a target!\r\n>... <size=200%><color=red>Unarmed!</color></size>\r\n>... Stay with me!",
            ">... Can't defend myself!\r\n>... Moving the item!\r\n>... <size=200%><color=red>Protect me!</color></size>",
            ">... No weapons access!\r\n>... <size=200%><color=red>I'm exposed!</color></size>\r\n>... Need cover!",
            ">... I'm vulnerable!\r\n>... Can't run!\r\n>... <size=200%><color=red>Guard me!</color></size>",
            ">... Hands are full!\r\n>... No shooting!\r\n>... <size=200%><color=red>Keep them off!</color></size>",
            ">... Can't sprint!\r\n>... <size=200%><color=red>Defenseless!</color></size>\r\n>... I need backup!",
            ">... Moving slow!\r\n>... Can't fight back!\r\n>... <size=200%><color=red>Cover me!</color></size>",
            ">... I'm exposed!\r\n>... <size=200%><color=red>No gear!</color></size>\r\n>... Protect me!",
            ">... This is restricting!\r\n>... Can't use anything!\r\n>... <size=200%><color=red>Defend me!</color></size>",
            ">... No combat ability!\r\n>... <size=200%><color=red>Walking only!</color></size>\r\n>... Stay close!",
            ">... I'm a liability!\r\n>... Can't shoot!\r\n>... <size=200%><color=red>Watch my back!</color></size>",
            ">... Can't defend!\r\n>... Carrying it!\r\n>... <size=200%><color=red>I need escort!</color></size>",
            ">... Too slow!\r\n>... <size=200%><color=red>Unarmed!</color></size>\r\n>... Keep them away!",
            ">... I'm helpless!\r\n>... No weapons!\r\n>... <size=200%><color=red>Guard me!</color></size>",
            ">... Can't run!\r\n>... Can't fight!\r\n>... <size=200%><color=red>I'm vulnerable!</color></size>",
            ">... This is heavy!\r\n>... <size=200%><color=red>No shooting!</color></size>\r\n>... Cover me!",
            ">... I'm exposed!\r\n>... Walking speed only!\r\n>... <size=200%><color=red>Protect me!</color></size>",
            ">... No gear access!\r\n>... <size=200%><color=red>Defenseless!</color></size>\r\n>... Need backup!",
            ">... Can't use weapons!\r\n>... Moving the item!\r\n>... <size=200%><color=red>Defend me!</color></size>",
            ">... I'm vulnerable!\r\n>... <size=200%><color=red>Can't sprint!</color></size>\r\n>... Stay with me!",
            ">... No combat!\r\n>... Hands full!\r\n>... <size=200%><color=red>Keep them off!</color></size>",
            ">... Can't fight!\r\n>... <size=200%><color=red>I'm exposed!</color></size>\r\n>... I need help!",
            ">... Walking only!\r\n>... No weapons!\r\n>... <size=200%><color=red>Cover me!</color></size>",
            ">... I'm defenseless!\r\n>... <size=200%><color=red>Carrying it!</color></size>\r\n>... Protect me!",
            ">... Too slow!\r\n>... Can't shoot!\r\n>... <size=200%><color=red>Guard me!</color></size>",
            ">... No defensive options!\r\n>... <size=200%><color=red>I'm a target!</color></size>\r\n>... Need cover!",
            ">... Can't access gear!\r\n>... Moving it!\r\n>... <size=200%><color=red>Defend me!</color></size>",
            ">... I'm unarmed!\r\n>... <size=200%><color=red>Walking speed!</color></size>\r\n>... Stay close!",
            ">... This restricts me!\r\n>... Can't fight!\r\n>... <size=200%><color=red>Watch my back!</color></size>",
            ">... No weapons!\r\n>... Can't run!\r\n>... <size=200%><color=red>I'm vulnerable!</color></size>",
            ">... I'm exposed!\r\n>... <size=200%><color=red>Hands full!</color></size>\r\n>... Keep them away!",
            ">... Can't defend!\r\n>... Moving slow!\r\n>... <size=200%><color=red>Cover me!</color></size>",
            ">... No combat!\r\n>... <size=200%><color=red>I'm helpless!</color></size>\r\n>... Protect me!",
            ">... Can't sprint!\r\n>... Carrying it!\r\n>... <size=200%><color=red>I need backup!</color></size>",

            // Theme 2: Team Coordination & Escort (70 messages: 25 with itemName, 45 generic)
            ">... Carrier needs escort!\r\n>... <size=200%><color=red>Stay close!</color></size>\r\n>... Protect them!",
            $">... I have the {itemName}!\r\n>... Form up around me!\r\n>... <size=200%><color=red>Keep them off!</color></size>",
            ">... Form a perimeter!\r\n>... <size=200%><color=red>Protect the carrier!</color></size>\r\n>... [footsteps]",
            $">... Moving the {itemName}!\r\n>... Need escort formation!\r\n>... <size=200%><color=red>Stay together!</color></size>",
            ">... Cover the carrier!\r\n>... They're defenseless!\r\n>... <size=200%><color=red>Watch all angles!</color></size>",
            $">... Carrying the {itemName}!\r\n>... <size=200%><color=red>Team formation!</color></size>\r\n>... Don't leave me!",
            ">... Escort formation!\r\n>... <size=200%><color=red>Protect the carrier!</color></size>\r\n>... They can't fight!",
            $">... I have the {itemName}!\r\n>... Stay with me!\r\n>... <size=200%><color=red>Cover all sides!</color></size>",
            ">... Keep close!\r\n>... Carrier is vulnerable!\r\n>... <size=200%><color=red>Watch their back!</color></size>",
            $">... Moving the {itemName}!\r\n>... <size=200%><color=red>Form up!</color></size>\r\n>... Need protection!",
            ">... Protect the carrier!\r\n>... <size=200%><color=red>Stay together!</color></size>\r\n>... They're exposed!",
            $">... {itemName} in transit!\r\n>... Team escort needed!\r\n>... <size=200%><color=red>Stay close!</color></size>",
            ">... Cover them!\r\n>... Carrier's unarmed!\r\n>... <size=200%><color=red>Watch the flanks!</color></size>",
            $">... I'm carrying the {itemName}!\r\n>... <size=200%><color=red>Need escort!</color></size>\r\n>... Form around me!",
            ">... Formation around carrier!\r\n>... <size=200%><color=red>Don't split up!</color></size>\r\n>... Keep them safe!",
            $">... Transporting {itemName}!\r\n>... Stay in formation!\r\n>... <size=200%><color=red>Watch all sides!</color></size>",
            ">... Guard the carrier!\r\n>... They can't defend!\r\n>... <size=200%><color=red>Cover them!</color></size>",
            $">... {itemName} secured!\r\n>... <size=200%><color=red>Escort formation!</color></size>\r\n>... Don't leave me!",
            ">... Carrier moving!\r\n>... Stay close!\r\n>... <size=200%><color=red>Protect them!</color></size>",
            $">... I've got the {itemName}!\r\n>... Need team protection!\r\n>... <size=200%><color=red>Form up!</color></size>",
            ">... Form on carrier!\r\n>... <size=200%><color=red>Watch their back!</color></size>\r\n>... They're vulnerable!",
            $">... Moving with {itemName}!\r\n>... Team escort!\r\n>... <size=200%><color=red>Stay together!</color></size>",
            ">... Cover the carrier!\r\n>... <size=200%><color=red>Don't split up!</color></size>\r\n>... They need us!",
            $">... Carrying {itemName}!\r\n>... <size=200%><color=red>Stay in formation!</color></size>\r\n>... Cover me!",
            ">... Protect them!\r\n>... Carrier's defenseless!\r\n>... <size=200%><color=red>Watch all angles!</color></size>",
            ">... Keep together!\r\n>... <size=200%><color=red>Guard the carrier!</color></size>\r\n>... Formation tight!",
            ">... Escort the carrier!\r\n>... They can't fight!\r\n>... <size=200%><color=red>Stay close!</color></size>",
            ">... Form a box!\r\n>... Carrier in middle!\r\n>... <size=200%><color=red>Protect them!</color></size>",
            ">... Cover all sides!\r\n>... <size=200%><color=red>Carrier moving!</color></size>\r\n>... Watch their back!",
            ">... Stay in formation!\r\n>... Protect the carrier!\r\n>... <size=200%><color=red>Don't break!</color></size>",
            ">... Guard them!\r\n>... <size=200%><color=red>Carrier's exposed!</color></size>\r\n>... Keep close!",
            ">... Team formation!\r\n>... Carrier in center!\r\n>... <size=200%><color=red>Watch all angles!</color></size>",
            ">... Don't leave them!\r\n>... <size=200%><color=red>Protect the carrier!</color></size>\r\n>... Stay together!",
            ">... Escort formation!\r\n>... They're vulnerable!\r\n>... <size=200%><color=red>Cover them!</color></size>",
            ">... Form up tight!\r\n>... <size=200%><color=red>Guard the carrier!</color></size>\r\n>... Watch flanks!",
            ">... Carrier needs cover!\r\n>... Stay close!\r\n>... <size=200%><color=red>Protect them!</color></size>",
            ">... Keep formation!\r\n>... <size=200%><color=red>Don't split up!</color></size>\r\n>... Guard them!",
            ">... Cover them!\r\n>... Carrier's moving!\r\n>... <size=200%><color=red>Watch their back!</color></size>",
            ">... Stay together!\r\n>... <size=200%><color=red>Protect the carrier!</color></size>\r\n>... They're exposed!",
            ">... Formation on carrier!\r\n>... Watch all sides!\r\n>... <size=200%><color=red>Stay close!</color></size>",
            ">... Guard them!\r\n>... <size=200%><color=red>Carrier's defenseless!</color></size>\r\n>... Keep together!",
            ">... Escort positions!\r\n>... Carrier moving!\r\n>... <size=200%><color=red>Cover them!</color></size>",
            ">... Form around them!\r\n>... <size=200%><color=red>Protect the carrier!</color></size>\r\n>... Watch angles!",
            ">... Don't break formation!\r\n>... Guard them!\r\n>... <size=200%><color=red>Stay close!</color></size>",
            ">... Carrier escort!\r\n>... <size=200%><color=red>Watch their back!</color></size>\r\n>... Keep together!",
            ">... Cover all sides!\r\n>... Carrier's vulnerable!\r\n>... <size=200%><color=red>Protect them!</color></size>",
            ">... Stay in position!\r\n>... <size=200%><color=red>Guard the carrier!</color></size>\r\n>... Don't split!",
            ">... Form up!\r\n>... Carrier needs cover!\r\n>... <size=200%><color=red>Stay together!</color></size>",
            ">... Watch their back!\r\n>... <size=200%><color=red>Carrier's exposed!</color></size>\r\n>... Keep close!",
            ">... Escort formation!\r\n>... They're defenseless!\r\n>... <size=200%><color=red>Cover them!</color></size>",
            ">... Guard them!\r\n>... Carrier moving!\r\n>... <size=200%><color=red>Watch all angles!</color></size>",
            ">... Stay together!\r\n>... <size=200%><color=red>Protect the carrier!</color></size>\r\n>... Formation tight!",
            ">... Cover them!\r\n>... They can't fight!\r\n>... <size=200%><color=red>Stay close!</color></size>",
            ">... Formation around carrier!\r\n>... <size=200%><color=red>Don't break!</color></size>\r\n>... Watch their back!",
            ">... Guard the carrier!\r\n>... Stay in position!\r\n>... <size=200%><color=red>Keep together!</color></size>",
            ">... Escort tight!\r\n>... <size=200%><color=red>Carrier's vulnerable!</color></size>\r\n>... Cover all sides!",
            ">... Protect them!\r\n>... Don't split up!\r\n>... <size=200%><color=red>Stay close!</color></size>",
            ">... Form on carrier!\r\n>... <size=200%><color=red>Watch all angles!</color></size>\r\n>... Guard them!",
            ">... Keep together!\r\n>... Carrier needs cover!\r\n>... <size=200%><color=red>Protect them!</color></size>",
            ">... Cover their back!\r\n>... <size=200%><color=red>Carrier's exposed!</color></size>\r\n>... Stay in formation!",
            ">... Guard them!\r\n>... They're defenseless!\r\n>... <size=200%><color=red>Watch flanks!</color></size>",
            ">... Formation tight!\r\n>... <size=200%><color=red>Protect the carrier!</color></size>\r\n>... Don't leave them!",
            ">... Escort positions!\r\n>... Carrier moving!\r\n>... <size=200%><color=red>Stay together!</color></size>",
            ">... Cover them!\r\n>... <size=200%><color=red>Watch their back!</color></size>\r\n>... Keep close!",
            ">... Form up!\r\n>... Guard the carrier!\r\n>... <size=200%><color=red>Stay in formation!</color></size>",
            ">... Protect them!\r\n>... Carrier's vulnerable!\r\n>... <size=200%><color=red>Watch all sides!</color></size>",
            ">... Stay together!\r\n>... <size=200%><color=red>Escort formation!</color></size>\r\n>... Don't break!",
            ">... Cover the carrier!\r\n>... They need us!\r\n>... <size=200%><color=red>Stay close!</color></size>",

            // Theme 3: Item Pickup & Transport (60 messages: 30 with itemName, 30 generic)
            $">... Found the {itemName}!\r\n>... <size=200%><color=red>Picking it up!</color></size>\r\n>... [mechanical hum]",
            ">... It's heavy!\r\n>... <size=200%><color=red>Moving slow!</color></size>\r\n>... Stay with me!",
            $">... {itemName} located!\r\n>... Securing it!\r\n>... <size=200%><color=red>Got it!</color></size>",
            ">... Item acquired!\r\n>... <size=200%><color=red>This is heavy!</color></size>\r\n>... [grunting]",
            $">... There's the {itemName}!\r\n>... <size=200%><color=red>Picking up!</color></size>\r\n>... Moving it now!",
            ">... Got the item!\r\n>... It's weighing me down!\r\n>... <size=200%><color=red>Moving slow!</color></size>",
            $">... {itemName} secured!\r\n>... <size=200%><color=red>Transporting!</color></size>\r\n>... [straining]",
            ">... Picked it up!\r\n>... <size=200%><color=red>This is heavy!</color></size>\r\n>... Need escort!",
            $">... Located the {itemName}!\r\n>... Acquiring it!\r\n>... <size=200%><color=red>Got it!</color></size>",
            ">... Item in hand!\r\n>... <size=200%><color=red>Moving out!</color></size>\r\n>... Stay close!",
            $">... {itemName} found!\r\n>... <size=200%><color=red>Picking up now!</color></size>\r\n>... [mechanical sounds]",
            ">... Secured the item!\r\n>... It's bulky!\r\n>... <size=200%><color=red>Moving slow!</color></size>",
            $">... There it is!\r\n>... The {itemName}!\r\n>... <size=200%><color=red>Grabbing it!</color></size>",
            ">... Got it!\r\n>... <size=200%><color=red>This thing's heavy!</color></size>\r\n>... Need cover!",
            $">... {itemName} in sight!\r\n>... Picking up!\r\n>... <size=200%><color=red>Secured!</color></size>",
            ">... Item acquired!\r\n>... <size=200%><color=red>Transporting!</color></size>\r\n>... Form up!",
            $">... Found it!\r\n>... The {itemName}!\r\n>... <size=200%><color=red>Taking it!</color></size>",
            ">... Picked up!\r\n>... It's restricting!\r\n>... <size=200%><color=red>Moving now!</color></size>",
            $">... {itemName} located!\r\n>... <size=200%><color=red>Securing it!</color></size>\r\n>... [clicking]",
            ">... Got the item!\r\n>... <size=200%><color=red>Heavy!</color></size>\r\n>... Stay with me!",
            $">... There's the {itemName}!\r\n>... Acquiring!\r\n>... <size=200%><color=red>Secured!</color></size>",
            ">... Item in hand!\r\n>... This is bulky!\r\n>... <size=200%><color=red>Moving slow!</color></size>",
            $">... {itemName} found!\r\n>... <size=200%><color=red>Picking up!</color></size>\r\n>... Got it!",
            ">... Acquired!\r\n>... <size=200%><color=red>It's heavy!</color></size>\r\n>... Need escort!",
            $">... Located the {itemName}!\r\n>... Securing!\r\n>... <size=200%><color=red>Got it!</color></size>",
            ">... Item secured!\r\n>... <size=200%><color=red>Transporting now!</color></size>\r\n>... Form up!",
            $">... There it is!\r\n>... {itemName}!\r\n>... <size=200%><color=red>Taking it!</color></size>",
            ">... Picked up!\r\n>... <size=200%><color=red>This is heavy!</color></size>\r\n>... [grunting]",
            $">... {itemName} in sight!\r\n>... <size=200%><color=red>Acquiring!</color></size>\r\n>... Secured!",
            ">... Got it!\r\n>... It's weighing me down!\r\n>... <size=200%><color=red>Moving!</color></size>",
            ">... Item acquired!\r\n>... <size=200%><color=red>Heavy load!</color></size>\r\n>... Stay close!",
            ">... Picked it up!\r\n>... This is bulky!\r\n>... <size=200%><color=red>Moving out!</color></size>",
            ">... Secured!\r\n>... <size=200%><color=red>It's restricting!</color></size>\r\n>... Need cover!",
            ">... Got the item!\r\n>... <size=200%><color=red>Transporting!</color></size>\r\n>... Form up!",
            ">... Item in hand!\r\n>... It's heavy!\r\n>... <size=200%><color=red>Moving slow!</color></size>",
            ">... Acquired!\r\n>... <size=200%><color=red>This is bulky!</color></size>\r\n>... Stay with me!",
            ">... Picked up!\r\n>... <size=200%><color=red>Moving it!</color></size>\r\n>... [straining]",
            ">... Secured the item!\r\n>... It's heavy!\r\n>... <size=200%><color=red>Need escort!</color></size>",
            ">... Got it!\r\n>... <size=200%><color=red>Weighing me down!</color></size>\r\n>... Moving now!",
            ">... Item acquired!\r\n>... This is restricting!\r\n>... <size=200%><color=red>Transporting!</color></size>",
            ">... Picked up!\r\n>... <size=200%><color=red>Heavy!</color></size>\r\n>... Form up!",
            ">... Secured!\r\n>... It's bulky!\r\n>... <size=200%><color=red>Moving out!</color></size>",
            ">... Got the item!\r\n>... <size=200%><color=red>This is heavy!</color></size>\r\n>... Stay close!",
            ">... Item in hand!\r\n>... Moving it!\r\n>... <size=200%><color=red>Need cover!</color></size>",
            ">... Acquired!\r\n>... <size=200%><color=red>It's restricting!</color></size>\r\n>... Moving now!",
            ">... Picked up!\r\n>... This is heavy!\r\n>... <size=200%><color=red>Transporting!</color></size>",
            ">... Secured the item!\r\n>... <size=200%><color=red>Moving slow!</color></size>\r\n>... Form up!",
            ">... Got it!\r\n>... It's bulky!\r\n>... <size=200%><color=red>Need escort!</color></size>",
            ">... Item acquired!\r\n>... <size=200%><color=red>Heavy load!</color></size>\r\n>... Moving out!",
            ">... Picked up!\r\n>... Weighing me down!\r\n>... <size=200%><color=red>Transporting!</color></size>",
            ">... Secured!\r\n>... <size=200%><color=red>This is heavy!</color></size>\r\n>... Stay with me!",
            ">... Got the item!\r\n>... It's restricting!\r\n>... <size=200%><color=red>Moving now!</color></size>",
            ">... Item in hand!\r\n>... <size=200%><color=red>Heavy!</color></size>\r\n>... Need cover!",
            ">... Acquired!\r\n>... This is bulky!\r\n>... <size=200%><color=red>Moving out!</color></size>",
            ">... Picked up!\r\n>... <size=200%><color=red>Weighing me down!</color></size>\r\n>... Form up!",
            ">... Secured the item!\r\n>... It's heavy!\r\n>... <size=200%><color=red>Transporting!</color></size>",
            ">... Got it!\r\n>... <size=200%><color=red>This is restricting!</color></size>\r\n>... Moving now!",
            ">... Item acquired!\r\n>... Heavy load!\r\n>... <size=200%><color=red>Need escort!</color></size>",

            // Theme 4: Error Alarms & Combat While Carrying (50 messages: 15 with itemName, 35 generic)
            ">... [alarm blaring]\r\n>... Pickup triggered it!\r\n>... <size=200%><color=red>Keep moving!</color></size>",
            $">... Carrier has the {itemName}!\r\n>... <size=200%><color=red>Defend them!</color></size>\r\n>... [gunfire]",
            ">... Error alarm!\r\n>... <size=200%><color=red>They're spawning!</color></size>\r\n>... Protect the carrier!",
            $">... Moving the {itemName}!\r\n>... Alarm's going!\r\n>... <size=200%><color=red>Keep fighting!</color></size>",
            ">... [klaxon wailing]\r\n>... Pickup triggered them!\r\n>... <size=200%><color=red>Cover the carrier!</color></size>",
            $">... I have the {itemName}!\r\n>... <size=200%><color=red>Error alarm!</color></size>\r\n>... They're coming!",
            ">... Alarm's blaring!\r\n>... Carrier moving!\r\n>... <size=200%><color=red>Fight them off!</color></size>",
            $">... Transporting {itemName}!\r\n>... Alarm triggered!\r\n>... <size=200%><color=red>Defend me!</color></size>",
            ">... [alarm]\r\n>... <size=200%><color=red>They're spawning!</color></size>\r\n>... Guard the carrier!",
            $">... Carrying {itemName}!\r\n>... Error alarm!\r\n>... <size=200%><color=red>Keep them off!</color></size>",
            ">... Pickup triggered alarm!\r\n>... <size=200%><color=red>Protect the carrier!</color></size>\r\n>... [gunfire]",
            $">... I've got the {itemName}!\r\n>... <size=200%><color=red>Alarm's going!</color></size>\r\n>... Cover me!",
            ">... Error alarm!\r\n>... Carrier's exposed!\r\n>... <size=200%><color=red>Fight them off!</color></size>",
            $">... Moving {itemName}!\r\n>... Alarm triggered!\r\n>... <size=200%><color=red>Defend me!</color></size>",
            ">... [klaxon]\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... Guard the carrier!",
            ">... Alarm's blaring!\r\n>... <size=200%><color=red>Carrier needs cover!</color></size>\r\n>... Keep fighting!",
            ">... Pickup alarm!\r\n>... They're spawning!\r\n>... <size=200%><color=red>Protect them!</color></size>",
            ">... Error alarm!\r\n>... <size=200%><color=red>Carrier's defenseless!</color></size>\r\n>... [gunfire]",
            ">... [alarm wailing]\r\n>... Fight while moving!\r\n>... <size=200%><color=red>Cover the carrier!</color></size>",
            ">... Alarm triggered!\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... Guard them!",
            ">... Error alarm!\r\n>... Carrier moving!\r\n>... <size=200%><color=red>Keep fighting!</color></size>",
            ">... [klaxon blaring]\r\n>... <size=200%><color=red>Spawning!</color></size>\r\n>... Protect the carrier!",
            ">... Alarm's going!\r\n>... They're exposed!\r\n>... <size=200%><color=red>Defend them!</color></size>",
            ">... Pickup triggered it!\r\n>... <size=200%><color=red>Error alarm!</color></size>\r\n>... Cover them!",
            ">... [alarm]\r\n>... Carrier's vulnerable!\r\n>... <size=200%><color=red>Fight them off!</color></size>",
            ">... Error alarm!\r\n>... <size=200%><color=red>They're spawning!</color></size>\r\n>... Guard the carrier!",
            ">... Alarm blaring!\r\n>... Keep moving!\r\n>... <size=200%><color=red>Protect them!</color></size>",
            ">... Triggered alarm!\r\n>... <size=200%><color=red>Carrier needs cover!</color></size>\r\n>... [gunfire]",
            ">... [klaxon]\r\n>... They're coming!\r\n>... <size=200%><color=red>Defend the carrier!</color></size>",
            ">... Error alarm!\r\n>... <size=200%><color=red>Fight while moving!</color></size>\r\n>... Cover them!",
            ">... Alarm's going!\r\n>... Carrier's exposed!\r\n>... <size=200%><color=red>Keep fighting!</color></size>",
            ">... Pickup alarm!\r\n>... <size=200%><color=red>They're spawning!</color></size>\r\n>... Guard them!",
            ">... [alarm wailing]\r\n>... Carrier moving!\r\n>... <size=200%><color=red>Protect them!</color></size>",
            ">... Error alarm!\r\n>... They're defenseless!\r\n>... <size=200%><color=red>Cover the carrier!</color></size>",
            ">... Alarm triggered!\r\n>... <size=200%><color=red>Keep moving!</color></size>\r\n>... Fight them off!",
            ">... [klaxon blaring]\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... Defend them!",
            ">... Error alarm!\r\n>... Carrier needs cover!\r\n>... <size=200%><color=red>Keep fighting!</color></size>",
            ">... Alarm's going!\r\n>... <size=200%><color=red>Spawning!</color></size>\r\n>... Protect the carrier!",
            ">... Pickup triggered it!\r\n>... They're exposed!\r\n>... <size=200%><color=red>Guard them!</color></size>",
            ">... [alarm]\r\n>... <size=200%><color=red>Carrier's vulnerable!</color></size>\r\n>... [gunfire]",
            ">... Error alarm!\r\n>... Fight while moving!\r\n>... <size=200%><color=red>Cover them!</color></size>",
            ">... Alarm blaring!\r\n>... <size=200%><color=red>They're spawning!</color></size>\r\n>... Defend the carrier!",
            ">... Triggered alarm!\r\n>... Carrier moving!\r\n>... <size=200%><color=red>Keep fighting!</color></size>",
            ">... [klaxon]\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... Protect them!",
            ">... Error alarm!\r\n>... They're defenseless!\r\n>... <size=200%><color=red>Guard the carrier!</color></size>",
            ">... Alarm's going!\r\n>... <size=200%><color=red>Keep moving!</color></size>\r\n>... Cover them!",
            ">... Pickup alarm!\r\n>... Carrier needs cover!\r\n>... <size=200%><color=red>Fight them off!</color></size>",
            ">... [alarm wailing]\r\n>... <size=200%><color=red>Spawning!</color></size>\r\n>... Defend them!",
            ">... Error alarm!\r\n>... Carrier's exposed!\r\n>... <size=200%><color=red>Protect them!</color></size>",
            ">... Alarm triggered!\r\n>... <size=200%><color=red>They're coming!</color></size>\r\n>... Guard the carrier!",

            // Theme 5: Item Dropping & Re-pickup (20 messages: 10 with itemName, 10 generic)
            $">... Dropping the {itemName}!\r\n>... <size=200%><color=red>Need to fight!</color></size>\r\n>... We'll get it after!",
            ">... Had to drop it!\r\n>... Clear them out!\r\n>... <size=200%><color=red>Then pick it back up!</color></size>",
            $">... Setting down the {itemName}!\r\n>... <size=200%><color=red>Fight first!</color></size>\r\n>... [thud]",
            ">... Dropped the item!\r\n>... <size=200%><color=red>Kill them!</color></size>\r\n>... Then re-pickup!",
            $">... {itemName} down!\r\n>... Need my weapon!\r\n>... <size=200%><color=red>Clear the area!</color></size>",
            ">... Item dropped!\r\n>... <size=200%><color=red>Fight them!</color></size>\r\n>... We'll get it after!",
            $">... Dropping {itemName}!\r\n>... <size=200%><color=red>Too many of them!</color></size>\r\n>... Need to fight!",
            ">... Had to set it down!\r\n>... Clear the zone!\r\n>... <size=200%><color=red>Then pick up!</color></size>",
            $">... {itemName} dropped!\r\n>... <size=200%><color=red>Need my gun!</color></size>\r\n>... Fight first!",
            ">... Dropped it!\r\n>... <size=200%><color=red>Kill them all!</color></size>\r\n>... Then re-secure!",
            ">... Item down!\r\n>... Fight first!\r\n>... <size=200%><color=red>Pick up after!</color></size>",
            ">... Had to drop it!\r\n>... <size=200%><color=red>Too many!</color></size>\r\n>... Clear them out!",
            ">... Dropped the item!\r\n>... Need to fight!\r\n>... <size=200%><color=red>Get it after!</color></size>",
            ">... Item set down!\r\n>... <size=200%><color=red>Clear the area!</color></size>\r\n>... Then pickup!",
            ">... Dropped it!\r\n>... <size=200%><color=red>Fight them!</color></size>\r\n>... [gunfire]",
            ">... Had to set it down!\r\n>... Kill them!\r\n>... <size=200%><color=red>Then re-pickup!</color></size>",
            ">... Item dropped!\r\n>... <size=200%><color=red>Need my weapon!</color></size>\r\n>... Fight first!",
            ">... Dropped it!\r\n>... Clear the zone!\r\n>... <size=200%><color=red>Pick up after!</color></size>",
            ">... Item down!\r\n>... <size=200%><color=red>Too many of them!</color></size>\r\n>... Fight back!",
            ">... Had to drop it!\r\n>... <size=200%><color=red>Kill them all!</color></size>\r\n>... Then secure!",

            // Theme 6: Extraction with Item (20 messages: 10 with itemName, 10 generic)
            $">... {itemName} in the scan!\r\n>... <size=200%><color=red>Extraction starting!</color></size>\r\n>... [scan humming]",
            ">... Get it in the circle!\r\n>... <size=200%><color=red>Extraction zone!</color></size>\r\n>... Almost there!",
            $">... {itemName} at extraction!\r\n>... <size=200%><color=red>Scan starting!</color></size>\r\n>... Hold position!",
            ">... Item in scan zone!\r\n>... <size=200%><color=red>Extraction active!</color></size>\r\n>... [scanning]",
            $">... {itemName} in circle!\r\n>... Extraction initiated!\r\n>... <size=200%><color=red>Almost out!</color></size>",
            ">... Item at extraction!\r\n>... <size=200%><color=red>Scan starting!</color></size>\r\n>... Hold here!",
            $">... {itemName} in scan!\r\n>... <size=200%><color=red>Extraction running!</color></size>\r\n>... [humming]",
            ">... Get it in!\r\n>... <size=200%><color=red>Extraction zone!</color></size>\r\n>... Almost done!",
            $">... {itemName} at extraction!\r\n>... <size=200%><color=red>Scan active!</color></size>\r\n>... Hold position!",
            ">... Item in circle!\r\n>... Extraction initiated!\r\n>... <size=200%><color=red>Nearly there!</color></size>",
            ">... Item at extraction!\r\n>... <size=200%><color=red>Scan starting!</color></size>\r\n>... [scanning]",
            ">... Get it in the scan!\r\n>... <size=200%><color=red>Extraction zone!</color></size>\r\n>... Almost out!",
            ">... Item in circle!\r\n>... <size=200%><color=red>Extraction active!</color></size>\r\n>... Hold here!",
            ">... Item at extraction!\r\n>... Scan running!\r\n>... <size=200%><color=red>Almost done!</color></size>",
            ">... Get it in!\r\n>... <size=200%><color=red>Extraction initiated!</color></size>\r\n>... [humming]",
            ">... Item in scan zone!\r\n>... <size=200%><color=red>Extraction starting!</color></size>\r\n>... Hold position!",
            ">... Item at extraction!\r\n>... <size=200%><color=red>Scan active!</color></size>\r\n>... Nearly there!",
            ">... Get it in the circle!\r\n>... Extraction zone!\r\n>... <size=200%><color=red>Almost out!</color></size>",
            ">... Item in scan!\r\n>... <size=200%><color=red>Extraction running!</color></size>\r\n>... [scanning]",
            ">... Item at extraction!\r\n>... Scan initiated!\r\n>... <size=200%><color=red>Almost done!</color></size>",
        }))!);
        #endregion
    }
}
