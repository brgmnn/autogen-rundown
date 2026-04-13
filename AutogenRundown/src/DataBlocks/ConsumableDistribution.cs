using AutogenRundown.DataBlocks.Items;
using AutogenRundown.PeerMods;

namespace AutogenRundown.DataBlocks;

public record ConsumableDistribution : DataBlock<ConsumableDistribution>
{
    /// <summary>
    /// Mostly glowsticks and lock melters, with a small chance of tripmines and c-foam. Also include long range
    /// flashlight by popular demand.
    /// This distribution is intended to be applicable for any zone.
    /// </summary>
    public static ConsumableDistribution Baseline = new()
    {
        Name = "Baseline",
        PersistentId = 500,
        SpawnsPerZone = 6,
        SpawnData = Collections.Flatten(
            ItemSpawn.GlowSticks(8.0),
            new List<ItemSpawn> {
                new() { Weight = 5.0, Item = Items.Item.LockMelter },
                new() { Weight = 1.0, Item = Items.Item.CfoamGrenade },
                new() { Weight = 1.0, Item = Items.Item.ExplosiveTripmine },
                new() { Weight = 1.0, Item = Items.Item.CfoamTripmine },
                new() { Weight = 1.0, Item = Items.Item.LongRangeFlashlight },
                new() { Weight = 1.0, Item = Items.Item.FogRepeller }
            }
        ),
    };

    /// <summary>
    /// Variant of Baseline but for Tech labs
    /// </summary>
    public static ConsumableDistribution Baseline_TechComplex = new()
    {
        Name = "Baseline_TechComplex",
        PersistentId = 502,
        SpawnsPerZone = 6,
        SpawnData = Collections.Flatten(
            ItemSpawn.GlowSticks(8.0),
            new List<ItemSpawn> {
                new() { Weight = 2.0, Item = Items.Item.LockMelter },
                new() { Weight = 2.0, Item = Items.Item.Syringe_Health },
                new() { Weight = 2.0, Item = Items.Item.Syringe_Melee },
                new() { Weight = 1.0, Item = Items.Item.CfoamGrenade },
                new() { Weight = 1.0, Item = Items.Item.ExplosiveTripmine },
                new() { Weight = 1.0, Item = Items.Item.LongRangeFlashlight },
                new() { Weight = 1.0, Item = Items.Item.FogRepeller }
            }
        ),
    };

    public static ConsumableDistribution Baseline_Syringes = new()
    {
        Name = "Baseline_Syringes",
        PersistentId = 504,
        SpawnsPerZone = 6,
        SpawnData = Collections.Flatten(
            ItemSpawn.GlowSticks(2.0),
            new List<ItemSpawn> {
                new() { Weight = 2.0, Item = Items.Item.Syringe_Health },
                new() { Weight = 1.0, Item = Items.Item.Syringe_Melee },
            }
        ),
    };

    /// <summary>
    /// Baseline with heavy weighting to fog repellers. Averages to 4 repellers, 2 glowsticks,
    /// and 1 lock melter per zone.
    /// </summary>
    public static ConsumableDistribution Baseline_FogRepellers = new()
    {
        Name = "Baseline_FogRepellers",
        PersistentId = 505,
        SpawnsPerZone = 8,
        SpawnData = Collections.Flatten(
            ItemSpawn.GlowSticks(3.0),
            new List<ItemSpawn> {
                new() { Weight = 1.0, Item = Items.Item.LockMelter },
                new() { Weight = 4.0, Item = Items.Item.FogRepeller }
            }
        )
    };

    public static ConsumableDistribution Baseline_LockMelters = new()
    {
        Name = "Baseline_LockMelters",
        PersistentId = 506,
        SpawnsPerZone = 6,
        SpawnData = Collections.Flatten(
            ItemSpawn.GlowSticks(2.0),
            new List<ItemSpawn> {
                new() { Weight = 8.0, Item = Items.Item.LockMelter },
                new() { Weight = 1.0, Item = Items.Item.FogRepeller }
            }
        )
    };

