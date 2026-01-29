namespace AutogenRundown.DataBlocks;

/// <summary>
/// A lot of the interesting sounds can be found under: sfx\battery_station
/// </summary>
public enum Sound : uint
{
    None = 0,

    /// <summary>
    /// Loud electrical sound of lights turning off
    /// </summary>
    LightsOff = 1479064690,

    /// <summary>
    /// Generator / Heavy fan noise
    /// </summary>
    KdsDeepVentilationProcedure = 2591647810,

    ReactorShutdown = 2741402798,

    Warp = 1998147319,
    WarpReality = 231201012,

    // Objective update ping
    ObjectiveNoise = 979647287,


    #region R8C2 Unplugged

    /// <summary>
    /// R8C2
    /// </summary>
    R8C2_Startupshort = 2578878269,

    /// <summary>
    /// Sounds like a short mic cutout
    ///
    /// Seems to have multiple versions
    /// </summary>
    StaticCutout_Short =  469352613,

    StaticCutout_Medium = 2575020920,

    // Schaeffer saying fuck = 380255615

    /// <summary>
    /// Robot voice: "Message Sent"
    /// </summary>
    Hearsay_MessageSent = 507879784,


    /// <summary>
    /// Short noise played just before the stack empty success screen
    ///
    /// Duration = 1.0 seconds
    /// </summary>
    Warden_ChipRemove = 3869511737,


    /// <summary>
    /// Also sounds:
    ///     * 2871316840
    ///     * 2952136044
    /// </summary>
    Woooo_Machine1 = 2871316841,

    #endregion

    #region R8E1 Valiant

    MachineryBlow = 1007447703,

    // This seems like it's supposed to be the other team shooting
    Gunfire = 3665191083,

    DramaticTension = 1819247891,

    MonsterNoises = 637231409,

    R8E1_ErrorAlarm = 1068424543,

    R8E1_GargantaWarning = 3030964334,

    #endregion


    #region Alarms
    Alarms_MissingItem = 2200133294, // decon_unit_missing_alarm

    /// <summary>
    /// This is also the error alarm loop noise played when dropping into R2C2 ???
    ///
    /// This sound will loop. Play the `Alarms_Error_AmbientStop` to turn off this loop noise
    /// </summary>
    Alarms_Error_AmbientLoop = 1190355274,   // alarm_ambient_loop

    /// <summary>
    /// Play this sound to stop Alarms_Error_AmbientLoop
    /// </summary>
    Alarms_Error_AmbientStop = 560124168,

    Alarms_AmbientMix = 42582178,
    Alarms_AmbientStereo = 439388169, // Sounds like nothing?

    #endregion

    #region Atmosphere and mood
    /// <summary>
    /// This is a tense deep ominous sound. Played when you enter the R3B2 mother room
    /// </summary>
    TenseRevelation = 1819247891,
    #endregion

    #region Liquid and Disinfect

    FoleyDisinfectPack = 2543859690,

    #endregion

    #region Environment noises
    Environment_DistantMetalImpacts = 3865016528,

    /// <summary>
    /// Sounds like a big loud power generator starting
    /// </summary>
    Environment_KdsDeepHsuPowerup = 2081519152,

    Environment_DoorUnstuck = 104566516,
    Environment_DoorLoosen = 836335444,

    Environment_DistantFan = 3164826086,

    Environment_MetalFanDeepCreak_04 = 207117837,

    // Sounds like a fuse blowing
    Environment_PowerdownFailure = 3655606696,
    #endregion

    #region Environment: Electrical System and Power

    Sparks = 2380278191,

    LightsOn_Vol1 = 3206896764, // Not too loud
    LightsOn_Vol2 = 3206896767, // Decently loud but ok
    LightsOn_Vol3 = 3206896766, // Still quite loud
    LightsOn_Vol4 = 3206896761, // Very loud

    #endregion

    SheetMetalLand = 157965313,
    DistantFanBlade = 166915794,

    #region Enemies
    Enemies_DistantLowRoar = 3344868585,

    PouncerSpawnGrowl = 3503733109,
    // TankRoar = 106273434,
    // TankRoar = 1641625378,
    TankRoar = 2134610730,

    /// <summary>
    /// A relatively high-pitched long roar
    /// </summary>
    BossRoar = 2134610730,

    EnemyHeartbeat = 2209321909,
    EnemyHeartbeatLarge = 3978814126

    #endregion
}
