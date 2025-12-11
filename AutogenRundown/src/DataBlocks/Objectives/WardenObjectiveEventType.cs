namespace AutogenRundown.DataBlocks.Objectives;

public enum WardenObjectiveEventType : uint
{
    None = 0,
    OpenSecurityDoor = 1,
    UnlockSecurityDoor = 2,
    AllLightsOff = 3,
    AllLightsOn = 4,
    PlaySound = 5,
    SetFogSettings = 6,
    DimensionFlashTeam = 7,
    DimensionWarpTeam = 8,
    SpawnEnemyWave = 9,
    StopEnemyWaves = 10,
    UpdateCustomSubObjective = 11,
    ForceCompleteObjective = 12,
    LightsInZone = 13,
    LightsInZoneToggle = 14,
    AnimationTrigger = 15,
    SpawnEnemyOnPoint = 16,
    SetNavMarker = 17,
    StepProgressionObjective = 18,
    SetWorldEventCondition = 19,
    LockSecurityDoor_Base = 20,
    SetTerminalCommand = 21,
    ActivateChainedPuzzle = 22,
    LightOnWorldEventObjective = 23,

    // New R8 events (we think)
    AddToTimer = 24,
    ResetTimer = 25,
    WinOnDeath = 26,
    ForceInstantWin = 27,
    DialogueOnClosest = 28,
    GetAchievement = 29,
    ClearDimension = 30,
    StartRepeatingFog = 31,
    StopSustainedEvent = 32,

    EventBreak = 999,


    #region MOD: EOSExt_SecuritySensor
    /// <summary>
    /// Toggle a security sensor group state
    /// </summary>
    ToggleSecuritySensor = 400,
    #endregion

    #region MOD: AdvancedWardenObjective
    /**
     * This region covers AWO (AdvancedWardenObjective) events. These give a plethora of new events
     * to use in the rundown giving a lot of fun flexibility in designing levels.
     */

    #region --- Modder: Flow's Events ---

    LockSecurityDoor = 10001,
    ForceCompleteLevel = 10008,
    Countdown = 10010,
    SetLightDataInZone = 10016,
    AlertEnemiesInZone = 10017,

    #endregion

    #region --- Modder: Hirnu's Events ---
    #endregion

    #region --- Modder: Armor's Events ---
    StartEventLoop = 20001,
    StopEventLoop = 20002,

    AdjustAwoTimer = 20007,

    ShakeScreen = 20011,

    CustomHudText = 20017,
    #endregion
    #endregion
}
