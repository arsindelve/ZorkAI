using FluentAssertions;
using Model;
using Model.AIGeneration;
using Model.Interface;
using Moq;
using ZorkOne.GlobalCommand.Implementation;

namespace ZorkOne.Tests;

[TestFixture]
public class DiagnoseProcessorTests
{
    [SetUp]
    public void Setup()
    {
        _diagnoseProcessor = new DiagnoseProcessor();
        _mockContext = new Mock<IContext>();
        _mockGenerationClient = new Mock<IGenerationClient>();
        _testZorkContext = new TestZorkIContext();
        _runtime = new Runtime();
    }

    private DiagnoseProcessor _diagnoseProcessor;
    private Mock<IContext> _mockContext;
    private Mock<IGenerationClient> _mockGenerationClient;
    private Runtime _runtime;
    private TestZorkIContext _testZorkContext;

    [Test]
    public async Task Process_WithNoWoundsNoDeaths_ReturnsOnlyHealthyMessage()
    {
        // Arrange
        _testZorkContext.LightWoundCounter = 0;
        _testZorkContext.DeathCounter = 0;

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().Be("You are in perfect health.\nYou can be killed by a serious wound. ");
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(10)]
    public async Task Process_WithLightWoundOnly_ReturnsWoundMessage(int woundCounter)
    {
        // Arrange
        _testZorkContext.LightWoundCounter = woundCounter;
        _testZorkContext.DeathCounter = 0;

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().Be($"You have a light wound, which will be cured after {woundCounter} moves.\nYou can be killed by one more light wound. ");
    }

    [Test]
    public async Task Process_WithOneDeathOnly_ReportsOneDeathAndHealthy()
    {
        // Arrange
        _testZorkContext.LightWoundCounter = 0;
        _testZorkContext.DeathCounter = 1;

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().Be("You are in perfect health.\nYou can be killed by a serious wound. \nYou have been killed once. ");
    }

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(10)]
    public async Task Process_WithMultipleDeathsOnly_ReportsMultipleDeathsAndHealthy(int deathCount)
    {
        // Arrange
        _testZorkContext.LightWoundCounter = 0;
        _testZorkContext.DeathCounter = deathCount;

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().Be($"You are in perfect health.\nYou can be killed by a serious wound. \nYou have been killed {deathCount} times. ");
    }

    [TestCase(1, 1)]
    [TestCase(5, 1)]
    [TestCase(10, 1)]
    public async Task Process_WithLightWoundAndOneDeath_ReportsWoundAndOneDeath(int woundCounter, int deathCount)
    {
        // Arrange
        _testZorkContext.LightWoundCounter = woundCounter;
        _testZorkContext.DeathCounter = deathCount;

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().Be($"You have a light wound, which will be cured after {woundCounter} moves.\nYou can be killed by one more light wound. \nYou have been killed once. ");
    }

    [TestCase(1, 2)]
    [TestCase(3, 5)]
    [TestCase(7, 10)]
    public async Task Process_WithLightWoundAndMultipleDeaths_ReportsWoundAndMultipleDeaths(int woundCounter, int deathCount)
    {
        // Arrange
        _testZorkContext.LightWoundCounter = woundCounter;
        _testZorkContext.DeathCounter = deathCount;

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().Be($"You have a light wound, which will be cured after {woundCounter} moves.\nYou can be killed by one more light wound. \nYou have been killed {deathCount} times. ");
    }

    [Test]
    public async Task Process_WithZeroLightWoundCounter_ReportsHealthy()
    {
        // Arrange
        _testZorkContext.LightWoundCounter = 0;
        _testZorkContext.DeathCounter = 0;

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().Contain("You are in perfect health");
        result.Should().Contain("You can be killed by a serious wound");
    }

    [Test]
    public async Task Process_WithNegativeLightWoundCounter_TreatsAsHealthy()
    {
        // Arrange
        _testZorkContext.LightWoundCounter = -1; // Edge case
        _testZorkContext.DeathCounter = 0;

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().Contain("You are in perfect health");
        result.Should().NotContain("light wound");
    }

    [Test]
    public async Task Process_WithMaxLightWoundCounter_HandlesLargeNumbers()
    {
        // Arrange
        _testZorkContext.LightWoundCounter = int.MaxValue; // Edge case
        _testZorkContext.DeathCounter = 0;

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().Contain($"will be cured after {int.MaxValue} moves");
    }

    [Test]
    public async Task Process_WithNegativeDeathCounter_HandlesEdgeCase()
    {
        // Arrange - edge case that shouldn't happen in normal gameplay
        _testZorkContext.LightWoundCounter = 0;
        _testZorkContext.DeathCounter = -1;

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().NotContain("You have been killed");
    }

    [Test]
    public async Task Process_WithMaxDeathCounter_HandlesLargeNumbers()
    {
        // Arrange
        _testZorkContext.LightWoundCounter = 0;
        _testZorkContext.DeathCounter = int.MaxValue; // Edge case

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().Contain($"You have been killed {int.MaxValue} times");
    }

    [Test]
    public async Task Process_WithExtremeCombination_HandlesEdgeCase()
    {
        // Arrange - extreme combination
        _testZorkContext.LightWoundCounter = int.MaxValue;
        _testZorkContext.DeathCounter = int.MaxValue;

        // Act
        var result = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Should().Contain($"will be cured after {int.MaxValue} moves");
        result.Should().Contain($"You have been killed {int.MaxValue} times");
    }

    [Test]
    public async Task Process_IgnoresInputValue_SameResponseForDifferentInputs()
    {
        // Arrange
        _testZorkContext.LightWoundCounter = 5;
        _testZorkContext.DeathCounter = 2;

        // Act - try with different inputs including null
        var result1 = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);
        var result2 = await _diagnoseProcessor.Process("anything", _testZorkContext, _mockGenerationClient.Object, _runtime);
        var result3 = await _diagnoseProcessor.Process(null, _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert - response should be based on context, not input
        result1.Should().Be(result2);
        result2.Should().Be(result3);
        result1.Should().Contain("light wound");
        result1.Should().Contain("killed 2 times");
    }
    
    [Test]
    public async Task Process_WithChangingContext_ResponseChangesAccordingly()
    {
        // Arrange - initial state
        _testZorkContext.LightWoundCounter = 5;
        _testZorkContext.DeathCounter = 0;

        // Act - first call
        var initialResult = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Change context
        _testZorkContext.LightWoundCounter = 0;
        _testZorkContext.DeathCounter = 1;

        // Act - second call with same input but different context
        var updatedResult = await _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        initialResult.Should().Contain("light wound");
        initialResult.Should().NotContain("been killed"); // Fixed assertion

        updatedResult.Should().Contain("perfect health");
        updatedResult.Should().Contain("killed once");

        initialResult.Should().NotBe(updatedResult);
    }
    
    [Test]
    public void Process_WithNoWounds_ReturnsHealthyMessage()
    {
        // Arrange
        _testZorkContext.LightWoundCounter = 0;

        // Act
        var result = _diagnoseProcessor.Process("diagnose", _testZorkContext, _mockGenerationClient.Object, _runtime);

        // Assert
        result.Result.Should().Contain("You are in perfect health");
    }

    [Test]
    public void Process_WithNonZorkContext_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _diagnoseProcessor.Process("diagnose", _mockContext.Object, _mockGenerationClient.Object, _runtime));
    }

    /// <summary>
    /// A test implementation of ZorkIContext for testing purposes
    /// </summary>
    private class TestZorkIContext : ZorkIContext
    {
    }
}