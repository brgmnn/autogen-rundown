using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AutogenRundown.TerminalPlacements;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

/// <summary>
///
/// </summary>
public partial record Zone : DataBlock
{
    #region Direction manipulation
    /// <summary>
    /// Sets this zones expansion and build directions to be a branching direction of the parent zone
    ///
    /// It's quite an annoying function as we must enumerate how each rotation works for a
    /// given direction
    /// </summary>
    /// <param name="zone"></param>
    public void SetExpansionAsBranchOfZone(Zone zone, Direction leftOrRight)
    {
        switch (zone.ZoneExpansion)
        {
            case ZoneExpansion.Forward:
            {
                if (leftOrRight == Direction.Left)
                {
                    ZoneExpansion = ZoneExpansion.Left;
                    StartExpansion = ZoneBuildExpansion.Left;

                    // TODO: Better heuristic for what to do with this
                    StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                }
                else
                {
                    ZoneExpansion = ZoneExpansion.Right;
                    StartExpansion = ZoneBuildExpansion.Right;
                    StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                }

                break;
            }

            case ZoneExpansion.Left:
            {
                if (leftOrRight == Direction.Left)
                {
                    ZoneExpansion = ZoneExpansion.Backward;
                    StartExpansion = ZoneBuildExpansion.Backward;
                    StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                }
                else
                {
                    ZoneExpansion = ZoneExpansion.Forward;
                    StartExpansion = ZoneBuildExpansion.Forward;
                    StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                }

                break;
            }

            case ZoneExpansion.Right:
            {
                if (leftOrRight == Direction.Left)
                {
                    ZoneExpansion = ZoneExpansion.Forward;
                    StartExpansion = ZoneBuildExpansion.Forward;
                    StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                }
                else
                {
                    ZoneExpansion = ZoneExpansion.Backward;
                    StartExpansion = ZoneBuildExpansion.Backward;
                    StartPosition = ZoneEntranceBuildFrom.BetweenStartAndFurthest;
                }

                break;
            }

            case ZoneExpansion.Backward:
            {
                if (leftOrRight == Direction.Left)
                {
                    ZoneExpansion = ZoneExpansion.Right;
                    StartExpansion = ZoneBuildExpansion.Right;
                }
                else
                {
                    ZoneExpansion = ZoneExpansion.Left;
                    StartExpansion = ZoneBuildExpansion.Left;
                }

                break;
            }
        }
    }

    public void SetStartExpansionFromExpansion()
    {
        StartExpansion = ZoneExpansion switch
        {
            ZoneExpansion.Forward => ZoneBuildExpansion.Forward,
            ZoneExpansion.Backward => ZoneBuildExpansion.Backward,
            ZoneExpansion.Left => ZoneBuildExpansion.Left,
            ZoneExpansion.Right => ZoneBuildExpansion.Right,
            _ => StartExpansion
        };
    }
    #endregion

    #region Alarms
    private void AlarmModifier_LightsOff()
    {
        EventsOnDoorScanStart.AddLightsOff(Generator.Between(3, 8));
        EventsOnDoorScanDone.AddLightsOn(Generator.Between(1, 9));
    }

    private void AlarmModifier_FogFlood(double duration)
    {
        var fog = level.FogSettings;
        var isInfectious = level.FogSettings.Infection > 0.01;
        var inverted = fog.IsInverted;
        var tempFog = (isInfectious, inverted) switch
        {
            (false, false) => Fog.Normal_Altitude_8,
            (false, true ) => Fog.Inverted_Altitude_minus8,
            (true,  false) => Fog.NormalInfectious_Altitude_8,
            (true,  true ) => Fog.InvertedInfectious_Altitude_minus8
        };

        EventsOnDoorScanStart.AddSetFog(tempFog, Generator.Between(3, 15), duration);
        EventsOnDoorScanDone.AddSetFog(
            fog,
            5.0,
            21.0,
            "VENTILATION SYSTEM REBOOTED - SYSTEMS ONLINE");
    }

