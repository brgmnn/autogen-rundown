namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Projectiles;

public record FireSetting
{
    /// <summary>
    /// Swap to this FireSetting when target is over this distance
    /// </summary>
    public double FromDistance { get; set; } = -1.0;

    /// <summary>
    /// Set this value to false if you only want to touch settings
    /// </summary>
    public bool OverrideProjectileType { get; set; } = true;

    /// <summary>
    /// TargetingTiny - 3
    /// TargetingSmall - 0
    /// TargetingMedium - 1
    /// TargetingLarge - 2
    /// SemiTargetingQuick - 6
    /// InfectionBomb - 7
    /// TargetingSmallFast - 8
    /// NotTargetingSmallFast - 9
    /// SemiTargetingBoss - 10
    /// SemiTargetingBossArtillery - 11
    /// RagingBoss - 12
    /// GlueFlying - 4 // DON'T
    /// GlueLanded - 5 // DON'T
    /// Or other custom IDs! (check ProjectileDefinitions Section)
    /// </summary>
    public uint ProjectileType { get; set; } = 50;

    /// <summary>
    /// BasedValue (based on default value): How many Projectile should be fired for single attack?
    /// </summary>
    public int BurstCount { get; set; } = 2;

    /// <summary>
    /// BasedValue (based on default value): Interval Between firing
    /// </summary>
    public double BurstDelay { get; set; } = 0.8;

    /// <summary>
    /// BasedValue (based on default value): Min Shot delay for each projectile
    /// </summary>
    public double ShotDelayMin { get; set; } = 0.005;

    /// <summary>
    /// BasedValue (based on default value): Max Shot delay for each projectile
    /// </summary>
    public double ShotDelayMax { get; set; } = 0.01;

    /// <summary>
    /// BasedValue (based on default value): Initial Shot delay after spawn
    /// </summary>
    public double InitialFireDelay { get; set; } = 0.0;

    /// <summary>
    /// BasedValue (based on default value): Min X degree spread value (Left-Right)
    /// </summary>
    public double ShotSpreadXMin { get; set; } = -45.0;

    /// <summary>
    /// BasedValue (based on default value): Max X
    /// </summary>
    public double ShotSpreadXMax { get; set; } = 45.0;

    /// <summary>
    /// BasedValue (based on default value): Min Y degree spread value (Up-Down)
    /// </summary>
    public double ShotSpreadYMin { get; set; } = -20.0;

    /// <summary>
    /// BasedValue (based on default value): Max Y
    /// </summary>
    public double ShotSpreadYMax { get; set; } = 20.0;
}
