﻿using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundownTests.DataBlocks.Zones;

[TestClass]
public class ZoneNode_Tests
{
    [TestMethod]
    public void Test_TwoZoneNodesAreEqual()
    {
        Assert.AreEqual(
            new ZoneNode(Bulkhead.Main, 0),
            new ZoneNode(Bulkhead.Main, 0));
    }

    [TestMethod]
    public void Test_TwoZoneNodesAreEqual_WithUnionBulkhead()
    {
        Assert.AreEqual(
            new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0),
            new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0));
    }

    [TestMethod]
    public void Test_TwoZoneNodesAreNotEqual()
    {
        Assert.AreNotEqual(
            new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0),
            new ZoneNode(Bulkhead.Main, 0));
    }

    [TestMethod]
    public void Test_Equals_TwoZonesAreEqualWithMax()
    {
        Assert.AreEqual(
            new ZoneNode(Bulkhead.Main, 1, "primary", 2),
            new ZoneNode(Bulkhead.Main, 1, "primary", 3));
    }
}

[TestClass]
public class LayoutPlanner_Tests
{
    #region Connect()
    [TestMethod]
    public void Test_Connect_AddsANewZoneNode()
    {
        var planner = new LayoutPlanner();

        planner.Connect(new ZoneNode(Bulkhead.Main, 0));

        var zones = planner.GetZones();

        Assert.AreEqual(1, zones.Count);
        Assert.AreEqual(new ZoneNode(Bulkhead.Main, 0), zones[0]);
    }

    //[TestMethod]
    public void Test_Connect_DoesNotAddAZoneToItself()
    {
        var planner = new LayoutPlanner();

        planner.Connect(new ZoneNode(Bulkhead.Main, 0), new ZoneNode(Bulkhead.Main, 0));

        var zones = planner.GetZones();
        var connections = planner.GetConnections(new ZoneNode(Bulkhead.Main, 0));

        Assert.AreEqual(1, zones.Count);
        Assert.AreEqual(new ZoneNode(Bulkhead.Main, 0), zones[0]);

        Assert.AreEqual(0, connections.Count);
    }
    #endregion

    #region GetBulkheadEntranceZones()
    [TestMethod]
    public void Test_GetBulkheadEntranceZones()
    {
        var planner = new LayoutPlanner();

        // Add main zones
        planner.Connect(
            new ZoneNode(Bulkhead.Main, 0),
            new ZoneNode(Bulkhead.Main, 1));
        planner.Connect(
            new ZoneNode(Bulkhead.Main, 1),
            new ZoneNode(Bulkhead.Main, 2));
        planner.Connect(
            new ZoneNode(Bulkhead.Main, 2),
            new ZoneNode(Bulkhead.Main, 3));

        // Add extreme zones
        planner.Connect(
            new ZoneNode(Bulkhead.Main, 2),
            new ZoneNode(Bulkhead.Extreme, 0));
        planner.Connect(
            new ZoneNode(Bulkhead.Extreme, 0),
            new ZoneNode(Bulkhead.Extreme, 1));

        var entrances = planner.GetBulkheadEntranceZones();

        // Assert
        Assert.AreEqual(1, entrances.Count);
        Assert.AreEqual(
            new ZoneNode(Bulkhead.Main, 2),
            entrances[0]);
    }
    #endregion

    #region GetZones()
    [TestMethod]
    public void Test_GetZones_OnlyReturnsZonesForABulkhead()
    {
        var planner = new LayoutPlanner();

        // Add main zones
        planner.Connect(
            new ZoneNode(Bulkhead.Main, 0),
            new ZoneNode(Bulkhead.Main, 1));
        planner.Connect(
            new ZoneNode(Bulkhead.Main, 1),
            new ZoneNode(Bulkhead.Main, 2));

        // Add extreme zones
        planner.Connect(
            new ZoneNode(Bulkhead.Main, 2),
            new ZoneNode(Bulkhead.Extreme, 0));
        planner.Connect(
            new ZoneNode(Bulkhead.Extreme, 0),
            new ZoneNode(Bulkhead.Extreme, 1));

        // Add some more main zones
        planner.Connect(
            new ZoneNode(Bulkhead.Main, 2),
            new ZoneNode(Bulkhead.Main, 3));

        var entrances = planner.GetZones(Bulkhead.Extreme);

        // Assert
        Assert.AreEqual(2, entrances.Count);
        Assert.AreEqual(new ZoneNode(Bulkhead.Extreme, 0), entrances[0]);
        Assert.AreEqual(new ZoneNode(Bulkhead.Extreme, 1), entrances[1]);
    }

