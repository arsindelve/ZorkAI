using GameEngine;
using Model.AIParsing;
using Model.Intent;
using ZorkOne.GlobalCommand;

namespace UnitTests;

/// <summary>
/// Deterministic tests for the Layer-1 <see cref="DeterministicParser" /> (no AI/network). They prove the
/// parser resolves the common clean command shapes against the real ZorkOne vocabulary, AND — just as
/// important — that it stays SILENT (returns null -> AI fallback) for anything ambiguous or multi-noun, so
/// it can never regress those commands.
/// </summary>
[TestFixture]
public class DeterministicParserTests
{
    private DeterministicParser _parser = null!;

    [SetUp]
    public void Setup()
    {
        Repository.Reset();
        _parser = new DeterministicParser("ZorkOne");
    }

    [TestFixture]
    public class Hits : DeterministicParserTests
    {
        [Test]
        public void SimpleVerbNoun_BecomesSimpleIntent()
        {
            var result = _parser.Parse("examine mailbox");

            result.Should().BeOfType<SimpleIntent>();
            var simple = (SimpleIntent)result!;
            simple.Verb.Should().Be("examine");
            simple.Noun.Should().Be("mailbox");
        }

        [Test]
        public void StripsArticles()
        {
            var result = _parser.Parse("open the mailbox");

            result.Should().BeOfType<SimpleIntent>();
            ((SimpleIntent)result!).Noun.Should().Be("mailbox");
        }

        [Test]
        public void PutItemInContainer_BecomesMultiNounIntent()
        {
            var result = _parser.Parse("put the sword in the trophy case");

            result.Should().BeOfType<MultiNounIntent>();
            var multi = (MultiNounIntent)result!;
            multi.Verb.Should().Be("put");
            multi.NounOne.Should().Be("sword");
            multi.NounTwo.Should().Be("trophy case");
            multi.Preposition.Should().Be("in");
        }

        [Test]
        public void LookUnderObject_KeepsThePrepositionAsAdverb()
        {
            var result = _parser.Parse("look under the rug");

            result.Should().BeOfType<SimpleIntent>();
            var simple = (SimpleIntent)result!;
            simple.Verb.Should().Be("look");
            simple.Noun.Should().Be("rug");
            simple.Adverb.Should().Be("under");
        }

        [TestCase("go to the kitchen", "kitchen")]
        [TestCase("walk to the cellar", "cellar")]
        [TestCase("head to the kitchen", "kitchen")]
        [TestCase("travel to the attic", "attic")]
        [TestCase("go into the cellar", "cellar")]
        [TestCase("walk into the kitchen", "kitchen")]
        public void TravelPrefixes_BecomeGoToDestinationIntent(string input, string expectedDestination)
        {
            var result = _parser.Parse(input);

            result.Should().BeOfType<GoToDestinationIntent>();
            ((GoToDestinationIntent)result!).Destination.Should().Be(expectedDestination);
        }

        [TestCase("put the sword into the trophy case")]
        [TestCase("put sword inside the trophy case")]
        public void PutItemInContainer_AcceptsIntoAndInside(string input)
        {
            var result = _parser.Parse(input);

            result.Should().BeOfType<MultiNounIntent>();
            var multi = (MultiNounIntent)result!;
            multi.NounOne.Should().Be("sword");
            multi.NounTwo.Should().Be("trophy case");
        }

        [Test]
        public void LookThroughKnownObject_KeepsThroughAsAdverb()
        {
            var result = _parser.Parse("look through the window");

            result.Should().BeOfType<SimpleIntent>();
            var simple = (SimpleIntent)result!;
            simple.Verb.Should().Be("look");
            simple.Noun.Should().Be("window");
            simple.Adverb.Should().Be("through");
        }

        [Test]
        public void IsCaseInsensitive()
        {
            var result = _parser.Parse("OPEN THE MAILBOX");

            result.Should().BeOfType<SimpleIntent>();
            ((SimpleIntent)result!).Noun.Should().Be("mailbox");
        }

        [Test]
        public void MultiWordNoun_IsResolved()
        {
            var result = _parser.Parse("examine the trophy case");

            result.Should().BeOfType<SimpleIntent>();
            ((SimpleIntent)result!).Noun.Should().Be("trophy case");
        }
    }

    [TestFixture]
    public class Misses : DeterministicParserTests
    {
        [Test]
        public void TakeIsLeftToTheMultiItemAi()
        {
            // Take/drop deliberately fall through so the multi-item take/drop AI still runs.
            _parser.Parse("take lamp").Should().BeNull();
            _parser.Parse("drop sword").Should().BeNull();
        }

