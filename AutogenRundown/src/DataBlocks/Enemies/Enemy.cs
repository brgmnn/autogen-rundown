namespace AutogenRundown.DataBlocks.Enemies
{
    public enum Enemy : uint
    {
        /** Wave enemies
         * (these are the red variants) */
        Shooter_Wave = 11,
        Striker_Wave = 13,
        StrikerGiant_Wave = 16,

        /** Regular sleepers and giants
         * (these are the white sleeping variants) */
        Striker = 24,
        Shooter = 26,
        StrikerGiant = 28,
        ShooterGiant = 18,

        StrikerBoss = 19,

        /** Chargers */
        Charger = 30,
        ChargerGiant = 39,

        /** Shadows */
        Shadow = 21,
        ShadowGiant = 35,

        #region Hybrids
        /// <summary>
        /// Regular hybrids
        /// </summary>
        Hybrid = 33,

        /// <summary>
        /// This enemy was never used in the base game. It uses the ShooterGiant model but moves
        /// like a hybrid and shoots giant shooter projectiles that deal infection rather than
        /// damage.
        ///
        /// Base game calls it a Nightmare Shooter Giant, we will call it something closer to what
        /// it is
        /// </summary>
        ShooterGiant_Infected = 34,
        #endregion


        #region Nightmare enemies
        /** Beserkers / Nightmare */
        /// <summary>
        /// Nightmare shooter, weirdly has a tounge attack
        /// </summary>
        NightmareShooter = 52,

        /// <summary>
        /// Nightmare striker, acts a bit like a charger
        /// </summary>
        NightmareStriker = 53,

        /// <summary>
        /// Unused in the base game, it looks very goofy
        /// </summary>
        NightmareBaby = 63,

        /// <summary>
        /// This is the Potato tank
        /// </summary>
        NightmareStrikerGiant = 62,
        #endregion

        /** Bosses */
        /**  --> Tanks */
        Tank = 29,
        TankBoss = 47, // Immortal?
        TankPotato = NightmareStrikerGiant,

        /**  --> Mothers */
        Mother = 36,
        PMother = 37,
        MegaMother = 55, // tbd verify
        Baby = 38,

        /**  --> Pouncer */
        Pouncer = 46,


        /** Scouts */
        Scout = 20,
        ScoutShadow = 40,
        ScoutCharger = 41,
        ScoutZoomer = 54, // tbd verify
        ScoutNightmare = 56, // tbd verify


        /** Flyers */
        Flyer = 42,
        FlyerBig = 45,

        /** Cocoon */
        Cocoon = 22,
    }
}
