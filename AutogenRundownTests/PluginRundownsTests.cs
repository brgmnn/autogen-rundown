using System.Reflection;
using AutogenRundown;
using AutogenRundown.DataBlocks;

namespace AutogenRundownTests;

[TestClass]
public class PluginRundowns_Tests
{
    /// <summary>
    /// Every Rundown.R_* persistent id, found by reflection so a newly added rundown is
    /// picked up here without anyone remembering to update this file.
    /// </summary>
    private static IEnumerable<(string Name, uint Id)> RundownIds()
        => typeof(Rundown)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.Name.StartsWith("R_") && field.FieldType == typeof(uint))
            .Select(field => (field.Name, (uint)field.GetValue(null)!));

    /// <summary>
    /// The regression guard: R_Duo was added without a matching PluginRundown, so "Local_6"
    /// silently resolved to None and every consumer of the mapping quietly skipped Duo.
    /// </summary>
    [TestMethod]
    public void Test_EveryRundownIdMapsToItsOwnPluginRundown()
    {
        foreach (var (name, id) in RundownIds())
        {
            var rundown = PluginRundowns.FromRundownKey($"Local_{id}");

            Assert.AreNotEqual(PluginRundown.None, rundown, $"{name} (Local_{id}) has no PluginRundown");
            Assert.AreEqual(id, (uint)rundown, $"{name} does not line up with PluginRundown.{rundown}");
        }
    }

    [TestMethod]
    public void Test_UnknownRundownKeysMapToNone()
    {
        Assert.AreEqual(PluginRundown.None, PluginRundowns.FromRundownKey("Local_99"));
        Assert.AreEqual(PluginRundown.None, PluginRundowns.FromRundownKey("Local_1000"));
        Assert.AreEqual(PluginRundown.None, PluginRundowns.FromRundownKey(""));
        Assert.AreEqual(PluginRundown.None, PluginRundowns.FromRundownKey(null));
    }

    [TestMethod]
    public void Test_WithLogsHoldsRealRundownsOnly()
    {
        CollectionAssert.AllItemsAreUnique(PluginRundowns.WithLogs);

        // Daily re-rolls every day, so there is nothing to track across sessions
        CollectionAssert.DoesNotContain(PluginRundowns.WithLogs, PluginRundown.Daily);
        CollectionAssert.DoesNotContain(PluginRundowns.WithLogs, PluginRundown.None);

        var ids = RundownIds().Select(rundown => rundown.Id).ToList();

        foreach (var rundown in PluginRundowns.WithLogs)
            CollectionAssert.Contains(ids, (uint)rundown, $"PluginRundown.{rundown} has no Rundown.R_* id");
    }
}
