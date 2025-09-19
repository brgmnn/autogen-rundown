using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

public record Dimension : DataBlock
{
    #region Properties

    /// <summary>
    /// For some reason the devs decided to put all the properties under this data key
    /// </summary>
    [JsonProperty("DimensionData")]
    public Dimensions.DimensionData Data { get; init; } = new();

    #endregion

    #region --- Static Dimensions ---

    /// <summary>
    /// None
    /// </summary>
    public static readonly Dimension None = new() { PersistentId = 0, Name = "None" };

    /// <summary>
    /// The PouncerArena needs to be set up and added to
    /// </summary>
    public static readonly Dimension PouncerArena = new()
    {
        PersistentId = 14,
        Name = "Dimension_Pouncer_Arena",
        Data = new Dimensions.DimensionData
        {
            LevelLayoutData = 0,
            DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/PouncerArena/Dimension_Pouncer_Arena_01.prefab",
            VerticalExtentsUp = 50.0,
            VerticalExtentsDown = 10.0,
            DimensionResourceSetID = 47,
            DimensionFogData = 93,
            EnvironmentWetness = 0.199,
            DustColor = new Color { Alpha = 1.0, Red = 0.65, Green = 0.6042968, Blue = 0.487499952 },
            DustAlphaBoost = 0.0,
            DustTurbulence = 1.0,
            SoundEventOnWarpTo = (Sound)1998147319,
            DisableVFXEventOnWarp = false,
            UseLocalDimensionSeeds = false,
            ForbidWaveSpawning = true,
            ForbidCarryItemWarps = true,
            LeaveDeployablesOnWarp = true,
            IsStaticDimension = true,
            StaticZoneSeed = 0,
            StaticMarkerSeed = 0,
            StaticLightSeed = 0,
            StaticLightSettings = 20,
            StaticAliasOverride = -1,
            StaticEnemySpawningInZone = new JArray(),
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
            ForbidTerminalsInDimension = true,
            IsOutside = true,
            LightAzimuth = -4.7,
            LightElevation = 81.4,
            LightIntensity = 0.3198,
            AmbientIntensity = 1.106,
            ReflectionsIntensity = 1.547,
            GodrayRange = 7622.0,
            GodrayExponent = 1.227,
            AtmosphereData = 3,
            AtmosphereDensity = 7.23,
            Exposure = 1.43,
            AerialScale = 23.2,
            MieScattering = 11.8,
            MieG = 0.77,
            MultipleScattering = 19.2,
            CloudsData = 1,
            CloudsCoverage = 0.5,
            CloudsDensity = 0.5,
            CloudsSharpness = 0.0,
            CloudsShadowOpacity = 1.0,
            CloudsTimescale = 0.02,
            CloudsCrawling = 0.1,
            CloudsFade = 0.1,
            Sandstorm = false,
            SandstormEdgeA = 0.4,
            SandstormEdgeB = 0.6,
            SandstormMinFog = 0.01,
            ObjectiveType = 0,
            LinkedToLayer = 0
        }
    };

    #endregion

    public bool RecordEqual(Dimension? other)
    {
        if (other is null || GetType() != other.GetType())
            return false;

        return Data == other.Data;
    }

    public void Persist(BlocksBin<Dimension>? bin = null)
    {
        bin ??= Bins.Dimensions;
        bin.AddBlock(this);
    }

    public static Dimension FindOrPersist(Dimension dimension)
    {
        // We specifically don't want to persist none, as we want to set the PersistentID to 0
        if (dimension == None)
            return None;

        var existing = Bins.Dimensions.GetBlock(dimension.RecordEqual);

        if (existing != null)
            return existing;

        if (dimension.PersistentId == 0)
            dimension.PersistentId = Generator.GetPersistentId(PidOffsets.WaveSettings);

        dimension.Persist();

        return dimension;
    }

    public new static void SaveStatic()
    {
        PouncerArena.Persist();
    }

    /// <summary>
    /// Instance version of static method
    /// </summary>
    /// <returns></returns>
    public Dimension FindOrPersist() => FindOrPersist(this);
}
