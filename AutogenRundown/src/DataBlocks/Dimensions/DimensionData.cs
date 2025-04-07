namespace AutogenRundown.DataBlocks.Dimensions;

public class DimensionData
{
    #region Properties

    public int LevelLayoutData { get; set; } = 193;

    public string DimensionGeomorph { get; set; } =
        "Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_32x32_elevator_shaft_tech_transition_01.prefab";

    public double VerticalExtentsUp { get; set; } = 50.0;

    public double VerticalExtentsDown { get; set; } = 50.0;

    public int DimensionResourceSetID { get; set; } = 51;

    public int DimensionFogData { get; set; } = 81;

    public double EnvironmentWetness { get; set; } = 0.0;

    public Color DustColor { get; set; } = new() { Alpha = 1.0, Red = 0.5, Green = 0.5, Blue = 0.5 };

    public double DustAlphaBoost { get; set; } = 0.0;

    public double DustTurbulence { get; set; } = 1.0;

    public uint SoundEventOnWarpTo { get; set; } = 1064851100;

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

    public bool ForbidTerminalsInDimension { get; set; } = false;

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
    // "Exposure": 1.0,
    // "AerialScale": 10.0,
    // "MieScattering": 1.0,
    // "MieG": 0.8,
    // "MultipleScattering": 1.0,
    // "CloudsData": 0,
    // "CloudsCoverage": 1.0,
    // "CloudsDensity": 0.5,
    // "CloudsSharpness": 0.0,
    // "CloudsShadowOpacity": 1.0,
    // "CloudsTimescale": 0.1,
    // "CloudsCrawling": 0.1,
    // "CloudsFade": 0.1,
    // "Sandstorm": false,
    // "SandstormEdgeA": 0.4,
    // "SandstormEdgeB": 0.6,
    // "SandstormMinFog": 0.01,
    // "ObjectiveType": 2,
    // "LinkedToLayer": 0

    #endregion
}
