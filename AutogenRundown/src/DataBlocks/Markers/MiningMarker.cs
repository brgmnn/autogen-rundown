using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Markers;

public record MiningMarker : DataBlock<MiningMarker>
{
    #region Properties

    public JObject CommonData { get; set; } = new();

    #endregion

    public MiningMarker(PidOffsets offsets = PidOffsets.None)
    { }

    public static void Setup()
        => Setup<MiningMarker>(Bins.MiningMarkers, "MiningMarker");

    public new static void SaveStatic()
    {
        var terminal = new MiningMarker
        {
            CommonData = new JObject
            {
                ["FunctionComponentLinks"] = new JArray
                {
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray
                    {
                        0
                    },
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray(),
                    new JArray()
                },
                ["Compositions"] = new JArray
                {
                    new JObject
                    {
                        ["weight"] = 1.0,
                        ["prefab"] = "Assets/AssetPrefabs/Complex/Generic/FunctionMarkers/Terminal_Floor.prefab",
                        ["function"] = 7,
                        ["Shard"] = 19
                    },
                    new JObject
                    {
                        ["weight"] = 1.0,
                        ["function"] = 0,
                        ["Shard"] = 19
                    }
                },
                ["AssetBundleName"] = 2,
                ["Group"] = 6,
                ["FunctionPotential"] = 1.0,
                ["RotationSnap"] = 0,
                ["RotationNoise"] = 0.0,
                ["EditorMesh"] = "Markerproducers/Mining/M_mining_container_locker_200x300x50",
                ["BoundingVolume"] = 3000.0
            },
            Name = "Mining_Terminal_Floor_200x300x50",
            PersistentId = 156
        };

        // Box cluster marker used by Red_Leicester_Cheese's Lab_I_RLC_02. The tile's marker
        // producers reference this persistent ID, so it must match the CheeseGeos README.
        var rlcBoxCluster = new MiningMarker
        {
            CommonData = new JObject
            {
                ["FunctionComponentLinks"] = new JArray(
                    Enumerable.Range(0, 22).Select(_ => new JArray())),
                ["Compositions"] = new JArray(
                    Enumerable.Range(1, 4).Select(i => new JObject
                    {
                        ["weight"] = 1.0,
                        ["prefab"] = $"Assets/Bundles/RLC_Markers/Boxes_{i}.prefab",
                        ["function"] = 0,
                        ["Shard"] = 0
                    })),
                ["AssetBundleName"] = 2,
                ["Group"] = 6,
                ["FunctionPotential"] = 2.0,
                ["RotationSnap"] = 0,
                ["RotationNoise"] = 0.0,
                ["EditorMesh"] = "Markerproducers/Mining/M_refinery_machine_1_4x4x4",
                ["BoundingVolume"] = 6400.0
            },
            Name = "MiningMarker_Digsite_BossTile1BoxCluster",
            PersistentId = 20003
        };

        Bins.MiningMarkers.AddBlock(terminal);
        Bins.MiningMarkers.AddBlock(rlcBoxCluster);
    }
}
