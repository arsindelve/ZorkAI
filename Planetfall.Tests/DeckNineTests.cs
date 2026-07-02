using FluentAssertions;
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
}
