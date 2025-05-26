using FluentAssertions;
using Model;
using Model.AIGeneration;
using Model.Interface;
using Moq;
using ZorkOne;
using ZorkOne.GlobalCommand.Implementation;

namespace ZorkOne.Tests.GlobalCommand;

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
        public int LightWoundCounter { get; set; } = 0;
    }
}