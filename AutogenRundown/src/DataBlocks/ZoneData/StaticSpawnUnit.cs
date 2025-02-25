namespace AutogenRundown.DataBlocks.ZoneData;

public enum StaticSpawnUnit : uint
{
    None = 0,

    /// <summary>
    /// The infectious Spitter. A classic
    /// </summary>
    Spitter = 1,

    /// <summary>
    /// The mother sacks. A classic
    /// </summary>
    EggSack = 3,

    /// <summary>
    /// Do not use this unit, it doesn't seem finished.
    ///
    /// The model is very messed up but it appears to function kind of like a black colored
    /// spitter that slows the player instead of adding infection
    /// </summary>
    [Obsolete("This unit doesn't look finished in the game")]
    Anemone = 5,

    /// <summary>
    /// This is the black enemy sack that you see in respawn rooms. Note that it only works if
    /// Zone.EnemyRespawning is set to `true`.
    ///
    /// Respawning _does not_ need this to be configured though, it will already spawn respawners
    /// in the roof for you. So this one is basically useless unless you want MORE respawners.
    ///
    /// Base game doesn't use this at all and just seems to rely on the game spawning them
    /// automatically.
    /// </summary>
    [Obsolete("This unit is not used at all in the base game (manually)")]
    Respawner = 6,

    /// <summary>
    /// Very creepy looking red/brown corpses. Used in R8B1
    /// </summary>
    Corpses = 8,

    /// <summary>
    /// Tentacle base, looks like a smallish ball with blackened holes that tentacles come out of.
    /// Used in R8B1
    /// </summary>
    Parasite = 9
}
