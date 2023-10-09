using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks
{
    internal class Vector3
    {
        private System.Numerics.Vector3 vec = new System.Numerics.Vector3 { X = 0.0f, Y = 0.0f, Z = 0.0f };

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
    }
}
