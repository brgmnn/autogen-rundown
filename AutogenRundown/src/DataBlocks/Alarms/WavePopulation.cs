using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.Extensions;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Alarms;

/// <summary>
/// If a wave population doesn't define an enemy for each of it's five roles, the game will default to picking
/// an enemy for us. And ususally it picks a Giant Striker which isn't what we want.
/// </summary>
public record WavePopulation : DataBlock
{
    #region Properties
    /// <summary>
    /// EnemyDataBlock PersistentID of eEnemyType for this population
    /// Cost: 0.75
    /// </summary>
    public Enemy WaveRoleWeakling { get; set; }

    /// <summary>
    /// EnemyDataBlock PersistentID of eEnemyType for this population
    /// Cost: 1.0
    /// </summary>
    public Enemy WaveRoleStandard { get; set; }

    /// <summary>
    /// EnemyDataBlock PersistentID of eEnemyType for this population
    /// Cost: 1.0
    /// </summary>
    public Enemy WaveRoleSpecial { get; set; }

    /// <summary>
    /// EnemyDataBlock PersistentID of eEnemyType for this population
    /// Cost: 2.0
    /// </summary>
    public Enemy WaveRoleMiniBoss { get; set; }

    /// <summary>
    /// EnemyDataBlock PersistentID of eEnemyType for this population
    ///
    /// Allegedly doesn't work
    /// Cost: 2.0
    /// </summary>
    public Enemy WaveRoleBoss { get; set; }

    /// <summary>
    /// Difficulty factor of this enemy population.
    ///
    /// This is really so we can adjust the difficulty of Nightmare enemies which are
    /// significantly harder than other enemy types.
    ///
    /// Default = 1.0
    /// </summary>
    [JsonIgnore]
    public double DifficultyFactor { get; set; } = 1.0;
    #endregion

    public WavePopulation(PidOffsets offsets = PidOffsets.WavePopulation)
        : base(Generator.GetPersistentId(offsets))
    { }

    public override string ToString()
        => DifficultyFactor.ApproxEqual(1.0) ?
            $"WavePopulation {{ Name = {Name}, PersistentId = {PersistentId} }}" :
            $"WavePopulation {{ Name = {Name}, PersistentId = {PersistentId}, Difficulty = {DifficultyFactor} }}";

    public static void Setup()
        => Setup<GameDataWavePopulation, WavePopulation>(Bins.WavePopulations, "SurvivalWavePopulation");

    public static List<(double, int, WavePopulation)> BuildPack(string tier, LevelSettings settings)
    {
        var pack = new List<(double, int, WavePopulation)>();

        // Chargers
        if (!settings.Modifiers.Contains(Levels.LevelModifiers.NoChargers))
        {
            pack.Add((2.0, 10, Baseline_Chargers));
            pack.Add((1.0, 2, OnlyChargers));
        }
        if (settings.Modifiers.Contains(Levels.LevelModifiers.ManyChargers))
            pack.Add((5.0, 4, OnlyChargers));

        // Shadows
        if (!settings.Modifiers.Contains(Levels.LevelModifiers.NoShadows))
        {
            pack.Add((2.0, 10, Baseline_Shadows));
            pack.Add((1.0, 3, OnlyShadows));
        }
        if (settings.Modifiers.Contains(Levels.LevelModifiers.ManyShadows))
            pack.Add((5.0, 4, OnlyShadows));

        // Nightmares
        if (!settings.Modifiers.Contains(Levels.LevelModifiers.NoNightmares))
        {
            pack.Add((2.0, 10, Baseline_Nightmare));
            pack.Add((1.0, 10, OnlyNightmares));
        }
        if (settings.Modifiers.Contains(Levels.LevelModifiers.ManyNightmares))
            pack.Add((5.0, 4, OnlyNightmares));

        // Flyers
        if (!settings.Modifiers.Contains(Levels.LevelModifiers.NoFlyers))
        {
            pack.Add((2.0, 10, Baseline_Flyers));
        }


        switch (tier)
        {
            case "A":
            {
                pack.Add((1.0, 25, Baseline));
                break;
            }

            case "B":
            {
                pack.Add((1.0, 25, Baseline));
                break;
            }

            case "C":
            {
                pack.Add((1.0, 25, Baseline));
                pack.Add((1.0, 5, Baseline_Hybrids));
                break;
            }

            case "D":
            {
                pack.Add((1.0, 25, Baseline));
                pack.Add((1.0, 5, Baseline_Hybrids));
                break;
            }

            case "E":
            {
                pack.Add((1.0, 25, Baseline));
                pack.Add((1.0, 5, Baseline_Hybrids));
                break;
            }
        }

        return pack;
    }

