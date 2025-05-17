using FluentAssertions;
using GameEngine.StaticCommand;
using Model;
using Model.AIGeneration;
using Model.Interface;
using Moq;
using ZorkOne.GlobalCommand;

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

    [Test]
    public async Task GetGlobalCommands_WithRepent_ReturnsSimpleResponseCommand()
    {
        // Act
        var command = _factory.GetGlobalCommands("repent");
        var result = await command?.Process("repent", _mockContext.Object, _mockGenerationClient.Object, _runtime);

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<SimpleResponseCommand>();
        result.Should().Be("It could very well be too late! ");
    }

    [Test]
    public async Task GetGlobalCommands_WithCaseInsensitiveInput_ReturnsCorrectCommand()
    {
        // Act
        var command = _factory.GetGlobalCommands("REPENT");
        var result = await command?.Process("REPENT", _mockContext.Object, _mockGenerationClient.Object, _runtime);

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<SimpleResponseCommand>();
        result.Should().Be("It could very well be too late! ");
    }

    [Test]
    public async Task GetGlobalCommands_WithLeadingAndTrailingSpaces_ReturnsCorrectCommand()
    {
        // Act
        var command = _factory.GetGlobalCommands("  repent  ");
        var result = await command?.Process("  repent  ", _mockContext.Object, _mockGenerationClient.Object, _runtime);

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<SimpleResponseCommand>();
        result.Should().Be("It could very well be too late! ");
    }

    [Test]
    public async Task GetGlobalCommands_WithNonAlphaCharacters_ReturnsCorrectCommand()
    {
        // Act
        var command = _factory.GetGlobalCommands("re-pent!");
        var result = await command?.Process("re-pent!", _mockContext.Object, _mockGenerationClient.Object, _runtime);

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<SimpleResponseCommand>();
        result.Should().Be("It could very well be too late! ");
    }

    [Test]
    public void GetGlobalCommands_WithXyzzy_ReturnsMagicWordCommand()
    {
        // Act
        var command = _factory.GetGlobalCommands("xyzzy");

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<SimpleResponseCommand>();
    }

    [Test]
    public void GetGlobalCommands_WithPlugh_ReturnsMagicWordCommand()
    {
        // Act
        var command = _factory.GetGlobalCommands("plugh");

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType<SimpleResponseCommand>();
    }

    [Test]
    public void GetGlobalCommands_WithNoInput_ReturnsNull()
    {
        // Act
        var command = _factory.GetGlobalCommands(null);

        // Assert
        command.Should().BeNull();
    }

    [Test]
    public void GetGlobalCommands_WithEmptyString_ReturnsNull()
    {
        // Act
        var command = _factory.GetGlobalCommands("");

        // Assert
        command.Should().BeNull();
    }

    [Test]
    public void GetGlobalCommands_WithWhitespaceOnly_ReturnsNull()
    {
        // Act
        var command = _factory.GetGlobalCommands("   ");

        // Assert
        command.Should().BeNull();
    }

    [Test]
    public void GetGlobalCommands_WithUnrecognizedCommand_ReturnsNull()
    {
        // Act
        var command = _factory.GetGlobalCommands("unknowncommand");

        // Assert
        command.Should().BeNull();
    }
}