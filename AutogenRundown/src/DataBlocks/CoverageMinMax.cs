using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    public enum CoverageSize : int
    {
        Nano = 25,
        Tiny = 35,
        Small = 50,
        Medium = 75,
        Large = 100,
        Huge = 125,
    }

    public class CoverageMinMax
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
        /// Minimum size
        /// </summary>
        [JsonProperty("x")]
        public double Min { get => vec.X; set => vec.X = (float)value; }

        /// <summary>
        /// Maximum size
        /// </summary>
        [JsonProperty("y")]
        public double Max { get => vec.Y; set => vec.Y = (float)value; }

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

        public CoverageMinMax() { }

        public CoverageMinMax(double value)
        {
            Min = value;
            Max = value;
        }

        public static readonly CoverageMinMax Nano = new() { Min = (double)CoverageSize.Nano, Max = (double)CoverageSize.Nano };
        public static readonly CoverageMinMax Tiny = new() { Min = (double)CoverageSize.Tiny, Max = (double)CoverageSize.Tiny };
        public static readonly CoverageMinMax Small = new() { Min = (double)CoverageSize.Small, Max = (double)CoverageSize.Small };
        public static readonly CoverageMinMax Medium = new() { Min = (double)CoverageSize.Medium, Max = (double)CoverageSize.Medium };
        public static readonly CoverageMinMax Large = new() { Min = (double)CoverageSize.Large, Max = (double)CoverageSize.Large };
        public static readonly CoverageMinMax Huge = new() { Min = (double)CoverageSize.Huge, Max = (double)CoverageSize.Huge };

        /// <summary>
        /// Generates a randomly picked size
        /// </summary>
        /// <returns></returns>
        public static CoverageMinMax GenCoverage()
            => new CoverageMinMax { Min = Generator.Pick(Sizes), Max = Generator.Pick(Sizes) };


        #region Normal size generation
        /// <summary>
        ///
        /// </summary>
        private static List<double> NormalSizes = new List<double>
        {
            10.0,
            15.0,
            20.0, 20.0,
            25.0, 25.0,
            30.0, 30.0,
            35.0, 35.0,
            40.0, 40.0,
            45.0, 45.0,
            50.0
        };

        /// <summary>
        /// Randomly selects two sizes from the NormalSizes list for Min and Max size. The Normal
        /// sizes list follows a somewhat normal distribution around the 25-30 size, which seems
        /// typical for the game.
        /// </summary>
        /// <returns></returns>
        public static CoverageMinMax GenNormalSize()
        {
            var size = Generator.Pick(NormalSizes);

            return new CoverageMinMax { Min = size, Max = size };
        }
        #endregion

        public static CoverageMinMax GenStartZoneSize()
        {
            var size = Generator.Pick(new List<double> { 25.0, 30.0, 35.0, 50.0 });

            return new CoverageMinMax { Min = size, Max = size + 20.0 };
        }

        public static CoverageMinMax GenSize(int zoneIndex)
            => zoneIndex == 0 ? GenStartZoneSize() : GenNormalSize();
    }
}
