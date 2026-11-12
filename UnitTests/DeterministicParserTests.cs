using GameEngine;
using Model.AIParsing;
using Model.Intent;
using Model.Movement;
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

        [TestCase("walk to the north", Direction.N)]
        [TestCase("go to the south", Direction.S)]
        public void TravelPrefixWithDirection_BecomesMoveIntent(string input, Direction expectedDirection)
        {
            var result = _parser.Parse(input);

            result.Should().BeOfType<MoveIntent>();
            ((MoveIntent)result!).Direction.Should().Be(expectedDirection);
        }

        [TestCase("enter boat")]
        [TestCase("board boat")]
        public void EnterOrBoardKnownNoun_BecomesEnterSubLocationIntent(string input)
        {
            var result = _parser.Parse(input);

            result.Should().BeOfType<EnterSubLocationIntent>();
            ((EnterSubLocationIntent)result!).Noun.Should().Be("boat");
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

        [Test]
        public void LocationDeclaredSceneryNoun_ResolvesDeterministically()
        {
            // "bolt" backs no item — it is scenery declared on the Dam location (Dam.SceneryNouns) and
            // harvested into the vocabulary by Repository.GetNouns. Proving this parses is proof the engine
            // needs NO hardcoded per-game scenery list: the noun is understood purely because its location
            // declares it. (Before this refactor, "bolt" only worked via a hardcoded list inside the engine.)
            var result = _parser.Parse("turn the bolt with the wrench");

            result.Should().BeOfType<MultiNounIntent>();
            var multi = (MultiNounIntent)result!;
            multi.NounOne.Should().Be("bolt");
            multi.NounTwo.Should().Be("wrench");
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
        public void TravelPrefixWithUnknownDestination_FallsBackToAi()
        {
            _parser.Parse("walk to the flux capacitor").Should().BeNull();
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
            // The second noun must be genuinely outside the game's vocabulary. "forest" is NOT usable here:
            // the Forest locations declare it, so it is a known noun the parser rightly resolves. Use a word
            // the game never mentions so this still exercises the unknown-container -> AI fallback.
            _parser.Parse("put the sword in the volcano").Should().BeNull();
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

/// <summary>
/// The location-scenery harvest is game-agnostic: the deterministic parser resolves Planetfall's
/// location-declared scenery nouns too, with no ZorkOne special-casing. Planetfall has no deterministic
/// walkthrough harness, so these stand in as the end-to-end proof that the newly-declared scenery nouns
/// actually resolve (parser vocabulary -> intent), not just that they are harvested.
/// </summary>
[TestFixture]
public class DeterministicParserPlanetfallTests
{
    private DeterministicParser _parser = null!;

    [SetUp]
    public void Setup()
    {
        Repository.Reset();
        _parser = new DeterministicParser("Planetfall");
    }

    [TestCase("examine the rift", "examine", "rift")]          // RiftLocationBase.SceneryNouns
    [TestCase("examine the crevice", "examine", "crevice")]    // AdminCorridorSouth.SceneryNouns
    [TestCase("examine the window", "examine", "window")]      // BioLockEast.SceneryNouns (#423)
    [TestCase("push the green button", "push", "green button")] // MachineShop.SceneryNouns (compound)
    public void LocationDeclaredSceneryNoun_ResolvesToASimpleIntent(string input, string verb, string noun)
    {
        var result = _parser.Parse(input);

        result.Should().BeOfType<SimpleIntent>();
        var simple = (SimpleIntent)result!;
        simple.Verb.Should().Be(verb);
        simple.Noun.Should().Be(noun);
    }

    [TestCase("pull the lever", "pull", "lever")]            // ShuttleControl.DecelerateVerbs
    [TestCase("collapse the ladder", "collapse", "ladder")]  // Ladder.RetractVerbs
    [TestCase("deactivate floyd", "deactivate", "floyd")]    // TurnOnOrOffProcessor.TurnOffVerbs
    public void AVerbAHandlerDeclares_IsKnown_WithoutBeingListedInTheParser(string input, string verb, string noun)
    {
        // None of these verbs is in Verbs.cs, and the parser holds no verb list of its own. They are known
        // because the object that answers them declares a VerbFamily, which Repository.GetVerbs harvests.
        var result = _parser.Parse(input);

        result.Should().BeOfType<SimpleIntent>();
        var simple = (SimpleIntent)result!;
        simple.Verb.Should().Be(verb);
        simple.Noun.Should().Be(noun);
    }

    [TestCase("examine metal bar, curved into a u-shape")]
    [TestCase("examine the metal bar, curved into a u-shape")]
    public void ANounThatContainsAPrepositionAndAnArticle_IsStillOneNoun(string input)
    {
        // The magnet is declared (as a puzzle) as "metal bar, curved into a u-shape". "into" is a word the
        // grammar splits two nouns on and "a" is an article it strips, so the phrase used to be cut in half
        // and declined. The whole phrase is tried as a noun first, and the noun the handler will match on
        // comes back exactly as the item declares it.
        var result = _parser.Parse(input);

        result.Should().BeOfType<SimpleIntent>();
        ((SimpleIntent)result!).Noun.Should().Be("metal bar, curved into a u-shape");
    }

    [Test]
    public void ANounThatContainsAPreposition_CanStillBeOneOfTwoNouns()
    {
        var result = _parser.Parse("give the metal bar, curved into a u-shape to floyd");

        result.Should().BeOfType<MultiNounIntent>();
        var multi = (MultiNounIntent)result!;
        multi.NounOne.Should().Be("metal bar, curved into a u-shape");
        multi.Preposition.Should().Be("to");
        multi.NounTwo.Should().Be("floyd");
    }

    [TestCase("exit the blue door")]
    [TestCase("leave the booth")]
    [TestCase("exit web")]
    public void LeavingSomething_FallsBackToAi_EvenThoughAnObjectDeclaresTheVerb(string input)
    {
        // The safety webbing answers "leave"/"exit", so both are in Planetfall's harvested vocabulary. But
        // leaving a thing is the disembark intent, which only the AI path builds (with its door and
        // sub-location handling). A SimpleIntent here would reach nothing for a door or a booth.
        _parser.Parse(input).Should().BeNull();
    }

    [TestCase("yank the lever")]      // "yank" is the admin corridor's verb for the key; the lever answers "pull"
    [TestCase("fetch the canteen")]   // likewise "fetch": to the canteen it is a take, which only the AI can say
    [TestCase("nab the pliers")]
    [TestCase("collapse the canteen")] // the ladder's verb, on something that is not the ladder
    public void AVerbOnlyOneObjectDeclares_IsNotTrustedOnAnotherObject(string input)
    {
        // Harvesting makes every declared verb KNOWN, but known is not the same as answered. A rare synonym
        // one object lists ("yank", "fetch", "nab") reaches nothing when aimed at any other object - where
        // the AI would have reduced it to the common verb that object does answer. So the grammar resolves
        // an object's own verbs only on that object's own nouns, and leaves the rest to the AI.
        _parser.Parse(input).Should().BeNull();
    }

    [TestCase("examine the lever", "examine")]   // Verbs.ExamineVerbs
    [TestCase("push the canteen", "push")]       // Verbs.PushVerbs
    [TestCase("smell the pliers", "smell")]      // SmellInteractionProcessor
    public void AVerbTheEngineSharesAcrossObjects_IsTrustedOnAnyNoun(string input, string verb)
    {
        _parser.Parse(input).Should().BeOfType<SimpleIntent>().Which.Verb.Should().Be(verb);
    }

    [Test]
    public void AVerbOnlyAnotherGameDeclares_FallsBackToAi()
    {
        // "rearrange" is the ZorkOne rug's verb. Each game's vocabulary is its own.
        _parser.Parse("rearrange the ladder").Should().BeNull();
        new DeterministicParser("ZorkOne").Parse("rearrange the rug").Should().BeOfType<SimpleIntent>();
        new DeterministicParser("ZorkOne").Parse("rearrange the lamp").Should().BeNull("only the rug answers it");
    }

    [Test]
    public void TravelPrefix_WithAWholeWordOfARoomName_BecomesGoToDestinationIntent()
    {
        // DestinationNavigation lets the player use any whole word of a room's name ("go to the office"
        // -> "Lab Office"), so the parser's known-destination gate must accept the same terms. "office"
        // is deliberately a word that names rooms but no object, so the noun vocabulary cannot vouch for it.
        var result = _parser.Parse("go to the office");

        result.Should().BeOfType<GoToDestinationIntent>();
        ((GoToDestinationIntent)result!).Destination.Should().Be("office");
    }

    [Test]
    public void TravelPrefix_WithAFragmentOfARoomName_FallsBackToAi()
    {
        _parser.Parse("go to the offi").Should().BeNull();
    }
}

[TestFixture]
public class DeterministicParserStationfallTests
{
    [SetUp]
    public void Setup()
    {
        Repository.Reset();
    }

    [Test]
    public void GameWideScenery_ResolvesToASimpleIntent()
    {
        // "ceiling" is declared only in StationfallGame.GlobalScenery - on the game, not on any item or
        // location - so a vocabulary built from GetNouns alone never sees it.
        var result = new DeterministicParser("Stationfall").Parse("examine the ceiling");

        result.Should().BeOfType<SimpleIntent>();
        var simple = (SimpleIntent)result!;
        simple.Verb.Should().Be("examine");
        simple.Noun.Should().Be("ceiling");
    }
}
