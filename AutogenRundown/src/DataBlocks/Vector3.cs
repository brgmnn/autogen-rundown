using AutogenRundown.Extensions;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public class Vector3
{
    private System.Numerics.Vector3 vec = new() { X = 0.0f, Y = 0.0f, Z = 0.0f };

    #region Input values
    [JsonProperty("x")]
    public double X { get => vec.X; set => vec.X = (float)value; }

    [JsonProperty("y")]
    public double Y { get => vec.Y; set => vec.Y = (float)value; }

    [JsonProperty("z")]
    public double Z { get => vec.Z; set => vec.Z = (float)value; }
    #endregion

    #region Computed values
    [JsonProperty("magnitude")]
    public double Magnitude { get => vec.Length(); }

    [JsonProperty("sqrMagnitude")]
    public double SqrMagnitude { get => vec.LengthSquared(); }
    #endregion

    public virtual bool Equals(Vector3? other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other is null || GetType() != other.GetType())
            return false;

        return X.ApproxEqual(other.X) && Y.ApproxEqual(other.Y) && Z.ApproxEqual(other.Z);
    }

    public static Vector3 Zero() => new() { X = 0, Y = 0, Z = 0 };

    public static Vector3 One() => new() { X = 1, Y = 1, Z = 1 };
}
