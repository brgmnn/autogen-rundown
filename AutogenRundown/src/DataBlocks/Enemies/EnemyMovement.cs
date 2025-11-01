namespace AutogenRundown.DataBlocks.Enemies;

public record EnemyMovement : DataBlock
{
    #region Properties

    /// <summary>
    /// Default = false
    /// </summary>
    public bool ForceDisableAnimator { get; set; } = false;

    /// <summary>
    /// Default = 84
    /// </summary>
    public int AnimationControllers { get; set; } = 84;

    /// <summary>
    /// Default = false
    /// </summary>
    public bool MoveSlowerWhenInPlayerRoom { get; set; } = false;

    /// <summary>
    /// Default = 20
    /// </summary>
    public double MoveSlowerWithinTargetDistance { get; set; } = 20.0;

    /// <summary>
    /// Default = 1.2
    /// </summary>
    public double GlobalAnimSpeedMulti { get; set; } = 1.2;

    /// <summary>
    /// Default = 1.0
    /// </summary>
    public double PathMoveAnimSpeedMulti { get; set; } = 1.0;

    /// <summary>
    /// Probably don't need to set this
    ///
    /// Default = 6
    /// </summary>
    public double RotationLerp { get; set; } = 6.0;

    /// <summary>
    /// Probably don't need to set this
    ///
    /// Default = 6
    /// </summary>
    public double RotationLerpWhenNotMoving { get; set; } = 6.0;

    /// <summary>
    /// Probably don't need to set this
    ///
    /// Default = 0.15
    /// </summary>
    public double BlendIntoAttackAnim { get; set; } = 0.15;

    /// <summary>
    /// Probably don't need to set this
    ///
    /// Default = 0.15
    /// </summary>
    public double BlendIntoScreamAnim { get; set; } = 0.15;

    /// <summary>
    /// Probably don't need to set this
    ///
    /// Default = false
    /// </summary>
    public bool AllowMovmentBlendOutOfPathMove { get; set; } = false;

    /// <summary>
    /// Probably don't need to set this
    ///
    /// Default = 0.05
    /// </summary>
    public double PathMoveAnimDampTime { get; set; } = 0.05;

    /// <summary>
    /// Probably don't need to set this
    ///
    /// Default = true
    /// </summary>
    public bool AllowClimbDownLadders { get; set; } = true;

    /// <summary>
    /// Probably don't need to set this
    ///
    /// Default = 2
    /// </summary>
    public int LocomotionPathMove { get; set; } = 2;

    /// <summary>
    /// Probably don't need to set this
    ///
    /// Default = 7
    /// </summary>
    public int LocomotionHitReact { get; set; } = 7;

    /// <summary>
    /// Probably don't need to set this
    ///
    /// Default = 11
    /// </summary>
    public int LocomotionDead { get; set; } = 11;

    /// <summary>
    /// Probably don't need to set this
    ///
    /// Default = 18
    /// </summary>
    public int LocomotionShooterAttack { get; set; } = 18;

    /// <summary>
    /// Default = 6
    /// </summary>
    public int LocomotionScream { get; set; } = 16;

    #endregion

    public EnemyMovement(PidOffsets offsets = PidOffsets.Normal)
        : base(Generator.GetPersistentId(offsets))
    { }

    public void Persist(BlocksBin<EnemyMovement>? bin = null)
    {
        bin ??= Bins.EnemyMovements;
        bin.AddBlock(this);
    }

    public static readonly EnemyMovement NightmareGiant = new()
    {
        AnimationControllers = 20,
        GlobalAnimSpeedMulti = 1.1,
        PathMoveAnimSpeedMulti = 1.3
    };

    public static void Setup()
    {
        // Loads the base game balancing
        Setup<GameDataEnemyMovement, EnemyMovement>(Bins.EnemyMovements, "EnemyMovement");

        NightmareGiant.Persist();
    }
}

public record GameDataEnemyMovement : EnemyMovement
{
    /// <summary>
    /// We explicitly want to not have PIDs set when loading data, they come with their own
    /// Pids. This stops the Pid counter getting incremented when loading vanilla data.
    /// </summary>
    public GameDataEnemyMovement() : base(PidOffsets.None)
    { }
}
