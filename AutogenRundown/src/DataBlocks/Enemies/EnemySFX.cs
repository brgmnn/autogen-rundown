using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Enemies;

public record EnemySFX : DataBlock<EnemySFX>
{
    #region Properties

    public Vector2 hibernateIdleInterval { get; set; } = new() { X = 0, Y = 0 };

    [JsonProperty("SFX_ID_walk")]
    public uint Walk { get; set; } = 0;

    [JsonProperty("SFX_ID_run")]
    public uint Run { get; set; } = 0;

    [JsonProperty("SFX_ID_climbLadder")]
    public uint ClimbLadder { get; set; } = 0;

    [JsonProperty("SFX_ID_heartbeatPulse")]
    public uint HeartbeatPulse { get; set; } = 0;

    [JsonProperty("SFX_ID_hibernateIdle")]
    public uint HibernateIdle { get; set; } = 0;

    [JsonProperty("SFX_ID_hibernateWakeUp")]
    public uint HibernateWakeUp { get; set; } = 0;

    [JsonProperty("SFX_ID_hibernateDie")]
    public uint HibernateDie { get; set; } = 0;

    [JsonProperty("SFX_ID_hibernateDetectionStart")]
    public uint HibernateDetectionStart { get; set; } = 0;

    [JsonProperty("SFX_ID_scream")]
    public uint Scream { get; set; } = 0;

    [JsonProperty("SFX_ID_stuckInGlue")]
    public uint StuckInGlue { get; set; } = 0;

    [JsonProperty("SFX_ID_releaseFromGlue")]
    public uint ReleaseFromGlue { get; set; } = 0;

    [JsonProperty("SFX_ID_attackWindUp")]
    public uint AttackWindUp { get; set; } = 0;

    [JsonProperty("SFX_ID_attackWindUp_NotLocalTarget")]
    public uint AttackWindUp_NotLocalTarget { get; set; } = 0;

    [JsonProperty("SFX_ID_attackFire")]
    public uint AttackFire { get; set; } = 0;

    [JsonProperty("SFX_ID_hurtSmall")]
    public uint HurtSmall { get; set; } = 0;

    [JsonProperty("SFX_ID_hurtBig")]
    public uint HurtBig { get; set; } = 0;

    [JsonProperty("SFX_ID_die")]
    public uint Die { get; set; } = 0;

    [JsonProperty("SFX_ID_JumpStart")]
    public uint JumpStart { get; set; } = 0;

    [JsonProperty("SFX_ID_JumpInAir")]
    public uint JumpInAir { get; set; } = 0;

    [JsonProperty("SFX_ID_JumpLand")]
    public uint JumpLand { get; set; } = 0;

    [JsonProperty("SFX_ID_BigTentacleStart")]
    public uint BigTentacleStart { get; set; } = 0;

    [JsonProperty("SFX_ID_BigTentacleTipLoop")]
    public uint BigTentacleTipLoop { get; set; } = 0;

    [JsonProperty("SFX_ID_BigTentacleEnd")]
    public uint BigTentacleEnd { get; set; } = 0;

    #endregion

    public EnemySFX(PidOffsets offsets = PidOffsets.Normal)
        : base(Generator.GetPersistentId(offsets))
    { }

    public void Persist(BlocksBin<EnemySFX>? bin = null)
    {
        bin ??= Bins.EnemySFXs;
        bin.AddBlock(this);
    }

    public static readonly EnemySFX NightmareGiant = new()
    {
        Walk = 1201976284,
        Run = 1727064594,
        ClimbLadder = 1923878719,
        HeartbeatPulse = (uint)Sound.EnemyHeartbeatLarge, // 2209321909, // From Giant Striker
        HibernateIdle = 36402231,
        HibernateWakeUp = 3595781181,
        HibernateDie = 3356588661,
        HibernateDetectionStart = 4055144979,
        Scream = 835661530,
        StuckInGlue = 2161386474,
        ReleaseFromGlue = 1097708630,
        HurtSmall = 2161386474,
        HurtBig = 2161386474,
        Die = 3356588661,
        JumpStart = 3391976123,
        JumpLand = 3296246808
    };

    public static void Setup()
    {
        // Loads the base game balancing
        Setup<GameDataEnemySFX>(Bins.EnemySFXs, "EnemySFX");

        NightmareGiant.Persist();
    }
}

public record GameDataEnemySFX : EnemySFX
{
    /// <summary>
    /// We explicitly want to not have PIDs set when loading data, they come with their own
    /// Pids. This stops the Pid counter getting incremented when loading vanilla data.
    /// </summary>
    public GameDataEnemySFX() : base(PidOffsets.None)
    { }
}
