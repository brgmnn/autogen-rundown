namespace AutogenRundown.PeerMods;

public class OmegaWeapons : SupportedMod
{
    private OmegaWeapons()
    {
        ModName = "OmegaWeapons";
    }

    public static void Configure()
    {
        var mod = new OmegaWeapons();

        if (!Peers.HasMod(mod.ModName) || mod.PluginFolder is null)
            return;

        Plugin.Logger.LogInfo($"Configuring peer mod: {mod.ModName}");

        mod.CopyGameDataJson();
    }
}
