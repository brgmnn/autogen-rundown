namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Abilities;

public record Birthing : CustomRecord
{
    /// <summary>
    /// Persistent ID to EnemyGroupDataBlock you want to spawn
    /// </summary>
    public uint EnemyGroupToSpawn { get; set; }

    /// <summary>
    /// Single Enemy Cost, Check out EnemyPopulationDataBlock for get right value
    /// </summary>
    public double ChildrenCost { get; set; } = 3.0;

    /// <summary>
    /// Maximum Number of babies in single spawn.
    /// </summary>
    public int ChildrenPerBirth { get; set; } = 8;

    /// <summary>
    /// At least this count of babies will be spawned. (If this count of
    /// babies breaks spawncap, birther won't spawn more)
    /// </summary>
    public int ChildrenPerBirthMin { get; set; } = 4;

    /// <summary>
    /// Max Childrens that can be in active at once
    /// </summary>
    public int ChildrenMax { get; set; } = 16;

    /// <summary>
    /// Min Timer for next birth when Birther has spawned not that much babies.
    /// (in this case, if it only spawned 4)
    /// </summary>
    public double MinDelayUntilNextBirth { get; set; } = 1.0;

    /// <summary>
    /// Max Timer for next birth when Birther has spawned maximum babies it can
    /// done in once. (in this case, if it spawned full 8 in once)
    /// </summary>
    public double MaxDelayUntilNextBirth { get; set; } = 14.0;
}
