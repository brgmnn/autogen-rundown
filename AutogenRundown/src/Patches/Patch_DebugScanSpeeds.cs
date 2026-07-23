using System.Text;
using ChainedPuzzles;
using GameData;
using HarmonyLib;
using LevelGeneration;

namespace AutogenRundown.Patches;

/// <summary>
/// TEMPORARY debug logging (remove after the scan-duration investigation).
///
/// Logs, for every ChainedPuzzle the game actually builds, the owning datablock's
/// persistentID + name, each component's PuzzleType, and the source area. This ties a
/// runtime scan back to its datablock block so we can confirm whether the tested zone
/// really uses PuzzleType 24 (and whether the [24] block is ever built at all).
/// </summary>
[HarmonyPatch(typeof(ChainedPuzzleInstance), nameof(ChainedPuzzleInstance.Setup))]
public static class Patch_DebugChainedPuzzleBlock
{
    static void Prefix(ChainedPuzzleDataBlock data, LG_Area sourceArea)
    {
        if (data == null)
            return;

        var types = new StringBuilder();
        if (data.ChainedPuzzle != null)
        {
            for (var i = 0; i < data.ChainedPuzzle.Count; i++)
            {
                if (i > 0)
                    types.Append(", ");
                types.Append(data.ChainedPuzzle[i].PuzzleType);
            }
        }

        // Resolve the in-game zone number ("Zone 21") the puzzle is attached to.
        var zone = "<null zone>";
        var lgZone = sourceArea?.m_zone;
        if (lgZone != null)
        {
            try { zone = $"{lgZone.AliasName} (LocalIndex={lgZone.LocalIndex})"; }
            catch { zone = "<zone unresolved>"; }
        }

        Plugin.Logger.LogInfo(
            $"[ScanSpeedDebug] ChainedPuzzleInstance.Setup block persistentID={data.persistentID} " +
            $"name={data.name} PuzzleTypes=[{types}] zone={zone} area={(sourceArea != null ? sourceArea.name : "<null>")}");
    }
}

/// <summary>
/// TEMPORARY debug logging (remove after the scan-duration investigation).
///
/// Prints the runtime CP_PlayerScanner.m_scanSpeeds values (plus requirement and
/// movement config) for every bioscan at Setup, so we can verify the values the game
/// actually loaded match the prefab. For CP_Bioscan_Big_RequireAll_Moving_3_random
/// (PuzzleType 24) all four should read 0.01 (= 1/0.01 = 100s at any player count).
///
/// Read-only: never writes m_scanSpeeds, so it is safe to run alongside or instead of
/// Patch_NormalizeScanSpeed.
/// </summary>
[HarmonyPatch(typeof(CP_Bioscan_Core), nameof(CP_Bioscan_Core.Setup))]
public static class Patch_DebugScanSpeeds
{
    static void Postfix(CP_Bioscan_Core __instance)
    {
        var scanner = __instance.m_PlayerScannerComp?.TryCast<CP_PlayerScanner>();
        if (scanner == null)
            return;

        // Primary ask: the four (or more) per-player-count scan speed values.
        var speeds = scanner.m_scanSpeeds;
        var speedText = new StringBuilder();
        if (speeds == null)
        {
            speedText.Append("<null>");
        }
        else
        {
            speedText.Append($"len={speeds.Length} [");
            for (var i = 0; i < speeds.Length; i++)
            {
                if (i > 0)
                    speedText.Append(", ");
                speedText.Append(speeds[i].ToString("0.######"));
            }
            speedText.Append(']');
        }

        // Movement config (moving scans only). A large position/path count reveals that
        // Patch_SetupMovement replaced the native path with an oversized travel loop,
        // which can stall a RequireAll scan as players/bots fall out of the circle.
        var moveText = "not movable";
        var movable = __instance.m_movingComp?.TryCast<CP_BasicMovable>();
        if (movable != null)
        {
            var scanPositions = movable.ScanPositions != null ? movable.ScanPositions.Count : 0;
            moveText = $"positions={movable.AmountOfPositions} scanPositions={scanPositions} " +
                       $"onlyMoveWhenScanning={movable.OnlyMoveWhenScannig}";
        }

        Plugin.Logger.LogInfo(
            $"[ScanSpeedDebug] {__instance.gameObject.name} | " +
            $"requirement={scanner.ScanPlayersRequired} radius={scanner.Radius:0.##} " +
            $"reduceWhenNoPlayer={scanner.ReduceWhenNoPlayer} | " +
            $"m_scanSpeeds: {speedText} | movement: {moveText}");
    }
}
