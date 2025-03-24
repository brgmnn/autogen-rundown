namespace AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;

public enum Mode
{
    /// <summary>
    /// Uses persistentIDs field, enemy with matching id will be selected
    /// </summary>
    PersistentId,

    /// <summary>
    /// Uses nameParam field, If enemy's datablock name is equal to this
    /// </summary>
    NameEquals,

    /// <summary>
    /// Uses nameParam field, If enemy's datablock name contains this
    /// substring.
    /// </summary>
    NameContains,

    /// <summary>
    /// this applies to every enemies!
    /// </summary>
    Everything,

    /// <summary>
    /// uses categories field, If enemy is matching with any category that
    /// listed (SEE ALSO: Category.json file!)
    /// </summary>
    CategoryAny,

    /// <summary>
    /// Uses categories field, If enemy is matching with all category that
    /// listed.
    /// </summary>
    CategoryAll
}
