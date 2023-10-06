using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace AutogenRundown;

[BepInPlugin("AutogenRundown", "AutogenRundown", "0.1.0")]
[BepInProcess("GTFO.exe")]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("Inas07-LocalProgression-1.1.5", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{
    public static ManualLogSource Logger { get; private set; } = new ManualLogSource("MyFirstPlugin");

    public override void Load()
    {
        Logger = Log;

        // Plugin startup logic
        Log.LogInfo("Startup...");

        var seedCfg = Config.Bind(
            new ConfigDefinition("AutogenRundown", "Seed"),
            "",
            new ConfigDescription("Specify a seed to override rundown generation"));
        var seed = seedCfg.BoxedValue.ToString() ?? "";

        Generator.ReadOrSetSeed(seed);
        RundownFactory.Build();

        Log.LogInfo($"Rundown generated, with seed=\"{Generator.Seed}\"");
    }
}
