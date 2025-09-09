using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public class Vector2
{
    private System.Numerics.Vector2 vec = new() { X = 0.0f, Y = 0.0f };

    [JsonProperty("x")]
    public double X { get => vec.X; set => vec.X = (float)value; }

    [JsonProperty("y")]
    public double Y { get => vec.Y; set => vec.Y = (float)value; }
}
