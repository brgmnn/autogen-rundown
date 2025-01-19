using AutogenRundown.DataBlocks.Enemies;

namespace AutogenRundown.DataBlocks.Objectives
{
    public static class EventCollectionExtensions
    {
        #region Doors
        /// <summary>
        ///
        /// </summary>
        /// <param name="events"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static ICollection<WardenObjectiveEvent> AddUnlockDoor(
            this ICollection<WardenObjectiveEvent> events,
            Bulkhead bulkhead,
            int zoneIndex,
            string? message = null,
            WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart,
            double delay = 0.0)
        {
            EventBuilder.AddUnlockDoor(events, bulkhead, zoneIndex, message, trigger, delay);

            return events;
        }
        #endregion

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
            double delay = 0.0)
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.SpawnEnemyWave,
                    Delay = delay,
                    SoundId = Sound.Enemies_DistantLowRoar,
                    EnemyWaveData = wave
                });

            return events;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="events"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static ICollection<WardenObjectiveEvent> AddTurnOffAlarms(
            this ICollection<WardenObjectiveEvent> events,
            double delay = 0.0)
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.StopEnemyWaves,
                    Delay = delay
                });

            return events;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="events"></param>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static ICollection<WardenObjectiveEvent> AddMessage(
            this ICollection<WardenObjectiveEvent> events,
            string message,
            double delay = 0.0)
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.None,
                    Delay = delay,
                    WardenIntel = message
                });

            return events;
        }

        public static ICollection<WardenObjectiveEvent> AddLightsOff(
            this ICollection<WardenObjectiveEvent> events,
            double delay = 0.0,
            string message = ":://CRITICAL FAILURE - SUPPORT SYSTEMS OFFLINE")
        {
            EventBuilder.AddLightsOff(events, delay, message);

            return events;
        }

        public static ICollection<WardenObjectiveEvent> AddLightsOn(
            this ICollection<WardenObjectiveEvent> events,
            double delay = 0.0,
            string message = "AUXILIARY ELECTRICAL SYSTEM ONLINE")
        {
            EventBuilder.AddLightsOn(events, delay, message);

            return events;
        }

        public static ICollection<WardenObjectiveEvent> AddFillFog(
            this ICollection<WardenObjectiveEvent> events,
            double delay = 5.0,
            double duration = 20.0,
            string? message = ":://CRITICAL FAILURE - VENTILATION SYSTEMS OFFLINE")
        {
            EventBuilder.AddFillFog(events, delay, duration, message);

            return events;
        }
    }

    public class EventBuilder
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
        /// <param name="zoneIndex"></param>
        /// <param name="message"></param>
        /// <param name="trigger"></param>
        /// <param name="delay"></param>
        public static void AddOpenDoor(
            ICollection<WardenObjectiveEvent> events,
            Bulkhead bulkhead,
            int zoneIndex,
            string? message = null,
            WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart,
            double delay = 0.0)
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.OpenSecurityDoor,
                    LocalIndex = zoneIndex,
                    Layer = GetLayerFromBulkhead(bulkhead),
                    Delay = message != null ? delay + 1.0 : delay,
                    Trigger = trigger,
                });

            if (message != null)
            {
                events.Add(
                    new WardenObjectiveEvent
                    {
                        Type = WardenObjectiveEventType.None,
                        Delay = delay,
                        Trigger = trigger,
                        WardenIntel = message
                    });
            }
        }

        /// <summary>
        /// Adds events to unlock a door. This will also add a warden intel message to the event
        /// if passed in.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="zoneIndex"></param>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        public static void AddUnlockDoor(
            ICollection<WardenObjectiveEvent> events,
            Bulkhead bulkhead,
            int zoneIndex,
            string? message = null,
            WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart,
            double delay = 0.0)
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.UnlockSecurityDoor,
                    LocalIndex = zoneIndex,
                    Layer = GetLayerFromBulkhead(bulkhead),
                    Delay = message != null ? delay + 1.0 : delay,
                    Trigger = trigger,
                });

            if (message != null)
                AddMessage(events, message, delay, trigger);
        }
        #endregion

        #region Enemies
        /// <summary>
        /// Spawns a generic enemy wave with text and noise. Can be used for almost any wave, but
        /// is great for spawning single boss enemies.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="wave"></param>
        /// <param name="delay"></param>
        /// <param name="message"></param>
        public static ICollection<WardenObjectiveEvent> AddSpawnEnemies(
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

            return events;
        }

        /// <summary>
        /// This adds a scripted error alarm which isn't actually an alarm but instead a long list
        /// of events that trigger periodically. R7D1 snatcher alarm does this with 1 snatcher
        /// every 4 minutes. The error alarm isn't infinite, it runs out after 1hr 26mins (19 waves).
        ///
        /// Results of this are:
        ///     - No combat music
        ///     - Players get out of combat stamina between waves
        ///     - "Alarm" _cannot_ be deactivated by DEACTIVATE_ALARMS
        ///
        /// See link for more details:
        /// https://gtfo.fandom.com/wiki/R7D1#Trivia
        /// </summary>
        /// <param name="events"></param>
        /// <param name="wave"></param>
        /// <param name="delay"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ICollection<WardenObjectiveEvent> AddScriptedErrorAlarm(
            ICollection<WardenObjectiveEvent> events,
            GenericWave wave,
            double delay = 2.0,
            string message = ":://WARNING - BIOMASS SIGNATURE")
        {
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
        public static void AddFillFog(
            ICollection<WardenObjectiveEvent> events,
            double delay = 5.0,
            double duration = 20.0,
            string? message = ":://WARNING - VENTILATION SYSTEM OFFLINE")
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

            // Don't play the sound or show the message if message is null.
            if (message == null) return;

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

        /// <summary>
        /// Turns on all the lights in the whole level.
        /// </summary>
        /// <param name="events">Target event list to add the events to</param>
        /// <param name="delay">How many seconds to delay the event by</param>
        /// <param name="message">Warden intel message to display</param>
        public static void AddLightsOn(
            ICollection<WardenObjectiveEvent> events,
            double delay = 2.0,
            string message = "ELECTRICAL SYSTEMS RESTARTED")
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.AllLightsOn,
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

        #region Messages
        /// <summary>
        /// Displays a warden intel message
        /// </summary>
        /// <param name="events"></param>
        /// <param name="trigger"></param>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        public static void AddMessage(
            ICollection<WardenObjectiveEvent> events,
            string message,
            double delay = 0.0,
            WardenObjectiveEventTrigger trigger = WardenObjectiveEventTrigger.OnStart)
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.None,
                    Delay = delay,
                    Trigger = trigger,
                    WardenIntel = message
                });
        }
        #endregion

        #region Objectives
        public static void SetSurvivalTimer(
            ICollection<WardenObjectiveEvent> events,
            double duration,
            string message = "",
            double delay = 0.0)
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.AddToTimer,
                    Duration = duration,
                    Delay = delay,
                    Trigger = WardenObjectiveEventTrigger.OnEnd,
                    WardenIntel = message
                });
        }
        #endregion

        #region Sounds
        /// <summary>
        /// Displays a warden intel message
        /// </summary>
        /// <param name="events"></param>
        /// <param name="trigger"></param>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /*public static void AddSound(
            ICollection<WardenObjectiveEvent> events,
            WardenObjectiveEventTrigger trigger,
            double delay = 0.0)
        {
            events.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.PlaySound,
                    Trigger = WardenObjectiveEventTrigger.OnStart,
                    SoundId = Sound.LightsOff,
                    WardenIntel = message,
                    Delay = delay
                });
        }*/
        #endregion
    }
}
