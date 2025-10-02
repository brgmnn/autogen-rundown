using AutogenRundown.PeerMods;
using BepInEx;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AutogenRundown;

public static class Peers
{
    private static List<Mod> Mods { get; set; } = new();

    public static void Init()
    {
        try
        {
            var yaml = File.ReadAllText(Path.Combine(Paths.BepInExRootPath, "..", "mods.yml"));

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            Mods = deserializer.Deserialize<List<Mod>>(yaml);
        }
        catch (FileNotFoundException)
        {
            Plugin.Logger.LogWarning("Peers.Init(): Could not find mods.yml");
        }
    }

    /// <summary>
    /// Returns true if the mods.yml r2modman file has a mod included and enabled
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool HasMod(string name)
        => Mods.Any(mod => mod.Name == name && mod.Enabled);

    public static void Configure()
    {
        // --- Mods directly installed by Autogen ---
        LocalProgression.WriteConfig();

        // --- Supported Peers ---
        Arsenality.Configure();
        ArsenalityRebalance.Configure();
        OmegaWeapons.Configure();
        VanillaReloaded.Configure();
    }
}
