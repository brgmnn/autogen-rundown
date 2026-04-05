using AutogenRundown;

namespace AutogenRundownTests;

public partial class Generator_Tests
{
    private static void SeedForConstrained(string variant = "constrained")
    {
        Generator.Seed = variant;
        Generator.Reload();
    }

    private static bool IsInBlockedRange(int value, IEnumerable<(int start, int end)> ranges)
    {
        foreach (var (start, end) in ranges)
        {
            var lo = Math.Min(start, end);
            var hi = Math.Max(start, end);
            if (value >= lo && value <= hi)
                return true;
        }
        return false;
    }

    #region Basic behavior

    [TestMethod]
    public void Test_BetweenConstrained_NoBlockedRanges_ReturnsWithinRange()
    {
        SeedForConstrained("no_blocked");
        var blocked = Array.Empty<(int, int)>();

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"no_blocked_{i}");
            var result = Generator.BetweenConstrained(0, 100, blocked, freeAfter: 1);
            Assert.IsTrue(result >= 0 && result <= 100, $"Result {result} out of range");
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_ResultNeverInBlockedRange()
    {
        var blocked = new[] { (20, 30), (50, 60) };

        for (int i = 0; i < 100; i++)
        {
            SeedForConstrained($"not_blocked_{i}");
            var result = Generator.BetweenConstrained(0, 100, blocked, freeAfter: 1);
            Assert.IsFalse(IsInBlockedRange(result, blocked),
                $"Result {result} is in a blocked range");
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_FreeAfterRespected()
    {
        var blocked = new[] { (20, 30), (60, 70) };
        int freeAfter = 5;

        for (int i = 0; i < 100; i++)
        {
            SeedForConstrained($"freeafter_{i}");
            var result = Generator.BetweenConstrained(0, 100, blocked, freeAfter);

            // All numbers from result to result+freeAfter must be unblocked
            for (int offset = 0; offset <= freeAfter; offset++)
            {
                Assert.IsFalse(IsInBlockedRange(result + offset, blocked),
                    $"Result {result} + offset {offset} = {result + offset} is in a blocked range");
            }

            Assert.IsTrue(result + freeAfter <= 100,
                $"Result {result} + freeAfter {freeAfter} exceeds max 100");
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_IsDeterministic()
    {
        var blocked = new[] { (10, 20), (40, 50) };

        SeedForConstrained("det");
        var result1 = Generator.BetweenConstrained(0, 100, blocked, freeAfter: 3);

        SeedForConstrained("det");
        var result2 = Generator.BetweenConstrained(0, 100, blocked, freeAfter: 3);

        Assert.AreEqual(result1, result2);
    }

    #endregion

    #region Blocked range positions

    [TestMethod]
    public void Test_BetweenConstrained_BlockedAtStart()
    {
        var blocked = new[] { (0, 10) };

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"blocked_start_{i}");
            var result = Generator.BetweenConstrained(0, 50, blocked, freeAfter: 1);
            Assert.IsTrue(result > 10, $"Result {result} should be after blocked range");
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_BlockedAtEnd()
    {
        var blocked = new[] { (40, 50) };

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"blocked_end_{i}");
            var result = Generator.BetweenConstrained(0, 50, blocked, freeAfter: 1);
            Assert.IsTrue(result < 40, $"Result {result} should be before blocked range");
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_BlockedInMiddle()
    {
        var blocked = new[] { (20, 30) };

        for (int i = 0; i < 100; i++)
        {
            SeedForConstrained($"blocked_mid_{i}");
            var result = Generator.BetweenConstrained(0, 50, blocked, freeAfter: 1);
            Assert.IsFalse(result >= 20 && result <= 30,
                $"Result {result} should not be in blocked range [20, 30]");
        }
    }

    #endregion

    #region Multiple and merged ranges

    [TestMethod]
    public void Test_BetweenConstrained_OverlappingBlockedRanges()
    {
        // Overlapping ranges should be merged correctly
        var blocked = new[] { (10, 25), (20, 35) };

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"overlap_{i}");
            var result = Generator.BetweenConstrained(0, 50, blocked, freeAfter: 1);
            Assert.IsFalse(result >= 10 && result <= 35,
                $"Result {result} is in merged blocked range [10, 35]");
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_AdjacentBlockedRanges()
    {
        // Adjacent ranges (10-20) and (21-30) should merge into (10-30)
        var blocked = new[] { (10, 20), (21, 30) };

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"adjacent_{i}");
            var result = Generator.BetweenConstrained(0, 50, blocked, freeAfter: 1);
            Assert.IsFalse(result >= 10 && result <= 30,
                $"Result {result} is in merged adjacent range [10, 30]");
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_MultipleDisjointBlockedRanges()
    {
        var blocked = new[] { (5, 10), (20, 25), (35, 40) };

        for (int i = 0; i < 100; i++)
        {
            SeedForConstrained($"disjoint_{i}");
            var result = Generator.BetweenConstrained(0, 50, blocked, freeAfter: 1);
            Assert.IsFalse(IsInBlockedRange(result, blocked),
                $"Result {result} is in a blocked range");
        }
    }

    #endregion

    #region Edge cases

    [TestMethod]
    public void Test_BetweenConstrained_EntireRangeBlocked_Throws()
    {
        var blocked = new[] { (0, 100) };

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            SeedForConstrained("all_blocked");
            Generator.BetweenConstrained(0, 100, blocked, freeAfter: 1);
        });
    }

    [TestMethod]
    public void Test_BetweenConstrained_FreeAfterTooLarge_Throws()
    {
        // Range is 0-10, freeAfter=20 means we need 20 free numbers which is impossible
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            SeedForConstrained("freeafter_large");
            Generator.BetweenConstrained(0, 10, Array.Empty<(int, int)>(), freeAfter: 20);
        });
    }

