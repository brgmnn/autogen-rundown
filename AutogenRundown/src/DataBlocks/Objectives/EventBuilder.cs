using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;

namespace AutogenRundown.DataBlocks.Objectives
{
    public class EventBuilder
    {
        #region Enemies
        /// <summary>
        /// Spawns a generic enemy wave with text and noise. Can be used for almost any wave, but
        /// is great for spawning single boss enemies.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="wave"></param>
        /// <param name="delay"></param>
        /// <param name="message"></param>
        public static void AddSpawnEnemies(
            ICollection<WardenObjectiveEvent> events,
            GenericWave wave,
            double delay = 2.0,
            string message = ":://WARNING - BIOMASS SIGNATURE")
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.SpawnEnemyWave,
                    Delay = delay + 3.0,
                    EnemyWaveData = wave
                });
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.PlaySound,
                    SoundId = Sound.TankRoar,
                    Delay = delay + 3.0
                });
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.None,
                    Delay = delay,
                    WardenIntel = message
                });
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
        public static void AddFillFog(
            ICollection<WardenObjectiveEvent> events,
            double delay = 5.0,
            double duration = 20.0,
            string message = ":://WARNING - VENTILATION SYSTEM OFFLINE")
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.SetFogSettings,
                    Trigger = WardenObjectiveEventTrigger.OnStart,
                    FogSetting = Fog.FullFog.PersistentId,
                    FogTransitionDuration = duration,
                    Delay = delay + 0.7,
                });
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
        }

        /// <summary>
        /// Floods the level with infectious fog. This will add a significant challenge as players
        /// will be unable to see anything as well as maxing out their infection level unless they
        /// have any infection resistance.
        ///
        /// It's strongly advised to include fog repellers or turbines in combination with this
        /// event.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="delay"></param>
        /// <param name="duration"></param>
        /// <param name="message"></param>
        public static void AddFillInfectiousFog(
            ICollection<WardenObjectiveEvent> events,
            double delay = 5.0,
            double duration = 20.0,
            string message = ":://ERROR - CONTAINMENT VENTS OPENED")
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.SetFogSettings,
                    Trigger = WardenObjectiveEventTrigger.OnStart,
                    FogSetting = Fog.FullFog_Infectious.PersistentId,
                    FogTransitionDuration = duration,
                    Delay = delay + 0.7,
                });
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
        }

        /// <summary>
        /// Clears the level from fog, sets to the default fog value.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="delay"></param>
        /// <param name="duration"></param>
        /// <param name="message"></param>
        public static void AddClearFog(
            ICollection<WardenObjectiveEvent> events,
            double delay = 2.0,
            double duration = 20.0,
            string message = "VENTILATION SYSTEM ONLINE")
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.SetFogSettings,
                    Trigger = WardenObjectiveEventTrigger.OnStart,
                    FogSetting = Fog.DefaultFog.PersistentId,
                    FogTransitionDuration = duration,
                    Delay = delay + 0.7,
                });
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
        }
        #endregion

        #region Lights
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
        #endregion
    }
}
