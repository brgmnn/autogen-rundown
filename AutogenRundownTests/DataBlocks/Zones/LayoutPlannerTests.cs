using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundownTests.DataBlocks.Zones
{
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
        #endregion

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
    }
}
