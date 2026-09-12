using GameEngine;
using GameEngine.Item;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Admin;
using ZorkOne.Item;

namespace UnitTests;

/// <summary>
/// Direct tests for the noun predicates extracted from TakeOrDropInteractionProcessor (issue #502).
/// While they were private statics in the processor, every case here cost a whole game context plus a
/// stubbed AI parser to reach; the take/drop tests still cover them end-to-end, but the edge cases
/// that actually make these functions tricky - word-boundary padding, the container override, both
/// directions of adjective - belong in rows like these.
/// </summary>
public class NounMatchTests
{
    [SetUp]
    public void SetUp()
    {
        Repository.Reset();
    }

    [TestFixture]
    public class CouldNameTests : NounMatchTests
    {
        [TestCase("lantern", TestName = "CouldName_ExactNoun")]
        [TestCase("lamp", TestName = "CouldName_Synonym")]
        [TestCase("brass lantern", TestName = "CouldName_ParserAddedAnAdjective")]
        [TestCase("LANTERN", TestName = "CouldName_IsCaseInsensitive")]
        [TestCase("  lantern  ", TestName = "CouldName_IgnoresSurroundingWhitespace")]
        public void CouldName_MatchesTheLantern(string noun)
        {
            NounMatch.CouldName(Repository.GetItem<Lantern>(), noun).Should().BeTrue();
        }

        [TestCase("xyzzy", TestName = "CouldName_RejectsNonsense")]
        [TestCase("medicine", TestName = "CouldName_RejectsAnUnrelatedRealNoun")]
        [TestCase("", TestName = "CouldName_RejectsEmpty")]
        [TestCase("   ", TestName = "CouldName_RejectsWhitespace")]
        [TestCase(null, TestName = "CouldName_RejectsNull")]
        public void CouldName_DoesNotMatchTheLantern(string? noun)
        {
            NounMatch.CouldName(Repository.GetItem<Lantern>(), noun).Should().BeFalse();
        }

        [Test]
        public void CouldName_MatchesAContainerItemTheOverrideWouldReject()
        {
            // The reason this helper exists instead of a call to HasMatchingNoun: ContainerBase and
            // OpenAndCloseContainerBase override that with plain exact equality, so a BrownSack - whose
            // nouns are "sack"/"brown sack"/"bag"/"elongated brown sack" - would reject "elongated
            // sack" outright. Proven by asserting the override really does say no.
            var sack = Repository.GetItem<BrownSack>();

            sack.HasMatchingNoun("elongated sack", lookInsideContainers: false).HasItem.Should()
                .BeFalse("this is the container override the helper deliberately bypasses");

            NounMatch.CouldName(sack, "elongated sack").Should().BeTrue();
        }

        [Test]
        public void CouldName_LooksOnlyAtTheItemsOwnNouns_NotItsContents()
        {
            // A bag that happens to hold the named object is not itself the named object - this is
            // exactly the reported "take medicine" case, where the medicine sat inside a held bottle.
            var sack = Repository.GetItem<BrownSack>();
            sack.IsOpen = true;
            sack.ItemPlacedHere(Repository.GetItem<Lunch>());

            sack.HasMatchingNoun("lunch").HasItem.Should()
                .BeTrue("HasMatchingNoun searches inside, which is why it is the wrong tool here");

            NounMatch.CouldName(sack, "lunch").Should().BeFalse();
        }

        [Test]
        public void CouldName_MatchesWhenTheItemCarriesTheExtraAdjective()
        {
            // The reverse direction from "brass lantern": here the item's own noun is the longer
            // phrase and the player typed the bare one.
            NounMatch.CouldName(Repository.GetItem<BrownSack>(), "sack").Should().BeTrue();
        }

        [Test]
        public void CouldName_DoesNotMatchOnAWordFragment()
        {
            // The padding exists so "key" cannot match inside "monkey".
            NounMatch.CouldName(new FakeItem("monkey"), "key").Should().BeFalse();
        }

        private class FakeItem(params string[] nouns) : ItemBase
        {
            public override string[] NounsForMatching => nouns;
        }
    }

    [TestFixture]
    public class NamesASingleObjectTests : NounMatchTests
    {
        [TestCase("lantern", "take the lantern", TestName = "NamesASingleObject_PlainTake")]
        [TestCase("sack", "drop sack", TestName = "NamesASingleObject_PlainDrop")]
        [TestCase("ball", "take the ball", TestName = "NamesASingleObject_BallDoesNotTripOnAll")]
        [TestCase("button", "press the button", TestName = "NamesASingleObject_ButtonDoesNotTripOnBut")]
        [TestCase("bothy", "take the bothy", TestName = "NamesASingleObject_BothyDoesNotTripOnBoth")]
        [TestCase("candle", "take candle", TestName = "NamesASingleObject_NoOriginalInputMarkers")]
        public void NamesASingleObject_IsTrue(string noun, string originalInput)
        {
            NounMatch.NamesASingleObject(noun, originalInput).Should().BeTrue();
        }

