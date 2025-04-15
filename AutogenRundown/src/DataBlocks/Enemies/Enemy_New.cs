using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Abilities;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Materials;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Enemies;

public record Enemy_New : DataBlock
{
    #region Properties

    /*
     * We probably aren't going to be modifying any of these. They're just
     * here, so we can copy enemies for the purposes of customization via
     * the EEC mod
     */
    public int EnemyType { get; set; }
    public int AssetBundle { get; set; }
    public int BundleShard { get; set; }
    public List<string> BasePrefabs { get; set; } = new();
    public JArray ModelDatas { get; set; } = new();
    public int DetectionDataId { get; set; }
    public int BehaviorDataId { get; set; }
    public int MovementDataId { get; set; }
    public int BalancingDataId { get; set; }
    public int SFXDataId { get; set; }
    public JArray ArenaDimensions { get; set; } = new();
    public JArray LinkedSlaveModels { get; set; } = new();
    public int InternalMaterial { get; set; }
    public bool isCoccoon { get; set; }
    public int EnemySpottedDialogId { get; set; }
    public JArray AI_Abilities { get; set; } = new();

    #endregion

    #region --- Enemy references ---
    #region Base game enemies

    /** Wave enemies
     * (these are the red variants) */
    public static Enemy_New Shooter_Wave { get; set; } = new() { PersistentId = 11 };
    public static Enemy_New Striker_Wave { get; set; } = new() { PersistentId = 13 };
    public static Enemy_New StrikerGiant_Wave { get; set; } = new() { PersistentId = 16 };

    /** Regular sleepers and giants
     * (these are the white sleeping variants) */
    public static Enemy_New Striker { get; set; } = new() { PersistentId = 24 };
    public static Enemy_New Shooter { get; set; } = new() { PersistentId = 26 };
    public static Enemy_New StrikerGiant { get; set; } = new() { PersistentId = 28 };
    public static Enemy_New ShooterGiant { get; set; } = new() { PersistentId = 18 };

    public static Enemy_New StrikerBoss { get; set; } = new() { PersistentId = 19 };

    /** Chargers */
    public static Enemy_New Charger { get; set; } = new() { PersistentId = 30 };
    public static Enemy_New ChargerGiant { get; set; } = new() { PersistentId = 39 };

    /** Shadows */
    public static Enemy_New Shadow { get; set; } = new() { PersistentId = 21 };
    public static Enemy_New ShadowGiant { get; set; } = new() { PersistentId = 35 };

    #region Hybrids
    /// <summary>
    /// Regular hybrids
    /// </summary>
    public static Enemy_New Hybrid { get; set; } = new() { PersistentId = 33 };

    /// <summary>
    /// This enemy was never used in the base game. It uses the ShooterGiant model but moves
    /// like a hybrid and shoots giant shooter projectiles that deal infection rather than
    /// damage.
    ///
    /// Base game calls it a Nightmare Shooter Giant, we will call it something closer to what
    /// it is
    /// </summary>
    public static Enemy_New ShooterGiant_Infected { get; set; } = new() { PersistentId = 34 };
    #endregion

    #region Nightmare enemies
    /** Beserkers / Nightmare */
    /// <summary>
    /// Nightmare shooter, weirdly has a tounge attack
    /// </summary>
    public static Enemy_New NightmareShooter { get; set; } = new() { PersistentId = 52 };

    /// <summary>
    /// Nightmare striker, acts a bit like a charger
    /// </summary>
    public static Enemy_New NightmareStriker { get; set; } = new() { PersistentId = 53 };

    /// <summary>
    /// Unused in the base game, it looks very goofy
    /// </summary>
    public static Enemy_New NightmareBaby { get; set; } = new() { PersistentId = 63 };
    #endregion

    /** Bosses */
    /**  --> Tanks */
    public static Enemy_New Tank { get; set; } = new() { PersistentId = 29 };
    public static Enemy_New TankBoss { get; set; } = new() { PersistentId = 47 };

    /// <summary>
    /// Called NightmareStrikerGiant in the base game files
    /// </summary>
    public static Enemy_New TankPotato { get; set; } = new() { PersistentId = 62 };

    /**  --> Mothers */
    public static Enemy_New Mother { get; set; } = new() { PersistentId = 36 };
    public static Enemy_New PMother { get; set; } = new() { PersistentId = 37 };
    public static Enemy_New MegaMother { get; set; } = new() { PersistentId = 55 };
    public static Enemy_New Baby { get; set; } = new() { PersistentId = 38 };

    /**  --> Pouncer */
    public static Enemy_New Pouncer { get; set; } = new() { PersistentId = 46 };


    /** Scouts */
    public static Enemy_New Scout { get; set; } = new() { PersistentId = 20 };
    public static Enemy_New ScoutShadow { get; set; } = new() { PersistentId = 40 };
    public static Enemy_New ScoutCharger { get; set; } = new() { PersistentId = 41 };
    public static Enemy_New ScoutZoomer { get; set; } = new() { PersistentId = 54 };
    public static Enemy_New ScoutNightmare { get; set; } = new() { PersistentId = 56 };


    /** Flyers */
    public static Enemy_New Flyer { get; set; } = new() { PersistentId = 42 };
    public static Enemy_New FlyerBig { get; set; } = new() { PersistentId = 45 };

    /** Cocoon */
    public static Enemy_New Cocoon { get; set; } = new() { PersistentId = 22 };

