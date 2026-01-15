using AIGraph;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using SNetwork;

namespace AutogenRundown.Patches;

/// <summary>
/// Patches terminal uplink wave behavior so that completing an uplink only stops
/// the uplink-specific waves, not all active warden objective waves.
///
/// The vanilla game calls WardenObjectiveManager.StopAllWardenObjectiveEnemyWaves()
/// when an uplink completes, which stops ALL tracked waves. This patch intercepts
/// that call and only stops waves that were spawned by the uplink itself.
///
/// IMPORTANT: Uses NodeID (int) instead of AIG_CourseNode object references because
/// IL2CPP creates different C# wrapper instances for the same game object, so
/// reference equality (HashSet.Contains) fails even for the same node.
/// </summary>
[HarmonyPatch]
public static class Patch_UplinkWaveIsolation
{
    // Active uplink node IDs - set when uplink connects, cleared when uplink completes
    private static readonly HashSet<int> ActiveUplinkNodeIds = new();

    // Map node ID -> set of wave event IDs for that uplink
    private static readonly Dictionary<int, HashSet<ushort>> UplinkWaveIds = new();

    // Context tracking for detecting uplink verify callback
    private static LG_ComputerTerminalCommandInterpreter? CurrentInterpreter = null;

    internal static void Setup()
    {
        LevelAPI.OnLevelCleanup += () =>
        {
            ActiveUplinkNodeIds.Clear();
            UplinkWaveIds.Clear();
            CurrentInterpreter = null;
        };
    }

