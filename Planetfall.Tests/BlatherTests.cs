using FluentAssertions;
using GameEngine;
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
}