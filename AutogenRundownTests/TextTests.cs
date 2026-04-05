using AutogenRundown;

namespace AutogenRundownTests;

[TestClass]
public class Text_Tests
{
    #region ToRoman - Basic values

    [TestMethod]
    [DataRow(1, "I")]
    [DataRow(2, "II")]
    [DataRow(3, "III")]
    [DataRow(4, "IV")]
    [DataRow(5, "V")]
    [DataRow(6, "VI")]
    [DataRow(7, "VII")]
    [DataRow(8, "VIII")]
    [DataRow(9, "IX")]
    [DataRow(10, "X")]
    public void Test_ToRoman_SingleDigits(int number, string expected)
    {
        Assert.AreEqual(expected, Text.ToRoman(number));
    }

    [TestMethod]
    [DataRow(11, "XI")]
    [DataRow(14, "XIV")]
    [DataRow(19, "XIX")]
    [DataRow(20, "XX")]
    [DataRow(40, "XL")]
    [DataRow(49, "XLIX")]
    [DataRow(50, "L")]
    [DataRow(90, "XC")]
    [DataRow(99, "XCIX")]
    [DataRow(100, "C")]
    public void Test_ToRoman_TensAndHundreds(int number, string expected)
    {
        Assert.AreEqual(expected, Text.ToRoman(number));
    }

    [TestMethod]
    [DataRow(400, "CD")]
    [DataRow(500, "D")]
    [DataRow(900, "CM")]
    [DataRow(1000, "M")]
    [DataRow(1994, "MCMXCIV")]
    [DataRow(2024, "MMXXIV")]
    [DataRow(3999, "MMMCMXCIX")]
    public void Test_ToRoman_LargerValues(int number, string expected)
    {
        Assert.AreEqual(expected, Text.ToRoman(number));
    }

    #endregion

    #region ToRoman - Edge cases

    [TestMethod]
    public void Test_ToRoman_ZeroReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, Text.ToRoman(0));
    }

    [TestMethod]
    public void Test_ToRoman_NegativeThrows()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Text.ToRoman(-1));
    }

    [TestMethod]
    public void Test_ToRoman_Above3999Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Text.ToRoman(4000));
    }

    [TestMethod]
    public void Test_ToRoman_MaxValid()
    {
        Assert.AreEqual("MMMCMXCIX", Text.ToRoman(3999));
    }

    #endregion
}
