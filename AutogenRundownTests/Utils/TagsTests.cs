using AutogenRundown.Utils;

namespace AutogenRundownTests.Utils;

[TestClass]
public class Tags_Tests
{
    [TestMethod]
    public void Test_TagsCanBeAddedAndChecked()
    {
        var tags = new Tags("hello", "world");

        Assert.AreEqual(tags.Contains("hello"), true);
    }

    [TestMethod]
    public void Test_TagsCanBePrintedToString()
    {
        var tags = new Tags("hello", "world");

        Assert.AreEqual(tags.ToString(), "{hello, world}");
    }
}
