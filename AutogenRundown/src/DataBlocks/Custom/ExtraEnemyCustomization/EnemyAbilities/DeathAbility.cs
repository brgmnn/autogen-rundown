namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.EnemyAbilities;

public record DeathAbility : CustomRecord
{
    public List<AbilityReference> Abilities { get; set; } = new();
}
