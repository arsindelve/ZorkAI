using FluentAssertions;
using GameEngine;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

public class BlatherTests : EngineTestsBase
{
    // Issue #407: BLATHER-F (planetfall-source/globals.zil:742-772) also handles ATTACK/KICK (an
    // authored death), SALUTE, and TAKE. The port only covered examine and throw, so these verbs
    // fell through to the generic narrator instead of their authored responses.

    [Test]
    public async Task AttackBlather_KillsThePlayer()
    {
        var target = GetTarget();
        var deckNine = StartHere<DeckNine>();
        GetItem<Blather>().JoinsTheScene(target.Context, deckNine);

        var response = await target.GetResponse("attack blather");

        response.Should().Contain("Blather removes several of your appendages and internal organs");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task KickBlather_KillsThePlayer()
    {
        var target = GetTarget();
        var deckNine = StartHere<DeckNine>();
        GetItem<Blather>().JoinsTheScene(target.Context, deckNine);

        var response = await target.GetResponse("kick blather");

        response.Should().Contain("Blather removes several of your appendages and internal organs");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task SaluteBlather_SneerSoftens()
    {
        var target = GetTarget();
        var deckNine = StartHere<DeckNine>();
        GetItem<Blather>().JoinsTheScene(target.Context, deckNine);

        var response = await target.GetResponse("salute blather");

        response.Should().Contain("Blather's sneer softens a bit");
        response.Should().Contain("First right thing you've done today. Only five demerits.");
    }

    [Test]
    public async Task TakeBlather_BrushesYouAway()
    {
        var target = GetTarget();
        var deckNine = StartHere<DeckNine>();
        GetItem<Blather>().JoinsTheScene(target.Context, deckNine);

        var response = await target.GetResponse("take blather");

        response.Should().Contain("Blather brushes you away, muttering about suspended shore leave.");
    }

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
}