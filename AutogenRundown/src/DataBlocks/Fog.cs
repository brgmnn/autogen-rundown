using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutogenRundown.DataBlocks
{
    public record class Fog : DataBlock
    {
        public static Fog None = new Fog { PersistentId = 0 };

        /*{
          "Enabled": true,
          "FogColor": {
            "a": 0.0,
            "r": 0.5235849,
            "g": 0.682390034,
            "b": 1.0
          },
          "FogDensity": 0.0004,
          "FogAmbience": 0.0,
          "DensityNoiseDirection": {
            "x": 1.0,
            "y": 1.0,
            "z": 1.0,
            "normalized": {
              "x": 0.577350259,
              "y": 0.577350259,
              "z": 0.577350259,
              "magnitude": 1.0,
              "sqrMagnitude": 0.99999994
            },
            "magnitude": 1.73205078,
            "sqrMagnitude": 3.0
          },
          "DensityNoiseSpeed": 0.0372,
          "DensityNoiseScale": 0.08,
          "DensityHeightAltitude": -5.2,
          "DensityHeightRange": 0.6,
          "DensityHeightMaxBoost": 0.002,
          "Infection": 0.0,
          "name": "Fog_light_with_soft_layer_R1A1",
          "internalEnabled": true,
          "persistentID": 139
        }*/
        public static Fog DefaultFog = new Fog
        {
            FogColor = new Color { Red = 0.5235849, Green = 0.682390034, Blue = 1.0, Alpha = 0.0 },
            FogDensity = 0.0004,
            DensityNoiseDirection = new Vector3 { X = 1.0, Y = 1.0, Z = 1.0 },
            DensityNoiseSpeed = 0.0372,
            DensityNoiseScale = 0.08,
            DensityHeightAltitude = -5.2,
            DensityHeightRange = 0.6,
            DensityHeightMaxBoost = 0.002,
            Infection = 0.0
        };

        public static Fog FullFog = new Fog { DensityHeightAltitude = 1.0 };

        public static Fog FullFog_Infectious = FullFog with
        {
            PersistentId = Generator.GetPersistentId(),
            Infection = 0.03
        };

        public static new void SaveStatic()
        {
            Bins.Fogs.AddBlock(DefaultFog);
            Bins.Fogs.AddBlock(FullFog);
            Bins.Fogs.AddBlock(FullFog_Infectious);
        }

        /// <summary>
        /// Determine the color of the fog. This can affect the lighting of the entire level and
        /// create some preferable visual effect.
        ///
        /// It seems that starting in Rundown 7, the way fog occludes sight and interacts with
        /// lights has changed. Colored fog may not block sight well, so it's best to use white
        /// fog for that purpose.
        /// </summary>
        public Color FogColor { get; set; } = Color.White;

        /// <summary>
        /// The base fog density in the entire level.
        ///
        /// To create inversed fog (e.g. R5E1), you have to set `FogDensity` and
        /// `DensityHeightMaxBoost` such that `FogDensity` < `DensityHeightMaxBoost`.
        /// </summary>
        public double FogDensity { get; set; } = 0.02;

        /// <summary>
        /// Unused
        /// </summary>
        [Obsolete("Unused")]
        public double FogAmbience { get; set; } = 0.0001;

        /// <summary>
        /// Noise are those floating particles right above the fog plane. These particles make
        /// the fog plane resemble a tide.
        ///
        /// This field controls the floating direction of those particles / tide pattern of the
        /// fog plane.
        /// </summary>
        public Vector3 DensityNoiseDirection { get; set; } = new Vector3 { Y = 1.0 };

        /// <summary>
        /// Controls how fast noise particles float / how uneven the tide pattern of the fog
        /// plane is.
        /// </summary>
        public double DensityNoiseSpeed { get; set; } = 0.055;

        /// <summary>
        /// Seems to be the size of the noise particles. Higher values can make them look
        /// visibly separated, kind of like dust clouds.
        /// </summary>
        public double DensityNoiseScale { get; set; } = 0.045;

        /// <summary>
        /// The lowest point for the fog height. For non-inversed fog, everything with altitude
        /// lower than this value would be fully submerged by the fog.
        ///
        /// As a reference for zone altitude: Low : -4.0, Mid : 0.0, High: 4.0
        /// </summary>
        public double DensityHeightAltitude { get; set; } = 5.0;

        /// <summary>
        /// Distance above lowest point for fog height, used in calculating the highest point for
        /// fog height.
        /// </summary>
        public double DensityHeightRange { get; set; } = 2.5;

        /// <summary>
        /// This is the actual field for controlling the density of (non-inversed) fog. The larger,
        /// the more occluding.
        /// </summary>
        public double DensityHeightMaxBoost { get; set; } = 0.00075;

        /// <summary>
        /// The maximum value for how fast infection accumulates from fog. For example, 0.1 would
        /// take you 10 seconds to get fully infected. 0.03 is a common, average value to set to.
        ///
        /// Note that how much infection you get depends on how deep in the fog you are with
        /// linear scaling, where the highest point of infection (and lowest actual infection gain
        /// rate) is at DensityHeightAltitude + DensityHeightRange and lowest point is
        /// DensityHeightAltitude
        /// </summary>
        public double Infection { get; set; } = 0.0;
    }
}
