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

        [TestCase("take the lamp", "take", "lamp")]
        [TestCase("drop the sword", "drop", "sword")]
        [TestCase("read the leaflet", "read", "leaflet")]
        public void TakeDropAndOtherVerbs_AreHandledDeterministically(string input, string verb, string noun)
        {
            // Standard grammar goes deterministic — no AI. (take/drop are no longer punted; "read" is a verb
            // the engine handles that wasn't in Verbs.cs and is now recognised.)
            var result = _parser.Parse(input);

            result.Should().BeOfType<SimpleIntent>();
            var simple = (SimpleIntent)result!;
            simple.Verb.Should().Be(verb);
            simple.Noun.Should().Be(noun);
        }

        [Test]
        public void MultiNounWithTool_IsHandledDeterministically()
        {
            // The case you called out: "kill troll with sword" is clean grammar, not AI fodder.
            var result = _parser.Parse("kill the troll with the sword");

            result.Should().BeOfType<MultiNounIntent>();
            var multi = (MultiNounIntent)result!;
            multi.Verb.Should().Be("kill");
            multi.NounOne.Should().Be("troll");
            multi.NounTwo.Should().Be("sword");
            multi.Preposition.Should().Be("with");
        }

        [TestCase("turn on the lantern", "activate")]
        [TestCase("turn off the lantern", "deactivate")]
        public void MultiWordVerbPhrases_AreCanonicalised(string input, string canonicalVerb)
        {
            var result = _parser.Parse(input);

            result.Should().BeOfType<SimpleIntent>();
            var simple = (SimpleIntent)result!;
            simple.Verb.Should().Be(canonicalVerb);
            simple.Noun.Should().Be("lantern");
        }

        [TestCase("take sword?")]
        [TestCase("open the mailbox.")]
        public void TrailingPunctuation_IsStripped(string input)
        {
            _parser.Parse(input).Should().BeOfType<SimpleIntent>();
        }
    }

    [TestFixture]
    public class Misses : DeterministicParserTests
    {
        [Test]
        public void UnknownNoun_FallsBackToAi()
        {
            _parser.Parse("examine flux capacitor").Should().BeNull();
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
