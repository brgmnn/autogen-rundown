using GTFO.API;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

internal static class ZoneSensorAssets
{
    public static GameObject? CircleSensor { get; private set; }
    public static GameObject? SmallScan { get; private set; }

    public static bool AssetsLoaded => CircleSensor != null && SmallScan != null;

    private const string VANILLA_CP_PREFAB_PATH =
        "Assets/AssetPrefabs/Complex/Generic/ChainedPuzzles/CP_Bioscan_sustained_RequireAll.prefab";

    public static void Init()
    {
        CircleSensor = AssetAPI.GetLoadedAsset<GameObject>("Assets/SecuritySensor/CircleSensor.prefab");
        SmallScan = AssetAPI.GetLoadedAsset<GameObject>(VANILLA_CP_PREFAB_PATH);

        if (CircleSensor != null)
            Plugin.Logger.LogDebug("ZoneSensor: Loaded CircleSensor prefab");
        else
            Plugin.Logger.LogWarning("ZoneSensor: Failed to load CircleSensor prefab");

        if (SmallScan != null)
            Plugin.Logger.LogDebug("ZoneSensor: Loaded SmallScan (vanilla bioscan) prefab");
        else
            Plugin.Logger.LogWarning("ZoneSensor: Failed to load SmallScan prefab");
    }
}
