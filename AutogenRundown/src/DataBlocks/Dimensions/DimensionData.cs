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

    public Sound SoundEventOnWarpTo { get; set; } = Sound.Warp; //(Sound)1064851100;

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
    /// R6B1
    ///
    /// Takes about 2 mins to run from spawn to the base
    /// </summary>
    public static readonly DimensionData AlphaTwo = new()
    {
        LevelLayoutData = 0,
        DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Static_01.prefab",
        VerticalExtentsUp = 30.0,
        VerticalExtentsDown = 30.0,
        DimensionResourceSetID = 47,
        DimensionFogData = 87,
        EnvironmentWetness = 0.0,
        DustColor = new Color { Alpha = 1.0, Red = 0.5, Green = 0.37109375, Blue = 0.25 },
        DustAlphaBoost = 2.0,
        DustTurbulence = 30.0,
        SoundEventOnWarpTo = Sound.Warp,
        DisableVFXEventOnWarp = false,
        UseLocalDimensionSeeds = false,
        ForbidWaveSpawning = false,
        ForbidCarryItemWarps = false,
        LeaveDeployablesOnWarp = false,
        IsStaticDimension = true,
        StaticZoneSeed = 0,
        StaticMarkerSeed = 0,
        StaticLightSeed = 0,
        StaticLightSettings = 0,
        StaticAliasOverride = -1,
        StaticAllowResourceContainerAllocation = false,
        StaticAllowSmallPickupsAllocation = false,
        StaticForceBigPickupsAllocation = false,
        StaticConsumableDistributionInZone = 0,
        StaticBigPickupDistributionInZone = 0,
        StaticGroundSpawnersInZone = 0,
        StaticHealthMulti = 0.0,
        StaticToolAmmoMulti = 0.0,
        StaticWeaponAmmoMulti = 0.0,
        StaticDisinfectionMulti = 0.0,
        ForbidTerminalsInDimension = false,
        IsOutside = true,

        LightAzimuth = 170.8,
        LightElevation = 28.1,
        LightIntensity = 0.5,
        AmbientIntensity = 0.7,
        ReflectionsIntensity = 1.0,
        GodrayRange = 4500.0,
        GodrayExponent = 2.5,
        AtmosphereData = 2,
        AtmosphereDensity = 4.0,
        Exposure = 4.0,
        AerialScale = 10.0,
        MieScattering = 1.0,
        MieG = 0.85,
        MultipleScattering = 6.0,
        CloudsData = 1,
        CloudsCoverage = 0.962,
        CloudsDensity = 1.0,
        CloudsSharpness = 0.5,
        CloudsShadowOpacity = 1.0,
        CloudsTimescale = 0.0,
        CloudsCrawling = -0.02,
        CloudsFade = 0.35,
        Sandstorm = true,
        SandstormEdgeA = 0.25,
        SandstormEdgeB = 1.0,
        SandstormMinFog = 0.006,
        ObjectiveType = 0,
        LinkedToLayer = 0
    };

    /// <summary>
    ///
    /// </summary>
    public static readonly DimensionData AlphaThree_Top = new()
    {
        LevelLayoutData = 1398700845,
        DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_R6A2.prefab",
        VerticalExtentsUp = 50.0,
        VerticalExtentsDown = 10.0,
        DimensionResourceSetID = 47,
        DimensionFogData = 109,
        EnvironmentWetness = 0.0,
        DustColor = new Color { Alpha = 0.129411772, Red = 0.6981132, Green = 0.6952906, Blue = 0.68823427 },
        DustAlphaBoost = 1.0,
        DustTurbulence = 20.0,
        SoundEventOnWarpTo = Sound.Warp,
        DisableVFXEventOnWarp = false,
        UseLocalDimensionSeeds = false,
        ForbidWaveSpawning = false,
        ForbidCarryItemWarps = false,
        LeaveDeployablesOnWarp = false,
        IsStaticDimension = true,
        StaticZoneSeed = 418421869,
        StaticMarkerSeed = 123486931,
        StaticLightSeed = 0,
        StaticLightSettings = 56,
        StaticAliasOverride = -1,
        // StaticEnemySpawningInZone = [],
        StaticAllowResourceContainerAllocation = false,
        StaticAllowSmallPickupsAllocation = false,
        StaticForceBigPickupsAllocation = false,
        StaticConsumableDistributionInZone = 0,
        StaticBigPickupDistributionInZone = 0,
        StaticGroundSpawnersInZone = 0,
        StaticHealthMulti = 1.4,
        StaticToolAmmoMulti = 1.2,
        StaticWeaponAmmoMulti = 1.4,
        StaticDisinfectionMulti = 0.0,
        ForbidTerminalsInDimension = false,
        IsOutside = true,
        LightAzimuth = -22.3,
        LightElevation = 58.9,
        LightIntensity = 0.3048,
        AmbientIntensity = 1.0,
        ReflectionsIntensity = 1.0,
        GodrayRange = 6580.0,
        GodrayExponent = 1.348,
        AtmosphereData = 9,
        AtmosphereDensity = 6.171,
        Exposure = 3.37,
        AerialScale = 17.77,
        MieScattering = 13.6,
        MieG = 0.8231,
        MultipleScattering = 13.6,
        CloudsData = 1,
        CloudsCoverage = 0.863,
        CloudsDensity = 0.6827,
        CloudsSharpness = 0.0,
        CloudsShadowOpacity = 1.0,
        CloudsTimescale = 0.0,
        CloudsCrawling = 0.005,
        CloudsFade = 0.2356,
        Sandstorm = false,
        SandstormEdgeA = 0.4,
        SandstormEdgeB = 0.6,
        SandstormMinFog = 0.01,
        ObjectiveType = 0,
        LinkedToLayer = 0
    };

    /// <summary>
    /// Doesn't work great with spawning walking enemies due to how the two rooms vs single room
    /// split works
    /// </summary>
    public static readonly DimensionData AlphaThree_Shaft = new()
    {
        LevelLayoutData = 0,
        DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Mining_Shaft.prefab",
        VerticalExtentsUp = 100.0,
        VerticalExtentsDown = 50.0,
        DimensionResourceSetID = 47,
        DimensionFogData = 106,
        EnvironmentWetness = 0.0118,
        DustColor = new Color { Alpha = 1.0, Red = 0.5, Green = 0.351260275, Blue = 0.2523585 },
        DustAlphaBoost = 2.0,
        DustTurbulence = 20.0,
        SoundEventOnWarpTo = Sound.Warp,
        DisableVFXEventOnWarp = false,
        UseLocalDimensionSeeds = false,
        ForbidWaveSpawning = false,
        ForbidCarryItemWarps = false,
        LeaveDeployablesOnWarp = false,
        IsStaticDimension = true,
        StaticZoneSeed = 0,
        StaticMarkerSeed = 234,
        StaticLightSeed = 2234,
        StaticLightSettings = 65,
        StaticAliasOverride = -1,
        // StaticEnemySpawningInZone = [],
        StaticAllowResourceContainerAllocation = false,
        StaticAllowSmallPickupsAllocation = false,
        StaticForceBigPickupsAllocation = false,
        StaticConsumableDistributionInZone = 0,
        StaticBigPickupDistributionInZone = 0,
        StaticGroundSpawnersInZone = 0,
        StaticHealthMulti = 2.4,
        StaticToolAmmoMulti = 2.4,
        StaticWeaponAmmoMulti = 3.8,
        StaticDisinfectionMulti = 0.0,
        // StaticTerminalPlacements = []
        ForbidTerminalsInDimension = false,
        // EventsOnBossDeath = [],
        IsOutside = true,
        LightAzimuth = -169.5,
        LightElevation = 80.0,
        LightIntensity = 0.25,
        AmbientIntensity = 1.0,
        ReflectionsIntensity = 0.3,
        GodrayRange = 8000.0,
        GodrayExponent = 1.0,
        AtmosphereData = 8,
        AtmosphereDensity = 6.0,
        Exposure = 5.0,
        AerialScale = 10.0,
        MieScattering = 40.0,
        MieG = 0.75,
        MultipleScattering = 5.0,
        CloudsData = 1,
        CloudsCoverage = 0.0,
        CloudsDensity = 1.0,
        CloudsSharpness = 0.5,
        CloudsShadowOpacity = 1.0,
        CloudsTimescale = 0.0,
        CloudsCrawling = -0.02,
        CloudsFade = 0.3,
        Sandstorm = false,
        SandstormEdgeA = 0.25,
        SandstormEdgeB = 1.0,
        SandstormMinFog = 0.013,
        ObjectiveType = 0,
        LinkedToLayer = 0
    };

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
