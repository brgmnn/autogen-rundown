using GTFO.API;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

internal static class ZoneSensorAssets
{
    public static GameObject? CircleSensor { get; private set; }
    public static bool AssetsLoaded => CircleSensor != null;

    public static void Init()
    {
        CircleSensor = AssetAPI.GetLoadedAsset<GameObject>("Assets/SecuritySensor/CircleSensor.prefab");

        if (CircleSensor != null)
            Plugin.Logger.LogDebug("ZoneSensor: Loaded CircleSensor prefab");
        else
            Plugin.Logger.LogWarning("ZoneSensor: Failed to load CircleSensor prefab");
    }
}