    /// <summary>
    /// Ideas for alarm modifications
    ///
    ///     * Lights off during alarm
    ///         * Lights flashing during alarm
    ///         * Lights change during alarm?
    ///     * Fog rising during alarm
    ///         * Fog lowering from ceiling
    ///         * Fog clears in fog level during alarm
    ///     * Spawn custom enemies, tank / mother / pouncer etc.
    ///     * Turn on the security error scans from R8?
    ///
    /// </summary>
    /// <param name="puzzlePack"></param>
    /// <param name="wavePopulationPack"></param>
    /// <param name="waveSettingsPack"></param>
    public void RollAlarms(
        Level level,
        LevelLayout layout,
        ICollection<(double, int, ChainedPuzzle)> puzzlePack,
        ICollection<(double, int, WavePopulation)> wavePopulationPack,
        ICollection<(double, int, WaveSettings)> waveSettingsPack)
    {
        // TODO: should we just be trying to set the alarm if there isn't already an alarm here?
        if (LocalIndex == 0 ||
            Alarm == ChainedPuzzle.SkipZone ||
            Alarm == ChainedPuzzle.AlarmError_Template ||
            Alarm.PersistentId != 0 ||
            Alarm != ChainedPuzzle.None)
        {
            return;
        }

        // Grab a random puzzle from the puzzle pack
        var puzzle = Generator.DrawSelect(puzzlePack);

        if (puzzle == null)
            return;

        // TODO: Randomize things like travel distance here
        // We only copy the population settings in if we have an actual alarm here
        if (puzzle.TriggerAlarmOnActivate && !puzzle.FixedAlarm)
        {
            var population = Generator.DrawSelect(wavePopulationPack)!;
            var settings = Generator.DrawSelect(waveSettingsPack)!;

            // Rescale settings by difficulty factor
            if (!population.DifficultyFactor.ApproxEqual(1.0))
            {
                settings = settings.ScaleDownFor(population.DifficultyFactor).FindOrPersist();
                Plugin.Logger.LogDebug($"Rescaling wave settings! difficulty={population.DifficultyFactor}, settings={settings}");
            }

            puzzle = puzzle with { Population = population, Settings = settings };
            Plugin.Logger.LogDebug($"Zone {LocalIndex} alarm(rolled): {puzzle}");
        }
        else
        {
            Plugin.Logger.LogDebug($"Zone {LocalIndex} alarm(fixed): {puzzle}");
        }

        if (puzzle.Comment != null)
            Plugin.Logger.LogWarning($"Zone {LocalIndex} alarm(secret): EventsOnDoorScanStart={puzzle.EventsOnDoorScanStart.Count} EventsOnDoorOpen={puzzle.EventsOnOpenDoor.Count}");

        EventsOnApproachDoor.AddRange(puzzle.EventsOnApproachDoor);
        EventsOnUnlockDoor.AddRange(puzzle.EventsOnUnlockDoor);
        EventsOnOpenDoor.AddRange(puzzle.EventsOnOpenDoor);
        EventsOnDoorScanStart.AddRange(puzzle.EventsOnDoorScanStart);
        EventsOnDoorScanDone.AddRange(puzzle.EventsOnDoorScanDone);

        var parent = level.Planner.GetBuildFrom(new ZoneNode { Bulkhead = Bulkhead.Extreme, ZoneNumber = LocalIndex });
        var isInfection = level.FogSettings.IsInfectious;

        // [Error  :     Unity] ERROR: Tried to add enemyType: 106 into WaveID: 1. This enemyType is not Linked to an ENEMY_TYPE in EnemyGroup.cs/TryGetAKSwitchIDFromEnemyType
        // [Error  :     Unity] ERROR: Tried to add enemyType: 106 into WaveID: 1. This enemyType is not Linked to an ENEMY_TYPE in EnemyGroup.cs/TryGetAKSwitchIDFromEnemyType
        // [Error  :     Unity] ERROR: Tried to add enemyType: 106 into WaveID: 1. This enemyType is not Linked to an ENEMY_TYPE in EnemyGroup.cs/TryGetAKSwitchIDFromEnemyType
        // [Message:     Unity] <b>LG_SecurityDoor:</b> OnSyncDoorStatusChange: m_lastState.hasBeenApproached: True, state.hasBeenApproached: True, LinkedToZoneData: GameData.ExpeditionZoneData
        // [Message:     Unity] WardenObjectiveManager.CheckAndExecuteEventsOnTrigger, 0 trigger: None SNet.IsMaster: True Duration: 0
        // [Error  :     Unity] ERROR: Tried to add enemyType: 106 into WaveID: 2. This enemyType is not Linked to an ENEMY_TYPE in EnemyGroup.cs/TryGetAKSwitchIDFromEnemyType

        // Add custom events on alarms
        if (!puzzle.FixedAlarm)
        {
            switch (level.Tier)
            {
                case "A":
                {
                    puzzle.WantedDistanceBetweenPuzzleComponents *= Generator.NextDouble(0.9, 1.05);

                    break;
                }

                case "B":
                {
                    if (Generator.Flip(0.05))
                        puzzle.WantedDistanceFromStartPos += Generator.Between(15, 20);

                    // Small chance to disable lights during the alarm
                    if (Generator.Flip(0.08))
                        AlarmModifier_LightsOff();
                    else if (Generator.Flip(0.01))
                        AlarmModifier_FogFlood(puzzle.ClearTime(1.2, 1.4));

                    if (Generator.Flip(0.005))
                        EventsOnApproachDoor.AddSpawnWave(
                            GenericWave.SinglePouncer,
                            Generator.Between(1, 6));

                    break;
                }

                case "C":
                {
                    if (Generator.Flip(0.2))
                        puzzle.WantedDistanceFromStartPos += Generator.Between(20, 25);

                    puzzle.WantedDistanceBetweenPuzzleComponents *= Generator.NextDouble(1.0, 1.1);

                    // Infection hybrids during wave, lower the chance if it's in fog
                    if (isInfection && !InFog && Generator.Flip(0.2) )
                        EventsOnDoorScanStart.AddGenericWave(new GenericWave
                        {
                            Population = WavePopulation.OnlyInfectedHybrids,
                            Settings = Generator.Select(new List<(double, WaveSettings)>
                            {
                                (0.35, WaveSettings.SingleWave_MiniBoss_4pts),
                                (0.65, WaveSettings.SingleWave_MiniBoss_8pts)
                            })
                        }, puzzle.ClearTime() * Generator.NextDouble(0.2, 0.6));

                    // Small chance to disable lights during the alarm
                    if (Generator.Flip(0.1))
                        AlarmModifier_LightsOff();
                    else if (Generator.Flip(0.05))
                        AlarmModifier_FogFlood(puzzle.ClearTime(1.2, 1.4));

                    // Tiny chance for bait door approach pouncer
                    if (Generator.Flip(0.01))
                        EventsOnApproachDoor.AddSpawnWave(
                            GenericWave.SinglePouncer,
                            Generator.Between(1, 6));

                    break;
                }

                case "D":
                {
                    if (Generator.Flip(0.32))
                        puzzle.WantedDistanceFromStartPos += Generator.Between(20, 25);

                    puzzle.WantedDistanceBetweenPuzzleComponents *= puzzle.Puzzle.Count switch
                    {
                        3 => Generator.NextDouble(1.5, 1.8),
                        4 => Generator.NextDouble(1.2, 1.4),
                        _ => Generator.NextDouble(1.0, 1.2)
                    };

                    // Infection hybrids during wave, lower the chance if it's in fog
                    if (isInfection && Generator.Flip(InFog ? 0.2 : 0.4) )
                        EventsOnDoorScanStart.AddGenericWave(new GenericWave
                        {
                            Population = WavePopulation.OnlyInfectedHybrids,
                            Settings = Generator.Select(new List<(double, WaveSettings)>
                            {
                                (0.35, WaveSettings.SingleWave_MiniBoss_4pts),
                                (0.60, WaveSettings.SingleWave_MiniBoss_8pts),
                                (0.05, WaveSettings.SingleWave_MiniBoss_12pts)
                            })
                        }, puzzle.ClearTime() * Generator.NextDouble(0.2, 0.6));

                    // Small chance to disable lights during the alarm
                    if (Generator.Flip(0.12))
                    {
                        AlarmModifier_LightsOff();

                        // Shadow pouncer with the lights off!
                        // TODO: spawn later in the wave?
                        if (Generator.Flip(0.1))
                            EventsOnDoorScanStart.AddSpawnWave(
                                GenericWave.SinglePouncerShadow,
                                Generator.Between(4, 16));
                    }

                    if (Generator.Flip(0.07))
                        AlarmModifier_FogFlood(puzzle.ClearTime(1.1, 1.3));

                    // Tiny chance for bait door approach pouncer
                    if (Generator.Flip(0.01))
                        EventsOnApproachDoor.AddSpawnWave(
                            Generator.Flip() ?
                                GenericWave.SinglePouncerShadow :
                                GenericWave.SinglePouncer,
                            Generator.Between(1, 6));

                    // "Fun" single enemies on start wave
                    if (Generator.Flip(0.05))
                        EventsOnDoorScanStart.AddSpawnWave(GenericWave.SingleTank, Generator.Between(1, 27));
                    break;
                }

                case "E":
                {
                    if (Generator.Flip(0.45))
                        puzzle.WantedDistanceFromStartPos += Generator.Between(20, 30);

                    puzzle.WantedDistanceBetweenPuzzleComponents *= puzzle.Puzzle.Count switch
                    {
                        3 => Generator.NextDouble(1.6, 2.0),
                        4 => Generator.NextDouble(1.3, 1.5),
                        _ => Generator.NextDouble(1.1, 1.3)
                    };

                    // Infection hybrids during wave, lower the chance if it's in fog
                    if (isInfection && Generator.Flip(InFog ? 0.4 : 0.6) )
                        EventsOnDoorScanStart.AddGenericWave(new GenericWave
                        {
                            Population = WavePopulation.OnlyInfectedHybrids,
                            Settings = Generator.Select(new List<(double, WaveSettings)>
                            {
                                (0.2, WaveSettings.SingleWave_MiniBoss_4pts),
                                (0.6, WaveSettings.SingleWave_MiniBoss_8pts),
                                (0.2, WaveSettings.SingleWave_MiniBoss_12pts)
                            })
                        }, puzzle.ClearTime() * Generator.NextDouble(0.2, 0.6));

                    // Small chance to disable lights during the alarm
                    if (Generator.Flip(0.3))
                    {
                        AlarmModifier_LightsOff();

                        // Shadow pouncer with the lights off!
                        if (Generator.Flip(0.1))
                            EventsOnDoorScanStart.AddSpawnWave(
                                GenericWave.SinglePouncerShadow,
                                Generator.Between(4, 16));
                    }

                    if (Generator.Flip(0.15))
                        AlarmModifier_FogFlood(puzzle.ClearTime(1.1, 1.1));

                    // Tiny chance to be pouncered when opening the door or
                    // approaching the door
                    if (Generator.Flip(0.02))
                        EventsOnOpenDoor.AddSpawnWave(
                            GenericWave.SinglePouncerShadow,
                            Generator.Between(4, 16));
                    else if (Generator.Flip(0.03))
                        EventsOnApproachDoor.AddSpawnWave(
                            Generator.Flip(0.7) ?
                                GenericWave.SinglePouncerShadow :
                                GenericWave.SinglePouncer,
                            Generator.Between(1, 6));

                    // Single enemies on wave start
                    if (Generator.Flip(0.05))
                        EventsOnDoorScanStart.AddSpawnWave(GenericWave.SingleTank, Generator.Between(5, 27));
                    else if (Generator.Flip(0.03))
                        EventsOnDoorScanStart.AddSpawnWave(GenericWave.SingleTankPotato, Generator.Between(5, 27));
                    break;
                }
            }
        }

        if (Bins.ChainedPuzzles.Contains(puzzle))
            Plugin.Logger.LogInfo($"Zone {LocalIndex} alarm reassigned: {puzzle}");

        Alarm = ChainedPuzzle.FindOrPersist(puzzle);
    }
    #endregion

