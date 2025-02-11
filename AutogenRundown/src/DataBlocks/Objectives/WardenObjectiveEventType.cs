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
    LockSecurityDoor = 20,
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


    #region MOD: AdvancedWardenObjective
    /**
     * This region covers AWO (AdvancedWardenObjective) events. These give a plethora of new events
     * to use in the rundown giving a lot of fun flexibility in designing levels.
     */

    SetLightDataInZone = 10016,

    StartEventLoop = 20001,
    StopEventLoop = 20002,
    #endregion
}
