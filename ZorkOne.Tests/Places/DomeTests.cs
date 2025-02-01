using FluentAssertions;
using GameEngine;
using Model.Interface;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class DomeTests : EngineTestsBase
{
    [Test]
    public async Task DomeRoom_TieRopeToRailing_DoNotHaveTheRope()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DomeRoom>();
        ((ICanHoldItems)target.Context.CurrentLocation).ItemPlacedHere(Repository.GetItem<Rope>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("tie rope to railing");

        // Assert
        response.Should().Contain("don't have the rope");
        Repository.GetItem<Rope>().TiedToRailing.Should().BeFalse();
    }

    [Test]
    public async Task DomeRoom_TieRopeToRailing_Success()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DomeRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        target.Context.Take(Repository.GetItem<Rope>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("tie rope to railing");

        // Assert
        response.Should().Contain("drops over the side");
        Repository.GetItem<Rope>().TiedToRailing.Should().BeTrue();
    }

    [Test]
    public async Task DomeRoom_TakingTheRopeUntiesIt()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DomeRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        target.Context.Take(Repository.GetItem<Rope>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        await target.GetResponse("tie rope to railing");
        target.Context.HasItem<Rope>().Should().BeFalse();
        var response = await target.GetResponse("take rope");

        // Assert
        response.Should().Contain("Taken");
        Repository.GetItem<Rope>().TiedToRailing.Should().BeFalse();
        target.Context.HasItem<Rope>().Should().BeTrue();
    }

    [Test]
    public async Task DomeRoom_Jump_Fatal()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DomeRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        target.Context.Take(Repository.GetItem<Rope>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("jump");

        // Assert
        response.Should().Contain("died");
        target.Context.DeathCounter.Should().Be(1);
    }
}