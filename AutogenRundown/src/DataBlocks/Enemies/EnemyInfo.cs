namespace AutogenRundown.DataBlocks.Enemies
{
    public record class EnemyInfo
    {
        public EnemyRole Role { get; set; }

        public Enemy Enemy { get; set; }

        public double Points { get; set; }

        public static EnemyInfo Striker = new EnemyInfo { Role = EnemyRole.Melee, Enemy = Enemy.Striker, Points = 1.0 };
        public static EnemyInfo StrikerGiant = new EnemyInfo { Role = EnemyRole.Melee, Enemy = Enemy.StrikerGiant, Points = 4.0 };
        public static EnemyInfo Shooter = new EnemyInfo { Role = EnemyRole.Ranged, Enemy = Enemy.Shooter, Points = 1.0 };
        public static EnemyInfo ShooterGiant = new EnemyInfo { Role = EnemyRole.Ranged, Enemy = Enemy.ShooterGiant, Points = 4.0 };

        public static EnemyInfo Charger = new EnemyInfo { Role = EnemyRole.Lurker, Enemy = Enemy.Charger, Points = 2.0 };
        public static EnemyInfo ChargerGiant = new EnemyInfo { Role = EnemyRole.Lurker, Enemy = Enemy.ChargerGiant, Points = 4.0 };

        public static EnemyInfo Shadow = new EnemyInfo { Role = EnemyRole.Melee, Enemy = Enemy.Shadow, Points = 1.0 };
        public static EnemyInfo ShadowGiant = new EnemyInfo { Role = EnemyRole.Melee, Enemy = Enemy.ShadowGiant, Points = 4.0 };

        public static EnemyInfo Hybrid = new EnemyInfo { Role = EnemyRole.PureSneak, Enemy = Enemy.Hybrid, Points = 3.0 };
        public static EnemyInfo Hybrid_Hunter = new EnemyInfo { Role = EnemyRole.Hunter, Enemy = Enemy.Hybrid, Points = 4.0 };

        public static EnemyInfo Mother = new EnemyInfo { Role = EnemyRole.PureSneak, Enemy = Enemy.Mother, Points = 10.0 };
        public static EnemyInfo Mother_Hunter = new EnemyInfo { Role = EnemyRole.Hunter, Enemy = Enemy.Mother, Points = 10.0 };
        public static EnemyInfo Mother_MiniBoss = new EnemyInfo { Role = EnemyRole.MiniBoss, Enemy = Enemy.Mother, Points = 10.0 };
        public static EnemyInfo PMother = new EnemyInfo { Role = EnemyRole.PureSneak, Enemy = Enemy.PMother, Points = 10.0 };
        public static EnemyInfo PMother_Hunter = new EnemyInfo { Role = EnemyRole.Hunter, Enemy = Enemy.PMother, Points = 10.0 };
        public static EnemyInfo PMother_MiniBoss = new EnemyInfo { Role = EnemyRole.Boss, Enemy = Enemy.PMother, Points = 10.0 };

        public static EnemyInfo BirtherChild = new EnemyInfo { Role = EnemyRole.BirtherChild, Enemy = Enemy.Baby, Points = 1.0 };

        public static EnemyInfo Tank = new EnemyInfo { Role = EnemyRole.PureSneak, Enemy = Enemy.Tank, Points = 10.0 };
        public static EnemyInfo Tank_Hunter = new EnemyInfo { Role = EnemyRole.Hunter, Enemy = Enemy.Tank, Points = 10.0 };
        public static EnemyInfo Tank_MiniBoss = new EnemyInfo { Role = EnemyRole.MiniBoss, Enemy = Enemy.Tank, Points = 10.0 };

        public static EnemyInfo Pouncer = new EnemyInfo { Role = EnemyRole.Hunter, Enemy = Enemy.Pouncer, Points = 1.0 };
    }
}
