using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Markers;

public class CommonData
{
    public List<JArray> FunctionComponentLinks { get; set; } = new();

    public List<JObject> Compositions { get; set; } = new();

    public int AssetBundleName { get; set; }

    public int Group { get; set; }

    public double FunctionPotential { get; set; }

    public int RotationSnap { get; set; }

    public double RotationNoise { get; set; }

    public string EditorMesh { get; set; }

    public double BoundingVolume { get; set; }
}
