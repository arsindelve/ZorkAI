using FluentAssertions;
using GameEngine.Item.ItemProcessor;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Intent;
using Model.Interface;
using Moq;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

/// <summary>
/// Issue #354 follow-up: DeckNine.Act() rolls a 1-in-6 (ambassador) / 1-in-6 (Blather) chance every
/// time it's called while Context.Moves is in the 2-6 range and neither has joined yet. Free commands
/// (look/score/inventory/time) run actors without advancing Moves, so without a guard, the same Moves
/// value would get an independent roll on every consecutive free command instead of exactly one roll
/// per distinct Moves value - inflating the true encounter probability.
/// </summary>
public class DeckNineTests : EngineTestsBase
{
    [Test]
    public async Task EncounterRoll_DoesNotRepeat_OnConsecutiveFreeCommandsAtSameMoves()
    {
        var engine = GetTarget();
        var deckNine = StartHere<DeckNine>();
        engine.Context.RegisterActor(deckNine);
        engine.Context.Moves = 3; // within the (1, 7) trigger range

        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(6)).Returns(6); // no encounter this roll
        deckNine.Chooser = mockChooser.Object;

        await engine.GetResponse("look"); // free - Moves stays 3
        await engine.GetResponse("score"); // free again - Moves still 3

        mockChooser.Verify(c => c.RollDice(6), Times.Once);
    }

    [Test]
    public async Task EncounterRoll_RollsAgain_OnceMovesAdvances()
    {
        // Control: the guard must only block re-rolling at the SAME Moves value, not forever.
        var engine = GetTarget();
        var deckNine = StartHere<DeckNine>();
        engine.Context.RegisterActor(deckNine);
        engine.Context.Moves = 3;

        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(6)).Returns(6);
        deckNine.Chooser = mockChooser.Object;

        await engine.GetResponse("look"); // free - Moves stays 3, rolls once
        await engine.GetResponse("wait"); // real - Moves advances to 4, should roll again

        mockChooser.Verify(c => c.RollDice(6), Times.Exactly(2));
    }

    [Test]
    public async Task Ambassador_Joins_WhenRollLandsOnAmbassador()
    {
        var engine = GetTarget();
        var deckNine = StartHere<DeckNine>();
        engine.Context.RegisterActor(deckNine);
        engine.Context.Moves = 3;

        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(6)).Returns(1);
        deckNine.Chooser = mockChooser.Object;

        await engine.GetResponse("wait");

        deckNine.Items.Should().Contain(GetItem<Ambassador>());
    }

    /// <summary>
    /// The ambassador's celery must be in the player's scope while he is present. In the original,
    /// the celery is moved into the room alongside him (globals.zil:825) so its authored TAKE and
    /// EAT responses can fire. Sealing it inside the ambassador (a closed, opaque container) put it
    /// out of GetItemInScope's reach, so a live AI-tagged "take the celery" (a TakeIntent) fell
    /// through to the "take something that is not portable" AI narrator instead.
    /// </summary>
    [TestFixture]
    public class CeleryTests : EngineTestsBase
    {
        [Test]
        public async Task TakeCelery_ViaTakeIntent_WhileAmbassadorIsPresent_YieldsProtocolRefusal()
        {
            // Production's real AI parser tags "take the celery" as a TakeIntent, which GameEngine
            // dispatches straight to TakeOrDropInteractionProcessor.Process(TakeIntent, ...). That
            // path resolves the noun with Repository.GetItemInScope, which rejects anything nested
            // in a closed, opaque container - so the TakeIntent overload must be invoked directly,
            // exactly as GameEngine.cs does (same technique as the issue #342 tests).
            var engine = GetTarget();
            var deckNine = StartHere<DeckNine>();
            GetItem<Ambassador>().JoinsTheScene(engine.Context, deckNine);

            var takeAndDropParser = new Mock<IAITakeAndAndDropParser>();
            takeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(["celery"]);
            var processor = new TakeOrDropInteractionProcessor(takeAndDropParser.Object);

            var (_, message) = await processor.Process(
                new TakeIntent { Noun = "celery", OriginalInput = "take the celery" },
                engine.Context, Mock.Of<IGenerationClient>());

            message.Should().Contain("The ambassador seems perturbed by your lack of normal protocol.");
            engine.Context.Items.Should().NotContain(GetItem<Celery>());
        }

        [Test]
        public async Task TakeCelery_WhileAmbassadorIsPresent_YieldsProtocolRefusal()
        {
            var engine = GetTarget();
            var deckNine = StartHere<DeckNine>();
            GetItem<Ambassador>().JoinsTheScene(engine.Context, deckNine);

            var response = await engine.GetResponse("take celery");

            response.Should().Contain("The ambassador seems perturbed by your lack of normal protocol.");
            engine.Context.Items.Should().NotContain(GetItem<Celery>());
        }

        [Test]
        public async Task GetCelery_WhileAmbassadorIsPresent_YieldsProtocolRefusal()
        {
            // Issue #406: "get" is a TAKE synonym in the original (syntax.zil:334
            // <SYNONYM TAKE GET HOLD CARRY>), and the engine already treats it as one for takeable
            // items (Verbs.TakeVerbs). CannotBeTakenProcessor, however, kept its own hardcoded copy
            // of the take-verb family that had drifted ("get"/"grab" missing), so "get celery"
            // skipped the ambassador's authored refusal and fell through to the improvised
            // "verb has no effect" AI narration.
            var engine = GetTarget();
            var deckNine = StartHere<DeckNine>();
            GetItem<Ambassador>().JoinsTheScene(engine.Context, deckNine);

            var response = await engine.GetResponse("get celery");

            response.Should().Contain("The ambassador seems perturbed by your lack of normal protocol.");
            engine.Context.Items.Should().NotContain(GetItem<Celery>());
        }

        [Test]
        public async Task EatCelery_WhileAmbassadorIsPresent_KillsThePlayer()
        {
            var engine = GetTarget();
            var deckNine = StartHere<DeckNine>();
            GetItem<Ambassador>().JoinsTheScene(engine.Context, deckNine);

            var response = await engine.GetResponse("eat celery");

            response.Should().Contain("Blow'k-Bibben-Gordoan metabolism is not compatible with our own");
            response.Should().Contain("You die of all sorts of convulsions");
        }

        [Test]
        public async Task TakeAll_WhileAmbassadorIsPresent_ReportsTheCeleryRefusalAndDoesNotTakeIt()
        {
            // Pins the "take all" behavior for the now-room-level celery: the engine's convention
            // is to report non-takeable items with their CannotBeTakenDescription (a deliberate,
            // player-friendly divergence from the original's TAKE ALL, which skips non-TAKEBIT
            // objects entirely).
            var engine = GetTarget();
            var deckNine = StartHere<DeckNine>();
            GetItem<Ambassador>().JoinsTheScene(engine.Context, deckNine);

            var response = await engine.GetResponse("take all");

            response.Should().Contain("The ambassador seems perturbed by your lack of normal protocol.");
            engine.Context.Items.Should().NotContain(GetItem<Celery>());
        }

        [Test]
        public async Task AmbassadorRandomDeparture_TakesTheCeleryWithHim()
        {
            // The random-departure branch of Ambassador.Act, forced deterministically via the
            // injected IRandomChooser (RollDice(10): <=2 idle, <=4 leave, else quirky speech).
            var engine = GetTarget();
            var deckNine = StartHere<DeckNine>();
            var ambassador = GetItem<Ambassador>();
            ambassador.JoinsTheScene(engine.Context, deckNine);

            var mockChooser = new Mock<IRandomChooser>();
            mockChooser.Setup(c => c.RollDice(10)).Returns(3); // force the "leaves" branch
            ambassador.Chooser = mockChooser.Object;

            var response = await engine.GetResponse("wait");

            response.Should().Contain("grunts a polite farewell");
            deckNine.Items.Should().NotContain(ambassador);
            deckNine.Items.Should().NotContain(GetItem<Celery>());
        }

        [Test]
        public void Celery_ArrivesAndLeavesWithTheAmbassador()
        {
            // Mirrors the original: MOVE CELERY,HERE on arrival (globals.zil:825) and
            // REMOVE CELERY when he goes (globals.zil:806).
            var engine = GetTarget();
            var deckNine = StartHere<DeckNine>();
            var ambassador = GetItem<Ambassador>();

            ambassador.JoinsTheScene(engine.Context, deckNine);
            deckNine.Items.Should().Contain(GetItem<Celery>());

            ambassador.LeavesTheScene(engine.Context);
            deckNine.Items.Should().NotContain(GetItem<Celery>());
        }

        [Test]
        public async Task Celery_IsNotListedInTheRoomDescription()
        {
            // NDESCBIT in the original: the celery is present and interactable but never listed.
            // The word-level assertion is deliberately strict: neither the item list nor any
            // authored description currently mentions celery outside the arrival text. If a future
            // change adds "celery" to the ambassador's or slime's LOOK-visible description on
            // purpose, scope this assertion to the item list (e.g. "There is a celery") instead.
            var engine = GetTarget();
            var deckNine = StartHere<DeckNine>();
            GetItem<Ambassador>().JoinsTheScene(engine.Context, deckNine);

            var response = await engine.GetResponse("look");

            response.Should().NotContain("celery");
        }
    }
}
