using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace AutogenRundown;

[BepInPlugin("the_tavern-AutogenRundown", "AutogenRundown", Version)]
[BepInProcess("GTFO.exe")]
[BepInDependency("com.dak.MTFO")]
[BepInDependency("dev.gtfomodding.gtfo-api")]
[BepInDependency("FlowGeos")]
[BepInDependency("Inas.LocalProgression")]
public class Plugin : BasePlugin
{
    public const string Version = "0.45.0";

    public const string Name = "the_tavern-AutogenRundown";

    public static ManualLogSource Logger { get; private set; } = new("AutogenRundown");

    public override void Load()
    {
        Logger = Log;

        var seedConfig = Config.Bind(
            new ConfigDefinition("AutogenRundown", "Seed"),
            "",
            new ConfigDescription("Specify a seed to override rundown generation"));

        var regenerateOnStartup = Config.Bind(
            new ConfigDefinition("AutogenRundown", "RegenerateOnStartup"),
            true,
            new ConfigDescription("Should datablocks be regenerated on game startup."));

        Log.LogInfo($"ConfigSeed=\"{seedConfig.Value}\"");
        Log.LogInfo($"RegenerateOnStartup={regenerateOnStartup.Value}");

        if (regenerateOnStartup.Value)
        {
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

        // Apply patches
        var harmony = new Harmony("the_tavern-AutogenRundown");
        harmony.PatchAll();
    }
}
