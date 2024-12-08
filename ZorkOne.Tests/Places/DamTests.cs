using FluentAssertions;
using GameEngine;
using NUnit.Framework;
using UnitTests;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

public class DamTests : EngineTestsBase
{
    [Test]
    public async Task CannotTurnBolt()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Dam>();
        target.Context.Take(Repository.GetItem<Wrench>());

        var response = await target.GetResponse("turn bolt with wrench");

        response.Should().Contain("best effort");
    }

    [Test]
    public async Task PressYellow_CanTurnBolt()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MaintenanceRoom>();
        target.Context.Take(Repository.GetItem<Wrench>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        await target.GetResponse("press the yellow button");
        await target.GetResponse("S");
        await target.GetResponse("S");

        var response = await target.GetResponse("turn bolt with wrench");

        response.Should().Contain("sluice gates open");
    }

    [Test]
    public async Task PressYellow_NeedWrench()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MaintenanceRoom>();
        target.Context.Take(Repository.GetItem<Wrench>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        await target.GetResponse("press the yellow button");
        await target.GetResponse("S");
        await target.GetResponse("S");

        var response = await target.GetResponse("turn the bolt");

        response.Should().Contain("Your bare hands don't appear to be enough");
    }

    [Test]
    public async Task NeedWrench()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MaintenanceRoom>();
        target.Context.Take(Repository.GetItem<Wrench>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        await target.GetResponse("S");
        await target.GetResponse("S");

        var response = await target.GetResponse("turn the bolt");

        response.Should().Contain("Your bare hands don't appear to be enough");
    }

    [Test]
    public async Task PressYellow_ThenBrown_CannotTurnBolt()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MaintenanceRoom>();
        target.Context.Take(Repository.GetItem<Wrench>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        await target.GetResponse("press the yellow button");
        await target.GetResponse("press the brown button");
        await target.GetResponse("S");
        await target.GetResponse("S");

        var response = await target.GetResponse("turn bolt with wrench");

        response.Should().Contain("best effort");
    }

    [Test]
    public async Task Draining()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MaintenanceRoom>();
        target.Context.Take(Repository.GetItem<Wrench>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        await target.GetResponse("press the yellow button");
        await target.GetResponse("S");
        await target.GetResponse("S");

        await target.GetResponse("turn bolt with wrench");

        var sr = Repository.GetLocation<ReservoirSouth>();
        sr.IsDraining.Should().BeTrue();
        sr.IsDrained.Should().BeFalse();
        sr.IsFilling.Should().BeFalse();
        sr.IsFilling.Should().BeFalse();

        sr.Description.Should()
            .Contain("ou notice, however, that the water level appears to be dropping at a rapid rate");

        await target.GetResponse("W");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        response.Should()
            .Contain("The water level is now quite low here and you could easily cross over to the other side.");

        sr.IsDraining.Should().BeFalse();
        sr.IsDrained.Should().BeTrue();
        sr.IsFilling.Should().BeFalse();
        sr.IsFilling.Should().BeFalse();
    }

    [Test]
    public async Task Dam_Red_Button()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MaintenanceRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("press the red button");

        // Assert
        response.Should().Contain("lights");
        Repository.GetLocation<MaintenanceRoom>().IsNoLongerDark.Should().BeTrue();
        target.Context.ItIsDarkHere.Should().BeFalse();
        Repository.GetItem<Lantern>().IsOn = false;
        target.Context.ItIsDarkHere.Should().BeFalse();

        // Act Again
        await target.GetResponse("press the red button");

        // Assert again
        target.Context.ItIsDarkHere.Should().BeTrue();
        Repository.GetLocation<MaintenanceRoom>().IsNoLongerDark.Should().BeFalse();
    }

    [Test]
    public async Task Dam_Yellow_Button_And_Brown_Button()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MaintenanceRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        Repository.GetItem<ControlPanel>().GreenBubbleGlowing.Should().BeFalse();

        // Act
        var response = await target.GetResponse("press the yellow button");

        // Assert
        response.Should().Contain("Click");
        Repository.GetItem<ControlPanel>().GreenBubbleGlowing.Should().BeTrue();

        // Act Again
        await target.GetResponse("press the yellow button");

        // Assert again. Still good. No effect, 
        Repository.GetItem<ControlPanel>().GreenBubbleGlowing.Should().BeTrue();

        // Act Again
        await target.GetResponse("press the brown button");
        Repository.GetItem<ControlPanel>().GreenBubbleGlowing.Should().BeFalse();
    }
}