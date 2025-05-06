namespace AutogenRundown.PeerMods;

public class ArsenalityRebalance : SupportedMod
{
    public ArsenalityRebalance()
    {
        ModName = "leezurli-ArsenalityRebalance";
    }

    public static void Configure()
    {
        var mod = new ArsenalityRebalance();

        if (!Peers.HasMod(mod.ModName))
            return;

        Plugin.Logger.LogInfo($"Configuring peer mod: {mod.ModName}");

        mod.CopyGameDataJson();
        mod.CopyCustomMccad00();
        mod.CopyGearPartTransform();
    }
}
