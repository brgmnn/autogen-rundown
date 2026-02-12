using HarmonyLib;
using Player;
using SNetwork;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_TimedTerminalSequenceSolo
{
    private const float SoloExtraTime = 50f;

    [HarmonyPatch(typeof(TimedTerminalSequencePuzzle), nameof(TimedTerminalSequencePuzzle.StartTimedConfirmation))]
    [HarmonyPostfix]
    public static void StartTimedConfirmation_Postfix(TimedTerminalSequencePuzzle __instance)
    {
        var humanCount = 0;

        foreach (var player in PlayerManager.PlayerAgentsInLevel)
            if (player != null && player.Owner != null && !player.Owner.IsBot)
                humanCount++;

        if (humanCount > 1)
            return;

        __instance.ConfirmationTime += SoloExtraTime;

        Plugin.Logger.LogDebug(
            $"[TimedTerminalSequenceSolo] Solo player detected, " +
            $"added {SoloExtraTime}s to confirmation time " +
            $"(now {__instance.ConfirmationTime}s)");
    }
}
