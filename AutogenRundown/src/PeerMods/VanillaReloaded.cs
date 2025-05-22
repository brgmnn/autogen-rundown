﻿namespace AutogenRundown.PeerMods;

public class VanillaReloaded : SupportedMod
{
    public VanillaReloaded()
    {
        ModName = "tru0067-VanillaReloaded";
    }

    public static void Configure()
    {
        var mod = new VanillaReloaded();

        if (!Peers.HasMod(mod.ModName))
            return;

        Plugin.Logger.LogInfo($"Configuring peer mod: {mod.ModName}");

        mod.CopyGameDataJson();

        var weaponCustomization = Path.Combine("Custom", "ExtraWeaponCustomization", "VanillaReloaded-ExtraWeaponCustomization.json");
        var pluginPath = Path.Combine(mod.PluginFolder, weaponCustomization);
        var gameDataPath = Path.Combine(mod.GameDataFolder, weaponCustomization);

        if (!File.Exists(pluginPath))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(gameDataPath)!);

        File.Copy(pluginPath, gameDataPath, overwrite: true);

        Plugin.Logger.LogDebug($"{mod.ModName}: Copied -> {weaponCustomization}");
    }
}
