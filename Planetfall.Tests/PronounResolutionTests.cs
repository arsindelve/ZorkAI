using FluentAssertions;
using Planetfall.Item.Lawanda.LabOffice;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Tests;

/// <summary>
/// Tests for pronoun resolution in Planetfall, specifically the gas mask scenario
/// that exposed the bug where LastInput stored the original "take it" instead of
/// the resolved "take gas mask", breaking chained pronoun resolution.
///
/// The key fix: GameEngine now stores the RESOLVED input in LastInput (e.g., "take gas mask")
/// instead of the original input (e.g., "take it"), enabling chained pronoun resolution.
/// </summary>
public class PronounResolutionTests : EngineTestsBase
{
    [TestFixture]
    public class LastInputFix : PronounResolutionTests
    {
        [Test]
        public async Task LastInput_ShouldBeSetAfterCommand()
        {
            // Basic verification that LastInput is set
            var target = GetTarget();
            StartHere<LabOffice>();

            await target.GetResponse("look");

            // LastInput should be set to the command
            Context.LastInput.Should().Be("look");
        }

        [Test]
        public async Task LastInput_ShouldStoreDirectCommand()
        {
            // Verify LastInput stores direct commands correctly
            var target = GetTarget();
            StartHere<LabOffice>();

            await target.GetResponse("wear gas mask");

            // Even if the action fails, LastInput should be set
            Context.LastInput.Should().Be("wear gas mask");
        }

        [Test]
        public async Task WearGasMaskDirectly_ShouldWork()
        {
            // Direct command without any pronouns - verify basic wearing works
            var target = GetTarget();
            StartHere<LabOffice>();

            var gasMask = GetItem<GasMask>();
            // Take the gas mask into inventory directly
            Context.ItemPlacedHere(gasMask);

            // Now wear it
            await target.GetResponse("wear gas mask");

            gasMask.BeingWorn.Should().BeTrue(
                because: "Gas mask should be worn after 'wear gas mask'");
        }

        [Test]
        public async Task ChainedPronoun_WearIt_AfterHavingMask_ShouldWork()
        {
            // Test the chained pronoun resolution: LastInput contains "gas mask"
            // so "wear it" should resolve to "wear gas mask"
            var target = GetTarget();
            StartHere<LabOffice>();

            var gasMask = GetItem<GasMask>();
            Context.ItemPlacedHere(gasMask);

            // Set LastInput to simulate previous command containing "gas mask"
            Context.LastInput = "take gas mask";
            Context.LastNoun = "gas mask";

            // Now "wear it" should resolve via chained pronoun resolution
            await target.GetResponse("wear it");

            gasMask.BeingWorn.Should().BeTrue(
                because: "Chained pronoun resolution should resolve 'wear it' to 'wear gas mask'");
        }

        [Test]
        public async Task ChainedPronoun_PutItOn_AfterHavingMask_ShouldWork()
        {
            // Test "put it on" variant
            var target = GetTarget();
            StartHere<LabOffice>();

            var gasMask = GetItem<GasMask>();
            Context.ItemPlacedHere(gasMask);

            // Set LastInput to simulate previous command containing "gas mask"
            Context.LastInput = "take gas mask";
            Context.LastNoun = "gas mask";

            // Now "put it on" should resolve via chained pronoun resolution
            await target.GetResponse("put it on");

            gasMask.BeingWorn.Should().BeTrue(
                because: "Chained pronoun resolution should resolve 'put it on' to 'wear gas mask'");
        }
    }

    [TestFixture]
    public class EdgeCasesForPlanetfall : PronounResolutionTests
    {
        [Test]
        public async Task TakeIt_WhenNothingRevealed_ShouldAskForClarification()
        {
            var target = GetTarget();
            StartHere<LabOffice>();
            Context.LastNoun = "";
            Context.LastInput = null;
            Context.LastResponse = null;

            var result = await target.GetResponse("take it");

            // Should ask for clarification since there's no context
            result.Should().Contain("What item are you referring to");
        }

        [Test]
        public async Task DropIt_AfterTakingMemo_ShouldDropMemo()
        {
            // Test the drop it resolution
            var target = GetTarget();
            StartHere<LabOffice>();

            var memo = GetItem<Memo>();
            Context.ItemPlacedHere(memo);
            Context.LastInput = "take memo";
            Context.LastNoun = "memo";

            await target.GetResponse("drop it");

            Context.HasItem<Memo>().Should().BeFalse(
                because: "Memo should be dropped after 'drop it'");
        }
    }
}
