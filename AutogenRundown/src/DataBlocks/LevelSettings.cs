using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

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
            case LevelModifiers.Fog:
            case LevelModifiers.HeavyFog:
                Remove(LevelModifiers.NoFog);
                break;

            case LevelModifiers.NoFog:
                Remove(LevelModifiers.Fog);
                Remove(LevelModifiers.HeavyFog);
                Remove(LevelModifiers.FogIsInfectious);
                break;

            case LevelModifiers.NoChargers:
            case LevelModifiers.Chargers:
            case LevelModifiers.ManyChargers:
            case LevelModifiers.OnlyChargers:
                Remove(LevelModifiers.NoChargers);
                Remove(LevelModifiers.Chargers);
                Remove(LevelModifiers.ManyChargers);
                Remove(LevelModifiers.OnlyChargers);
                break;

            case LevelModifiers.NoFlyers:
            case LevelModifiers.Flyers:
            case LevelModifiers.ManyFlyers:
            case LevelModifiers.OnlyFlyers:
                Remove(LevelModifiers.NoFlyers);
                Remove(LevelModifiers.Flyers);
                Remove(LevelModifiers.ManyFlyers);
                Remove(LevelModifiers.OnlyFlyers);
                break;

            case LevelModifiers.NoNightmares:
            case LevelModifiers.Nightmares:
            case LevelModifiers.ManyNightmares:
            case LevelModifiers.OnlyNightmares:
                Remove(LevelModifiers.NoNightmares);
                Remove(LevelModifiers.Nightmares);
                Remove(LevelModifiers.ManyNightmares);
                Remove(LevelModifiers.OnlyNightmares);
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

            case LevelModifiers.NoInfection:
            case LevelModifiers.Infection:
            case LevelModifiers.HeavyInfection:
                Remove(LevelModifiers.NoInfection);
                Remove(LevelModifiers.Infection);
                Remove(LevelModifiers.HeavyInfection);
                break;

            case LevelModifiers.NoRespawnCocoons:
            case LevelModifiers.RespawnCocoons:
                Remove(LevelModifiers.NoRespawnCocoons);
                Remove(LevelModifiers.RespawnCocoons);
                break;
        }

        return base.Add(modifier);
    }

    public override string ToString() => $"[{string.Join(", ", this)}]";
}

public class LevelSettings
{
    public string Tier { get; set; } = "";

    /// <summary>
    /// Which of the three bulkheads this level has.
    /// </summary>
    public Bulkhead Bulkheads { get; set; } = Bulkhead.Main;

    public BukheadStrategy BulkheadStrategy { get; set; } = BukheadStrategy.Default;

    public ModifiersSet Modifiers { get; set; } = new()
    {
        LevelModifiers.NoFog,
        LevelModifiers.NoInfection
    };

    /// <summary>
    /// A pack of enemies that will be drawn for placing bosses in the level.
    /// </summary>
    public List<(double, int, EnemySpawningData)> EnemyBossPack { get; set; } = new();

    /// <summary>
    /// Enemy hibernation packs for the whole level
    /// </summary>
    public List<EnemySpawningData> EnemyHibernationPack { get; set; } = new();

    /// <summary>
    /// Used for adding new GatherSmallItem pickup packs. This is so we don't have multiple
    /// objectives all using the same item. The items count towards all objectives which is
    /// not desired.
    /// </summary>
    public List<(double weight, int count, WardenObjectiveItem item)> SmallPickupPack { get; set; } =
        new()
        {
            // Currently disabled items.
            //  * MemoryStick: The model is quite small and hard to see especially in boxes.
            //    Removed until some other pickup spot can be used

            // (1.0, 1, WardenObjectiveItem.MemoryStick),

            (1.0, 1, WardenObjectiveItem.PersonnelId),
            (1.0, 1, WardenObjectiveItem.PartialDecoder),
            (1.0, 1, WardenObjectiveItem.Harddrive),
            (0.7, 1, WardenObjectiveItem.Glp_1),
            (0.7, 1, WardenObjectiveItem.Glp_2),
            (1.0, 1, WardenObjectiveItem.Osip),
            (1.0, 1, WardenObjectiveItem.PlantSample),
            (0.4, 1, WardenObjectiveItem.DataCube),
            (0.4, 1, WardenObjectiveItem.DataCubeBackup),
            (0.4, 1, WardenObjectiveItem.DataCubeTampered)
        };

    #region Build directions
    /// <summary>
    /// We need to store the relative directions of each of the bulkheads so we can use them
    /// to try and reduce level lockup
    /// </summary>
    private readonly Dictionary<Bulkhead, RelativeDirection> bulkheadDirections = new();

    public RelativeDirection GetDirections(Bulkhead bulkhead)
        => bulkheadDirections.GetValueOrDefault(bulkhead, RelativeDirection.Global_Forward);

    public void SetDirections(Bulkhead bulkhead, RelativeDirection direction)
        => bulkheadDirections[bulkhead] = direction;
    #endregion

