namespace AutogenRundown.DataBlocks.Enemies
{
    public enum EnemyRole : uint
    {
        Melee = 0,
        Ranged = 1,
        Tank = 2,
        Lurker = 3,
        PureSneak = 4,

        /// <summary>
        /// Special. Spawns enemies in scout mode so only use for scouts
        /// </summary>
        Scout = 5,

        /// <summary>
        /// Value is broken, do not use.
        /// </summary>
        [Obsolete("Value is broken in the game")]
        Patroller = 6,

        Hunter = 7,
        BirtherChild = 8,
        MiniBoss = 9,
        Boss = 10
    }
}
