
using Model;

namespace UnitTests;

[TestFixture]
public class DirectionParserTests
{
    [TestCase("n", Direction.N)]
    [TestCase("north", Direction.N)]
    [TestCase("e", Direction.E)]
    [TestCase("east", Direction.E)]
    [TestCase("s", Direction.S)]
    [TestCase("south", Direction.S)]
    [TestCase("w", Direction.W)]
    [TestCase("west", Direction.W)]
    [TestCase("sw", Direction.SW)]
    [TestCase("south-west", Direction.SW)]
    [TestCase("south west", Direction.SW)]
    [TestCase("nw", Direction.NW)]
    [TestCase("north-west", Direction.NW)]
    [TestCase("north west", Direction.NW)]
    [TestCase("ne", Direction.NE)]
    [TestCase("north-east", Direction.NE)]
    [TestCase("north east", Direction.NE)]
    [TestCase("se", Direction.SE)]
    [TestCase("south-east", Direction.SE)]
    [TestCase("south east", Direction.SE)]
    [TestCase("up", Direction.Up)]
    [TestCase("u", Direction.Up)]
    [TestCase("down", Direction.Down)]
    [TestCase("d", Direction.Down)]
    [TestCase("in", Direction.In)]
    [TestCase("enter", Direction.In)]
    [TestCase("out", Direction.Out)]
    [TestCase("exit", Direction.Out)]
    [TestCase("", Direction.Unknown)]
    [TestCase(null, Direction.Unknown)]
    [TestCase("unknown direction", Direction.Unknown)]
    public void TestParseDirection(string rawDirection, Direction expectedDirection)
    {
        // Act
        var result = DirectionParser.ParseDirection(rawDirection);

        // Assert
        result.Should().Be(expectedDirection);
    }
}