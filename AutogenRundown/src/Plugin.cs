using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace AutogenRundown;

[BepInPlugin("AutogenRundown", "AutogenRundown", "0.3.0")]
[BepInProcess("GTFO.exe")]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("Inas07-LocalProgression-1.1.5", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{
    public static string GameRevision { get; private set; } = "33871";

    public static ManualLogSource Logger { get; private set; } = new ManualLogSource("MyFirstPlugin");

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

        if (RegenerateOnStartup.Value == true)
        {
            RundownFactory.Build();
            Log.LogInfo($"Rundown generated, Seed=\"{Generator.Seed}\"");
        }
        else
        {
            Log.LogInfo($"Rundown not generated, Seed=\"{Generator.Seed}\"");
        }
    }
}
