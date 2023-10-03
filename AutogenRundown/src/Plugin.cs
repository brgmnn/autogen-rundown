using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using BepInEx.Configuration;

namespace AutogenRundown;

[BepInPlugin("AutogenRundown", "AutogenRundown", "1.0.0")]
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


        var seedCfg = Config.Bind(
            new ConfigDefinition("AutogenRundown", "Seed"),
            "",
            new ConfigDescription("Specify a seed to override rundown generation"));
        var seed = seedCfg.BoxedValue.ToString() ?? "";


        Generator.ReadOrSetSeed(seed);
        RundownFactory.Build();

        //if (!RundownFactory.JsonDataExists())
        //{
        //if (Generator.Seed == "")
        //{
        //    //Generator.GenerateTimeSeed();
        //    Generator.RegenerateSeed();
        //}

        //RundownFactory.Build();
        //}

        Log.LogInfo(Paths.PluginPath);
        Log.LogInfo($"== Seed \"{Generator.Seed}\", Config Seed: \"{seed}\"");

        //var harmony = new Harmony("AutogenRundown");

        //AssetAPI.OnImplReady += () =>
        //{
            //ClassInjector.RegisterTypeInIl2Cpp<HotReloader>();

            //harmony.PatchAll(typeof(HotReloadInjector));
            //harmony.PatchAll(typeof(GameDataLoader));
            //harmony.PatchAll(typeof(SetRundownInjector));
        //};
    }
}