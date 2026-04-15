using HarmonyLib;

namespace AutogenRundown.Patches;

[HarmonyPatch(typeof(PlayerGuiLayer), nameof(PlayerGuiLayer.Update))]
public static class Patch_GTFuckingXP
{
    private static bool _done;

    public static void Postfix(PlayerGuiLayer __instance)
    {
        if (_done)
            return;

        if (!Peers.HasMod("GTFuckingXP"))
        {
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

        xpText.Translate(120f, 60f, 0f);

        _done = true;
    }
}