    #region Fog
    /// <summary>
    /// Set's the zone to be out of fog (based on the level settings).
    /// </summary>
    /// <param name="level"></param>
    public void SetOutOfFog(Level level)
    {
        Height? altitude = Generator.Pick(level.FogSettings.NoFogLevels);

        if (altitude != null)
        {
            Altitude = new Altitude { AllowedZoneAltitude = (Height)altitude };
            InFog = false;
        }
    }

    /// <summary>
    /// Set's the zone to be in fog (based on the level settings).
    /// </summary>
    /// <param name="level"></param>
    public void SetInFog(Level level)
    {
        Height? altitude = Generator.Pick(level.FogSettings.FogLevels);

        if (altitude != null)
        {
            Altitude = new Altitude { AllowedZoneAltitude = (Height)altitude };
            InFog = true;

            // Always add fog repellers into the fog zone
            ConsumableDistributionInZone = ConsumableDistribution.Baseline_FogRepellers.PersistentId;
        }
    }

    /// <summary>
    /// Checks the level settings for fog and optionally sets the altitude
    /// </summary>
    public void RollFog(Level level)
    {
        // If we don't have fog enabled then return early.
        if (level.Settings.Modifiers.Contains(LevelModifiers.NoFog))
            return;

        double chance;

        if (level.Settings.Modifiers.Contains(LevelModifiers.Fog))
            chance = 0.5;
        else if (level.Settings.Modifiers.Contains(LevelModifiers.HeavyFog))
            chance = 0.75;
        else
            return;

        if (Generator.Flip(chance))
            SetInFog(level);
        else
            SetOutOfFog(level);
    }
    #endregion

