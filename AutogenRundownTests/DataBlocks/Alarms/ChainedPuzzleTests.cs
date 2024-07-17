using AutogenRundown;
using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Alarms;

namespace AutogenRundownTests.DataBlocks.Alarms;

[TestClass]
public class ChainedPuzzle_Tests
{
    [TestMethod]
    public void Test_NewPuzzlesGetNoPersistentId()
    {
        var puzzle = new ChainedPuzzle { PublicAlarmName = "Test Alarm" };

        Assert.AreEqual(puzzle.PersistentId, 0u);
    }

    [TestMethod]
    public void Test_PersistAssignsANewId()
    {
        // Mock
        Generator.pid = 100u;
        var bin = new LazyBlocksBin<ChainedPuzzle>();

        // Setup
        var puzzle = new ChainedPuzzle { PublicAlarmName = "Test Alarm" };

        // Run
        puzzle.Persist(bin);
        bin.Persist();

        // Assert
        Assert.AreEqual(100u, puzzle.PersistentId); // Puzzle gets an Persistent Id
        Assert.AreEqual(puzzle, bin.Find(100u)); // Puzzle is added to the block bin
    }

    [TestMethod]
    public void Test_ExtendedRecordsOnlyAssignIdsOnce()
    {
        // Mock
        Generator.pid = 100u;
        var bin = new LazyBlocksBin<ChainedPuzzle>();

        // Setup
        var puzzle1 = new ChainedPuzzle { PublicAlarmName = "Test Alarm" };
        var puzzle2 = puzzle1 with { WantedDistanceFromStartPos = 3.0 };

        // Run
        puzzle2.Persist(bin);
        bin.Persist();

        // Assert
        Assert.AreEqual(100u, puzzle2.PersistentId); // Puzzle gets an Persistent Id
        Assert.AreEqual(puzzle2, bin.Find(100u)); // Puzzle is added to the block bin
    }

    [TestMethod]
    public void Test_ExtendedRecordsPreserveTheOriginal()
    {
        // Mock
        Generator.pid = 100u;
        var bin = new LazyBlocksBin<ChainedPuzzle>();

        // Setup
        var puzzle1 = new ChainedPuzzle { PublicAlarmName = "Test Alarm" };
        var puzzle2 = puzzle1 with { WantedDistanceFromStartPos = 3.0 };

        // Run
        puzzle1.Persist(bin);
        puzzle2.Persist(bin);
        bin.Persist();

        // Assert
        Assert.AreEqual(100u, puzzle1.PersistentId); // Puzzle gets an Persistent Id
        Assert.AreEqual(puzzle1, bin.Find(100u)); // Puzzle is added to the block bin

        Assert.AreEqual(101u, puzzle2.PersistentId); // Puzzle gets an Persistent Id
        Assert.AreEqual(puzzle2, bin.Find(101u)); // Puzzle is added to the block bin
    }

    [TestMethod]
    public void Test_ExtendedRecordsPreserveAnAlreadyPersistedOriginal()
    {
        // Mock
        Generator.pid = 100u;
        var bin = new LazyBlocksBin<ChainedPuzzle>();

        // Setup and run
        var puzzle1 = new ChainedPuzzle { PublicAlarmName = "Test Alarm" };
        puzzle1.Persist(bin);

        var puzzle2 = puzzle1 with { WantedDistanceFromStartPos = 3.0 };
        puzzle2.Persist(bin);

        bin.Persist();

        // Assert
        Assert.AreEqual(100u, puzzle1.PersistentId); // Puzzle gets an Persistent Id
        Assert.AreEqual(puzzle1, bin.Find(100u)); // Puzzle is added to the block bin

        Assert.AreEqual(101u, puzzle2.PersistentId); // Puzzle gets an Persistent Id
        Assert.AreEqual(puzzle2, bin.Find(101u)); // Puzzle is added to the block bin
    }

    [TestMethod]
    public void Test_PersistIsIdempotent()
    {
        // Mock
        Generator.pid = 100u;
        var bin = new LazyBlocksBin<ChainedPuzzle>();

        // Setup and run
        var puzzle1 = new ChainedPuzzle { PublicAlarmName = "Test Alarm" };
        puzzle1.Persist(bin);
        puzzle1.Persist(bin);
        bin.Persist();

        // Assert
        Assert.AreEqual(100u, puzzle1.PersistentId); // Puzzle gets an Persistent Id
        Assert.AreEqual(puzzle1, bin.Find(100u)); // Puzzle is added to the block bin
        Assert.AreEqual(null, bin.Find(101u)); // Make sure no id=101 was added
        Assert.AreEqual(1, bin.Blocks.Count); // Only 1 element in there
    }
}
