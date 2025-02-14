using FluentAssertions;
using GameEngine;
using Model.Interface;
using Moq;
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

        sr.GetDescription(Mock.Of<IContext>()).Should()
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


    [Test]
    public async Task OpenTube()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        var response = await target.GetResponse("open tube");

        response.Should().Contain("reveals a viscous material");
    }

    [Test]
    public async Task ReadTube()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        var response = await target.GetResponse("read tube");

        response.Should().Contain("Gunk");
    }

    [Test]
    public async Task ExamineTube()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        var response = await target.GetResponse("examine tube");

        response.Should().Contain("Gunk");
    }

    [Test]
    public async Task OpenTubeInInventory()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        await target.GetResponse("take tube");
        await target.GetResponse("open tube");
        var response = await target.GetResponse("i");

        response.Should().Contain("A tube");
        response.Should().Contain("tube contains");
        response.Should().Contain("A viscous material");
    }

    [Test]
    public async Task ClosedTubeInInventory()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        await target.GetResponse("take tube");
        var response = await target.GetResponse("i");

        response.Should().Contain("A tube");
        response.Should().NotContain("tube contains");
        response.Should().NotContain("A viscous material");
    }

    [Test]
    public async Task TakeGunk()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        await target.GetResponse("open tube");
        var response = await target.GetResponse("take material");

        // Unbelievably, we can take and hold the gunk. 
        response.Should().Contain("Taken");
    }

    [Test]
    public async Task HaveGunk()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        await target.GetResponse("open tube");
        await target.GetResponse("take material");
        var response = await target.GetResponse("i");

        // Unbelievably, we can take and hold the gunk. 
        response.Should().Contain("viscous material");
    }

    [Test]
    public async Task Leak_StayAndDie()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        var response = await target.GetResponse("press blue button");
        response.Should().Contain("rumbling sound");
        response.Should().Contain("ankle");

        response = await target.GetResponse("wait");
        response.Should().Contain("shin");

        response = await target.GetResponse("wait");
        response.Should().Contain("shin");

        response = await target.GetResponse("wait");
        response.Should().Contain("knees");

        response = await target.GetResponse("wait");
        response.Should().Contain("knees");

        response = await target.GetResponse("wait");
        response.Should().Contain("hips");

        response = await target.GetResponse("wait");
        response.Should().Contain("hips");

        response = await target.GetResponse("wait");
        response.Should().Contain("waist");

        response = await target.GetResponse("wait");
        response.Should().Contain("waist");

        response = await target.GetResponse("wait");
        response.Should().Contain("chest");

        response = await target.GetResponse("wait");
        response.Should().Contain("chest");

        response = await target.GetResponse("wait");
        response.Should().Contain("neck");

        response = await target.GetResponse("wait");
        response.Should().Contain("drowned yourself");

        GetLocation<MaintenanceRoom>().RoomFlooded.Should().BeTrue();
    }

    [Test]
    public async Task Leak_LeaveAndCannotReenter()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        await target.GetResponse("press blue button");
        await target.GetResponse("w");

        for (var i = 0; i < 14; i++)
            await target.GetResponse("wait");

        var response = await target.GetResponse("e");
        response.Should().Contain("full of water and cannot be");

        GetLocation<MaintenanceRoom>().RoomFlooded.Should().BeTrue();
    }
    
    
    [Test]
    public async Task Leak_LeaveAndComeBack_StillGoing()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        await target.GetResponse("press blue button");
        await target.GetResponse("w");

        for (var i = 0; i < 10; i++)
        {
            var text = await target.GetResponse("wait");
            text.Should().NotContainAny("head", "shin", "ankle", "knees", "hips", "waist", "chest", "neck");
        }

        var response = await target.GetResponse("e");
        response.Should().Contain("chest");
    }
    
    [Test]
    public async Task Leak_FixIt()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        await target.GetResponse("press blue button");
        await target.GetResponse("open tube");
        var response = await target.GetResponse("apply the gunk to the water");
        response.Should().Contain("Zorkian");

        for (var i = 0; i < 15; i++)
        {
            var text = await target.GetResponse("wait");
            text.Should().NotContainAny("head", "ankle", "shin", "knees", "hips");
            
            GetLocation<MaintenanceRoom>().RoomFlooded.Should().BeFalse();
        }
    }
    
    [Test]
    public async Task Leak_FixIt_SecondPhasing()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        await target.GetResponse("press blue button");
        await target.GetResponse("open tube");
        var response = await target.GetResponse("plug the leak with the gunk");
        response.Should().Contain("Zorkian");

        for (var i = 0; i < 15; i++)
        {
            var text = await target.GetResponse("wait");
            text.Should().NotContainAny("head", "ankle", "shin", "knees", "hips");
            
            GetLocation<MaintenanceRoom>().RoomFlooded.Should().BeFalse();
        }
    }
    
    [Test]
    public async Task PressBlueButton_SecondTime()
    {
        var target = GetTarget();
        Take<Torch>();
        StartHere<MaintenanceRoom>();

        await target.GetResponse("press blue button");
        var response = await target.GetResponse("press blue button");
        response.Should().Contain("jammed");
    }
}