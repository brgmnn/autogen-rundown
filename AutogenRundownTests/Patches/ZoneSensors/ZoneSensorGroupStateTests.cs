using AutogenRundown.Patches.ZoneSensors;

namespace AutogenRundownTests.Patches.ZoneSensors;

[TestClass]
public class ZoneSensorGroupState_Tests
{
    #region Constructor Tests

    [TestMethod]
    public void Test_DefaultConstructor_AllEnabled()
    {
        var state = new ZoneSensorGroupState();

        // All 128 sensors should be enabled by default
        for (int i = 0; i < 128; i++)
        {
            Assert.IsTrue(state.IsSensorEnabled(i), $"Sensor {i} should be enabled");
        }

        // None should be triggered
        for (int i = 0; i < 128; i++)
        {
            Assert.IsFalse(state.IsSensorTriggered(i), $"Sensor {i} should not be triggered");
        }
    }

    [TestMethod]
    public void Test_Constructor_WithEnabled_True()
    {
        var state = new ZoneSensorGroupState(true);

        for (int i = 0; i < 128; i++)
        {
            Assert.IsTrue(state.IsSensorEnabled(i), $"Sensor {i} should be enabled");
        }
    }

    [TestMethod]
    public void Test_Constructor_WithEnabled_False()
    {
        var state = new ZoneSensorGroupState(false);

        for (int i = 0; i < 128; i++)
        {
            Assert.IsFalse(state.IsSensorEnabled(i), $"Sensor {i} should be disabled");
        }
    }

    #endregion

    #region IsSensorEnabled Tests

    [TestMethod]
    public void Test_IsSensorEnabled_AllFields()
    {
        var state = new ZoneSensorGroupState(true);

        // Test boundary indices for each 32-bit field
        Assert.IsTrue(state.IsSensorEnabled(0));    // SensorMask0 start
        Assert.IsTrue(state.IsSensorEnabled(31));   // SensorMask0 end
        Assert.IsTrue(state.IsSensorEnabled(32));   // SensorMask1 start
        Assert.IsTrue(state.IsSensorEnabled(63));   // SensorMask1 end
        Assert.IsTrue(state.IsSensorEnabled(64));   // SensorMask2 start
        Assert.IsTrue(state.IsSensorEnabled(95));   // SensorMask2 end
        Assert.IsTrue(state.IsSensorEnabled(96));   // SensorMask3 start
        Assert.IsTrue(state.IsSensorEnabled(127));  // SensorMask3 end
    }

    [TestMethod]
    public void Test_IsSensorEnabled_OutOfRange()
    {
        var state = new ZoneSensorGroupState(true);

        // Out of range indices should return false
        Assert.IsFalse(state.IsSensorEnabled(-1));
        Assert.IsFalse(state.IsSensorEnabled(128));
        Assert.IsFalse(state.IsSensorEnabled(1000));
    }

    #endregion

    #region SetSensorDisabled Tests

    [TestMethod]
    public void Test_SetSensorDisabled_EachField()
    {
        // Test disabling sensor in each mask field
        int[] testIndices = { 0, 31, 32, 63, 64, 95, 96, 127 };

        foreach (int index in testIndices)
        {
            var state = new ZoneSensorGroupState(true);
            state.SetSensorDisabled(index);

            Assert.IsFalse(state.IsSensorEnabled(index), $"Sensor {index} should be disabled");

            // Verify adjacent sensors are still enabled
            if (index > 0)
                Assert.IsTrue(state.IsSensorEnabled(index - 1), $"Sensor {index - 1} should still be enabled");
            if (index < 127)
                Assert.IsTrue(state.IsSensorEnabled(index + 1), $"Sensor {index + 1} should still be enabled");
        }
    }

    [TestMethod]
    public void Test_SetSensorDisabled_Multiple()
    {
        var state = new ZoneSensorGroupState(true);

        state.SetSensorDisabled(0);
        state.SetSensorDisabled(50);
        state.SetSensorDisabled(100);

        Assert.IsFalse(state.IsSensorEnabled(0));
        Assert.IsFalse(state.IsSensorEnabled(50));
        Assert.IsFalse(state.IsSensorEnabled(100));

        // Others still enabled
        Assert.IsTrue(state.IsSensorEnabled(1));
        Assert.IsTrue(state.IsSensorEnabled(49));
        Assert.IsTrue(state.IsSensorEnabled(51));
    }

    [TestMethod]
    public void Test_SetSensorDisabled_OutOfRange()
    {
        var state = new ZoneSensorGroupState(true);

        // Should not throw and should have no effect
        state.SetSensorDisabled(-1);
        state.SetSensorDisabled(128);

        // All sensors still enabled
        Assert.IsTrue(state.IsSensorEnabled(0));
        Assert.IsTrue(state.IsSensorEnabled(127));
    }

    #endregion

    #region SetSensorTriggered Tests

    [TestMethod]
    public void Test_SetSensorTriggered_EachField()
    {
        int[] testIndices = { 0, 31, 32, 63, 64, 95, 96, 127 };

        foreach (int index in testIndices)
        {
            var state = new ZoneSensorGroupState(true);
            state.SetSensorTriggered(index);

            Assert.IsTrue(state.IsSensorTriggered(index), $"Sensor {index} should be triggered");

            // Verify adjacent sensors not triggered
            if (index > 0)
                Assert.IsFalse(state.IsSensorTriggered(index - 1), $"Sensor {index - 1} should not be triggered");
            if (index < 127)
                Assert.IsFalse(state.IsSensorTriggered(index + 1), $"Sensor {index + 1} should not be triggered");
        }
    }

