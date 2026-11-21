using AutogenRundown;
using AutogenRundown.DataBlocks;
using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.ZoneScan;
using Newtonsoft.Json;

namespace AutogenRundownTests.DataBlocks.Alarms;

[TestClass]
public class ZoneScan_Tests
{
    [TestMethod]
    [DataRow("A")]
    [DataRow("B")]
    [DataRow("C")]
    [DataRow("D")]
    [DataRow("E")]
    public void Test_EveryTierOffersOnlyWellFormedZoneScanAlarms(string tier)
    {
        var pack = ZoneScanAlarms.BuildPack(tier);

        Assert.IsTrue(pack.Count > 0, $"Tier {tier} has no zone scan alarms");

        // ZoneScan applies to every scan of a listed chained puzzle, so a mixed alarm would
        // either turn regular scans into area scans or leave zero radius scans unlisted.
        foreach (var (_, _, puzzle) in pack)
        {
            var types = puzzle.Puzzle.Select(ZoneScans.Find).ToList();

            Assert.IsTrue(types.Count > 0, $"{puzzle.PublicAlarmName} has no scans");
            Assert.IsTrue(types.All(type => type is not null), $"{puzzle.PublicAlarmName} mixes scans");
            Assert.AreEqual(1, types.Select(type => type!.UseZone).Distinct().Count(),
                $"{puzzle.PublicAlarmName} mixes area and zone scans");
        }
    }

    [TestMethod]
    public void Test_SettingsListOnlyPersistedZoneScanAlarms()
    {
        Generator.pid = 500u;
        var bin = new LazyBlocksBin<ChainedPuzzle>();

        var area = ZoneScanAlarms.AlarmClass3_AreaScan with { };
        var zone = ZoneScanAlarms.AlarmClass1_ZoneScan_Sustained with { };
        var regular = new ChainedPuzzle
        {
            PublicAlarmName = "Regular",
            Puzzle = new List<PuzzleComponent> { PuzzleComponent.AllBig, PuzzleComponent.ClusterSmall }
        };

        area.Persist(bin);
        zone.Persist(bin);
        regular.Persist(bin);
        bin.Persist();

        // Parse it the way ZoneScan does: case sensitive System.Text.Json
        var json = JsonConvert.SerializeObject(ZoneScans.BuildSettings(bin.Blocks));
        var settings = System.Text.Json.JsonSerializer.Deserialize<ZoneScanSettingsMirror>(json)!;

        CollectionAssert.AreEquivalent(
            new[] { area.PersistentId, zone.PersistentId },
            settings.AllowedPersistentIDs.Select(entry => entry.ID).ToList());

        Assert.IsFalse(settings.AllowedPersistentIDs.Single(e => e.ID == area.PersistentId).UseZoneInsteadOfNode);
        Assert.IsTrue(settings.AllowedPersistentIDs.Single(e => e.ID == zone.PersistentId).UseZoneInsteadOfNode);
        Assert.IsTrue(settings.AllowedPersistentIDs.All(entry => entry.AutoGenScanGraphic));
    }

    #region Consumer mirrors
    private class ZoneScanSettingsMirror
    {
        public List<ZoneScanEntryMirror> AllowedPersistentIDs { get; set; } = new();
    }

    private class ZoneScanEntryMirror
    {
        public uint ID { get; set; }
        public bool UseZoneInsteadOfNode { get; set; }
        public bool AutoGenScanGraphic { get; set; }
    }
    #endregion
}
