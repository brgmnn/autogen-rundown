namespace AutogenRundown.DataBlocks.Enemies;

public record EnemyBalancing : DataBlock<EnemyBalancing>
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
            HealthMax = 200,
            DamageUntilHitreact = 50,
            BodypartHealth = 200,
            WeakspotDamageMulti = 2.5,
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

    /// <summary>
    /// Infested strikers are weaker than normal strikers (12 vs 20 health)
    /// but hit harder (4 vs 3 damage tentacle attack) to emphasise prioritizing
    /// them but at the expense of letting the babies build up in numbers and
    /// flooding with more fog
    /// </summary>
    public static readonly EnemyBalancing StrikerInfested = new()
    {
        Health = new()
        {
            HealthMax = 12,
            DamageUntilHitreact = 5,
            BodypartHealth = 10,
        },
        GlueTolerance = 4,
        GlueFadeOutTime = 8,
        CanBePushed = true,
        TentacleAttackDamage = 4.0,
        MeleeAttackDamage = 3.0
    };

    public static void Setup()
    {
        // Loads the base game balancing
        Setup<GameDataEnemyBalancing>(Bins.EnemyBalancing, "EnemyBalancing");

        NightmareGiant.Persist();
        StrikerInfested.Persist();
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
