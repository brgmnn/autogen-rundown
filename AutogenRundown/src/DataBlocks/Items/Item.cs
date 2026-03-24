namespace AutogenRundown.DataBlocks.Items;

/// <summary>
/// A full list of all items.
/// </summary>
public enum Item : uint
{
    None = 0,

    #region Big pickups
    PowerCell = 131,

    FogTurbine = 133,

    // Objective big items

    #region Objectives

    // --- Neonate HSU ---
    /// <summary>
    /// Fully sealed and powered off
    /// </summary>
    NeonateHsu_Stage1 = 137,

    /// <summary>
    /// Fully enclosed, like stage 1 but the window is visible with glowing activated text on it
    /// </summary>
    NeonateHsu_Stage2 = 141,

    /// <summary>
    /// Opened up, but the baby is still vacuum sealed and not moving
    /// </summary>
    NeonateHsu_Stage3 = 143,

    /// <summary>
    /// You can clearly see the baby here
    /// </summary>
    NeonateHsu_Stage4 = 170,

    // --- Data Sphere ---
    DataSphere = 151,

    #endregion


    Crate1 = 154,
    Crate2 = 175,
    Data2 = 181,

    MatterWaveProjector = 166,
    CargoCrate = 138,
    CargoCrateHighSecurity = 154,
    CryoCase = 148,
    #endregion

    #region Consumables
    LongRangeFlashlight = 30,

    GlowStick = 114,
    /// <summary>
    /// Glowstick: Christmas = red
    /// </summary>
    GlowStick_Christmas = 130,

    /// <summary>
    /// Glowstick: Halloween = orange
    /// </summary>
    GlowStick_Halloween = 167,

    /// <summary>
    /// Glowstick: yellow = yellow (very surprising)
    /// </summary>
    GlowStick_Yellow = 174,

    CfoamGrenade = 115,
    CfoamTripmine = 144,
    ExplosiveTripmine = 139,

    LockMelter = 116,

    FogRepeller = 117,

    /// <summary>
    /// I2-LP Heal Syringe
    ///
    /// GTFriendlyO stats:
    /// +10-20% Health, +5% Disinfection
    /// </summary>
    Syringe_Health = 140,

    /// <summary>
    /// IIx Melee Syringe
    ///
    /// GTFriendlyO stats:
    /// Melee buff (300s), +1.1x move speed (900s), 0-10% Infection
    /// </summary>
    Syringe_Melee = 142,
    #endregion

    #region === MODs: Carb_Crusaders/GTFriendlyO
    // Key Themes
    //
    // - Pure utility: 140 (heal), 229 (disinfect), 271 (heal+disinfect+regen)
    // - Risk/reward tradeoffs: 225 (speed+regen but infection), 230 (full heal but delayed DOT), 232 (massive buffs but kills you at 600s), 233 (ammo refill but DOT+infection)
    // - Resource conversion: 227 drains ammo/tool for health+disinfect
    // - Suicide bombs: 228 (small bomb) and 234 (big nuke) — self-destruct with area damage
    // - Speed buffs: 224 (fast burst), 225 (moderate sustained), 142 (long duration)
    // - Recovery: 231 (slow but safe regen) and 241 (better regen, more speed penalty)
    //
    // IDs 140 and 142 are vanilla game syringes; all others (224+) are custom additions from the GTFriendlyO mod.

    /// <summary>
    /// Speed Syringe
    ///
    /// +1.4x move speed (60s), 5-10% Infection
    /// </summary>
    ModGtfriendly_Syringe_Speed = 224,

    /// <summary>
    /// Adrenaline Syringe
    ///
    /// +1.15x move speed (120s), 5-15% Infection, full health regen over 10s
    /// </summary>
    ModGtfriendly_Syringe_Adrenaline = 225,

    /// <summary>
    /// Heal Munitions Drain
    ///
    /// -20% Ammo & Tool, health regen (200s), +30% Disinfection
    /// </summary>
    ModGtfriendly_Syringe_HealMunitionsDrain = 227,

    /// <summary>
    /// Virus Bomb
    ///
    /// +25% Infection, self-destruct after 15s (350 dmg, 7.5m radius)
    /// </summary>
    ModGtfriendly_Syringe_VirusBomb  = 228,

    /// <summary>
    /// Antibiotic Syringe
    ///
    /// +30% Disinfection (pure cure)
    /// </summary>
    ModGtfriendly_Syringe_Antibiotic = 229,

    /// <summary>
    /// Health Surge
    ///
    /// +100% Health instantly, then damage-over-time (0.1/s for 300s after 30s delay)
    /// </summary>
    ModGtfriendly_Syringe_HealthSurge = 230,

    /// <summary>
    /// Recovery Syringe
    ///
    /// 0.9x move speed (180s), slow health regen (0.2 HP every 3s for 180s)
    /// </summary>
    ModGtfriendly_Syringe_Recovery = 231,

    /// <summary>
    /// Rage Syringe
    ///
    /// +50% Health, melee buff (600s), +1.1x speed (600s), +25% Infection, instant-kill damage (30) after 600s
    /// </summary>
    ModGtfriendly_Syringe_Rage = 232,

    /// <summary>
    /// Ammo Symbiotic Regurgitant
    ///
    /// +35% Ammo & Tool, damage-over-time (0.6/s for 70s after 10s delay), +30% Infection
    /// </summary>
    ModGtfriendly_Syringe_AmmoSymbiotic = 233,

    /// <summary>
    /// Virus Nuke
    ///
    /// +50% Infection, self-destruct after 10s (700 dmg, 15m radius)
    /// </summary>
    ModGtfriendly_Syringe_VirusNuke = 234,

    /// <summary>
    /// Recovery Syringe II
    ///
    /// 0.85x move speed (180s), better health regen (0.4 HP every 3s for 180s)
    /// </summary>
    ModGtfriendly_Syringe_Recovery2 = 241,

    /// <summary>
    /// Antibiotic-IX-REC
    ///
    /// +30% Disinfection, +25% Health, massive regen (2.0 HP/s for 10s)
    /// </summary>
    ModGtfriendly_Syringe_AntibioticIX = 271,

    #endregion
}
