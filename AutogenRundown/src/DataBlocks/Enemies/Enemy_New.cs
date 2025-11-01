using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Abilities;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.EnemyAbilities;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Bones;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Glows;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Models.Materials;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Projectiles;
using AutogenRundown.DataBlocks.Custom.ExtraEnemyCustomization.Properties;
using Newtonsoft.Json;
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
    public List<ModelData> ModelDatas { get; set; } = new();
    public int DetectionDataId { get; set; }
    public int BehaviorDataId { get; set; }
    public uint MovementDataId { get; set; }
    public uint BalancingDataId { get; set; }
    public uint SFXDataId { get; set; }
    public JArray ArenaDimensions { get; set; } = new();
    public JArray LinkedSlaveModels { get; set; } = new();
    public int InternalMaterial { get; set; }
    public bool isCoccoon { get; set; }
    public int EnemySpottedDialogId { get; set; }

    [JsonProperty("AI_Abilities")]
    public List<AiAbility> Abilities { get; set; } = new();

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

    /** Kraken */
    public static Enemy_New Kraken { get; set; } = new() { PersistentId = 61 };

    #endregion

    #region Autogen enemies

    /// <summary>
    /// This is a shadow version of the pouncer
    ///
    /// PersistentID = 70
    /// </summary>
    public static Enemy_New PouncerShadow { get; set; } = new() { PersistentId = 0 };

    /// <summary>
    /// Infection hybrid
    ///
    /// PersistentID = 71
    /// </summary>
    public static Enemy_New HybridInfected { get; set; } = new() { PersistentId = 0 };


    /// <summary>
    /// Giant nightmare striker
    ///
    /// PersistentID = 72
    /// </summary>
    public static Enemy_New NightmareGiant { get; set; } = new() { PersistentId = 0 };

    /// <summary>
    /// Striker that's infested and spawns babies
    ///
    /// PersistentID = 73
    /// </summary>
    public static Enemy_New StrikerInfested { get; set; } = new() { PersistentId = 0 };

    #endregion
    #endregion

    public Enemy_New(PidOffsets offsets = PidOffsets.None)
        : base(Generator.GetPersistentId(offsets))
    { }

    // TODO: this doesn't seem to properly duplicate. ModelData's seem shared
    public static Enemy_New Duplicate(Enemy_New other)
    {
        var dupe = other with
        {
            BasePrefabs = new List<string>(other.BasePrefabs),
            ModelDatas = new List<ModelData>(other.ModelDatas),
            Abilities = new List<AiAbility>(other.Abilities),
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
        PouncerShadow.Name = "Shadow_Pouncer";

        HybridInfected = Duplicate(Hybrid);
        HybridInfected.Name = "Hybrid_Infected";

        NightmareGiant = Duplicate(NightmareStriker);
        NightmareGiant.Name = "Nightmare_Giant2";

        StrikerInfested = Duplicate(Striker);
        StrikerInfested.Name = "Striker_Infested";

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
            MaxScore = 30.0,
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
                ChildrenMax = 32,
                ChildrenPerBirth = 30,
                ChildrenPerBirthMin = 12,
                MaxDelayUntilNextBirth = 6,
                MinDelayUntilNextBirth = 1,
            });

        // Remove the vanilla baby births. We override it with our own settings
        MegaMother.Abilities.RemoveAt(0);

        #endregion
        #endregion

        #region Custom enemy configuration

        #region Shadow Pouncer
        /*
         * It's just like a regular pouncer, but it's invisible
         */
        {
            var shadowPouncer = new Target
            {
                Mode = Mode.PersistentId,
                PersistentIds = new() { PouncerShadow.PersistentId }
            };

            // Shadow Pouncer
            EnemyCustomization.Model.Shadows.Add(
                new Shadow
                {
                    Target = shadowPouncer,
                    Type = "NewShadows"
                });

            EnemyCustomization.Property.DistantRoars.Add(new DistantRoar
            {
                Target = shadowPouncer,
                RoarSound = "Pouncer",
                RoarSize = "Medium"
            });
        }
        #endregion

        #region Infected Hybrid
        /*
         * Infected Hybrids are hybrids that deal infection damage and are themed with
         * more green and infection
         *
         * Compared with normal hybrids:
         *  - Similar ranged attacks: 14 projectile burst, similar speed/spread/homing
         *  - 2.1% damage projectiles from infected hybrids vs 3.57% damage from hybrids
         *      - Full barrage from infected = 30% damage
         *      - Full barrage from hybrid = 50% damage
         *  - 2% infection per projectile from infected hybrids
         *      - Full barrage applies 28% infection
         *  - Melee attacks are the same, with infected hybrids dealing 8% infection per hit
         */
        {
            var hybridInfected = new Target
            {
                Mode = Mode.PersistentId,
                PersistentIds = new() { HybridInfected.PersistentId }
            };
            // Green infection color
            var activeColor = "#5bff8b";

            EnemyCustomization.Model.Materials.Add(
                new Material
                {
                    Target = hybridInfected,
                    MaterialSets = new List<MaterialSet>
                    {
                        new()
                        {
                            From = MaterialType.ShooterRapidFire,
                            To = MaterialType.MtrCocoon,
                            SkinNoise = SkinNoise.KeepOriginal
                        }
                    }
                });
            EnemyCustomization.Model.Glows.Add(
                new Glow
                {
                    Target = hybridInfected,
                    DefaultColor = "black",
                    HeartbeatColor = $"{activeColor} * 1.5",
                    DetectionColor = activeColor,
                    SelfWakeupColor = "red",
                    PropagateWakeupColor = "red",
                    TentacleAttackColor = "red",
                    ShooterFireColor = $"{activeColor} * 2.0",
                    PulseEffects = new List<PuseEffect>
                    {
                        // new()
                        // {
                        //     Target = "Hibernate",
                        //     Duration = 1.75,
                        //     GlowPattern = "08",
                        //     GlowColor = $"{activeColor} * 10.0"
                        // },
                        // new()
                        // {
                        //     Target = "Combat | Scout",
                        //     Duration = 5.0,
                        //     GlowPattern = "0+6+0+6+060606",
                        //     GlowColor = $"{activeColor} * 10.0"
                        // }
                    }
                });

            EnemyCustomization.Projectile.Projectiles.Add(
                new Custom.ExtraEnemyCustomization.Projectiles.Projectile
                {
                    Name = "Small-Cyan-HighHoming",
                    Id = 50,
                    BaseProjectile = "Targetingtiny",
                    Speed = 16,
                    CheckEvasiveDistance = 4.0,
                    HomingDelay = 0.0,
                    HomingStrength = 12.0,
                    LifeTime = "100%",
                    TrailColor = activeColor,
                    TrailTime = 1.0,
                    TrailWidth = "100%",
                    GlowColor = activeColor,
                    GlowRange = 0.8,
                    Damage = 2.1
                });
            EnemyCustomization.Projectile.ShooterFires.Add(
                new ShooterFire
                {
                    Target = hybridInfected,
                    FireSettings = new List<FireSetting>
                    {
                        new()
                        {
                            FromDistance = -1.0,
                            ProjectileType = 50,
                            BurstCount = 14,
                            BurstDelay = 3.0,
                            ShotDelayMin = 0.14,
                            ShotDelayMax = 0.143,
                            InitialFireDelay = 0.8,
                            ShotSpreadXMin = -70.0,
                            ShotSpreadXMax = 70.0,
                            ShotSpreadYMin = -20.0,
                            ShotSpreadYMax = 80.0
                        }
                    }
                });

            EnemyCustomization.Ability.InfectionAttacks.Add(
                new InfectionAttack
                {
                    Target = hybridInfected,
                    MeleeData = new InfectionAttackData
                    {
                        Infection = 0.08
                    },
                    ProjectileData = new InfectionAttackData
                    {
                        Infection = 0.02
                    }
                });

            EnemyCustomization.Property.DistantRoars.Add(new DistantRoar
            {
                Target = hybridInfected,
                RoarSound = "Shooter_Spread",
                RoarSize = "Big"
            });
        }
        #endregion

        #region Nightmare Giant Striker
        /*
         * Similar to Giant Chargers, these are giant versions of the Nightmare Beserkers
         */
        {
            var nightmareGiant = new Target
            {
                Mode = Mode.PersistentId,
                PersistentIds = new() { NightmareGiant.PersistentId }
            };

            NightmareGiant.MovementDataId = EnemyMovement.NightmareGiant.PersistentId;
            NightmareGiant.BalancingDataId = EnemyBalancing.NightmareGiant.PersistentId;
            NightmareGiant.BehaviorDataId = 28; // Giant / Giant Charger
            NightmareGiant.SFXDataId = EnemySFX.NightmareGiant.PersistentId;
            NightmareGiant.Abilities = new List<AiAbility>
            {
                new ()
                {
                    AbilityPrefab = "Assets/AssetPrefabs/Characters/Enemies/Abilities/EAB_StrikerMelee.prefab",
                    AbilityType = 1,
                    Cooldown = 0.8
                },
                new()
                {
                    AbilityPrefab = "Assets/AssetPrefabs/Characters/Enemies/Abilities/EAB_DoorBreakerStriker.prefab",
                    AbilityType = 8,
                    Cooldown = 1.0
                },
                // new()
                // {
                //     AbilityPrefab = "Assets/AssetPrefabs/Characters/Enemies/Abilities/EAB_StrikerBigTentacle.prefab",
                //     AbilityType = 2,
                //     Cooldown = 10.0
                // }
            };
            NightmareGiant.ModelDatas = new List<ModelData>
            {
                new()
                {
                    ModelFile = "Assets/AssetPrefabs/CharacterBuilder/Enemies/Striker/Striker_Berserk_CB.prefab",
                    ModelCustomization =
                        "Assets/AssetPrefabs/CharacterBuilder/Enemies/Striker/Customization_StrikerBerserk.prefab",
                    NeckScale = Vector3.Zero(),
                    HeadScale = Vector3.Zero(),
                    ChestScale = new Vector3 { X = 1.05, Y = 1.05, Z = 1.05 },
                    ArmScale = new Vector3 { X = 1.0, Y = 1.0, Z = 1.0 },
                    LegScale = new Vector3 { X = 1.05, Y = 1.05, Z = 1.05 },
                    SizeRange = new Vector2 { X = 1.9, Y = 2.0 }
                }
            };

            EnemyCustomization.Property.DistantRoars.Add(new DistantRoar
            {
                Target = nightmareGiant,
                RoarSound = "Striker_Berserk",
                RoarSize = "Big"
            });
        }
        #endregion

        #region Striker Infested
        /*
         * Infested strikers are like normal strikers, except they spawn two babies upon death
         */
        {
            var strikerInfested = new Target
            {
                Mode = Mode.PersistentId,
                PersistentIds = new() { StrikerInfested.PersistentId }
            };

            // Set up balancing
            StrikerInfested.BalancingDataId = EnemyBalancing.StrikerInfested.PersistentId;

            const string heartbeatColor = "#f9a448";
            const string sleepColor = "#ec3970";
            const string attackColor = "#ff6600";
            const string fogColor = "#0a0600";

            EnemyCustomization.Model.Materials.Add(
                new Material
                {
                    Target = strikerInfested,
                    MaterialSets = new List<MaterialSet>
                    {
                        new()
                        {
                            // Keep original looks good
                            From = MaterialType.MtrStrikerHibernate,
                            To = MaterialType.MtrStomacheFix,
                            SkinNoise = SkinNoise.KeepOriginal
                        },
                    }
                });

            EnemyCustomization.Model.Glows.Add(
                new Glow
                {
                    Target = strikerInfested,
                    DefaultColor = $"{sleepColor}",
                    HeartbeatColor = $"{heartbeatColor} * 4.0",
                    DetectionColor = $"{heartbeatColor} * 4.0",
                    SelfWakeupColor = "red",
                    PropagateWakeupColor = "red",
                    TentacleAttackColor = $"{attackColor} * 2.0",
                    ShooterFireColor = $"{sleepColor} * 2.0",
                    PulseEffects = new List<PuseEffect>
                    {
                        new()
                        {
                            Target = "Hibernate",
                            Duration = 7,
                            GlowPattern = "4809",
                            GlowColor = $"{sleepColor} * 2.0"
                        },
                        new()
                        {
                            Target = "Combat",
                            Duration = 1.8,
                            GlowPattern = "6f",
                            GlowColor = $"{heartbeatColor} * 2.0"
                        }
                    }
                });
            EnemyCustomization.EnemyAbility.SpawnEnemyAbilities.Add(new SpawnEnemyAbility
            {
                EnemyId = Baby.PersistentId,
                AgentMode = "Agressive",
                TotalCount = 2,
                CountPerSpawn = 2,
                Name = "Spawn_two_baby"
            });
            EnemyCustomization.EnemyAbility.FogSphereAbilities.Add(new FogSphereAbility
            {
                ColorMin = $"{fogColor}99",
                ColorMax = $"{fogColor}aa",
                RangeMin = 0.2,
                RangeMax = 4.0,
                DensityMin = 0.8,
                DensityMax = 0.8,
                DensityAmountMin = 0.7,
                DensityAmountMax = 1.0,
                IntensityMin = 0.5,
                IntensityMax = 0.5,
                Duration = 1,
                Name = "Foggy_boi_fog_explode"
            });
            EnemyCustomization.EnemyAbility.FogSphereAbilities.Add(new FogSphereAbility
            {
                ColorMin = $"{fogColor}aa",
                ColorMax = $"{fogColor}66",
                RangeMin = 4.0,
                RangeMax = 5.0,
                DensityMin = 0.8,
                DensityMax = 0.4,
                DensityAmountMin = 1.0,
                DensityAmountMax = 0.7,
                IntensityMin = 0.5,
                IntensityMax = 0.4,
                Duration = 35,
                Name = "Foggy_boi_fog_linger"
            });
            EnemyCustomization.EnemyAbility.DeathAbilities.Add(new DeathAbility
            {
                Target = strikerInfested,
                Abilities = new List<AbilityReference>
                {
                    new()
                    {
                        AbilityName = "Spawn_two_baby",
                        Delay = 0.1,
                        AllowedMode = "Hibernate | Agressive"
                    },
                    new()
                    {
                        AbilityName = "Foggy_boi_fog_explode",
                        Delay = 0.0,
                        AllowedMode = "Hibernate | Agressive"
                    },
                    new()
                    {
                        AbilityName = "Foggy_boi_fog_linger",
                        Delay = 0.90,
                        AllowedMode = "Hibernate | Agressive"
                    }
                }
            });

            StrikerInfested.ModelDatas = new List<ModelData>
            {
                new()
                {
                    ModelFile = "Assets/AssetPrefabs/CharacterBuilder/Enemies/Striker/Striker_CB.prefab",
                    ModelCustomization =
                        "Assets/AssetPrefabs/CharacterBuilder/Enemies/Striker/Customization_StrikerHibernate.prefab",
                    PositionOffset = Vector3.Zero(),
                    RotationOffset = Vector3.Zero(),
                    NeckScale = Vector3.Zero(),
                    HeadScale = Vector3.Zero(),
                    ChestScale = new Vector3 { X = 1.0, Y = 1.0, Z = 1.0 },
                    ArmScale = new Vector3 { X = 0.05, Y = 0.05, Z = 0.05 },
                    LegScale = Vector3.One(),
                    SizeRange = new Vector2 { X = 1.0, Y = 1.2 }
                }
            };

            EnemyCustomization.Property.DistantRoars.Add(new DistantRoar
            {
                Target = strikerInfested,
                RoarSound = "OldDistantRoar",
                RoarSize = "Big"
            });
        }
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
