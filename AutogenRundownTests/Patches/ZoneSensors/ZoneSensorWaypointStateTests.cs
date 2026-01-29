using AutogenRundown.Patches.ZoneSensors;

namespace AutogenRundownTests.Patches.ZoneSensors;

[TestClass]
public class ZoneSensorWaypointState_Tests
{
    [TestMethod]
    public void Test_CalculateBatchCount_ZeroWaypoints()
    {
        Assert.AreEqual(0, ZoneSensorWaypointState.CalculateBatchCount(0));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_SingleBatch()
    {
        // 1-20 waypoints = 1 batch
        Assert.AreEqual(1, ZoneSensorWaypointState.CalculateBatchCount(1));
        Assert.AreEqual(1, ZoneSensorWaypointState.CalculateBatchCount(10));
        Assert.AreEqual(1, ZoneSensorWaypointState.CalculateBatchCount(20));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_TwoBatches()
    {
        // 21-40 waypoints = 2 batches
        Assert.AreEqual(2, ZoneSensorWaypointState.CalculateBatchCount(21));
        Assert.AreEqual(2, ZoneSensorWaypointState.CalculateBatchCount(30));
        Assert.AreEqual(2, ZoneSensorWaypointState.CalculateBatchCount(40));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_ThreeBatches()
    {
        // 41-60 waypoints = 3 batches (typical Moving=3 paths: 27-53 waypoints)
        Assert.AreEqual(3, ZoneSensorWaypointState.CalculateBatchCount(41));
        Assert.AreEqual(3, ZoneSensorWaypointState.CalculateBatchCount(53));  // Common case
        Assert.AreEqual(3, ZoneSensorWaypointState.CalculateBatchCount(60));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_MaxBatches()
    {
        // 141-160 waypoints = 8 batches (max supported)
        Assert.AreEqual(8, ZoneSensorWaypointState.CalculateBatchCount(141));
        Assert.AreEqual(8, ZoneSensorWaypointState.CalculateBatchCount(160));
    }

    [TestMethod]
    public void Test_CalculateBatchCount_ExceedsMax()
    {
        // 161+ waypoints would need 9+ batches (will be clamped by FromArrayBatched)
        Assert.AreEqual(9, ZoneSensorWaypointState.CalculateBatchCount(161));
        Assert.AreEqual(10, ZoneSensorWaypointState.CalculateBatchCount(200));
    }

    [TestMethod]
    public void Test_Constants_AreCorrect()
    {
        // Verify constants match expected values for payload size calculations
        Assert.AreEqual(20, ZoneSensorWaypointState.MaxWaypointsPerBatch);
        Assert.AreEqual(8, ZoneSensorWaypointState.MaxBatchesPerSensor);
    }

    [TestMethod]
    public void Test_CalculateBatchCount_BoundaryConditions()
    {
        // Test exact boundaries between batch counts
        for (int batches = 1; batches <= 8; batches++)
        {
            int maxForBatch = batches * 20;
            int minForNextBatch = maxForBatch + 1;

            Assert.AreEqual(batches, ZoneSensorWaypointState.CalculateBatchCount(maxForBatch),
                $"Expected {batches} batches for {maxForBatch} waypoints");

            if (batches < 8)
            {
                Assert.AreEqual(batches + 1, ZoneSensorWaypointState.CalculateBatchCount(minForNextBatch),
                    $"Expected {batches + 1} batches for {minForNextBatch} waypoints");
            }
        }
    }
}
