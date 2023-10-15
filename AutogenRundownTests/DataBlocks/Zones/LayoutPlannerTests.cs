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
        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            var planner = new LayoutPlanner();

            // Act
            planner.Connect(
                new ZoneNode(Bulkhead.Main, 0),
                new ZoneNode(Bulkhead.Main, 1));

            // Assert
            var leaves = planner.GetLeafZones();
        }

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
    }
}
