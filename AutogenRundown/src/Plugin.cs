using AutogenRundown.Patches;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace AutogenRundown;

/// <summary>
/// Main plugin class
///
/// Note that we must use a GUID for the plugin that places this plugin before any of the other
/// plugins that depend on the generated JSON files from Autogen. This includes:
///     * Inas07.ExtraObjectiveSetup
///     * Inas07.EOSExt.Reactor
///
/// For now we just solve this by padding 0's to the front of the guid name. It's not ideal but
/// it works.
/// </summary>
[BepInPlugin("000-the_tavern-AutogenRundown", "AutogenRundown", Version)]
[BepInProcess("GTFO.exe")]
[BepInDependency("com.dak.MTFO")]
[BepInDependency("dev.gtfomodding.gtfo-api")]
public class Plugin : BasePlugin
{
    public const string Version = "0.72.0";

    public const string Name = "the_tavern-AutogenRundown";

    public static ManualLogSource Logger { get; private set; } = new("AutogenRundown");

    public static bool Config_UsePlayerColoredGlowsticks { get; set; }

    public override void Load()
    {
        Logger = Log;

        #region Configuration
        var seedConfig = Config.Bind(
            new ConfigDefinition("AutogenRundown", "Seed"),
            "",
            new ConfigDescription("Specify a seed to override rundown generation"));

        var regenerateOnStartup = Config.Bind(
            new ConfigDefinition("AutogenRundown", "RegenerateOnStartup"),
            true,
            new ConfigDescription("Should datablocks be regenerated on game startup."));

        var usePlayerColorGlowsticks = Config.Bind(
            new ConfigDefinition("AutogenRundown", "UsePlayerColorGlowsticks"),
            false,
            new ConfigDescription("Use per player color glow sticks. Client side only."));

        Config_UsePlayerColoredGlowsticks = usePlayerColorGlowsticks.Value;

        #endregion


        Log.LogInfo($"ConfigSeed=\"{seedConfig.Value}\"");
        Log.LogInfo($"RegenerateOnStartup={regenerateOnStartup.Value}");

        if (regenerateOnStartup.Value)
        {
            Peers.Init();

            RundownFactory.Build(seedConfig.Value);
            Log.LogInfo($"Rundown generated, Seed=\"{Generator.Seed}\"");
        }
        else
        {
            Log.LogInfo($"Rundown not generated, Seed=\"{Generator.Seed}\"");
        }

        PlayFabManager.add_OnTitleDataUpdated((Action)Patches.RundownNames.OnTitleDataUpdated);

        // EventAPI.OnManagersSetup += LocalProgressionManager.Current.Init;
        // AssetAPI.OnAssetBundlesLoaded += Assets.Init;

        Patch_LG_ComputerTerminal_Setup.Setup();
        Patch_LG_SecurityDoor.Setup();

        // Apply patches
        var harmony = new Harmony("the_tavern-AutogenRundown");
        harmony.PatchAll();
    }
}
