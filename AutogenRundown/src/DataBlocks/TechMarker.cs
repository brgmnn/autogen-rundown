using AutogenRundown.DataBlocks.Markers;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

public record TechMarker : DataBlock<TechMarker>
{
    #region Properties

    public CommonData CommonData { get; set; } = new();

    #endregion

    public TechMarker(PidOffsets offsets = PidOffsets.None)
    { }

    public static void Setup()
        => Setup<TechMarker>(Bins.TechMarkers, "TechMarker");

    public new static void SaveStatic()
    {
        var dogsTechGenerator = new TechMarker
        {
            CommonData = new CommonData
            {
                FunctionComponentLinks = new List<JArray>
                {
                    new(),
                    new(),
                    new(),
                    new() { 0 },
                    new(),
                    new() { 0 },
                    new() { 0 },
                    new() { 0 },
                    new() { 0 },
                    new(),
                    new() { 0 },
                    new() { 0 },
                    new() { 0 },
                    new(),
                    new(),
                    new() { 0 },
                    new(),
                    new(),
                    new(),
                    new() { 0 },
                    new(),
                    new()
                },
                Compositions = new List<JObject>
                {
                    new()
                    {
                        ["weight"] = 1.0,
                        ["prefab"] = "Assets/DogCustomGeos/DogsDevDecorator/Markers/Tech/Custom/refinery_open_2200x2200x1000_gc_datacenter.prefab",
                        ["function"] = 19,
                        ["Shard"] = 6
                    },
                    new()
                    {
                        ["weight"] = 1.0,
                        ["function"] = 0,
                        ["Shard"] = 0
                    }
                },
                AssetBundleName = 2,
                Group = 1,
                FunctionPotential = 4.0,
                RotationSnap = 0,
                RotationNoise = 0.0,
                EditorMesh = "Markerproducers/Mining/M_dig_site_open_2200x2200x1000",
                BoundingVolume = 10648001.0
            },
            Name = "mining_tech_gencluster_open_2200x2200x2200",
            PersistentId = 7509
        };

        Bins.TechMarkers.AddBlock(dogsTechGenerator);
    }
}
