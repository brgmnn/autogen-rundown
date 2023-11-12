using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks
{
    public class WeightedModifier : Generator.ISelectable
    {
        public LevelModifiers Modifier { get; set; }

        public double Weight { get; set; }
    }

    public class ModifiersSet : HashSet<LevelModifiers>
    {
        /// <summary>
        /// Handle removing conflicting modifiers
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public new bool Add(LevelModifiers modifier)
        {
            switch (modifier)
            {
                case LevelModifiers.NoChargers:
                case LevelModifiers.Chargers:
                case LevelModifiers.ManyChargers:
                case LevelModifiers.OnlyChargers:
                    Remove(LevelModifiers.NoChargers);
                    Remove(LevelModifiers.Chargers);
                    Remove(LevelModifiers.ManyChargers);
                    Remove(LevelModifiers.OnlyChargers);
                    break;

                case LevelModifiers.NoShadows:
                case LevelModifiers.Shadows:
                case LevelModifiers.ManyShadows:
                case LevelModifiers.OnlyShadows:
                    Remove(LevelModifiers.NoShadows);
                    Remove(LevelModifiers.Shadows);
                    Remove(LevelModifiers.ManyShadows);
                    Remove(LevelModifiers.OnlyShadows);
                    break;
            }

            return base.Add(modifier);
        }

        public override string ToString() => $"[{string.Join(", ", this)}]";
    }

    public class LevelSettings
    {
        public string Tier { get; set; } = "A";

        public Bulkhead Bulkheads { get; set; } = Bulkhead.Main;

        public Dictionary<Bulkhead, ZoneBuildExpansion> Directions { get; private set; }
            = new Dictionary<Bulkhead, ZoneBuildExpansion>();

        public ModifiersSet Modifiers { get; set; } = new ModifiersSet();

        /// <summary>
        /// A pack of enemies that will be drawn for placing bosses in the level.
        /// </summary>
        public List<EnemySpawningData> EnemyBossPack { get; set; } = new List<EnemySpawningData>();

        /// <summary>
        /// Enemy hibernation packs for the whole level
        /// </summary>
        public List<EnemySpawningData> EnemyHibernationPack { get; set; } = new List<EnemySpawningData>();

        public LevelSettings(string? tier = null)
        {
            if (tier != null)
            {
                Tier = tier;
                Generate();
            }
        }

        /// <summary>
        /// Create a deck of hibernating enemies to draw for the level. We will need quite a lot
        /// of these as we will be drawing from this deck multiple times for every zone.
        /// </summary>
        public List<EnemySpawningData> GenerateHibernatingEnemyPack()
        {
            var pack = new List<EnemySpawningData>();

            var baseGroup = new EnemySpawningData
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = Tier switch
                {
                    "A" => (uint)AutogenDifficulty.TierA,
                    "B" => (uint)AutogenDifficulty.TierB,
                    "C" => (uint)AutogenDifficulty.TierC,
                    "D" => (uint)AutogenDifficulty.TierD,
                    "E" => (uint)AutogenDifficulty.TierE,
                    _ => (uint)AutogenDifficulty.TierC
                }
            };
            var chargerGroup = new EnemySpawningData
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = Tier switch
                {
                    "A" => (uint)(AutogenDifficulty.TierA | AutogenDifficulty.Chargers),
                    "B" => (uint)(AutogenDifficulty.TierB | AutogenDifficulty.Chargers),
                    "C" => (uint)(AutogenDifficulty.TierC | AutogenDifficulty.Chargers),
                    "D" => (uint)(AutogenDifficulty.TierD | AutogenDifficulty.Chargers),
                    "E" => (uint)(AutogenDifficulty.TierE | AutogenDifficulty.Chargers),
                    _ => (uint)(AutogenDifficulty.TierC | AutogenDifficulty.Chargers)
                },
            };
            var shadowGroup = new EnemySpawningData
            {
                GroupType = EnemyGroupType.Hibernate,
                Difficulty = Tier switch
                {
                    "A" => (uint)(AutogenDifficulty.TierA | AutogenDifficulty.Shadows),
                    "B" => (uint)(AutogenDifficulty.TierB | AutogenDifficulty.Shadows),
                    "C" => (uint)(AutogenDifficulty.TierC | AutogenDifficulty.Shadows),
                    "D" => (uint)(AutogenDifficulty.TierD | AutogenDifficulty.Shadows),
                    "E" => (uint)(AutogenDifficulty.TierE | AutogenDifficulty.Shadows),
                    _ => (uint)(AutogenDifficulty.TierC | AutogenDifficulty.Shadows)
                },
            };

            // Add base groups. Note that if we have _only_ chargers or shadows we will not add the base group.
            if (!Modifiers.Contains(LevelModifiers.OnlyChargers) && !Modifiers.Contains(LevelModifiers.OnlyShadows))
                for (int i = 0; i < 100; i++)
                    pack.Add(baseGroup);

            var chargers = 0;

            if (Modifiers.Contains(LevelModifiers.Chargers))
                chargers = 20;
            else if (Modifiers.Contains(LevelModifiers.ManyChargers))
                chargers = 60;
            else if (Modifiers.Contains(LevelModifiers.OnlyChargers))
                chargers = 100;

            var shadows = 0;

            if (Modifiers.Contains(LevelModifiers.Shadows))
                shadows = 20;
            else if (Modifiers.Contains(LevelModifiers.ManyShadows))
                shadows = 60;
            else if (Modifiers.Contains(LevelModifiers.OnlyShadows))
                shadows = 100;

            /*for (int i = 0; i < chargers; i++)
                pack.Add(baseGroup with { EnemyType = EnemyType.Charger });

            for (int i = 0; i < shadows; i++)
                pack.Add(baseGroup with { EnemyType = EnemyType.Shadow });*/

            return pack;
        }

        public void Generate()
        {
            switch (Tier)
            {
                case "A":
                    {
                        Modifiers.Add(LevelModifiers.NoChargers);
                        Modifiers.Add(LevelModifiers.NoShadows);
                        break;
                    }

                case "B":
                    {
                        Modifiers.Add(LevelModifiers.NoChargers);
                        Modifiers.Add(LevelModifiers.NoShadows);
                        break;
                    }

                case "C":
                    {
                        Modifiers.Add(LevelModifiers.NoShadows);
                        Modifiers.Add(
                            Generator.Select(new List<WeightedModifier>
                            {
                                new WeightedModifier { Modifier = LevelModifiers.NoChargers,   Weight = 0.4 },
                                new WeightedModifier { Modifier = LevelModifiers.Chargers,     Weight = 0.5 },
                                new WeightedModifier { Modifier = LevelModifiers.ManyChargers, Weight = 0.1 },
                            }).Modifier);

                        EnemyBossPack = Generator.Select(
                            new List<(double, List<EnemySpawningData>)>
                            {
                                (0.8, new List<EnemySpawningData>()),
                                (0.2, new List<EnemySpawningData>
                                {
                                    EnemySpawningData.Mother with { Points = 10 }
                                })
                            });
                        break;
                    }

                case "D":
                    {
                        Modifiers.Add(
                            Generator.Select(new List<WeightedModifier>
                            {
                                new WeightedModifier { Modifier = LevelModifiers.NoShadows,   Weight = 0.4 },
                                new WeightedModifier { Modifier = LevelModifiers.Shadows,     Weight = 0.5 },
                                new WeightedModifier { Modifier = LevelModifiers.ManyShadows, Weight = 0.1 },
                            }).Modifier);
                        Modifiers.Add(
                            Generator.Select(new List<WeightedModifier>
                            {
                                new WeightedModifier { Modifier = LevelModifiers.NoChargers,   Weight = 0.4 },
                                new WeightedModifier { Modifier = LevelModifiers.Chargers,     Weight = 0.5 },
                                new WeightedModifier { Modifier = LevelModifiers.ManyChargers, Weight = 0.1 },
                            }).Modifier);

                        EnemyBossPack = Generator.Select(
                            new List<(double, List<EnemySpawningData>)>
                            {
                                (0.6, new List<EnemySpawningData>()),
                                (0.2, new List<EnemySpawningData>
                                {
                                    EnemySpawningData.Mother with { Points = 10 }
                                }),
                                (0.2, new List<EnemySpawningData>
                                {
                                    EnemySpawningData.Tank with { Points = 10 }
                                })
                            });
                        break;
                    }

                case "E":
                    {
                        Modifiers.Add(
                            Generator.Select(new List<WeightedModifier>
                            {
                                new WeightedModifier { Modifier = LevelModifiers.NoShadows,   Weight = 0.3 },
                                new WeightedModifier { Modifier = LevelModifiers.Shadows,     Weight = 0.6 },
                                new WeightedModifier { Modifier = LevelModifiers.ManyShadows, Weight = 0.1 },
                            }).Modifier);
                        Modifiers.Add(
                            Generator.Select(new List<WeightedModifier>
                            {
                                new WeightedModifier { Modifier = LevelModifiers.NoChargers,   Weight = 0.3 },
                                new WeightedModifier { Modifier = LevelModifiers.Chargers,     Weight = 0.6 },
                                new WeightedModifier { Modifier = LevelModifiers.ManyChargers, Weight = 0.1 },
                            }).Modifier);

                        EnemyBossPack = Generator.Select(
                            new List<(double, List<EnemySpawningData>)>
                            {
                                (1.0, new List<EnemySpawningData>()),
                                (1.0, new List<EnemySpawningData>
                                {
                                    EnemySpawningData.Mother with { Points = 10 }
                                }),
                                (1.0, new List<EnemySpawningData>
                                {
                                    EnemySpawningData.Tank with { Points = 10 }
                                }),
                                (1.0, new List<EnemySpawningData>
                                {
                                    EnemySpawningData.Mother with { Points = 20 }
                                }),
                                (1.0, new List<EnemySpawningData>
                                {
                                    EnemySpawningData.Mother with { Points = 20 },
                                    EnemySpawningData.Tank with { Points = 10 }
                                })
                            });
                        break;
                    }
            }

            EnemyHibernationPack = GenerateHibernatingEnemyPack();
        }
    }
}
