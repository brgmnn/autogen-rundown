using HarmonyLib;
using Player;
using SNetwork;

namespace AutogenRundown.Patches;

[HarmonyPatch]
public class Patch_TimedTerminalSequenceSolo
{
    private const float NormalTime = 10;
    private const float SoloTime = 60f;

    [HarmonyPatch(typeof(TimedTerminalSequencePuzzle), nameof(TimedTerminalSequencePuzzle.StartTimedConfirmation))]
    [HarmonyPostfix]
    public static void StartTimedConfirmation_Postfix(TimedTerminalSequencePuzzle __instance)
    {
        var humanCount = 0;

        foreach (var player in PlayerManager.PlayerAgentsInLevel)
            if (player != null && player.Owner != null && !player.Owner.IsBot)
                humanCount++;

        __instance.ConfirmationTime = humanCount == 1 ? SoloTime : NormalTime;

        if (humanCount == 1)
            Plugin.Logger.LogDebug("[TimedTerminalSequenceSolo] Solo player detected");
    }
}