    public new static void SaveStatic()
    {
        Bins.WavePopulations.AddBlock(Baseline);
        Bins.WavePopulations.AddBlock(Baseline_Hybrids);
        Bins.WavePopulations.AddBlock(Baseline_Chargers);
        Bins.WavePopulations.AddBlock(Baseline_Flyers);
        Bins.WavePopulations.AddBlock(Baseline_Nightmare);
        Bins.WavePopulations.AddBlock(Baseline_Shadows);

        // Single enemy variant population
        Bins.WavePopulations.AddBlock(OnlyGiantStrikers);
        Bins.WavePopulations.AddBlock(OnlyGiantShooters);
        Bins.WavePopulations.AddBlock(OnlyChargers);
        Bins.WavePopulations.AddBlock(OnlyHybrids);
        Bins.WavePopulations.AddBlock(OnlyInfectedHybrids);
        Bins.WavePopulations.AddBlock(OnlyShadows);
        Bins.WavePopulations.AddBlock(OnlyNightmares);

        // Shadows
        Bins.WavePopulations.AddBlock(Shadows_WithHybrids);

        // Single enemy
        Bins.WavePopulations.AddBlock(SingleEnemy_Mother);
        Bins.WavePopulations.AddBlock(SingleEnemy_PMother);
        Bins.WavePopulations.AddBlock(SingleEnemy_Tank);
        Bins.WavePopulations.AddBlock(SingleEnemy_TankPotato);
        Bins.WavePopulations.AddBlock(SingleEnemy_Pouncer);
        Bins.WavePopulations.AddBlock(SingleEnemy_PouncerShadow);
    }

    public static WavePopulation None = new() { Name = "None", PersistentId = 0 };

    #region Alarm waves
    #region Baseline waves
    /// <summary>
    /// Same as vanilla baseline
    /// </summary>
    public static WavePopulation Baseline = new WavePopulation
    {
        WaveRoleWeakling = Enemy.Shadow,
        WaveRoleStandard = Enemy.Striker_Wave,
        WaveRoleSpecial = Enemy.Shooter_Wave,
        WaveRoleMiniBoss = Enemy.StrikerGiant_Wave,
        WaveRoleBoss = Enemy.ShooterGiant,
        Name = "Baseline"
    };

    public static WavePopulation Baseline_Hybrids = new WavePopulation
    {
        WaveRoleWeakling = Enemy.Shadow,
        WaveRoleStandard = Enemy.Striker_Wave,
        WaveRoleSpecial = Enemy.Shooter_Wave,
        WaveRoleMiniBoss = Enemy.Hybrid,
        WaveRoleBoss = Enemy.Hybrid,
        Name = "Baseline_Hybrids"
    };

    public static WavePopulation Baseline_Chargers = new WavePopulation
    {
        WaveRoleWeakling = Enemy.Shadow,
        WaveRoleStandard = Enemy.Striker_Wave,
        WaveRoleSpecial = Enemy.Charger,
        WaveRoleMiniBoss = Enemy.StrikerGiant_Wave,
        WaveRoleBoss = Enemy.ChargerGiant,
        Name = "Baseline_Chargers"
    };

    public static WavePopulation Baseline_Flyers = new WavePopulation
    {
        WaveRoleWeakling = Enemy.Shadow,
        WaveRoleStandard = Enemy.Striker_Wave,
        WaveRoleSpecial = Enemy.Flyer,
        WaveRoleMiniBoss = Enemy.StrikerGiant_Wave,
        WaveRoleBoss = Enemy.FlyerBig,
        Name = "Baseline_Flyers"
    };

    public static WavePopulation Baseline_Nightmare = new WavePopulation
    {
        WaveRoleWeakling = Enemy.Shadow,
        WaveRoleStandard = Enemy.Striker_Wave,
        WaveRoleSpecial = Enemy.NightmareStriker,
        WaveRoleMiniBoss = Enemy.StrikerGiant_Wave,
        WaveRoleBoss = Enemy.NightmareShooter,
        Name = "Baseline_Nightmare"
    };

    public static WavePopulation Baseline_Shadows = new WavePopulation
    {
        WaveRoleWeakling = Enemy.Shadow,
        WaveRoleStandard = Enemy.Striker_Wave,
        WaveRoleSpecial = Enemy.Shadow,
        WaveRoleMiniBoss = Enemy.ShooterGiant,
        WaveRoleBoss = Enemy.ShadowGiant,
        Name = "Baseline_Shadows"
    };
    #endregion

    public static WavePopulation OnlyGiantStrikers = new()
    {
        WaveRoleWeakling = Enemy.StrikerGiant_Wave,
        WaveRoleStandard = Enemy.StrikerGiant_Wave,
        WaveRoleSpecial = Enemy.StrikerGiant_Wave,
        WaveRoleMiniBoss = Enemy.StrikerGiant_Wave,
        Name = "OnlyGiantShooters"
    };

