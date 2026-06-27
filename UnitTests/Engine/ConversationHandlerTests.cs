using ChatLambda;
using FluentAssertions;
using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
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

    /// <summary>
    /// A known talker named "floyd" that also answers to the generic synonym "robot" — mirrors the
    /// real Floyd, used to verify the synonym is not treated as a name for direct address.
    /// </summary>
    public class RobotTalker : ItemBase, ICanBeTalkedTo
    {
        public override string[] NounsForMatching => ["floyd", "robot"];

        public override string GenericDescription(ILocation? currentLocation) => string.Empty;

        public Task<string> OnBeingTalkedTo(string text, IContext context, IGenerationClient client) =>
            Task.FromResult("floyd talked");
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

    [TestCase("blather you are a fool", "you are a fool", TestName = "NoComma_Statement")]
    [TestCase("blather, you are a fool", "you are a fool", TestName = "Comma_Statement")]
    [TestCase("blather what should i do now", "what should i do now", TestName = "NoComma_Question")]
    [TestCase("hey blather you fool", "you fool", TestName = "Opener_NoComma")]
    [TestCase("ensign blather you fool", "you fool", TestName = "MultiWordName_NoComma")]
    public async Task CheckForConversation_PresentTalker_NamedDirectAddress_RoutesEvenWhenClassifierSaysNo(
        string input, string expectedRemainder)
    {
        // #286: addressing a PRESENT talkable NPC by name must engage them even when the
        // nondeterministic classifier misfires and reports "not conversational". The deterministic
        // backstop used to require a comma after the name, so the bare "blather you are a fool" form
        // had no fallback and ~3-5% of the time deflected to a generic third-person narrator quip. It
        // now mirrors the absent-NPC path and recognizes the leading-name form (bare, vocative, or
        // behind a casual opener) without a comma, routing the text after the name to the character.
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>())).ReturnsAsync((false, string.Empty));

        var mockTalker = new Mock<ICanBeTalkedTo>();
        mockTalker.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "blather", "ensign blather" });
        mockTalker.Setup(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()))
                  .ReturnsAsync("Blather sneers.");

        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>());
        var context = new ZorkIContext();
        context.Items.Add((IItem)mockTalker.Object);

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert - engaged with the text after the leading name, not dropped to the narrator.
        result.Should().Be("Blather sneers.");
        mockTalker.Verify(
            t => t.OnBeingTalkedTo(expectedRemainder, context, It.IsAny<IGenerationClient>()),
            Times.Once);
    }

    [TestCase("robot you are broken", "you are broken", TestName = "Synonym_NoComma")]
    [TestCase("the robot is broken", "is broken", TestName = "Synonym_WithLeadingArticle")]
    public async Task CheckForConversation_PresentTalker_AddressedBySynonym_RoutesEvenWhenClassifierSaysNo(
        string input, string expectedRemainder)
    {
        // #286: a present NPC answers to its synonyms too (Floyd IS the "robot", the Ambassador the
        // "alien"). Leading with a synonym is direct address just like leading with the name, mirroring
        // the absent path (see CheckForConversation_AbsentKnownTalker_GenericSynonymIsTreatedAsAddress).
        // This is the one deliberate behavior change: such input used to fall through when the
        // classifier said "no"; now it deterministically reaches the present NPC.
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>())).ReturnsAsync((false, string.Empty));

        var mockTalker = new Mock<ICanBeTalkedTo>();
        mockTalker.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "floyd", "robot" });
        mockTalker.Setup(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()))
                  .ReturnsAsync("Floyd beeps.");

        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>());
        var context = new ZorkIContext();
        context.Items.Add((IItem)mockTalker.Object);

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert
        result.Should().Be("Floyd beeps.");
        mockTalker.Verify(
            t => t.OnBeingTalkedTo(expectedRemainder, context, It.IsAny<IGenerationClient>()),
            Times.Once);
    }

    [TestCase("examine blather", TestName = "BareMention")]
    [TestCase("look at blather", TestName = "LookAtMention")]
    [TestCase("attack blather with the brush", TestName = "AttackMention")]
    [TestCase("the lever near blather is stuck", TestName = "NameMidSentence")]
    public async Task CheckForConversation_PresentTalker_NonAddress_DoesNotHijack(string input)
    {
        // #286 guard: the name is merely mentioned, not used to address the NPC. With the comma
        // requirement relaxed, the leading-name check (after stripping an opener/article) must still
        // decline these so a real command is not hijacked into the conversation handler. The classifier
        // is consulted and (unconfigured) also says "not conversational", so the input falls through to
        // normal parsing and the character is never engaged.
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>())).ReturnsAsync((false, string.Empty));

        var mockTalker = new Mock<ICanBeTalkedTo>();
        mockTalker.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "blather" });
        mockTalker.Setup(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()))
                  .ReturnsAsync("should not be called");

        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>());
        var context = new ZorkIContext();
        context.Items.Add((IItem)mockTalker.Object);

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert - not hijacked: the character is never engaged and the input falls through.
        result.Should().BeNull();
        mockTalker.Verify(
            t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()),
            Times.Never);
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
        // Arrange - Gizmo is a known talker (in the roster) but NOT in the room/inventory. The
        // generation client here produces nothing, so we get the static fallback (the genuine
        // narrator path is exercised by NarratesAbsenceViaGenerationWhenEnabled below).
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("gizmo, go up", context);

        // Assert - short-circuits with the "isn't here" message, never the AI conversation rewriter.
        result.Should().Be("Gizmo isn't here. ");
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_AbsentKnownTalker_NarratesAbsenceViaGenerationWhenEnabled()
    {
        // Arrange - when generation is available, the narrator tells the player in its own voice.
        var mockParser = new Mock<IParseConversation>();
        var mockClient = new Mock<IGenerationClient>();
        mockClient.Setup(c => c.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
                  .ReturnsAsync("You look around, but Gizmo is nowhere to be seen.");
        var handler = new ConversationHandler(null, mockParser.Object, mockClient.Object,
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("gizmo, go up", context);

        // Assert - the generated narration is used, and it is a narration request (not the AI rewriter).
        result.Should().Be("You look around, but Gizmo is nowhere to be seen.");
        mockClient.Verify(c => c.GenerateNarration(It.IsAny<TalkingToAbsentCharacterRequest>(), It.IsAny<string>()),
            Times.Once);
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_AbsentKnownTalker_FallsBackToStaticTextWhenGenerationEmpty()
    {
        // Arrange - graceful degradation: if generation yields nothing, fall back to the static line
        // rather than leaking the command back into player parsing.
        var mockParser = new Mock<IParseConversation>();
        var mockClient = new Mock<IGenerationClient>();
        mockClient.Setup(c => c.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
                  .ReturnsAsync(string.Empty);
        var handler = new ConversationHandler(null, mockParser.Object, mockClient.Object,
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("gizmo, go up", context);

        // Assert
        result.Should().Be("Gizmo isn't here. ");
    }

    [Test]
    public async Task CheckForConversation_AbsentKnownTalker_DoesNotCallGenerationWhenDisabled()
    {
        // Arrange - in NoGeneratedResponses mode the guard must stay deterministic and not call AI.
        var mockParser = new Mock<IParseConversation>();
        var mockClient = new Mock<IGenerationClient>();
        mockClient.Setup(c => c.IsDisabled).Returns(true);
        var handler = new ConversationHandler(null, mockParser.Object, mockClient.Object,
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("gizmo, go up", context);

        // Assert
        result.Should().Be("Gizmo isn't here. ");
        mockClient.Verify(c => c.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()), Times.Never);
    }

    [TestCase("floyd go up")]
    [TestCase("floyd drop the diary")]
    public async Task CheckForConversation_AbsentKnownTalker_BareLeadingNameNoComma_SaysNotHere(string input)
    {
        // Arrange - the bare "Name <rest>" form (no comma, no lead-in verb) is the gap reported on
        // the PR: it used to leak straight to player parsing and move/drop for the player.
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(RobotTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert
        result.Should().Be("Floyd isn't here. ");
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [TestCase("captain, go north", TestName = "VocativeWithArticle")]
    [TestCase("the captain, go north", TestName = "VocativeWithLeadingArticle")]
    [TestCase("the captain go north", TestName = "BareWithLeadingArticle")]
    public async Task CheckForConversation_AbsentKnownTalker_LeadingArticle_SaysNotHere(string input)
    {
        // Arrange - "the <name>, ..." must be caught too, not just the bare/imperative forms.
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(CaptainTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert
        result.Should().Be("The captain isn't here. ");
    }

    [TestCase("tell the robot to go up", TestName = "ImperativeWithSynonym")]
    [TestCase("robot, go up", TestName = "VocativeWithSynonym")]
    [TestCase("robot go up", TestName = "BareSynonym")]
    [TestCase("hey robot, go up", TestName = "InterjectionWithSynonym")]
    public async Task CheckForConversation_AbsentKnownTalker_GenericSynonymIsTreatedAsAddress(string input)
    {
        // Arrange - Floyd answers to "robot", so addressing "the robot" reaches him (owner's call).
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(RobotTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert - deterministic, so the classifier is not consulted.
        result.Should().Be("Floyd isn't here. ");
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [TestCase("hey gizmo, go up", TestName = "Hey")]
    [TestCase("yo gizmo", TestName = "Yo")]
    [TestCase("hi gizmo, where are you", TestName = "Hi")]
    [TestCase("hello gizmo", TestName = "Hello")]
    public async Task CheckForConversation_AbsentKnownTalker_InterjectionOpener_SaysNotHere(string input)
    {
        // Arrange
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert - deterministic, no classifier call.
        result.Should().Be("Gizmo isn't here. ");
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_AbsentKnownTalker_DefersToClassifierForUnusualPhrasing()
    {
        // Arrange - not one of the explicit forms, but it names Gizmo and the classifier says it's
        // conversational, so the LLM-backed path recognizes it as address.
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>())).ReturnsAsync((true, "where are you"));
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("could you let gizmo know to wait for me", context);

        // Assert - classifier consulted (deterministic check missed), then narrated absence (fallback).
        result.Should().Be("Gizmo isn't here. ");
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task CheckForConversation_AbsentKnownTalker_ClassifierSaysNotConversational_FallsThrough()
    {
        // Arrange - names Gizmo but the classifier says it's a command about Gizmo, not address.
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>())).ReturnsAsync((false, string.Empty));
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("the lever near gizmo is stuck", context);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task CheckForConversation_AbsentKnownTalker_UnusualPhrasing_NotConsultedWhenGenerationDisabled()
    {
        // Arrange - offline (NoGeneratedResponses): the classifier backstop is skipped, so only the
        // deterministic forms are caught. A non-explicit phrasing falls through rather than calling AI.
        var mockParser = new Mock<IParseConversation>();
        var mockClient = new Mock<IGenerationClient>();
        mockClient.Setup(c => c.IsDisabled).Returns(true);
        var handler = new ConversationHandler(null, mockParser.Object, mockClient.Object,
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("could you let gizmo know to wait", context);

        // Assert
        result.Should().BeNull();
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_TwoKnownTalkers_RecognizesGenuinelyAddressedOne()
    {
        // Arrange - both are known and absent. The sentence mentions "gizmo" but genuinely addresses
        // "captain"; the guard must pick the one actually addressed, not the first name it sees.
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker), typeof(CaptainTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("captain, give the gizmo to me", context);

        // Assert
        result.Should().Be("The captain isn't here. ");
    }

    [Test]
    public async Task CheckForConversation_TwoAbsentTalkers_ClassifierFallback_AttributesToEarliestNamed()
    {
        // Arrange - deterministic check misses for both; the classifier says it's address but not
        // whom. Attribution must go to the talker NAMED first ("captain"), not roster order (Gizmo).
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>())).ReturnsAsync((true, ""));
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker), typeof(CaptainTalker) });
        var context = new ZorkIContext();

        // Act - "captain" appears before "gizmo", and neither is an explicit (deterministic) address.
        var result = await handler.CheckForConversation(
            "could you let captain know that gizmo wandered off", context);

        // Assert
        result.Should().Be("The captain isn't here. ");
    }

    [Test]
    public async Task CheckForConversation_PresentTalker_NotMatchedByPartialWord()
    {
        // Arrange - a present NPC named "bob" must not be engaged by a partial-word hit inside
        // "bobbin"; the present path now uses the same whole-word matching as the absent path.
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>())).ReturnsAsync((true, "hello"));

        var mockTalker = new Mock<ICanBeTalkedTo>();
        mockTalker.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "bob" });
        mockTalker.Setup(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()))
                  .ReturnsAsync("engaged");

        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>());
        var context = new ZorkIContext();
        context.Items.Add((IItem)mockTalker.Object);

        // Act
        var result = await handler.CheckForConversation("examine the bobbin", context);

        // Assert
        result.Should().BeNull();
        mockTalker.Verify(
            t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()),
            Times.Never);
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
        // Arrange - the name is merely mentioned; these are real commands, not direct address. The
        // deterministic check declines, the classifier is consulted as the backstop and (unconfigured
        // here) also says "not conversational", so the input falls through to normal parsing.
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>(),
            new[] { typeof(GizmoTalker) });
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert
        result.Should().BeNull();
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Once);
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

    // --- #284: nameless conversational forms route to the SOLE present talker ------------------
    // Bare quoted speech ("you are a fool") and an untargeted "say …"/greeting name no one, so the
    // present-name and absent-name lookups both miss them. When exactly one talkable NPC is in the
    // room they should reach that NPC anyway, the same outcome "blather, …" already produces.

    /// <summary>Builds a handler with a single present mock talker that echoes a sentinel reply.</summary>
    private static (ConversationHandler handler, ZorkIContext context, Mock<ICanBeTalkedTo> talker)
        SinglePresentTalker(Mock<IParseConversation> mockParser, IGenerationClient client, string noun = "floyd")
    {
        var talker = new Mock<ICanBeTalkedTo>();
        talker.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { noun });
        talker.Setup(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()))
              .ReturnsAsync("npc replied");

        var handler = new ConversationHandler(null, mockParser.Object, client);
        var context = new ZorkIContext();
        context.Items.Add((IItem)talker.Object);
        return (handler, context, talker);
    }

    [TestCase("\"you are a fool\"", "you are a fool", TestName = "QuotedSpeech")]
    [TestCase("  \"you are a fool\"  ", "you are a fool", TestName = "QuotedSpeechWithSurroundingWhitespace")]
    [TestCase("“you are a fool”", "you are a fool", TestName = "SmartQuotedSpeech")]
    [TestCase("\"you are a fool", "you are a fool", TestName = "UnterminatedQuotedSpeech")]
    [TestCase("say hello", "hello", TestName = "UntargetedSay")]
    [TestCase("say, hello", "hello", TestName = "UntargetedSayWithComma")]
    [TestCase("shout get out", "get out", TestName = "UntargetedShout")]
    [TestCase("say \"hello\"", "hello", TestName = "UntargetedSayQuoted")]
    [TestCase("hello", "hello", TestName = "BareGreeting")]
    public async Task CheckForConversation_NamelessSpeech_SinglePresentTalker_Routes(string input, string expectedSpoken)
    {
        // Arrange - the classifier is never consulted for the nameless path, so leaving it unset is
        // deliberate (a call would throw on the strict-by-default mock).
        var mockParser = new Mock<IParseConversation>();
        var (handler, context, talker) = SinglePresentTalker(mockParser, Mock.Of<IGenerationClient>());

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert - the words inside the quotes / after the verb are what reaches the NPC.
        result.Should().Be("npc replied");
        talker.Verify(t => t.OnBeingTalkedTo(expectedSpoken, context, It.IsAny<IGenerationClient>()), Times.Once);
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_NamelessSpeech_NoPresentTalker_ReturnsNull()
    {
        // Arrange - quoted speech but nobody is here and the roster is empty: must fall through.
        var mockParser = new Mock<IParseConversation>();
        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>());
        var context = new ZorkIContext();

        // Act
        var result = await handler.CheckForConversation("\"you are a fool\"", context);

        // Assert
        result.Should().BeNull();
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_NamelessSpeech_MultiplePresentTalkers_DoesNotRoute()
    {
        // Arrange - two talkers present and no name in the input: we cannot know who is addressed,
        // so the ambiguous case falls through rather than guessing (issue allows this).
        var mockParser = new Mock<IParseConversation>();

        var floyd = new Mock<ICanBeTalkedTo>();
        floyd.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "floyd" });
        floyd.Setup(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()))
             .ReturnsAsync("floyd replied");
        var captain = new Mock<ICanBeTalkedTo>();
        captain.As<IItem>().Setup(i => i.NounsForMatching).Returns(new[] { "captain" });
        captain.Setup(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()))
               .ReturnsAsync("captain replied");

        var handler = new ConversationHandler(null, mockParser.Object, Mock.Of<IGenerationClient>());
        var context = new ZorkIContext();
        context.Items.Add((IItem)floyd.Object);
        context.Items.Add((IItem)captain.Object);

        // Act
        var result = await handler.CheckForConversation("\"you are a fool\"", context);

        // Assert - neither was engaged.
        result.Should().BeNull();
        floyd.Verify(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()),
            Times.Never);
        captain.Verify(t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()),
            Times.Never);
    }

    [TestCase("say to guard let me pass", TestName = "SayToOtherParty")]
    [TestCase("say, to guard let me pass", TestName = "SayCommaToOtherParty")]
    [TestCase("whisper to ghost that we should leave", TestName = "WhisperToOtherParty")]
    [TestCase("yell at the guard to halt", TestName = "YellAtOtherParty")]
    [TestCase("speak with the wizard", TestName = "SpeakWithOtherParty")]
    public async Task CheckForConversation_DirectedAtAnotherParty_SinglePresentTalker_DoesNotHijack(string input)
    {
        // Arrange - "<speak verb> to/at/with <someone>" names a recipient (here unknown/absent), so
        // it must not be put in the present NPC's mouth — it falls through to normal parsing.
        var mockParser = new Mock<IParseConversation>();
        var (handler, context, talker) = SinglePresentTalker(mockParser, Mock.Of<IGenerationClient>());

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert
        result.Should().BeNull();
        talker.Verify(
            t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()),
            Times.Never);
    }

    [TestCase("examine the wall", TestName = "Examine")]
    [TestCase("go north", TestName = "Move")]
    [TestCase("take the lamp", TestName = "Take")]
    [TestCase("open door", TestName = "Open")]
    public async Task CheckForConversation_RealCommand_SinglePresentTalker_DoesNotHijack(string input)
    {
        // Arrange - a real command that names no talker and is not speech must NOT be swallowed by
        // the present NPC; it has to fall through to normal parsing.
        var mockParser = new Mock<IParseConversation>();
        var (handler, context, talker) = SinglePresentTalker(mockParser, Mock.Of<IGenerationClient>());

        // Act
        var result = await handler.CheckForConversation(input, context);

        // Assert
        result.Should().BeNull();
        talker.Verify(
            t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()),
            Times.Never);
        mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CheckForConversation_NamelessSpeech_SinglePresentTalker_NotRoutedWhenDisabled()
    {
        // Arrange - routing relies on the NPC's AI backend, so the kill-switch must suppress it just
        // like the present-name path, keeping NoGeneratedResponses mode deterministic.
        var mockParser = new Mock<IParseConversation>();
        var mockClient = new Mock<IGenerationClient>();
        mockClient.Setup(c => c.IsDisabled).Returns(true);
        var (handler, context, talker) = SinglePresentTalker(mockParser, mockClient.Object);

        // Act
        var result = await handler.CheckForConversation("\"you are a fool\"", context);

        // Assert
        result.Should().BeNull();
        talker.Verify(
            t => t.OnBeingTalkedTo(It.IsAny<string>(), It.IsAny<IContext>(), It.IsAny<IGenerationClient>()),
            Times.Never);
    }

    [Test]
    public async Task CheckForConversation_NamedAddressToPresentTalker_StillUsesNamedPath()
    {
        // Arrange - guard against the nameless branch shadowing the existing named path: when the
        // talker IS named, the classifier-driven named path runs (and the name is stripped), exactly
        // as before #284.
        var mockParser = new Mock<IParseConversation>();
        mockParser.Setup(p => p.ParseAsync(It.IsAny<string>())).ReturnsAsync((true, "you are a fool"));
        var (handler, context, talker) = SinglePresentTalker(mockParser, Mock.Of<IGenerationClient>());

        // Act
        var result = await handler.CheckForConversation("floyd, you are a fool", context);

        // Assert
        result.Should().Be("npc replied");
        talker.Verify(t => t.OnBeingTalkedTo("you are a fool", context, It.IsAny<IGenerationClient>()), Times.Once);
        mockParser.Verify(p => p.ParseAsync("floyd, you are a fool"), Times.Once);
    }
}
