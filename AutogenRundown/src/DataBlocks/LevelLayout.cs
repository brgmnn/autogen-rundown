using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Items;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public enum SizeFactor
{
    Small,
    Medium,
    Large
}

public partial record LevelLayout : DataBlock
{
    #region hidden data
    [JsonIgnore]
    private Level level;

    [JsonIgnore]
    private BuildDirector director;

    [JsonIgnore]
    private RelativeDirection direction;

    [JsonIgnore]
    private LayoutPlanner planner;

    [JsonIgnore]
    private LevelSettings settings;
    #endregion

    public int ZoneAliasStart { get; set; }

    public List<Zone> Zones { get; set; } = new();

    public LevelLayout(Level level, BuildDirector director, LevelSettings settings, LayoutPlanner planner)
    {
        // Always assign a new persistent id upfront to level layout
        PersistentId = Generator.GetPersistentId();

        this.director = director;
        this.level = level;
        this.planner = planner;
        this.settings = settings;
    }

    /// <summary>
    /// Roll for door alarms
    /// </summary>
    public void RollAlarms(
        ICollection<(double, int, ChainedPuzzle)> puzzlePack,
        ICollection<(double, int, WavePopulation)> wavePopulationPack,
        ICollection<(double, int, WaveSettings)> waveSettingsPack)
    {
        foreach (var zone in Zones)
            zone.RollAlarms(level, this, puzzlePack, wavePopulationPack, waveSettingsPack);
    }

