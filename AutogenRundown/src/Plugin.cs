using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace AutogenRundown;

[BepInPlugin("AutogenRundown", "AutogenRundown", Version)]
[BepInProcess("GTFO.exe")]
[BepInDependency("com.dak.MTFO")]
[BepInDependency("dev.gtfomodding.gtfo-api")]
[BepInDependency("FlowGeos")]
[BepInDependency("Inas.LocalProgression", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{
    public const string Version = "0.36.0";

    public const string Name = "the_tavern-AutogenRundown";

    public static ManualLogSource Logger { get; private set; } = new("AutogenRundown");

    public override void Load()
    {
        Logger = Log;

        // Plugin startup logic
        Log.LogInfo("Startup...");

        var SeedConfig = Config.Bind(
            new ConfigDefinition("AutogenRundown", "Seed"),
            "",
            new ConfigDescription("Specify a seed to override rundown generation"));

        var RegenerateOnStartup = Config.Bind(
            new ConfigDefinition("AutogenRundown", "RegenerateOnStartup"),
            true,
            new ConfigDescription("Should datablocks be regenerated on game startup."));

        Log.LogInfo($"ConfigSeed=\"{SeedConfig.Value}\"");
        Log.LogInfo($"RegenerateOnStartup={RegenerateOnStartup.Value}");

        // Reads or generates the seed
        Generator.ReadOrSetSeed(SeedConfig.Value);

        if (RegenerateOnStartup.Value)
        {
            RundownFactory.Build();
            Log.LogInfo($"Rundown generated, Seed=\"{Generator.Seed}\"");
        }
        else
        {
            Log.LogInfo($"Rundown not generated, Seed=\"{Generator.Seed}\"");
        }

        PlayFabManager.add_OnTitleDataUpdated((Action)Patches.RundownNames.OnTitleDataUpdated);

        // Apply patches
        var harmony = new Harmony("AutogenRundown");
        harmony.PatchAll();
    }
}
