using System.Runtime.CompilerServices;
using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundownTests.DataBlocks;

[TestClass]
public class Level_CanConsumeOpenSlots_Tests
{
    private static Level BuildLevel(Bulkhead bulkheads, BukheadStrategy strategy)
    {
        // Level's constructor touches game interop assemblies that don't exist under
        // `dotnet test`; CanConsumeOpenSlots only reads Settings and Planner.
        var level = (Level)RuntimeHelpers.GetUninitializedObject(typeof(Level));

        level.Settings = new LevelSettings
        {
            Bulkheads = bulkheads,
            BulkheadStrategy = strategy,
        };
        level.Planner = new LayoutPlanner();

        return level;
    }

    #region SingleChain
    [TestMethod]
    public void Test_SingleChain_MainLayer_LastSlotIsReservedForExtreme()
    {
        var level = BuildLevel(
            Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload,
            BukheadStrategy.SingleChain);

        // Elevator: 1 child of max 2 -> 1 open slot. Zone 1 is a closed dead end.
        var elevator = new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0);
        var zone1 = new ZoneNode(Bulkhead.Main, 1, "primary", 0);
        level.Planner.Connect(elevator, zone1);

        Assert.IsFalse(level.CanConsumeOpenSlots(Bulkhead.Main, DimensionIndex.Reality));
    }

    [TestMethod]
    public void Test_SingleChain_MainLayer_SpareSlotCanBeConsumed()
    {
        var level = BuildLevel(
            Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload,
            BukheadStrategy.SingleChain);

        // Elevator: 1 open slot. Zone 1: 1 open slot. Total = 2.
        var elevator = new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0);
        var zone1 = new ZoneNode(Bulkhead.Main, 1, "primary", 1);
        level.Planner.Connect(elevator, zone1);

        Assert.IsTrue(level.CanConsumeOpenSlots(Bulkhead.Main, DimensionIndex.Reality));
    }

    [TestMethod]
    public void Test_SingleChain_MainLayer_NoReservationOnceExtremeIsPlaced()
    {
        var level = BuildLevel(
            Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload,
            BukheadStrategy.SingleChain);

        // Only 1 open Main slot remains, but Extreme's entrance already exists.
        var elevator = new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0);
        var zone1 = new ZoneNode(Bulkhead.Main, 1, "primary", 0);
        level.Planner.Connect(elevator, zone1);
        level.Planner.Connect(elevator, new ZoneNode(Bulkhead.Extreme, 0, "primary", 0));

        Assert.IsTrue(level.CanConsumeOpenSlots(Bulkhead.Main, DimensionIndex.Reality));
    }

    [TestMethod]
    public void Test_SingleChain_ExtremeLayer_LastSlotIsReservedWhenMainIsFull()
    {
        var level = BuildLevel(
            Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload,
            BukheadStrategy.SingleChain);

        // Main fully closed; Extreme has exactly 1 open slot and Overload is unplaced.
        var elevator = new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0);
        var zone1 = new ZoneNode(Bulkhead.Main, 1, "primary", 0);
        var extreme = new ZoneNode(Bulkhead.Extreme, 0, "primary", 1);
        level.Planner.Connect(elevator, zone1);
        level.Planner.Connect(elevator, extreme);

        Assert.IsFalse(level.CanConsumeOpenSlots(Bulkhead.Extreme, DimensionIndex.Reality));
    }

    [TestMethod]
    public void Test_SingleChain_ExtremeLayer_MainFallbackAllowsConsumingLastSlot()
    {
        var level = BuildLevel(
            Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload,
            BukheadStrategy.SingleChain);

        // Extreme has 1 open slot, but Main still has one too: Overload can fall back to Main.
        var elevator = new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0);
        var extreme = new ZoneNode(Bulkhead.Extreme, 0, "primary", 1);
        level.Planner.Connect(elevator, extreme);

        Assert.IsTrue(level.CanConsumeOpenSlots(Bulkhead.Extreme, DimensionIndex.Reality));
    }
    #endregion

    #region Default_NoMainBulkhead
    [TestMethod]
    public void Test_DefaultNoMainBulkhead_ReservesOneSlotPerPendingEntrance()
    {
        var level = BuildLevel(
            Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload,
            BukheadStrategy.Default_NoMainBulkhead);

        // 2 open Main slots, 2 pending entrances -> consuming one would starve.
        level.Planner.Connect(new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0));

        Assert.IsFalse(level.CanConsumeOpenSlots(Bulkhead.Main, DimensionIndex.Reality));

        // A third open slot makes the consumption safe.
        level.Planner.Connect(
            new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0),
            new ZoneNode(Bulkhead.Main, 1, "primary", 2));

        Assert.IsTrue(level.CanConsumeOpenSlots(Bulkhead.Main, DimensionIndex.Reality));
    }
    #endregion

    #region Eager strategies never reserve
    [TestMethod]
    public void Test_DefaultStrategy_NeverReserves()
    {
        var level = BuildLevel(
            Bulkhead.Main | Bulkhead.Extreme | Bulkhead.Overload,
            BukheadStrategy.Default);

        // Even with a single open slot and nothing placed, Default pre-places entrances
        // in the starting area, so nothing needs reserving here.
        var elevator = new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0);
        var zone1 = new ZoneNode(Bulkhead.Main, 1, "primary", 0);
        level.Planner.Connect(elevator, zone1);

        Assert.IsTrue(level.CanConsumeOpenSlots(Bulkhead.Main, DimensionIndex.Reality));
    }
    #endregion
}
