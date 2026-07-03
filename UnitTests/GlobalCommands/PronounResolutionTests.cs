using System.Text.RegularExpressions;
using GameEngine;
using ZorkOne.GlobalCommand;

namespace UnitTests.GlobalCommands;

/// <summary>
/// Comprehensive tests for pronoun resolution, specifically the LastInput fix
/// that stores the resolved input instead of the original input, enabling
/// chained pronoun resolution.
/// </summary>
public class PronounResolutionTests : EngineTestsBase
{
    [TestFixture]
    public class LastInputStorage : PronounResolutionTests
    {
        [Test]
        public async Task LastInput_ShouldStoreResolvedInput_AfterPronounResolution()
        {
            // This tests the key fix: LastInput should store "take lantern" not "take it"
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            // First command sets up context
            await target.GetResponse("look");

            // Set up the LastInput to simulate a prior interaction
            target.Context.LastInput = "examine lantern";
            target.Context.LastNoun = "lantern";

            // Now use "it" - should resolve to lantern
            await target.GetResponse("take it");

            // Verify LastInput now contains the resolved input, not "take it"
            target.Context.LastInput.Should().Contain("lantern",
                because: "LastInput should store resolved input for chained pronoun resolution");
            target.Context.LastInput.Should().NotBe("take it",
                because: "LastInput should NOT store the original pronoun-containing input");
        }

        [Test]
        public async Task LastInput_ShouldNotBeOverwritten_WhenNoResolutionNeeded()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            // Direct command without pronouns
            await target.GetResponse("take lantern");

            // LastInput should be the command itself
            target.Context.LastInput.Should().Be("take lantern");
        }

        [Test]
        public async Task LastInput_ShouldUpdate_AfterEachCommand()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            await target.GetResponse("take lantern");
            target.Context.LastInput.Should().Be("take lantern");

            await target.GetResponse("drop lantern");
            target.Context.LastInput.Should().Be("drop lantern");