    /// <summary>
    /// Spawns a lot of fog repellers. We need this for certain alarm zones
    /// </summary>
    public static ConsumableDistribution Alarms_FogRepellers = new()
    {
        Name = "Alarms_FogRepellers",
        PersistentId = 550,
        SpawnsPerZone = 12,
        SpawnData = Collections.Flatten(
            ItemSpawn.GlowSticks(),
            new List<ItemSpawn> {
                new() { Weight = 8.0, Item = Items.Item.FogRepeller }
            }
        )
    };

    /// <summary>
    /// Baseline with heavy weighting to fog repellers. Averages to 4 repellers, 2 glowsticks,
    /// and 1 lock melter per zone.
    /// </summary>
    public static ConsumableDistribution Reactor_FogRepellers = new()
    {
        Name = "Reactor_FogRepellers",
        PersistentId = 590,
        SpawnsPerZone = 10,
        SpawnData = Collections.Flatten(
            ItemSpawn.GlowSticks(2.0),
            new List<ItemSpawn> {
                new() { Weight = 8.0, Item = Items.Item.FogRepeller }
            }
        )
    };

    /// <summary>
    ///
    /// </summary>
    public static ConsumableDistribution MedicalBay_Consumables = new()
    {
        Name = "MedicalBay_Consumables",
        PersistentId = 610,
        SpawnsPerZone = 8,
        SpawnData = Collections.Flatten(
            new List<ItemSpawn> {
                new() { Weight = 6.0, Item = Items.Item.Syringe_Health },
                new() { Weight = 3.0, Item = Items.Item.Syringe_Melee },
                new() { Weight = 1.0, Item = Items.Item.FogRepeller }
            }
        )
    };

    public static ConsumableDistribution? MedicalBay_GTFriendlyO_Healing;
    public static ConsumableDistribution? MedicalBay_GTFriendlyO_Combat;

