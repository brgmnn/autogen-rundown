namespace AutogenRundown.DataBlocks.Enemies
{
    public record class EnemyInfo
    {
        public EnemyRole Role { get; set; }

        public Enemy Enemy { get; set; }

        public double Points { get; set; }

        #region Enemy definitions

        public static readonly EnemyInfo Striker      = new() { Role = EnemyRole.Melee,  Enemy = Enemy.Striker,      Points = 1.0 };
        public static readonly EnemyInfo StrikerGiant = new() { Role = EnemyRole.Melee,  Enemy = Enemy.StrikerGiant, Points = 4.0 };
        public static readonly EnemyInfo Shooter      = new() { Role = EnemyRole.Ranged, Enemy = Enemy.Shooter,      Points = 1.0 };
        public static readonly EnemyInfo ShooterGiant = new() { Role = EnemyRole.Ranged, Enemy = Enemy.ShooterGiant, Points = 4.0 };

        public static readonly EnemyInfo Charger      = new() { Role = EnemyRole.Lurker, Enemy = Enemy.Charger,      Points = 2.0 };
        public static readonly EnemyInfo ChargerGiant = new() { Role = EnemyRole.Lurker, Enemy = Enemy.ChargerGiant, Points = 4.0 };

        public static readonly EnemyInfo Shadow       = new() { Role = EnemyRole.Melee, Enemy = Enemy.Shadow,      Points = 1.0 };
        public static readonly EnemyInfo ShadowGiant  = new() { Role = EnemyRole.Melee, Enemy = Enemy.ShadowGiant, Points = 4.0 };

        public static readonly EnemyInfo Hybrid        = new() { Role = EnemyRole.PureSneak, Enemy = Enemy.Hybrid, Points = 3.0 };
        public static readonly EnemyInfo Hybrid_Hunter = new() { Role = EnemyRole.Hunter,    Enemy = Enemy.Hybrid, Points = 4.0 };

        public static readonly EnemyInfo NightmareShooter      = new() { Role = EnemyRole.Ranged, Enemy = Enemy.NightmareShooter,      Points = 1.0  };
        public static readonly EnemyInfo NightmareStriker      = new() { Role = EnemyRole.Melee,  Enemy = Enemy.NightmareStriker,      Points = 1.0  };
        public static readonly EnemyInfo NightmareStrikerGiant = new() { Role = EnemyRole.Ranged, Enemy = Enemy.NightmareStrikerGiant, Points = 10.0 };

        public static readonly EnemyInfo Flyer    = new() { Role = EnemyRole.Ranged, Enemy = Enemy.Flyer,    Points = 1.0  };
        public static readonly EnemyInfo FlyerBig = new() { Role = EnemyRole.Ranged, Enemy = Enemy.FlyerBig, Points = 10.0 };

        public static readonly EnemyInfo Mother           = new() { Role = EnemyRole.PureSneak, Enemy = Enemy.Mother,  Points = 10.0 };
        public static readonly EnemyInfo Mother_Hunter    = new() { Role = EnemyRole.Hunter,    Enemy = Enemy.Mother,  Points = 10.0 };
        public static readonly EnemyInfo Mother_MiniBoss  = new() { Role = EnemyRole.MiniBoss,  Enemy = Enemy.Mother,  Points = 10.0 };
        public static readonly EnemyInfo PMother          = new() { Role = EnemyRole.PureSneak, Enemy = Enemy.PMother, Points = 10.0 };
        public static readonly EnemyInfo PMother_Hunter   = new() { Role = EnemyRole.Hunter,    Enemy = Enemy.PMother, Points = 10.0 };
        public static readonly EnemyInfo PMother_MiniBoss = new() { Role = EnemyRole.Boss,      Enemy = Enemy.PMother, Points = 10.0 };
        public static readonly EnemyInfo MegaMother       = new() { Role = EnemyRole.PureSneak, Enemy = Enemy.MegaMother, Points = 40.0 };

        public static readonly EnemyInfo BirtherChild = new() { Role = EnemyRole.BirtherChild, Enemy = Enemy.Baby, Points = 1.0 };

        public static readonly EnemyInfo Tank          = new() { Role = EnemyRole.PureSneak, Enemy = Enemy.Tank, Points = 10.0 };
        public static readonly EnemyInfo Tank_Hunter   = new() { Role = EnemyRole.Hunter,    Enemy = Enemy.Tank, Points = 10.0 };
        public static readonly EnemyInfo Tank_MiniBoss = new() { Role = EnemyRole.MiniBoss,  Enemy = Enemy.Tank, Points = 10.0 };

        public static readonly EnemyInfo TankPotato    = new() { Role = EnemyRole.PureSneak, Enemy = Enemy.TankPotato, Points = 10.0 };

        public static readonly EnemyInfo Pouncer       = new() { Role = EnemyRole.Hunter,    Enemy = Enemy.Pouncer, Points = 4.0 };
        public static readonly EnemyInfo Pouncer_Sneak = new() { Role = EnemyRole.PureSneak, Enemy = Enemy.Pouncer, Points = 4.0 };
        #endregion

        // These are enemies that can be spawn aligned
        public static readonly List<EnemyInfo> SpawnAlignedBosses = new()
        {
            Mother,
            PMother,
            Tank,
            TankPotato,

            MegaMother,
            Pouncer_Sneak,
        };
    }
}
