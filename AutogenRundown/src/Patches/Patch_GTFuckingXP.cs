using GTFO.API;
using HarmonyLib;
using UnityEngine;

namespace AutogenRundown.Patches;

[HarmonyPatch(typeof(PlayerGuiLayer), nameof(PlayerGuiLayer.Update))]
public static class Patch_GTFuckingXP
{
    private static bool _done;
    private static int _searchFrames;
    private const int MaxSearchFrames = 300;

    public static void Setup()
    {
        LevelAPI.OnLevelCleanup += Reset;
    }

    private static void Reset()
    {
        _done = false;
        _searchFrames = 0;
    }

    public static void Postfix(PlayerGuiLayer __instance)
    {
        if (_done)
            return;

        if (!Peers.HasMod("GTFuckingXP"))
        {
            _done = true;
            return;
        }

        _searchFrames++;
        if (_searchFrames > MaxSearchFrames)
        {
            Plugin.Logger.LogWarning("GTFuckingXP: XpText not found after timeout");
            _done = true;
            return;
        }

        var root = __instance.GuiLayerBase;
        if (root == null)
            return;

        var movementRoot = root.transform.FindChild("MovementRoot");
        if (movementRoot == null)
            return;

        var xpText = movementRoot.FindChild("XpText");
        if (xpText == null)
            return;

        xpText.Translate(0f, 100f, 0f);
        _done = true;
        Plugin.Logger.LogDebug("GTFuckingXP: Repositioned XpText +100 Y");
    }
}
