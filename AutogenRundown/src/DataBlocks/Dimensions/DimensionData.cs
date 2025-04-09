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
    // public int DimensionResourceSetID { get; set; } = 47; // 51;

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

    public JArray StaticTerminalPlacements { get; set; } = new();

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
}