    public new static void SaveStatic()
    {
        if (Peers.HasMod("GTFriendlyO"))
        {
            // Enhance existing distributions with custom syringes
            Baseline.SpawnData.AddRange(new List<ItemSpawn>
            {
                new() { Weight = 0.2, Item = Items.Item.Syringe_Health },
                new() { Weight = 0.2, Item = Items.Item.Syringe_Melee },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_Speed },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_Adrenaline },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_Antibiotic },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_Recovery },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_Recovery2 },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_AntibioticIX },
            });

            Baseline_TechComplex.SpawnData.AddRange(new List<ItemSpawn>
            {
                new() { Weight = 0.7, Item = Items.Item.ModGtfriendly_Syringe_Speed },
                new() { Weight = 0.7, Item = Items.Item.ModGtfriendly_Syringe_Antibiotic },
                new() { Weight = 0.5, Item = Items.Item.ModGtfriendly_Syringe_Recovery },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_Adrenaline },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_HealMunitionsDrain },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_VirusBomb },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_HealthSurge },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_Recovery2 },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_Rage },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_AmmoSymbiotic },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_VirusNuke },
                new() { Weight = 0.1, Item = Items.Item.ModGtfriendly_Syringe_AntibioticIX },
            });

            Baseline_Syringes.SpawnData.AddRange(new List<ItemSpawn>
            {
                new() { Weight = 0.8, Item = Items.Item.ModGtfriendly_Syringe_Speed },
                new() { Weight = 0.8, Item = Items.Item.ModGtfriendly_Syringe_Antibiotic },
                new() { Weight = 0.6, Item = Items.Item.ModGtfriendly_Syringe_Recovery },
                new() { Weight = 0.5, Item = Items.Item.ModGtfriendly_Syringe_Adrenaline },
            });

            MedicalBay_Consumables.SpawnData.AddRange(new List<ItemSpawn>
            {
                new() { Weight = 2.0, Item = Items.Item.ModGtfriendly_Syringe_Antibiotic },
                new() { Weight = 1.5, Item = Items.Item.ModGtfriendly_Syringe_Recovery },
                new() { Weight = 1.5, Item = Items.Item.ModGtfriendly_Syringe_AntibioticIX },
            });

            // New syringe-only med bay distributions
            MedicalBay_GTFriendlyO_Healing = new()
            {
                Name = "MedicalBay_GTFriendlyO_Healing",
                PersistentId = 800,
                SpawnsPerZone = 10,
                SpawnData = new List<ItemSpawn>
                {
                    new() { Weight = 5.0, Item = Items.Item.Syringe_Health },
                    new() { Weight = 4.0, Item = Items.Item.ModGtfriendly_Syringe_Antibiotic },
                    new() { Weight = 2.5, Item = Items.Item.ModGtfriendly_Syringe_Recovery },
                    new() { Weight = 2.0, Item = Items.Item.ModGtfriendly_Syringe_Recovery2 },
                    new() { Weight = 2.0, Item = Items.Item.ModGtfriendly_Syringe_AntibioticIX },
                    new() { Weight = 1.5, Item = Items.Item.ModGtfriendly_Syringe_HealMunitionsDrain },
                    new() { Weight = 1.5, Item = Items.Item.ModGtfriendly_Syringe_Adrenaline },
                    new() { Weight = 1.0, Item = Items.Item.FogRepeller },
                }
            };

            MedicalBay_GTFriendlyO_Combat = new()
            {
                Name = "MedicalBay_GTFriendlyO_Combat",
                PersistentId = 801,
                SpawnsPerZone = 10,
                SpawnData = new List<ItemSpawn>
                {
                    new() { Weight = 4.0, Item = Items.Item.Syringe_Health },
                    new() { Weight = 3.0, Item = Items.Item.Syringe_Melee },
                    new() { Weight = 2.5, Item = Items.Item.ModGtfriendly_Syringe_Speed },
                    new() { Weight = 2.0, Item = Items.Item.ModGtfriendly_Syringe_Adrenaline },
                    new() { Weight = 1.5, Item = Items.Item.ModGtfriendly_Syringe_HealthSurge },
                    new() { Weight = 1.0, Item = Items.Item.ModGtfriendly_Syringe_AmmoSymbiotic },
                    new() { Weight = 1.0, Item = Items.Item.ModGtfriendly_Syringe_Rage },
                    new() { Weight = 0.5, Item = Items.Item.ModGtfriendly_Syringe_VirusBomb },
                    new() { Weight = 0.5, Item = Items.Item.ModGtfriendly_Syringe_VirusNuke },
                    new() { Weight = 1.0, Item = Items.Item.FogRepeller },
                }
            };

            Bins.ConsumableDistributions.AddBlock(MedicalBay_GTFriendlyO_Healing);
            Bins.ConsumableDistributions.AddBlock(MedicalBay_GTFriendlyO_Combat);
        }

        Bins.ConsumableDistributions.AddBlock(Baseline);
        Bins.ConsumableDistributions.AddBlock(Baseline_TechComplex);
        Bins.ConsumableDistributions.AddBlock(Baseline_FogRepellers);
        Bins.ConsumableDistributions.AddBlock(Baseline_LockMelters);
        Bins.ConsumableDistributions.AddBlock(Baseline_Syringes);

        Bins.ConsumableDistributions.AddBlock(Alarms_FogRepellers);

        Bins.ConsumableDistributions.AddBlock(Reactor_FogRepellers);

        Bins.ConsumableDistributions.AddBlock(MedicalBay_Consumables);
    }

    public uint SpawnsPerZone { get; set; } = 5;

    public double ChanceToSpawnInResourceContainer { get; set; } = 0.9;

    public List<ItemSpawn> SpawnData { get; set; } = new();
}
