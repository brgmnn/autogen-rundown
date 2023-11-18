namespace AutogenRundown.DataBlocks.Items
{
    /// <summary>
    /// A full list of all items.
    /// </summary>
    public enum Item : uint
    {
        None = 0,

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

        Syringe_Health = 140,
        Syringe_Melee = 142,
        #endregion
    }
}
