using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks
{
    public record class Color
    {
        static public Color MenuVisuals = new Color
        {
            Alpha = 1.0,
            Red = 0.4509804,
            Green = 0.7490196,
            Blue = 0.858823538,
        };

        public static Color White = new Color { Red = 1.0, Green = 1.0, Blue = 1.0, Alpha = 1.0 };

        #region Fog Preset colors
        /// <summary>
        /// Preset color used in several of the R8D1 fog colors
        /// </summary>
        public static Color InfectiousFog_R8D1 = new()
            {
                Red = 0.356862754,
                Green = 1.0,
                Blue = 0.545098066,
                Alpha = 0.03137255
            };
        #endregion

        [JsonProperty("a")]
        public double Alpha { get; set; }

        [JsonProperty("r")]
        public double Red { get; set; }

        [JsonProperty("g")]
        public double Green { get; set; }

        [JsonProperty("b")]
        public double Blue { get; set; }
    }
}
