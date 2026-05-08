using AutogenRundown.DataBlocks;
using AutogenRundown.Utils;
using GameData;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;
using SNetwork;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Event types the mod intercepts in the 400-series warden-event range.
/// 400-404: Sensor toggle variants (see ZoneSensorManager).
/// 405:     Generic — play a Wwise sound at the center of a target zone.
/// 406:     Disable + drop pending Enable toggles for the same id.
/// </summary>
public static class ZoneSensorEventTypes
{
    public const int Toggle = 400;
    public const int TogglePreserveTriggered = 401;
    public const int ToggleResetTriggered = 402;
    public const int Disable = 403;
    public const int Enable = 404;
    public const int PlayZoneSound = 405;
    public const int DisableCancelPending = 406;
}

/// <summary>
/// Harmony patch to handle zone sensor toggle events (types 400-404).
/// This overrides EOSExt_SecuritySensor's handling entirely.
/// </summary>
[HarmonyPatch]
public static class Patch_ZoneSensorToggle
{
    [HarmonyPatch(typeof(WardenObjectiveManager),
        nameof(WardenObjectiveManager.CheckAndExecuteEventsOnTrigger),
        new[] { typeof(WardenObjectiveEventData), typeof(eWardenObjectiveEventTrigger), typeof(bool), typeof(float) })]
    [HarmonyPrefix]
    public static bool CheckAndExecuteEventsOnTrigger_Prefix(
        WardenObjectiveEventData eventToTrigger,
        eWardenObjectiveEventTrigger trigger,
        bool ignoreTrigger,
        float currentDuration)
    {
        if (eventToTrigger == null)
            return true;

        var eventType = (int)eventToTrigger.Type;

        // Only intercept mod-owned 400-series events
        if (eventType != ZoneSensorEventTypes.Toggle &&
            eventType != ZoneSensorEventTypes.TogglePreserveTriggered &&
            eventType != ZoneSensorEventTypes.ToggleResetTriggered &&
            eventType != ZoneSensorEventTypes.Disable &&
            eventType != ZoneSensorEventTypes.Enable &&
            eventType != ZoneSensorEventTypes.PlayZoneSound &&
            eventType != ZoneSensorEventTypes.DisableCancelPending)
            return true;

        // Handle trigger check (same as vanilla)
        if (!ignoreTrigger && eventToTrigger.Trigger != trigger)
            return false;

        // Handle delay check (same as vanilla)
        if (currentDuration != 0f && eventToTrigger.Delay <= currentDuration)
            return false;

        // PlayZoneSound: resolve the target zone, post the sound at its center via SoundPlayer.
        // CellSound.PostDelayed handles the delay natively, so no custom scheduler is needed.
        if (eventType == ZoneSensorEventTypes.PlayZoneSound)
        {
            var soundDelay = Mathf.Max(eventToTrigger.Delay - currentDuration, 0f);

            if (Builder.CurrentFloor != null &&
                Builder.CurrentFloor.TryGetZoneByLocalIndex(
                    eventToTrigger.DimensionIndex,
                    eventToTrigger.Layer,
                    eventToTrigger.LocalIndex,
                    out var soundZone) &&
                soundZone != null)
            {
                SoundPlayer.PlayDelayed((Sound)eventToTrigger.SoundID, soundZone.CenterPosition, soundDelay);
            }
            else
            {
                Plugin.Logger.LogWarning(
                    $"PlayZoneSound: could not resolve zone (dim={eventToTrigger.DimensionIndex}, " +
                    $"layer={eventToTrigger.Layer}, zone={eventToTrigger.LocalIndex})");
            }

            return false;
        }

        // Determine flags based on event type
        bool enabled;
        bool preserveTriggered;
        var resetTriggered = false;
        var cancelPendingEnable = false;

        if (eventType == ZoneSensorEventTypes.Disable)
        {
            enabled = false;
            preserveTriggered = false;  // N/A when disabling
        }
        else if (eventType == ZoneSensorEventTypes.DisableCancelPending)
        {
            enabled = false;
            preserveTriggered = false;
            cancelPendingEnable = true;
        }
        else if (eventType == ZoneSensorEventTypes.Enable)
        {
            enabled = true;
            preserveTriggered = true;   // Keep triggered sensors hidden
        }
        else
        {
            // Existing toggle events use Enabled field
            enabled = eventToTrigger.Enabled;
            preserveTriggered = eventType == ZoneSensorEventTypes.TogglePreserveTriggered;
            resetTriggered = eventType == ZoneSensorEventTypes.ToggleResetTriggered;
        }

        // Schedule the toggle with delay
        var delaySeconds = Mathf.Max(eventToTrigger.Delay - currentDuration, 0f);

        // Determine targeting mode based on Count field
        if (eventToTrigger.Count > 0)
        {
            // ID targeting: Count IS the definition ID (direct targeting)
            ZoneSensorToggleScheduler.Schedule(eventToTrigger.Count, enabled, delaySeconds, preserveTriggered, resetTriggered, cancelPendingEnable);
        }
        else
        {
            // Zone targeting: use LocalIndex and Layer
            var dimension = eventToTrigger.DimensionIndex;
            var layer = eventToTrigger.Layer;
            var zoneIndex = eventToTrigger.LocalIndex;

            var definitionIds = ZoneSensorManager.Current.GetIdsForZone(dimension, layer, zoneIndex);
            if (definitionIds.Count == 0)
            {
                Plugin.Logger.LogWarning($"ZoneSensor: No sensor groups found in zone {zoneIndex}, layer {layer}");
            }
            foreach (var definitionId in definitionIds)
            {
                ZoneSensorToggleScheduler.Schedule(definitionId, enabled, delaySeconds, preserveTriggered, resetTriggered, cancelPendingEnable);
            }
        }

        return false; // Skip original and any other patches
    }
}

