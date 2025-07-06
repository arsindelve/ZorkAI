using FluentAssertions;
using GameEngine.StaticCommand;
using Model;
using Model.AIGeneration;
using Model.Interface;
using Moq;
using ZorkOne.GlobalCommand;
using ZorkOne.GlobalCommand.Implementation;

namespace ZorkOne.Tests.GlobalCommand;

[TestFixture]
public class ZorkOneGlobalCommandFactoryTests
{
    [SetUp]
    public void Setup()
    {
        _factory = new ZorkOneGlobalCommandFactory();
        _mockContext = new Mock<IContext>();
        _mockGenerationClient = new Mock<IGenerationClient>();
        _runtime = new Runtime();
    }

    private ZorkOneGlobalCommandFactory _factory;
    private Mock<IContext> _mockContext;
    private Mock<IGenerationClient> _mockGenerationClient;
    private Runtime _runtime;

    [TestCase("repent", "It could very well be too late! ")]
    [TestCase("REPENT", "It could very well be too late! ")]
    [TestCase("  repent  ", "It could very well be too late! ")]
    [TestCase("re-pent!", "It could very well be too late! ")]
    [TestCase("xyzzy", "A hollow voice says 'fool' ")]
    [TestCase("plugh", "A hollow voice says 'fool' ")]
    [TestCase("echo", "echo echo...")]
    [TestCase("ulysses", "Wasn't he a sailor? ")]
    [TestCase("odysseus", "Wasn't he a sailor? ")]
    [TestCase("win", "Naturally!")]
    [TestCase("zork", "At your service!")]
    [TestCase("frobozz", "The FROBOZZ Corporation created, owns, and operates this dungeon. ")]
    [TestCase("lose", "Preposterous!")]
    [TestCase("chomp", "Preposterous!")]
    [TestCase("vomit", "Preposterous!")]
    [TestCase("sigh", "You'll have to speak up if you expect me to hear you!")]
    [TestCase("mumble", "You'll have to speak up if you expect me to hear you!")]
    public async Task GetGlobalCommands_WithValidInput_ReturnsSimpleResponseCommand(string input, string expectedResponse)
    {
        // Act
        var command = _factory.GetGlobalCommands(input);
        var result = await command?.Process(input, _mockContext.Object, _mockGenerationClient.Object, _runtime);

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<SimpleResponseCommand>();
        result.Should().Be(expectedResponse);
    }

    [TestCase("diagnose")]
    public void GetGlobalCommands_WithDiagnose_ReturnsDiagnoseProcessor(string input)
    {
        // Act
        var command = _factory.GetGlobalCommands(input);

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<DiagnoseProcessor>();
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("unknowncommand")]
    public void GetGlobalCommands_WithInvalidInput_ReturnsNull(string input)
    {
        // Act
        var command = _factory.GetGlobalCommands(input);

        // Assert
        command.Should().BeNull();
    }
}