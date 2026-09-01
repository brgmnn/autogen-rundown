using AutogenRundown.Components;
using AutogenRundown.Config;
using AutogenRundown.Managers;
using AutogenRundown.Patches;
using AutogenRundown.Patches.TravelScan;
using AutogenRundown.Patches.CustomTerminals;
using AutogenRundown.Patches.ZoneSensors;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using GTFO.API;
using HarmonyLib;

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
[BepInDependency("Amor.AmorLib")]
public class Plugin : BasePlugin
{
    public const string Version = "1.1.1";

    public const string Name = "the_tavern-AutogenRundown";

    public static string GameRevision => CellBuildData.GetRevision().ToString();

    public static string GameDataPath => Path.Combine(Paths.BepInExRootPath, "GameData", GameRevision);

    public static ManualLogSource Logger { get; private set; } = new("AutogenRundown");

    public override void Load()
    {
        Logger = Log;

        PluginConfig.Setup(Config);

        if (PluginConfig.RegenerateOnStartup)
        {
            Peers.Init();
            RundownFactory.Build(
                dailySeed: PluginConfig.DailySeed,
                weeklySeed: PluginConfig.WeeklySeed,
                monthlySeed: PluginConfig.MonthlySeed,
                seasonalSeed: PluginConfig.SeasonalSeed,
                unlockAll: PluginConfig.UnlockAllLevels);
        }
        else
        {
            var metadata = DataBlocks.RundownMetadata.Load();
            if (metadata != null)
            {
                foreach (var entry in metadata.Rundowns)
                {
                    DataBlocks.Bins.Rundowns.AddBlock(new DataBlocks.Rundown
                    {
                        PersistentId = entry.PersistentId,
                        Title = entry.Title
                    });
                }
                Generator.WeekNumber = metadata.WeekNumber;
                Generator.InputDailySeed = metadata.InputDailySeed;
                Generator.InputWeeklySeed = metadata.InputWeeklySeed;
                Generator.InputMonthlySeed = metadata.InputMonthlySeed;
                Generator.SeasonalSeason = metadata.SeasonalSeason;
                Generator.SeasonalYear = metadata.SeasonalYear;
            }
        }

        PlayFabManager.add_OnTitleDataUpdated((Action)RundownNames.OnTitleDataUpdated);

        EventManager.Setup();
        PatchManager.Setup();

        GameDataAPI.OnGameDataInitialized += Patch_CentralGeneratorCluster.Setup;
        GameDataAPI.OnGameDataInitialized += LogArchivistManager.Setup;
        GameDataAPI.OnGameDataInitialized += BuildFailureManager.Setup;
        GameDataAPI.OnGameDataInitialized += ZoneSensorManager.Setup;
        GameDataAPI.OnGameDataInitialized += TravelScanRegistry.Setup;
        GameDataAPI.OnGameDataInitialized += CustomTerminalSpawnManager.Setup;
        GameDataAPI.OnGameDataInitialized += Patch_ForceMinAreaCount.Setup;

        // LevelAPI.OnLevelCleanup += SignBorderManager.Clear;
        // LevelAPI.OnEnterLevel += () =>
        // {
        //     SignBorderManager.SetBorderColor(0, new Color { r = 1.0f, b = 0.0f, g = 0.0f });
        // };

        AssetAPI.OnAssetBundlesLoaded += ExpeditionSuccessPage_ArchivistIcon.OnAssetBundlesLoaded;
        AssetAPI.OnAssetBundlesLoaded += RundownTierMarkerArchivist.OnAssetBundlesLoaded;
        AssetAPI.OnAssetBundlesLoaded += ZoneSensorAssets.Init;

        // Apply patches
        var harmony = new Harmony("the_tavern-AutogenRundown");
        harmony.PatchAll();

        // Native detour for methods that crash HarmonyX's DMD codegen
        Patch_LG_NodeTools.Setup();
    }
}