    public void Test_GetZones_ReturnsAllZonesByDefault()
    {
        // Setup
        var planner = new LayoutPlanner();

        var startingArea = new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0);
        var main = new ZoneNode(Bulkhead.Main, 1);
        var extreme = new ZoneNode(Bulkhead.Extreme, 0);
        var overload = new ZoneNode(Bulkhead.Overload, 0);

        planner.Connect(startingArea, main);
        planner.Connect(startingArea, extreme);
        planner.Connect(extreme, overload);

        var nodes = planner.GetZones();

        Assert.AreEqual(4, nodes.Count);
    }
    #endregion

    #region GetOpenZones()
    [TestMethod]
    public void Test_GetOpenZones_OnlyReturnsZonesThatCanAttachMoreZones()
    {
        var planner = new LayoutPlanner();

        // Add main zones
        planner.Connect(new ZoneNode(Bulkhead.Main, 0), new ZoneNode(Bulkhead.Main, 1));
        planner.Connect(new ZoneNode(Bulkhead.Main, 1), new ZoneNode(Bulkhead.Main, 2));

        // Add extreme zones
        planner.Connect(new ZoneNode(Bulkhead.Main, 2), new ZoneNode(Bulkhead.Extreme, 0));
        planner.Connect(new ZoneNode(Bulkhead.Extreme, 0), new ZoneNode(Bulkhead.Extreme, 1));

        // Add some more main zones
        planner.Connect(new ZoneNode(Bulkhead.Main, 2), new ZoneNode(Bulkhead.Main, 3));

        var open = planner.GetOpenZones(Bulkhead.Main);

        // Assert
        Assert.AreEqual(3, open.Count);
        Assert.AreEqual(new ZoneNode(Bulkhead.Main, 0), open[0]);
        Assert.AreEqual(new ZoneNode(Bulkhead.Main, 1), open[1]);
        Assert.AreEqual(new ZoneNode(Bulkhead.Main, 3), open[2]);
    }
    #endregion

    #region GetBulkheadEntranceZones()
    [TestMethod]
    public void Test_GetBulkheadEntranceZones_OnlyReturnsConnectingBulkheadZones()
    {
        var planner = new LayoutPlanner();

        var startingZone = new ZoneNode(Bulkhead.Main | Bulkhead.StartingArea, 0);

        var mainZone1 = new ZoneNode(Bulkhead.Main, 1);
        var mainZone2 = new ZoneNode(Bulkhead.Main, 2);

        var extremeZone0 = new ZoneNode(Bulkhead.Extreme, 0);
        var extremeZone1 = new ZoneNode(Bulkhead.Extreme, 1);

        var overloadZone0 = new ZoneNode(Bulkhead.Overload, 0);
        var overloadZone1 = new ZoneNode(Bulkhead.Overload, 1);

        // Connect
        planner.Connect(startingZone, mainZone1);
        planner.Connect(startingZone, extremeZone0);

        planner.Connect(mainZone1, mainZone2);
        planner.Connect(extremeZone0, extremeZone1);

        planner.Connect(extremeZone1, overloadZone0);
        planner.Connect(overloadZone0, overloadZone1);

        var bulkheadZones = planner.GetBulkheadEntranceZones();

        // Assert
        Assert.AreEqual(2, bulkheadZones.Count);
        Assert.AreEqual(startingZone, bulkheadZones[0]);
        Assert.AreEqual(extremeZone1, bulkheadZones[1]);
    }
    #endregion

    #region CountOpenSlots()
    [TestMethod]
    public void Test_CountOpenSlots_CountsCorrectlyForDenseZones()
    {
        var planner = new LayoutPlanner();

        var zone0 = new ZoneNode(Bulkhead.Main, 0);
        var zone1 = new ZoneNode(Bulkhead.Main, 1);
        var zone2 = new ZoneNode(Bulkhead.Main, 2);
        var zone3 = new ZoneNode(Bulkhead.Main, 3);
        var zone4 = new ZoneNode(Bulkhead.Main, 4);

        // Add main zones
        planner.Connect(zone0, zone1);
        planner.Connect(zone1, zone2);
        planner.Connect(zone1, zone3);
        planner.Connect(zone2, zone4);
        planner.Connect(new ZoneNode(Bulkhead.Main, 1), new ZoneNode(Bulkhead.Main, 2));
        planner.Connect(new ZoneNode(Bulkhead.Main, 1), new ZoneNode(Bulkhead.Main, 3));
        planner.Connect(new ZoneNode(Bulkhead.Main, 2), new ZoneNode(Bulkhead.Main, 4));

        var openSlots = planner.CountOpenSlots(new List<ZoneNode> { zone0, zone1 });

        Assert.AreEqual(1, openSlots);
    }

    [TestMethod]
    public void Test_CountOpenSlots_CountsCorrectlyForManyZones()
    {
        var planner = new LayoutPlanner();

        var zone0 = new ZoneNode(Bulkhead.Main, 0);
        var zone1 = new ZoneNode(Bulkhead.Main, 1);
        var zone2 = new ZoneNode(Bulkhead.Main, 2);
        var zone3 = new ZoneNode(Bulkhead.Main, 3);
        var zone4 = new ZoneNode(Bulkhead.Main, 4);

        // Add main zones
        planner.Connect(zone0, zone1);
        planner.Connect(zone1, zone2);
        planner.Connect(zone1, zone3);
        planner.Connect(zone2, zone4);
        planner.Connect(new ZoneNode(Bulkhead.Main, 1), new ZoneNode(Bulkhead.Main, 2));
        planner.Connect(new ZoneNode(Bulkhead.Main, 1), new ZoneNode(Bulkhead.Main, 3));
        planner.Connect(new ZoneNode(Bulkhead.Main, 2), new ZoneNode(Bulkhead.Main, 4));

        var openSlots = planner.CountOpenSlots(new List<ZoneNode> { zone0, zone1, zone3, zone4 });

        Assert.AreEqual(5, openSlots);
    }

    [TestMethod]
    public void Test_CountOpenSlots_CountsCorrectlyForVariableMaxConnections()
    {
        var planner = new LayoutPlanner();

        var zone0 = new ZoneNode(Bulkhead.Main, 0);
        var zone1 = new ZoneNode(Bulkhead.Main, 1, "primary", 3);
        var zone2 = new ZoneNode(Bulkhead.Main, 2);
        var zone3 = new ZoneNode(Bulkhead.Main, 3);
        var zone4 = new ZoneNode(Bulkhead.Main, 4);

        // Add main zones
        planner.Connect(zone0, zone1);
        planner.Connect(zone1, zone2);
        planner.Connect(zone1, zone3);
        planner.Connect(zone2, zone4);

        var openSlots = planner.CountOpenSlots(new List<ZoneNode> { zone0, zone1 });

        Assert.AreEqual(2, openSlots);
    }

    [TestMethod]
    public void Test_CountOpenSlots_CountsCorrectlyForAnEmptyList()
    {
        var planner = new LayoutPlanner();

        var zone0 = new ZoneNode(Bulkhead.Main, 0);
        var zone1 = new ZoneNode(Bulkhead.Main, 1, "primary", 3);
        var zone2 = new ZoneNode(Bulkhead.Main, 2);
        var zone3 = new ZoneNode(Bulkhead.Main, 3);
        var zone4 = new ZoneNode(Bulkhead.Main, 4);

        // Add main zones
        planner.Connect(zone0, zone1);
        planner.Connect(zone1, zone2);
        planner.Connect(zone1, zone3);
        planner.Connect(zone2, zone4);
        planner.Connect(new ZoneNode(Bulkhead.Main, 1), new ZoneNode(Bulkhead.Main, 2));
        planner.Connect(new ZoneNode(Bulkhead.Main, 1), new ZoneNode(Bulkhead.Main, 3));
        planner.Connect(new ZoneNode(Bulkhead.Main, 2), new ZoneNode(Bulkhead.Main, 4));

        var openSlots = planner.CountOpenSlots(new List<ZoneNode>());

        Assert.AreEqual(0, openSlots);
    }
    #endregion
}
