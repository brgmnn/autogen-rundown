using AutogenRundown;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.Extensions;
using Newtonsoft.Json;

namespace AutogenRundownTests.Extensions;

[TestClass]
public class WardenObjectiveEventCollections_Tests
{
    [TestInitialize]
    public void Setup()
    {
        // Mock
        Generator.pid = 100u;
    }

    // Note: tests pass message: "" throughout. A non-empty message constructs a Text data
    // block, and the Bins static initializer requires the game's interop assemblies -- that
    // path only runs in-game.

    #region AddScriptedErrorAlarm

    [TestMethod]
    public void Test_AddScriptedErrorAlarm_EmitsSingleFiniteEventLoop()
    {
        var events = new List<WardenObjectiveEvent>();

        events.AddScriptedErrorAlarm(
            GenericWave.SinglePouncerShadow,
            waveCount: 12,
            interval: 300.0,
            delay: 180.0,
            message: "");

        Assert.AreEqual(1, events.Count);

        var start = events[0];
        Assert.AreEqual(WardenObjectiveEventType.StartEventLoop, start.Type);
        Assert.AreEqual(180.0, start.Delay);
        Assert.IsNotNull(start.EventLoop);
        Assert.AreEqual(12, start.EventLoop!.LoopCount);
        Assert.AreEqual(300.0, start.EventLoop.LoopDelay);
    }

    [TestMethod]
    public void Test_AddScriptedErrorAlarm_InnerEventsUseTriggerNone()
    {
        var events = new List<WardenObjectiveEvent>();

        events.AddScriptedErrorAlarm(GenericWave.SinglePouncerShadow, message: "");

        var inner = events[0].EventLoop!.EventsToActivate;

        // Sound + wave; the intel event is skipped for empty messages
        Assert.AreEqual(2, inner.Count);

        foreach (var e in inner)
            Assert.AreEqual(WardenObjectiveEventTrigger.None, e.Trigger);
    }

    [TestMethod]
    public void Test_AddScriptedErrorAlarm_ForcesTriggerAlarmOffWithoutMutatingPreset()
    {
        var events = new List<WardenObjectiveEvent>();

        // ErrorAlarm_Easy is a real error alarm preset with TriggerAlarm = true
        events.AddScriptedErrorAlarm(GenericWave.ErrorAlarm_Easy, message: "");

        var spawn = events[0].EventLoop!.EventsToActivate
            .Single(e => e.Type == WardenObjectiveEventType.SpawnEnemyWave);

        Assert.IsFalse(spawn.EnemyWaveData.TriggerAlarm);
        Assert.IsTrue(GenericWave.ErrorAlarm_Easy.TriggerAlarm);
    }

    [TestMethod]
    public void Test_AddScriptedErrorAlarm_NoneWaveIsSkipped()
    {
        var events = new List<WardenObjectiveEvent>();

        events.AddScriptedErrorAlarm(GenericWave.None);

        Assert.AreEqual(0, events.Count);
    }

    [TestMethod]
    public void Test_AddScriptedErrorAlarm_EventLoopSerializes()
    {
        var events = new List<WardenObjectiveEvent>();

        events.AddScriptedErrorAlarm(GenericWave.SinglePouncerShadow, message: "");

        var json = JsonConvert.SerializeObject(events[0]);
        StringAssert.Contains(json, "\"EventLoop\"");
        StringAssert.Contains(json, "\"EventsToActivate\"");
    }

    [TestMethod]
    public void Test_AddSpawnWave_DoesNotSerializeEventLoop()
    {
        var events = new List<WardenObjectiveEvent>();

        events.AddSpawnWave(GenericWave.SinglePouncerShadow, 0.0, null);

        var json = JsonConvert.SerializeObject(events[0]);
        Assert.IsFalse(json.Contains("\"EventLoop\""));
    }

    #endregion
}
