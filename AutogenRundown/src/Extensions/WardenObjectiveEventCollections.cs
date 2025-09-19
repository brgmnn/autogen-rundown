using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;

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

    #region Security Sensors
    /// <summary>
    /// Adds a security sensor toggle event
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
    #endregion
}
