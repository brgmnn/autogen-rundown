namespace AutogenRundown.DataBlocks.Levels;

public enum LevelModifiers
{
    /// <summary>
    /// Whether the level should be dark or not
    /// </summary>
    Darkness,

    #region Fog modifiers
    /// <summary>
    /// Will set the fog settings to clear resulting in R1A1 fog.
    /// </summary>
    NoFog,

    /// <summary>
    /// Will set the level to have some fog zones, (occupying one altitude layer).
    /// </summary>
    Fog,

    /// <summary>
    /// Level will be heavy fog, (occupying two altitude layers).
    /// </summary>
    HeavyFog,

    /// <summary>
    /// Set's fog to be infectious.
    /// </summary>
    FogIsInfectious,
    #endregion

    #region Enemy modifiers
    NoChargers,
    Chargers,
    ManyChargers,
    OnlyChargers,

    NoFlyers,
    Flyers,
    ManyFlyers,
    OnlyFlyers,

    NoNightmares,
    Nightmares,
    ManyNightmares,
    OnlyNightmares,

    NoShadows,
    Shadows,
    ManyShadows,
    OnlyShadows,

    Hybrids,
    InfectionHybrids,
    #endregion

    #region Spitters

    NoSpitters,
    Spitters,
    ManySpitters,

    #endregion

    #region Infection

    NoInfection,
    Infection,
    HeavyInfection,

    #endregion

    #region RespawnCocoons

    NoRespawnCocoons,
    RespawnCocoons,

    #endregion
}
