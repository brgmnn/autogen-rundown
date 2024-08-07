﻿namespace AutogenRundown.DataBlocks
{
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

        #region Environment noises
        Environment_DistantMetalImpacts = 3865016528,

        Environment_DoorUnstuck = 104566516,
        Environment_DoorLoosen = 836335444,

        Environment_DistantFan = 3164826086,

        Alarms_Error = 2200133294,
        Alarms_ErrorOff = 1190355274,

        // Sounds like a fuse blowing
        Environment_PowerdownFailure = 3655606696,
        #endregion

        SheetMetalLand = 157965313,
        DistantFanBlade = 166915794,

        #region Enemies
        Enemies_DistantLowRoar = 3344868585,

        PouncerSpawnGrowl = 3503733109,
        TankRoar = 106273434,
        #endregion
    }
}
