using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public record class Fog : DataBlock
{
    public static double DENSITY_CLEAR = 0.00008;
    public static double DENSITY_LOW   = 0.0007;
    public static double DENSITY_MED   = 0.005;
    public static double DENSITY_HIGH  = 0.015;
    public static double DENSITY_FULL  = 0.045;

    public static double HEIGHT_LOW  = -4.0;
    public static double HEIGHT_MED  =  0.0;
    public static double HEIGHT_HIGH =  4.0;

    public static Fog None = new Fog { PersistentId = 0 };

    public static Fog DefaultFog = new Fog
    {
        Name = "Fog_DefaultFog",
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

    #region Preset fogs
    /// <summary>
    /// Fully submerged low levels, mid/high are clear of fog (low density).
    /// </summary>
    public static Fog LowFog = new Fog
    {
        Name = "Fog_Low",
        FogColor = new Color { Red = 0.4, Green = 0.68, Blue = 1.0, Alpha = 0.0 },
        DensityHeightAltitude = HEIGHT_LOW,
        DensityHeightMaxBoost = DENSITY_FULL,
        FogDensity = DENSITY_LOW,

        FogLevels = { Height.OnlyLow },
        NoFogLevels =
        {
            Height.OnlyMid,
            Height.OnlyHigh,
            Height.MidHigh,
        }
    };

    /// <summary>
    /// Low and mid levels are submerged, high levels are clear of fog (low density).
    /// </summary>
    public static Fog LowMidFog = new Fog
    {
        Name = "Fog_LowMid",
        FogColor = new Color { Red = 0.4, Green = 0.68, Blue = 1.0, Alpha = 0.0 },
        DensityHeightAltitude = HEIGHT_MED,
        DensityHeightMaxBoost = DENSITY_FULL,
        FogDensity = DENSITY_LOW,

        FogLevels =
        {
            Height.OnlyLow,
            Height.OnlyMid,
            Height.LowMid
        },
        NoFogLevels = { Height.OnlyHigh }
    };

    public static Fog FullFog = new Fog
    {
        Name = "Fog_Full",
        DensityHeightAltitude = 4.0,
        DensityHeightRange = 10,
        FogColor = new() { Red = 1.4, Green = 1.4, Blue = 1.4, Alpha = 0.0 },

        FogLevels =
        {
            Height.LowMidHigh,
            Height.OnlyLow,
            Height.OnlyMid,
            Height.OnlyHigh,
            Height.LowMid,
            Height.MidHigh,
            Height.LowHigh
        },
        NoFogLevels = { }
    };

    public static Fog LowFog_Infectious = LowFog with
    {
        Name = "Fog_Low_Infectious",
        PersistentId = Generator.GetPersistentId(),
        FogColor = Color.InfectiousFog_R8D1,
        Infection = 0.03
    };

    public static Fog LowMidFog_Infectious = LowMidFog with
    {
        Name = "Fog_LowMid_Infectious",
        PersistentId = Generator.GetPersistentId(),
        FogColor = Color.InfectiousFog_R8D1,
        Infection = 0.03
    };

    public static Fog FullFog_Infectious = FullFog with
    {
        Name = "Fog_Full_Infectious",
        PersistentId = Generator.GetPersistentId(),
        FogColor = Color.InfectiousFog_R8D1,
        Infection = 0.03
    };
    #endregion

    public new static void SaveStatic()
    {
        Bins.Fogs.AddBlock(DefaultFog);

        Bins.Fogs.AddBlock(LowFog);
        Bins.Fogs.AddBlock(FullFog);

        Bins.Fogs.AddBlock(LowFog_Infectious);
        Bins.Fogs.AddBlock(LowMidFog_Infectious);
        Bins.Fogs.AddBlock(FullFog_Infectious);
    }

    #region Internal properties not exposed in the data block
    /// <summary>
    ///
    /// </summary>
    [JsonIgnore]
    public List<Height> FogLevels { get; set; } = new List<Height>();

    /// <summary>
    ///
    /// </summary>
    [JsonIgnore]
    public List<Height> NoFogLevels { get; set; } = new List<Height>
    {
        Height.LowMidHigh,
        Height.OnlyLow,
        Height.OnlyHigh,
        Height.OnlyMid,
        Height.LowMid,
        Height.MidHigh,
        Height.LowHigh,
    };
    #endregion

    #region Properties

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

    #endregion

    /// <summary>
    /// True if the fog is infectious
    /// </summary>
    public bool IsInfectious => Infection > 0.01;
}
