using AutogenRundown;
using AutogenRundown.DataBlocks;
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

    #region AddCyclingFog

    [TestMethod]
    public void Test_AddCyclingFog_EmitsSingleInfiniteLoop()
    {
        var events = new List<WardenObjectiveEvent>();

        events.AddCyclingFog(
            Fog.CyclingFog_Heavy,
            Fog.CyclingFog_Clear,
            loopIndex: 7,
            delay1: 90.0,
            duration1: 35.0,
            delay2: 30.0,
            duration2: 22.0);

        Assert.AreEqual(1, events.Count);

        var start = events[0];
        Assert.AreEqual(WardenObjectiveEventType.StartEventLoop, start.Type);
        Assert.IsNotNull(start.EventLoop);
        Assert.AreEqual(7, start.EventLoop!.LoopIndex);
        Assert.AreEqual(-1, start.EventLoop.LoopCount);
        Assert.AreEqual(35.0 + 90.0 + 22.0 + 30.0, start.EventLoop.LoopDelay);

        var inner = start.EventLoop.EventsToActivate;
        Assert.AreEqual(2, inner.Count);

        foreach (var e in inner)
        {
            Assert.AreEqual(WardenObjectiveEventType.SetFogSettings, e.Type);
            Assert.AreEqual(WardenObjectiveEventTrigger.None, e.Trigger);
        }

        Assert.AreEqual(Fog.CyclingFog_Heavy.PersistentId, inner[0].FogSetting);
        Assert.AreEqual(35.0, inner[0].FogTransitionDuration);
        Assert.AreEqual(0.0, inner[0].Delay);

        Assert.AreEqual(Fog.CyclingFog_Clear.PersistentId, inner[1].FogSetting);
        Assert.AreEqual(22.0, inner[1].FogTransitionDuration);
        Assert.AreEqual(35.0 + 90.0, inner[1].Delay);
    }

    [TestMethod]
    public void Test_AddCyclingFog_StartDelayIsAppliedToLoopStart()
    {
        var events = new List<WardenObjectiveEvent>();

        events.AddCyclingFog(
            Fog.CyclingFog_Heavy,
            Fog.CyclingFog_Clear,
            startDelay: 60.0);

        Assert.AreEqual(60.0, events[0].Delay);
    }

    [TestMethod]
    public void Test_AddCyclingFog_DefaultStartDelayIsZero()
    {
        var events = new List<WardenObjectiveEvent>();

        events.AddCyclingFog(Fog.CyclingFog_Heavy, Fog.CyclingFog_Clear);

        Assert.AreEqual(0.0, events[0].Delay);
    }

    #endregion

    #region AddCountdownWithExpiryChain

    private static List<WardenObjectiveEvent> BuildChain(
        int fallbackCount = 3,
        double fallbackDuration = 240.0,
        string identifier = "test_surge")
    {
        var events = new List<WardenObjectiveEvent>();

        events.AddCountdownWithExpiryChain(
            duration: 300.0,
            wave: GenericWave.ErrorAlarm_Easy,
            identifier: identifier,
            fallbackCount: fallbackCount,
            fallbackDuration: fallbackDuration,
            titleText: "INITIAL",
            fallbackTitleText: "FALLBACK",
            delay: 5.0);

        return events;
    }

    /// <summary>
    /// Walks the expiry chain: element 0 is the initial countdown's EventsOnDone, each
    /// following element is the next fallback countdown's EventsOnDone.
    /// </summary>
    private static List<List<WardenObjectiveEvent>> WalkExpiryLevels(WardenObjectiveEvent outer)
    {
        var levels = new List<List<WardenObjectiveEvent>>();
        var current = outer;

        while (current != null)
        {
            var expiry = current.Countdown!.EventsOnDone;
            levels.Add(expiry);
            current = expiry.FirstOrDefault(e => e.Type == WardenObjectiveEventType.Countdown);
        }

        return levels;
    }

    [TestMethod]
    public void Test_AddCountdownWithExpiryChain_EmitsSingleCountdown()
    {
        var events = BuildChain();

        Assert.AreEqual(1, events.Count);

        var outer = events[0];
        Assert.AreEqual(WardenObjectiveEventType.Countdown, outer.Type);
        Assert.AreEqual(300.0, outer.Duration);
        Assert.AreEqual(5.0, outer.Delay);
        Assert.IsNotNull(outer.Countdown);
        Assert.AreEqual("INITIAL", outer.Countdown!.TitleText);
    }

    [TestMethod]
    public void Test_AddCountdownWithExpiryChain_ChainDepth()
    {
        var events = BuildChain(fallbackCount: 4, fallbackDuration: 180.0);
        var levels = WalkExpiryLevels(events[0]);

        // fallbackCount fallback countdowns -> fallbackCount + 1 expiry levels, the
        // innermost of which has no further countdown.
        Assert.AreEqual(5, levels.Count);

        foreach (var level in levels.Take(4))
        {
            var fallback = level.Single(e => e.Type == WardenObjectiveEventType.Countdown);
            Assert.AreEqual(180.0, fallback.Duration);
            Assert.AreEqual("FALLBACK", fallback.Countdown!.TitleText);
        }

        Assert.IsFalse(levels[4].Any(e => e.Type == WardenObjectiveEventType.Countdown));
    }

    [TestMethod]
    public void Test_AddCountdownWithExpiryChain_StopPrecedesSpawnAtEveryLevel()
    {
        var events = BuildChain(identifier: "surge_id");
        var levels = WalkExpiryLevels(events[0]);

        foreach (var level in levels)
        {
            var stop = level.Single(e => e.Type == WardenObjectiveEventType.StopEnemyWaves);
            var spawn = level.Single(e => e.Type == WardenObjectiveEventType.SpawnEnemyWave);

            Assert.AreEqual("surge_id", stop.Identifier);
            Assert.AreEqual("surge_id", spawn.Identifier);
            Assert.IsTrue(stop.Delay < spawn.Delay);
        }
    }

    [TestMethod]
    public void Test_AddCountdownWithExpiryChain_ExactlyOneSpawnPerExpiryLevel()
    {
        var events = BuildChain(fallbackCount: 6);
        var levels = WalkExpiryLevels(events[0]);

        // Guards the one-live-stream invariant: each expiry stops the previous stream
        // and spawns exactly one new one.
        foreach (var level in levels)
            Assert.AreEqual(1, level.Count(e => e.Type == WardenObjectiveEventType.SpawnEnemyWave));
    }

    [TestMethod]
    public void Test_AddCountdownWithExpiryChain_WarningsOnEveryCountdown()
    {
        var events = BuildChain(fallbackCount: 3);

        var countdowns = new List<WardenObjectiveEvent> { events[0] };
        countdowns.AddRange(WalkExpiryLevels(events[0])
            .SelectMany(level => level.Where(e => e.Type == WardenObjectiveEventType.Countdown)));

        Assert.AreEqual(4, countdowns.Count);

        var progressLists = new HashSet<object>();

        foreach (var countdown in countdowns)
        {
            var progress = countdown.Countdown!.EventsOnProgress;

            Assert.AreEqual(2, progress.Count);
            Assert.AreEqual(0.75, progress[0].Progress);
            Assert.AreEqual(0.90, progress[1].Progress);

            // Fresh list instances per countdown: shared instances would alias when the
            // chain serializes.
            progressLists.Add(progress);
            progressLists.Add(progress[0].Events);
            progressLists.Add(progress[1].Events);
        }

        Assert.AreEqual(countdowns.Count * 3, progressLists.Count);
    }

    [TestMethod]
    public void Test_AddCountdownWithExpiryChain_WavePresetNotMutated()
    {
        var events = BuildChain();
        var spawn = WalkExpiryLevels(events[0])[0]
            .Single(e => e.Type == WardenObjectiveEventType.SpawnEnemyWave);

        // The stream keeps the preset's real alarm state (ambience) — and the preset
        // itself is untouched.
        Assert.IsTrue(spawn.EnemyWaveData.TriggerAlarm);
        Assert.IsTrue(GenericWave.ErrorAlarm_Easy.TriggerAlarm);
    }

    [TestMethod]
    public void Test_AddCountdownWithExpiryChain_Serializes()
    {
        var events = BuildChain();

        var json = JsonConvert.SerializeObject(events[0]);
        StringAssert.Contains(json, "\"Countdown\"");
        StringAssert.Contains(json, "\"EventsOnDone\"");
        StringAssert.Contains(json, "\"EventsOnProgress\"");
    }

    #endregion

    #region CustomHudText delay

    [TestMethod]
    public void Test_AddCustomHudText_AssignsDelay()
    {
        var events = new List<WardenObjectiveEvent>();

        events.AddCustomHudText("banner", 10.0);

        Assert.AreEqual(10.0, events[0].Delay);
    }

    [TestMethod]
    public void Test_RemoveCustomHudText_AssignsDelay()
    {
        var events = new List<WardenObjectiveEvent>();

        events.RemoveCustomHudText(10.0);

        Assert.AreEqual(10.0, events[0].Delay);
    }

    #endregion
}