            await target.GetResponse("examine sword");
            target.Context.LastInput.Should().Be("examine sword");
        }
    }

    [TestFixture]
    public class ChainedPronounResolution : PronounResolutionTests
    {
        [Test]
        public async Task ChainedIt_TakeIt_ThenDropIt_ShouldResolveCorrectly()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            // Step 1: Look at lantern
            await target.GetResponse("examine lantern");
            target.Context.LastNoun.Should().Be("lantern");

            // Step 2: Take it - should take the lantern
            var takeResult = await target.GetResponse("take it");
            takeResult.Should().Contain("Taken");
            target.Context.HasItem<Lantern>().Should().BeTrue();

            // Step 3: Drop it - should drop the lantern (chained resolution)
            // This tests that LastInput was stored as "take lantern" so drop can resolve "it"
            var dropResult = await target.GetResponse("drop it");
            dropResult.Should().Contain("Dropped");
            target.Context.HasItem<Lantern>().Should().BeFalse();
        }

        [Test]
        public async Task ChainedIt_MultipleItemInteractions_ShouldTrackCorrectItem()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            // Take lantern
            await target.GetResponse("take lantern");
            target.Context.HasItem<Lantern>().Should().BeTrue();

            // Take sword
            await target.GetResponse("take sword");
            target.Context.HasItem<Sword>().Should().BeTrue();

            // Now "drop it" should drop the sword (most recent item)
            // because LastInput is "take sword"
            var dropResult = await target.GetResponse("drop it");
            target.Context.HasItem<Sword>().Should().BeFalse();
            target.Context.HasItem<Lantern>().Should().BeTrue(
                because: "Lantern should not have been dropped - only sword was the 'it'");
        }

        [Test]
        public async Task ChainedIt_ExamineAfterTake_ShouldResolveToSameItem()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            // Take lantern
            await target.GetResponse("take lantern");
            target.Context.HasItem<Lantern>().Should().BeTrue();

            // Examine it - should examine the lantern
            var examineResult = await target.GetResponse("examine it");
            examineResult.Should().Contain("lamp",
                because: "Examining the lantern should describe it as a lamp");
        }
    }

    [TestFixture]
    public class BasicItResolution : PronounResolutionTests
    {
        [Test]
        public async Task It_WithLastNoun_ShouldResolve()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            await target.GetResponse("take lantern");
            var result = await target.GetResponse("drop it");

            target.Context.HasItem<Lantern>().Should().BeFalse();
            result.Should().Contain("Dropped");
        }

        [Test]
        public async Task It_WithoutLastNoun_ShouldAskForClarification()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
            target.Context.LastNoun = ""; // Explicitly clear

            var result = await target.GetResponse("take it");
            result.Should().Contain("What item are you referring to");
        }

        [Test]
        public async Task It_AfterClarification_ShouldWork()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
            target.Context.LastNoun = "";

            await target.GetResponse("take it");
            var result = await target.GetResponse("lantern");

            target.Context.HasItem<Lantern>().Should().BeTrue();
            result.Should().Contain("Taken");
        }
    }

    [TestFixture]
    public class ThemResolution : PronounResolutionTests
    {
        [Test]
        public async Task Them_WithPluralNoun_ShouldResolve()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<Altar>();

            await target.GetResponse("take candles");
            var result = await target.GetResponse("drop them");

            target.Context.HasItem<Candles>().Should().BeFalse();
            result.Should().Contain("Dropped");
        }

        [Test]
        public async Task Them_WithSingularNoun_ShouldAskForClarification()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            await target.GetResponse("take lantern");
            var result = await target.GetResponse("drop them");

            // Lantern is not plural, so should ask for clarification
            target.Context.HasItem<Lantern>().Should().BeTrue(
                because: "Lantern should NOT have been dropped since 'them' doesn't apply");
            result.Should().Contain("What item are you referring to");
        }
    }

    [TestFixture]
    public class EdgeCases : PronounResolutionTests
    {
        [Test]
        public async Task It_InMiddleOfSentence_ShouldResolve()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            // Set up context
            await target.GetResponse("examine lantern");

            // "turn it on" - 'it' is in the middle
            var result = await target.GetResponse("turn it on");

            // Should have tried to turn on the lantern
            var lantern = Repository.GetItem<Lantern>();
            lantern.IsOn.Should().BeTrue(because: "The lantern should be turned on");
        }

        [Test]
        public async Task EmptyLastInput_ShouldNotCrash()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
            target.Context.LastInput = "";
            target.Context.LastNoun = "";

            // Should not throw, but should ask for clarification
            var action = async () => await target.GetResponse("take it");
            await action.Should().NotThrowAsync();
        }

        [Test]
        public async Task NullLastInput_ShouldNotCrash()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
            target.Context.LastInput = null;
            target.Context.LastNoun = "";

            var action = async () => await target.GetResponse("take it");
            await action.Should().NotThrowAsync();
        }

        [Test]
        public async Task PronounInNonActionContext_ShouldNotResolve()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            // "look" doesn't use pronouns
            var result = await target.GetResponse("look");

            // Should just work normally
            result.Should().Contain("Living Room");
        }
    }

    [TestFixture]
    public class LastNounTracking : PronounResolutionTests
    {
        [Test]
        public async Task LastNoun_ShouldBeSetAfterSimpleInteraction()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            await target.GetResponse("take lantern");
            target.Context.LastNoun.Should().Be("lantern");
        }

        [Test]
        public async Task LastNoun_ShouldBeSetAfterExamine()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            await target.GetResponse("examine sword");
            target.Context.LastNoun.Should().Be("sword");
        }

        [Test]
        public async Task LastNoun_ShouldSurviveMove_WhenItemIsStillCarried()
        {
            // Issue #248: moving used to clear the antecedent unconditionally, even for an item
            // the player is still holding. "it" should keep referring to a carried item after a move.
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            await target.GetResponse("take lantern");
            target.Context.LastNoun.Should().Be("lantern");

            // Move east (to Kitchen) - this is a valid move
            await target.GetResponse("east");
            target.Context.LastNoun.Should().Be("lantern",
                because: "Movement keeps the antecedent for items the player is still carrying");
        }

        [Test]
        public async Task LastNoun_ShouldBeClearedAfterMove_WhenItemWasLeftBehind()
        {
            // Issue #248: the antecedent should only be forgotten for items left behind in the
            // previous room. The rug stays in the Living Room, so "it" no longer refers to it.
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            await target.GetResponse("examine rug");
            target.Context.LastNoun.Should().Be("rug");

            await target.GetResponse("east");
            target.Context.LastNoun.Should().BeEmpty(
                because: "An item left behind in the previous room is no longer the antecedent");
        }

        [Test]
        public async Task It_AfterMove_StillResolves_ForCarriedItem()
        {
            // Issue #248: "take lantern; examine it; <move>; drop it" should drop the lantern,
            // because it is still in the player's hand after walking to the next room.
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            await target.GetResponse("take lantern");
            await target.GetResponse("examine it");
            await target.GetResponse("east");

            var result = await target.GetResponse("drop it");

            result.Should().NotContain("What item are you referring to");
            result.Should().Contain("Dropped");
            target.Context.HasItem<Lantern>().Should().BeFalse();
        }
    }

    /// <summary>
    /// Issue #341 (AB-044): in production, after a multi-object "take all", the AI pronoun resolver's
    /// only context is the "take all" command and a response naming every item taken - so it conflates
    /// singular "it" with the whole object set, and "drop it" ends up dropping everything. Singular
    /// "it" must resolve to just the last object the multi-object action handled; "them" (issue #248)
    /// remains the collection pronoun.
    /// </summary>
    [TestFixture]
    public class TakeAllThenSingularIt : PronounResolutionTests
    {
        /// <summary>
        /// Mimics the production gpt-4o-mini pronoun resolver's failure mode: after "take all", it
        /// rewrites singular "it" into a conjunction of every item named in the previous response,
        /// exactly like its own "take them" -&gt; "take sword and shield" example would for a plural
        /// pronoun (see the resolver's prompt examples) - except here the player used singular "it".
        /// </summary>
        private sealed class ConflatesSingularItWithTakenSetParser()
            : TestParser(new ZorkOneGlobalCommandFactory())
        {
            public override Task<string?> ResolvePronounsAsync(string input, string? lastInput, string? lastResponse)
            {
                if (Regex.IsMatch(input, @"\bit\b", RegexOptions.IgnoreCase)
                    && (lastInput ?? string.Empty).Trim().Equals("take all", StringComparison.OrdinalIgnoreCase))
                    return Task.FromResult<string?>(
                        Regex.Replace(input, @"\bit\b", "rope and knife", RegexOptions.IgnoreCase));

                return base.ResolvePronounsAsync(input, lastInput, lastResponse);
            }
        }

        [Test]
        public async Task DropIt_AfterTakeAll_DropsOnlyTheLastItemTaken()
        {
            var parser = new ConflatesSingularItWithTakenSetParser();
            var target = GetTarget(parser);
            // The Attic is a DarkLocation; a lit lamp is needed to see well enough to drop things there.
            var lantern = GetItem<Lantern>();
            lantern.IsOn = true;
            target.Context.ItemPlacedHere(lantern);
            target.Context.CurrentLocation = Repository.GetLocation<Attic>();

            await target.GetResponse("take all");
            target.Context.HasItem<Rope>().Should().BeTrue();
            target.Context.HasItem<NastyKnife>().Should().BeTrue();

            var result = await target.GetResponse("drop it");

            target.Context.HasItem<NastyKnife>().Should().BeFalse(
                because: "singular 'it' should resolve to the last item handled by 'take all'");
            target.Context.HasItem<Rope>().Should().BeTrue(
                because: "'it' is singular and must not drop the whole object set taken by 'take all'");
            result.Should().Contain("Dropped");
        }

        [Test]
        public async Task DropThem_AfterTakeAll_StillDropsTheWholeSet()
        {
            // Guards against over-correcting: plural "them" (issue #248) must still resolve to
            // everything "take all" picked up.
            var target = GetTarget();
            var lantern = GetItem<Lantern>();
            lantern.IsOn = true;
            target.Context.ItemPlacedHere(lantern);
            target.Context.CurrentLocation = Repository.GetLocation<Attic>();

            await target.GetResponse("take all");
            await target.GetResponse("drop them");

            target.Context.HasItem<Rope>().Should().BeFalse();
            target.Context.HasItem<NastyKnife>().Should().BeFalse();
        }
    }
}