    #region Clear Time Calculation
    // Clear time is the time taken to clear and get through the zone.
    // Most components have a scale factor associated with them to allow fine-tuning of time
    // grants to give as accurate an estimate as possible.
    //
    // Time estimates are supposed to be as accurate as possible for a team to quickly and
    // efficiently clear a zone.

    private const double clearTimeFactor_AlarmsTime     = 1.20; // Time factor for duration of each scan
    private const double clearTimeFactor_AlarmsTraverse = 1.20; // Distance factor for walking between scans
    private const double clearTimeFactor_AreaCoverage   = 1.30; // Map area factor for size of zone
    private const double clearTimeFactor_EnemyPoints    = 1.80; // Points factor for each enemy point
    private const double clearTimeFactor_Bosses         = 1.00; // Per boss factor

    /// <summary>
    /// Estimates the time (in seconds) to clear a zone based on it's Area coverage.
    /// </summary>
    /// <returns></returns>
    public double ClearTime_AreaCoverage()
        => Coverage.Max * clearTimeFactor_AreaCoverage;

    /// <summary>
    /// Time based on door alarms
    /// We sum for the component durations, the distance from start pos, and the distance
    /// between the alarm components
    /// </summary>
    /// <returns></returns>
    public double ClearTime_Alarm()
        => 10.0 + Alarm.Puzzle.Sum(component => component.Duration)
                * clearTimeFactor_AlarmsTime
                + Alarm.WantedDistanceFromStartPos
                * clearTimeFactor_AlarmsTraverse
                + (Alarm.Puzzle.Count - 1)
                * Alarm.WantedDistanceBetweenPuzzleComponents
                * clearTimeFactor_AlarmsTraverse;

