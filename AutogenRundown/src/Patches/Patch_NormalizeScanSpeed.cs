using System;
using System.Collections.Generic;
using AutogenRundown.DataBlocks.Alarms;
using ChainedPuzzles;
using GameData;
using HarmonyLib;

namespace AutogenRundown.Patches;

/// <summary>
/// Forces specific long team/sustained scans to always progress at the 4-player
/// (full-team) speed, regardless of how many players are in the scan circle.
/// These scans require the whole team to progress anyway, so solo/small teams
/// are not penalised with much longer durations. m_playerRequirement is left
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
    /// Extend this set to cover additional scan types.
    /// </summary>
    internal static readonly HashSet<uint> FullSpeedScanTypes = new()
    {
        (uint)PuzzleType.SustainedMegaHuge,      // 17  - Sustained Mega Huge room scan
        (uint)PuzzleType.TravelTeam_MediumGreen, // 24  - Travel team short (Green)
        (uint)PuzzleType.TravelTeam_Short,       // 31  - Travel team short
        (uint)PuzzleType.SustainedTravel,        // 100 - Sustained team travel (custom)
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
