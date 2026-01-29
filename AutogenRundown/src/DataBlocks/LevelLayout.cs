using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public enum SizeFactor
{
    Small,
    Medium,
    Large
}

public partial record LevelLayout : DataBlock<LevelLayout>
{
    #region hidden data
    [JsonIgnore]
    private readonly Level level;

    [JsonIgnore]
    internal readonly BuildDirector director;

    [JsonIgnore]
    private readonly WardenObjective objective;

    [JsonIgnore]
    private RelativeDirection direction;

    [JsonIgnore]
    private readonly LayoutPlanner planner;

    [JsonIgnore]
    private readonly LevelSettings settings;

    [JsonIgnore]
    public List<(double chance, int count, ChainedPuzzle puzzle)> PuzzlePack { get; set; } = new();

    [JsonIgnore]
    public List<(double chance, int count, WavePopulation population)> WavePopulationPack { get; set; } = new();

    [JsonIgnore]
    public List<(double chance, int count, WaveSettings)> WaveSettingsPack { get; set; } = new();

    #endregion

    /// <summary>
    /// This is dynamically pulled from the level. This lets us centralize and coordinate
    /// generating zone ranges
    /// </summary>
    public int ZoneAliasStart => level.GetZoneAliasStart(director.Bulkhead);

    public List<Zone> Zones { get; set; } = new();

    public LevelLayout(
        Level level,
        BuildDirector director,
        WardenObjective objective,
        LevelSettings settings,
        LayoutPlanner planner)
    {
        // Always assign a new persistent id upfront to level layout
        PersistentId = Generator.GetPersistentId();

        this.director = director;
        this.level = level;
        this.objective = objective;
        this.planner = planner;
        this.settings = settings;
    }

    /// <summary>
    /// Roll for door alarms
    /// </summary>
    public void RollAlarms()
    {
        // We want to roll the alarms on the zones in a random order. The alarm packs have
        // weighted chances on them. This means zones rolled first have a higher chance of
        // getting high weight alarms and later zones have a higher chance of rarer alarms.
        // By shuffling the zone list we ensure the alarms are randomly distributed, while
        // preserving the weightings of alarms.
        var zones = Zones.Shuffle();

        foreach (var zone in zones)
            zone.RollAlarms();
    }

    /// <summary>
    /// Helper to add forward extract candidate build zones.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="chance"></param>
    public void AddForwardExtractStart(ZoneNode node, double chance = 1.0)
    {
        if (director.Bulkhead != Bulkhead.Main)
            return;

        level.ForwardExtractStartCandidates.Add((chance, node));
    }

