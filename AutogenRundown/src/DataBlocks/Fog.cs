using AutogenRundown.Extensions;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks;

public record Fog : DataBlock
{
    public static double DENSITY_CLEAR = 0.00008;
    public static double DENSITY_LOW   = 0.0007;
    public static double DENSITY_MED   = 0.005;
    public static double DENSITY_HIGH  = 0.015;

    /// <summary>
    /// Value = 0.045
    /// </summary>
    public static double DENSITY_FULL  = 0.045;

    public static double HEIGHT_LOW  = -4.0;
    public static double HEIGHT_MED  =  0.0;
    public static double HEIGHT_HIGH =  4.0;
    public static double HEIGHT_MAX  = 12.0;

    public static double INFECTION_SLOW   = 0.015;
    public static double INFECTION_MEDIUM = 0.022;
    public static double INFECTION_FAST   = 0.030;

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

    #region Dimensions

    public static readonly Fog AlphaSix = new()
    {
        PersistentId = 92,

        FogColor = new Color { Red = 0.735849, Green = 0.583935559, Blue = 0.385279447, Alpha = 0.0 },
        FogDensity = 0.0001,
        FogAmbience = 0.0,
        DensityNoiseDirection = new Vector3 { X = -1, Y = 0, Z = 0 },
        DensityNoiseSpeed = 0.6,
        DensityNoiseScale = 0.017,
        DensityHeightRange = 6.0,
        DensityHeightMaxBoost = 0.007,
        Infection = 0.0
    };

    #endregion

    #region By level

    #region Bases

    public static readonly Fog NormalFog = DefaultFog with
    {
        Name = "Normal_Fog",
        FogDensity = DENSITY_LOW,
        DensityHeightMaxBoost = DENSITY_HIGH,
        DensityHeightAltitude = HEIGHT_MAX,
        DensityHeightRange = 0.2,
        DensityNoiseSpeed = 0.074,
        DensityNoiseScale = 0.131,
        DensityNoiseDirection = new Vector3 { X = 0.0, Y = -1.0, Z = 0.0 },
    };

    public static readonly Fog InvertedFog = DefaultFog with
    {
        Name = "Inverted_Fog",
        FogDensity = DENSITY_HIGH,
        DensityHeightMaxBoost = DENSITY_LOW,
        DensityHeightAltitude = HEIGHT_MAX,
        DensityHeightRange = 0.2,
        DensityNoiseSpeed = 0.074,
        DensityNoiseScale = 0.131,
        DensityNoiseDirection = new Vector3 { X = 0.0, Y = -1.0, Z = 0.0 },
    };

    public static readonly Fog NormalInfectiousFog = DefaultFog with
    {
        Name = "NormalInfectious_Fog",
        FogDensity = DENSITY_LOW,
        DensityHeightMaxBoost = DENSITY_HIGH,
        DensityHeightAltitude = HEIGHT_MAX,
        DensityHeightRange = 0.2,
        DensityNoiseSpeed = 0.074,
        DensityNoiseScale = 0.131,
        DensityNoiseDirection = new Vector3 { X = 0.0, Y = -1.0, Z = 0.0 },

        FogColor = Color.InfectiousFog_R8D1,
        Infection = INFECTION_MEDIUM,
    };

    public static readonly Fog InvertedInfectiousFog = DefaultFog with
    {
        Name = "InvertedInfectious_Fog",
        FogDensity = DENSITY_HIGH,
        DensityHeightMaxBoost = DENSITY_LOW,
        DensityHeightAltitude = HEIGHT_MAX,
        DensityHeightRange = 0.2,
        DensityNoiseSpeed = 0.074,
        DensityNoiseScale = 0.131,
        DensityNoiseDirection = new Vector3 { X = 0.0, Y = -1.0, Z = 0.0 },

        FogColor = Color.InfectiousFog_R8D1,
        Infection = INFECTION_MEDIUM,
    };

    #endregion

    #region Normal

