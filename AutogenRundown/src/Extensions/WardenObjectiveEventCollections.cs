using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
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
                WardenIntel = alertMessage ?? string.Empty
            });

        return events;
    }

    /// <summary>
    /// Turn's off alarms. Optionally with a given identifier
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
                WardenIntel = message
            });

        return events;
    }

    public static ICollection<WardenObjectiveEvent> AddCyclingFog(
        this ICollection<WardenObjectiveEvent> events,
        Fog fogSettings1,
        Fog fogSettings2,
        int loopIndex = 1,
        double delay = 30.0,
        double duration = 45.0)
    {
        var eventLoop = new EventLoop()
        {
            LoopIndex = loopIndex,
            LoopDelay = delay,
            LoopCount = -1
        };

        eventLoop.EventsToActivate.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SetFogSettings,
                Dimension = DimensionIndex.Reality, // TODO: support dimensions
                Trigger = WardenObjectiveEventTrigger.OnStart,
                FogSetting = fogSettings1.PersistentId,
                FogTransitionDuration = duration,
                SoundId = (Sound)2275333205,
                Delay = 0
            });
        eventLoop.EventsToActivate.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.SetFogSettings,
                Dimension = DimensionIndex.Reality, // TODO: support dimensions
                Trigger = WardenObjectiveEventTrigger.OnStart,
                FogSetting = fogSettings2.PersistentId,
                FogTransitionDuration = duration,
                SoundId = (Sound)2275333205,
                Delay = duration + delay
            });

        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.StartEventLoop,
                EventLoop = eventLoop
            });

        return events;
    }

    #endregion

    #region Lights
    /// <summary>
    /// Add all lights off. Turns off all lights in all zones
    /// </summary>
    /// <param name="events"></param>
    /// <param name="delay"></param>
    /// <param name="trigger"></param>
    /// <returns></returns>
    public static ICollection<WardenObjectiveEvent> AddAllLightsOff(
        this ICollection<WardenObjectiveEvent> events,
        double delay = 0.0,
        WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.AllLightsOff,
                Trigger = trigger,
                Delay = delay
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
                WardenIntel = intel ?? "",
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
    public static ICollection<WardenObjectiveEvent> DisableZoneSensors(
        this ICollection<WardenObjectiveEvent> events,
        int sensorId,
        double delay = 0.0)
    {
        events.Add(
            new WardenObjectiveEvent
            {
                Type = WardenObjectiveEventType.DisableSecuritySensor,
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

    #endregion
}
