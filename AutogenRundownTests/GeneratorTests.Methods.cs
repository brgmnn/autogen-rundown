using AutogenRundown;

namespace AutogenRundownTests;

public partial class Generator_Tests
{
    private static void Seed(string seed = "test")
    {
        Generator.Seed = seed;
        Generator.Reload();
    }

    #region Reload / Determinism

    [TestMethod]
    public void Test_Reload_SameSeedProducesSameSequence()
    {
        Seed("determinism");
        var a1 = Generator.Random.Next();
        var a2 = Generator.Random.Next();
        var a3 = Generator.Random.Next();

        Seed("determinism");
        Assert.AreEqual(a1, Generator.Random.Next());
        Assert.AreEqual(a2, Generator.Random.Next());
        Assert.AreEqual(a3, Generator.Random.Next());
    }

    [TestMethod]
    public void Test_Reload_DifferentSeedsProduceDifferentSequences()
    {
        Seed("alpha");
        var a = Generator.Random.Next();

        Seed("beta");
        var b = Generator.Random.Next();

        Assert.AreNotEqual(a, b);
    }

    #endregion

    #region GetHashCode

    [TestMethod]
    public void Test_GetHashCode_Deterministic()
    {
        var hash1 = Generator.GetHashCode("hello");
        var hash2 = Generator.GetHashCode("hello");

        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void Test_GetHashCode_DifferentStrings()
    {
        var hash1 = Generator.GetHashCode("hello");
        var hash2 = Generator.GetHashCode("world");

        Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void Test_GetHashCode_EmptyString()
    {
        var hash = Generator.GetHashCode("");
        // Should not throw and should return a consistent value
        Assert.AreEqual(Generator.GetHashCode(""), hash);
    }

    [TestMethod]
    public void Test_GetHashCode_SingleChar()
    {
        var hash = Generator.GetHashCode("A");
        Assert.AreEqual(Generator.GetHashCode("A"), hash);
    }

    [TestMethod]
    public void Test_GetHashCode_SimilarStrings()
    {
        // Strings that differ by one char should hash differently
        Assert.AreNotEqual(
            Generator.GetHashCode("2025_01_01"),
            Generator.GetHashCode("2025_01_02"));
    }

    #endregion

    #region Between

    [TestMethod]
    public void Test_Between_MinEqualsMax()
    {
        Seed("between");
        Assert.AreEqual(5, Generator.Between(5, 5));
    }

    [TestMethod]
    public void Test_Between_ReturnsWithinRange()
    {
        Seed("between_range");

        for (int i = 0; i < 100; i++)
        {
            var result = Generator.Between(10, 20);
            Assert.IsTrue(result >= 10, $"Result {result} < 10");
            Assert.IsTrue(result <= 20, $"Result {result} > 20");
        }
    }

    [TestMethod]
    public void Test_Between_IsDeterministic()
    {
        Seed("between_det");
        var first = Generator.Between(0, 1000);

        Seed("between_det");
        Assert.AreEqual(first, Generator.Between(0, 1000));
    }

    #endregion

    #region NextDouble

    [TestMethod]
    public void Test_NextDouble_ReturnsInZeroOneRange()
    {
        Seed("nextdouble");

        for (int i = 0; i < 100; i++)
        {
            var result = Generator.NextDouble();
            Assert.IsTrue(result >= 0.0, $"Result {result} < 0");
            Assert.IsTrue(result < 1.0, $"Result {result} >= 1");
        }
    }

    [TestMethod]
    public void Test_NextDouble_WithMax()
    {
        Seed("nextdouble_max");

        for (int i = 0; i < 100; i++)
        {
            var result = Generator.NextDouble(5.0);
            Assert.IsTrue(result >= 0.0);
            Assert.IsTrue(result < 5.0);
        }
    }

    [TestMethod]
    public void Test_NextDouble_WithMinMax()
    {
        Seed("nextdouble_minmax");

        for (int i = 0; i < 100; i++)
        {
            var result = Generator.NextDouble(10.0, 20.0);
            Assert.IsTrue(result >= 10.0, $"Result {result} < 10");
            Assert.IsTrue(result < 20.0, $"Result {result} >= 20");
        }
    }

    #endregion

    #region Flip

    [TestMethod]
    public void Test_Flip_ZeroChanceAlwaysFalse()
    {
        Seed("flip_zero");

        for (int i = 0; i < 100; i++)
            Assert.IsFalse(Generator.Flip(0.0));
    }

    [TestMethod]
    public void Test_Flip_OneChanceAlwaysTrue()
    {
        Seed("flip_one");

        for (int i = 0; i < 100; i++)
            Assert.IsTrue(Generator.Flip(1.0));
    }

    [TestMethod]
    public void Test_Flip_IsDeterministic()
    {
        Seed("flip_det");
        var results1 = Enumerable.Range(0, 20).Select(_ => Generator.Flip()).ToList();

        Seed("flip_det");
        var results2 = Enumerable.Range(0, 20).Select(_ => Generator.Flip()).ToList();

        CollectionAssert.AreEqual(results1, results2);
    }

    #endregion

    #region Pick

    [TestMethod]
    public void Test_Pick_EmptyCollectionReturnsDefault()
    {
        Seed("pick_empty");
        var result = Generator.Pick(new List<string>());
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Test_Pick_SingleElementReturnsThatElement()
    {
        Seed("pick_single");
        var result = Generator.Pick(new List<string> { "only" });
        Assert.AreEqual("only", result);
    }

    [TestMethod]
    public void Test_Pick_ReturnsElementFromCollection()
    {
        Seed("pick_multi");
        var items = new List<string> { "a", "b", "c", "d" };
        var result = Generator.Pick(items);
        Assert.IsTrue(items.Contains(result!));
    }

    [TestMethod]
    public void Test_Pick_DoesNotModifyCollection()
    {
        Seed("pick_nomod");
        var items = new List<string> { "a", "b", "c" };
        Generator.Pick(items);
        Assert.AreEqual(3, items.Count);
    }

    #endregion

    #region Draw

    [TestMethod]
    public void Test_Draw_EmptyCollectionReturnsDefault()
    {
        Seed("draw_empty");
        var result = Generator.Draw(new List<string>());
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Test_Draw_RemovesElementFromCollection()
    {
        Seed("draw_remove");
        var items = new List<string> { "a", "b", "c" };
        var drawn = Generator.Draw(items);

        Assert.IsNotNull(drawn);
        Assert.AreEqual(2, items.Count);
        Assert.IsFalse(items.Contains(drawn));
    }

    [TestMethod]
    public void Test_Draw_SingleElementEmptiesCollection()
    {
        Seed("draw_single");
        var items = new List<string> { "only" };
        var drawn = Generator.Draw(items);

        Assert.AreEqual("only", drawn);
        Assert.AreEqual(0, items.Count);
    }

    [TestMethod]
    public void Test_Draw_AllElementsCanBeDrawn()
    {
        Seed("draw_all");
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var drawn = new List<int>();

        while (items.Count > 0)
            drawn.Add(Generator.Draw(items));

        Assert.AreEqual(5, drawn.Count);
        CollectionAssert.AreEquivalent(new List<int> { 1, 2, 3, 4, 5 }, drawn);
    }

    #endregion

    #region Shuffle

    [TestMethod]
    public void Test_Shuffle_PreservesAllElements()
    {
        Seed("shuffle_preserve");
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var shuffled = Generator.Shuffle(items);

        Assert.AreEqual(5, shuffled.Count);
        CollectionAssert.AreEquivalent(items, shuffled);
    }

    [TestMethod]
    public void Test_Shuffle_DoesNotModifyOriginal()
    {
        Seed("shuffle_nomod");
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var original = new List<int>(items);
        Generator.Shuffle(items);

        CollectionAssert.AreEqual(original, items);
    }

    [TestMethod]
    public void Test_Shuffle_IsDeterministic()
    {
        Seed("shuffle_det");
        var shuffled1 = Generator.Shuffle(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

        Seed("shuffle_det");
        var shuffled2 = Generator.Shuffle(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

        CollectionAssert.AreEqual(shuffled1, shuffled2);
    }

    [TestMethod]
    public void Test_Shuffle_SingleElement()
    {
        Seed("shuffle_one");
        var shuffled = Generator.Shuffle(new List<int> { 42 });
        Assert.AreEqual(1, shuffled.Count);
        Assert.AreEqual(42, shuffled[0]);
    }

    [TestMethod]
    public void Test_Shuffle_EmptyCollection()
    {
        Seed("shuffle_empty");
        var shuffled = Generator.Shuffle(new List<int>());
        Assert.AreEqual(0, shuffled.Count);
    }

    #endregion

    #region Select

    [TestMethod]
    public void Test_Select_SingleElementAlwaysReturnsThatElement()
    {
        Seed("select_single");
        var items = new List<(double, string)> { (1.0, "only") };

        for (int i = 0; i < 10; i++)
            Assert.AreEqual("only", Generator.Select(items));
    }

    [TestMethod]
    public void Test_Select_ZeroWeightElementNeverSelected()
    {
        Seed("select_zero");
        var items = new List<(double, string)>
        {
            (0.0, "never"),
            (1.0, "always")
        };

        for (int i = 0; i < 50; i++)
            Assert.AreEqual("always", Generator.Select(items));
    }

    [TestMethod]
    public void Test_Select_IsDeterministic()
    {
        Seed("select_det");
        var items = new List<(double, string)>
        {
            (1.0, "a"),
            (1.0, "b"),
            (1.0, "c")
        };
        var result1 = Generator.Select(items);

        Seed("select_det");
        var result2 = Generator.Select(items);

        Assert.AreEqual(result1, result2);
    }

    [TestMethod]
    public void Test_Select_RespectsWeights()
    {
        // With a very dominant weight, the heavy item should be selected much more often
        Seed("select_weights");
        var counts = new Dictionary<string, int> { { "heavy", 0 }, { "light", 0 } };

        for (int i = 0; i < 1000; i++)
        {
            // Re-seed each iteration to get different random values
            Generator.Seed = $"select_weight_{i}";
            Generator.Reload();

            var items = new List<(double, string)>
            {
                (100.0, "heavy"),
                (1.0, "light")
            };
            counts[Generator.Select(items)]++;
        }

        Assert.IsTrue(counts["heavy"] > counts["light"],
            $"Heavy={counts["heavy"]}, Light={counts["light"]}");
    }

    [TestMethod]
    public void Test_Select_DoesNotModifyCollection()
    {
        Seed("select_nomod");
        var items = new List<(double, string)>
        {
            (1.0, "a"),
            (1.0, "b"),
            (1.0, "c")
        };
        Generator.Select(items);
        Assert.AreEqual(3, items.Count);
    }

    #endregion

    #region SelectRun

    [TestMethod]
    public void Test_SelectRun_ExecutesExactlyOneAction()
    {
        Seed("selectrun");
        var callCount = 0;

        var actions = new List<(double, Action)>
        {
            (1.0, () => callCount++),
            (1.0, () => callCount++),
            (1.0, () => callCount++)
        };

        Generator.SelectRun(actions);
        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void Test_SelectRun_SingleActionAlwaysExecuted()
    {
        Seed("selectrun_single");
        var executed = false;

        var actions = new List<(double, Action)>
        {
            (1.0, () => executed = true)
        };

        Generator.SelectRun(actions);
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void Test_SelectRun_ZeroWeightNeverExecuted()
    {
        Seed("selectrun_zero");
        var neverCalled = true;

        var actions = new List<(double, Action)>
        {
            (0.0, () => neverCalled = false),
            (1.0, () => { })
        };

        for (int i = 0; i < 50; i++)
        {
            Seed($"selectrun_zero_{i}");
            Generator.SelectRun(actions);
        }

        Assert.IsTrue(neverCalled);
    }

    #endregion

    #region DrawSelect (additional tests)

    [TestMethod]
    public void Test_DrawSelect_RemovesElementWhenCountReachesZero()
    {
        Seed("drawselect_remove");
        var items = new List<(double, int, string)>
        {
            (1.0, 1, "once"),
            (1.0, 5, "many")
        };

        // Draw from it repeatedly until "once" would be depleted
        var drawn = new List<string>();
        for (int i = 0; i < 6; i++)
            drawn.Add(Generator.DrawSelect(items));

        // The item with count=1 should have been removed after being drawn
        // Collection should only have "many" remaining (or be empty)
        foreach (var remaining in items)
            Assert.AreEqual("many", remaining.Item3);
    }

    [TestMethod]
    public void Test_DrawSelect_DepletesEntirePool()
    {
        Seed("drawselect_deplete");
        var items = new List<(double, int, string)>
        {
            (1.0, 2, "a"),
            (1.0, 1, "b")
        };

        var drawn = new List<string>();
        for (int i = 0; i < 3; i++)
            drawn.Add(Generator.DrawSelect(items));

        Assert.AreEqual(0, items.Count);
        Assert.AreEqual(3, drawn.Count);
        CollectionAssert.AreEquivalent(
            new List<string> { "a", "a", "b" },
            drawn);
    }

    [TestMethod]
    public void Test_DrawSelect_IsDeterministic()
    {
        Seed("drawselect_det");
        var items1 = new List<(double, int, string)>
        {
            (1.0, 3, "x"),
            (2.0, 2, "y"),
            (0.5, 4, "z")
        };
        var drawn1 = new List<string>();
        for (int i = 0; i < 9; i++)
            drawn1.Add(Generator.DrawSelect(items1));

        Seed("drawselect_det");
        var items2 = new List<(double, int, string)>
        {
            (1.0, 3, "x"),
            (2.0, 2, "y"),
            (0.5, 4, "z")
        };
        var drawn2 = new List<string>();
        for (int i = 0; i < 9; i++)
            drawn2.Add(Generator.DrawSelect(items2));

        CollectionAssert.AreEqual(drawn1, drawn2);
    }

    #endregion

    #region DrawSelectFrequency

    [TestMethod]
    public void Test_DrawSelectFrequency_IgnoresWeightsUsesCount()
    {
        // Item with high weight but low count vs low weight but high count
        // Frequency-based should favor the high-count item
        var highCountWins = 0;
        var highWeightWins = 0;

        for (int i = 0; i < 200; i++)
        {
            Seed($"drawfreq_{i}");
            var items = new List<(double, int, string)>
            {
                (100.0, 1, "high_weight"),
                (0.01, 50, "high_count")
            };
            var result = Generator.DrawSelectFrequency(items);
            if (result == "high_count") highCountWins++;
            else highWeightWins++;
        }

        Assert.IsTrue(highCountWins > highWeightWins,
            $"HighCount={highCountWins}, HighWeight={highWeightWins}");
    }

    [TestMethod]
    public void Test_DrawSelectFrequency_DecrementsCount()
    {
        Seed("drawfreq_dec");
        var items = new List<(double, int, string)>
        {
            (1.0, 3, "item")
        };

        Generator.DrawSelectFrequency(items);

        Assert.AreEqual(1, items.Count);
        Assert.AreEqual(2, items[0].Item2);
    }

    [TestMethod]
    public void Test_DrawSelectFrequency_RemovesAtZeroCount()
    {
        Seed("drawfreq_zero");
        var items = new List<(double, int, string)>
        {
            (1.0, 1, "once")
        };

        Generator.DrawSelectFrequency(items);

        Assert.AreEqual(0, items.Count);
    }

    [TestMethod]
    public void Test_DrawSelectFrequency_DepletesEntirePool()
    {
        Seed("drawfreq_deplete");
        var items = new List<(double, int, string)>
        {
            (5.0, 2, "a"),
            (1.0, 3, "b")
        };

        var drawn = new List<string>();
        for (int i = 0; i < 5; i++)
            drawn.Add(Generator.DrawSelectFrequency(items));

        Assert.AreEqual(0, items.Count);
        Assert.AreEqual(5, drawn.Count);
        CollectionAssert.AreEquivalent(
            new List<string> { "a", "a", "b", "b", "b" },
            drawn);
    }

    #endregion

    #region GetPersistentId

    [TestMethod]
    public void Test_GetPersistentId_NoneReturnsZero()
    {
        Assert.AreEqual(0u, Generator.GetPersistentId(PidOffsets.None));
    }

    [TestMethod]
    public void Test_GetPersistentId_NormalIncrements()
    {
        var startPid = Generator.pid;
        var id1 = Generator.GetPersistentId(PidOffsets.Normal);
        var id2 = Generator.GetPersistentId(PidOffsets.Normal);

        Assert.AreEqual(startPid, id1);
        Assert.AreEqual(startPid + 1, id2);
    }

    [TestMethod]
    public void Test_GetPersistentId_NoneDoesNotIncrement()
    {
        var startPid = Generator.pid;
        Generator.GetPersistentId(PidOffsets.None);
        Generator.GetPersistentId(PidOffsets.None);
        Assert.AreEqual(startPid, Generator.pid);
    }

    #endregion

    #region AdvanceSequence

    [TestMethod]
    public void Test_AdvanceSequence_ChangesRandomState()
    {
        Seed("advance");
        var before = Generator.Random.Next();

        Seed("advance");
        Generator.AdvanceSequence(1);
        var after = Generator.Random.Next();

        Assert.AreNotEqual(before, after);
    }

    [TestMethod]
    public void Test_AdvanceSequence_IsDeterministic()
    {
        Seed("advance_det");
        Generator.AdvanceSequence(10);
        var result1 = Generator.Random.Next();

        Seed("advance_det");
        Generator.AdvanceSequence(10);
        var result2 = Generator.Random.Next();

        Assert.AreEqual(result1, result2);
    }

    [TestMethod]
    public void Test_AdvanceSequence_ZeroRoundsNoChange()
    {
        Seed("advance_zero");
        var expected = Generator.Random.Next();

        Seed("advance_zero");
        Generator.AdvanceSequence(0);
        var actual = Generator.Random.Next();

        Assert.AreEqual(expected, actual);
    }

    #endregion

    #region ShortHexHash

    [TestMethod]
    public void Test_ShortHexHash_ReturnsFourCharHexString()
    {
        Seed("hex");
        var hash = Generator.ShortHexHash();

        Assert.AreEqual(4, hash.Length);
        Assert.IsTrue(hash.All(c => "0123456789ABCDEF".Contains(c)),
            $"'{hash}' contains non-hex characters");
    }

    [TestMethod]
    public void Test_ShortHexHash_IsDeterministic()
    {
        Seed("hex_det");
        var hash1 = Generator.ShortHexHash();

        Seed("hex_det");
        var hash2 = Generator.ShortHexHash();

        Assert.AreEqual(hash1, hash2);
    }

    #endregion

    #region GetSeason

    [TestMethod]
    [DataRow(12, "Winter")]
    [DataRow(1, "Winter")]
    [DataRow(2, "Winter")]
    [DataRow(3, "Spring")]
    [DataRow(4, "Spring")]
    [DataRow(5, "Spring")]
    [DataRow(6, "Summer")]
    [DataRow(7, "Summer")]
    [DataRow(8, "Summer")]
    [DataRow(9, "Fall")]
    [DataRow(10, "Fall")]
    [DataRow(11, "Fall")]
    public void Test_GetSeason_MapsMonthCorrectly(int month, string expectedSeason)
    {
        Assert.AreEqual(expectedSeason, Generator.GetSeason(month));
    }

    #endregion

    #region GetSeasonYear

    [TestMethod]
    public void Test_GetSeasonYear_JanuaryReturnsPreviousYear()
    {
        Assert.AreEqual(2025, Generator.GetSeasonYear(new DateTime(2026, 1, 15)));
    }

    [TestMethod]
    public void Test_GetSeasonYear_FebruaryReturnsPreviousYear()
    {
        Assert.AreEqual(2025, Generator.GetSeasonYear(new DateTime(2026, 2, 1)));
    }

    [TestMethod]
    public void Test_GetSeasonYear_MarchReturnsCurrentYear()
    {
        Assert.AreEqual(2026, Generator.GetSeasonYear(new DateTime(2026, 3, 1)));
    }

    [TestMethod]
    public void Test_GetSeasonYear_DecemberReturnsCurrentYear()
    {
        Assert.AreEqual(2026, Generator.GetSeasonYear(new DateTime(2026, 12, 31)));
    }

    #endregion
}