    [TestMethod]
    public void Test_BetweenConstrained_FreeAfterExactlyFits()
    {
        // Range [0, 5], freeAfter=5, no blocked ranges → only valid result is 0
        SeedForConstrained("exact_fit");
        var result = Generator.BetweenConstrained(0, 5, Array.Empty<(int, int)>(), freeAfter: 5);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void Test_BetweenConstrained_FreeAfterZero()
    {
        var blocked = new[] { (5, 5) };

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"freeafter_zero_{i}");
            // freeAfter=0 means no tail requirement, but looking at the code:
            // lastStart = e - freeAfter = e - 0 = e, so the full allowed segment is valid
            var result = Generator.BetweenConstrained(0, 10, blocked, freeAfter: 0);
            Assert.AreNotEqual(5, result);
            Assert.IsTrue(result >= 0 && result <= 10);
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_MinEqualsMax_NoBlocked()
    {
        SeedForConstrained("min_eq_max");
        var result = Generator.BetweenConstrained(5, 5, Array.Empty<(int, int)>(), freeAfter: 0);
        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public void Test_BetweenConstrained_MinEqualsMax_Blocked_Throws()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            SeedForConstrained("min_eq_max_blocked");
            Generator.BetweenConstrained(5, 5, new[] { (5, 5) }, freeAfter: 0);
        });
    }

    [TestMethod]
    public void Test_BetweenConstrained_BlockedRangesOutsideBoundsIgnored()
    {
        // Blocked ranges entirely outside [min, max] should have no effect
        var blocked = new[] { (-10, -1), (101, 200) };

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"outside_{i}");
            var result = Generator.BetweenConstrained(0, 100, blocked, freeAfter: 1);
            Assert.IsTrue(result >= 0 && result <= 100);
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_ReversedBlockedRange()
    {
        // Blocked range with start > end should still be handled (code normalizes)
        var blocked = new[] { (30, 20) }; // reversed

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"reversed_{i}");
            var result = Generator.BetweenConstrained(0, 50, blocked, freeAfter: 1);
            Assert.IsFalse(result >= 20 && result <= 30,
                $"Result {result} should not be in reversed blocked range");
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_OnlyOneNumberValid()
    {
        // Range [0, 10], blocked [1, 10], freeAfter=0 → only valid result is 0
        SeedForConstrained("one_valid");
        var result = Generator.BetweenConstrained(0, 10, new[] { (1, 10) }, freeAfter: 0);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void Test_BetweenConstrained_NarrowGapBetweenBlocked()
    {
        // Range [0, 100], blocked [0, 40] and [45, 100], freeAfter=1
        // Only valid: 41, 42, 43 (44 needs freeAfter=1 so 44+1=45 is blocked)
        // Actually: allowed segment is [41, 44], freeAfter=1, so valid starts are 41, 42, 43
        var blocked = new[] { (0, 40), (45, 100) };

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"narrow_gap_{i}");
            var result = Generator.BetweenConstrained(0, 100, blocked, freeAfter: 1);
            Assert.IsTrue(result >= 41 && result <= 43,
                $"Result {result} should be in [41, 43]");
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_LargeFreeAfterConstrainsResult()
    {
        // Range [0, 100], no blocked, freeAfter=90 → valid results are [0, 10]
        var blocked = Array.Empty<(int, int)>();

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"large_freeafter_{i}");
            var result = Generator.BetweenConstrained(0, 100, blocked, freeAfter: 90);
            Assert.IsTrue(result >= 0 && result <= 10,
                $"Result {result} should be in [0, 10] with freeAfter=90");
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_BlockedPartiallyOverlapsBounds()
    {
        // Blocked range extends beyond max — should be clipped
        var blocked = new[] { (80, 200) };

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"partial_overlap_{i}");
            var result = Generator.BetweenConstrained(0, 100, blocked, freeAfter: 1);
            Assert.IsTrue(result < 80, $"Result {result} should be < 80");
        }
    }

    [TestMethod]
    public void Test_BetweenConstrained_EmptyBlockedEnumerable()
    {
        SeedForConstrained("empty_enum");
        var result = Generator.BetweenConstrained(0, 50, Enumerable.Empty<(int, int)>(), freeAfter: 1);
        Assert.IsTrue(result >= 0 && result <= 50);
    }

    #endregion

    #region Zone alias start scenario (real-world usage)

    [TestMethod]
    public void Test_BetweenConstrained_ZoneAliasScenario()
    {
        // Simulate real usage: selecting zone alias starts (typically 100-900 range)
        // with existing zones blocking certain ranges, needing room for N zones after
        var existingZones = new[] { (100, 115), (200, 210), (400, 420) };
        int freeAfter = 10; // need room for 10 zones

        for (int i = 0; i < 50; i++)
        {
            SeedForConstrained($"zone_alias_{i}");
            var result = Generator.BetweenConstrained(100, 900, existingZones, freeAfter);

            Assert.IsTrue(result >= 100 && result <= 900);
            Assert.IsFalse(IsInBlockedRange(result, existingZones));

            // Verify freeAfter zone slots are available
            for (int z = 0; z <= freeAfter; z++)
            {
                Assert.IsFalse(IsInBlockedRange(result + z, existingZones),
                    $"Zone alias {result} + {z} conflicts with existing zones");
            }
        }
    }

    #endregion
}
