using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    /// <summary>
    /// Selects unique themes from the available pool for Cryptomnesia layouts.
    /// Uses Generator.Draw for seed-deterministic selection without replacement.
    /// </summary>
    public static List<CryptomnesiaTheme> SelectCryptomnesiaThemes(int count)
    {
        var pool = new List<CryptomnesiaTheme>
        {
            CryptomnesiaTheme.ErrorAlarm,
            CryptomnesiaTheme.Giants,
            CryptomnesiaTheme.Chargers,
            CryptomnesiaTheme.InfectionFog,
            CryptomnesiaTheme.Shadows,
            CryptomnesiaTheme.Nightmares,
        };

        var selected = new List<CryptomnesiaTheme>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            var theme = Generator.Draw(pool)!;
            selected.Add(theme);
        }

        return selected;
    }

    /// <summary>
    /// Applies a Cryptomnesia theme to a layout. Must be called AFTER zones are created
    /// but BEFORE FinalizeLayout(). Sets SkipRollEnemies, populates enemies directly,
    /// and initializes enter/exit event lists on the objective for this dimension.
    /// </summary>
    public static void ApplyCryptomnesiaTheme(
        CryptomnesiaTheme theme,
        LevelLayout layout,
        Dimension? dimension,
        Level level,
        BuildDirector director,
        WardenObjective objective)
    {
        var dimIndex = layout.Dimension;

        // Initialize the enter/exit event lists for this dimension
        objective.Cryptomnesia_EnterEvents[dimIndex] = new List<WardenObjectiveEvent>();
        objective.Cryptomnesia_ExitEvents[dimIndex] = new List<WardenObjectiveEvent>();

        switch (theme)
        {
            case CryptomnesiaTheme.ErrorAlarm:
                ApplyTheme_ErrorAlarm(layout, level, director);
                break;
            case CryptomnesiaTheme.Giants:
                ApplyTheme_Giants(layout, level, director);
                break;
            case CryptomnesiaTheme.Chargers:
                ApplyTheme_Chargers(layout, level, director);
                break;
            case CryptomnesiaTheme.InfectionFog:
                ApplyTheme_InfectionFog(layout, dimension, level, director);
                break;
            case CryptomnesiaTheme.Shadows:
                ApplyTheme_Shadows(layout, level, director);
                break;
            case CryptomnesiaTheme.Nightmares:
                ApplyTheme_Nightmares(layout, level, director);
                break;
        }

        Plugin.Logger.LogDebug($"Cryptomnesia: Applied theme {theme} to {layout.Name}");
    }

    #region Theme Implementations

    /// <summary>
    /// Error Alarm theme: base strikers/shooters sleeping in zones, plus an error alarm
    /// on a middle zone.
    /// </summary>
    private static void ApplyTheme_ErrorAlarm(LevelLayout layout, Level level, BuildDirector director)
    {
        layout.SkipRollEnemies = true;

        var zoneNodes = level.Planner.GetZones(director.Bulkhead, null, layout.Dimension);

        foreach (var node in zoneNodes)
        {
            var zone = level.Planner.GetZone(node);
            if (zone == null) continue;

            var points = director.GetPoints(zone);
            if (points < 3) continue;

            zone.EnemySpawningInZone.Add(EnemySpawningData.Striker with { Points = points / 2 });
            zone.EnemySpawningInZone.Add(EnemySpawningData.Shooter with { Points = points / 2 });
        }

        // Place error alarm on a middle zone (not first, not last)
        if (zoneNodes.Count < 2)
            return;

        var candidates = zoneNodes
            .Where(n => n.ZoneNumber != zoneNodes.First().ZoneNumber
                     && n.ZoneNumber != zoneNodes.Last().ZoneNumber)
            .ToList();

        if (!candidates.Any())
            candidates = zoneNodes.Skip(1).ToList();

        var alarmNode = Generator.Pick(candidates);
        layout.AddErrorAlarm(alarmNode, null, WaveSettings.Error_Normal, WavePopulation.Baseline);
    }

    /// <summary>
    /// Giants theme: StrikerGiant and ShooterGiant in every zone. One zone becomes a
    /// "heavy scout room" with ~6 regular scouts plus giants.
    /// </summary>
    private static void ApplyTheme_Giants(LevelLayout layout, Level level, BuildDirector director)
    {
        layout.SkipRollEnemies = true;

        var zoneNodes = level.Planner.GetZones(director.Bulkhead, null, layout.Dimension);
        var scoutRoomPicked = false;

        foreach (var node in zoneNodes)
        {
            var zone = level.Planner.GetZone(node);
            if (zone == null) continue;

            var points = director.GetPoints(zone);
            if (points < 3) continue;

            zone.EnemySpawningInZone.Add(new EnemySpawningData
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = (uint)Enemy.StrikerGiant,
                Points = points / 2
            });
            zone.EnemySpawningInZone.Add(new EnemySpawningData
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = (uint)Enemy.ShooterGiant,
                Points = points / 2
            });

            // One non-first zone becomes the scout room
            if (!scoutRoomPicked && node.ZoneNumber != zoneNodes.First().ZoneNumber)
            {
                scoutRoomPicked = true;

                for (int i = 0; i < 6; i++)
                    zone.EnemySpawningInZone.Add(
                        EnemySpawningData.Scout with
                        {
                            Distribution = EnemyZoneDistribution.ForceOne,
                            Points = 25
                        });
            }
        }
    }

    /// <summary>
    /// Chargers theme: Chargers and ChargerGiants throughout. One zone gets a Tank,
    /// another gets a ChargerScout.
    /// </summary>
    private static void ApplyTheme_Chargers(LevelLayout layout, Level level, BuildDirector director)
    {
        layout.SkipRollEnemies = true;

        var zoneNodes = level.Planner.GetZones(director.Bulkhead, null, layout.Dimension);
        var tankPlaced = false;
        var scoutPlaced = false;

        foreach (var node in zoneNodes)
        {
            var zone = level.Planner.GetZone(node);
            if (zone == null) continue;

            var points = director.GetPoints(zone);
            if (points < 3) continue;

            zone.EnemySpawningInZone.Add(EnemySpawningData.Charger with { Points = (int)(points * 0.6) });
            zone.EnemySpawningInZone.Add(EnemySpawningData.ChargerGiant with { Points = (int)(points * 0.4) });

            // Place a tank in one non-first zone
            if (!tankPlaced && node.ZoneNumber != zoneNodes.First().ZoneNumber)
            {
                tankPlaced = true;
                zone.EnemySpawningInZone.Add(EnemySpawningData.Tank with
                {
                    Distribution = EnemyZoneDistribution.ForceOne,
                    Points = 25
                });
            }

            // Place a charger scout in a different non-first zone
            if (tankPlaced && !scoutPlaced && node.ZoneNumber != zoneNodes.First().ZoneNumber)
            {
                scoutPlaced = true;
                zone.EnemySpawningInZone.Add(EnemySpawningData.ScoutCharger with
                {
                    Distribution = EnemyZoneDistribution.ForceOne,
                    Points = 25
                });
            }
        }
    }

    /// <summary>
    /// Infection Fog theme: all zones submerged in infectious fog with shooters (spitters)
    /// and strikers. Disinfection packs in every zone plus a disinfection station side room.
    /// </summary>
    private static void ApplyTheme_InfectionFog(
        LevelLayout layout,
        Dimension? dimension,
        Level level,
        BuildDirector director)
    {
        layout.SkipRollEnemies = true;

        // Set fog on the dimension or on the level for Reality
        if (dimension != null)
        {
            dimension.Data.DimensionFogData = Fog.FullFog_Infectious.PersistentId;
        }
        else
        {
            level.FogSettings = Fog.FindOrPersist(Fog.FullFog_Infectious);
        }

        var zoneNodes = level.Planner.GetZones(director.Bulkhead, null, layout.Dimension);

        foreach (var node in zoneNodes)
        {
            var zone = level.Planner.GetZone(node);
            if (zone == null) continue;

            zone.InFog = true;
            zone.DisinfectPacks += Generator.Between(2, 4);

            var points = director.GetPoints(zone);
            if (points < 3) continue;

            zone.EnemySpawningInZone.Add(EnemySpawningData.Shooter with { Points = (int)(points * 0.6) });
            zone.EnemySpawningInZone.Add(EnemySpawningData.Striker with { Points = (int)(points * 0.4) });
        }

        // Add a disinfection station side room
        var openZones = level.Planner.GetOpenZones(director.Bulkhead, null, layout.Dimension);
        if (openZones.Any())
        {
            var from = Generator.Pick(openZones);
            layout.AddDisinfectionZone(from);
        }
    }

    /// <summary>
    /// Shadows theme: Shadow and ShadowGiant enemies throughout with a ShadowScout.
    /// </summary>
    private static void ApplyTheme_Shadows(LevelLayout layout, Level level, BuildDirector director)
    {
        layout.SkipRollEnemies = true;

        var zoneNodes = level.Planner.GetZones(director.Bulkhead, null, layout.Dimension);
        var scoutPlaced = false;

        foreach (var node in zoneNodes)
        {
            var zone = level.Planner.GetZone(node);
            if (zone == null) continue;

            var points = director.GetPoints(zone);
            if (points < 3) continue;

            zone.EnemySpawningInZone.Add(new EnemySpawningData
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = (uint)Enemy.Shadow,
                Points = (int)(points * 0.6)
            });
            zone.EnemySpawningInZone.Add(new EnemySpawningData
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = (uint)Enemy.ShadowGiant,
                Points = (int)(points * 0.4)
            });

            // Place a shadow scout in one non-first zone
            if (!scoutPlaced && node.ZoneNumber != zoneNodes.First().ZoneNumber)
            {
                scoutPlaced = true;
                zone.EnemySpawningInZone.Add(EnemySpawningData.ScoutShadow with
                {
                    Distribution = EnemyZoneDistribution.ForceOne,
                    Points = 25
                });
            }
        }
    }

    /// <summary>
    /// Nightmares theme: NightmareStriker, NightmareShooter, and NightmareGiant.
    /// One zone gets a NightmareScout.
    /// </summary>
    private static void ApplyTheme_Nightmares(LevelLayout layout, Level level, BuildDirector director)
    {
        layout.SkipRollEnemies = true;

        var zoneNodes = level.Planner.GetZones(director.Bulkhead, null, layout.Dimension);
        var scoutPlaced = false;

        foreach (var node in zoneNodes)
        {
            var zone = level.Planner.GetZone(node);
            if (zone == null) continue;

            var points = director.GetPoints(zone);
            if (points < 3) continue;

            zone.EnemySpawningInZone.Add(new EnemySpawningData
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = (uint)Enemy.NightmareStriker,
                Points = (int)(points * 0.4)
            });
            zone.EnemySpawningInZone.Add(new EnemySpawningData
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = (uint)Enemy.NightmareShooter,
                Points = (int)(points * 0.3)
            });
            zone.EnemySpawningInZone.Add(EnemySpawningData.NightmareGiant with
            {
                Points = (int)(points * 0.3)
            });

            // Place a nightmare scout in one non-first zone
            if (!scoutPlaced && node.ZoneNumber != zoneNodes.First().ZoneNumber)
            {
                scoutPlaced = true;
                zone.EnemySpawningInZone.Add(EnemySpawningData.ScoutNightmare with
                {
                    Distribution = EnemyZoneDistribution.ForceOne,
                    Points = 25
                });
            }
        }
    }

    #endregion
}
