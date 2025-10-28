namespace AutogenRundown.PeerMods;

/// <summary>
/// TODO: Add support for merging text data block items
/// </summary>
public class GTFriendlyO : SupportedMod
{
    private GTFriendlyO()
    {
        ModName = "Carb_Crusaders-GTFriendlyO";
    }

    public static void Configure()
    {
        var mod = new GTFriendlyO();

        if (!Peers.HasMod(mod.ModName))
            return;

        Plugin.Logger.LogInfo($"Configuring peer mod: {mod.ModName}");

        mod.CopyGameDataJson();

        mod.CopyCustom("CustomBoosters");
        mod.CopyCustom("ExtraSyringeCustomization");
        mod.CopyCustom("ExtraToolCustomization");
        mod.CopyCustom("ExtraWeaponCustomization");
        mod.CopyCustom("GtfXP");
        mod.CopyCustom("mccad00");
    }
}
