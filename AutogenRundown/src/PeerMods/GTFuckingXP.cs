namespace AutogenRundown.PeerMods;

/// <summary>
///
/// </summary>
public class GTFuckingXP : SupportedMod
{
    private GTFuckingXP()
    {
        ModName = "GTFuckingXP";
    }

    public static void Configure()
    {
        var mod = new GTFuckingXP();

        if (!Peers.HasMod(mod.ModName) || mod.PluginFolder is null)
            return;

        Plugin.Logger.LogInfo($"Configuring peer mod: {mod.ModName}");
    }
}
