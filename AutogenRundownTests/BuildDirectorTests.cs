using AutogenRundown;
using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundownTests;

[TestClass]
public partial class BuildDirector_Tests
{
    private static void Seed(string seed = "director")
    {
        Generator.Seed = seed;
        Generator.Reload();
    }

    /// <summary>
    /// The 15 base objectives that are always in the initial pool (before filtering).
    /// </summary>
    private static readonly HashSet<WardenObjectiveType> BaseObjectives = new()
    {
        WardenObjectiveType.HsuFindSample,
        WardenObjectiveType.ReactorStartup,
        WardenObjectiveType.ReactorShutdown,
        WardenObjectiveType.GatherSmallItems,
        WardenObjectiveType.ClearPath,
        WardenObjectiveType.SpecialTerminalCommand,
        WardenObjectiveType.RetrieveBigItems,
        WardenObjectiveType.PowerCellDistribution,
        WardenObjectiveType.TerminalUplink,
        WardenObjectiveType.CentralGeneratorCluster,
        WardenObjectiveType.HsuActivateSmall,
        WardenObjectiveType.Survival,
        WardenObjectiveType.GatherTerminal,
        WardenObjectiveType.CorruptedTerminalUplink,
        WardenObjectiveType.TimedTerminalSequence,
    };

    /// <summary>
    /// Objectives that are only valid for Main bulkhead.
    /// </summary>
    private static readonly HashSet<WardenObjectiveType> MainOnlyObjectives = new()
    {
        WardenObjectiveType.HsuActivateSmall,
        WardenObjectiveType.ClearPath,
        WardenObjectiveType.Survival,
        WardenObjectiveType.ReachKdsDeep,
        WardenObjectiveType.Cryptomnesia,
    };

    /// <summary>
    /// The short-objective pool used inside KDS Deep / Cryptomnesia contexts.
    /// </summary>
    private static readonly HashSet<WardenObjectiveType> ShortObjectives = new()
    {
        WardenObjectiveType.SpecialTerminalCommand,
        WardenObjectiveType.GatherSmallItems,
        WardenObjectiveType.PowerCellDistribution,
        WardenObjectiveType.HsuFindSample,
        WardenObjectiveType.GatherTerminal,
    };

    /// <summary>
    /// Runs GenObjective many times with different seeds and collects all selected objectives.
    /// </summary>
    private static HashSet<WardenObjectiveType> CollectObjectives(
        BuildDirector director,
        List<WardenObjectiveType> exclude,
        int iterations = 300)
    {
        var seen = new HashSet<WardenObjectiveType>();
        for (int i = 0; i < iterations; i++)
        {
            Seed($"collect_{i}");
            director.GenObjective(exclude);
            seen.Add(director.Objective);
        }
        return seen;
    }

    #region Main Bulkhead

    [TestMethod]
    public void Test_GenObjective_MainBulkhead_NeverSelectsSpecialTerminalCommand()
    {
        var director = new BuildDirector
        {
            Tier = "A",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Tech,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>());
        Assert.IsFalse(seen.Contains(WardenObjectiveType.SpecialTerminalCommand),
            "Main bulkhead should never select SpecialTerminalCommand");
    }

    [TestMethod]
    public void Test_GenObjective_MainBulkhead_CanSelectMainOnlyObjectives()
    {
        var director = new BuildDirector
        {
            Tier = "A",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Tech,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>(), 500);

        Assert.IsTrue(seen.Contains(WardenObjectiveType.ClearPath),
            "Main bulkhead should be able to select ClearPath");
        Assert.IsTrue(seen.Contains(WardenObjectiveType.Survival),
            "Main bulkhead should be able to select Survival");
        Assert.IsTrue(seen.Contains(WardenObjectiveType.HsuActivateSmall),
            "Main bulkhead should be able to select HsuActivateSmall");
    }

    #endregion

    #region Non-Main Bulkheads

    [TestMethod]
    public void Test_GenObjective_ExtremeBulkhead_NeverSelectsMainOnlyObjectives()
    {
        var director = new BuildDirector
        {
            Tier = "C",
            Bulkhead = Bulkhead.Extreme,
            Complex = Complex.Mining,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>());

        foreach (var mainOnly in MainOnlyObjectives)
        {
            Assert.IsFalse(seen.Contains(mainOnly),
                $"Extreme bulkhead should never select {mainOnly}");
        }
    }

    [TestMethod]
    public void Test_GenObjective_OverloadBulkhead_NeverSelectsMainOnlyObjectives()
    {
        var director = new BuildDirector
        {
            Tier = "B",
            Bulkhead = Bulkhead.Overload,
            Complex = Complex.Service,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>());

        foreach (var mainOnly in MainOnlyObjectives)
        {
            Assert.IsFalse(seen.Contains(mainOnly),
                $"Overload bulkhead should never select {mainOnly}");
        }
    }

