using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

public record ExpeditionBalance : DataBlock
{
    /// <summary>
    /// This is the default balance settings for the game
    /// </summary>
    public static readonly ExpeditionBalance DefaultBalance = new()
    {
        PersistentId = 1,

        // We will set these to 0.2 so the multi value for a zone corresponds to the number of
        // uses dropped in the zone.
        HealthPerZone = 0.2,
        DisinfectionPerZone = 0.2,
        WeaponAmmoPerZone = 0.2,
        ToolAmmoPerZone = 0.2
    };

    /// <summary>
    /// Resaves the datablock with a new persistent Id. Very useful for modifying the alarm
    /// </summary>
    public void Persist(BlocksBin<ExpeditionBalance>? bin = null)
    {
        bin ??= Bins.ExpeditionBalances;
        bin.AddBlock(this);
    }

    public new static void SaveStatic()
    {
        DefaultBalance.Persist();
    }

    #region Properties
    // All the properties are initialized by default to the vanilla balance values extracted
    // from the game data folder

    #region Resources
    // The resources section. This determines how many uses of each resource (ammo etc.) are by
    // default in one zone as a multiple of 100%. So setting say health per zone to 1.0 and a
    // health multi in a zone to 1.0 will drop _5_ uses of 20% health into the zone.

    public double HealthPerZone { get; set; } = 1.0;

    public double DisinfectionPerZone { get; set; } = 1.0;

    public double WeaponAmmoPerZone { get; set; } = 0.8;

    public double ToolAmmoPerZone { get; set; } = 0.7;
    #endregion

    public double CommodityValuePerZone { get; set; } = 0.0;

    public double ChanceToPutCommodityInResourceContainer { get; set; } = 0.7;

    public double ChanceToPutArtifactInResourceContainer { get; set; } = 0.15;

    public double ChanceToSpawnCommodityLargePack { get; set; } = 0.3;

    public double ChanceToSpawnCommodityMediumPack { get; set; } = 0.5;

    public double ChanceToReUseResourceContainer { get; set; } = 0.5;

    public int MaxPacksPerResourceContainer { get; set; } = 4;

    public double EmptyWeakResourceContainersPerZone { get; set; } = 0.0;

    public double EmptySecureResourceContainersPerZone { get; set; } = 3.0;

    public double LootPerZone { get; set; } = 3.0;

    public double AirPerZone { get; set; } = 1.0;

    public double AirPerZoneInNoAir { get; set; } = 5.0;

    public double TerminalsPerZone { get; set; } = 1.0;

    public JObject TentacleTraps { get; set; } = new()
    {
        ["Health"] = 10.0,
        ["AttackDamage"] = 1.0,
        ["MaxPerZone"] = 3,
        ["MaxSmallArea"] = 1,
        ["MaxMediumArea"] = 1,
        ["MaxLargeArea"] = 1,
        ["MaxHugeArea"] = 1
    };

    public JObject ParasiteNests { get; set; } = new()
    {
        ["Health"] = 20.0,
        ["AttackDamage"] = 2.0,
        ["MaxPerZone"] = 7,
        ["MaxSmallArea"] = 2,
        ["MaxMediumArea"] = 3,
        ["MaxLargeArea"] = 4,
        ["MaxHugeArea"] = 4
    };

    public int EnemyPatrolGroupsPerZone { get; set; } = 5;

    public int StaticEnemiesMaxPerZone { get; set; } = 10;

    public int StaticEnemiesMaxSmallArea { get; set; } = 1;

    public int StaticEnemiesMaxMediumArea { get; set; } = 2;

    public int StaticEnemiesMaxLargeArea { get; set; } = 3;

    public int StaticEnemiesMaxHugeArea { get; set; } = 5;

    public double EnemyPopulationPerZone { get; set; } = 25.0;

    public double VoxelCoverageAreaMultiplier { get; set; } = 0.037;

    public double VoxelCoverageAreaScoringRandomMultiplier { get; set; } = 0.25;

    public int ArtifactsPerSegment { get; set; } = 1;

    public int ArtifactsPerLayer { get; set; } = 30;

    public double WeakDoor4x4Health { get; set; } = 7.0;

    public double WeakDoor8x4Health { get; set; } = 12.0;

    public double WeakDoorChanceLockWeightNoLock { get; set; } = 3.0;

    public double WeakDoorChanceLockWeightMeleeLock { get; set; } = 1.0;

    public double WeakDoorChanceLockWeightHackableLock { get; set; } = 1.0;

    public double WeakDoorUnlockedChanceForOpen { get; set; } = 0.0;

    public double WeakDoorOpenChanceForWallRemoverUsed { get; set; } = 0.0;

    public double WeakDoorLockHealth { get; set; } = 15.0;

    public double WeakResourceContainerWithPackChanceForLocked { get; set; } = 0.75;

    public JArray ResourcePackSizes { get; set; } = new()
    {
        0.6,
        1.0,
        0.4,
        0.2
    };

    public double GlueVolumeToDoorHealthConversion { get; set; } = 0.5;

    public double GlueVolumeForDoorGlueMaxState { get; set; } = 27.0;

    #endregion
}
