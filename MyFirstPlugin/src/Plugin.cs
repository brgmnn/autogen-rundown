using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;

namespace MyFirstPlugin;

/**
 * Objective types
 * - Gather Items
 * - Establish Uplink
 * - Retrieve Item
 * - Retrieve HSU
 * - Input Command
 * - Clear Path
 * - Distribute Powercells
 * - Startup Reactor
 * - Activate Generator Cluster
 * - Process Item
 * - Survive Warden Protocol
 * - Reactor Shutdown
 */

[BepInPlugin("MyFirstPlugin", "MyFirstPlugin", "1.0.0")]
[BepInProcess("GTFO.exe")]
//[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("Inas07-LocalProgression-1.1.5", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{
    public static ManualLogSource Logger { get; private set; } = new ManualLogSource("MyFirstPlugin");

    public override void Load()
    {
        Logger = Log;

        // Plugin startup logic
        Log.LogInfo($"====== Plugin is loaded! ======");

        Generator.ReadSeed();

        //if (!RundownFactory.JsonDataExists())
        //{
            if (Generator.Seed == "")
            {
                //Generator.GenerateTimeSeed();
                Generator.RegenerateSeed();
            }

            RundownFactory.Build();
        //}

        Log.LogInfo(Paths.PluginPath);
        Log.LogInfo($"== Seed \"{Generator.Seed}\"");

        //var harmony = new Harmony("MyFirstPlugin");

        //AssetAPI.OnImplReady += () =>
        //{
            //ClassInjector.RegisterTypeInIl2Cpp<HotReloader>();

            //harmony.PatchAll(typeof(HotReloadInjector));
            //harmony.PatchAll(typeof(GameDataLoader));
            //harmony.PatchAll(typeof(SetRundownInjector));
        //};
    }
}