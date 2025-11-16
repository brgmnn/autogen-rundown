using AutogenRundown.Components;
using AutogenRundown.Managers;
using AutogenRundown.Patches;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using GTFO.API;
using HarmonyLib;
using UnityEngine;

namespace AutogenRundown;

/// <summary>
/// Main plugin class
///
/// Note that we must use a GUID for the plugin that places this plugin before any of the other
/// plugins that depend on the generated JSON files from Autogen. This includes:
///     * com.dak.MTFO
///     * Inas07.ExtraObjectiveSetup
///     * Inas07.EOSExt.Reactor
///
/// For now we just solve this by padding 0's to the front of the guid name. It's not ideal but
/// it works.
/// </summary>
[BepInPlugin("000-the_tavern-AutogenRundown", "AutogenRundown", Version)]
[BepInProcess("GTFO.exe")]
[BepInDependency("dev.gtfomodding.gtfo-api")]
public class Plugin : BasePlugin
{
    public const string Version = "0.79.0";

    public const string Name = "the_tavern-AutogenRundown";

    public static string GameRevision => CellBuildData.GetRevision().ToString();

    public static string GameDataPath => Path.Combine(Paths.BepInExRootPath, "GameData", GameRevision);

    public static ManualLogSource Logger { get; private set; } = new("AutogenRundown");

    public static bool Config_UsePlayerColoredGlowsticks { get; set; }

    public override void Load()
    {
        Logger = Log;

        #region Configuration

        var seedDailyConfig = Config.Bind(
            new ConfigDefinition("AutogenRundown.Seeds", "DailySeed"),
            "",
            new ConfigDescription("Specify a seed for the Daily Rundown generation. Any string " +
                                  "can be used here, this defaults to today's date. " +
                                  "E.g. 2025_08_15 for August 15th 2025."));

        var seedWeeklyConfig = Config.Bind(
            new ConfigDefinition("AutogenRundown.Seeds", "WeeklySeed"),
            "",
            new ConfigDescription("Specify a seed for the Weekly Rundown.\nExpected format is " +
                                  "\"YYYY_MM_DD\" where YYYY is the year, MM is the month, and " +
                                  "DD is the day.\ne.g 2025_08_03 for August 3rd 2025.\n" +
                                  "Week number is automatically calculated from the date."));

        var seedMonthlyConfig = Config.Bind(
            new ConfigDefinition("AutogenRundown.Seeds", "MonthlySeed"),
            "",
            new ConfigDescription("Specify a seed for the Monthly Rundown.\nExpected format is " +
                                  "\"YYYY_MM\" where YYYY is the year (e.g 2025) and MM is the " +
                                  "month (e.g 03 for March)"));

        var seedSeasonalConfig = Config.Bind(
            new ConfigDefinition("AutogenRundown.Seeds", "SeasonalSeed"),
            "",
            new ConfigDescription("Specify a seed for the Seasonal Rundown.\nExpected format is " +
                                  "\"SEASON_YYYY\" where YYYY is the year (e.g 2025) and SEASON " +
                                  "is one of the four seasons (Winter, Spring, Summer, Fall)." +
                                  "e.g SPRING_2025"));

        var regenerateOnStartup = Config.Bind(
            new ConfigDefinition("AutogenRundown", "RegenerateOnStartup"),
            true,
            new ConfigDescription("Should datablocks be regenerated on game startup. " +
                                  "Applies to all rundowns."));

        var usePlayerColorGlowsticks = Config.Bind(
            new ConfigDefinition("AutogenRundown", "UsePlayerColorGlowsticks"),
            false,
            new ConfigDescription("Use per player color glow sticks. Client side only."));

        Config_UsePlayerColoredGlowsticks = usePlayerColorGlowsticks.Value;

        // // Reset any broken save configs from 0.73.0
        // // TODO: remove probably after a year? 2026/08/16
        // if (seedDailyConfig.Value == "Todays date, YYYY_MM_DD")
        //     seedDailyConfig.Value = "";
        // if (seedWeeklyConfig.Value == "Todays date, YYYY_MM_DD")
        //     seedWeeklyConfig.Value = "";
        // if (seedMonthlyConfig.Value == "Todays month, YYYY_MM")
        //     seedMonthlyConfig.Value = "";

        Config.Save();

        #endregion

        if (regenerateOnStartup.Value)
        {
            Peers.Init();
            RundownFactory.Build(
                seedDailyConfig.Value,
                seedWeeklyConfig.Value,
                seedMonthlyConfig.Value,
                seedSeasonalConfig.Value);
        }

        PlayFabManager.add_OnTitleDataUpdated((Action)RundownNames.OnTitleDataUpdated);

        Patch_LG_ComputerTerminal_Setup.Setup();
        Patch_LG_SecurityDoor.Setup();

        EventManager.Setup();

        Fix_NavMeshMarkerSubSeed.Setup();

        GameDataAPI.OnGameDataInitialized += Patch_CentralGeneratorCluster.Setup;
        GameDataAPI.OnGameDataInitialized += LogArchivistManager.Setup;

        RundownTierMarkerArchivist.PluginSetup();

        AssetAPI.OnAssetBundlesLoaded += ExpeditionSuccessPage_ArchivistIcon.OnAssetBundlesLoaded;
        AssetAPI.OnAssetBundlesLoaded += RundownTierMarkerArchivist.OnAssetBundlesLoaded;

        // Apply patches
        var harmony = new Harmony("the_tavern-AutogenRundown");
        harmony.PatchAll();
    }
}
