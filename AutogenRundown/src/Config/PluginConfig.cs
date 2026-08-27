using AutogenRundown.Managers;
using BepInEx.Configuration;

namespace AutogenRundown.Config;

/// <summary>
/// Owns all BepInEx config binding for the plugin. Bound once from Plugin.Load; the bound
/// values are exposed as static properties and pushed into GenerationOverrides.Current.
/// </summary>
public static class PluginConfig
{
    private static ConfigEntry<string> dailySeed = null!;
    private static ConfigEntry<string> weeklySeed = null!;
    private static ConfigEntry<string> monthlySeed = null!;
    private static ConfigEntry<string> seasonalSeed = null!;
    private static ConfigEntry<bool> unlockAllLevels = null!;
    private static ConfigEntry<int> maxLevelRebuilds = null!;
    private static ConfigEntry<bool> regenerateOnStartup = null!;
    private static ConfigEntry<bool> usePlayerColorGlowsticks = null!;

    public static string DailySeed => dailySeed.Value;
    public static string WeeklySeed => weeklySeed.Value;
    public static string MonthlySeed => monthlySeed.Value;
    public static string SeasonalSeed => seasonalSeed.Value;
    public static bool UnlockAllLevels => unlockAllLevels.Value;
    public static bool RegenerateOnStartup => regenerateOnStartup.Value;
    public static bool UsePlayerColorGlowsticks => usePlayerColorGlowsticks.Value;

    public static void Setup(ConfigFile config)
    {
        // Bind everything first, save once at the end (each Bind would otherwise trigger a
        // full file rewrite).
        var saveOnConfigSet = config.SaveOnConfigSet;
        config.SaveOnConfigSet = false;

        #region AutogenRundown.Seeds

        dailySeed = config.Bind(
            new ConfigDefinition("AutogenRundown.Seeds", "DailySeed"),
            "",
            new ConfigDescription("Specify a seed for the Daily Rundown generation. Any string " +
                                  "can be used here, this defaults to today's date. " +
                                  "E.g. 2025_08_15 for August 15th 2025."));

        weeklySeed = config.Bind(
            new ConfigDefinition("AutogenRundown.Seeds", "WeeklySeed"),
            "",
            new ConfigDescription("Specify a seed for the Weekly Rundown.\nExpected format is " +
                                  "\"YYYY_MM_DD\" where YYYY is the year, MM is the month, and " +
                                  "DD is the day.\ne.g 2025_08_03 for August 3rd 2025.\n" +
                                  "Week number is automatically calculated from the date."));

        monthlySeed = config.Bind(
            new ConfigDefinition("AutogenRundown.Seeds", "MonthlySeed"),
            "",
            new ConfigDescription("Specify a seed for the Monthly Rundown.\nExpected format is " +
                                  "\"YYYY_MM\" where YYYY is the year (e.g 2025) and MM is the " +
                                  "month (e.g 03 for March)"));

        seasonalSeed = config.Bind(
            new ConfigDefinition("AutogenRundown.Seeds", "SeasonalSeed"),
            "",
            new ConfigDescription("Specify a seed for the Seasonal Rundown.\nExpected format is " +
                                  "\"SEASON_YYYY\" where YYYY is the year (e.g 2025) and SEASON " +
                                  "is one of the four seasons (Winter, Spring, Summer, Fall)." +
                                  "e.g SPRING_2025"));

        #endregion

        #region AutogenRundown.Levels

        unlockAllLevels = config.Bind(
            new ConfigDefinition("AutogenRundown.Levels", "UnlockAllLevels"),
            false,
            new ConfigDescription("Disables all tier unlock requirements on rundowns, unlocking all levels"));

        maxLevelRebuilds = config.Bind(
            new ConfigDefinition("AutogenRundown.Levels", "MaxLevelRebuilds"),
            10,
            new ConfigDescription("Rebuild attempts the host allows before aborting the drop and " +
                                  "permanently locking the expedition out of the rundown. " +
                                  "0 disables the limit."));

        #endregion

        #region AutogenRundown

        regenerateOnStartup = config.Bind(
            new ConfigDefinition("AutogenRundown", "RegenerateOnStartup"),
            true,
            new ConfigDescription("Should datablocks be regenerated on game startup. " +
                                  "Applies to all rundowns."));

        usePlayerColorGlowsticks = config.Bind(
            new ConfigDefinition("AutogenRundown", "UsePlayerColorGlowsticks"),
            false,
            new ConfigDescription("Use per player color glow sticks. Client side only."));

        #endregion

        #region Advanced

        // The advanced settings live in their own config file next to the main one, so they
        // stay grouped together at the end of the config folder listing instead of being
        // alphabetically interleaved by ConfigFile.Save()'s hard-coded section sort.
        var advanced = new ConfigFile(
            Path.ChangeExtension(config.ConfigFilePath, ".Advanced.cfg"),
            saveOnInit: false);
        advanced.SaveOnConfigSet = false;

        var forceComplex = advanced.Bind(
            new ConfigDefinition("Daily", "ForceComplex"),
            "",
            new ConfigDescription("Force every level in the Daily rundown to use this complex: " +
                                  "Mining, Tech, or Service. Empty disables forcing. " +
                                  "Must match across all players in a lobby."));

        var preferGardens = advanced.Bind(
            new ConfigDefinition("Daily", "PreferGardens"),
            false,
            new ConfigDescription("Prefer Gardens tiles on Service levels in the Daily rundown: " +
                                  "forced tiles take Gardens variants where available and the " +
                                  "random tile pool becomes gardens-heavy. " +
                                  "Must match across all players in a lobby."));

        var rebuildCheckDescriptions = new (string Name, string Description)[]
        {
            (RebuildCheck.NavMeshReachability,
             "Rebuild the level when a zone's navmesh fails to connect (unreachable course " +
             "nodes). Disable only for testing; broken zones will load as-is."),
            (RebuildCheck.MissingCustomGeomorph,
             "Rebuild the level when a zone that requires a custom geomorph doesn't get it " +
             "placed. Disable only for testing; affected objectives may be missing."),
            (RebuildCheck.NullSourceExpander,
             "Rebuild the level when a zone completes without a source expander. Disable only " +
             "for testing; the vanilla build code will throw on the missing expander."),
            (RebuildCheck.ZoneZeroAreas,
             "Rebuild the level when a zone fake-completes with zero areas. Disable only for " +
             "testing; downstream zones will fail to build from it."),
            (RebuildCheck.FailedToFindStartArea,
             "Reroll zone seeds and rebuild when a zone can't find a start area in its parent. " +
             "Disable only for testing; the build may hang forever on affected seeds."),
            (RebuildCheck.MissingGeneratorCluster,
             "Rebuild the level when a zone that requires a power generator cluster doesn't " +
             "spawn it. Disable only for testing; the objective will be uncompletable."),
        };

        var rebuildChecks = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, description) in rebuildCheckDescriptions)
            rebuildChecks[name] = advanced.Bind(
                new ConfigDefinition("RebuildChecks", name),
                true,
                new ConfigDescription(description + " Must match across all players in a lobby.")).Value;

        #endregion

        FactoryJobManager.MaxRebuilds = maxLevelRebuilds.Value;

        GenerationOverrides.Setup(forceComplex.Value, preferGardens.Value, rebuildChecks);

        config.Save();
        config.SaveOnConfigSet = saveOnConfigSet;

        advanced.Save();
        advanced.SaveOnConfigSet = true;
    }
}