    /// <summary>
    /// Estimates the time (in seconds) to clear a zone only of the enemies marked as bosses.
    /// </summary>
    /// <returns></returns>
    public double ClearTime_Bosses()
        => EnemySpawningInZone
               .Where(spawn => spawn.Tags.Contains("boss"))
               .Sum(spawn =>
                   (Enemy)spawn.Difficulty switch
                   {
                       Enemy.Mother => 60.0 * (spawn.Points / 10.0),
                       Enemy.PMother => 75.0 * (spawn.Points / 10.0),
                       Enemy.Tank => 45.0 * (spawn.Points / 10.0),
                       Enemy.TankPotato => 30.0 * (spawn.Points / 10.0),

                       // Some unknown enemy, we won't add time for unknowns
                       _ => 0.0
                   })
           * clearTimeFactor_Bosses;

    /// <summary>
    /// TODO: this needs to be improved
    /// </summary>
    /// <returns></returns>
    public double ClearTime_BloodDoor()
        => BloodDoor != BloodDoor.None ? 20.0 : 0.0;

    /// <summary>
    /// Estimates the time (in seconds) to clear a zones hibernating enemies
    ///
    /// Note that this will add points based on bosses, but there's a separate extra
    /// factor for clearing bosses due to their points not being very high
    /// </summary>
    /// <returns></returns>
    public double ClearTime_Enemies()
        => EnemySpawningInZone.Sum(spawn => spawn.Points)
           * clearTimeFactor_EnemyPoints;

