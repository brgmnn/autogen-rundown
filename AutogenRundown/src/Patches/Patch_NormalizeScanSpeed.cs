using AutogenRundown.DataBlocks.Alarms;
using ChainedPuzzles;
using GameData;
using HarmonyLib;

namespace AutogenRundown.Patches;

/// <summary>
/// Forces whole-team (PlayerRequirement.All) scans to always progress at the
/// 4-player (full-team) speed, regardless of how many players are in the circle.
/// These scans require the whole team to progress anyway, so solo/small teams
/// are not penalized with much longer durations. m_playerRequirement is left
/// untouched, so the whole-team requirement is preserved.
///
/// Mechanism: the game stores a per-player-count speed curve in
/// CP_PlayerScanner.m_scanSpeeds (float[4], indexed by playersInCircle-1). We
/// flatten every entry to the 4-player slot so any occupant count scans at the
/// full-team rate.
/// </summary>
internal static class ScanSpeedNormalize
{
    /// <summary>
    /// ChainedPuzzle component PuzzleType pids that always scan at 4-player speed.
    /// These are the confirmed whole-team (PlayerRequirement.All) scans autogen
    /// generates, verified against each pid's prefab in the game data dump
    /// (the "RequireAll" prefabs). Solo-capable moving scans (42/52/60) and the
    /// ambiguous 43 (CP_Bioscan_Big_Moving_R8B4_v2, not RequireAll) / 37 geo-scan
    /// are deliberately excluded. Extend this set to cover additional scan types.
    /// </summary>
    internal static readonly HashSet<uint> FullSpeedScanTypes = new()
    {
        // Full team scans (RequireAll)
        (uint)PuzzleType.AllBig,                 // 6
        (uint)PuzzleType.AllLarge,               // 8
        (uint)PuzzleType.AllLarge_Slow,          // 20
        (uint)PuzzleType.AllBig_BlueActive,      // 25
        (uint)PuzzleType.AllBig_GreenActive,     // 29

        // Sustained S-class scans (RequireAll)
        (uint)PuzzleType.Sustained,              // 13
        (uint)PuzzleType.SustainedSmall,         // 14
        (uint)PuzzleType.SustainedBig_Cluster,   // 16
        (uint)PuzzleType.SustainedMegaHuge,      // 17
        (uint)PuzzleType.SustainedHuge,          // 18
        (uint)PuzzleType.SustainedMedium,        // 32

        // Travel team scans (RequireAll, moving)
        (uint)PuzzleType.TravelTeam_LongGreen,   // 21
        (uint)PuzzleType.TravelTeam_Long,        // 22
        (uint)PuzzleType.TravelTeam_MediumGreen, // 24
        (uint)PuzzleType.TravelTeam_Short,       // 31

        // Sustained travel (RequireAll, custom runtime movable)
        (uint)PuzzleType.SustainedTravel,        // 100
    };

    /// <summary>
    /// Set by the ChainedPuzzleInstance.Setup prefix when the puzzle being built
    /// contains a matching component; consumed by the CP_Bioscan_Core.Setup
    /// postfix; cleared in the ChainedPuzzleInstance.Setup postfix.
    /// </summary>
    internal static bool Pending;
}

/// <summary>
/// Flags the current ChainedPuzzleInstance.Setup as containing a full-speed scan
/// component so the CP_Bioscan_Core.Setup postfix knows to flatten its speed table.
/// </summary>
[HarmonyPatch(typeof(ChainedPuzzleInstance), nameof(ChainedPuzzleInstance.Setup))]
public static class Patch_ChainedPuzzleInstance_ScanSpeed
{
    static void Prefix(ChainedPuzzleDataBlock data)
    {
        ScanSpeedNormalize.Pending = false;

        if (data?.ChainedPuzzle == null)
            return;

        for (var i = 0; i < data.ChainedPuzzle.Count; i++)
        {
            if (ScanSpeedNormalize.FullSpeedScanTypes.Contains(data.ChainedPuzzle[i].PuzzleType))
            {
                ScanSpeedNormalize.Pending = true;
                return;
            }
        }
    }

    static void Postfix() => ScanSpeedNormalize.Pending = false;
}

/// <summary>
/// While a matching puzzle is being set up, flattens the bioscan's per-player-count
/// speed curve so every occupant count scans at the 4-player (full-team) rate.
/// </summary>
[HarmonyPatch(typeof(CP_Bioscan_Core), nameof(CP_Bioscan_Core.Setup))]
public static class Patch_CP_Bioscan_Core_ScanSpeed
{
    static void Postfix(CP_Bioscan_Core __instance)
    {
        if (!ScanSpeedNormalize.Pending)
            return;

        var scanner = __instance.m_PlayerScannerComp?.TryCast<CP_PlayerScanner>();
        var speeds = scanner?.m_scanSpeeds;
        if (speeds == null || speeds.Length == 0)
            return;

        // "As if 4 players" = the 4-player slot (index 3), clamped for safety.
        var fourPlayerSpeed = speeds[Math.Min(3, speeds.Length - 1)];
        for (var i = 0; i < speeds.Length; i++)
            speeds[i] = fourPlayerSpeed;

        Plugin.Logger.LogDebug(
            $"[ScanSpeedNormalize] Flattened scan speeds to {fourPlayerSpeed} (4-player) for a matched scan");
    }
}