/// <summary>
/// MonoBehaviour that handles delayed execution of sensor toggles.
/// Uses Update() with a timer instead of coroutines for IL2CPP compatibility.
/// </summary>
public class ZoneSensorToggleScheduler : MonoBehaviour
{
    private static ZoneSensorToggleScheduler? instance;
    private readonly List<PendingToggle> pendingToggles = new();

    private struct PendingToggle
    {
        public int Id;
        public bool Enabled;
        public float ExecuteTime;
        public bool PreserveTriggered;
        public bool ResetTriggered;
    }

    static ZoneSensorToggleScheduler()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ZoneSensorToggleScheduler>();
    }

    public static void Schedule(int definitionId, bool enabled, float delaySeconds, bool preserveTriggered = false, bool resetTriggered = false, bool cancelPendingEnable = false)
    {
        EnsureInstance();

        if (instance == null)
        {
            Plugin.Logger.LogError("ZoneSensorToggleScheduler: Failed to create instance");
            return;
        }

        // Opt-in: a Disable that should override an in-flight cycle re-enable drops any
        // queued Enable for the same id before being added itself.
        if (cancelPendingEnable)
        {
            var removed = instance.pendingToggles.RemoveAll(t => t.Id == definitionId && t.Enabled);
            if (removed > 0)
                Plugin.Logger.LogDebug($"ZoneSensor: Dropped {removed} pending Enable(s) for group {definitionId} on cancel-pending Disable");
        }

        instance.pendingToggles.Add(new PendingToggle
        {
            Id = definitionId,
            Enabled = enabled,
            ExecuteTime = Time.time + delaySeconds,
            PreserveTriggered = preserveTriggered,
            ResetTriggered = resetTriggered
        });

        Plugin.Logger.LogDebug($"ZoneSensor: Scheduled toggle for group {definitionId} to {(enabled ? "enabled" : "disabled")} in {delaySeconds}s (preserveTriggered={preserveTriggered}, resetTriggered={resetTriggered})");
    }

    /// <summary>
    /// Clears all pending toggles. Called during level cleanup to prevent
    /// stale toggles from persisting across level transitions.
    /// </summary>
    public static void ClearPending()
    {
        instance?.pendingToggles.Clear();
    }

    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        var go = new GameObject("ZoneSensorToggleScheduler");

        DontDestroyOnLoad(go);

        instance = go.AddComponent<ZoneSensorToggleScheduler>();
    }

    void Update()
    {
        if (pendingToggles.Count == 0)
            return;

        var currentTime = Time.time;

        for (var i = pendingToggles.Count - 1; i >= 0; i--)
        {
            var toggle = pendingToggles[i];

            if (currentTime >= toggle.ExecuteTime)
            {
                pendingToggles.RemoveAt(i);

                if (!SNet.IsMaster)
                    continue;

                Plugin.Logger.LogDebug($"ZoneSensor: Toggling group {toggle.Id} to {(toggle.Enabled ? "enabled" : "disabled")} (preserveTriggered={toggle.PreserveTriggered}, resetTriggered={toggle.ResetTriggered})");
                ZoneSensorManager.Current.ToggleSensorGroup(toggle.Id, toggle.Enabled, toggle.PreserveTriggered, toggle.ResetTriggered);
            }
        }
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
