using ChatLambda;
using FluentAssertions;
using GameEngine;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;
using Moq;
using NUnit.Framework;
using ZorkOne;

namespace UnitTests.Engine;

[TestFixture]
public class ConversationHandlerTests
{
    [Test]
    public async Task CheckForConversation_ReturnsNull_WhenDisabled()
    {
        // Arrange
        var mockParser = new Mock<IParseConversation>();
        var mockClient = new Mock<IGenerationClient>();
        mockClient.Setup(c => c.IsDisabled).Returns(true);
        var handler = new ConversationHandler(null, mockParser.Object, mockClient.Object);
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("floyd, go north", context);

        // Assert
        result.Should().BeNull();
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_ReturnsNull_WhenEnabledButNoTalkableEntities()
    {
        // Arrange
        var mockParser = new Mock<IParseConversation>();
        var mockClient = new Mock<IGenerationClient>();
        var handler = new ConversationHandler(null, mockParser.Object, mockClient.Object);
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("hello", context);

        // Assert
        result.Should().BeNull();
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_ReturnsNull_WhenNoMatchingCharacter()
    {
        // Arrange
        var mockParser = new Mock<IParseConversation>();
        var mockClient = new Mock<IGenerationClient>();

        var mockTalker = new Mock<ICanBeTalkedTo>();
        mockTalker.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "robot" });

        var handler = new ConversationHandler(null, mockParser.Object, mockClient.Object);
        var context = new ZorkIContext();
        context.Items.Add((IItem)mockTalker.Object);

        // Act - talking about "floyd" but only "robot" is available
        var result = await handler.CheckForConversation("floyd, go north", context);

        // Assert
        result.Should().BeNull();
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_ReturnsNull_WhenParseConversationSaysNotConversational()
    {
        // Arrange
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>()))
                  .ReturnsAsync((false, string.Empty));

        var mockTalker = new Mock<ICanBeTalkedTo>();
        mockTalker.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "floyd" });

        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>());
        var context = new ZorkIContext();
        context.Items.Add((IItem)mockTalker.Object);

        // Act
        var result = await handler.CheckForConversation("floyd, go north", context);

        // Assert
        result.Should().BeNull();
        mockParser.Verify(p => p.ParseAsync("floyd, go north"), Times.Once);
    }

    [Test]
    public async Task CheckForConversation_ReturnsResponse_WhenConversationDetected()
    {
        // Arrange
        const string expectedResponse = "I'm moving north now!";

        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>()))
                  .ReturnsAsync((true, "go north"));

        var mockTalker = new Mock<ICanBeTalkedTo>();
        mockTalker.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "floyd" });
        mockTalker.Setup(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()))
                  .ReturnsAsync(expectedResponse);

        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>());
        var context = new ZorkIContext();
        context.Items.Add((IItem)mockTalker.Object);

        // Act
        var result = await handler.CheckForConversation("floyd, go north", context);

        // Assert
        result.Should().Be(expectedResponse);
        mockParser.Verify(p => p.ParseAsync("floyd, go north"), Times.Once);
        mockTalker.Verify(t => t.OnBeingTalkedTo("go north", context, It.IsAny<IGenerationClient>()), Times.Once);
    }

    [Test]
    public async Task CheckForConversation_FindsCharacterInLocation()
    {
        // Arrange
        const string expectedResponse = "Hello there!";

        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>()))
                  .ReturnsAsync((true, "hello"));

        var mockTalker = new Mock<ICanBeTalkedTo>();
        mockTalker.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "troll" });
        mockTalker.Setup(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()))
                  .ReturnsAsync(expectedResponse);

        var mockLocation = new Mock<ILocation>();
        mockLocation.As<ICanContainItems>()
                    .Setup(l => l.Items)
                    .Returns(new List<IItem> { (IItem)mockTalker.Object });

        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>());
        var context = new ZorkIContext
        {
            CurrentLocation = mockLocation.Object
        };

        // Act
        var result = await handler.CheckForConversation("troll, hello", context);

        // Assert
        result.Should().Be(expectedResponse);
    }

    [Test]
    public async Task CheckForConversation_CaseInsensitiveMatching()
    {
        // Arrange
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>()))
                  .ReturnsAsync((true, "HELLO"));

        var mockTalker = new Mock<ICanBeTalkedTo>();
        mockTalker.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "floyd" });
        mockTalker.Setup(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()))
                  .ReturnsAsync("Response");

        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>());
        var context = new ZorkIContext();
        context.Items.Add((IItem)mockTalker.Object);

        // Act - Input is uppercase, noun is lowercase
        var result = await handler.CheckForConversation("FLOYD, HELLO", context);

        // Assert
        result.Should().Be("Response");
    }

    [Test]
    public async Task CheckForConversation_MatchesPartialNounInInput()
    {
        // Arrange
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>()))
                  .ReturnsAsync((true, "thank you"));

        var mockTalker = new Mock<ICanBeTalkedTo>();
        mockTalker.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "ambassador" });
        mockTalker.Setup(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()))
                  .ReturnsAsync("You're welcome");

        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>());
        var context = new ZorkIContext();
        context.Items.Add((IItem)mockTalker.Object);

        // Act - "ambassador" appears in the middle of the input
        var result = await handler.CheckForConversation("tell the ambassador thank you", context);

        // Assert
        result.Should().Be("You're welcome");
    }
}
