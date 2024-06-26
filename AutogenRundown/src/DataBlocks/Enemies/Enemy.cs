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

        /** Hybrids */
        Hybrid = 33,


        /** Beserkers / Nightmare */
        NightmareCharger = 63,
        NightmareStriker = 53,
        NightmareStrikerGiant = 62,
        NightmareShooterGiant = 34,


        /** Bosses */
        /**  --> Tanks */
        Tank = 29,
        TankBoss = 47, // Immortal?
        TankPotato = 33,

        /**  --> Mothers */
        Mother = 36,
        PMother = 37,
        MegaMother = 55,
        Baby = 38,

        /**  --> Pouncer */
        Pouncer = 46,


        /** Scouts */
        Scout = 20,
        ScoutShadow = 40,
        ScoutCharger = 41,
        ScoutZoomer = 54,
        ScoutNightmare = 56,


        /** Flyers */
        Flyer = 42,
        FlyerBig = 45,

        /** Cocoon */
        Cocoon = 22,
    }
}
