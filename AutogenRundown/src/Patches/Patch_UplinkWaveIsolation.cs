using System.Reflection;
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
///
/// NOTE: We can't patch FireEndOfQueuCallbacks because IL2CPP invokes the delegate
/// directly without going through the patchable C# method wrapper. Instead, we detect
/// uplink completion by monitoring puzzle state changes in TerminalUplinkVerify.
/// </summary>
[HarmonyPatch]
public static class Patch_UplinkWaveIsolation
{
    // Active uplink node IDs - set when uplink connects, cleared when uplink completes
    private static readonly HashSet<int> ActiveUplinkNodeIds = new();

    // Map node ID -> set of wave event IDs for that uplink
    private static readonly Dictionary<int, HashSet<ushort>> UplinkWaveIds = new();

    // Flag: Should we intercept the next StopAllWardenObjectiveEnemyWaves call?
    private static bool InterceptNextStopAll = false;

    // Which node's waves should be stopped when intercepting?
    private static int? CompletingUplinkNodeId = null;

    internal static void Setup()
    {
        LevelAPI.OnLevelCleanup += () =>
        {
            ActiveUplinkNodeIds.Clear();
            UplinkWaveIds.Clear();
            InterceptNextStopAll = false;
            CompletingUplinkNodeId = null;
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
    /// Nested patch class for Mastermind.TriggerSurvivalWave using HarmonyTargetMethods
    /// to patch ALL overloads. This avoids the "Ambiguous match" error.
    /// </summary>
    [HarmonyPatch]
    public static class Patch_TriggerSurvivalWave
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(Mastermind).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == nameof(Mastermind.TriggerSurvivalWave))
                .Cast<MethodBase>();
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
    /// Prefix patch on TerminalUplinkVerify to capture pre-solve puzzle state.
    /// We use __state to pass whether the puzzle was already solved BEFORE this method runs.
    /// </summary>
    [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.TerminalUplinkVerify))]
    [HarmonyPrefix]
    public static void TerminalUplinkVerify_Prefix(
        LG_ComputerTerminalCommandInterpreter __instance,
        out bool __state)
    {
        // Record if puzzle was already solved BEFORE this method runs
        __state = __instance.m_terminal?.UplinkPuzzle?.Solved ?? true;
    }

    /// <summary>
    /// Postfix patch on TerminalUplinkVerify to detect when uplink completes.
    /// If puzzle wasn't solved before but is now, we set the intercept flag.
    /// </summary>
    [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.TerminalUplinkVerify))]
    [HarmonyPostfix]
    public static void TerminalUplinkVerify_Postfix(
        LG_ComputerTerminalCommandInterpreter __instance,
        bool __state)
    {
        // __state = was puzzle solved BEFORE the method ran?
        var puzzle = __instance.m_terminal?.UplinkPuzzle;
        if (puzzle == null)
            return;

        // If puzzle wasn't solved before but IS solved now, uplink just completed
        if (!__state && puzzle.Solved)
        {
            var nodeId = __instance.m_terminal?.SpawnNode?.NodeID;
            if (nodeId.HasValue && UplinkWaveIds.ContainsKey(nodeId.Value))
            {
                InterceptNextStopAll = true;
                CompletingUplinkNodeId = nodeId.Value;
                Plugin.Logger.LogDebug($"[UplinkWaveIsolation] Uplink completed! Setting intercept flag for node {nodeId}");
            }
        }
    }

    /// <summary>
    /// Prefix patch on StopAllWardenObjectiveEnemyWaves to intercept the global stop
    /// and replace it with targeted uplink-only stop when called from uplink completion.
    /// </summary>
    [HarmonyPatch(typeof(WardenObjectiveManager), nameof(WardenObjectiveManager.StopAllWardenObjectiveEnemyWaves))]
    [HarmonyPrefix]
    public static bool StopAllWardenObjectiveEnemyWaves_Prefix()
    {
        // Check if we should intercept this call
        if (!InterceptNextStopAll || !CompletingUplinkNodeId.HasValue)
        {
            return true; // Let original run
        }

        var nodeId = CompletingUplinkNodeId.Value;

        // Clear flags immediately to avoid intercepting unrelated calls
        InterceptNextStopAll = false;
        CompletingUplinkNodeId = null;

        if (!UplinkWaveIds.TryGetValue(nodeId, out var waveIds) || waveIds.Count == 0)
        {
            Plugin.Logger.LogDebug($"[UplinkWaveIsolation] Intercept flag set but no tracked waves for nodeId {nodeId}");
            return true; // No waves to stop, let original run
        }

        Plugin.Logger.LogDebug($"[UplinkWaveIsolation] Intercepting StopAll - stopping {waveIds.Count} uplink waves only for node ID {nodeId}");

        // Play the alarm stop sound to match vanilla behavior
        WardenObjectiveManager.Current?.m_sound?.Post(AK.EVENTS.APEX_PUZZLE_STOP_ALARM);

        // Only run wave stopping on master
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