    #region Error Alarms

    /// <summary>
    /// The zones which contain error alarms
    ///
    /// We want to track this across the entire level
    /// </summary>
    public List<ZoneNode> ErrorAlarmZones { get; set; } = new();

    /// <summary>
    /// Used for the random error alarm rolls. Does _not_ apply to the
    /// specific level layout error alarms that get rolled.
    /// </summary>
    public int MaxErrorAlarms { get; set; } = -1;

    #endregion

    public LevelSettings(string? tier = null)
    {
        if (tier != null)
        {
            Tier = tier;
            Generate();
        }
    }

    public bool HasFog() =>
        Modifiers.Contains(LevelModifiers.Fog) || Modifiers.Contains(LevelModifiers.HeavyFog);

    public bool HasChargers() =>
        Modifiers.Contains(LevelModifiers.Chargers) || Modifiers.Contains(LevelModifiers.ManyChargers);

    public bool HasFlyers() =>
        Modifiers.Contains(LevelModifiers.Flyers) || Modifiers.Contains(LevelModifiers.ManyFlyers);

    public bool HasNightmares() =>
        Modifiers.Contains(LevelModifiers.Nightmares) || Modifiers.Contains(LevelModifiers.ManyNightmares);

    public bool HasShadows() =>
        Modifiers.Contains(LevelModifiers.Shadows) || Modifiers.Contains(LevelModifiers.ManyShadows);

    /// <summary>
    /// Create a deck of hibernating enemies to draw for the level. We will need quite a lot
    /// of these as we will be drawing from this deck multiple times for every zone.
    ///
    /// TODO: this actually looks unused. LevelLayout is responsible for generating enemies
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

        /*var chargers = 0;

        if (Modifiers.Contains(LevelModifiers.Chargers))
            chargers = 20;
        else if (Modifiers.Contains(LevelModifiers.ManyChargers))
            chargers = 60;
        else if (Modifiers.Contains(LevelModifiers.OnlyChargers))
            chargers = 100;*/

        /*var shadows = 0;

        if (Modifiers.Contains(LevelModifiers.Shadows))
            shadows = 20;
        else if (Modifiers.Contains(LevelModifiers.ManyShadows))
            shadows = 60;
        else if (Modifiers.Contains(LevelModifiers.OnlyShadows))
            shadows = 100;*/

        /*for (int i = 0; i < chargers; i++)
            pack.Add(baseGroup with { EnemyType = EnemyType.Charger });

        for (int i = 0; i < shadows; i++)
            pack.Add(baseGroup with { EnemyType = EnemyType.Shadow });*/

