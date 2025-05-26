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

    [TestCase(2, 1000)]
    [TestCase(6, 1000)]
    [TestCase(20, 1000)]
    public void RollDiceSuccess_ProbabilityTest_ShouldBeReasonablyAccurate(int sides, int iterations)
    {
        // Arrange
        var successes = 0;
        var expectedProbability = 1.0 / sides;

        // Act
        for (var i = 0; i < iterations; i++)
            if (_randomChooser.RollDiceSuccess(sides))
                successes++;

        var actualProbability = (double)successes / iterations;

        // Assert
        // Allow for some deviation (e.g., 30% of the expected probability)
        var allowedDeviation = expectedProbability * 0.3;

        Math.Abs(actualProbability - expectedProbability).Should().BeLessThanOrEqualTo(allowedDeviation,
            $"Expected probability around {expectedProbability}, got {actualProbability}");
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

    [TestCase(6, 1000)]
    [TestCase(20, 1000)]
    public void RollDice_DistributionTest_ShouldBeReasonablyDistributed(int sides, int iterations)
    {
        // Arrange
        var counts = new Dictionary<int, int>();

        // Initialize counts dictionary
        for (var i = 1; i <= sides; i++) counts[i] = 0;

        // Act
        for (var i = 0; i < iterations; i++)
        {
            var result = _randomChooser.RollDice(sides);
            counts[result]++;
        }

        // Assert
        // Calculate expected average and acceptable deviation
        var expectedAvg = (double)iterations / sides;
        
        // Use a statistical approach based on standard deviation
        // For a discrete uniform distribution, standard deviation is approximately sqrt(expectedAvg)
        // We'll allow 3 standard deviations (99.7% of normal distribution)
        var standardDeviation = Math.Sqrt(expectedAvg);
        var maxDeviation = Math.Max(3 * standardDeviation, 0.6 * expectedAvg);
        
        // For very small expected averages, ensure we have at least a minimum absolute deviation allowed
        maxDeviation = Math.Max(maxDeviation, 6.0);

        // Check that all possible values were generated at least once
        counts.Should().NotContainValue(0, "all possible values should be rolled at least once");

        // Check that the distribution is reasonably uniform
        foreach (var key in counts.Keys)
            Math.Abs(counts[key] - expectedAvg).Should().BeLessThanOrEqualTo(maxDeviation,
                $"Value {key} had {counts[key]} occurrences, which deviates too much from expected {expectedAvg}");
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}