    /// <summary>
    /// Estimate the time it would take to clear this zone. This tries to give a close estimate
    /// for expedient clearing of the area. There are scale factors to adjust the times which
    /// is helpful for granting more/less time based on level tier. Knowing this value is very
    /// helpful for a number of objectives where players need to race against the clock to
    /// achieve some objective. This function lets us grant the players an amount of time that
    /// is difficult but still possible.
    /// </summary>
    /// <returns>Estimated time to clear the zone and any alarms to _enter_ the zone</returns>
    public double GetClearTimeEstimate()
    {
        const double factorAlarms = 1.20;
        const double factorBoss = 1.0;
        const double factorCoverage = 1.30;
        const double factorEnemyPoints = 2.40;

        // Add time based on the zone size
        var timeCoverage = ClearTime_AreaCoverage();
        // var timeCoverage = Coverage.Max * factorCoverage;

        // Time based on enemy points in zones
        var timeEnemyPoints = ClearTime_Enemies();
        // var timeEnemyPoints = EnemySpawningInZone
        //     .Sum(spawn => spawn.Points) * factorEnemyPoints;

        // Find and add extra time for bosses. These are generally quite hard to deal with.
        var timeBosses = ClearTime_Bosses();
        // var timeBosses = EnemySpawningInZone
        //     .Where(spawn => spawn.Tags.Contains("boss"))
        //     .Sum(spawn =>
        //         (Enemy)spawn.Difficulty switch
        //         {
        //             Enemy.Mother => 60.0 * (spawn.Points / 10.0),
        //             Enemy.PMother => 75.0 * (spawn.Points / 10.0),
        //             Enemy.Tank => 45.0 * (spawn.Points / 10.0),
        //             Enemy.TankPotato => 30.0 * (spawn.Points / 10.0),
        //
        //             // Some unknown enemy, we won't add time for unknowns
        //             _ => 0.0
        //         });
        // timeBosses *= factorBoss;

        // Time based on door alarms
        // We sum for the component durations, the distance from start pos, and the distance
        // between the alarm components
        var timeAlarms = ClearTime_Alarm();
        // var timeAlarms = 10.0 + Alarm.Puzzle.Sum(component => component.Duration)
        //                     + Alarm.WantedDistanceFromStartPos
        //                     + (Alarm.Puzzle.Count - 1) * Alarm.WantedDistanceBetweenPuzzleComponents;
        // timeAlarms *= factorAlarms;

        // Give +20s for a blood door.
        // TODO: adjust based on spawns in the blood door.
        var timeBloodDoor = 0.0;
        var timeBloodDoorBoss = 0.0;

        if (BloodDoor != BloodDoor.None)
        {
            timeBloodDoor = 20.0;

            // We need to add extra time for mothers. BloodDoor_BossMother is 1x mother
            // TODO: I'd prefer to do a full enemy count like we do for the whole zone.
            if (BloodDoor.EnemyGroupInfrontOfDoor == (uint)VanillaEnemyGroup.BloodDoor_BossMother)
                timeBloodDoorBoss += 60.0;
            if (BloodDoor.EnemyGroupInArea == (uint)VanillaEnemyGroup.BloodDoor_BossMother)
                timeBloodDoorBoss += 60.0;
        }

        // Sum the values
        var total = timeCoverage
                    + timeEnemyPoints
                    + timeBosses
                    + timeAlarms
                    + timeBloodDoor
                    + timeBloodDoorBoss;

        Plugin.Logger.LogDebug($"Zone {LocalIndex} time budget: total={total}s -- "
                               + $"alarms={timeAlarms}s coverage={timeCoverage}s "
                               + $"enemies={timeEnemyPoints}s "
                               + $"bosses={timeBosses}s "
                               + $"blood_doors={timeBloodDoor}s "
                               + $"blood_door_bosses={timeBloodDoorBoss}s");
        return total;
    }
    #endregion

    #region Internal plugin properties
    [JsonIgnore]
    internal Level level { get; init; }

    [JsonIgnore]
    internal LevelLayout layout { get; init; }

    /// <summary>
    /// Flags whether the zone is in fog or not.
    /// </summary>
    [JsonIgnore]
    public bool InFog { get; set; } = false;

    /// <summary>
    /// Factor used to multiply the enemy spawning points in this zone.
    /// </summary>
    [JsonIgnore]
    public double EnemyPointsMultiplier { get; set; } = 1.0;
    #endregion

    #region Unused by us properties
    public int AliasOverride = -1;
    #endregion

    #region Seed properties

    public int SubSeed { get; set; } = 24;

    public int MarkerSubSeed { get; set; } = 3;

    public int LightsSubSeed { get; set; } = 1;

    #endregion

    /// <summary>
    /// Zone number offset from the level layout
    /// </summary>
    public int LocalIndex { get; set; } = 0;

    /// <summary>
    /// Which zone to build this zone from
    /// </summary>
    public int BuildFromLocalIndex { get; set; } = 0;

    #region Zone Prefix
    /// <summary>
    /// Computed property. If this returns true the game will use the provided alias prefixes
    /// </summary>
    [JsonProperty("OverrideAliasPrefix")]
    public bool OverrideAliasPrefix => AliasPrefix != "";

