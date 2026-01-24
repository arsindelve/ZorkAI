using GameEngine;

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
        public async Task LastNoun_ShouldBeClearedAfterSuccessfulMove()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

            await target.GetResponse("take lantern");
            target.Context.LastNoun.Should().Be("lantern");

            // Move east (to Kitchen) - this is a valid move
            await target.GetResponse("east");
            target.Context.LastNoun.Should().BeEmpty(
                because: "Successful movement should clear LastNoun");
        }
    }
}