        [Test]
        public void VerbWithToolAndSecondNoun_FallsBackToAi()
        {
            // "attack troll with sword" must NOT become a single-noun act — the exact-noun rule means the
            // remainder "troll with sword" isn't a known noun, so it falls back to the multi-noun AI.
            _parser.Parse("attack troll with sword").Should().BeNull();
            _parser.Parse("unlock the grating with the key").Should().BeNull();
        }

        [Test]
        public void UnknownNoun_FallsBackToAi()
        {
            _parser.Parse("examine flux capacitor").Should().BeNull();
        }

        [Test]
        public void MultiWordVerbPhrase_FallsBackToAi()
        {
            // "turn on lamp" -> the remainder "on lamp" is not a known noun, so we don't guess; the AI
            // (which canonicalises "turn on" -> activate) handles it.
            _parser.Parse("turn on lamp").Should().BeNull();
        }

        [Test]
        public void Gibberish_FallsBackToAi()
        {
            _parser.Parse("xyzzy plugh").Should().BeNull();
            _parser.Parse("").Should().BeNull();
            _parser.Parse("   ").Should().BeNull();
        }

        [TestCase("go through the window")]
        [TestCase("climb through the window")]
        [TestCase("walk through the window")]
        public void MovementThroughAnOpening_FallsBackToAi(string input)
        {
            // The #3 fix: "go/climb/walk through <known object>" is usually a request to MOVE through it
            // (the AI resolves it to a MoveIntent), so the deterministic parser must NOT turn it into a
            // SimpleIntent. "window" IS a known noun, so the ONLY reason these return null is the movement
            // verb being excluded from the through-rule (cf. LookThroughKnownObject_KeepsThroughAsAdverb).
            _parser.Parse(input).Should().BeNull();
        }

        [Test]
        public void PutOnSomething_IsNotTreatedAsPutIn()
        {
            // "put on the X" (wear) must not be mis-read as "put X in a container"; it falls back to the AI
            // (which canonicalises to "don").
            _parser.Parse("put on the trophy case").Should().BeNull();
        }

        [Test]
        public void PutInUnknownContainer_FallsBackToAi()
        {
            _parser.Parse("put the sword in the forest").Should().BeNull();
        }

        [TestCase("examine the mailbox carefully")]
        [TestCase("read the leaflet slowly")]
        public void VerbWithTrailingWords_FallsBackToAi(string input)
        {
            // The exact-noun rule: the whole post-verb phrase must be a known noun. Trailing adverbs/words
            // ("... carefully") mean the remainder isn't an exact noun, so we don't guess.
            _parser.Parse(input).Should().BeNull();
        }

        [Test]
        public void SingleWord_FallsBackToAi()
        {
            _parser.Parse("mailbox").Should().BeNull();
        }
    }
}

/// <summary>
/// Verifies the deterministic-first WIRING in <see cref="IntentParser" />: a deterministic hit skips the AI
/// entirely; a miss falls back to the AI parser.
/// </summary>
[TestFixture]
public class IntentParserDeterministicWiringTests
{
    [SetUp]
    public void Setup() => Repository.Reset();

    [Test]
    public async Task DeterministicHit_DoesNotCallTheAiParser()
    {
        var aiParser = new Mock<IAIParser>();
        aiParser.Setup(p => p.AskTheAIParser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new NullIntent()); // sentinel: if this is returned, the AI was (wrongly) used

        var target = new IntentParser(aiParser.Object, new ZorkOneGlobalCommandFactory(), "ZorkOne");

        var result = await target.DetermineComplexIntentType("examine mailbox", "somewhere", "session");

        result.Should().BeOfType<SimpleIntent>();
        aiParser.Verify(p => p.AskTheAIParser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task DeterministicMiss_FallsBackToTheAiParser()
    {
        var aiIntent = new SimpleIntent { Verb = "pray", Noun = "altar", OriginalInput = "pray at the altar" };
        var aiParser = new Mock<IAIParser>();
        aiParser.Setup(p => p.AskTheAIParser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(aiIntent);

        var target = new IntentParser(aiParser.Object, new ZorkOneGlobalCommandFactory(), "ZorkOne");

        var result = await target.DetermineComplexIntentType("pray at the altar", "somewhere", "session");

        result.Should().BeSameAs(aiIntent);
        aiParser.Verify(p => p.AskTheAIParser("pray at the altar", "somewhere", "session"), Times.Once);
    }
}
