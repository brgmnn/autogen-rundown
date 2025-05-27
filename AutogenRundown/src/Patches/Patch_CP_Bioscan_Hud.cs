using ChainedPuzzles;
using HarmonyLib;
using Player;
using SNetwork;

namespace AutogenRundown.Patches;

// public struct ExtendedBioscanState
// {
//     public SNetStructs.pPlayer playerInScan5;
//     public SNetStructs.pPlayer playerInScan6;
//     public SNetStructs.pPlayer playerInScan7;
//     public SNetStructs.pPlayer playerInScan8;
// }

/// <summary>
/// Seems that the .Setup() method doesn't get called on scans?
/// </summary>
[HarmonyPatch]
public static class Patch_CP_Bioscan_Hud
{
    // private static readonly string[] PlayerChars = new[] { "A", "B", "C", "D", "W", "X", "Y", "Z" };

    /// <summary>
    /// Patches the Bioscan HUD to work with LobbyExpansion mod. This should
    /// be backwards compatible with the base game.
    /// </summary>
    /// <param name="__instance"></param>
    // [HarmonyPatch(typeof(CP_Bioscan_Hud), nameof(CP_Bioscan_Hud.Update))]
    // [HarmonyPrefix]
    // private static void Post_CP_Bioscan_Hud_Update_Prefix(CP_Bioscan_Hud __instance)
    // {
    //     // Plugin.Logger.LogDebug("Post_CP_Bioscan_Hud_Update()");
    //     // __instance.m_progressBarPlayerChar = PlayerChars;
    // }

    // SetPlayerData

    // [HarmonyPatch(typeof(CP_Bioscan_Hud), nameof(CP_Bioscan_Hud.SetPlayerData))]
    // [HarmonyPostfix]
    // private static void Post_CP_Bioscan_Hud_SetPlayerData(CP_Bioscan_Hud __instance)
    // {
    //     Plugin.Logger.LogDebug($"Post_CP_Bioscan_Hud_SetPlayerData(): max players = {__instance.m_playersMax}");
    //
    //     __instance.m_progressBarPlayerChar = new[] { "A", "B", "C", "D", "W", "X", "Y", "Z" };
    // }

    // [HarmonyPatch(typeof(CP_Bioscan_Hud), nameof(CP_Bioscan_Hud.Update))]
    // [HarmonyPostfix]
    // private static void Post_CP_Bioscan_Hud_Update(CP_Bioscan_Hud __instance)
    // {
    //     Plugin.Logger.LogDebug($"Post_CP_Bioscan_Hud_Update(): max players = {__instance.m_playersMax}, players_in_scan = {__instance.m_playersInScan}");
    // }

    // Master_OnPlayerScanChangedCheckProgress

    // [HarmonyPatch(typeof(CP_Bioscan_Sync), nameof(CP_Bioscan_Sync.SetStateData))]
    // [HarmonyPostfix]
    // public static void Post_TestSync(List<PlayerAgent> playersInScan)
    // {
    //     Plugin.Logger.LogDebug($"Players in scan = {playersInScan.Count}");
    // }

    // [HarmonyPatch(typeof(CP_Bioscan_Core), nameof(CP_Bioscan_Core.AddPlayersInScanToList))]
    // [HarmonyPostfix]
    // public static void Post_CP_Bioscan_Core_AddPlayersInScanToList(pBioscanState state, List<PlayerAgent> playerAgents)
    // {
    // }
}
