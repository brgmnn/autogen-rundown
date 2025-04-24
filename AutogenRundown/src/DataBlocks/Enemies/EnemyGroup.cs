using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks.Enemies;

public enum VanillaEnemyGroup : uint
{
    BloodDoor_Easy = 30,
    BloodDoor_Medium = 76,
    BloodDoor_Bigs = 74,

    BloodDoor_Chargers_Easy = 32,
    BloodDoor_ChargersGiant_Easy = 72,

    BloodDoor_Hybrids_Easy = 31,
    BloodDoor_Hybrids_Medium = 33,

    BloodDoor_Shadows_Easy = 77,
    BloodDoor_Shadows_Medium = 35, // This seems to be the same as Shadows_Easy

    BloodDoor_BossMother = 36,
    BloodDoor_BossMotherSolo = 49,
    BloodDoor_BossTank = 46,

    BloodDoor_Pouncers = 75,
}

public record class EnemyGroupRole
{
    public EnemyRole Role { get; set; }

    public EnemyRoleDistribution Distribution { get; set; } = EnemyRoleDistribution.Rel100;
}

public record EnemyGroup : DataBlock
{
    #region Properties
    /// <summary>
    ///
    /// </summary>
    public EnemyGroupType Type { get; set; } = EnemyGroupType.Hibernate;

    /// <summary>
    ///
    /// </summary>
    public uint Difficulty { get; set; } = 0;

    /// <summary>
    /// Technically this is the following enum:
    /// https://gtfo-modding.gitbook.io/wiki/reference/enum-types#espawnplacementtype
    /// </summary>
    public SpawnPlacementType SpawnPlacementType { get; set; } = SpawnPlacementType.Default;

    /// <summary>
    /// This is actually the number of points to use
    /// </summary>
    public double MaxScore { get; set; } = 1.0;

    /// <summary>
    /// Looks like this is almost always 3.0 except for some bosses
    /// </summary>
    public double ScoreInAreaPaddingMulti { get; set; } = 3.0;

    /// <summary>
    ///
    /// </summary>
    public double RelativeWeight { get; set; } = 1.0;

    /// <summary>
    ///
    /// </summary>
    public List<EnemyGroupRole> Roles { get; set; } = new();
    #endregion

    public EnemyGroup(PidOffsets offsets = PidOffsets.EnemyGroup)
        : base(Generator.GetPersistentId(offsets))
    { }

    public void Persist(BlocksBin<EnemyGroup>? bin = null)
    {
        bin ??= Bins.EnemyGroups;

        if (PersistentId == 0)
            PersistentId = Generator.GetPersistentId(PidOffsets.EnemyGroup);

        bin.AddBlock(this);
    }

    #region Blood Door groups

    public static EnemyGroup BloodDoor_HybridInfected_Hard = new()
    {
        Type = EnemyGroupType.Hunter,
        Difficulty = Enemy_New.HybridInfected.PersistentId,
        MaxScore = 24,
        ScoreInAreaPaddingMulti = 1.0,
        Roles = new List<EnemyGroupRole>
        {
            new() { Role = EnemyRole.Hunter, Distribution = EnemyRoleDistribution.Rel100 }
        }
    };

    #endregion

    public static void Setup()
    {
        JArray array = JArray.Parse(VanillaData);
        var groups = array.ToObject<List<GameDataEnemyGroup>>();

        if (groups == null)
            throw new Exception("Failed to parse vanilla enemy groups data");

        foreach (var group in groups)
        {
            Bins.EnemyGroups.AddBlock(group);
        }
    }