    /// <summary>
    /// What Zone Alias prefix to use. Leave this empty for the default `ZONE` prefix. This appears
    /// on doors, etc.
    /// </summary>
    [JsonProperty("AliasPrefixOverride")]
    public string AliasPrefix { get; set; } = "";

    /// <summary>
    /// The short prefix
    ///
    /// Default = "Z"
    /// </summary>
    [JsonProperty("AliasPrefixShortOverride")]
    public string AliasPrefixShort { get; set; } = "Z";
    #endregion

    /// <summary>
    /// Which tileset to use
    /// </summary>
    public SubComplex SubComplex { get; set; } = SubComplex.All;

    [JsonProperty("CoverageMinMax")]
    public CoverageMinMax Coverage { get; set; } = CoverageMinMax.Medium;

    /// <summary>
    /// Where in the source zone to make the entrance for this zone
    /// </summary>
    public ZoneEntranceBuildFrom StartPosition { get; set; } = ZoneEntranceBuildFrom.Random;

    /// <summary>
    /// What direction to try and place the door for this zone
    /// </summary>
    public ZoneBuildExpansion StartExpansion { get; set; } = ZoneBuildExpansion.Random;

    /// <summary>
    /// What direction to build this zone towards
    /// </summary>
    public ZoneExpansion ZoneExpansion { get; set; } = ZoneExpansion.Random;

    /// <summary>
    /// If we specify a custom geomorph it goes here
    /// </summary>
    public string? CustomGeomorph { get; set; } = null;

    public bool IgnoreRandomGeomorphRotation { get; set; } = false;

    /// <summary>
    /// Which Light to select
    /// </summary>
    public Lights.Light LightSettings { get; set; } = Lights.Light.AlmostWhite_1;

    /// <summary>
    /// What allowed altitiudes the level can have
    /// </summary>
    [JsonProperty("AltitudeData")]
    public Altitude Altitude { get; set; } = new();

    #region Events
    public List<WardenObjectiveEvent> EventsOnEnter { get; set; } = new();
    public List<WardenObjectiveEvent> EventsOnPortalWarp { get; set; } = new();
    public List<WardenObjectiveEvent> EventsOnApproachDoor { get; set; } = new();
    public List<WardenObjectiveEvent> EventsOnUnlockDoor { get; set; } = new();
    public List<WardenObjectiveEvent> EventsOnOpenDoor { get; set; } = new();
    public List<WardenObjectiveEvent> EventsOnDoorScanStart { get; set; } = new();
    public List<WardenObjectiveEvent> EventsOnDoorScanDone { get; set; } = new();
    #endregion

    #region Puzzle settings
    public ProgressionPuzzle ProgressionPuzzleToEnter { get; set; } = new();

    /// <summary>
    /// Reference to the alarm we are using for the chained puzzle
    /// </summary>
    [JsonIgnore]
    public ChainedPuzzle Alarm { get; set; } = ChainedPuzzle.None;

    /// <summary>
    /// Which security scan to use to enter
    /// </summary>
    public uint ChainedPuzzleToEnter
    {
        get => Alarm.PersistentId;
        private set { }
    }
    #endregion

    #region Security door
    public bool IsCheckpointDoor { get; set; } = false;

    public bool PlayScannerVoiceAudio { get; set; } = false;

    public bool SkipAutomaticProgressionObjective { get; set; } = false;

    public SecurityGate SecurityGateToEnter { get; set; } = SecurityGate.Security;

    public bool UseStaticBioscanPointsInZone { get; set; } = false;
    #endregion

    #region Error alarms
    /// <summary>
    /// Whether the alarm (typically an ://ERROR! Alarm) of this zone is turned off on a terminal.
    /// </summary>
    public bool TurnOffAlarmOnTerminal { get; set; } = false;

    /// <summary>
    /// Terminal data used when TurnOffAlarmOnTerminal is set to true.
    /// </summary>
    public TerminalZoneSelection TerminalPuzzleZone { get; set; } = new TerminalZoneSelection();

    /// <summary>
    /// Events to trigger when disable alarm terminal command is called.
    /// </summary>
    public List<WardenObjectiveEvent> EventsOnTerminalDeactivateAlarm = new List<WardenObjectiveEvent>();
    #endregion

    #region Enemies
    /// <summary>
    ///
    /// </summary>
    [JsonProperty("ActiveEnemyWave")]
    public BloodDoor BloodDoor { get; set; } = BloodDoor.None;

    public List<EnemySpawningData> EnemySpawningInZone { get; set; } = new();
    #endregion