    #endregion

    #region Autogen enemies

    /// <summary>
    /// This is a shadow version of the pouncer
    /// </summary>
    public static Enemy_New PouncerShadow { get; set; } = new() { PersistentId = 0 };

    /// <summary>
    /// Infection hybrid
    /// </summary>
    public static Enemy_New HybridInfected { get; set; } = new() { PersistentId = 0 };

    #endregion
    #endregion

    public Enemy_New(PidOffsets offsets = PidOffsets.Enemy)
        : base(Generator.GetPersistentId(offsets))
    { }

    public static Enemy_New Duplicate(Enemy_New other)
    {
        var dupe = other with
        {
            PersistentId = Generator.GetPersistentId(PidOffsets.Enemy)
        };

        dupe.Persist();

        return dupe;
    }

    public void Persist(BlocksBin<Enemy_New>? bin = null)
    {
        bin ??= Bins.Enemy;
        bin.AddBlock(this);
    }

    public static Enemy_New Find(uint persistentId) => Bins.Enemy.Find(persistentId)!;

    public static void Setup()
    {
        // Loads the base game enemies
        Setup<GameDataEnemy, Enemy_New>(Bins.Enemy, "Enemy");

        #region --- Reference the base game enemies ---
        Shooter_Wave = Find(11);
        Striker_Wave = Find(13);
        StrikerGiant_Wave = Find(16);

        Striker = Find(24);
        Shooter = Find(26);
        StrikerGiant = Find(28);
        ShooterGiant = Find(18);

        StrikerBoss = Find(19);

        Charger = Find(30);
        ChargerGiant = Find(39);

        Shadow = Find(21);
        ShadowGiant = Find(35);

        Hybrid = Find(33);

        ShooterGiant_Infected = Find(34);

        NightmareShooter = Find(52);
        NightmareStriker = Find(53);
        NightmareBaby = Find(63);

        Tank = Find(29);
        TankBoss = Find(47);
        TankPotato = Find(62);

        Mother = Find(36);
        PMother = Find(37);
        MegaMother = Find(55);
        Baby = Find(38);

        Pouncer = Find(46);

        Scout = Find(20);
        ScoutShadow = Find(40);
        ScoutCharger = Find(41);
        ScoutZoomer = Find(54);
        ScoutNightmare = Find(56);

        Flyer = Find(42);
        FlyerBig = Find(45);

        Cocoon = Find(22);
        #endregion

        PouncerShadow = Duplicate(Pouncer);
        HybridInfected = Duplicate(Hybrid);

        #region MOD: ExtraEnemyCustomization (EEC)
        EnemyCustomization.Setup();
        EnemyCustomization.Ability.Setup();
        EnemyCustomization.Model.Setup();
        #endregion

        #region Base game enemy customization
        #region --- Mega Mother ---
        // This is to customize the MegaMom birthing ability, as by default it
        // is set up to spawn chargers, and it's extremely hard. This configures
        // it to be more like the R8 MegaMom fight
        //
        // For now we have it birthing both strikers and babies. 5 strikers, 7 babies per birth

        var group = new EnemyGroup
        {
            Type = EnemyGroupType.Awake,
            Difficulty = (uint)AutogenDifficulty.MegaMotherSpawn,
            MaxScore = 18.0,
            Roles = new List<EnemyGroupRole>
            {
                new() { Role = EnemyRole.BirtherChild, Distribution = EnemyRoleDistribution.Rel100 }
            }
        };
        group.Persist();

        // Striker births
        EnemyCustomization.Ability.Birthings.Add(
            new Birthing
            {
                Target = new Target
                {
                    Mode = Mode.PersistentId,
                    PersistentIds = new() { MegaMother.PersistentId }
                },
                EnemyGroupToSpawn = group.PersistentId,
                ChildrenCost = 1.0,
                ChildrenMax = 16,
                ChildrenPerBirth = 12,
                ChildrenPerBirthMin = 6,
                MaxDelayUntilNextBirth = 6,
                MinDelayUntilNextBirth = 1,
            });

        // Remove the vanilla baby births. We override it with our own settings
        MegaMother.AI_Abilities.RemoveAt(0);

        #endregion
        #endregion

        #region Custom enemy configuration

        #region Shadow Pouncer
        // Shadow Pouncer
        EnemyCustomization.Model.Shadows.Add(
            new Shadow
            {
                Target = new Target
                {
                    Mode = Mode.PersistentId,
                    PersistentIds = new() { PouncerShadow.PersistentId }
                },
                Type = "NewShadows"
            });
        #endregion

        #region Infection Hybrid

        EnemyCustomization.Model.Materials.Add(
            new Material
            {
                Target = new Target
                {
                    Mode = Mode.PersistentId,
                    PersistentIds = new() { HybridInfected.PersistentId }
                },
                MaterialSets = new List<MaterialSet>
                {
                    new()
                    {
                        From = MaterialType.ShooterRapidFire,
                        To = MaterialType.MtrCocoon,
                        SkinNoise = "KeepOriginal"
                    }
                }
            });

        #endregion

        #endregion
    }
}

public record GameDataEnemy : Enemy_New
{
    /// <summary>
    /// We explicitly want to not have PIDs set when loading data, they come with their own
    /// Pids. This stops the Pid counter getting incremented when loading vanilla data.
    /// </summary>
    public GameDataEnemy() : base(PidOffsets.None)
    { }
}
