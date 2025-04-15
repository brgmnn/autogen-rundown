namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Projectiles;

public record ShooterFire : CustomRecord
{
    public ICollection<FireSetting> FireSettings { get; set; } = new List<FireSetting>();
}
