using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.Extensions;

public static class WardenObjectiveEventCollections
{
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
