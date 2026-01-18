using AutogenRundown.DataBlocks;

namespace AutogenRundown.PeerMods;

/// <summary>
///
/// </summary>
public class GTFriendlyO : SupportedMod
{
    private GTFriendlyO()
    {
        ModName = "GTFriendlyO";
    }

    public static void Configure()
    {
        var mod = new GTFriendlyO();

        if (!Peers.HasMod(mod.ModName) || mod.PluginFolder is null)
            return;

        Plugin.Logger.LogInfo($"Configuring peer mod: {mod.ModName}");

        // This mod reduces the infection rate of fog in the game
        foreach (var fog in Bins.Fogs.Blocks.Where(fog => fog.Infection > 0.005))
        {
            fog.Infection = 0.005;
        }

        mod.CopyGameDataJson();

        mod.CopyCustom("CustomBoosters");
        mod.CopyCustomExtraEnemyCustomization();
        mod.CopyCustom("ExtraSyringeCustomization");
        mod.CopyCustom("ExtraToolCustomization");
        mod.CopyCustom("ExtraWeaponCustomization");
        mod.CopyCustom("GtfXP");
        mod.CopyCustom("mccad00");
    }
}
