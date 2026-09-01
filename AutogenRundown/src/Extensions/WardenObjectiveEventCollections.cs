using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;
using AutogenRundown.DataBlocks.Custom.ZoneSensors;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Light;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.Extensions;

public static class WardenObjectiveEventCollections
{
    private static int GetLayerFromBulkhead(Bulkhead bulkhead)
        => bulkhead switch
        {
            Bulkhead.Main => 0,
            Bulkhead.Extreme => 1,
            Bulkhead.Overload => 2,
            _ => 0
        };

    #region Doors
    /// <summary>
    ///
    /// </summary>
    /// <param name="events"></param>
    /// <param name="bulkhead"></param>
    /// <param name="zoneIndex"></param>
    /// <param name="delay"></param>
    /// <param name="trigger"></param>
    /// <param name="lockMessage"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddLockDoor(
        this ICollection<WardenObjectiveEvent> events,
        Bulkhead bulkhead,
        int zoneIndex,
        double delay = 0.0,
        WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart,
        string? lockMessage = null)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.LockSecurityDoor,
                Dimension = DimensionIndex.Reality,
                Layer = GetLayerFromBulkhead(bulkhead),
                LocalIndex = zoneIndex,
                SpecialText = lockMessage ?? Lore.LockedDoorMessage
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddLockExtreme(
        this ICollection<WardenObjectiveEvent> events,
        double delay = 0.0,
        WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart,
        string? lockMessage = null)
        => events.AddLockDoor(Bulkhead.Extreme, 0, delay, trigger, lockMessage);

    public static ICollection<WardenObjectiveEvent> AddLockOverload(
        this ICollection<WardenObjectiveEvent> events,
        double delay = 0.0,
        WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart,
        string? lockMessage = null)
        => events.AddLockDoor(Bulkhead.Overload, 0, delay, trigger, lockMessage);
    #endregion

    #region Enemies

    /// <summary>
    ///
    /// </summary>
    /// <param name="events"></param>
    /// <param name="bulkhead"></param>
    /// <param name="zoneIndex"></param>
    /// <param name="delay"></param>
    /// <param name="trigger"></param>
    /// <param name="alertMessage"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddAlertEnemies(
        this ICollection<WardenObjectiveEvent> events,
        Bulkhead bulkhead,
        int zoneIndex,
        double delay = 0.0,
        WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart,
        string? alertMessage = null)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.AlertEnemiesInZone,
                Dimension = DimensionIndex.Reality,
                Layer = GetLayerFromBulkhead(bulkhead),
                LocalIndex = zoneIndex,
                Enabled = true,
                Delay = delay,
                Trigger = trigger,
                WardenIntel = new DataBlocks.Text(alertMessage ?? string.Empty)
            });

        return events;
    }

    /// <summary>
    /// Turn's off alarms. Optionally with a given identifier.
    ///
    /// With an identifier, AWO scopes the stop to waves spawned with that same identifier.
    /// Vanilla alone would ignore it and stop everything
    /// (Modules-ASM/WardenObjectiveManager.cs:2305 -> StopAllWardenObjectiveEnemyWaves).
    /// Without an identifier the stop is global — including untagged waves such as
    /// WavesOnElevatorLand entries (e.g. the BossAlarm signature's boss stream).
    /// </summary>
    /// <param name="events"></param>
    /// <param name="delay"></param>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddTurnOffAlarms(
        this ICollection<WardenObjectiveEvent> events,
        double delay = 0.0,
        string? identifier = null)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.StopEnemyWaves,
                Identifier = identifier,
                Delay = delay
            });

        return events;
    }

    /// <summary>
    /// Adds a spawn wave event
    /// </summary>
    /// <param name="events"></param>
    /// <param name="wave"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddSpawnWave(
        this ICollection<WardenObjectiveEvent> events,
        GenericWave wave,
        double delay = 0.0,
        string? identifier = null)
    {
        if (wave == GenericWave.None)
            return events;

        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SpawnEnemyWave,
                Delay = delay,
                Identifier = identifier,
                EnemyWaveData = wave
            });

        return events;
    }

    /// <summary>
    /// Adds a scripted error alarm: not a real alarm but an event loop that periodically
    /// fires a warden intel message, a sound cue, and an enemy wave. R7D1's snatcher alarm
    /// does this with 1 snatcher every 4 minutes (finite there: 19 waves, ~1hr 16min); the
    /// default here is infinite, which is what the Stalker signature uses.
    ///
    /// Results of this are:
    ///     - No combat music (TriggerAlarm is forced off on the wave)
    ///     - Players get out of combat stamina between waves
    ///     - "Alarm" _cannot_ be deactivated by DEACTIVATE_ALARMS
    ///     - An infinite loop also survives StopAllWavesBeforeGotoWin and runs through
    ///       extraction (it is an EventLoop, not a warden wave)
    ///
    /// See link for more details:
    /// https://gtfo.fandom.com/wiki/R7D1#Trivia
    /// </summary>
    /// <param name="events"></param>
    /// <param name="wave">Wave to spawn each pulse. TriggerAlarm is forced to false.</param>
    /// <param name="waveCount">Total number of pulses; -1 (default) loops forever.</param>
    /// <param name="interval">Seconds between pulses.</param>
    /// <param name="delay">Grace period before the loop starts.</param>
    /// <param name="message">Warden intel shown each pulse. Empty string for no intel.</param>
    /// <param name="sound">Sound cue played each pulse.</param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddScriptedErrorAlarm(
        this ICollection<WardenObjectiveEvent> events,
        GenericWave wave,
        int waveCount = -1,
        double interval = 240.0,
        double delay = 2.0,
        string message = ":://WARNING - BIOMASS SIGNATURE",
        Sound sound = Sound.Enemies_DistantLowRoar)
    {
        if (wave == GenericWave.None)
            return events;

        var eventLoop = new EventLoop
        {
            LoopIndex = (int)Generator.GetPersistentId(),
            LoopDelay = interval,
            LoopCount = waveCount
        };

        // Inner events must keep the default Trigger = None: Start-triggered events inside an
        // EventLoop never fire (see EventLoop doc comment).
        if (message.Length > 0)
            eventLoop.EventsToActivate.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.None,
                    Delay = 0.0,
                    WardenIntel = new DataBlocks.Text(message)
                });
        eventLoop.EventsToActivate.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlaySound,
                SoundId = sound,
                Delay = 0.5
            });
        eventLoop.EventsToActivate.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SpawnEnemyWave,
                Delay = 4.0,
                EnemyWaveData = wave with { TriggerAlarm = false }
            });

        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.StartEventLoop,
                Delay = delay,
                EventLoop = eventLoop
            });

        return events;
    }

    #endregion

    #region Event Loops
    /// <summary>
    /// Disable an event loop
    /// </summary>
    /// <param name="events"></param>
    /// <param name="loopId"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddStopLoop(
        this ICollection<WardenObjectiveEvent> events,
        int loopId,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.StopEventLoop,
                Count = loopId,
                Delay = delay
            });

        return events;
    }

    #endregion

    #region Fog

    /// <summary>
    /// Floods the level with fog on event start.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="delay"></param>
    /// <param name="duration"></param>
    /// <param name="message"></param>
    public static ICollection<WardenObjectiveEvent> AddFillFog(
        this ICollection<WardenObjectiveEvent> events,
        double delay = 5.0,
        double duration = 20.0,
        string? message = ":://CRITICAL FAILURE - VENTILATION SYSTEMS OFFLINE")
        => events.AddSetFog(Fog.FullFog, delay, duration, message);

    /// <summary>
    /// Sets the fog in the level to the specified fog
    /// </summary>
    /// <param name="events"></param>
    /// <param name="fogSettings"></param>
    /// <param name="delay"></param>
    /// <param name="duration"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddSetFog(
        this ICollection<WardenObjectiveEvent> events,
        Fog fogSettings,
        double delay = 5.0,
        double duration = 20.0,
        string? message = ":://CRITICAL FAILURE - VENTILATION SYSTEMS OFFLINE")
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SetFogSettings,
                Dimension = DimensionIndex.Reality, // TODO: support dimensions
                Trigger = WardenObjectiveEventTrigger.OnStart,
                FogSetting = fogSettings.PersistentId,
                FogTransitionDuration = duration,
                Delay = delay + 0.7,
            });

        // Don't play the sound or show the message if message is null.
        if (message == null) return events;

        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlaySound,
                Trigger = WardenObjectiveEventTrigger.OnStart,
                SoundId = Sound.KdsDeepVentilationProcedure,
                Delay = delay
            });
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.None,
                Trigger = WardenObjectiveEventTrigger.OnStart,
                Delay = delay + 4.0,
                WardenIntel = new DataBlocks.Text(message)
            });

        return events;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="events"></param>
    /// <param name="fogSettings1"></param>
    /// <param name="fogSettings2"></param>
    /// <param name="loopIndex"></param>
    /// <param name="delay1">How long fog 1 stays active after transitioning</param>
    /// <param name="duration1">How long fog 1 transition lasts</param>
    /// <param name="delay2">How long fog 2 stays active after transitioning</param>
    /// <param name="duration2">How long fog 2 transition lasts</param>
    /// <param name="startDelay">Grace period before the first cycle begins</param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddCyclingFog(
        this ICollection<WardenObjectiveEvent> events,
        Fog fogSettings1,
        Fog fogSettings2,
        int loopIndex = 1,
        double delay1 = 30.0,
        double duration1 = 45.0,
        double delay2 = 30.0,
        double duration2 = 45.0,
        double startDelay = 0.0)
    {
        var eventLoop = new EventLoop
        {
            LoopIndex = loopIndex,
            LoopDelay = duration1 + delay1 + duration2 + delay2,
            LoopCount = -1
        };

        eventLoop.EventsToActivate.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SetFogSettings,
                Dimension = DimensionIndex.Reality, // TODO: support dimensions
                FogSetting = fogSettings1.PersistentId,
                FogTransitionDuration = duration1,
                SoundId = (Sound)2275333205,
                Delay = 0
            });
        eventLoop.EventsToActivate.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SetFogSettings,
                Dimension = DimensionIndex.Reality, // TODO: support dimensions
                FogSetting = fogSettings2.PersistentId,
                FogTransitionDuration = duration2,
                SoundId = (Sound)2275333205,
                Delay = duration1 + delay1
            });

        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.StartEventLoop,
                Delay = startDelay,
                EventLoop = eventLoop
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddCyclingFog(
        this ICollection<WardenObjectiveEvent> events,
        Level level,
        double startDelay = 0.0)
    {
        var (delay1, duration1, delay2, duration2) = (level.Tier, level.Settings.Modifiers.Contains(LevelModifiers.FogIsInfectious)) switch
        {
            ("D", true) => (60, 25, 75, 22),
            ("D", _   ) => (45, 25, 45, 25),

            ("E", true) => (90, 35, 30, 22),
            ("E", _   ) => (45, 25, 45, 25),

            (_, true) => (30, 25, 90, 35), // Shorter infectious cycle by default

            _ => (45, 30, 45, 30)
        };
        var (fog1, fog2) = (level.Settings.Modifiers.Contains(LevelModifiers.FogIsInfectious)) switch
        {
            true  => (Fog.CyclingFog_Heavy_Infectious, Fog.CyclingFog_Clear_Infectious),
            false => (Fog.CyclingFog_Heavy,            Fog.CyclingFog_Clear           )
        };

        Plugin.Logger.LogDebug($"AddCyclingFog()");

        return events.AddCyclingFog(fog1, fog2, (int)Generator.GetPersistentId(), delay1, duration1, delay2, duration2, startDelay);
    }

    #endregion

    #region Lights
    /// <summary>
    /// Add all lights off. Turns off all lights in all zones
    /// </summary>
    /// <param name="events"></param>
    /// <param name="delay"></param>
    /// <param name="trigger"></param>
    /// <param name="dimension"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddAllLightsOff(
        this ICollection<WardenObjectiveEvent> events,
        double delay = 0.0,
        WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart,
        DimensionIndex dimension = DimensionIndex.Reality)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.AllLightsOff,
                Trigger = trigger,
                Delay = delay,
                Dimension = dimension,
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddSetZoneLights(
        this ICollection<WardenObjectiveEvent> events,
        int zoneNumber,
        int layer,
        SetZoneLight setZoneLight,
        double duration,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SetLightDataInZone,
                Trigger = WardenObjectiveEventTrigger.OnStart,
                LocalIndex = zoneNumber,
                Layer = layer,
                Duration = duration,
                Delay = delay,
                SetZoneLight = setZoneLight
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddCyclingLights(
        this ICollection<WardenObjectiveEvent> events,
        int zoneNumber,
        int layer,
        LightSettings[] states,
        int loopIndex,
        double stateDuration = 1.5,
        double transitionDuration = 0.05)
    {
        var eventLoop = new EventLoop()
        {
            LoopIndex = loopIndex,
            LoopDelay = stateDuration * states.Length,
            LoopCount = -1
        };

        for (var i = 0; i < states.Length; i++)
        {
            eventLoop.EventsToActivate.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.SetLightDataInZone,
                    LocalIndex = zoneNumber,
                    Layer = layer,
                    Delay = i * stateDuration,
                    Duration = transitionDuration,
                    SetZoneLight = new SetZoneLight
                    {
                        LightSettings = states[i],
                        Duration = transitionDuration,
                        Seed = i + 1,
                    }
                });
        }

        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.StartEventLoop,
                EventLoop = eventLoop
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddRevertZoneLights(
        this ICollection<WardenObjectiveEvent> events,
        int zoneNumber,
        int layer,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SetLightDataInZone,
                LocalIndex = zoneNumber,
                Layer = layer,
                Delay = delay,
                SetZoneLight = new SetZoneLight
                {
                    SetLight = SetZoneLightType.Revert,
                    Duration = 1.0,
                }
            });

        return events;
    }

    #endregion

    #region Messaging

    public static ICollection<WardenObjectiveEvent> AddCustomHudText(
        this ICollection<WardenObjectiveEvent> events,
        string text,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.CustomHudText,
                Enabled = true,
                Delay = delay,
                CustomHudText = new WardenObjectiveEventCustomHudText
                {
                    Title = text
                }
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> RemoveCustomHudText(
        this ICollection<WardenObjectiveEvent> events,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.CustomHudText,
                Enabled = false,
                Delay = delay,
            });

        return events;
    }

    #endregion

    #region Objectives

    public static ICollection<WardenObjectiveEvent> AddActivateChainedPuzzle(
        this ICollection<WardenObjectiveEvent> events,
        string? filter = null,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.ActivateChainedPuzzle,
                Identifier = filter,
                Delay = delay
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddForceCompleteLevel(
        this ICollection<WardenObjectiveEvent> events,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.ForceCompleteLevel,
                Delay = delay
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddSetNavMarker(
        this ICollection<WardenObjectiveEvent> events,
        string? filter = null,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SetNavMarker,
                Identifier = filter,
                Delay = delay
            });

        return events;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="events"></param>
    /// <param name="header"></param>
    /// <param name="description"></param>
    /// <param name="intel"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddUpdateSubObjective(
        this ICollection<WardenObjectiveEvent> events,
        DataBlocks.Text? header = null,
        DataBlocks.Text? description = null,
        string? intel = null,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.UpdateCustomSubObjective,
                SubObjective = description ?? DataBlocks.Text.None,
                SubObjectiveHeader = header ?? DataBlocks.Text.None,
                WardenIntel = new DataBlocks.Text(intel ?? ""),
                Delay = delay
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddWinOnDeath(
        this ICollection<WardenObjectiveEvent> events,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.WinOnDeath,
                Delay = delay
            });

        return events;
    }

    #endregion

    #region Security Sensors
    /// <summary>
    /// Adds a security sensor toggle event (for EOSExt_SecuritySensor)
    /// </summary>
    /// <param name="events"></param>
    /// <param name="sensorIndex"></param>
    /// <param name="delay"></param>
    /// <param name="enabled"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddToggleSecuritySensors(
        this ICollection<WardenObjectiveEvent> events,
        bool enabled,
        int sensorIndex,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.ToggleSecuritySensor,
                Enabled = enabled,
                Count = sensorIndex,
                Delay = delay
            });

        return events;
    }

    /// <summary>
    /// Toggle a sensor by ID on/off.
    /// </summary>
    public static ICollection<WardenObjectiveEvent> ToggleZoneSensors(
        this ICollection<WardenObjectiveEvent> events,
        int sensorId,
        bool enabled,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.ToggleSecuritySensor,
                Enabled = enabled,
                Count = sensorId,
                Delay = delay
            });

        return events;
    }

    /// <summary>
    /// Enable a sensor by ID. Previously triggered sensors stay hidden.
    /// </summary>
    public static ICollection<WardenObjectiveEvent> EnableZoneSensors(
        this ICollection<WardenObjectiveEvent> events,
        int sensorId,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.EnableSecuritySensor,
                Count = sensorId,
                Delay = delay
            });

        return events;
    }

    /// <summary>
    /// Disable a sensor by ID.
    /// </summary>
    /// <param name="cancelPendingEnable">
    /// When true, any queued Enable toggle for the same sensor id is dropped before this
    /// Disable runs. Use for alarm-end / out-of-band hooks that must defeat an in-flight
    /// cycle re-enable. Default false preserves existing scheduler behavior.
    /// </param>
    public static ICollection<WardenObjectiveEvent> DisableZoneSensors(
        this ICollection<WardenObjectiveEvent> events,
        int sensorId,
        double delay = 0.0,
        bool cancelPendingEnable = false)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = cancelPendingEnable
                    ? WardenObjectiveEventType.DisableSecuritySensorCancelPending
                    : WardenObjectiveEventType.DisableSecuritySensor,
                Count = sensorId,
                Delay = delay
            });

        return events;
    }

    /// <summary>
    /// Enable a sensor by ID with full reset. All sensors reappear,
    /// including previously triggered ones.
    /// </summary>
    public static ICollection<WardenObjectiveEvent> EnableZoneSensorsWithReset(
        this ICollection<WardenObjectiveEvent> events,
        int sensorId,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.ToggleSecuritySensorResetTriggered,
                Enabled = true,
                Count = sensorId,
                Delay = delay
            });

        return events;
    }

    /// <summary>
    /// Toggle a sensor by ID with full reset. When enabling,
    /// all sensors reappear including previously triggered ones.
    /// </summary>
    public static ICollection<WardenObjectiveEvent> ToggleZoneSensorsWithReset(
        this ICollection<WardenObjectiveEvent> events,
        int sensorId,
        bool enabled,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.ToggleSecuritySensorResetTriggered,
                Enabled = enabled,
                Count = sensorId,
                Delay = delay
            });

        return events;
    }

    /// <summary>
    /// Enable sensors in a zone. Previously triggered sensors stay hidden.
    /// </summary>
    public static ICollection<WardenObjectiveEvent> EnableZoneSensorsInZone(
        this ICollection<WardenObjectiveEvent> events,
        ZoneNode zone,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.EnableSecuritySensor,
                Dimension = DimensionIndex.Reality,
                Layer = GetLayerFromBulkhead(zone.Bulkhead),
                LocalIndex = zone.ZoneNumber,
                Count = 0,  // Zone targeting mode
                Delay = delay
            });

        return events;
    }

    /// <summary>
    /// Disable sensors in a zone.
    /// </summary>
    public static ICollection<WardenObjectiveEvent> DisableZoneSensorsInZone(
        this ICollection<WardenObjectiveEvent> events,
        ZoneNode zone,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.DisableSecuritySensor,
                Dimension = DimensionIndex.Reality,
                Layer = GetLayerFromBulkhead(zone.Bulkhead),
                LocalIndex = zone.ZoneNumber,
                Count = 0,  // Zone targeting mode
                Delay = delay
            });

        return events;
    }

    /// <summary>
    /// Enable sensors in a zone with full reset. All sensors reappear,
    /// including previously triggered ones.
    /// </summary>
    public static ICollection<WardenObjectiveEvent> ResetZoneSensorsInZone(
        this ICollection<WardenObjectiveEvent> events,
        ZoneNode zone,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.ToggleSecuritySensorResetTriggered,
                Enabled = true,
                Dimension = DimensionIndex.Reality,
                Layer = GetLayerFromBulkhead(zone.Bulkhead),
                LocalIndex = zone.ZoneNumber,
                Count = 0,  // Zone targeting mode
                Delay = delay
            });

        return events;
    }
    #endregion

    #region Screen

    public static ICollection<WardenObjectiveEvent> AddScreenShake(
        this ICollection<WardenObjectiveEvent> events,
        double duration,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.ShakeScreen,
                Duration = duration,
                Delay = delay,
                CameraShake = new CameraShake
                {
                    Amplitude = 5.0,
                    Radius = 10.0,
                    Frequency = 90.0
                }
            });

        return events;
    }

    #endregion

    #region Infection

    public static ICollection<WardenObjectiveEvent> AddInfectPlayer(
        this ICollection<WardenObjectiveEvent> events,
        double infectionAmount,
        List<int>? playerFilter = null,
        bool useZone = false,
        Bulkhead? bulkhead = null,
        int zoneIndex = 0,
        double delay = 0.0)
    {
        var infectPlayer = new InfectPlayer
        {
            InfectionAmount = infectionAmount,
            UseZone = useZone
        };

        if (playerFilter != null)
            infectPlayer.PlayerFilter = playerFilter;

        var ev = new WardenObjectiveEvent
        {
            Type = WardenObjectiveEventType.InfectPlayer,
            Delay = delay,
            InfectPlayer = infectPlayer
        };

        if (bulkhead != null)
        {
            ev.Layer = GetLayerFromBulkhead(bulkhead.Value);
            ev.LocalIndex = zoneIndex;
        }

        events.Add(ev);

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddInfectPlayerOverTime(
        this ICollection<WardenObjectiveEvent> events,
        double infectionAmount,
        double interval = 1.0,
        List<int>? playerFilter = null,
        bool useZone = false,
        Bulkhead? bulkhead = null,
        int zoneIndex = 0,
        double delay = 0.0)
    {
        var infectPlayer = new InfectPlayer
        {
            InfectionAmount = infectionAmount,
            InfectOverTime = true,
            Interval = interval,
            UseZone = useZone
        };

        if (playerFilter != null)
            infectPlayer.PlayerFilter = playerFilter;

        var ev = new WardenObjectiveEvent
        {
            Type = WardenObjectiveEventType.InfectPlayer,
            Delay = delay,
            InfectPlayer = infectPlayer
        };

        if (bulkhead != null)
        {
            ev.Layer = GetLayerFromBulkhead(bulkhead.Value);
            ev.LocalIndex = zoneIndex;
        }

        events.Add(ev);

        return events;
    }

    #endregion

    #region Sound

    /// <summary>
    ///
    /// </summary>
    /// <param name="events"></param>
    /// <param name="message"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddSound(
        this ICollection<WardenObjectiveEvent> events,
        Sound sound,
        double delay = 0.0,
        WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart,
        uint subtitle = 0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlaySound,
                Trigger = trigger,
                Delay = delay,
                SoundId = sound,
                Subtitle = subtitle
            });

        return events;
    }

    /// <summary>
    /// Queue a sound to play at the center of a target zone. Dispatch is handled by
    /// Patch_ZoneSensorToggle — the game itself ignores this event type, so the sound
    /// is posted directly through SoundPlayer with the zone's CenterPosition as the
    /// 3D emitter.
    /// </summary>
    public static ICollection<WardenObjectiveEvent> AddZoneSound(
        this ICollection<WardenObjectiveEvent> events,
        Sound sound,
        Bulkhead bulkhead,
        int zoneNumber,
        double delay = 0.0,
        DimensionIndex dimension = DimensionIndex.Reality)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.PlayZoneSound,
                Dimension = dimension,
                Layer = GetLayerFromBulkhead(bulkhead),
                LocalIndex = zoneNumber,
                SoundId = sound,
                Delay = delay
            });

        return events;
    }

    /// <summary>
    /// Convenience overload that reads bulkhead/zone/dimension off a ZoneSensorDefinition.
    /// </summary>
    public static ICollection<WardenObjectiveEvent> AddZoneSound(
        this ICollection<WardenObjectiveEvent> events,
        Sound sound,
        ZoneSensorDefinition sensorDef,
        double delay = 0.0)
        => events.AddZoneSound(sound, sensorDef.Bulkhead, sensorDef.ZoneNumber, delay,
                               ParseDimension(sensorDef.DimensionIndex));

    private static DimensionIndex ParseDimension(string? name) => name switch
    {
        "Reality" => DimensionIndex.Reality,
        "Dimension_1" => DimensionIndex.Dimension1,
        "Dimension_2" => DimensionIndex.Dimension2,
        "Dimension_3" => DimensionIndex.Dimension3,
        "Dimension_4" => DimensionIndex.Dimension4,
        "Dimension_5" => DimensionIndex.Dimension5,
        "Dimension_6" => DimensionIndex.Dimension6,
        "Dimension_7" => DimensionIndex.Dimension7,
        "Dimension_8" => DimensionIndex.Dimension8,
        "Dimension_9" => DimensionIndex.Dimension9,
        "Dimension_10" => DimensionIndex.Dimension10,
        _ => DimensionIndex.Reality
    };

    #endregion

    #region Terminals

    /// <summary>
    /// When used with a terminal it will spawn the scan on the terminals static bioscan point
    /// and block the rest of the events until the scan is done
    /// </summary>
    /// <param name="events"></param>
    /// <param name="puzzle"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddScan(
        this ICollection<WardenObjectiveEvent> events,
        ChainedPuzzle puzzle,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.None,
                ChainPuzzle = puzzle.PersistentId,
                Delay = delay,
            });

        return events;
    }

    #endregion

    #region Timers
    /// <summary>
    /// Adjusts the current AWO timer time. Can accept both positive and negative duration
    /// adjustments. The value of `duration` will be added to the current timer. So calling
    /// with -10 will subtract 10s from the timer, calling with +20 will add 20s to the timer.
    /// </summary>
    /// <param name="events">The events to add the timer adjustment to</param>
    /// <param name="duration">
    /// Duration to adjust timer by (positive to add time, negative to remove time)
    /// </param>
    /// <param name="delay">Delay before firing this event. Default = 0.0</param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddAdjustTimer(
        this ICollection<WardenObjectiveEvent> events,
        double duration,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.AdjustAwoTimer,
                Duration = duration,
                Delay = delay
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddCountdown(
        this ICollection<WardenObjectiveEvent> events,
        double duration,
        WardenObjectiveEventCountdown countdown,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.Countdown,
                Delay = delay,
                Duration = duration,
                Countdown = countdown
            });

        return events;
    }

    /// <summary>
    /// Starts a countdown that self-rearms on expiry: each expiry stops any prior
    /// identifier-tagged wave stream, spawns a fresh tagged stream, and starts the next
    /// fallback countdown, to a finite depth. AdjustAwoTimer events extend whichever
    /// countdown in the chain is currently running; a scoped StopEnemyWaves with the same
    /// identifier silences the stream without touching the chain. The innermost expiry
    /// leaves the final stream running with no further countdown.
    ///
    /// AWO only ever runs one countdown, and starting a new one silently drops the old
    /// one's EventsOnDone — which is why the chain must be pre-built here rather than one
    /// countdown restarting itself: each fallback is only started by the previous one's
    /// expiry, when nothing is running. The stop event fires before the spawn at every
    /// level, so at most one tagged stream is ever live.
    /// </summary>
    /// <param name="events">The events to add the countdown to</param>
    /// <param name="duration">Duration of the initial countdown, in seconds</param>
    /// <param name="wave">Wave spawned on each expiry; streams until stopped</param>
    /// <param name="identifier">AWO wave identifier scoping the stream's stop events</param>
    /// <param name="fallbackCount">Number of self-rearming fallback countdowns</param>
    /// <param name="fallbackDuration">Duration of each fallback countdown, in seconds</param>
    /// <param name="titleText">HUD title of the initial countdown</param>
    /// <param name="fallbackTitleText">HUD title of the fallback countdowns</param>
    /// <param name="timerColor">Timer color of the initial countdown</param>
    /// <param name="fallbackTimerColor">Timer color of the fallback countdowns</param>
    /// <param name="expiryMessage">Warden intel shown on expiry. Empty string for none.</param>
    /// <param name="warningMessage">
    /// Warden intel shown at 75%/90% elapsed (90% repeats it in red). Empty string for none;
    /// the warning sound still plays.
    /// </param>
    /// <param name="expirySound">Sound played on each expiry</param>
    /// <param name="warningSound">Sound played at the 75%/90% warnings</param>
    /// <param name="delay">Delay before the initial countdown starts</param>
    public static ICollection<WardenObjectiveEvent> AddCountdownWithExpiryChain(
        this ICollection<WardenObjectiveEvent> events,
        double duration,
        GenericWave wave,
        string identifier,
        int fallbackCount = 8,
        double fallbackDuration = 240.0,
        string titleText = "",
        string fallbackTitleText = "",
        string timerColor = "red",
        string fallbackTimerColor = "#ffaa00",
        string expiryMessage = "",
        string warningMessage = "",
        Sound expirySound = Sound.Alarms_Error_AmbientLoop,
        Sound warningSound = Sound.Alarms_MissingItem,
        double delay = 0.0)
    {
        // Every list and event instance is built fresh per chain level — countdown data
        // serializes per-level and shared instances would alias across levels.
        List<ProgressEvent> BuildWarnings()
        {
            var closing = new List<WardenObjectiveEvent>().AddSound(warningSound).ToList();
            var critical = new List<WardenObjectiveEvent>().AddSound(warningSound).ToList();

            if (warningMessage.Length > 0)
            {
                closing.AddMessage(warningMessage, 0.5);
                critical.AddMessage($"<color=red>{warningMessage}</color>", 0.5);
            }

            return new List<ProgressEvent>
            {
                new() { Progress = 0.75, Events = closing },
                new() { Progress = 0.90, Events = critical }
            };
        }

        List<WardenObjectiveEvent> BuildExpiry(WardenObjectiveEvent? nextCountdown)
        {
            // Stop (0.0s) strictly before spawn (2.0s): kills any still-live prior stream
            // so lapsing several chain levels never stacks streams.
            var expiry = new List<WardenObjectiveEvent>()
                .AddTurnOffAlarms(0.0, identifier)
                .AddSpawnWave(wave, 2.0, identifier)
                .AddSound(expirySound, 2.0)
                .ToList();

            if (expiryMessage.Length > 0)
                expiry.AddMessage(expiryMessage, 0.5);

            if (nextCountdown != null)
                expiry.Add(nextCountdown);

            return expiry;
        }

        // Innermost level: surge with no re-arm — the stream runs out the level.
        var chain = BuildExpiry(null);

        for (var i = 0; i < fallbackCount; i++)
            chain = BuildExpiry(new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.Countdown,
                Delay = 1.0,
                Duration = fallbackDuration,
                Countdown = new WardenObjectiveEventCountdown
                {
                    TitleText = fallbackTitleText,
                    TimerColor = fallbackTimerColor,
                    EventsOnProgress = BuildWarnings(),
                    EventsOnDone = chain
                }
            });

        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.Countdown,
                Delay = delay,
                Duration = duration,
                Countdown = new WardenObjectiveEventCountdown
                {
                    TitleText = titleText,
                    TimerColor = timerColor,
                    EventsOnProgress = BuildWarnings(),
                    EventsOnDone = chain
                }
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddCountup(
        this ICollection<WardenObjectiveEvent> events,
        double duration,
        WardenObjectiveEventCountup countup,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.Countup,
                Delay = delay,
                Duration = duration,
                Countup = countup
            });

        return events;
    }

    /// <summary>
    /// Renders a horizontal progress fill bar at the top of the HUD — the same primitive used
    /// by bioscans and reactor startup waves (GuiManager.InteractionLayer.SetMessageTimer).
    /// The Message string can embed [TIMER] (mm:ss) and [PERCENT] placeholders.
    /// </summary>
    public static ICollection<WardenObjectiveEvent> AddSpecialHudTimer(
        this ICollection<WardenObjectiveEvent> events,
        double duration,
        WardenObjectiveEventSpecialHudTimer hud,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SpecialHudTimer,
                Delay = delay,
                Duration = duration,
                SpecialHudTimer = hud
            });

        return events;
    }

    #endregion

    #region Dimensions
    public static ICollection<WardenObjectiveEvent> AddDimensionWarp(
        this ICollection<WardenObjectiveEvent> events,
        DimensionIndex dimension,
        double delay = 0.0,
        WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.DimensionWarpTeam,
                Trigger = trigger,
                Dimension = dimension,
                Delay = delay
            });

        return events;
    }

    /// <summary>
    /// An event to kill all enemies brutally in a dimension.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="dimension"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddClearDimension(
        this ICollection<WardenObjectiveEvent> events,
        DimensionIndex dimension,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.ClearDimension,
                Dimension = dimension,
                Delay = delay
            });

        return events;
    }
    #endregion

    #region Utilities

    public static ICollection<WardenObjectiveEvent> AddEventBreak(
        this ICollection<WardenObjectiveEvent> events)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.EventBreak
            });

        return events;
    }

    #endregion
}
