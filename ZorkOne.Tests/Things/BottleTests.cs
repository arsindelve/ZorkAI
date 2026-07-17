using FluentAssertions;
using GameEngine;
using Model.Item;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Things;

public class BottleTests : EngineTestsBase
{
    [Test]
    public async Task ThrowBottle_ShattersAndSpillsWater()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        target.Context.Take(Repository.GetItem<Bottle>());

        var response = await target.GetResponse("throw bottle");

        response.Should().Contain("shatters");
        response.Should().Contain("water spills"); // water was inside from Init()
        target.Context.HasItem<Bottle>().Should().BeFalse(); // bottle destroyed
    }

    [Test]
    public async Task BreakBottle_DestroysIt()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        target.Context.Take(Repository.GetItem<Bottle>());

        var response = await target.GetResponse("break bottle");

        response.Should().Contain("destroys the bottle");
        target.Context.HasItem<Bottle>().Should().BeFalse();
    }

    [Test]
    public async Task SmashBottle_DestroysIt()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        target.Context.Take(Repository.GetItem<Bottle>());

        var response = await target.GetResponse("smash bottle");

        response.Should().Contain("destroys the bottle");
        target.Context.HasItem<Bottle>().Should().BeFalse();
    }

    [Test]
    public async Task ShakeOpenBottleWithWater_SpillsWater()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        var bottle = Repository.GetItem<Bottle>();
        target.Context.Take(bottle);
        ((IOpenAndClose)bottle).IsOpen = true; // water present from Init()

        var response = await target.GetResponse("shake bottle");

        response.Should().Contain("spills to the floor");
        bottle.Items.Should().NotContain(Repository.GetItem<Water>()); // water gone
        target.Context.HasItem<Bottle>().Should().BeTrue(); // bottle kept
    }

    [Test]
    public async Task ShakeClosedBottle_DoesNotSpillWater()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        var bottle = Repository.GetItem<Bottle>();
        target.Context.Take(bottle);
        // Bottle starts closed; shaking should NOT spill the water.

        var response = await target.GetResponse("shake bottle");

        response.Should().NotContain("spills to the floor");
        bottle.Items.Should().Contain(Repository.GetItem<Water>()); // water still inside
    }
}