    /// <summary>
    /// Roll for blood doors
    /// </summary>
    public void RollBloodDoors()
    {
        // We don't add blood doors for A-tier
        if (level.Tier == "A")
            return;

        var count = 0;
        var (max, chance, inAreaChance) = director.Tier switch
        {
            "A" => ( 0, 0.00, 0.00),
            "B" => ( 1, 0.20, 0.45),
            "C" => ( 2, 0.15, 0.55),
            "D" => ( 3, 0.15, 0.65),
            _ =>   (-1, 0.20, 0.80)
        };

        // Ensure that there are at least as many groups as 2x the max number of blood doors
        // that can spawn. For unlimited cap tiers (D and E) this is 2x the number of zones.
        // Door pack is used to select enemies that spawn behind the door.
        var doorPack = director.Tier switch
        {
            "B" => new List<(double, int, EnemyGroup)>
            {
                (1.0, 2, EnemyGroup.BloodDoor_Baseline_Easy),
                (1.0, 1, EnemyGroup.BloodDoor_Baseline_Normal),
                (0.8, 1, EnemyGroup.BloodDoor_Baseline_Hybrids_Easy)
            },

            "C" => new List<(double, int, EnemyGroup)>
            {
                (1.0, 3, EnemyGroup.BloodDoor_Baseline_Normal),
                (0.8, 1, EnemyGroup.BloodDoor_GiantStrikers_Normal),
                (1.0, 2, EnemyGroup.BloodDoor_Baseline_Hybrids_Normal),

                (0.2, 1, EnemyGroup.BloodDoor_Infested_Normal),

                (0.5, 1, EnemyGroup.BloodDoor_Tank),
                (0.8, 1, EnemyGroup.BloodDoor_TankPotato),
            },

            "D" => new List<(double, int, EnemyGroup)>
            {
                (1.0, 3, EnemyGroup.BloodDoor_Baseline_Hard),
                (1.0, 1, EnemyGroup.BloodDoor_GiantStrikers_Normal),
                (1.0, 1, EnemyGroup.BloodDoor_GiantStrikers_Hard),
                (1.0, 1, EnemyGroup.BloodDoor_GiantShooter_Hard),
                (1.0, 2, EnemyGroup.BloodDoor_Baseline_Hybrids_Hard),
                (1.0, 1, EnemyGroup.BloodDoor_Hybrid_Normal),

                (1.0, 1, EnemyGroup.BloodDoor_Infested_Normal),
                (0.3, 1, EnemyGroup.BloodDoor_Infested_Hard),

                (0.5, 1, EnemyGroup.BloodDoor_Mother),
                (0.7, 1, EnemyGroup.BloodDoor_Tank),
                (0.2, 1, EnemyGroup.BloodDoor_TankPotato_x3),
            },

            "E" => new List<(double, int, EnemyGroup)>
            {
                (1.0, 3, EnemyGroup.BloodDoor_Baseline_VeryHard),
                (1.0, 2, EnemyGroup.BloodDoor_GiantStrikers_Hard),
                (1.0, 1, EnemyGroup.BloodDoor_GiantShooter_Hard),
                (1.0, 2, EnemyGroup.BloodDoor_Baseline_Hybrids_VeryHard),
                (1.0, 1, EnemyGroup.BloodDoor_Hybrid_Hard),

                (0.7, 2, EnemyGroup.BloodDoor_Infested_Hard),

                (0.7, 1, EnemyGroup.BloodDoor_Mother),
                (0.7, 1, EnemyGroup.BloodDoor_Tank),
                (0.7, 1, EnemyGroup.BloodDoor_TankPotato_x3),

                (0.20, 1, EnemyGroup.BloodDoor_PMother),
                (0.15, 1, EnemyGroup.BloodDoor_Tank_x2),
            },

            _ => new List<(double, int, EnemyGroup)>()
        };

        // Area pack picks enemies to spawn further back, if we successfully roll to add them.
        var areaPack = director.Tier switch
        {
            "B" => new List<(double, int, EnemyGroup)>
            {
                (1.0, 2, EnemyGroup.BloodDoor_Baseline_Easy),
                (1.0, 1, EnemyGroup.BloodDoor_Baseline_Normal),
            },

            "C" => new List<(double, int, EnemyGroup)>
            {
                (1.0, 3, EnemyGroup.BloodDoor_Baseline_Easy),
                (1.0, 2, EnemyGroup.BloodDoor_Baseline_Hybrids_Easy),

                (0.2, 1, EnemyGroup.BloodDoor_Pouncer),

                (0.1, 1, EnemyGroup.BloodDoor_Tank),
                (0.8, 1, EnemyGroup.BloodDoor_TankPotato),
            },

            "D" => new List<(double, int, EnemyGroup)>
            {
                (1.0, 5, EnemyGroup.BloodDoor_Baseline_Normal),
                (1.0, 2, EnemyGroup.BloodDoor_Baseline_Hybrids_Normal),

                (1.0, 1, EnemyGroup.BloodDoor_TankPotato),
                (0.7, 1, EnemyGroup.BloodDoor_Infested_Normal),

                (1.0, 2, EnemyGroup.BloodDoor_Pouncer),
                (0.7, 1, EnemyGroup.BloodDoor_Pouncer_x2),

                (0.3, 1, EnemyGroup.BloodDoor_Mother),
                (0.2, 1, EnemyGroup.BloodDoor_Tank),
            },

            "E" => new List<(double, int, EnemyGroup)>
            {
                (1.0, 4, EnemyGroup.BloodDoor_Baseline_Hard),
                (1.0, 1, EnemyGroup.BloodDoor_GiantShooter_Hard),
                (1.0, 1, EnemyGroup.BloodDoor_Baseline_Hybrids_Hard),

                (1.0, 2, EnemyGroup.BloodDoor_TankPotato),
                (0.7, 2, EnemyGroup.BloodDoor_Infested_Hard),

                (1.0, 2, EnemyGroup.BloodDoor_Pouncer_x2),
                (0.5, 1, EnemyGroup.BloodDoor_Pouncer_x3),

                (0.5, 1, EnemyGroup.BloodDoor_Mother),
                (0.4, 1, EnemyGroup.BloodDoor_Tank),
            },

            _ => new List<(double, int, EnemyGroup)>()
        };

        switch (level.Tier)
        {
            case "C":
            {
                if (level.Settings.HasChargers())
                    doorPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (2.0, 1, EnemyGroup.BloodDoor_Chargers_Easy),
                        (2.0, 2, EnemyGroup.BloodDoor_Chargers_Normal)
                    });

                if (level.Settings.HasNightmares())
                    doorPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (2.0, 2, EnemyGroup.BloodDoor_Nightmares_Normal)
                    });
                break;
            }

            case "D":
            {
                if (level.Settings.HasChargers())
                    doorPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (2.0, 1, EnemyGroup.BloodDoor_Chargers_Normal),
                        (2.0, 2, EnemyGroup.BloodDoor_Chargers_Hard),
                        (2.0, 2, EnemyGroup.BloodDoor_GiantChargers_Normal)
                    });

                if (level.Settings.HasShadows())
                {
                    doorPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (1.0, 1, EnemyGroup.BloodDoor_Shadows_Normal),
                        (2.0, 2, EnemyGroup.BloodDoor_Shadows_Hard),
                        (2.0, 2, EnemyGroup.BloodDoor_ShadowGiant_Normal),
                    });
                    areaPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (2.0, 3, EnemyGroup.BloodDoor_PouncerShadow),
                        (1.5, 2, EnemyGroup.BloodDoor_PouncerShadow_x2)
                    });
                }

                if (level.Settings.HasNightmares())
                    doorPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (2.0, 2, EnemyGroup.BloodDoor_Nightmares_Normal),
                        (1.0, 1, EnemyGroup.BloodDoor_Nightmares_Hard),
                        (1.0, 2, EnemyGroup.BloodDoor_NightmareGiants_Normal)
                    });

                // TODO: they get stuck?
                // if (level.Settings.HasFlyers())
                //     areaPack.AddRange(new List<(double, int, EnemyGroup)>
                //     {
                //         (1.5, 1, EnemyGroup.BloodDoor_FlyerBig)
                //     });

                if (level.Settings.HasFog() && level.FogSettings.IsInfectious)
                    doorPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (2.0, 1, EnemyGroup.BloodDoor_HybridInfected_Normal),
                        (2.0, 1, EnemyGroup.BloodDoor_HybridInfected_Hard)
                    });
                break;
            }

            case "E":
            {
                if (level.Settings.HasChargers())
                    doorPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (2.0, 1, EnemyGroup.BloodDoor_Chargers_Hard),
                        (2.0, 2, EnemyGroup.BloodDoor_Chargers_VeryHard),
                        (2.0, 2, EnemyGroup.BloodDoor_GiantChargers_Hard)
                    });

                if (level.Settings.HasShadows())
                {
                    doorPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (1.0, 1, EnemyGroup.BloodDoor_Shadows_Hard),
                        (2.0, 2, EnemyGroup.BloodDoor_Shadows_VeryHard),
                        (2.0, 2, EnemyGroup.BloodDoor_ShadowGiant_Hard),
                    });
                    areaPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (2.0, 2, EnemyGroup.BloodDoor_PouncerShadow),
                        (1.5, 2, EnemyGroup.BloodDoor_PouncerShadow_x2),
                        (1.0, 1, EnemyGroup.BloodDoor_PouncerShadow_x3)
                    });
                }

                if (level.Settings.HasNightmares())
                    doorPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (2.0, 2, EnemyGroup.BloodDoor_Nightmares_Hard),
                        (3.0, 2, EnemyGroup.BloodDoor_NightmareGiants_Hard)
                    });

                // TODO: they get stuck?
                // if (level.Settings.HasFlyers())
                //     areaPack.AddRange(new List<(double, int, EnemyGroup)>
                //     {
                //         (2.0, 2, EnemyGroup.BloodDoor_FlyerBig)
                //     });

                if (level.Settings.HasFog() && level.FogSettings.IsInfectious)
                    doorPack.AddRange(new List<(double, int, EnemyGroup)>
                    {
                        (3.0, 2, EnemyGroup.BloodDoor_HybridInfected_Hard)
                    });
                break;
            }
        }

        // We shuffle for the same reason we shuffle the zones before rolling alarms. Subsequent
        // zones have different chances of blood doors based on how many are here.
        var zones = Zones.Shuffle();

        // Do not add blood doors to Zone 0, these are always either the elevator or bulkhead doors.
        // Do not add blood doors to Apex security doors
        foreach (var zone in zones)
            if (!planner.GetZoneNode(zone.LocalIndex).Tags.Contains("no_blood_door") &&
                zone is { LocalIndex: > 0, SecurityGateToEnter: SecurityGate.Security } &&
                Generator.Flip(chance) &&
                (count++ < max || max == -1))
            {
                var withArea = Generator.Flip(inAreaChance);

                zone.BloodDoor = new BloodDoor
                {
                    GroupBehindDoor = Generator.DrawSelect(doorPack),
                    GroupInArea = withArea ? Generator.DrawSelect(areaPack) : EnemyGroup.None,
                    AreaGroups = withArea ? 1 : 0
                };

                Plugin.Logger.LogInfo($"Blood Door in to Zone={zone.LocalIndex}, " +
                                      $"EnemyGroup={zone.BloodDoor.GroupBehindDoor}, " +
                                      $"AreaGroup={zone.BloodDoor.GroupInArea}");
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

        // We want to shuffle this list for the same reason as when we roll blood doors and door
        // alarms. It stops pack draws being weighted differently at the start and end of the
        // layout zones.
        var zoneNodes = planner.GetZones(director.Bulkhead, null).Shuffle();

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
                hybridChance = 0.3;

            var infectionHybridChance = 0.0;

            if (settings.Modifiers.Contains(LevelModifiers.InfectionHybrids))
                infectionHybridChance = zone.InFog ? 0.4 : 0.15;
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

            var groupChoices = new List<(double chance, List<AutogenDifficulty> groups)>
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

            if (chargerChance > 0 && hybridChance > 0)
                groupChoices.Add(
                    (0.1, new List<AutogenDifficulty>
                    {
                        AutogenDifficulty.Base,
                        AutogenDifficulty.Chargers,
                        AutogenDifficulty.Hybrids
                    }));

            // if (chargerChance > 0 && shadowChance > 0)
            //     groupChoices.Add(
            //         (0.2, new List<AutogenDifficulty>
            //             {
            //                 AutogenDifficulty.Base,
            //                 AutogenDifficulty.Chargers,
            //                 AutogenDifficulty.Shadows
            //             }));

            // if (chargerChance > 0 && nightmaresChance > 0)
            //     groupChoices.Add(
            //         (0.2, new List<AutogenDifficulty>
            //         {
            //             AutogenDifficulty.Base,
            //             AutogenDifficulty.Chargers,
            //             AutogenDifficulty.Nightmares
            //         }));

            // if (shadowChance > 0 && nightmaresChance > 0)
            //     groupChoices.Add(
            //         (0.2, new List<AutogenDifficulty>
            //         {
            //             AutogenDifficulty.Base,
            //             AutogenDifficulty.Shadows,
            //             AutogenDifficulty.Nightmares
            //         }));

            // if (chargerChance > 0 && shadowChance > 0 && hybridChance > 0)
            //     groupChoices.Add(
            //         (0.1, new List<AutogenDifficulty>
            //             {
            //                 AutogenDifficulty.Chargers,
            //                 AutogenDifficulty.Shadows,
            //                 AutogenDifficulty.Hybrids
            //             }));

            // if (chargerChance > 0 && nightmaresChance > 0 && hybridChance > 0)
            //     groupChoices.Add(
            //         (0.1, new List<AutogenDifficulty>
            //         {
            //             AutogenDifficulty.Chargers,
            //             AutogenDifficulty.Nightmares,
            //             AutogenDifficulty.Hybrids
            //         }));

            // if (shadowChance > 0 && nightmaresChance > 0 && hybridChance > 0)
            //     groupChoices.Add(
            //         (0.1, new List<AutogenDifficulty>
            //         {
            //             AutogenDifficulty.Shadows,
            //             AutogenDifficulty.Nightmares,
            //             AutogenDifficulty.Hybrids
            //         }));

            // // TODO: TBD if we like having a room with literally everything in it
            // // We don't
            // if (chargerChance > 0 && shadowChance > 0 && nightmaresChance > 0 && hybridChance > 0)
            //     groupChoices.Add(
            //         (0.1, new List<AutogenDifficulty>
            //         {
            //             AutogenDifficulty.Chargers,
            //             AutogenDifficulty.Shadows,
            //             AutogenDifficulty.Nightmares,
            //             AutogenDifficulty.Hybrids
            //         }));


            var groups = Generator.Select(groupChoices);
            var displayGroups = groups.Select(difficulty => difficulty.ToString()).ToList();

            if (Generator.Flip(infectionHybridChance))
            {
                zone.EnemySpawningInZone.Add(
                    new EnemySpawningData
                    {
                        GroupType = EnemyGroupType.Hibernate,
                        Difficulty = (uint)EnemyInfo.HybridInfected.Enemy,
                        Points = level.Tier switch
                        {
                            "D" => Generator.Select(new List<(double chance, int points)>
                            {
                                (0.5, 8),
                                (0.3, 12),
                                (0.1, 16)
                            }),
                            "E" => Generator.Select(new List<(double chance, int points)>
                            {
                                (0.5, 8),
                                (0.2, 12),
                                (0.3, 16)
                            }),

                            _ => 8
                        },
                    });
                displayGroups.Add("HybridInfected");
            }

            Plugin.Logger.LogDebug($"{Name} -- Zone {zone.LocalIndex} has {points}pts for enemies. Groups: {string.Join(", ", displayGroups)}");

            // TODO: reduce number of groups

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
                            (AutogenDifficulty.Nightmares, "C") => points / 1.2,
                            (AutogenDifficulty.Shadows, "E") => points * 1.2,

                            _ => points
                        } / groups.Count)
                    });
        }
    }

    /// <summary>
    /// Rolls for whether we should add an error alarm to this level layout.
    /// </summary>
    [Obsolete("Prefer more curated level design")]
    public void RollErrorAlarm()
    {
        // Skip adding any rolled error alarms if the main objective is a survival map
        // The error turnoff code message doesn't play well with the survival counter
        if (level.Director[Bulkhead.Main].Objective == WardenObjectiveType.Survival)
        {
            Plugin.Logger.LogDebug("Skipping rolled error alarms");
            return;
        }

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
            {
                Plugin.Logger.LogDebug("We had a problem, ");

                return;
            }

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
                    new Zone(level, this)
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
    /// <returns></returns>
    public static LevelLayout Build(
        Level level,
        BuildDirector director,
        WardenObjective objective)
    {
        var direction = level.Settings.GetDirections(director.Bulkhead);
        var layout = new LevelLayout(level, director, objective, level.Settings, level.Planner)
        {
            Name = $"{level.Tier}{level.Index} {level.Name} {director.Bulkhead}",
            direction = direction,

            PuzzlePack = ChainedPuzzle.BuildPack(level.Tier, director.Bulkhead, level.Settings),
            WavePopulationPack = WavePopulation.BuildPack(level.Tier, level.Settings),
            WaveSettingsPack = WaveSettings.BuildPack(level.Tier)
        };

        director.GenZones();

        // var puzzlePack = ChainedPuzzle.BuildPack(level.Tier, director.Bulkhead, level.Settings);
        // var wavePopulationPack = WavePopulation.BuildPack(level.Tier, level.Settings);
        // var waveSettingsPack = WaveSettings.BuildPack(level.Tier);

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
            ZoneExpansion.Right => ZoneBuildExpansion.Right,
            _ => startZone.StartExpansion
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
             * Central cluster of generators with cell fetching
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

            #region Autogen Custom Objectives

            /*
             * Modeled after R8E1 / R8E2 / R5E1
             */
            case WardenObjectiveType.ReachKdsDeep:
            {
                layout.BuildLayout_ReachKdsDeep(director, objective, start);
                break;
            }

            #endregion

            // Something has gone wrong if we are reaching this
            default:
            {
                layout.BuildBranch((ZoneNode)start, director.ZoneCount, "find_items");
                break;
            }
        }

        if (director.Bulkhead == Bulkhead.Main)
            layout.BuildLayout_ForwardExtract(objective);

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

        // Add disinfect packs
        if (level.Settings.HasFog() && level.FogSettings.IsInfectious)
        {
            foreach (var zone in level.Planner
                         .GetZones(director.Bulkhead, null)
                         .Select(zone => level.Planner.GetZone(zone))
                         .Where(zone => zone is { InFog: true })
                         .Cast<Zone>())
            {
                var (min, max) = level.Tier switch
                {
                    "D" => Generator.Select(new List<(double weight, (int min, int max))>
                    {
                        (0.66, (3, 4)),
                        (0.33, (0, 3))
                    }),
                    "E" => Generator.Select(new List<(double weight, (int min, int max))>
                    {
                        (0.5, (2, 3)),
                        (0.3, (1, 2)),
                        (0.2, (0, 0))
                    }),

                    _ => (3, 4)
                };

                zone.DisinfectPacks += Generator.Between(min, max);
            }
        }

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
        // layout.RollErrorAlarm(); // Deprecated
        layout.RollAlarms();
        layout.RollBloodDoors();
        layout.RollEnemies(director);

        Bins.LevelLayouts.AddBlock(layout);

        return layout;
    }
}
