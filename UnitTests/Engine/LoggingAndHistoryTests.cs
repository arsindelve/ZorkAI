using ChatLambda;
using CloudWatch;
using CloudWatch.Model;
using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.AIParsing;
using Model.Intent;
using Model.Interface;
using Model.Item;
using Model.Location;
using ZorkOne;
using ZorkOne.GlobalCommand;
using ZorkOne.Item;
using ZorkOne.Location;

namespace UnitTests.Engine;

public class LoggingAndHistoryTests
{
    private static GameEngine<ZorkI, ZorkIContext> BuildEngine(
        Mock<IGenerationClient> client,
        Mock<ICloudWatchLogger<TurnLog>> turnLogger,
        IIntentParser? intentParser = null)
    {
        // Simple take/drop parser used by ItemProcessorFactory in tests
        var takeAndDropParser = new Mock<IAITakeAndAndDropParser>();
        takeAndDropParser
            .Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string _) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
            });
        takeAndDropParser
            .Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string _) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
            });

        var itemProcessorFactory = new ItemProcessorFactory(takeAndDropParser.Object);
        var parser = intentParser ?? new IntentParser(Mock.Of<IAIParser>(), new ZorkOneGlobalCommandFactory());
        var secrets = Mock.Of<ISecretsManager>();

        var engine = new GameEngine<ZorkI, ZorkIContext>(
            itemProcessorFactory,
            parser,
            client.Object,
            secrets,
            turnLogger.Object,
            Mock.Of<IParseConversation>());

        // Ensure starting location is initialized consistently. NOTE: no Repository.Reset() here —
        // the test constructor above already reset it before building Context, so resetting again
        // would hand Init() a SECOND WestOfHouse instance while Context.CurrentLocation still points
        // at the first, leaving the player standing in a room with no mailbox in it.
        Repository.GetLocation<WestOfHouse>().Init();
        engine.Context.Verbosity = Verbosity.Verbose;
        return engine;
    }

    [Test]
    public async Task PostProcessing_WritesTurnLog_And_UpdatesLastFive()
    {
        // Arrange
        var client = new Mock<IGenerationClient>();
        client.SetupAllProperties();
        client.Object.LastFiveInputOutputs = new List<(string, string, bool)>();

        // For commands that generate narration (empty input path), provide a stubbed result
        client
            .Setup(c => c.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
            .ReturnsAsync("Generated");

        var turnLogger = new Mock<ICloudWatchLogger<TurnLog>>();
        var engine = BuildEngine(client, turnLogger);

        // Act: issue several commands that should all go through PostProcessing
        var inputs = new[] { "look", "inventory", "wait", "east", "west", "look" };
        foreach (var input in inputs)
        {
            var response = await engine.GetResponse(input);
            response.Should().NotBeNullOrEmpty();
        }

        // Assert CloudWatch logger called with expected payload at least once
        turnLogger.Verify(l => l.WriteLogEvents(It.Is<TurnLog>(t =>
                !string.IsNullOrEmpty(t.SessionId) &&
                !string.IsNullOrEmpty(t.Location) &&
                t.Moves >= 0 &&
                !string.IsNullOrEmpty(t.Input) &&
                !string.IsNullOrEmpty(t.Response)
            )),
            Times.AtLeastOnce);

        // Assert LastFiveInputOutputs capped and contains latest entry
        client.Object.LastFiveInputOutputs.Should().NotBeNull();
        client.Object.LastFiveInputOutputs.Count.Should().BeLessThanOrEqualTo(5);
        var last = client.Object.LastFiveInputOutputs.Last();
        last.Item1.Should().Be("look"); // last input
        last.Item2.Should().NotBeNullOrEmpty(); // response
    }

    /// <summary>
    ///     Issue #578: every turn log carries the shape the parser produced and which branch of the
    ///     engine ended the turn, so a tool reading the logs can tell "the engine considered this and
    ///     declined" from "nothing ran" — and can name WHICH deflection fired — without inferring it
    ///     from the narration. This retires the fragile <c>noGeneratedResponses</c> sentinel that
    ///     issue #538 had to resort to.
    /// </summary>
    [TestFixture]
    public class TurnDiagnosticsTests
    {
        private Mock<IGenerationClient> _client = null!;
        private List<TurnLog> _logs = null!;
        private Mock<ICloudWatchLogger<TurnLog>> _turnLogger = null!;

        [SetUp]
        public void SetUp()
        {
            Repository.Reset();

            _client = new Mock<IGenerationClient>();
            _client.SetupAllProperties();
            _client.Object.LastFiveInputOutputs = [];
            _client
                .Setup(c => c.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
                .ReturnsAsync("Generated");

            _logs = [];
            _turnLogger = new Mock<ICloudWatchLogger<TurnLog>>();
            _turnLogger
                .Setup(l => l.WriteLogEvents(It.IsAny<TurnLog>()))
                .Callback<TurnLog>(_logs.Add)
                .Returns(Task.CompletedTask);
        }

        [TearDown]
        public void TearDown()
        {
            Repository.Reset();
        }

        /// <summary>An engine whose complex parse is pinned to exactly this intent.</summary>
        private GameEngine<ZorkI, ZorkIContext> EngineFor(IntentBase intent)
        {
            return BuildEngine(_client, _turnLogger, new StubParser(intent));
        }

        private TurnLog LastLog
        {
            get
            {
                _logs.Should().NotBeEmpty("the turn must have written a log");
                return _logs[^1];
            }
        }

        [Test]
        public async Task ParsedIntent_RecordsTheSimpleIntentShape()
        {
            // Issue #540's defect 3 — "press 3" reaching the engine with no noun tag — was found by
            // inference from outcome frequencies across a 1,352-command trace. This is what it looks
            // like when a single turn simply says so.
            var engine = EngineFor(new SimpleIntent { Verb = "press", Noun = null, OriginalInput = "press 3" });

            await engine.GetResponse("press 3");

            LastLog.ParsedIntent.Should().Be("SimpleIntent verb='press' noun='' adjective='' adverb=''");
        }

        [Test]
        public async Task ParsedIntent_RecordsTheMultiNounShape_IncludingThePreposition()
        {
            // Issue #540's defect 1: the parsed preposition was being rewritten to a hardcoded
            // "with", which no log could show while only the player-facing prose was recorded.
            var engine = EngineFor(new MultiNounIntent
            {
                Verb = "put",
                NounOne = "leaflet",
                NounTwo = "mailbox",
                Preposition = "with",
                OriginalInput = "put leaflet in mailbox"
            });

            await engine.GetResponse("put leaflet in mailbox");

            LastLog.ParsedIntent.Should()
                .Be("MultiNounIntent verb='put' nounOne='leaflet' nounTwo='mailbox' preposition='with'");
        }

        [Test]
        public async Task ParsedIntent_RecordsAGlobalCommand_WithoutTheAIParser()
        {
            var engine = BuildEngine(_client, _turnLogger);

            await engine.GetResponse("look");

            LastLog.ParsedIntent.Should().Be("GlobalCommandIntent command='LookProcessor'");
            LastLog.TerminalPath.Should().Be(TurnTerminalPath.Handled);
        }

        [Test]
        public async Task ParsedIntent_RecordsTheDirectionOfAMove()
        {
            var engine = BuildEngine(_client, _turnLogger);

            await engine.GetResponse("north");

            LastLog.ParsedIntent.Should().Be("MoveIntent direction='N' noun=''");
            LastLog.TerminalPath.Should().Be(TurnTerminalPath.Handled);
        }

        [Test]
        public async Task TerminalPath_IsHandled_WhenAHandlerRan()
        {
            var engine = EngineFor(new SimpleIntent { Verb = "open", Noun = "mailbox", OriginalInput = "open mailbox" });

            await engine.GetResponse("open mailbox");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.Handled);
        }

        [Test]
        public async Task TerminalPath_IsNullIntent_WhenTheParserProducedNoUsableShape()
        {
            var engine = EngineFor(new NullIntent());

            await engine.GetResponse("this game is stupid");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.NullIntent);
            LastLog.ParsedIntent.Should().Be("NullIntent");
        }

        [Test]
        public async Task TerminalPath_IsUnmatchedIntentType_WhenDispatchHasNoArmForTheIntent()
        {
            var engine = EngineFor(new UnroutedTestIntent());

            await engine.GetResponse("something the switch has never heard of");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.UnmatchedIntentType);
            LastLog.ParsedIntent.Should().Be("UnroutedTestIntent");
        }

        [Test]
        public async Task TerminalPath_IsNoMatchingVerbForNounInLocation_WhenTheVerbLandsOnSomethingHere()
        {
            var engine = EngineFor(new SimpleIntent { Verb = "kick", Noun = "mailbox", OriginalInput = "kick mailbox" });

            await engine.GetResponse("kick mailbox");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.NoMatchingVerbForNounInLocation);
        }

        [Test]
        public async Task TerminalPath_IsNoMatchingVerbForNounInInventory_WhenTheVerbLandsOnSomethingCarried()
        {
            var engine = EngineFor(new SimpleIntent { Verb = "kick", Noun = "sword", OriginalInput = "kick sword" });
            engine.Context.Take(Repository.GetItem<Sword>());

            await engine.GetResponse("kick sword");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.NoMatchingVerbForNounInInventory);
        }

        [Test]
        public async Task TerminalPath_IsNounNotPresent_WhenTheNounExistsElsewhereInTheStory()
        {
            var engine = EngineFor(new SimpleIntent { Verb = "examine", Noun = "sword", OriginalInput = "examine sword" });
            Repository.GetItem<Sword>(); // in the story, but not here and not carried

            await engine.GetResponse("examine sword");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.NounNotPresent);
        }

        [Test]
        public async Task TerminalPath_IsNounNotInTheStory_WhenNothingAnywhereAnswersToTheNoun()
        {
            var engine = EngineFor(new SimpleIntent { Verb = "examine", Noun = "unicorn", OriginalInput = "examine unicorn" });

            await engine.GetResponse("examine unicorn");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.NounNotInTheStory);
        }

        [Test]
        public async Task TerminalPath_IsMultiNounNeitherNounInTheStory_WhenBothNounsAreInvented()
        {
            var engine = EngineFor(MultiNoun("tie", "unicorn", "to", "tequila"));

            await engine.GetResponse("tie unicorn to tequila");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.MultiNounNeitherNounInTheStory);
        }

        [Test]
        public async Task TerminalPath_IsMultiNounFirstNounMissing_WhenOnlyTheSecondIsHere()
        {
            var engine = EngineFor(MultiNoun("tie", "sword", "to", "mailbox"));
            Repository.GetItem<Sword>();

            await engine.GetResponse("tie sword to mailbox");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.MultiNounFirstNounMissing);
        }

        [Test]
        public async Task TerminalPath_IsMultiNounSecondNounMissing_WhenOnlyTheFirstIsHere()
        {
            var engine = EngineFor(MultiNoun("tie", "mailbox", "to", "sword"));
            Repository.GetItem<Sword>();

            await engine.GetResponse("tie mailbox to sword");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.MultiNounSecondNounMissing);
        }

        [Test]
        public async Task TerminalPath_IsMultiNounBothNounsMissing_WhenNeitherIsHere()
        {
            var engine = EngineFor(MultiNoun("tie", "sword", "to", "lantern"));
            Repository.GetItem<Sword>();
            Repository.GetItem<Lantern>();

            await engine.GetResponse("tie sword to lantern");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.MultiNounBothNounsMissing);
        }

        [Test]
        public async Task TerminalPath_IsMultiNounNounIsSceneryOnly_WhenOneNounIsOnlyInTheRoomDescription()
        {
            // "field" is in West of House's prose but is not backed by an item, so no real
            // interaction between it and the mailbox is possible.
            var engine = EngineFor(MultiNoun("tie", "field", "to", "mailbox"));

            await engine.GetResponse("tie field to mailbox");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.MultiNounNounIsSceneryOnly);
        }

        [Test]
        public async Task TerminalPath_IsMultiNounNoProcessorMatched_WhenBothItemsAreRealAndPresent()
        {
            var engine = EngineFor(MultiNoun("tie", "sword", "to", "mailbox"));
            engine.Context.Take(Repository.GetItem<Sword>());

            await engine.GetResponse("tie sword to mailbox");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.MultiNounNoProcessorMatched);
        }

        [Test]
        public async Task TerminalPath_IsMultiNounFirstNounMissingSecondIsAPerson()
        {
            var engine = EngineFor(MultiNoun("give", "sword", "to", "bob"));
            Repository.GetItem<Sword>();
            PlaceNamedPersonHere();

            await engine.GetResponse("give sword to bob");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.MultiNounFirstNounMissingSecondIsAPerson);
        }

        [Test]
        public async Task TerminalPath_IsMultiNounSecondNounMissingFirstIsAPerson()
        {
            var engine = EngineFor(MultiNoun("tie", "bob", "to", "sword"));
            Repository.GetItem<Sword>();
            PlaceNamedPersonHere();

            await engine.GetResponse("tie bob to sword");

            LastLog.TerminalPath.Should().Be(TurnTerminalPath.MultiNounSecondNounMissingFirstIsAPerson);
        }

        [Test]
        public async Task TerminalPath_IsEngineError_OnEveryLogTheRescuedTurnWrites()
        {
            var engine = BuildEngine(_client, _turnLogger, new ThrowingParser());

            var response = await engine.GetResponse("anything at all");

            // The safety net still returns a normal turn (issue #271)...
            response.Should().NotBeNullOrWhiteSpace();
            // ...and both the explicit error log and the ordinary one agree on how the turn ended,
            // rather than one of them claiming a branch that never finished.
            _logs.Should().HaveCount(2);
            _logs.Should().OnlyContain(l => l.TerminalPath == TurnTerminalPath.EngineError);
        }

        [Test]
        public async Task EverySentenceOfAMultiSentenceCommand_IsClassifiedSeparately()
        {
            // The diagnostics reset per sentence, not per request. Without that reset the second
            // sentence's log would inherit the first's deflection and report a handled LOOK as a
            // no-handler turn.
            var engine = EngineFor(new SimpleIntent
            {
                Verb = "examine", Noun = "unicorn", OriginalInput = "examine unicorn"
            });

            await engine.GetResponse("examine unicorn. look");

            _logs.Should().HaveCount(2);

            _logs[0].ParsedIntent.Should().Be("SimpleIntent verb='examine' noun='unicorn' adjective='' adverb=''");
            _logs[0].TerminalPath.Should().Be(TurnTerminalPath.NounNotInTheStory);

            _logs[1].ParsedIntent.Should().Be("GlobalCommandIntent command='LookProcessor'");
            _logs[1].TerminalPath.Should().Be(TurnTerminalPath.Handled);
        }

        private static MultiNounIntent MultiNoun(string verb, string nounOne, string preposition, string nounTwo)
        {
            return new MultiNounIntent
            {
                Verb = verb,
                NounOne = nounOne,
                NounTwo = nounTwo,
                Preposition = preposition,
                OriginalInput = $"{verb} {nounOne} {preposition} {nounTwo}"
            };
        }

        private static void PlaceNamedPersonHere()
        {
            Repository.GetLocation<WestOfHouse>().ItemPlacedHere(Repository.GetItem<TestNamedPerson>());
        }

        /// <summary>Zork has no named people; the two person-specific deflections need one.</summary>
        private class TestNamedPerson : ItemBase, IAmANamedPerson
        {
            public override string[] NounsForMatching => ["bob"];

            public string ExaminationDescription => "Bob looks back at you.";

            public override string GenericDescription(ILocation? currentLocation) => "Bob is here.";
        }

        /// <summary>An intent type the engine's dispatch switch deliberately has no arm for.</summary>
        private record UnroutedTestIntent : IntentBase;

        /// <summary>
        ///     Pins the complex parse to one exact intent, so a test can aim at one branch. The cheap
        ///     global/system passes are left real, so a sentence like "look" still resolves the way it
        ///     does in play.
        /// </summary>
        private sealed class StubParser(IntentBase intent) : IIntentParser
        {
            private readonly IntentParser _staticPasses = new(Mock.Of<IAIParser>(), new ZorkOneGlobalCommandFactory());

            public Guid? TurnCorrelationId { get; set; }

            public ICloudWatchLogger<GenerationLog>? Logger { get; set; }

            public IntentBase? DetermineGlobalIntentType(string? input)
                => _staticPasses.DetermineGlobalIntentType(input);

            public IntentBase? DetermineSystemIntentType(string? input)
                => _staticPasses.DetermineSystemIntentType(input);

            public Task<IntentBase> DetermineComplexIntentType(string? input, string locationDescription,
                string sessionId) => Task.FromResult(intent);

            public Task<string?> ResolvePronounsAsync(string input, string? lastInput, string? lastResponse)
                => Task.FromResult<string?>(null);
        }

        /// <summary>Blows up mid-turn, to exercise the engine-error safety net's own turn log.</summary>
        private sealed class ThrowingParser : IIntentParser
        {
            public Guid? TurnCorrelationId { get; set; }

            public ICloudWatchLogger<GenerationLog>? Logger { get; set; }

            public IntentBase? DetermineGlobalIntentType(string? input) => null;

            public IntentBase? DetermineSystemIntentType(string? input) => null;

            public Task<IntentBase> DetermineComplexIntentType(string? input, string locationDescription,
                string sessionId) => throw new InvalidOperationException("boom");

            public Task<string?> ResolvePronounsAsync(string input, string? lastInput, string? lastResponse)
                => Task.FromResult<string?>(null);
        }
    }
}