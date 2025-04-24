namespace AutogenRundown.DataBlocks.Enemies;

public record EnemyBalancing : DataBlock
{
    #region Properties

    public Health Health { get; set; } = new();

    public double GlueTolerance { get; set; } = 4.0;

    public double GlueFadeOutTime { get; set; } = 8.0;

    public bool CanBePushed { get; set; } = true;

    public bool ForbidTwitchHit { get; set; } = false;

    public bool AllowDamgeBonusFromBehind { get; set; } = true;

    public bool UseTentacleTunnelCheck { get; set; } = true;

    public bool UseVisibilityRaycastDuringTentacleAttack { get; set; } = false;

    public double TentacleAttackDamageRadiusIfNoTunnelCheck { get; set; } = 0.0;

    public double TentacleAttackDamage { get; set; } = 3.0;

    public double MeleeAttackDamage { get; set; } = 3.0;

    public double MeleeAttackDamageCheckRadius { get; set; } = 1.0;

    public double TagTime { get; set; } = 60.0;

    public double EnemyCollisionRadius { get; set; } = 0.45;

    public double EnemyCollisionPlayerMovementReduction { get; set; } = 0.4;

    public double EnemyCollisionMinimumMoveSpeedModifier { get; set; } = 0.2;

    public double ScoutTentaclesMoveOutMultiplier { get; set; } = 4.0;

    public double ScoutTentaclesMoveInTime { get; set; } = 3.0;

    public double ScoutTentaclesMoveInDetectedTime { get; set; } = 1.0;

    public double ScoutTentacleSourceExpansionOffset { get; set; } = 0.4;

    public Vector2 ScoutTentacleAngleMinMax { get; set; } = new();

    public int ScoutTentacleCount { get; set; } = 50;

    #endregion

    public EnemyBalancing(PidOffsets offsets = PidOffsets.Normal)
        : base(Generator.GetPersistentId(offsets))
    { }

    public void Persist(BlocksBin<EnemyBalancing>? bin = null)
    {
        bin ??= Bins.EnemyBalancing;
        bin.AddBlock(this);
    }

    public static readonly EnemyBalancing NightmareGiant = new()
    {
        Health = new()
        {
            HealthMax = 180,
            DamageUntilHitreact = 45,
            BodypartHealth = 180,
            WeakspotDamageMulti = 2.25,
            ArmorDamageMulti = 0.25
        },
        GlueTolerance = 20,
        GlueFadeOutTime = 9,
        CanBePushed = false,
        TentacleAttackDamage = 6.0,
        MeleeAttackDamage = 10,
        MeleeAttackDamageCheckRadius = 1.3,
        EnemyCollisionRadius = 0.8,
        EnemyCollisionPlayerMovementReduction = 0.5,
        EnemyCollisionMinimumMoveSpeedModifier = 0.3
    };

    public static void Setup()
    {
        // Loads the base game balancing
        Setup<GameDataEnemyBalancing, EnemyBalancing>(Bins.EnemyBalancing, "EnemyBalancing");

        NightmareGiant.Persist();
    }
}

public record GameDataEnemyBalancing : EnemyBalancing
{
    /// <summary>
    /// We explicitly want to not have PIDs set when loading data, they come with their own
    /// Pids. This stops the Pid counter getting incremented when loading vanilla data.
    /// </summary>
    public GameDataEnemyBalancing() : base(PidOffsets.None)
    { }
}
