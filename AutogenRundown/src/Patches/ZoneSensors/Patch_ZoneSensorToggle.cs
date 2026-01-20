using System.Collections.Generic;
using GameData;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using SNetwork;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Event types for zone sensor toggle operations.
/// 400: Standard toggle (resets all sensors on enable)
/// 401: Toggle preserving triggered state (only re-enable untriggered sensors)
/// 402: Toggle with reset (clear triggered state, then enable all)
/// 403: Disable sensor group (preserves triggered state)
/// 404: Enable sensor group (only untriggered sensors appear)
/// </summary>
public static class ZoneSensorEventTypes
{
    public const int Toggle = 400;
    public const int TogglePreserveTriggered = 401;
    public const int ToggleResetTriggered = 402;
    public const int Disable = 403;
    public const int Enable = 404;
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

        // Only intercept zone sensor events (400-404)
        if (eventType != ZoneSensorEventTypes.Toggle &&
            eventType != ZoneSensorEventTypes.TogglePreserveTriggered &&
            eventType != ZoneSensorEventTypes.ToggleResetTriggered &&
            eventType != ZoneSensorEventTypes.Disable &&
            eventType != ZoneSensorEventTypes.Enable)
            return true;

        // Handle trigger check (same as vanilla)
        if (!ignoreTrigger && eventToTrigger.Trigger != trigger)
            return false;

        // Handle delay check (same as vanilla)
        if (currentDuration != 0f && eventToTrigger.Delay <= currentDuration)
            return false;

        // Determine flags based on event type
        bool enabled;
        bool preserveTriggered;
        bool resetTriggered = false;

        if (eventType == ZoneSensorEventTypes.Disable)
        {
            enabled = false;
            preserveTriggered = false;  // N/A when disabling
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
        float delaySeconds = Mathf.Max(eventToTrigger.Delay - currentDuration, 0f);
        ZoneSensorToggleScheduler.Schedule(eventToTrigger.Count, enabled, delaySeconds, preserveTriggered, resetTriggered);

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
        public int GroupIndex;
        public bool Enabled;
        public float ExecuteTime;
        public bool PreserveTriggered;
        public bool ResetTriggered;
    }

    static ZoneSensorToggleScheduler()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ZoneSensorToggleScheduler>();
    }

    public static void Schedule(int groupIndex, bool enabled, float delaySeconds, bool preserveTriggered = false, bool resetTriggered = false)
    {
        EnsureInstance();

        if (instance == null)
        {
            Plugin.Logger.LogError("ZoneSensorToggleScheduler: Failed to create instance");
            return;
        }

        instance.pendingToggles.Add(new PendingToggle
        {
            GroupIndex = groupIndex,
            Enabled = enabled,
            ExecuteTime = Time.time + delaySeconds,
            PreserveTriggered = preserveTriggered,
            ResetTriggered = resetTriggered
        });

        Plugin.Logger.LogDebug($"ZoneSensor: Scheduled toggle for group {groupIndex} to {(enabled ? "enabled" : "disabled")} in {delaySeconds}s (preserveTriggered={preserveTriggered}, resetTriggered={resetTriggered})");
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

                Plugin.Logger.LogDebug($"ZoneSensor: Toggling group {toggle.GroupIndex} to {(toggle.Enabled ? "enabled" : "disabled")} (preserveTriggered={toggle.PreserveTriggered}, resetTriggered={toggle.ResetTriggered})");
                ZoneSensorManager.Current.ToggleSensorGroup(toggle.GroupIndex, toggle.Enabled, toggle.PreserveTriggered, toggle.ResetTriggered);
            }
        }
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
