namespace AutogenRundown.DataBlocks.Enemies;

/// <summary>
/// Hibernate is asleep, Hunter is for blood doors. There are others but they are broken
/// </summary>
public enum EnemyGroupType : uint
{
    /// <summary>
    /// Most spawned enemies should be hibernate
    /// </summary>
    Hibernate = 0,

    /// <summary>
    /// Usually for bosses that should be sneaked past.
    /// </summary>
    PureSneak = 1,

    /// <summary>
    /// Seems to be mostly Chargers
    /// </summary>
    Detect = 2,

    /// <summary>
    /// Scouts only
    /// </summary>
    Scout = 3,
    Awake = 5,

    /// <summary>
    /// Use only for blood doors
    /// </summary>
    Hunter = 6,
}
