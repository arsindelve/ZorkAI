using Utilities;

namespace UnitTests;

public class NumberConverter
{
    [Test]
    [TestCase("five", 5)]
    [TestCase("5", 5)]
    [TestCase("0", 0)]
    [TestCase("zero", 0)]
    [TestCase("eighty three", 83)]
    [TestCase("one hundred", 100)]
    [TestCase("dude", null)]
    public void TestConversion(string s, int? expected)
    {
        s.ToInteger().Should().Be(expected);
    }
}