    #region Objective settings
    public int HSUClustersInZone { get; set; } = 0;

    /// <summary>
    /// Used for distribute cells as well as generator puzzles
    /// </summary>
    public List<FunctionPlacementData> PowerGeneratorPlacements { get; set; } = new();
    #endregion

    #region Respawn settings
    /// <summary>
    /// Whether the enemies respawn
    /// </summary>
    public bool EnemyRespawning { get; set; } = false;

    public bool EnemyRespawnRequireOtherZone { get; set; } = true;

    public int EnemyRespawnRoomDistance { get; set; } = 2;

    public double EnemyRespawnTimeInterval { get; set; } = 10.0;

    public double EnemyRespawnCountMultiplier { get; set; } = 1.0;

    public JArray EnemyRespawnExcludeList = new JArray();
    #endregion

    #region Static Spawns (Spitters, Mother Sacks etc)
    /// <summary>
    ///
    /// </summary>
    public List<StaticSpawnDataContainer> StaticSpawnDataContainers { get; set; } = new();
    #endregion

    public int CorpseClustersInZone { get; set; } = 0;
    public int ResourceContainerClustersInZone { get; set; } = 0;
    public int GeneratorClustersInZone { get; set; } = 0;
    public DistributionAmount CorpsesInZone { get; set; } = DistributionAmount.None;
    public DistributionAmount GroundSpawnersInZone { get; set; } = DistributionAmount.Some;
    public DistributionAmount HSUsInZone { get; set; } = DistributionAmount.None;
    public DistributionAmount DeconUnitsInZone { get; set; } = DistributionAmount.None;
    public bool AllowSmallPickupsAllocation { get; set; } = true;
    public bool AllowResourceContainerAllocation { get; set; } = true;

    public List<TerminalPlacement> TerminalPlacements { get; set; } = new();

    public bool ForbidTerminalsInZone { get; set; } = false;
    public JArray DumbwaiterPlacements { get; set; } = new();

    #region Resources and items
    /// <summary>
    /// Consumables include glowsticks, c-foam grenades, other slot 5 consumable items.
    /// </summary>
    public uint ConsumableDistributionInZone { get; set; }
        = ConsumableDistribution.Baseline.PersistentId;

    /// <summary>
    /// How many health pack uses are in this zone. Default = 5
    /// </summary>
    [JsonProperty("HealthMulti")]
    public double HealthPacks { get; set; } = 5;

    public Placement HealthPlacement { get; set; } = new();

    /// <summary>
    /// How many ammo pack uses are in this zone. Default = 4
    /// </summary>
    [JsonProperty("WeaponAmmoMulti")]
    public double AmmoPacks { get; set; } = 4;

    public Placement WeaponAmmoPlacement { get; set; } = new();

    /// <summary>
    /// How many tool pack uses are in this zone. Default = 3.5
    /// </summary>
    [JsonProperty("ToolAmmoMulti")]
    public double ToolPacks { get; set; } = 3.5;

    public Placement ToolAmmoPlacement { get; set; } = new();

    /// <summary>
    /// How many disinfect pack uses are in this zone. Default = 0 (5 in base game)
    /// </summary>
    [JsonProperty("DisinfectionMulti")]
    public double DisinfectPacks { get; set; } = 0; // Default is 5 in base game

    public Placement DisinfectionPlacement { get; set; } = new();

    public List<FunctionPlacementData> DisinfectionStationPlacements { get; set; } = new();

    /// <summary>
    /// Takes an input function and applies it to each of the three major resource multiples.
    /// </summary>
    /// <param name="transformer"></param>
    public void SetMainResourceMulti(Func<double, double> transformer)
    {
        HealthPacks = transformer(HealthPacks);
        ToolPacks = transformer(ToolPacks);
        AmmoPacks = transformer(AmmoPacks);
    }
    #endregion

    public Zone(Level level, Bulkhead bulkhead) : this(level, level.GetLevelLayout(bulkhead)!)
    {
    }

    public Zone(Level level, LevelLayout layout)
    {
        this.level = level;
        this.layout = layout;

        SubSeed = Generator.Between(10, 100);

        // Always ensure a terminal is placed in the zone
        TerminalPlacements.Add(new TerminalPlacement());

        // Grant additional ammo for D and E tier levels
        AmmoPacks = level.Tier switch
        {
            "D" => 5,
            "E" => 6,
            _ => 4
        };
    }
}
