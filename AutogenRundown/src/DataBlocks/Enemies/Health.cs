namespace AutogenRundown.DataBlocks.Enemies;

public record Health
{
    public double HealthMax { get; set; } = 20.0;

    public double DamageUntilHitreact { get; set; } = 5.0;

    public double BodypartHealth { get; set; } = 10.0;

    public double WeakspotDamageMulti { get; set; } = 3.0;

    /// <summary>
    /// Default = 0.3
    /// </summary>
    public double ArmorDamageMulti { get; set; } = 0.3;

    public int HitreactOnLimbDestruction { get; set; } = 4;

    public bool TryForceHitreactOnLimbDestruction { get; set; } = true;
}
