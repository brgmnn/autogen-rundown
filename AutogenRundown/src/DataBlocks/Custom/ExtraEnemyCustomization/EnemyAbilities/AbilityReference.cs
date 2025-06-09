namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.EnemyAbilities;

public class AbilityReference
{
    /// <summary>
    /// One of AgentTarget mode:
    ///     * Off (Alias: Dead)
    ///     * Agressive (Alias: Combat)
    ///     * Hibernate (Alias: Hibernate, Hibernation, Hibernating, Sleeping)
    ///     * Scout (Alias: ScoutPatrolling)
    ///     * Patrolling
    /// </summary>
    public string AllowedMode { get; set; } = "Off";

    public double Delay { get; set; } = 0.0;

    public string AbilityName { get; set; } = "";
}
