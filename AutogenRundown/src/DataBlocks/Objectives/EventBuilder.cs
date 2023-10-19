namespace AutogenRundown.DataBlocks.Objectives
{
    internal class EventBuilder
    {
        /// <summary>
        /// Floods the level with fog on event start.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="delay"></param>
        /// <param name="duration"></param>
        /// <param name="message"></param>
        public static void AddFillFog(
            ICollection<WardenObjectiveEvent> events,
            double delay = 5.0,
            double duration = 20.0,
            string message = ":://CRITICAL FAILURE - AIR FILTRATION OFFLINE")
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.SetFogSettings,
                    Trigger = WardenObjectiveEventTrigger.OnStart,
                    FogSetting = Fog.FullFog.PersistentId,
                    FogTransitionDuration = duration,
                    Delay = 0.0,
                });
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.PlaySound,
                    Trigger = WardenObjectiveEventTrigger.OnStart,
                    SoundId = Sound.DistantHeavyFan,
                    Delay = 2.0
                });
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.None,
                    Trigger = WardenObjectiveEventTrigger.OnStart,
                    Delay = delay,
                    WardenIntel = message
                });
        }

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
