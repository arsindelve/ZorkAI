using FluentAssertions;
using GameEngine;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

public class FeinsteinTests : EngineTestsBase
{
    [Test]
    public async Task EnterBlatherLocationThreeTinesEndUpInBrig()
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
}