    [TestMethod]
    public void Test_GenObjective_ExtremeBulkhead_CanSelectSpecialTerminalCommand()
    {
        var director = new BuildDirector
        {
            Tier = "A",
            Bulkhead = Bulkhead.Extreme,
            Complex = Complex.Tech,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>(), 500);
        Assert.IsTrue(seen.Contains(WardenObjectiveType.SpecialTerminalCommand),
            "Extreme bulkhead should be able to select SpecialTerminalCommand");
    }

    #endregion

    #region ReachKdsDeep

    [TestMethod]
    public void Test_GenObjective_MiningTierC_CanSelectReachKdsDeep()
    {
        var director = new BuildDirector
        {
            Tier = "C",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Mining,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>(), 500);
        Assert.IsTrue(seen.Contains(WardenObjectiveType.ReachKdsDeep),
            "Mining Tier C Main should be able to select ReachKdsDeep");
    }

    [TestMethod]
    public void Test_GenObjective_MiningTierD_CanSelectReachKdsDeep()
    {
        var director = new BuildDirector
        {
            Tier = "D",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Mining,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>(), 500);
        Assert.IsTrue(seen.Contains(WardenObjectiveType.ReachKdsDeep),
            "Mining Tier D Main should be able to select ReachKdsDeep");
    }

    [TestMethod]
    public void Test_GenObjective_MiningTierE_CanSelectReachKdsDeep()
    {
        var director = new BuildDirector
        {
            Tier = "E",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Mining,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>(), 500);
        Assert.IsTrue(seen.Contains(WardenObjectiveType.ReachKdsDeep),
            "Mining Tier E Main should be able to select ReachKdsDeep");
    }

    [TestMethod]
    public void Test_GenObjective_MiningTierA_NeverSelectsReachKdsDeep()
    {
        var director = new BuildDirector
        {
            Tier = "A",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Mining,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>());
        Assert.IsFalse(seen.Contains(WardenObjectiveType.ReachKdsDeep),
            "Mining Tier A should never select ReachKdsDeep");
    }

    [TestMethod]
    public void Test_GenObjective_MiningTierB_NeverSelectsReachKdsDeep()
    {
        var director = new BuildDirector
        {
            Tier = "B",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Mining,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>());
        Assert.IsFalse(seen.Contains(WardenObjectiveType.ReachKdsDeep),
            "Mining Tier B should never select ReachKdsDeep");
    }

    [TestMethod]
    public void Test_GenObjective_TechComplex_NeverSelectsReachKdsDeep()
    {
        var director = new BuildDirector
        {
            Tier = "E",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Tech,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>());
        Assert.IsFalse(seen.Contains(WardenObjectiveType.ReachKdsDeep),
            "Tech complex should never select ReachKdsDeep");
    }

    [TestMethod]
    public void Test_GenObjective_ServiceComplex_NeverSelectsReachKdsDeep()
    {
        var director = new BuildDirector
        {
            Tier = "E",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Service,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>());
        Assert.IsFalse(seen.Contains(WardenObjectiveType.ReachKdsDeep),
            "Service complex should never select ReachKdsDeep");
    }

    [TestMethod]
    public void Test_GenObjective_MiningTierC_Extreme_NeverSelectsReachKdsDeep()
    {
        // ReachKdsDeep is Main-only, even when mining C+
        var director = new BuildDirector
        {
            Tier = "C",
            Bulkhead = Bulkhead.Extreme,
            Complex = Complex.Mining,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>());
        Assert.IsFalse(seen.Contains(WardenObjectiveType.ReachKdsDeep),
            "Extreme bulkhead should never select ReachKdsDeep even on Mining C+");
    }

    #endregion

    #region KDS Deep / Cryptomnesia context (short objective pool)

    [TestMethod]
    public void Test_GenObjective_InsideKdsDeep_UsesShortObjectivePool()
    {
        var director = new BuildDirector
        {
            Tier = "D",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Mining,
        };
        var exclude = new List<WardenObjectiveType> { WardenObjectiveType.ReachKdsDeep };

        var seen = CollectObjectives(director, exclude, 500);

        // Should only select from the short objectives pool
        foreach (var obj in seen)
        {
            Assert.IsTrue(ShortObjectives.Contains(obj),
                $"Inside KDS Deep context, {obj} should not be selectable");
        }
    }

    [TestMethod]
    public void Test_GenObjective_InsideCryptomnesia_UsesShortObjectivePool()
    {
        var director = new BuildDirector
        {
            Tier = "E",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Mining,
        };
        var exclude = new List<WardenObjectiveType> { WardenObjectiveType.Cryptomnesia };

        var seen = CollectObjectives(director, exclude, 500);

        foreach (var obj in seen)
        {
            Assert.IsTrue(ShortObjectives.Contains(obj),
                $"Inside Cryptomnesia context, {obj} should not be selectable");
        }
    }

