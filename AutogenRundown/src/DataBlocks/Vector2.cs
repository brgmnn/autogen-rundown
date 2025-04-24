using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public class Vector2
{
    private System.Numerics.Vector2 vec = new() { X = 0.0f, Y = 0.0f };

    #region Input values
    [JsonProperty("x")]
    public double X { get => vec.X; set => vec.X = (float)value; }

    [JsonProperty("y")]
    public double Y { get => vec.Y; set => vec.Y = (float)value; }
    #endregion

    // #region Computed values
    // [JsonProperty("magnitude")]
    // public double Magnitude { get => vec.Length(); }
    //
    // [JsonProperty("sqrMagnitude")]
    // public double SqrMagnitude { get => vec.LengthSquared(); }
    // #endregion
}
