using FluentAssertions;
using GameEngine;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

public class BlatherTests : EngineTestsBase
{
    [Test]
    public async Task StayInBlatherLocationThreeTimes_EndUpInBrig()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Up");
        await target.GetResponse("Up");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        // Assert
        response.Should().Contain("brig");
        target.Context.CurrentLocation.Should().BeOfType<Brig>();
    }

    [Test]
    public async Task StayInBlatherLocationThreeTimes_PartTwo_EndUpInBrig()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("East");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        // Assert
        response.Should().Contain("brig");
        target.Context.CurrentLocation.Should().BeOfType<Brig>();
    }

    [Test]
    public async Task EnterBlatherLocationThreeTimes_EndUpInBrig()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Up");
        await target.GetResponse("Up");
        await target.GetResponse("Down");
        await target.GetResponse("Up");
        await target.GetResponse("Down");
        var response = await target.GetResponse("Up");

        // Assert
        response.Should().Contain("brig");
        target.Context.CurrentLocation.Should().BeOfType<Brig>();
    }

    [Test]
    public async Task EnterBlatherLocationThreeTines_EndUpInBrig_PartTwo()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("E");
        await target.GetResponse("W");
        await target.GetResponse("E");
        await target.GetResponse("W");
        var response = await target.GetResponse("E");

        // Assert
        response.Should().Contain("brig");
        target.Context.CurrentLocation.Should().BeOfType<Brig>();
    }

    // Issue #407: Blather's single-noun interactions from the original (BLATHER-F,
    // globals.zil:742-772). Attacking or kicking him is fatal; saluting and trying to
    // take him have authored flavor responses.

    [Test]
    public async Task AttackBlather_KillsThePlayer()
    {
        var target = GetTarget();
        var deckNine = StartHere<DeckNine>();
        GetItem<Blather>().JoinsTheScene(target.Context, deckNine);

        var response = await target.GetResponse("attack blather");

        response.Should().Contain("Blather removes several of your appendages and internal organs");
        response.Should().Contain("*** You have died ***");
    }

    [Test]
    public async Task KickBlather_KillsThePlayer()
    {
        var target = GetTarget();
        var deckNine = StartHere<DeckNine>();
        GetItem<Blather>().JoinsTheScene(target.Context, deckNine);

        var response = await target.GetResponse("kick blather");

        response.Should().Contain("Blather removes several of your appendages and internal organs");
        response.Should().Contain("*** You have died ***");
    }

    [Test]
    public async Task KillBlather_KillsThePlayer()
    {
        var target = GetTarget();
        var deckNine = StartHere<DeckNine>();
        GetItem<Blather>().JoinsTheScene(target.Context, deckNine);

        var response = await target.GetResponse("kill blather");

        response.Should().Contain("Blather removes several of your appendages and internal organs");
        response.Should().Contain("*** You have died ***");
    }

    [Test]
    public async Task SaluteBlather_EarnsOnlyFiveDemerits()
    {
        var target = GetTarget();
        var deckNine = StartHere<DeckNine>();
        GetItem<Blather>().JoinsTheScene(target.Context, deckNine);

        var response = await target.GetResponse("salute blather");

        response.Should().Contain("Blather's sneer softens a bit");
        response.Should().Contain("First right thing you've done today. Only five demerits.");
    }

    [Test]
    public async Task TakeBlather_HeBrushesYouAway()
    {
        var target = GetTarget();
        var deckNine = StartHere<DeckNine>();
        GetItem<Blather>().JoinsTheScene(target.Context, deckNine);

        var response = await target.GetResponse("take blather");

        response.Should().Contain("Blather brushes you away, muttering about suspended shore leave.");
    }
}