    [TestMethod]
    public void Test_GenObjective_InsideKdsDeep_ExcludeStillApplied()
    {
        var director = new BuildDirector
        {
            Tier = "D",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Mining,
        };
        var exclude = new List<WardenObjectiveType>
        {
            WardenObjectiveType.ReachKdsDeep,
            WardenObjectiveType.GatherSmallItems,
            WardenObjectiveType.HsuFindSample,
        };

        var seen = CollectObjectives(director, exclude, 300);

        Assert.IsFalse(seen.Contains(WardenObjectiveType.GatherSmallItems));
        Assert.IsFalse(seen.Contains(WardenObjectiveType.HsuFindSample));
    }

    #endregion

    #region Overload + Survival excluded

    [TestMethod]
    public void Test_GenObjective_OverloadWithSurvivalExcluded_ForcesSpecialTerminalCommand()
    {
        var director = new BuildDirector
        {
            Tier = "C",
            Bulkhead = Bulkhead.Overload,
            Complex = Complex.Tech,
        };
        var exclude = new List<WardenObjectiveType> { WardenObjectiveType.Survival };

        var seen = CollectObjectives(director, exclude, 100);

        Assert.AreEqual(1, seen.Count, "Should only select one objective type");
        Assert.IsTrue(seen.Contains(WardenObjectiveType.SpecialTerminalCommand),
            "Overload with Survival excluded should force SpecialTerminalCommand");
    }

    #endregion

    #region Exclude list

    [TestMethod]
    public void Test_GenObjective_ExcludedObjectivesNeverSelected()
    {
        var director = new BuildDirector
        {
            Tier = "B",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Tech,
        };
        var exclude = new List<WardenObjectiveType>
        {
            WardenObjectiveType.ReactorStartup,
            WardenObjectiveType.ReactorShutdown,
            WardenObjectiveType.GatherSmallItems,
        };

        var seen = CollectObjectives(director, exclude, 500);

        foreach (var excluded in exclude)
        {
            Assert.IsFalse(seen.Contains(excluded),
                $"Excluded objective {excluded} should never be selected");
        }
    }

    [TestMethod]
    public void Test_GenObjective_EmptyExcludeList_AllBaseObjectivesAvailable()
    {
        var director = new BuildDirector
        {
            Tier = "A",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Tech,
        };

        var seen = CollectObjectives(director, new List<WardenObjectiveType>(), 1000);

        // Main bulkhead: base objectives minus SpecialTerminalCommand, plus no ReachKdsDeep (Tech, Tier A)
        var expected = new HashSet<WardenObjectiveType>(BaseObjectives);
        expected.Remove(WardenObjectiveType.SpecialTerminalCommand);

        foreach (var obj in expected)
        {
            Assert.IsTrue(seen.Contains(obj),
                $"Expected objective {obj} to be selectable but was never seen");
        }
    }

    #endregion

    #region Determinism

    [TestMethod]
    public void Test_GenObjective_IsDeterministic()
    {
        var director = new BuildDirector
        {
            Tier = "C",
            Bulkhead = Bulkhead.Main,
            Complex = Complex.Mining,
        };
        var exclude = new List<WardenObjectiveType>();

        Seed("determinism_test");
        director.GenObjective(exclude);
        var first = director.Objective;

        Seed("determinism_test");
        director.GenObjective(exclude);
        var second = director.Objective;

        Assert.AreEqual(first, second);
    }

    #endregion

    #region Result always valid

    [TestMethod]
    public void Test_GenObjective_ResultIsAlwaysAValidObjective()
    {
        var allValid = new HashSet<WardenObjectiveType>(BaseObjectives);
        allValid.Add(WardenObjectiveType.ReachKdsDeep);

        // Test across different configurations
        var configs = new[]
        {
            (tier: "A", bulkhead: Bulkhead.Main, complex: Complex.Mining),
            (tier: "C", bulkhead: Bulkhead.Main, complex: Complex.Mining),
            (tier: "E", bulkhead: Bulkhead.Main, complex: Complex.Tech),
            (tier: "B", bulkhead: Bulkhead.Extreme, complex: Complex.Service),
            (tier: "D", bulkhead: Bulkhead.Overload, complex: Complex.Mining),
        };

        foreach (var (tier, bulkhead, complex) in configs)
        {
            var director = new BuildDirector
            {
                Tier = tier,
                Bulkhead = bulkhead,
                Complex = complex,
            };

            for (int i = 0; i < 50; i++)
            {
                Seed($"valid_{tier}_{bulkhead}_{complex}_{i}");
                director.GenObjective(new List<WardenObjectiveType>());
                Assert.IsTrue(allValid.Contains(director.Objective),
                    $"Objective {director.Objective} is not a recognized objective type " +
                    $"(tier={tier}, bulkhead={bulkhead}, complex={complex})");
            }
        }
    }

    #endregion
}
