using AutogenRundown.DataBlocks.ZoneData;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Dimensions;

public record DimensionData
{
    #region Properties

    public int LevelLayoutData { get; set; } = 0;

    public string DimensionGeomorph { get; set; } =
        "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Static_01.prefab";

    public double VerticalExtentsUp { get; set; } = 50.0;

    public double VerticalExtentsDown { get; set; } = 50.0;

    public int DimensionResourceSetID { get; set; } = 51;

    public uint DimensionFogData { get; set; } = 81;

    public double EnvironmentWetness { get; set; } = 0.0;

    public Color DustColor { get; set; } = new() { Alpha = 1.0, Red = 0.5, Green = 0.5, Blue = 0.5 };

    public double DustAlphaBoost { get; set; } = 0.0;

    public double DustTurbulence { get; set; } = 1.0;

    public Sound SoundEventOnWarpTo { get; set; } = (Sound)1064851100;

    public bool DisableVFXEventOnWarp { get; set; } = false;

    public bool UseLocalDimensionSeeds { get; set; } = true;

    public bool ForbidWaveSpawning { get; set; } = false;

    public bool ForbidCarryItemWarps { get; set; } = false;

    public bool LeaveDeployablesOnWarp { get; set; } = false;

    public bool IsStaticDimension { get; set; } = false;

    public int StaticZoneSeed { get; set; } =  0;

    public int StaticMarkerSeed { get; set; } =  0;

    public int StaticLightSeed { get; set; } =  0;

    public int StaticLightSettings { get; set; } =  0;

    public int StaticAliasOverride { get; set; } =  -1;

    public JArray StaticEnemySpawningInZone { get; set; } = new();

    public bool StaticAllowResourceContainerAllocation { get; set; } = false;

    public bool StaticAllowSmallPickupsAllocation { get; set; } = false;

    public bool StaticForceBigPickupsAllocation { get; set; } = false;

    public int StaticConsumableDistributionInZone { get; set; } =  0;

    public int StaticBigPickupDistributionInZone { get; set; } =  0;

    public int StaticGroundSpawnersInZone { get; set; } =  0;

    public double StaticHealthMulti { get; set; } = 0.0;

    public double StaticToolAmmoMulti { get; set; } = 0.0;

    public double StaticWeaponAmmoMulti { get; set; } = 0.0;

    public double StaticDisinfectionMulti { get; set; } = 0.0;

    public List<TerminalPlacement> StaticTerminalPlacements { get; set; } = new();

    public bool ForbidTerminalsInDimension { get; set; } = false;

    /// <summary>
    /// Default = false
    /// </summary>
    public bool IsOutside { get; set; } = false;

    public double LightAzimuth { get; set; } = 0.0;

    public double LightElevation { get; set; } = 45.0;

    public double LightIntensity { get; set; } = 1.0;

    public double AmbientIntensity { get; set; } = 1.0;

    public double ReflectionsIntensity { get; set; } = 1.0;

    public double GodrayRange { get; set; } = 8000.0;

    public double GodrayExponent { get; set; } = 1.0;

    public int AtmosphereData { get; set; } = 0;

    public double AtmosphereDensity { get; set; } = 1.0;

    public double Exposure { get; set; } = 1.0;

    public double AerialScale { get; set; } = 10.0;

    public double MieScattering { get; set; } = 1.0;

    public double MieG { get; set; } = 0.8;

    public double MultipleScattering { get; set; } = 1.0;

    public int CloudsData { get; set; } = 0;

    public double CloudsCoverage { get; set; } = 1.0;

    public double CloudsDensity { get; set; } = 0.5;

    public double CloudsSharpness { get; set; } = 0.0;

    public double CloudsShadowOpacity { get; set; } = 1.0;

    public double CloudsTimescale { get; set; } = 0.1;

    public double CloudsCrawling { get; set; } = 0.1;

    public double CloudsFade { get; set; } = 0.1;

    public bool Sandstorm { get; set; } = false;

    public double SandstormEdgeA { get; set; } = 0.4;

    public double SandstormEdgeB { get; set; } = 0.6;

    public double SandstormMinFog { get; set; } = 0.01;

    public int ObjectiveType { get; set; } = 2;

    public int LinkedToLayer { get; set; } = 0;

    #endregion

    #region --- Prebuilt Static Dimensions ---

    /// <summary>
    /// This is the R7C2 desert camp that you have to run to
    ///
    /// Very little cover, enemies can spawn on you. Takes about 65s to run from spawn to
    /// the outpost terminal.
    ///
    /// Make sure to set:
    ///     ["MLSLevelKit"] = 1,
    ///
    /// In the level to make the materials work correctly
    /// </summary>
    public static readonly DimensionData AlphaSix = new()
    {
        DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Dune_camp_03.prefab",
        VerticalExtentsUp = 100.0,
        VerticalExtentsDown = 55.0,
        DimensionResourceSetID = 47,
        DimensionFogData = Fog.AlphaSix.PersistentId,
        DustColor = new Color { Alpha = 1.0, Red = 0.5019608, Green = 0.446603864, Blue = 0.3558902 },
        DustAlphaBoost = 5.0,
        DustTurbulence = 100.0,
        SoundEventOnWarpTo = Sound.Warp,
        UseLocalDimensionSeeds = false,
        IsStaticDimension = true,
        IsOutside = true,

        LightAzimuth = 180.0,
        LightElevation = 165.0,
        LightIntensity = 0.4,
        AmbientIntensity = 0.4,
        ReflectionsIntensity = 1.0,
        GodrayRange = 2000.0,
        GodrayExponent = 1.0,
        AtmosphereData = 4,
        AtmosphereDensity = 2.0,
        Exposure = 5.0,
        AerialScale = 20.0,
        MieScattering = 10.0,
        MieG = 0.85,
        MultipleScattering = 2.0,
        CloudsData = 1,
        CloudsCoverage = 0.625,
        CloudsDensity = 1.0,
        CloudsSharpness = 0.0,
        CloudsShadowOpacity = 1.0,
        CloudsTimescale = 0.0,
        CloudsCrawling = 0.02,
        CloudsFade = 0.3,
        Sandstorm = true,
        SandstormEdgeA = 0.5,
        SandstormEdgeB = 0.7583,
        SandstormMinFog = 0.0063,
        ObjectiveType = 0,
        LinkedToLayer = 0
    };

    #endregion
}
