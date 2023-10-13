namespace AutogenRundown.DataBlocks.Objectives
{
    internal class EventBuilder
    {
        /// <summary>
        /// Turns off all the lights in the whole level.
        /// </summary>
        /// <param name="events">Target event list to add the events to</param>
        /// <param name="delay">How many seconds to delay the event by</param>
        /// <param name="message">Warden intel message to display</param>
        public static void AddLightsOff(
            ICollection<WardenObjectiveEvent> events,
            double delay = 2.0,
            string message = ":://CRITICAL FAILURE - SUPPORT SYSTEMS OFFLINE")
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.AllLightsOff,
                    Trigger = WardenObjectiveEventTrigger.OnStart,
                    Delay = delay + 1.0
                });
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.PlaySound,
                    Trigger = WardenObjectiveEventTrigger.OnStart,
                    SoundId = Sound.LightsOff,
                    WardenIntel = message,
                    Delay = delay
                });
        }
    }
}