    public static readonly Fog Normal_Altitude_minus8 = NormalFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -8.0,
    };

    public static readonly Fog Normal_Altitude_minus6 = NormalFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -6.0,
    };

    public static readonly Fog Normal_Altitude_minus4 = NormalFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -4.0,
    };

    public static readonly Fog Normal_Altitude_minus2 = NormalFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -2.0,
    };

    public static readonly Fog Normal_Altitude_0 = NormalFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 0.0,
    };

    public static readonly Fog Normal_Altitude_2 = NormalFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 2.0,
    };

    public static readonly Fog Normal_Altitude_4 = NormalFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 4.0,
    };

    public static readonly Fog Normal_Altitude_6 = NormalFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 6.0,
    };

    public static readonly Fog Normal_Altitude_8 = NormalFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 8.0,
    };

    #endregion

    #region Inverted

    public static readonly Fog Inverted_Altitude_minus8 = InvertedFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -8.0,
    };

    public static readonly Fog Inverted_Altitude_minus6 = InvertedFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -6.0,
    };

    public static readonly Fog Inverted_Altitude_minus4 = InvertedFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -4.0,
    };

    public static readonly Fog Inverted_Altitude_minus2 = InvertedFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -2.0,
    };

    public static readonly Fog Inverted_Altitude_0 = InvertedFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 0.0,
    };

    public static readonly Fog Inverted_Altitude_2 = InvertedFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 2.0,
    };

    public static readonly Fog Inverted_Altitude_4 = InvertedFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 4.0,
    };

    public static readonly Fog Inverted_Altitude_6 = InvertedFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 6.0,
    };

    public static readonly Fog Inverted_Altitude_8 = InvertedFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 8.0,
    };

    #endregion

    #region Normal Infectious

    public static readonly Fog NormalInfectious_Altitude_minus8 = NormalInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -8.0,
    };

    public static readonly Fog NormalInfectious_Altitude_minus6 = NormalInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -6.0,
    };

    public static readonly Fog NormalInfectious_Altitude_minus4 = NormalInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -4.0,
    };

    public static readonly Fog NormalInfectious_Altitude_minus2 = NormalInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -2.0,
    };

    public static readonly Fog NormalInfectious_Altitude_0 = NormalInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 0.0,
    };

    public static readonly Fog NormalInfectious_Altitude_2 = NormalInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 2.0,
    };

    public static readonly Fog NormalInfectious_Altitude_4 = NormalInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 4.0,
    };

    public static readonly Fog NormalInfectious_Altitude_6 = NormalInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 6.0,
    };

    public static readonly Fog NormalInfectious_Altitude_8 = NormalInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 8.0,
    };

    #endregion

    #region Inverted Infectious

    public static readonly Fog InvertedInfectious_Altitude_minus8 = InvertedInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -8.0,
    };

    public static readonly Fog InvertedInfectious_Altitude_minus6 = InvertedInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -6.0,
    };

    public static readonly Fog InvertedInfectious_Altitude_minus4 = InvertedInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -4.0,
    };

    public static readonly Fog InvertedInfectious_Altitude_minus2 = InvertedInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = -2.0,
    };

    public static readonly Fog InvertedInfectious_Altitude_0 = InvertedInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 0.0,
    };

    public static readonly Fog InvertedInfectious_Altitude_2 = InvertedInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 2.0,
    };

    public static readonly Fog InvertedInfectious_Altitude_4 = InvertedInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 4.0,
    };

    public static readonly Fog InvertedInfectious_Altitude_6 = InvertedInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 6.0,
    };

    public static readonly Fog InvertedInfectious_Altitude_8 = InvertedInfectiousFog with
    {
        PersistentId = Generator.GetPersistentId(),
        DensityHeightAltitude = 8.0,
    };

    #endregion
    #endregion


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
        DensityHeightAltitude = 16.0,
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

    public static Fog HeavyFullFog = FullFog with
    {
        Name = "Fog_HeavyFull",
        PersistentId = Generator.GetPersistentId(),

        FogDensity = DENSITY_LOW,
        DensityHeightMaxBoost = DENSITY_HIGH,
        DensityHeightAltitude = HEIGHT_MAX,
        DensityHeightRange = 0.2,
        DensityNoiseSpeed = 0.074,
        DensityNoiseScale = 0.131,
        DensityNoiseDirection = new Vector3 { X = 0.0, Y = -1.0, Z = 0.0 },
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

    public static Fog HeavyFullFog_Infectious = HeavyFullFog with
    {
        Name = "Fog_Full_Infectious",
        PersistentId = Generator.GetPersistentId(),
        FogColor = new()
        {
            Red = 0.356862754,
            Green = 1.2,
            Blue = 0.545098066,
            Alpha = 0.03137255
        },
        Infection = INFECTION_SLOW,
    };
    #endregion

    public virtual bool Equals(Fog? other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other is null || GetType() != other.GetType())
            return false;

        return PersistentId == other.PersistentId &&
               FogColor.Equals(other.FogColor) &&
               FogDensity.ApproxEqual(other.FogDensity) &&
               DensityNoiseDirection.Equals(other.DensityNoiseDirection) &&
               DensityNoiseSpeed.ApproxEqual(other.DensityNoiseSpeed) &&
               DensityNoiseScale.ApproxEqual(other.DensityNoiseScale) &&
               DensityHeightAltitude.ApproxEqual(other.DensityHeightAltitude) &&
               DensityHeightRange.ApproxEqual(other.DensityHeightRange) &&
               DensityHeightMaxBoost.ApproxEqual(other.DensityHeightMaxBoost) &&
               Infection.ApproxEqual(other.Infection);
    }

    public void Persist(BlocksBin<Fog>? bin = null)
    {
        bin ??= Bins.Fogs;
        bin.AddBlock(this);
    }

    /// <summary>
    /// Given a puzzle, either find it's duplicate and use that or persist this one and return that
    /// instance.
    /// </summary>
    /// <param name="puzzle"></param>
    /// <returns></returns>
    public static Fog FindOrPersist(Fog fog)
    {
        // We specifically don't want to persist none, as we want to set the PersistentID to 0
        if (fog == None)
            return None;

        var existing = Bins.Fogs.GetBlock(fog.Equals);

        if (existing != null)
            return existing;

        if (fog.PersistentId == 0)
            fog.PersistentId = Generator.GetPersistentId();

        fog.Persist();

        return fog;
    }

    public new static void SaveStatic()
    {
        Bins.Fogs.AddBlock(DefaultFog);

        Bins.Fogs.AddBlock(LowFog);
        Bins.Fogs.AddBlock(FullFog);
        Bins.Fogs.AddBlock(HeavyFullFog);

        Bins.Fogs.AddBlock(LowFog_Infectious);
        Bins.Fogs.AddBlock(LowMidFog_Infectious);
        Bins.Fogs.AddBlock(FullFog_Infectious);
        Bins.Fogs.AddBlock(HeavyFullFog_Infectious);

        AlphaSix.Persist();

        Normal_Altitude_minus8.Persist();
        Normal_Altitude_minus6.Persist();
        Normal_Altitude_minus4.Persist();
        Normal_Altitude_minus2.Persist();
        Normal_Altitude_0.Persist();
        Normal_Altitude_2.Persist();
        Normal_Altitude_4.Persist();
        Normal_Altitude_6.Persist();
        Normal_Altitude_8.Persist();

        Inverted_Altitude_minus8.Persist();
        Inverted_Altitude_minus6.Persist();
        Inverted_Altitude_minus4.Persist();
        Inverted_Altitude_minus2.Persist();
        Inverted_Altitude_0.Persist();
        Inverted_Altitude_2.Persist();
        Inverted_Altitude_4.Persist();
        Inverted_Altitude_6.Persist();
        Inverted_Altitude_8.Persist();

        NormalInfectious_Altitude_minus8.Persist();
        NormalInfectious_Altitude_minus6.Persist();
        NormalInfectious_Altitude_minus4.Persist();
        NormalInfectious_Altitude_minus2.Persist();
        NormalInfectious_Altitude_0.Persist();
        NormalInfectious_Altitude_2.Persist();
        NormalInfectious_Altitude_4.Persist();
        NormalInfectious_Altitude_6.Persist();
        NormalInfectious_Altitude_8.Persist();

        InvertedInfectious_Altitude_minus8.Persist();
        InvertedInfectious_Altitude_minus6.Persist();
        InvertedInfectious_Altitude_minus4.Persist();
        InvertedInfectious_Altitude_minus2.Persist();
        InvertedInfectious_Altitude_0.Persist();
        InvertedInfectious_Altitude_2.Persist();
        InvertedInfectious_Altitude_4.Persist();
        InvertedInfectious_Altitude_6.Persist();
        InvertedInfectious_Altitude_8.Persist();
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
    public Vector3 DensityNoiseDirection { get; set; } = new() { Y = 1.0 };

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

    public bool IsInverted => FogDensity > DensityHeightMaxBoost;
}
