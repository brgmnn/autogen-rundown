namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Projectiles;

public record SpawnProjectile
{
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Projectile to spawn
    /// </summary>
    public uint ProjectileType { get; set; } = 71;

    /// <summary>
    /// Projectile spawned from backward direction of projectile
    /// </summary>
    public bool BackwardDirection { get; set; } = false;

    /// <summary>
    /// How many times should burst be fired? (total projectile = count * burst count)
    /// </summary>
    public int Count { get; set; } = 8;

    /// <summary>
    /// Burst fire count
    /// </summary>
    public int BurstCount { get; set; } = 2;

    /// <summary>
    /// Delay between shot
    /// </summary>
    public double Delay { get; set; } = 0.3;

    /// <summary>
    /// Delay between burst shot
    /// </summary>
    public double BurstDelay { get; set; } = 0.1;

    public double ShotSpreadXMin { get; set; } = -60.0;

    public double ShotSpreadXMax { get; set; } = 60.0;

    public double ShotSpreadYMin { get; set; } = -60.0;

    public double ShotSpreadYMax { get; set; } = 60.0;
}
