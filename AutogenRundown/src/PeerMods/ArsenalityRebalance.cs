namespace AutogenRundown.PeerMods;

public class ArsenalityRebalance : SupportedMod
{
    private ArsenalityRebalance()
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

        var pluginPath = Path.Combine(mod.PluginFolder, "GearPartTransform.json");
        var gameDataPath = Path.Combine(mod.GameDataFolder, "Custom", "mccad00", "GearPartTransform.json");

        if (!File.Exists(pluginPath))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(gameDataPath)!);
        File.Copy(pluginPath, gameDataPath, overwrite: true);

        Plugin.Logger.LogDebug($"{mod.ModName}: Copied -> GearPartTransform.json");
    }
}
