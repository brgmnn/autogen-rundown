using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    internal record class Color
    {
        static public Color MenuVisuals = new Color
        {
            Alpha = 1.0,
            Red = 0.4509804,
            Green = 0.7490196,
            Blue = 0.858823538,
        };

        public static Color White = new Color { Red = 1.0, Green = 1.0, Blue = 1.0, Alpha = 1.0 };

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
