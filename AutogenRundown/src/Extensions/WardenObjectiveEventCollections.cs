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
                DimensionIndex = 0,
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