    public static WavePopulation OnlyGiantShooters = new()
    {
        WaveRoleWeakling = Enemy.ShooterGiant,
        WaveRoleStandard = Enemy.ShooterGiant,
        WaveRoleSpecial = Enemy.ShooterGiant,
        WaveRoleMiniBoss = Enemy.ShooterGiant,
        Name = "OnlyGiantShooters"
    };

    public static WavePopulation OnlyChargers = new WavePopulation
    {
        WaveRoleWeakling = Enemy.Charger,
        WaveRoleStandard = Enemy.Charger,
        WaveRoleSpecial = Enemy.Charger,
        WaveRoleMiniBoss = Enemy.ChargerGiant,
        WaveRoleBoss = Enemy.ChargerGiant,
        Name = "OnlyChargers"
    };

    public static WavePopulation OnlyHybrids = new WavePopulation
    {
        WaveRoleWeakling = Enemy.Shooter_Wave,
        WaveRoleStandard = Enemy.Shooter_Wave,
        WaveRoleSpecial = Enemy.Hybrid,
        WaveRoleMiniBoss = Enemy.Hybrid,
        WaveRoleBoss = Enemy.Hybrid,
        Name = "OnlyHybrids"
    };

    public static WavePopulation OnlyInfectedHybrids = new()
    {
        WaveRoleWeakling = Enemy.Shooter_Wave,
        WaveRoleStandard = Enemy.Shooter_Wave,
        WaveRoleSpecial = (Enemy)Enemy_New.HybridInfected.PersistentId,
        WaveRoleMiniBoss = (Enemy)Enemy_New.HybridInfected.PersistentId,
        Name = "OnlyHybrids"
    };

    public static WavePopulation OnlyShadows = new WavePopulation
    {
        WaveRoleWeakling = Enemy.Shadow,
        WaveRoleStandard = Enemy.Shadow,
        WaveRoleSpecial = Enemy.Shadow,
        WaveRoleMiniBoss = Enemy.ShadowGiant,
        WaveRoleBoss = Enemy.ShadowGiant,
        Name = "OnlyShadows"
    };

    public static WavePopulation OnlyNightmares = new WavePopulation
    {
        WaveRoleWeakling = Enemy.NightmareStriker,
        WaveRoleStandard = Enemy.NightmareStriker,
        WaveRoleSpecial = Enemy.NightmareShooter,
        WaveRoleMiniBoss = Enemy.NightmareShooter,
        WaveRoleBoss = Enemy.NightmareShooter,
        DifficultyFactor = 1.20,
        Name = "OnlyNightmares"
    };

    public static WavePopulation Shadows_WithHybrids = new WavePopulation
    {
        WaveRoleWeakling = Enemy.Shadow,
        WaveRoleStandard = Enemy.Shadow,
        WaveRoleSpecial = Enemy.ShadowGiant,
        WaveRoleMiniBoss = Enemy.Hybrid,
        WaveRoleBoss = Enemy.Hybrid,
        Name = "Shadows_WithHybrids"
    };
    #endregion

    #region Specific enemies for custom waves
    public static WavePopulation Special_StrikerGiants = new WavePopulation
    {
        WaveRoleSpecial = Enemy.StrikerGiant_Wave,
        Name = "Special_StrikerGiants"
    };
    #endregion

    #region Single enemies
    public static WavePopulation SingleEnemy_Mother =     new() { WaveRoleMiniBoss = Enemy.Mother,  Name = "SingleEnemy_Mother" };
    public static WavePopulation SingleEnemy_PMother =    new() { WaveRoleMiniBoss = Enemy.PMother, Name = "SingleEnemy_PMother" };

    public static WavePopulation SingleEnemy_Tank =       new() { WaveRoleMiniBoss = Enemy.Tank,       Name = "SingleEnemy_Tank" };
    public static WavePopulation SingleEnemy_TankPotato = new() { WaveRoleMiniBoss = Enemy.TankPotato, Name = "SingleEnemy_TankPotato" };

    public static WavePopulation SingleEnemy_Pouncer =       new() { WaveRoleMiniBoss = Enemy.Pouncer, Name = "SingleEnemy_Pouncer" };
    public static WavePopulation SingleEnemy_PouncerShadow = new()
    {
        WaveRoleMiniBoss = (Enemy)Enemy_New.PouncerShadow.PersistentId,
        Name = "SingleEnemy_PouncerShadow"
    };
    #endregion
}

public record GameDataWavePopulation : WavePopulation
{
    /// <summary>
    /// We explicitly want to not have PIDs set when loading data, they come with their own
    /// Pids. This stops the Pid counter getting incremented when loading vanilla data.
    /// </summary>
    public GameDataWavePopulation() : base(PidOffsets.None)
    { }
}