    public new static void SaveStatic()
    {
        // Assign groups for hibernating enemy spawns. EnemyRole and Enemy will select the
        // enemy to pick from, the double specifies the max score for that group. Multiple
        // entries will be randomly picked between.
        var hibernatingGroups = new List<(EnemyRole, Enemy, double)>
        {
            (EnemyRole.Melee, Enemy.Striker, 4.0),
            (EnemyRole.Melee, Enemy.Striker, 6.0),
            (EnemyRole.Melee, Enemy.Striker, 10.0),

            (EnemyRole.Ranged, Enemy.Shooter, 4.0),
            (EnemyRole.Ranged, Enemy.Shooter, 6.0),
            (EnemyRole.Ranged, Enemy.Shooter, 10.0),

            // Chargers
            (EnemyRole.Lurker, Enemy.Charger, 4.0),
            (EnemyRole.Lurker, Enemy.Charger, 8.0),
            (EnemyRole.Lurker, Enemy.Charger, 10.0),

            // Shadows
            (EnemyRole.Melee, Enemy.Shadow, 4.0),
            (EnemyRole.Melee, Enemy.Shadow, 6.0),
            (EnemyRole.Melee, Enemy.Shadow, 10.0),

            // Nightmares
            (EnemyRole.Melee, Enemy.NightmareStriker, 4.0),

            // Giants always spawn in pairs
            (EnemyRole.Melee,  Enemy.StrikerGiant, 8.0),
            (EnemyRole.Ranged, Enemy.ShooterGiant, 8.0),
            (EnemyRole.Lurker, Enemy.ChargerGiant, 8.0),
            (EnemyRole.Melee,  Enemy.ShadowGiant,  8.0),

            // Hybrids
            (EnemyRole.PureSneak, Enemy.Hybrid, 6.0),
            (EnemyRole.PureSneak, Enemy.Hybrid, 9.0),

            // Babies
            (EnemyRole.BirtherChild, Enemy.Baby, 4.0),
            (EnemyRole.BirtherChild, Enemy.Baby, 8.0),

            // Bosses, these need to be individually spawned given their difficulty
            (EnemyRole.PureSneak, Enemy.Tank,       10.0),
            (EnemyRole.PureSneak, Enemy.TankPotato, 10.0),
            (EnemyRole.PureSneak, Enemy.Mother,     10.0),
            (EnemyRole.PureSneak, Enemy.PMother,    10.0),
            (EnemyRole.PureSneak, Enemy.Pouncer,    4.0),

            (EnemyRole.PureSneak, (Enemy)Enemy_New.PouncerShadow.PersistentId, 4.0),
            (EnemyRole.PureSneak, (Enemy)Enemy_New.HybridInfected.PersistentId, 4.0),

            (EnemyRole.Melee, (Enemy)Enemy_New.NightmareGiant.PersistentId, 4.0)
        };

        foreach (var (role, enemy, maxScore) in hibernatingGroups)
        {
            Bins.EnemyGroups.AddBlock(
                new EnemyGroup
                {
                    Type = EnemyGroupType.Hibernate,
                    Difficulty = (uint)enemy,
                    MaxScore = maxScore,
                    Roles = new List<EnemyGroupRole>
                    {
                        new() { Role = role, Distribution = EnemyRoleDistribution.Rel100 }
                    }
                });
        }

        #region Blood Doors

        // BloodDoor_Easy.Persist();

        BloodDoor_HybridInfected_Hard.Persist();

        #endregion

        #region Boss aligned spawn bosses
        // These bosses are set up to spawn in boss aligned spawn points
        foreach (var boss in EnemyInfo.SpawnAlignedBosses)
        {
            Bins.EnemyGroups.AddBlock(
                new EnemyGroup
                {
                    Type = EnemyGroupType.PureSneak,
                    Difficulty = (uint)AutogenDifficulty.BossAlignedSpawn | (uint)boss.Enemy,
                    MaxScore = boss.Points,
                    SpawnPlacementType = SpawnPlacementType.CycleAllAligns,
                    Roles = new List<EnemyGroupRole>
                    {
                        new() { Role = boss.Role, Distribution = EnemyRoleDistribution.Rel100 }
                    }
                });
        }
        #endregion

        #region AutoDiff, common groups
        // Assign common groups to all Autogen Difficulties. Often for the base enemies of
        // strikers / shooters we always want to be able to roll these spawns on levels if
        // just filling with enemies.
        var baseDifficulties = new List<AutogenDifficulty>
        {
            AutogenDifficulty.TierA,
            AutogenDifficulty.TierB,
            AutogenDifficulty.TierC,
            AutogenDifficulty.TierD,
            AutogenDifficulty.TierE,
        };

        foreach (var difficulty in baseDifficulties)
        {
            // Smaller standard mixes
            Bins.EnemyGroups.AddBlock(
                new EnemyGroup
                {
                    Type = EnemyGroupType.Hibernate,
                    Difficulty = (uint)difficulty,
                    MaxScore = 5,
                    Roles = new List<EnemyGroupRole>
                    {
                        new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel75 },
                        new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel25 }
                    }
                });
            Bins.EnemyGroups.AddBlock(
                new EnemyGroup
                {
                    Type = EnemyGroupType.Hibernate,
                    Difficulty = (uint)AutogenDifficulty.TierA,
                    MaxScore = 5,
                    Roles = new List<EnemyGroupRole>
                    {
                        new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel50 },
                        new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel50 }
                    }
                });

            // Larger mixes
            Bins.EnemyGroups.AddBlock(
                new EnemyGroup
                {
                    Type = EnemyGroupType.Hibernate,
                    Difficulty = (uint)difficulty,
                    MaxScore = 10,
                    Roles = new List<EnemyGroupRole>
                    {
                        new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel75 },
                        new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel25 }
                    }
                });
            Bins.EnemyGroups.AddBlock(
                new EnemyGroup
                {
                    Type = EnemyGroupType.Hibernate,
                    Difficulty = (uint)difficulty,
                    MaxScore = 8,
                    Roles = new List<EnemyGroupRole>
                    {
                        new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel50 },
                        new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel50 }
                    }
                });

            // Very big mixes
            Bins.EnemyGroups.AddBlock(
                new EnemyGroup
                {
                    Type = EnemyGroupType.Hibernate,
                    Difficulty = (uint)difficulty,
                    MaxScore = 24,
                    Roles = new List<EnemyGroupRole>
                    {
                        new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel75 },
                        new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel25 }
                    }
                });
            Bins.EnemyGroups.AddBlock(
                new EnemyGroup
                {
                    Type = EnemyGroupType.Hibernate,
                    Difficulty = (uint)difficulty,
                    MaxScore = 18,
                    Roles = new List<EnemyGroupRole>
                    {
                        new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel50 },
                        new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel50 }
                    }
                });
        }
        #endregion

        #region AutoDiff, Tier C
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)AutogenDifficulty.TierC,
                MaxScore = 6,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel100 },
                    new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel50 }
                }
            });
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)AutogenDifficulty.TierC,
                MaxScore = 12,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel100 },
                    new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel25 }
                }
            });
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)(AutogenDifficulty.TierC | AutogenDifficulty.Chargers),
                MaxScore = 6,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Lurker,  Distribution = EnemyRoleDistribution.Rel100 }
                }
            });

        // Chargers
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)(AutogenDifficulty.TierC | AutogenDifficulty.Chargers),
                MaxScore = 8,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Lurker,  Distribution = EnemyRoleDistribution.Rel100 }
                }
            });

        // Nightmares
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)(AutogenDifficulty.TierC | AutogenDifficulty.Nightmares),
                MaxScore = 6,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel50 },
                    new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel50 }
                }
            });
        #endregion

        #region AutoDiff, Tier D
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)AutogenDifficulty.TierD,
                MaxScore = 6,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel100 },
                    new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel50 }
                }
            });
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)AutogenDifficulty.TierD,
                MaxScore = 12,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel100 },
                    new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel25 }
                }
            });

        // Chargers
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)(AutogenDifficulty.TierD | AutogenDifficulty.Chargers),
                MaxScore = 8,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Lurker,  Distribution = EnemyRoleDistribution.Rel100 }
                }
            });

        // Shadows
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)(AutogenDifficulty.TierD | AutogenDifficulty.Shadows),
                MaxScore = 8,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel100 }
                }
            });

        //Hybrids
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)(AutogenDifficulty.TierD | AutogenDifficulty.Hybrids),
                MaxScore = 6,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.PureSneak,  Distribution = EnemyRoleDistribution.Rel100 }
                }
            });

        // Nightmares
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)(AutogenDifficulty.TierD | AutogenDifficulty.Nightmares),
                MaxScore = 6,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel50 },
                    new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel50 }
                }
            });
        #endregion

        #region AutoDiff, Tier E
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)AutogenDifficulty.TierE,
                MaxScore = 6,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel100 },
                    new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel50 }
                }
            });
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)AutogenDifficulty.TierE,
                MaxScore = 12,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel100 },
                    new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel25 }
                }
            });

        // Chargers
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)(AutogenDifficulty.TierE | AutogenDifficulty.Chargers),
                MaxScore = 8,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Lurker,  Distribution = EnemyRoleDistribution.Rel100 }
                }
            });

        // Shadows
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)(AutogenDifficulty.TierE | AutogenDifficulty.Shadows),
                MaxScore = 8,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel100 }
                }
            });

        //Hybrids
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)(AutogenDifficulty.TierE | AutogenDifficulty.Hybrids),
                MaxScore = 6,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.PureSneak,  Distribution = EnemyRoleDistribution.Rel100 }
                }
            });

        // Nightmares
        Bins.EnemyGroups.AddBlock(
            new EnemyGroup
            {
                Type = EnemyGroupType.Hibernate,
                Difficulty = (uint)(AutogenDifficulty.TierE | AutogenDifficulty.Nightmares),
                MaxScore = 8,
                Roles = new List<EnemyGroupRole>
                {
                    new EnemyGroupRole { Role = EnemyRole.Melee,  Distribution = EnemyRoleDistribution.Rel50 },
                    new EnemyGroupRole { Role = EnemyRole.Ranged, Distribution = EnemyRoleDistribution.Rel50 }
                }
            });
        #endregion
    }

    public const string VanillaData = @"[
    {
      ""Type"": 0,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 5.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        }
      ],
      ""name"": ""Hibernate Easy Mix"",
      ""internalEnabled"": true,
      ""persistentID"": 1
    },
    {
      ""Type"": 0,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 0.5,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 3
        },
        {
          ""Role"": 1,
          ""Distribution"": 3
        }
      ],
      ""name"": ""Hibernate Easy Small Mix A"",
      ""internalEnabled"": true,
      ""persistentID"": 8
    },
    {
      ""Type"": 0,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 0.5,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        }
      ],
      ""name"": ""Hibernate Easy Small Mix B"",
      ""internalEnabled"": true,
      ""persistentID"": 9
    },
    {
      ""Type"": 0,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 3.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 0.75,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        }
      ],
      ""name"": ""Hibernate Easy Tiny Mix A"",
      ""internalEnabled"": true,
      ""persistentID"": 12
    },
    {
      ""Type"": 0,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 3.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 0.75,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 3
        },
        {
          ""Role"": 1,
          ""Distribution"": 3
        }
      ],
      ""name"": ""Hibernate Easy Tiny Mix B"",
      ""internalEnabled"": true,
      ""persistentID"": 11
    },
    {
      ""Type"": 0,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 2.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 2.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hibernate Easy Tiny Melee"",
      ""internalEnabled"": true,
      ""persistentID"": 10
    },
    {
      ""Type"": 0,
      ""Difficulty"": 1,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 5.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        }
      ],
      ""name"": ""Hibernate Medium Mix B"",
      ""internalEnabled"": true,
      ""persistentID"": 16
    },
    {
      ""Type"": 0,
      ""Difficulty"": 1,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 3.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.5,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 3
        },
        {
          ""Role"": 1,
          ""Distribution"": 3
        }
      ],
      ""name"": ""Hibernate Medium Small Mix A"",
      ""internalEnabled"": true,
      ""persistentID"": 13
    },
    {
      ""Type"": 0,
      ""Difficulty"": 1,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.5,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        }
      ],
      ""name"": ""Hibernate Medium Small Mix B"",
      ""internalEnabled"": true,
      ""persistentID"": 55
    },
    {
      ""Type"": 0,
      ""Difficulty"": 1,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 2.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.5,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        }
      ],
      ""name"": ""Hibernate Medium Tiny Mix A"",
      ""internalEnabled"": true,
      ""persistentID"": 14
    },
    {
      ""Type"": 0,
      ""Difficulty"": 1,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 2.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.5,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 3
        },
        {
          ""Role"": 1,
          ""Distribution"": 3
        }
      ],
      ""name"": ""Hibernate Medium Tiny Mix B"",
      ""internalEnabled"": true,
      ""persistentID"": 15
    },
    {
      ""Type"": 0,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 1,
          ""Distribution"": 2
        },
        {
          ""Role"": 5,
          ""Distribution"": 1
        },
        {
          ""Role"": 0,
          ""Distribution"": 4
        }
      ],
      ""name"": ""Hibernate Hard Large Mix Scout"",
      ""internalEnabled"": true,
      ""persistentID"": 19
    },
    {
      ""Type"": 0,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 1,
          ""Distribution"": 2
        },
        {
          ""Role"": 0,
          ""Distribution"": 4
        }
      ],
      ""name"": ""Hibernate Hard Large Mix"",
      ""internalEnabled"": true,
      ""persistentID"": 57
    },
    {
      ""Type"": 0,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 5.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.2,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        },
        {
          ""Role"": 5,
          ""Distribution"": 1
        }
      ],
      ""name"": ""Hibernate Hard Mix Scout"",
      ""internalEnabled"": true,
      ""persistentID"": 17
    },
    {
      ""Type"": 0,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 2.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        }
      ],
      ""name"": ""Hibernate Hard Small Mix A"",
      ""internalEnabled"": true,
      ""persistentID"": 58
    },
    {
      ""Type"": 0,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 3.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 2.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        },
        {
          ""Role"": 5,
          ""Distribution"": 1
        }
      ],
      ""name"": ""Hibernate Hard Small Mix Scout"",
      ""internalEnabled"": true,
      ""persistentID"": 18
    },
    {
      ""Type"": 0,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 2.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 5.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        }
      ],
      ""name"": ""Hibernate Hard Small Mix B"",
      ""internalEnabled"": true,
      ""persistentID"": 59
    },
    {
      ""Type"": 0,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hibernate Biss Large Melee Shadows"",
      ""internalEnabled"": true,
      ""persistentID"": 7
    },
    {
      ""Type"": 0,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 5.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hibernate Biss Melee Shadows"",
      ""internalEnabled"": true,
      ""persistentID"": 60
    },
    {
      ""Type"": 0,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 3.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 3.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hibernate Biss Small Melee Shadows"",
      ""internalEnabled"": true,
      ""persistentID"": 61
    },
    {
      ""Type"": 0,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 2.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 4.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hibernate Biss Tiny Melee Shadows"",
      ""internalEnabled"": true,
      ""persistentID"": 62
    },
    {
      ""Type"": 0,
      ""Difficulty"": 7,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 8.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 2.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hibernate Buss Melee Big Shadows"",
      ""internalEnabled"": true,
      ""persistentID"": 63
    },
    {
      ""Type"": 0,
      ""Difficulty"": 7,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 3.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hibernate Buss Small Melee Big Shadows"",
      ""internalEnabled"": true,
      ""persistentID"": 64
    },
    {
      ""Type"": 1,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 4,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Sneak Easy"",
      ""internalEnabled"": true,
      ""persistentID"": 20
    },
    {
      ""Type"": 1,
      ""Difficulty"": 1,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 4,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Sneak Medium"",
      ""internalEnabled"": true,
      ""persistentID"": 21
    },
    {
      ""Type"": 1,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 2,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 4,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Sneak Hard Birther Align_1"",
      ""internalEnabled"": true,
      ""persistentID"": 22
    },
    {
      ""Type"": 1,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 3.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 4,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Sneak Biss Hybrids"",
      ""internalEnabled"": true,
      ""persistentID"": 50
    },
    {
      ""Type"": 1,
      ""Difficulty"": 7,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 8,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Sneak Buss Large BirtherChild"",
      ""internalEnabled"": true,
      ""persistentID"": 52
    },
    {
      ""Type"": 1,
      ""Difficulty"": 7,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 2.0,
      ""Roles"": [
        {
          ""Role"": 8,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Sneak Buss BirtherChild"",
      ""internalEnabled"": true,
      ""persistentID"": 65
    },
    {
      ""Type"": 1,
      ""Difficulty"": 7,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 2.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 3.0,
      ""Roles"": [
        {
          ""Role"": 8,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Sneak Buss Small BirtherChild"",
      ""internalEnabled"": true,
      ""persistentID"": 66
    },
    {
      ""Type"": 0,
      ""Difficulty"": 3,
      ""SpawnPlacementType"": 1,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 9,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hibernate Miniboss Minibos Birther Align_0"",
      ""internalEnabled"": true,
      ""persistentID"": 40
    },
    {
      ""Type"": 0,
      ""Difficulty"": 5,
      ""SpawnPlacementType"": 1,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 9,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hibernate Miniboss Megaboss Tank Align_0"",
      ""internalEnabled"": true,
      ""persistentID"": 44
    },
    {
      ""Type"": 0,
      ""Difficulty"": 4,
      ""SpawnPlacementType"": 2,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 4,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hibernate Boss Boss BirtherBoss Align_1"",
      ""internalEnabled"": true,
      ""persistentID"": 41
    },
    {
      ""Type"": 1,
      ""Difficulty"": 3,
      ""SpawnPlacementType"": 2,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 4,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hibernate Pure Sneak Miniboss BirtherBoss Align_1"",
      ""internalEnabled"": true,
      ""persistentID"": 71
    },
    {
      ""Type"": 1,
      ""Difficulty"": 4,
      ""SpawnPlacementType"": 1,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 4,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Sneak Boss Boss Squidward Align_0"",
      ""internalEnabled"": true,
      ""persistentID"": 69
    },
    {
      ""Type"": 1,
      ""Difficulty"": 5,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 4,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Sneak Megaboss Tank"",
      ""internalEnabled"": true,
      ""persistentID"": 70
    },
    {
      ""Type"": 3,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 5,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Detect Easy Scout"",
      ""internalEnabled"": true,
      ""persistentID"": 26
    },
    {
      ""Type"": 3,
      ""Difficulty"": 1,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 5,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Detect Medium Scout"",
      ""internalEnabled"": true,
      ""persistentID"": 27
    },
    {
      ""Type"": 3,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 5,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Detect Hard Scout"",
      ""internalEnabled"": true,
      ""persistentID"": 28
    },
    {
      ""Type"": 3,
      ""Difficulty"": 4,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 5,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Detect Boss Shadow Scout"",
      ""internalEnabled"": true,
      ""persistentID"": 45
    },
    {
      ""Type"": 3,
      ""Difficulty"": 3,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 5,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Detect MiniBoss Bullrush Scout"",
      ""internalEnabled"": true,
      ""persistentID"": 54
    },
    {
      ""Type"": 3,
      ""Difficulty"": 5,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 5,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Detect MegaBoss Scout R8B1"",
      ""internalEnabled"": true,
      ""persistentID"": 82
    },
    {
      ""Type"": 3,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 10.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 5,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Detect Biss Nightmare Scout"",
      ""internalEnabled"": true,
      ""persistentID"": 83
    },
    {
      ""Type"": 3,
      ""Difficulty"": 7,
      ""SpawnPlacementType"": 2,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 10.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 5,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Pure Detect Buss Nightmare Scout"",
      ""internalEnabled"": true,
      ""persistentID"": 85
    },
    {
      ""Type"": 2,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 6.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 3,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Detect Lurker Bullrush Striker"",
      ""internalEnabled"": true,
      ""persistentID"": 23
    },
    {
      ""Type"": 2,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 2.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 3.0,
      ""Roles"": [
        {
          ""Role"": 3,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Detect Lurker Small Bullrush Striker"",
      ""internalEnabled"": true,
      ""persistentID"": 67
    },
    {
      ""Type"": 2,
      ""Difficulty"": 1,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 3,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Detect Lurker Medium Bullrush Striker"",
      ""internalEnabled"": true,
      ""persistentID"": 43
    },
    {
      ""Type"": 2,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 3,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Detect Lurker Hard Small Bullrush Bigs"",
      ""internalEnabled"": true,
      ""persistentID"": 34
    },
    {
      ""Type"": 2,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 8.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 3,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Detect Lurker Hard Large Bullrush Bigs"",
      ""internalEnabled"": true,
      ""persistentID"": 73
    },
    {
      ""Type"": 2,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 3,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Striker Berserks"",
      ""internalEnabled"": true,
      ""persistentID"": 80
    },
    {
      ""Type"": 2,
      ""Difficulty"": 7,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 3.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 3,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Cluster Shooter"",
      ""internalEnabled"": true,
      ""persistentID"": 81
    },
    {
      ""Type"": 1,
      ""Difficulty"": 3,
      ""SpawnPlacementType"": 1,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 4,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Detect Lurker Megaboss_Align0"",
      ""internalEnabled"": true,
      ""persistentID"": 84
    },
    {
      ""Type"": 6,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        }
      ],
      ""name"": ""Hunter Easy Infront strikers shooters"",
      ""internalEnabled"": true,
      ""persistentID"": 30
    },
    {
      ""Type"": 6,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 15.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 2
        }
      ],
      ""name"": ""Hunter Medium Inback strikers shooters"",
      ""internalEnabled"": true,
      ""persistentID"": 76
    },
    {
      ""Type"": 6,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 1,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 7,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hunter Boss Birther"",
      ""internalEnabled"": true,
      ""persistentID"": 36
    },
    {
      ""Type"": 6,
      ""Difficulty"": 2,
      ""SpawnPlacementType"": 1,
      ""MaxScore"": 5.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 7,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hunter Boss Birther Only One"",
      ""internalEnabled"": true,
      ""persistentID"": 49
    },
    {
      ""Type"": 6,
      ""Difficulty"": 5,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hunter Boss Tank"",
      ""internalEnabled"": true,
      ""persistentID"": 46
    },
    {
      ""Type"": 6,
      ""Difficulty"": 5,
      ""SpawnPlacementType"": 2,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hunter Boss Tank_R4C3_L3"",
      ""internalEnabled"": true,
      ""persistentID"": 79
    },
    {
      ""Type"": 6,
      ""Difficulty"": 7,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 12.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 7,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hunter Bigs"",
      ""internalEnabled"": true,
      ""persistentID"": 74
    },
    {
      ""Type"": 6,
      ""Difficulty"": 3,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 7,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hunter Miniboss BirtherBoss"",
      ""internalEnabled"": true,
      ""persistentID"": 47
    },
    {
      ""Type"": 6,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 12.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 7,
          ""Distribution"": 4
        },
        {
          ""Role"": 0,
          ""Distribution"": 6
        },
        {
          ""Role"": 1,
          ""Distribution"": 7
        }
      ],
      ""name"": ""Hunter Easy Inback StrikersShootersHybrids"",
      ""internalEnabled"": true,
      ""persistentID"": 31
    },
    {
      ""Type"": 6,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 8.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 7,
          ""Distribution"": 1
        },
        {
          ""Role"": 0,
          ""Distribution"": 4
        },
        {
          ""Role"": 1,
          ""Distribution"": 7
        }
      ],
      ""name"": ""Hunter Easy Inback StrikersShootersHybrids V2"",
      ""internalEnabled"": true,
      ""persistentID"": 51
    },
    {
      ""Type"": 6,
      ""Difficulty"": 1,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 16.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 7,
          ""Distribution"": 4
        },
        {
          ""Role"": 7,
          ""Distribution"": 7
        },
        {
          ""Role"": 1,
          ""Distribution"": 8
        },
        {
          ""Role"": 0,
          ""Distribution"": 7
        }
      ],
      ""name"": ""Hunter Medium Inback StrikersShootersHybrids"",
      ""internalEnabled"": true,
      ""persistentID"": 33
    },
    {
      ""Type"": 6,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 8.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 3,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hunter Easy Bullrush"",
      ""internalEnabled"": true,
      ""persistentID"": 32
    },
    {
      ""Type"": 6,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 4.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hunter Easy Shadows R2E1"",
      ""internalEnabled"": true,
      ""persistentID"": 77
    },
    {
      ""Type"": 6,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 20.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 3,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hunter Easy Large Bullrush"",
      ""internalEnabled"": true,
      ""persistentID"": 72
    },
    {
      ""Type"": 6,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 13.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Hunter Medium Biss Melee Shadows"",
      ""internalEnabled"": true,
      ""persistentID"": 35
    },
    {
      ""Type"": 6,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 5.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 0,
          ""Distribution"": 4
        }
      ],
      ""name"": ""Hunter Medium Melee Shadows R2C1"",
      ""internalEnabled"": true,
      ""persistentID"": 78
    },
    {
      ""Type"": 6,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 30.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 8,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Birther Boss Children"",
      ""internalEnabled"": true,
      ""persistentID"": 38
    },
    {
      ""Type"": 6,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 20.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 8,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Birther Children"",
      ""internalEnabled"": true,
      ""persistentID"": 37
    },
    {
      ""Type"": 4,
      ""Difficulty"": 0,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 10.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 6,
          ""Distribution"": 5
        }
      ],
      ""name"": ""PatrolTest"",
      ""internalEnabled"": true,
      ""persistentID"": 29
    },
    {
      ""Type"": 6,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 2.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 8,
          ""Distribution"": 5
        }
      ],
      ""name"": ""Squid Boss Children"",
      ""internalEnabled"": true,
      ""persistentID"": 68
    },
    {
      ""Type"": 6,
      ""Difficulty"": 6,
      ""SpawnPlacementType"": 0,
      ""MaxScore"": 1.0,
      ""ScoreInAreaPaddingMulti"": 1.0,
      ""RelativeWeight"": 1.0,
      ""Roles"": [
        {
          ""Role"": 7,
          ""Distribution"": 1
        }
      ],
      ""name"": ""Hunter Biss Pouncer"",
      ""internalEnabled"": true,
      ""persistentID"": 75
    }
  ]";
}

public record class GameDataEnemyGroup : EnemyGroup
{
    /// <summary>
    /// We explicitly want to not have PIDs set when loading data, they come with their own
    /// Pids. This stops the Pid counter getting incremented when loading vanilla data.
    /// </summary>
    public GameDataEnemyGroup() : base(PidOffsets.None)
    { }
}
