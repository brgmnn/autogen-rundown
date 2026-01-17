using System.Collections.Generic;
using GameData;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using SNetwork;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Harmony patch to handle ALL ToggleSecuritySensor (type 400) events.
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

        // Only intercept type 400 (ToggleSecuritySensor)
        if (eventToTrigger.Type != (eWardenObjectiveEventType)400)
            return true;

        // Handle trigger check (same as vanilla)
        if (!ignoreTrigger && eventToTrigger.Trigger != trigger)
            return false;

        // Handle delay check (same as vanilla)
        if (currentDuration != 0f && eventToTrigger.Delay <= currentDuration)
            return false;

        // Schedule the toggle with delay
        float delaySeconds = Mathf.Max(eventToTrigger.Delay - currentDuration, 0f);
        ZoneSensorToggleScheduler.Schedule(eventToTrigger.Count, eventToTrigger.Enabled, delaySeconds);

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
    }

    static ZoneSensorToggleScheduler()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ZoneSensorToggleScheduler>();
    }

    public static void Schedule(int groupIndex, bool enabled, float delaySeconds)
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
            ExecuteTime = Time.time + delaySeconds
        });

        Plugin.Logger.LogDebug($"ZoneSensor: Scheduled toggle for group {groupIndex} to {(enabled ? "enabled" : "disabled")} in {delaySeconds}s");
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

                Plugin.Logger.LogDebug($"ZoneSensor: Toggling group {toggle.GroupIndex} to {(toggle.Enabled ? "enabled" : "disabled")}");
                ZoneSensorManager.Current.ToggleSensorGroup(toggle.GroupIndex, toggle.Enabled);
            }
        }
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