    [TestMethod]
    public void Test_SetSensorTriggered_Multiple()
    {
        var state = new ZoneSensorGroupState(true);

        state.SetSensorTriggered(10);
        state.SetSensorTriggered(75);

        Assert.IsTrue(state.IsSensorTriggered(10));
        Assert.IsTrue(state.IsSensorTriggered(75));
        Assert.IsFalse(state.IsSensorTriggered(11));
    }

    #endregion

    #region SetAllEnabled/Disabled Tests

    [TestMethod]
    public void Test_SetAllEnabled_ClearsDisabled()
    {
        var state = new ZoneSensorGroupState(true);

        // Disable some sensors
        state.SetSensorDisabled(0);
        state.SetSensorDisabled(64);
        state.SetSensorDisabled(127);

        // Re-enable all
        state.SetAllEnabled();

        // All should be enabled again
        Assert.IsTrue(state.IsSensorEnabled(0));
        Assert.IsTrue(state.IsSensorEnabled(64));
        Assert.IsTrue(state.IsSensorEnabled(127));
    }

    [TestMethod]
    public void Test_SetAllDisabled_DisablesAll()
    {
        var state = new ZoneSensorGroupState(true);

        state.SetAllDisabled();

        // All 128 sensors should be disabled
        for (int i = 0; i < 128; i++)
        {
            Assert.IsFalse(state.IsSensorEnabled(i), $"Sensor {i} should be disabled");
        }
    }

    #endregion

    #region ClearTriggered Tests

    [TestMethod]
    public void Test_ClearTriggered()
    {
        var state = new ZoneSensorGroupState(true);

        // Trigger some sensors in different fields
        state.SetSensorTriggered(5);
        state.SetSensorTriggered(40);
        state.SetSensorTriggered(80);
        state.SetSensorTriggered(120);

        // Clear all triggered
        state.ClearTriggered();

        // None should be triggered
        Assert.IsFalse(state.IsSensorTriggered(5));
        Assert.IsFalse(state.IsSensorTriggered(40));
        Assert.IsFalse(state.IsSensorTriggered(80));
        Assert.IsFalse(state.IsSensorTriggered(120));
    }

    #endregion

    #region ApplyTriggeredMask Tests

    [TestMethod]
    public void Test_ApplyTriggeredMask()
    {
        var state = new ZoneSensorGroupState(true);

        // Trigger sensors
        state.SetSensorTriggered(10);
        state.SetSensorTriggered(50);
        state.SetSensorTriggered(90);

        // Apply - triggered sensors should become disabled
        state.ApplyTriggeredMask();

        Assert.IsFalse(state.IsSensorEnabled(10));
        Assert.IsFalse(state.IsSensorEnabled(50));
        Assert.IsFalse(state.IsSensorEnabled(90));
    }

    [TestMethod]
    public void Test_ApplyTriggeredMask_PreservesOthers()
    {
        var state = new ZoneSensorGroupState(true);

        // Trigger only sensor 50
        state.SetSensorTriggered(50);

        // Apply
        state.ApplyTriggeredMask();

        // Non-triggered sensors should still be enabled
        Assert.IsTrue(state.IsSensorEnabled(0));
        Assert.IsTrue(state.IsSensorEnabled(49));
        Assert.IsTrue(state.IsSensorEnabled(51));
        Assert.IsTrue(state.IsSensorEnabled(127));

        // Only triggered one disabled
        Assert.IsFalse(state.IsSensorEnabled(50));
    }

    [TestMethod]
    public void Test_ApplyTriggeredMask_AllFields()
    {
        var state = new ZoneSensorGroupState(true);

        // Trigger one sensor in each field
        state.SetSensorTriggered(15);   // SensorMask0
        state.SetSensorTriggered(45);   // SensorMask1
        state.SetSensorTriggered(75);   // SensorMask2
        state.SetSensorTriggered(105);  // SensorMask3

        state.ApplyTriggeredMask();

        Assert.IsFalse(state.IsSensorEnabled(15));
        Assert.IsFalse(state.IsSensorEnabled(45));
        Assert.IsFalse(state.IsSensorEnabled(75));
        Assert.IsFalse(state.IsSensorEnabled(105));

        // Some others still enabled
        Assert.IsTrue(state.IsSensorEnabled(14));
        Assert.IsTrue(state.IsSensorEnabled(44));
        Assert.IsTrue(state.IsSensorEnabled(74));
        Assert.IsTrue(state.IsSensorEnabled(104));
    }

    #endregion

    #region CreateWithMasks Tests

    [TestMethod]
    public void Test_CreateWithMasks()
    {
        var state = ZoneSensorGroupState.CreateWithMasks(
            enabled: true,
            sensorMask0: 0xAAAAAAAA, // Alternating bits
            sensorMask1: 0x55555555, // Opposite alternating
            sensorMask2: 0xFFFFFFFF, // All enabled
            sensorMask3: 0x00000000, // All disabled
            triggeredMask0: 0, triggeredMask1: 0, triggeredMask2: 0, triggeredMask3: 0
        );

        // SensorMask0: 0xAAAAAAAA = 10101010... → even bits set
        Assert.IsFalse(state.IsSensorEnabled(0));  // Bit 0 = 0
        Assert.IsTrue(state.IsSensorEnabled(1));   // Bit 1 = 1

        // SensorMask1: 0x55555555 = 01010101... → odd bits set
        Assert.IsTrue(state.IsSensorEnabled(32));  // Bit 0 = 1
        Assert.IsFalse(state.IsSensorEnabled(33)); // Bit 1 = 0

        // SensorMask2: All enabled
        Assert.IsTrue(state.IsSensorEnabled(64));
        Assert.IsTrue(state.IsSensorEnabled(95));

        // SensorMask3: All disabled
        Assert.IsFalse(state.IsSensorEnabled(96));
        Assert.IsFalse(state.IsSensorEnabled(127));
    }

    #endregion
}
