using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    enum CoverageSize : int
    {
        Nano = 25,
        Tiny = 35,
        Small = 50,
        Medium = 75,
        Large = 125,
        Huge = 200,
    }

    internal class CoverageMinMax
    {
        private static List<double> Sizes = new List<double>
        {
            (double)CoverageSize.Nano,
            (double)CoverageSize.Tiny,
            (double)CoverageSize.Small,
            (double)CoverageSize.Medium,
            (double)CoverageSize.Large,
            (double)CoverageSize.Huge
        };

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

        public static CoverageMinMax Nano = new CoverageMinMax { X = (double)CoverageSize.Nano, Y = (double)CoverageSize.Nano };
        public static CoverageMinMax Tiny = new CoverageMinMax { X = (double)CoverageSize.Tiny, Y = (double)CoverageSize.Tiny };
        public static CoverageMinMax Small = new CoverageMinMax { X = (double)CoverageSize.Small, Y = (double)CoverageSize.Small };
        public static CoverageMinMax Medium = new CoverageMinMax { X = (double)CoverageSize.Medium, Y = (double)CoverageSize.Medium };
        public static CoverageMinMax Large = new CoverageMinMax { X = (double)CoverageSize.Large, Y = (double)CoverageSize.Large };
        public static CoverageMinMax Huge = new CoverageMinMax { X = (double)CoverageSize.Huge, Y = (double)CoverageSize.Huge };

        /// <summary>
        /// Generates a randomly picked size
        /// </summary>
        /// <returns></returns>
        public static CoverageMinMax GenCoverage()
            => new CoverageMinMax { X = Generator.Pick(Sizes), Y = Generator.Pick(Sizes) };
    }
}
