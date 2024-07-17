using AutogenRundown;

namespace AutogenRundownTests;

[TestClass]
public class Generator_Tests
{
    [TestMethod]
    public void Test_NewPuzzlesGetNoPersistentId()
    {
        // Setup
        Generator.Seed = "123";
        Generator.Reload();

        // Act
        var number = Generator.Random.Next(0, 10);

        // Assert
        Assert.AreEqual(6, number);
    }

    #region DrawSelect()
    /// <summary>
    /// DrawSelect() should draw an element from a pack of multiple copies of varying elements.
    /// </summary>

    [TestMethod]
    public void Test_DrawSelect_DrawsAnElement()
    {
        // Setup
        Generator.Seed = "111";
        Generator.Reload();

        var counts = new Dictionary<string, int>()
        {
            {"hello",  3},
            {"foobar", 6},
            {"world",  9}
        };

        var items = new List<(double, int, string)>()
        {
            (1.0, 3, "hello"),
            (1.0, 6, "foobar"),
            (1.0, 9, "world")
        };

        // Act
        var item = Generator.DrawSelect(items);

        // Assert
        Assert.AreEqual("foobar", item);
        Assert.AreEqual(counts["foobar"] - 1, items.Find(e => e.Item3 == item).Item2);
    }

    [TestMethod]
    public void Test_DrawSelect_DrawsTheLastElement()
    {
        // Setup
        Generator.Seed = "123";
        Generator.Reload();

        var items = new List<(double, int, string)> { (1.0, 3, "hello") };

        // Act
        var item = Generator.DrawSelect(items);

        // Assert
        Assert.AreEqual("hello", item);
        Assert.AreEqual(2, items[0].Item2);
    }
    #endregion
}
