using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Light;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

using LightSettings = Light.LightSettings;
using WardenObjective = Objectives.WardenObjective;

public partial record LevelLayout
{
    /// <summary>
    /// Selects unique themes from the available pool for Cryptomnesia layouts.
    /// Returns a list where index 0 = Reality and the last index = final dimension.
    ///
    /// Placement rules:
    ///   - InfectionFog can only be placed in the final dimension (last slot)
    ///   - Nightmares cannot be placed on Reality (index 0)
    ///   - ErrorAlarm has a higher chance of being picked for Reality
    /// </summary>
    public static List<CryptomnesiaTheme> SelectCryptomnesiaThemes(int count)
    {
        var pool = new List<(double weight, CryptomnesiaTheme theme)>
        {
            (1.0, CryptomnesiaTheme.ErrorAlarm),
            (1.0, CryptomnesiaTheme.Giants),
            (1.0, CryptomnesiaTheme.Chargers),
            (1.0, CryptomnesiaTheme.InfectionFog),
            (1.0, CryptomnesiaTheme.Shadows),
            (1.0, CryptomnesiaTheme.Nightmares),
        };

        var selected = new List<CryptomnesiaTheme>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            bool isReality = i == 0;
            bool isFinalDimension = i == count - 1 && count >= 2;

            // Build a filtered/weighted view for this slot
            var slotPool = new List<(double, CryptomnesiaTheme)>();

            foreach (var (weight, theme) in pool)
            {
                // InfectionFog can only go in the final dimension slot
                if (theme == CryptomnesiaTheme.InfectionFog && !isFinalDimension)
                    continue;

                // Nightmares can't go on Reality
                if (theme == CryptomnesiaTheme.Nightmares && isReality)
                    continue;

                // ErrorAlarm gets boosted weight on Reality
                var slotWeight = (theme == CryptomnesiaTheme.ErrorAlarm && isReality)
                    ? weight * 2.0
                    : weight;

                slotPool.Add((slotWeight, theme));
            }

            var pick = Generator.Select(slotPool);
            selected.Add(pick);
            pool.RemoveAll(t => t.theme == pick);
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

        // Clean all enemies from non-reality dimensions on exit (as players won't be going back)
        if (dimIndex != DimensionIndex.Reality)
            objective.Cryptomnesia_ExitEvents[dimIndex]
                .AddClearDimension(dimIndex, 2.0)
                .AddAllLightsOff(2.0, dimension: dimIndex);

        switch (theme)
        {
            case CryptomnesiaTheme.ErrorAlarm:
                ApplyTheme_ErrorAlarm(layout, level, director, objective, dimIndex);
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
    /// Error Alarm theme: base strikers/shooters sleeping in zones. The error alarm
    /// starts on entering this dimension and stops on exiting via enter/exit events.
    /// </summary>
    private static void ApplyTheme_ErrorAlarm(
        LevelLayout layout, Level level, BuildDirector director,
        WardenObjective objective, DimensionIndex dimIndex)
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

        // Start error alarm on enter
        var enterEvents = objective.Cryptomnesia_EnterEvents[dimIndex];
        enterEvents.AddSound(Sound.Alarms_Error_AmbientLoop, 1.0);
        enterEvents.AddSpawnWave(new GenericWave
        {
            Settings = WaveSettings.Error_Normal,
            Population = WavePopulation.Baseline,
            TriggerAlarm = true
        }, 1.0, "error_alarms");

        // Stop error alarm on exit
        var exitEvents = objective.Cryptomnesia_ExitEvents[dimIndex];
        exitEvents.AddTurnOffAlarms(0.0, "error_alarms");
        exitEvents.AddSound(Sound.Alarms_Error_AmbientStop, 0.5);
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

            var redLight = Generator.Pick(LightSettings.RedThemeLights);
            zone.LightSettings = (Lights.Light)redLight.PersistentId;

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

            // Spitters per zone: 30% none, 40% some, 30% lots
            var spitterCount = Generator.Select(new List<(double, int)>
            {
                (0.30, 0),
                (0.40, 100),
                (0.30, 250),
            });
            if (spitterCount > 0)
            {
                zone.StaticSpawnDataContainers.Add(new StaticSpawnDataContainer
                {
                    Count = spitterCount,
                    DistributionWeightType = 0,
                    DistributionWeight = 1.0,
                    DistributionRandomBlend = 0.0,
                    DistributionResultPow = 2.0,
                    Unit = StaticSpawnUnit.Spitter,
                    FixedSeed = Generator.Between(10, 150)
                });
            }

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

            var greenLight = Generator.Pick(LightSettings.GreenThemeLights);
            zone.LightSettings = (Lights.Light)greenLight.PersistentId;

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