        [TestCase("all", "take all the tools", TestName = "NamesASingleObject_CollectiveAll")]
        [TestCase("everything", "pick up everything", TestName = "NamesASingleObject_CollectiveEverything")]
        [TestCase("both", "grab both", TestName = "NamesASingleObject_CollectiveBoth")]
        [TestCase("them", "take them", TestName = "NamesASingleObject_CollectiveThem")]
        [TestCase("EVERYTHING", "pick up EVERYTHING", TestName = "NamesASingleObject_CollectiveIsCaseInsensitive")]
        public void NamesASingleObject_IsFalseForACollectiveNoun(string noun, string originalInput)
        {
            NounMatch.NamesASingleObject(noun, originalInput).Should().BeFalse();
        }

        [TestCase("sword", "take the sword and the lantern", TestName = "NamesASingleObject_Conjunction")]
        [TestCase("sword", "take sword, lantern", TestName = "NamesASingleObject_Comma")]
        [TestCase("sword", "take sword & lantern", TestName = "NamesASingleObject_Ampersand")]
        [TestCase("tube", "pick up everything except the tube", TestName = "NamesASingleObject_ExcludedNoun")]
        [TestCase("tube", "take all the tools but don't take the tube", TestName = "NamesASingleObject_ButExclusion")]
        [TestCase("tube", "take the tools besides the tube", TestName = "NamesASingleObject_Besides")]
        public void NamesASingleObject_IsFalseForAMultiObjectRequest(string noun, string originalInput)
        {
            // TakeIntent.Noun is only nouns.FirstOrDefault(), so on these phrasings it is either the
            // first of several nouns or - worse - the noun the player explicitly excluded.
            NounMatch.NamesASingleObject(noun, originalInput).Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void NamesASingleObject_IsFalseWithoutANoun(string? noun)
        {
            NounMatch.NamesASingleObject(noun, "take something").Should().BeFalse();
        }

        [Test]
        public void NamesASingleObject_ToleratesANullOriginalInput()
        {
            NounMatch.NamesASingleObject("lantern", null).Should().BeTrue();
        }
    }

    [TestFixture]
    public class PreciselyNamesTests : NounMatchTests
    {
        [Test]
        public void PreciselyNames_RejectsASuffixWordCoincidence()
        {
            // Issue #536: the ID card's bare noun "card" is a trailing word of "lower card", so the
            // looser CouldName says yes - which is exactly how "take lower card" in an empty room
            // dragged the ID card out of a pocket. PreciselyNames must say no: "lower card" is not one
            // of the ID card's own precise nouns.
            var idCard = Repository.GetItem<IdCard>();

            NounMatch.CouldName(idCard, "lower card").Should()
                .BeTrue("this is the loose match PreciselyNames is meant to be stricter than");
            NounMatch.PreciselyNames(idCard, "lower card").Should().BeFalse();
        }

        [Test]
        public void PreciselyNames_AcceptsTheCardThatActuallyRegistersThatPhrase()
        {
            // The port distinguishes the two cards in exactly this vocabulary: the lower elevator access
            // card registers "lower card" as one of its precise nouns, the ID card does not.
            NounMatch.PreciselyNames(Repository.GetItem<LowerElevatorAccessCard>(), "lower card").Should().BeTrue();
        }

        [TestCase("lantern", TestName = "PreciselyNames_ExactBareNoun")]
        [TestCase("brass lantern", TestName = "PreciselyNames_ExactAdjectivePhrase")]
        [TestCase("LANTERN", TestName = "PreciselyNames_IsCaseInsensitive")]
        [TestCase("  brass lantern  ", TestName = "PreciselyNames_IgnoresSurroundingWhitespace")]
        public void PreciselyNames_AcceptsTheItemsOwnNouns(string noun)
        {
            NounMatch.PreciselyNames(Repository.GetItem<Lantern>(), noun).Should().BeTrue();
        }

        [TestCase("lower lantern", TestName = "PreciselyNames_RejectsAnAdjectiveTheItemDoesNotRegister")]
        [TestCase("xyzzy", TestName = "PreciselyNames_RejectsNonsense")]
        [TestCase("", TestName = "PreciselyNames_RejectsEmpty")]
        [TestCase("   ", TestName = "PreciselyNames_RejectsWhitespace")]
        [TestCase(null, TestName = "PreciselyNames_RejectsNull")]
        public void PreciselyNames_RejectsWhatIsNotOneOfItsNouns(string? noun)
        {
            NounMatch.PreciselyNames(Repository.GetItem<Lantern>(), noun).Should().BeFalse();
        }
    }
}
