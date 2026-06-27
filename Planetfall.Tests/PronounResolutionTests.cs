using System.Text.RegularExpressions;
using FluentAssertions;
using Planetfall.GlobalCommand;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda.LabOffice;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Lawanda.LabOffice;
using UnitTests;

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

    /// <summary>
    /// Issue #275: in production, "it" lost its antecedent across movement for an item the player
    /// was still carrying. The deterministic engine seam (MoveEngine preserves LastNoun, ItProcessor
    /// resolves it) was already fixed by #248 and its unit test passes — but a production turn ALSO
    /// runs the AI pronoun resolver first, and after a move that resolver only sees the movement
    /// command (LastInput) and the destination room's description (LastResponse). It therefore
    /// re-bound "it" to a noun in the NEW room and the carried-item antecedent was lost.
    ///
    /// The stock <see cref="TestParser"/> can't reproduce this (its heuristic returns null for the
    /// post-move case, so the deterministic path always wins in tests). These tests drive the same
    /// path prod uses with a resolver that deliberately mimics the production failure mode.
    /// </summary>
    [TestFixture]
    public class ItAcrossMovementWithAiResolver : PronounResolutionTests
    {
        /// <summary>
        /// Models the production gpt-4o-mini pronoun resolver after a move: it re-binds "it" to a
        /// noun from the new room (here a fixed decoy, echoing the issue's "invisible gangway")
        /// instead of honoring the carried-item antecedent that #248 preserved. Subclasses the real
        /// <see cref="TestParser"/> so every non-post-move turn still parses exactly as normal.
        /// </summary>
        private sealed class RebindsItToNewRoomAfterMoveParser(string decoyNoun)
            : TestParser(new PlanetfallGlobalCommandFactory(), "Planetfall")
        {
            private static readonly HashSet<string> Directions = new(StringComparer.OrdinalIgnoreCase)
            {
                "north", "south", "east", "west", "up", "down", "in", "out",
                "ne", "nw", "se", "sw", "n", "s", "e", "w", "u", "d",
                "northeast", "northwest", "southeast", "southwest"
            };

            public override Task<string?> ResolvePronounsAsync(string input, string? lastInput, string? lastResponse)
            {
                // When the previous turn was a bare movement, the real resolver no longer has the
                // carried item in its context and grabs a noun from the destination room instead.
                if (Regex.IsMatch(input, @"\bit\b", RegexOptions.IgnoreCase)
                    && Directions.Contains((lastInput ?? string.Empty).Trim()))
                {
                    return Task.FromResult<string?>(
                        Regex.Replace(input, @"\bit\b", decoyNoun, RegexOptions.IgnoreCase));
                }

                return base.ResolvePronounsAsync(input, lastInput, lastResponse);
            }
        }

        [Test]
        public async Task DropIt_AfterMovingWithCarriedBar_StillDropsTheCarriedItem()
        {
            // Reproduces AB-020: take bar; examine it; <move>; drop it. The bar is carried the whole
            // time, so "drop it" must still drop it after walking to the next room — even though the
            // production AI resolver would otherwise re-bind "it" to the destination room.
            var parser = new RebindsItToNewRoomAfterMoveParser("gangway");
            var target = GetTarget(parser);

            var toolRoom = StartHere<ToolRoom>();
            var magnet = GetItem<Magnet>();
            toolRoom.ItemPlacedHere(magnet);

            await target.GetResponse("take bar");
            Context.HasItem<Magnet>().Should().BeTrue("the player just picked up the bar");

            await target.GetResponse("examine it"); // sets LastNoun = "bar"
            await target.GetResponse("east");        // a real parsed move to the Machine Shop

            var response = await target.GetResponse("drop it");

            response.Should().Contain("Dropped",
                "'it' still refers to the carried bar after a move (issues #248 / #275)");
            Context.HasItem<Magnet>().Should().BeFalse(
                "the carried bar should be dropped, not re-bound to a noun in the destination room");
        }
    }
}
