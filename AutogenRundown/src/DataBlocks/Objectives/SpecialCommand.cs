namespace AutogenRundown.DataBlocks.Objectives;

public enum SpecialCommand
{
    None = 0,

    ///
    /// Light based commands
    ///
    LightsOff,

    ///
    /// Fog based commands
    ///
    FillWithFog,
    //FillWithInfectiousFog = 3,

    ///
    /// Alarm commands
    ///
    ErrorAlarm,

    ///
    /// King of the Hill
    ///
    /// This is a room wide scan
    ///
    KingOfTheHill
}
