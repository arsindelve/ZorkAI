using FluentAssertions;
using GameEngine;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class CanyonViewTests : EngineTestsBase
{
    [Test]
    public async Task CanyonView_Jump_Fatal()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        // Act
        var response = await target.GetResponse("jump");

        // Assert
        response.Should().Contain("lousy place to jump");
        response.Should().Contain("died");
        target.Context.DeathCounter.Should().Be(1);
    }

    [Test]
    public async Task CanyonView_Leap_Fatal()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        // Act
        var response = await target.GetResponse("leap");

        // Assert
        response.Should().Contain("lousy place to jump");
        response.Should().Contain("died");
        target.Context.DeathCounter.Should().Be(1);
    }

    [Test]
    public async Task CanyonView_ClimbDown_NotFatal()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        // Act - climbing down is the legitimate, safe descent into the canyon
        var response = await target.GetResponse("climb down");

        // Assert
        response.Should().NotContain("died");
        target.Context.DeathCounter.Should().Be(0);
        target.Context.CurrentLocation.Should().BeOfType<RockyLedge>();
    }
}
