using GTFO.API;
using UnityEngine;

namespace AutogenRundown.Patches.ZoneSensors;

/// <summary>
/// Utility to instantiate clean TextMeshPro components from vanilla game assets.
/// The bundled CircleSensor prefab has corrupted TMPro that must be replaced.
/// </summary>
public static class GtfoTextMeshPro
{
    private const string VANILLA_CP_PREFAB_PATH =
        "Assets/AssetPrefabs/Complex/Generic/ChainedPuzzles/CP_Bioscan_sustained_RequireAll.prefab";

    public static GameObject? Instantiate(GameObject parent)
    {
        var vanillaCP = AssetAPI.GetLoadedAsset<GameObject>(VANILLA_CP_PREFAB_PATH);
        if (vanillaCP == null)
        {
            Plugin.Logger.LogError("GtfoTextMeshPro: Cannot find TMPPro from vanilla CP!");
            return null;
        }

        // TMPro is at: CP -> [0] -> [1] -> [0]
        var templateGO = vanillaCP.transform.GetChild(0).GetChild(1).GetChild(0).gameObject;
        return UnityEngine.Object.Instantiate(templateGO, parent.transform);
    }
}
