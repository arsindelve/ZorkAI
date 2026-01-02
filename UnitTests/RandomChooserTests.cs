using GameEngine;

namespace UnitTests;

[TestFixture]
public class RandomChooserTests
{
    [SetUp]
    public void Setup()
    {
        _randomChooser = new RandomChooser();
    }

    private RandomChooser _randomChooser;

    [Test]
    public void Choose_WithNullList_ThrowsArgumentNullException()
    {
        // Arrange
        List<string>? nullList = null;

        // Act & Assert
        FluentActions.Invoking(() => _randomChooser.Choose(nullList!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Choose_WithEmptyList_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var emptyList = new List<string>();

        // Act & Assert
        FluentActions.Invoking(() => _randomChooser.Choose(emptyList))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Choose_WithSingleItemList_ReturnsThatItem()
    {
        // Arrange
        var singleItemList = new List<string> { "OnlyItem" };

        // Act
        var result = _randomChooser.Choose(singleItemList);

        // Assert
        result.Should().Be("OnlyItem");
    }

    [Test]
    public void Choose_WithMultipleItems_ReturnsOneOfTheItems()
    {
        // Arrange
        var multipleItemList = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };

        // Act
        var result = _randomChooser.Choose(multipleItemList);

        // Assert
        multipleItemList.Should().Contain(result);
    }
    
    [Test]
    public void Choose_WithComplexObjects_ReturnsValidObject()
    {
        // Arrange
        var complexObjects = new List<TestObject>
        {
            new() { Id = 1, Name = "Object 1" },
            new() { Id = 2, Name = "Object 2" },
            new() { Id = 3, Name = "Object 3" }
        };

        // Act
        var result = _randomChooser.Choose(complexObjects);

        // Assert
        complexObjects.Should().Contain(result);
        result.Should().BeOfType<TestObject>();
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public void RollDiceSuccess_WithInvalidSides_ThrowsArgumentException(int sides)
    {
        // Act & Assert
        FluentActions.Invoking(() => _randomChooser.RollDiceSuccess(sides))
            .Should().Throw<ArgumentException>()
            .WithMessage("Number of sides must be greater than 1. (Parameter 'sides')");
    }
  
    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public void RollDice_WithInvalidSides_ThrowsArgumentOutOfRangeException(int sides)
    {
        // Act & Assert
        FluentActions.Invoking(() => _randomChooser.RollDice(sides))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestCase(2)]
    [TestCase(6)]
    [TestCase(20)]
    [TestCase(100)]
    public void RollDice_WithValidSides_ReturnsNumberInValidRange(int sides)
    {
        // Act
        var result = _randomChooser.RollDice(sides);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(1).And.BeLessThanOrEqualTo(sides);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}