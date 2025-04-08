using BepInEx;

namespace AutogenRundown.PeerMods;

public static class Arsenality
{
    public static string MOD_NAME = "W33B-Arsenality";

    public static void Configure()
    {
        if (!Peers.HasMod(MOD_NAME))
            return;

        Plugin.Logger.LogInfo($"Configuring peer mod: {MOD_NAME}");

        var pluginData = Path.Combine(Paths.BepInExRootPath, "plugins", MOD_NAME, "arsenality");

        var revision = CellBuildData.GetRevision();
        var gameData = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}");

        var paths = new List<string>
        {
            Path.Combine("Custom", "mccad00", "GearPartTransform.json"),
            "GameData_ArchetypeDataBlock_bin.json",
            "GameData_GearCategoryDataBlock_bin.json",
            "GameData_GearMeleeHandlePartDataBlock_bin.json",
            "GameData_GearMeleeHeadPartDataBlock_bin.json",
            "GameData_GearMeleeNeckPartDataBlock_bin.json",
            "GameData_GearMeleePommelPartDataBlock_bin.json",
            "GameData_GearSightPartDataBlock_bin.json",
            "GameData_GearStockPartDataBlock_bin.json",
            "GameData_GearToolPayloadPartDataBlock_bin.json",
            "GameData_GearToolScreenPartDataBlock_bin.json",
            "GameData_GearToolTargetingPartDataBlock_bin.json",
            "GameData_MeleeAnimationSetDataBlock_bin.json",
            "GameData_MeleeArchetypeDataBlock_bin.json",
            "GameData_PlayerOfflineGearDataBlock_bin.json",
        };

        paths.ForEach(path => File.Copy(
            Path.Combine(pluginData, path),
            Path.Combine(gameData, path),
            overwrite: true));
    }
}
