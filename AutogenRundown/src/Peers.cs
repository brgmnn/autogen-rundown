using AutogenRundown.PeerMods;
using BepInEx;
using Newtonsoft.Json.Linq;

namespace AutogenRundown;

public static class Peers
{
    private static List<Mod> Mods { get; set; } = new();

    public static void Init()
    {
        var plugins = Path.Combine(Paths.BepInExRootPath, "plugins");

        if (!Directory.Exists(plugins))
        {
            Plugin.Logger.LogError($"No plugins directory found: {plugins}");
            return;
        }

        Mods = new List<Mod>();

        var modDirs = Directory.GetDirectories(plugins);

        foreach (var dir in modDirs)
        {
            var manifestPath = Path.Combine(dir, "manifest.json");

            if (!File.Exists(manifestPath))
                continue;

            var data = JObject.Parse(File.ReadAllText(manifestPath));
            var mod = data.ToObject<Mod>();

            if (mod is null)
                continue;

            mod.Path = dir;

            Mods.Add(mod);

            Plugin.Logger.LogInfo($"Detected peer mod: {mod.Name} {mod.Version}");
        }
    }

    /// <summary>
    /// Returns true if the mods.yml r2modman file has a mod included and enabled
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool HasMod(string name) => Mods.Any(mod => mod.Name == name);

    public static string? ModPath(string name) => Mods.Find(mod => mod.Name == name)?.Path;

    public static void Configure()
    {
        // --- Mods directly installed by Autogen ---
        LocalProgression.WriteConfig();

        // --- Supported Peers ---
        Arsenality.Configure();
        ArsenalityRebalance.Configure();
        GTFriendlyO.Configure();
        OmegaWeapons.Configure();
        VanillaReloaded.Configure();
    }
}