    /// <summary>
    /// Postfix patch on StartTerminalUplinkSequence to record the uplink's spawn node ID.
    /// This marks which terminal is starting an uplink so we can track its wave IDs.
    /// </summary>
    [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.StartTerminalUplinkSequence))]
    [HarmonyPostfix]
    public static void StartTerminalUplinkSequence_Postfix(
        LG_ComputerTerminalCommandInterpreter __instance,
        bool corrupted)
    {
        var targetTerminal = corrupted
            ? __instance.m_terminal.CorruptedUplinkReceiver
            : __instance.m_terminal;

        if (targetTerminal?.SpawnNode == null)
            return;

        var nodeId = targetTerminal.SpawnNode.NodeID;

        ActiveUplinkNodeIds.Add(nodeId);
        UplinkWaveIds[nodeId] = new HashSet<ushort>();

        Plugin.Logger.LogDebug($"[UplinkWaveIsolation] Tracking uplink node ID: {nodeId} (zone {targetTerminal.SpawnNode.m_zone?.LocalIndex})");
    }

    /// <summary>
    /// Postfix patch on Mastermind.TriggerSurvivalWave to capture wave IDs spawned for uplinks.
    /// When a wave spawns with a spawn node matching an active uplink, we record its event ID.
    /// Targets the overload that calls RegisterSurvivalWaveID (the one without Vector3 position parameter).
    /// </summary>
    [HarmonyPatch]
    public static class Patch_TriggerSurvivalWave
    {
        [HarmonyTargetMethod]
        public static System.Reflection.MethodBase TargetMethod()
        {
            // Target the first overload (9 parameters) that calls RegisterSurvivalWaveID
            return AccessTools.Method(typeof(Mastermind), nameof(Mastermind.TriggerSurvivalWave),
                new Type[] {
                    typeof(AIG_CourseNode), typeof(uint), typeof(uint), typeof(ushort).MakeByRefType(),
                    typeof(SurvivalWaveSpawnType), typeof(float), typeof(bool), typeof(bool), typeof(UnityEngine.Vector3)
                });
        }

        [HarmonyPostfix]
        public static void Postfix(AIG_CourseNode refNode, ushort eventID, bool __result)
        {
            if (!__result || refNode == null)
                return;

            var nodeId = refNode.NodeID;

            if (ActiveUplinkNodeIds.Contains(nodeId) && UplinkWaveIds.TryGetValue(nodeId, out var waveIds))
            {
                waveIds.Add(eventID);
                Plugin.Logger.LogDebug($"[UplinkWaveIsolation] Captured uplink wave ID: {eventID} for node ID {nodeId}");
            }
        }
    }

    /// <summary>
    /// Prefix patch on FireEndOfQueuCallbacks to set context before callbacks run.
    /// </summary>
    [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.FireEndOfQueuCallbacks))]
    [HarmonyPrefix]
    public static void FireEndOfQueuCallbacks_Prefix(LG_ComputerTerminalCommandInterpreter __instance)
    {
        CurrentInterpreter = __instance;
    }

    /// <summary>
    /// Postfix patch on FireEndOfQueuCallbacks to clear context after callbacks run.
    /// </summary>
    [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.FireEndOfQueuCallbacks))]
    [HarmonyPostfix]
    public static void FireEndOfQueuCallbacks_Postfix()
    {
        CurrentInterpreter = null;
    }

    /// <summary>
    /// Prefix patch on StopAllWardenObjectiveEnemyWaves to intercept the global stop
    /// and replace it with targeted uplink-only stop when called from uplink completion.
    /// </summary>
    [HarmonyPatch(typeof(WardenObjectiveManager), nameof(WardenObjectiveManager.StopAllWardenObjectiveEnemyWaves))]
    [HarmonyPrefix]
    public static bool StopAllWardenObjectiveEnemyWaves_Prefix()
    {
        // Only intercept when called from a terminal callback context
        if (CurrentInterpreter == null)
            return true; // Let original run

        var terminal = CurrentInterpreter.m_terminal;
        if (terminal?.SpawnNode == null)
            return true;

        var nodeId = terminal.SpawnNode.NodeID;

        // Check if this terminal has uplink waves to stop
        if (!UplinkWaveIds.TryGetValue(nodeId, out var waveIds) || waveIds.Count == 0)
            return true; // No uplink waves tracked, let original run

        Plugin.Logger.LogDebug($"[UplinkWaveIsolation] Intercepting StopAll - stopping {waveIds.Count} uplink waves only for node ID {nodeId}");

        // Play the alarm stop sound to match vanilla behavior
        WardenObjectiveManager.Current?.m_sound?.Post(AK.EVENTS.APEX_PUZZLE_STOP_ALARM);

        // Only run on master
        if (!SNet.IsMaster)
        {
            // Still clean up tracking even on clients
            UplinkWaveIds.Remove(nodeId);
            ActiveUplinkNodeIds.Remove(nodeId);
            return false;
        }

        // Get reference to the global wave ID list via reflection
        var enemyWaveEventIDs = GetEnemyWaveEventIDs();

        // Stop only the uplink waves
        foreach (var waveId in waveIds)
        {
            if (Mastermind.Current != null && Mastermind.Current.TryGetEvent(waveId, out var masterMindEvent))
            {
                masterMindEvent.StopEvent();
                Plugin.Logger.LogDebug($"[UplinkWaveIsolation] Stopped wave {waveId}");
            }

            // Remove from global tracking list
            enemyWaveEventIDs?.Remove(waveId);
        }

        // Cleanup our tracking
        UplinkWaveIds.Remove(nodeId);
        ActiveUplinkNodeIds.Remove(nodeId);

        return false; // Skip original method
    }

    private static List<ushort>? _cachedEnemyWaveEventIDs = null;
    private static System.Reflection.FieldInfo? _enemyWaveEventIDsField = null;

    private static List<ushort>? GetEnemyWaveEventIDs()
    {
        if (WardenObjectiveManager.Current == null)
            return null;

        // Try to use cached reference if WOM instance is the same
        if (_cachedEnemyWaveEventIDs != null)
            return _cachedEnemyWaveEventIDs;

        // Get field via reflection
        _enemyWaveEventIDsField ??= typeof(WardenObjectiveManager)
            .GetField("m_enemyWaveEventIDs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (_enemyWaveEventIDsField == null)
        {
            Plugin.Logger.LogWarning("[UplinkWaveIsolation] Could not find m_enemyWaveEventIDs field");
            return null;
        }

        _cachedEnemyWaveEventIDs = _enemyWaveEventIDsField.GetValue(WardenObjectiveManager.Current) as List<ushort>;
        return _cachedEnemyWaveEventIDs;
    }
}