    /// <summary>
    /// Roll for blood doors
    ///
    /// TODO: rework with custom enemy groups
    /// TODO: increase difficulty, these are generally just too easy
    /// </summary>
    public void RollBloodDoors()
    {
        var count = 0;
        var (max, chance, inAreaChance) = director.Tier switch
        {
            // No blood doors for A
            "A" => (0, 0.0, 0.0),
            "B" => (1, 0.2, 0.3),
            "C" => (2, 0.15, 0.5),
            "D" => (3, 0.15, 0.5),
            _ => (-1, 0.2, 0.7)
        };

        var withInfection = level.FogSettings.IsInfectious;

        // Ensure that there are at least as many groups as 2x the max number of blood doors
        // that can spawn. For unlimited cap tiers (D and E) this is 2x the number of zones.
        // Door pack is used to select enemies that spawn behind the door.
        var doorPack = director.Tier switch
        {
            "B" => new List<(double, int, VanillaEnemyGroup)>
            {
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Medium)
            },

            "C" => new List<(double, int, VanillaEnemyGroup)>
            {
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Easy),
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Medium),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Bigs),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Hybrids_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Hybrids_Medium)
            },

            "D" => new List<(double, int, VanillaEnemyGroup)>
            {
                (1.0, 3, VanillaEnemyGroup.BloodDoor_Medium),
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Bigs),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Chargers_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_ChargersGiant_Easy),
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Hybrids_Medium),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Shadows_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_BossMother)
            },

            "E" => new List<(double, int, VanillaEnemyGroup)>
            {
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Medium),
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Bigs),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Chargers_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_ChargersGiant_Easy),
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Hybrids_Medium),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Shadows_Easy),
                (1.0, 2, VanillaEnemyGroup.BloodDoor_BossMother),

                (withInfection ? 10.0 : 1.0, 2, (VanillaEnemyGroup)EnemyGroup.BloodDoor_HybridInfected_Hard.PersistentId)
            },

            _ => new List<(double, int, VanillaEnemyGroup)>()
        };


        // Area pack picks enemies to spawn further back, if we successfully roll to add them.
        var areaPack = director.Tier switch
        {
            "B" => new List<(double, int, VanillaEnemyGroup)>
            {
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Medium)
            },

            "C" => new List<(double, int, VanillaEnemyGroup)>
            {
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Easy),
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Medium),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Bigs),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Chargers_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Hybrids_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Shadows_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Pouncers)
            },

            "D" => new List<(double, int, VanillaEnemyGroup)>
            {
                (1.0, 3, VanillaEnemyGroup.BloodDoor_Medium),
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Bigs),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Chargers_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_ChargersGiant_Easy),
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Hybrids_Medium),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Shadows_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_BossMother),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Pouncers),

                (withInfection ? 5.0 : 1.0, 1, (VanillaEnemyGroup)EnemyGroup.BloodDoor_HybridInfected_Hard.PersistentId)
            },

            "E" => new List<(double, int, VanillaEnemyGroup)>
            {
                (1.0, 3, VanillaEnemyGroup.BloodDoor_Medium),
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Bigs),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Chargers_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_ChargersGiant_Easy),
                (1.0, 2, VanillaEnemyGroup.BloodDoor_Hybrids_Medium),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Shadows_Easy),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_BossMother),
                (1.0, 1, VanillaEnemyGroup.BloodDoor_Pouncers),

                (withInfection ? 10.0 : 1.0, 2, (VanillaEnemyGroup)EnemyGroup.BloodDoor_HybridInfected_Hard.PersistentId)
            },

            _ => new List<(double, int, VanillaEnemyGroup)>()
        };

        // Do not add blood doors to Zone 0, these are always either the elevator or bulkhead doors.
        // Do not add blood doors to Apex security doors
        foreach (var zone in Zones)
            if (!planner.GetZoneNode(zone.LocalIndex).Tags.Contains("no_blood_door") &&
                zone.LocalIndex > 0 &&
                zone.SecurityGateToEnter == SecurityGate.Security &&
                Generator.Flip(chance) &&
                (count++ < max || max == -1))
            {
                var withArea = Generator.Flip(inAreaChance);

                zone.BloodDoor = new BloodDoor
                {
                    EnemyGroupInfrontOfDoor = (uint)Generator.DrawSelect(doorPack),
                    EnemyGroupInArea = withArea ? (uint)Generator.DrawSelect(areaPack) : 0,
                    EnemyGroupsInArea = withArea ? 1 : 0,
                };
            }
    }

    /// <summary>
    /// Roll enemies for each zone.
    ///
    /// TODO: we might want to use an enemies pack for building this
    /// </summary>
    public void RollEnemies(BuildDirector director)
    {
        // All scouts cost 5pts each
        var (chance, max, scoutPack) = director.Tier switch
        {
            "A" => (0.2, 2, new List<EnemySpawningData>
                {
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 5 },
                }),
            "B" => (0.2, 3, new List<EnemySpawningData>
                {
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 10 },
                }),
            "C" => (0.2, 5, new List<EnemySpawningData>
                {
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 10 },
                    EnemySpawningData.Scout with { Points = 10 },
                    EnemySpawningData.Scout with { Points = 15 },

                    // Chargers
                    EnemySpawningData.ScoutCharger with { Points = 5 },
                    EnemySpawningData.ScoutCharger with { Points = 5 },
                }),

            "D" => (0.3, -1, new List<EnemySpawningData>
                {
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 10 },
                    EnemySpawningData.Scout with { Points = 10 },
                    EnemySpawningData.Scout with { Points = 15 },

                    // Zoomers
                    EnemySpawningData.ScoutZoomer with { Points = 5 },
                    EnemySpawningData.ScoutZoomer with { Points = 5 },
                    EnemySpawningData.ScoutZoomer with { Points = 5 },
                    EnemySpawningData.ScoutZoomer with { Points = 10 },
                    EnemySpawningData.ScoutZoomer with { Points = 10 },

                    // Chargers
                    EnemySpawningData.ScoutCharger with { Points = 5 },
                    EnemySpawningData.ScoutCharger with { Points = 5 },
                    EnemySpawningData.ScoutCharger with { Points = 5 },
                    EnemySpawningData.ScoutCharger with { Points = 10 },

                    // Shadows
                    EnemySpawningData.ScoutShadow with { Points = 5 },
                    EnemySpawningData.ScoutShadow with { Points = 5 },
                    EnemySpawningData.ScoutShadow with { Points = 5 },
                    EnemySpawningData.ScoutShadow with { Points = 10 },

                    // Nightmare
                    EnemySpawningData.ScoutNightmare with { Points = 5 },
                    EnemySpawningData.ScoutNightmare with { Points = 5 },
                    EnemySpawningData.ScoutNightmare with { Points = 5 },
                    EnemySpawningData.ScoutNightmare with { Points = 10 },
                }),

            "E" => (0.3, -1, new List<EnemySpawningData>
                {
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 5 },
                    EnemySpawningData.Scout with { Points = 10 },
                    EnemySpawningData.Scout with { Points = 10 },
                    EnemySpawningData.Scout with { Points = 15 },

                    // Zoomers
                    EnemySpawningData.ScoutZoomer with { Points = 5 },
                    EnemySpawningData.ScoutZoomer with { Points = 5 },
                    EnemySpawningData.ScoutZoomer with { Points = 5 },
                    EnemySpawningData.ScoutZoomer with { Points = 10 },
                    EnemySpawningData.ScoutZoomer with { Points = 10 },

                    // Chargers
                    EnemySpawningData.ScoutCharger with { Points = 5 },
                    EnemySpawningData.ScoutCharger with { Points = 5 },
                    EnemySpawningData.ScoutCharger with { Points = 5 },
                    EnemySpawningData.ScoutCharger with { Points = 10 },

                    // Shadows
                    EnemySpawningData.ScoutShadow with { Points = 5 },
                    EnemySpawningData.ScoutShadow with { Points = 5 },
                    EnemySpawningData.ScoutShadow with { Points = 5 },
                    EnemySpawningData.ScoutShadow with { Points = 10 },

                    // Nightmare
                    EnemySpawningData.ScoutNightmare with { Points = 5 },
                    EnemySpawningData.ScoutNightmare with { Points = 5 },
                    EnemySpawningData.ScoutNightmare with { Points = 5 },
                    EnemySpawningData.ScoutNightmare with { Points = 10 },
                }),

            _ => (0.0, 0, new List<EnemySpawningData>())
        };
        var bossChance = director.Tier switch
        {
            "C" => 0.15,
            "D" => 0.20,
            "E" => 0.25,
            _ => 0.0
        };

        var scoutCount = 0;
        var zoneNodes = planner.GetZones(director.Bulkhead, null);

        foreach (var node in zoneNodes)
        {
            var zone = planner.GetZone(node);

            if (zone == null)
            {
                Plugin.Logger.LogWarning($"No zone found for ZoneNode: {node}");
                continue;
            }

            // Skip adding any enemies to the reactor area
            // TODO: we may want to add a chance for some enemies here
            if (node.Tags.Contains("reactor") || node.Tags.Contains("no_enemies"))
                continue;

            var points = director.GetPoints(zone);

            // Reduce the chance of scouts spawning in the zone if there's a blood door to enter.
            var scoutRollModifier = zone.BloodDoor.Enabled ? 0.5 : 1.0;

            // Roll for adding scouts
            // Do not role if the zone is tagged with `no_scouts`
            if (!node.Tags.Contains("no_scouts") &&
                Generator.Flip(chance * scoutRollModifier) &&
                (scoutCount++ < max || max == -1))
            {
                var scout = Generator.Draw(scoutPack);

                // Add scouts with force one, this is to guarantee we get exactly the right
                // number of scouts.
                if (scout != null)
                {
                    points = scout.Points;

                    for (int i = 0; i < scout.Points / 5; i++)
                        zone.EnemySpawningInZone.Add(
                            scout with { Distribution = EnemyZoneDistribution.ForceOne, Points = 25 });
                }
            }

            // If we have run out of points, skip adding enemies.
            if (points < 3)
                continue;

            // If we have a blood door, reduce the number of enemies that spawn in the zone
            // by 1/3rd.
            if (zone.BloodDoor.Enabled)
                points = (int)(points * 0.66);

            #region Charger roll check
            var chargerChance = 0.0;

            if (settings.Modifiers.Contains(LevelModifiers.Chargers))
                chargerChance = 0.2;

            if (settings.Modifiers.Contains(LevelModifiers.ManyChargers))
                chargerChance = 0.5;
            #endregion

            #region Shadows roll check
            var shadowChance = 0.0;

            if (settings.Modifiers.Contains(LevelModifiers.Shadows))
                shadowChance = 0.15;

            if (settings.Modifiers.Contains(LevelModifiers.ManyShadows))
                shadowChance = 0.5;
            #endregion

            #region Hybrid roll check
            var hybridChance = 0.0;

            if (settings.Modifiers.Contains(LevelModifiers.Hybrids))
                hybridChance = 0.2;
            #endregion

            #region Nightmares roll check
            var nightmaresChance = 0.0;

            if (settings.Modifiers.Contains(LevelModifiers.Nightmares))
                nightmaresChance = 0.2;

            if (settings.Modifiers.Contains(LevelModifiers.ManyNightmares))
                nightmaresChance = 0.5;
            #endregion

            // Boss settings
            // TODO: don't have totally independent of zone points
            if (Generator.Flip(bossChance) && settings.EnemyBossPack.Any())
            {
                var boss = Generator.DrawSelect(settings.EnemyBossPack);

                if (boss != null)
                {
                    zone.EnemySpawningInZone.Add(boss);
                    Plugin.Logger.LogDebug($"{Name} -- Zone {zone.LocalIndex} rolled a boss!");
                }
            }

            var groupChoices = new List<(double, List<AutogenDifficulty>)>
            {
                (1.0, new List<AutogenDifficulty> { AutogenDifficulty.Base }),

                // Chargers
                (chargerChance, new List<AutogenDifficulty> { AutogenDifficulty.Chargers }),
                (chargerChance, new List<AutogenDifficulty>
                {
                    AutogenDifficulty.Base,
                    AutogenDifficulty.Chargers
                }),

                // Shadows
                (shadowChance, new List<AutogenDifficulty> { AutogenDifficulty.Shadows }),
                (shadowChance, new List<AutogenDifficulty>
                {
                    AutogenDifficulty.Base,
                    AutogenDifficulty.Shadows
                }),

                // Hybrid is always mixed
                (hybridChance, new List<AutogenDifficulty>
                {
                    AutogenDifficulty.Base,
                    AutogenDifficulty.Hybrids
                }),

                // Nightmares
                (nightmaresChance, new List<AutogenDifficulty> { AutogenDifficulty.Nightmares }),
                (nightmaresChance, new List<AutogenDifficulty>
                {
                    AutogenDifficulty.Base,
                    AutogenDifficulty.Nightmares
                }),
            };

            if (chargerChance > 0 && shadowChance > 0)
                groupChoices.Add(
                    (0.2, new List<AutogenDifficulty>
                        {
                            AutogenDifficulty.Base,
                            AutogenDifficulty.Chargers,
                            AutogenDifficulty.Shadows
                        }));

            if (chargerChance > 0 && nightmaresChance > 0)
                groupChoices.Add(
                    (0.2, new List<AutogenDifficulty>
                    {
                        AutogenDifficulty.Base,
                        AutogenDifficulty.Chargers,
                        AutogenDifficulty.Nightmares
                    }));

            if (shadowChance > 0 && nightmaresChance > 0)
                groupChoices.Add(
                    (0.2, new List<AutogenDifficulty>
                    {
                        AutogenDifficulty.Base,
                        AutogenDifficulty.Shadows,
                        AutogenDifficulty.Nightmares
                    }));

            if (chargerChance > 0 && shadowChance > 0 && hybridChance > 0)
                groupChoices.Add(
                    (0.1, new List<AutogenDifficulty>
                        {
                            AutogenDifficulty.Chargers,
                            AutogenDifficulty.Shadows,
                            AutogenDifficulty.Hybrids
                        }));

            if (chargerChance > 0 && nightmaresChance > 0 && hybridChance > 0)
                groupChoices.Add(
                    (0.1, new List<AutogenDifficulty>
                    {
                        AutogenDifficulty.Chargers,
                        AutogenDifficulty.Nightmares,
                        AutogenDifficulty.Hybrids
                    }));

            if (shadowChance > 0 && nightmaresChance > 0 && hybridChance > 0)
                groupChoices.Add(
                    (0.1, new List<AutogenDifficulty>
                    {
                        AutogenDifficulty.Shadows,
                        AutogenDifficulty.Nightmares,
                        AutogenDifficulty.Hybrids
                    }));

            // TODO: TBD if we like having a room with literally everything in it
            if (chargerChance > 0 && shadowChance > 0 && nightmaresChance > 0 && hybridChance > 0)
                groupChoices.Add(
                    (0.1, new List<AutogenDifficulty>
                    {
                        AutogenDifficulty.Chargers,
                        AutogenDifficulty.Shadows,
                        AutogenDifficulty.Nightmares,
                        AutogenDifficulty.Hybrids
                    }));


            var groups = Generator.Select(groupChoices);

            Plugin.Logger.LogDebug($"{Name} -- Zone {zone.LocalIndex} has {points}pts for enemies. Groups: {string.Join(", ", groups)}");

            // By default we will just let the spawning data allocate out groups. If there
            // are multiple groups we just spawn equal numbers of them and let the game
            // divide that up into portions.
            foreach (var group in groups)
                zone.EnemySpawningInZone.Add(
                    new EnemySpawningData
                    {
                        GroupType = EnemyGroupType.Hibernate,
                        Difficulty = director.Tier switch
                        {
                            "A" => (uint)(AutogenDifficulty.TierA | group),
                            "B" => (uint)(AutogenDifficulty.TierB | group),
                            "C" => (uint)(AutogenDifficulty.TierC | group),
                            "D" => (uint)(AutogenDifficulty.TierD | group),
                            "E" => (uint)(AutogenDifficulty.TierE | group),
                            _ => (uint)(AutogenDifficulty.TierC | group)
                        },
                        // We manually do some point adjustment, as some enemy spawns
                        // (nightmares) are far harder than others. And others are easier
                        Points = (int)((group, level.Tier) switch
                        {
                            (AutogenDifficulty.Nightmares, "C") => points / 2.0,
                            (AutogenDifficulty.Nightmares, "D") => points / 1.5,
                            (AutogenDifficulty.Nightmares, "E") => points / 1.2,

                            (AutogenDifficulty.Shadows, "E") => points * 1.2,

                            _ => points
                        } / groups.Count)
                    });
        }
    }

    /// <summary>
    /// Rolls for whether we should add an error alarm to this level layout.
    /// </summary>
    public void RollErrorAlarm()
    {
        var alarmCount = director.Tier switch
        {
            // No error alarms for A/B
            "A" => 0,
            "B" => 0,
            "C" => Generator.Flip(0.1) ? 1 : 0,
            "D" => Generator.Flip(0.3) ? 1 : 0,
            "E" => Generator.Select(new List<(double, int)>
            {
                (0.15, 0),
                (0.50, 1),
                (0.35, 2)
            }),

            _ => 0
        };

        // No need to process further if we have no error alarm
        if (alarmCount < 1)
            return;

        for (var i = 0; i < alarmCount; i++)
        {
            // Return early as soon as we hit the error alarm max for this level
            if (level.Settings.ErrorAlarmZones.Count >= level.Settings.MaxErrorAlarms)
                return;

            // Error wave settings
            var settings = level.Tier switch
            {
                "C" => Generator.Flip() ? WaveSettings.Error_Easy : WaveSettings.Error_Normal,
                "D" => Generator.Flip() ? WaveSettings.Error_Normal : WaveSettings.Error_Hard,
                "E" => Generator.Flip() ? WaveSettings.Error_Hard : WaveSettings.Error_VeryHard,

                _ => WaveSettings.Error_Easy,
            };

            var population = WavePopulation.Baseline;

            // First set shadows if we have them
            if (level.Settings.HasShadows())
                population = Generator.Flip(0.4) ? WavePopulation.OnlyShadows : WavePopulation
                    .Baseline_Shadows;

            // Next check and set chargers first, then flyers
            if (level.Settings.HasChargers())
                population = WavePopulation.Baseline_Chargers;
            else if (level.Settings.HasFlyers())
                population = WavePopulation.Baseline_Flyers;

            // First try and find a zone in the middle without an alarm already.
            var candidates = Zones
                .Where(z => z.LocalIndex != 0 &&
                            z.Alarm == ChainedPuzzle.None &&
                            z.LocalIndex != Zones.Count - 1 &&
                            z.ProgressionPuzzleToEnter != ProgressionPuzzle.Locked &&
                            z.SecurityGateToEnter != SecurityGate.Apex);

            // If no candidates, search for any zone in the middle (we will overwrite the alarm)
            if (!candidates.Any())
                candidates = Zones.Where(z => z.LocalIndex != 0 &&
                                              z.LocalIndex != Zones.Count - 1 &&
                                              z.ProgressionPuzzleToEnter != ProgressionPuzzle.Locked &&
                                              z.SecurityGateToEnter != SecurityGate.Apex);

            // If there's still no candidates, include the last zone. Note this probably never
            // gets called as all levels have at least 3 zones.
            if (!candidates.Any())
                candidates = Zones.Where(z => z.LocalIndex != 0 &&
                                              z.ProgressionPuzzleToEnter != ProgressionPuzzle.Locked &&
                                              z.SecurityGateToEnter != SecurityGate.Apex);

            // Pick from all zones without alarms already that aren't the first zone
            var zone = Generator.Pick(candidates);

            // Something's gone wrong if this is the case and there were no zones to pick from.
            if (zone == null)
                return;

            var node = planner.GetZoneNode(zone.LocalIndex);
            var terminal = (ZoneNode?)null;
            // zone.Alarm = ChainedPuzzle.FindOrPersist(puzzle);

            level.Settings.ErrorAlarmZones.Add(node);

            // Give a flat chance of being able to turn off the alarm.
            if (Generator.Flip(0.7))
            {
                var branchOpenZones = planner.GetOpenZones(director.Bulkhead, node.Branch);

                // Fallback if there's no open zones in this branch. This will be _hard_.
                if (branchOpenZones.Count < 1)
                    branchOpenZones = planner.GetOpenZones(director.Bulkhead);

                // There's no open zones anywhere on this bulkhead. So bad luck this alarm can't be turned off.
                // TODO: we could fall back to setting the turnoff in an existing forward zone?
                if (!branchOpenZones.Any())
                    continue;

                var baseNode = Generator.Pick(branchOpenZones);
                var turnOff = new ZoneNode(director.Bulkhead, planner.NextIndex(director.Bulkhead), $"error_off_{i}");

                planner.Connect(baseNode, turnOff);
                planner.AddZone(
                    turnOff,
                    new Zone(level)
                    {
                        Coverage = new CoverageMinMax(Generator.NextDouble(40, 80)),
                        LightSettings = Lights.GenRandomLight(),
                    });

                var turnOffZone = planner.GetZone(turnOff)!;

                // Unlock the turn-off zone door when the alarm door has opened.
                // zone.EventsOnDoorScanDone.AddUnlockDoor(director.Bulkhead, turnOff.ZoneNumber);

                turnOffZone.ProgressionPuzzleToEnter = ProgressionPuzzle.Locked;

                Plugin.Logger.LogDebug($"{Name} -- Zone {zone.LocalIndex} error alarm can be disable in: Zone {turnOff.ZoneNumber}");

                // For now set the alarm to be in the next zone.
                // zone.TerminalPuzzleZone.LocalIndex = turnOff.ZoneNumber;
                // zone.TurnOffAlarmOnTerminal = true;

                // TODO: remove when we move roll alarms to use planner entirely.
                Zones.Add(turnOffZone);

                terminal = turnOff;
            }

            AddErrorAlarm(node, terminal, settings, population);
        }
    }

    /// <summary>
    /// Generates a number to be used for level layout generation based on size factors for the inputs
    /// </summary>
    /// <param name="tier"></param>
    /// <param name="bulkhead"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    int GenNumZones(string tier, Bulkhead bulkhead, SizeFactor size)
        => (tier, bulkhead, size) switch
        {
            ("A", Bulkhead.Main, _) => 1,
            ("A", Bulkhead.Extreme, _) => 1,
            ("A", Bulkhead.Overload, _) => 1,

            ("B", Bulkhead.Main, _) => 1,
            ("B", Bulkhead.Extreme, _) => 1,
            ("B", Bulkhead.Overload, _) => 1,

            ("C", Bulkhead.Main, _) => 1,
            ("C", Bulkhead.Extreme, _) => 1,
            ("C", Bulkhead.Overload, _) => 1,

            ("D", Bulkhead.Main, _) => 1,
            ("D", Bulkhead.Extreme, _) => 1,
            ("D", Bulkhead.Overload, _) => 1,

            ("E", Bulkhead.Main, _) => 1,
            ("E", Bulkhead.Extreme, _) => 1,
            ("E", Bulkhead.Overload, _) => 1,

            (_, _, _) => 1
        };

    /// <summary>
    /// Builds the main level
    ///
    /// Objective is not a fully initialized objective, it is a pre-built objective with just
    /// the basics needed for level generation
    /// </summary>
    /// <param name="level"></param>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="direction">What direction we should build this level layout for</param>
    /// <returns></returns>
    public static LevelLayout Build(
        Level level,
        BuildDirector director,
        WardenObjective objective)
    {
        var direction = level.Settings.GetDirections(director.Bulkhead);
        var layout = new LevelLayout(level, director, level.Settings, level.Planner)
        {
            Name = $"{level.Tier}{level.Index} {level.Name} {director.Bulkhead}",
            ZoneAliasStart = level.GetZoneAliasStart(director.Bulkhead),
            direction = direction
        };

        director.GenZones();

        var puzzlePack = ChainedPuzzle.BuildPack(level.Tier, level.Settings);
        var wavePopulationPack = WavePopulation.BuildPack(level.Tier, level.Settings);
        var waveSettingsPack = WaveSettings.BuildPack(level.Tier);

        Plugin.Logger.LogDebug($"Building layout ({layout.Name}), Objective = {objective.Type}");

        if (objective.Type == WardenObjectiveType.RetrieveBigItems)
            Plugin.Logger.LogDebug($" -- Retrieve Item(s) = {objective.RetrieveItems.First()}");

        if (director.Bulkhead == Bulkhead.Main)
            layout.BuildStartingArea(director);

        // Use the helper method to get the first starting zone. This will also initialize it if
        // the first zone hasn't been set up yet
        var (start, startZone) = layout.StartingArea_GetBuildStart(director.Bulkhead);

        #region Set the right directions
        /*
         * Attempt to set the very first zones build expansion and direction
         */
        startZone.ZoneExpansion = direction.Forward;
        startZone.StartExpansion = direction.Forward switch
        {
            ZoneExpansion.Forward => ZoneBuildExpansion.Forward,
            ZoneExpansion.Backward => ZoneBuildExpansion.Backward,
            ZoneExpansion.Left => ZoneBuildExpansion.Left,
            ZoneExpansion.Right => ZoneBuildExpansion.Right
        };
        #endregion

        switch (director.Objective)
        {
            /**
             * Reactor startup has quite a complicated layout construction for the fetch codes version
             * */
            case WardenObjectiveType.ReactorStartup:
                {
                    if (objective.ReactorStartupGetCodes)
                        layout.BuildLayout_ReactorStartup_FetchCodes(director, objective, start);
                    else
                        layout.BuildLayout_ReactorStartup_Simple(director, objective, start);

                    break;
                }

            /**
             * In some ways similar to the ReactorStartup mission but much shorter/simpler
             * */
            case WardenObjectiveType.ReactorShutdown:
            {
                layout.BuildLayout_ReactorShutdown(director, objective, start);
                break;
            }

            /**
             * Clear Path expeditions should generally be a lot of fun and tough to clear through
             * */
            case WardenObjectiveType.ClearPath:
            {
                layout.BuildLayout_ClearPath(director, objective, start);
                break;
            }

            /**
             * These all involve entering a command on a terminal, though this includes things
             * like King of the Hill
             */
            case WardenObjectiveType.SpecialTerminalCommand:
            {
                layout.BuildLayout_SpecialTerminalCommand(director, objective, start);
                break;
            }

            /**
             * Big items are often single, but we can spawn multiple big items (up to 4 for
             * E levels). Custom logic for interesting geo's should be added here.
             * */
            case WardenObjectiveType.RetrieveBigItems:
                {
                    layout.BuildLayout_RetrieveBigItems(director, objective, start);
                    break;
                }

            /**
             * When building the power cell distribution layout, here we are modelling a hub with offshoot zones.
             * */
            case WardenObjectiveType.PowerCellDistribution:
                {
                    layout.BuildLayout_PowerCellDistribution(director, objective, start);
                    break;
                }

            /**
             * --- DO NOT USE ---
             * This objective is completely bugged
             * */
            case WardenObjectiveType.CentralGeneratorCluster:
            {
                layout.BuildLayout_CentralGeneratorCluster(director, objective, start);
                break;
            }

            /**
             * Survival is a very custom objective
             */
            case WardenObjectiveType.Survival:
            {
                layout.BuildLayout_Survival(director, objective, start);
                break;
            }

            /*
             *
             */
            case WardenObjectiveType.HsuActivateSmall:
            {
                layout.BuildLayout_HsuActivateSmall(director, objective, start);
                break;
            }

            /**
             * Terminal Uplink
             */
            case WardenObjectiveType.TerminalUplink:
            {
                layout.BuildLayout_TerminalUplink(director, objective, start);
                break;
            }

            case WardenObjectiveType.GatherTerminal:
            {
                layout.BuildLayout_GatherTerminal(director, objective, start);
                break;
            }

            case WardenObjectiveType.CorruptedTerminalUplink:
            {
                layout.BuildLayout_CorruptedTerminalUplink(director, objective, start);
                break;
            }

            /**
             * Survival is a very custom objective
             */
            case WardenObjectiveType.TimedTerminalSequence:
            {
                layout.BuildLayout_TimedTerminalSequence(director, objective, (ZoneNode)start);
                break;
            }

            /**
             * This is finding the HSU
             */
            case WardenObjectiveType.HsuFindSample:
            {
                layout.BuildLayout_HsuFindSample(director, objective, (ZoneNode)start);
                break;
            }

            /**
             * Gather small items such as GLPs or plant samples
             */
            case WardenObjectiveType.GatherSmallItems:
            {
                layout.BuildLayout_GatherSmallItems(director, objective, (ZoneNode)start);
                break;
            }

            // Something has gone wrong if we are reaching this
            default:
            {
                layout.BuildBranch((ZoneNode)start, director.ZoneCount, "find_items");
                break;
            }
        }

        // Attempt to reduce the chance of generation locking where zones cannot be placed
        level.Planner.PlanBulkheadPlacements(director.Bulkhead, direction);

        var numberOfFogZones = level.Planner
            .GetZones(director.Bulkhead, null)
            .Select(node => level.Planner.GetZone(node))
            .Count(zone => zone is { InFog: true });

        Plugin.Logger.LogDebug($"{layout.Name} -- Number of in-fog zones: {numberOfFogZones}");

        var disinfectChance = level.Tier switch
        {
            "D" => 0.6,
            "E" => 0.4,
            _ => 0.9
        };

        // Fog level specific settings
        if (director.Bulkhead == Bulkhead.Main && numberOfFogZones > 0)
        {
            // Add the fog turbine to the last starting area
            var lastNode = level.Planner.GetZones(Bulkhead.StartingArea, null).Last();
            var lastZone = level.Planner.GetZone(lastNode)!;
            lastZone.BigPickupDistributionInZone = BigPickupDistribution.FogTurbine.PersistentId;

            // TODO: move this somewhere else? Currently it will only apply in main
            if (level.FogSettings.IsInfectious && Generator.Flip(disinfectChance))
            {
                var open = level.Planner.GetOpenZones(Bulkhead.All, null).Take(4);
                var from = Generator.Pick(open);

                layout.AddDisinfectionZone(from);
            }
        }

        // Write the zones
        foreach (var node in level.Planner.GetZones(director.Bulkhead, null))
        {
            var zone = level.Planner.GetZone(node);

            if (zone != null)
            {
                // Crude way to force the direction of zones for now
                // TODO: This should live in the zone node building process. We should be able to set the direction
                //       of these more dynamically
                // TODO: we probably want to remove this
                if (node.Branch == "primary")
                    zone.ZoneExpansion = direction.Forward;

                layout.Zones.Add(zone);

                Plugin.Logger.LogDebug(
                    $"{layout.Name} -- Zone_{zone.LocalIndex} " +
                    $"number={layout.ZoneAliasStart + zone.LocalIndex} " +
                    $"pid={zone.PersistentId} -- " +
                    $"branch={node.Branch} -- " +
                    $"Lights={zone.LightSettings}, InFog={zone.InFog}, Tags={node.Tags}");
            }
        }

        // TODO: most or all of these need to be moved
        layout.RollErrorAlarm();
        layout.RollAlarms(puzzlePack, wavePopulationPack, waveSettingsPack);
        layout.RollBloodDoors();
        layout.RollEnemies(director);

        Bins.LevelLayouts.AddBlock(layout);

        return layout;
    }
}