        return pack;
    }

    /// <summary>
    /// We want to run this after the other modifiers like fog and enemy types are run
    /// </summary>
    public List<(double, int, EnemySpawningData)> GenerateBossPack()
    {
        var pack = new List<(double, int, EnemySpawningData)>();

        switch (Tier)
        {
            case "C":
            {
                pack = new List<(double, int, EnemySpawningData)>
                {
                    (0.65, 2, EnemySpawningData.Pouncer with { Points = 4 }),
                    (0.65, 1, EnemySpawningData.Tank with { Points = 10 }),
                    (0.35, 1, EnemySpawningData.Mother with { Points = 10 })
                };
                break;
            }

            case "D":
            {
                pack  = new List<(double, int, EnemySpawningData)>
                {
                    (0.15, 2, EnemySpawningData.Pouncer with { Points = 8 }),
                    (0.05, 1, EnemySpawningData.PouncerShadow with { Points = 4 }),
                    (0.30, 1, EnemySpawningData.Mother with { Points = 10 }),
                    (0.10, 1, EnemySpawningData.Mother with { Points = 20 }),
                    (0.35, 1, EnemySpawningData.Tank with { Points = 10 }),
                    (0.25, 1, EnemySpawningData.Tank with { Points = 20 })
                };

                // Remove items from the pack until we reach the boss cap
                while (pack.Sum(boss => boss.Item2) > 3)
                    Generator.DrawSelectFrequency(pack);
                break;
            }

            case "E":
            {
                pack = new List<(double, int, EnemySpawningData)>
                {
                    (0.10, 2, EnemySpawningData.Pouncer with { Points = 12 }),
                    (0.10, 1, EnemySpawningData.PouncerShadow with { Points = 4 }),
                    (0.20, 2, EnemySpawningData.Mother with { Points = 10 }),
                    (0.30, 2, EnemySpawningData.Tank with { Points = 10 }),
                    (0.20, 1, EnemySpawningData.Mother with { Points = 20 }),
                    (0.10, 1, EnemySpawningData.Tank with { Points = 20 })
                };
                break;
            }
        }

        var bossMax = Tier switch
        {
            "C" => 1,
            "D" => 2,
            "E" => 4,
            _ => 1
        };

        // Remove items from the pack until we reach the boss cap
        while (pack.Sum(boss => boss.Item2) > bossMax)
            Generator.DrawSelectFrequency(pack);

        return pack;
    }

    public void Generate()
    {
        switch (Tier)
        {
            case "A":
            {
                MaxErrorAlarms = 0;

                Modifiers.Add(LevelModifiers.NoChargers);
                Modifiers.Add(LevelModifiers.NoFlyers);
                Modifiers.Add(LevelModifiers.NoNightmares);
                Modifiers.Add(LevelModifiers.NoShadows);
                break;
            }

            case "B":
            {
                MaxErrorAlarms = 0;

                Modifiers.Add(LevelModifiers.NoChargers);
                Modifiers.Add(LevelModifiers.NoFlyers);
                Modifiers.Add(LevelModifiers.NoNightmares);
                Modifiers.Add(LevelModifiers.NoShadows);

                // Fog modifiers
                if (Generator.Flip(0.1))
                    Modifiers.Add(LevelModifiers.FogIsInfectious);

                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.85, LevelModifiers.NoFog),
                        (0.15, LevelModifiers.Fog),
                    }));

                break;
            }

            case "C":
            {
                MaxErrorAlarms = 2;

                Modifiers.Add(LevelModifiers.NoShadows);
                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.4, LevelModifiers.NoChargers),
                        (0.5, LevelModifiers.Chargers),
                        (0.1, LevelModifiers.ManyChargers),
                    }));
                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.6, LevelModifiers.NoNightmares),
                        (0.3, LevelModifiers.Nightmares),
                        (0.1, LevelModifiers.ManyNightmares),
                    }));

                if (Generator.Flip(0.3))
                    Modifiers.Add(LevelModifiers.FogIsInfectious);

                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.6, LevelModifiers.NoFog),
                        (0.4, LevelModifiers.Fog),
                    }));
                break;
            }

            case "D":
            {
                MaxErrorAlarms = 3;

                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.4, LevelModifiers.NoShadows),
                        (0.5, LevelModifiers.Shadows),
                        (0.1, LevelModifiers.ManyShadows),
                    }));
                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.4, LevelModifiers.NoChargers),
                        (0.5, LevelModifiers.Chargers),
                        (0.1, LevelModifiers.ManyChargers),
                    }));
                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.5, LevelModifiers.NoNightmares),
                        (0.4, LevelModifiers.Nightmares),
                        (0.1, LevelModifiers.ManyNightmares),
                    }));
                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.4, LevelModifiers.NoFlyers),
                        (0.5, LevelModifiers.Flyers),
                        (0.1, LevelModifiers.ManyFlyers),
                    }));

                if (Generator.Flip(0.5))
                    Modifiers.Add(LevelModifiers.FogIsInfectious);

                // Roll infection hybrids
                if (Modifiers.Contains(LevelModifiers.FogIsInfectious) && Generator.Flip(0.3))
                    Modifiers.Add(LevelModifiers.InfectionHybrids);

                // Roll regular hybrids
                if (Generator.Flip(0.4))
                    Modifiers.Add(LevelModifiers.Hybrids);

                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.4, LevelModifiers.NoFog),
                        (0.5, LevelModifiers.Fog),
                        (0.1, LevelModifiers.HeavyFog),
                    }));
                break;
            }

            case "E":
            {
                MaxErrorAlarms = 5;

                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.3, LevelModifiers.NoShadows),
                        (0.6, LevelModifiers.Shadows),
                        (0.1, LevelModifiers.ManyShadows),
                    }));
                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.3, LevelModifiers.NoChargers),
                        (0.6, LevelModifiers.Chargers),
                        (0.1, LevelModifiers.ManyChargers),
                    }));
                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.4, LevelModifiers.NoNightmares),
                        (0.5, LevelModifiers.Nightmares),
                        (0.1, LevelModifiers.ManyNightmares),
                    }));
                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.3, LevelModifiers.NoFlyers),
                        (0.6, LevelModifiers.Flyers),
                        (0.1, LevelModifiers.ManyFlyers),
                    }));

                if (Generator.Flip(0.9))
                    Modifiers.Add(LevelModifiers.FogIsInfectious);

                // Roll infection hybrids
                if (Modifiers.Contains(LevelModifiers.FogIsInfectious) && Generator.Flip(0.65))
                    Modifiers.Add(LevelModifiers.InfectionHybrids);

                // Roll regular hybrids
                if (Generator.Flip(0.7))
                    Modifiers.Add(LevelModifiers.Hybrids);

                Modifiers.Add(
                    Generator.Select(new List<(double, LevelModifiers)>
                    {
                        (0.3, LevelModifiers.NoFog),
                        (0.5, LevelModifiers.Fog),
                        (0.2, LevelModifiers.HeavyFog),
                    }));
                break;
            }
        }

        EnemyBossPack = GenerateBossPack();
        EnemyHibernationPack = GenerateHibernatingEnemyPack();
    }
}
