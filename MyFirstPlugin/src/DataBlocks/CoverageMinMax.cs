using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstPlugin.DataBlocks
{
    internal class CoverageMinMax
    {
        private Vector2 vec = new Vector2 { X = 0.0f, Y = 0.0f };

        #region Input values
        /// <summary>
        /// Width value
        /// </summary>
        [JsonProperty("x")]
        public double X { get => vec.X; set => vec.X = (float)value; }

        /// <summary>
        /// Height value
        /// </summary>
        [JsonProperty("y")]
        public double Y { get => vec.Y; set => vec.Y = (float)value; }

        #endregion

        #region Computed values

        [JsonProperty("normalized")]
        public JObject Normalized 
        {
            get
            {
                var normal = Vector2.Normalize(vec);

                return new JObject
                {
                    ["x"] = normal.X,
                    ["y"] = normal.Y,
                    ["magnitude"] = 1.0,
                    ["sqrMagnitude"] = 1.0
                };
            }
        }

        [JsonProperty("magnitude")]
        public double Magnitude { get => vec.Length(); }

        [JsonProperty("sqrMagnitude")]
        public double SqrMagnitude { get => vec.LengthSquared(); }

        #endregion

        public static CoverageMinMax Small = new CoverageMinMax { X = 3.0, Y = 3.0 };
        public static CoverageMinMax Medium = new CoverageMinMax { X = 10.0, Y = 10.0 };
        public static CoverageMinMax Large = new CoverageMinMax { X = 20.0, Y = 20.0 };
        public static CoverageMinMax Huge = new CoverageMinMax { X = 32.0, Y = 32.0 };
    }
}
