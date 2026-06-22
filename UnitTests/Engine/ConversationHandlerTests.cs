using ChatLambda;
using FluentAssertions;
using GameEngine;
using GameEngine.Item;
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
    [SetUp]
    public void SetUp()
    {
        // CollectAllKnownTalkers resolves the roster through the (static) Repository, so keep it
        // clean between tests.
        Repository.Reset();
    }

    /// <summary>A known talkable NPC whose default "isn't here" text capitalizes its name.</summary>
    public class GizmoTalker : ItemBase, ICanBeTalkedTo
    {
        public override string[] NounsForMatching => ["gizmo"];

        public override string GenericDescription(ILocation? currentLocation) => string.Empty;

        public Task<string> OnBeingTalkedTo(string text, IContext context, IGenerationClient client) =>
            Task.FromResult("gizmo talked");
    }

    /// <summary>A known talkable NPC that overrides the "isn't here" text (a title, not a name).</summary>
    public class CaptainTalker : ItemBase, ICanBeTalkedTo
    {
        public override string[] NounsForMatching => ["captain"];

        public override string GenericDescription(ILocation? currentLocation) => string.Empty;

        public string NotHereDescription => "The captain isn't here. ";

        public Task<string> OnBeingTalkedTo(string text, IContext context, IGenerationClient client) =>
            Task.FromResult("captain talked");
    }

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

    // --- #264: addressing a KNOWN but ABSENT talkable NPC -----------------------------------

    [Test]
    public async Task CheckForConversation_AbsentKnownTalker_VocativeAddress_SaysNotHere()
    {
        // Arrange - Gizmo is a known talker (in the roster) but NOT in the room/inventory.
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("gizmo, go up", context);

        // Assert - short-circuits with the deterministic "isn't here" message, no AI rewrite.
        result.Should().Be("Gizmo isn't here. ");
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_AbsentKnownTalker_RunsEvenWhenGenerationDisabled()
    {
        // Arrange - the absent-NPC guard is deterministic, so it must fire in NoGeneratedResponses mode.
        var mockParser = new Mock<IParseConversation>();
        var mockClient = new Mock<IGenerationClient>();
        mockClient.Setup(c => c.IsDisabled).Returns(true);
        var handler = new ConversationHandler(null, mockParser.Object, mockClient.Object,
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("gizmo, drop the lamp", context);

        // Assert
        result.Should().Be("Gizmo isn't here. ");
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_AbsentKnownTalker_UsesOverriddenNotHereDescription()
    {
        // Arrange
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(CaptainTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("captain, go north", context);

        // Assert
        result.Should().Be("The captain isn't here. ");
    }

    [TestCase("tell gizmo to go up")]
    [TestCase("ask gizmo about the lamp")]
    [TestCase("talk to gizmo")]
    [TestCase("yell at gizmo to stop")]
    public async Task CheckForConversation_AbsentKnownTalker_ImperativeAddress_SaysNotHere(string input)
    {
        // Arrange
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert
        result.Should().Be("Gizmo isn't here. ");
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [TestCase("examine gizmo", TestName = "BareMention")]
    [TestCase("look at the gizmo", TestName = "LookAtMention")]
    [TestCase("ask about the gizmo", TestName = "NameNotImmediatelyAfterVerb")]
    [TestCase("attack gizmo with sword", TestName = "AttackMention")]
    public async Task CheckForConversation_AbsentKnownTalker_NonAddress_DoesNotHijack(string input)
    {
        // Arrange - the name is merely mentioned; these are real commands, not direct address.
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert - falls through to normal parsing; the guard does not fire.
        result.Should().BeNull();
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_AbsentKnownTalker_NonVocativeMention_DoesNotHijack()
    {
        // Arrange - "examine gizmo" merely mentions the name; it is a real command, not direct address.
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("examine gizmo", context);

        // Assert - falls through to normal parsing; the guard does not fire.
        result.Should().BeNull();
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_UnknownName_NotInRoster_ReturnsNull()
    {
        // Arrange - "wizard" is not a known talker anywhere.
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("wizard, go up", context);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task CheckForConversation_PresentTalker_PrefersConversationOverNotHere()
    {
        // Arrange - the talker is BOTH in the roster and present in the room. Presence wins: we
        // route the utterance to them rather than saying "isn't here".
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>())).ReturnsAsync((true, "hello"));

        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();
        context.Items.Add(Repository.GetItem<GizmoTalker>());

        // Act
        var result = await handler.CheckForConversation("gizmo, hello", context);

        // Assert
        result.Should().Be("gizmo talked");
        result.Should().NotContain("isn't here");
    }
}
