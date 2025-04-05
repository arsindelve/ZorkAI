using Azure;
using FluentAssertions;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Tests;

public class FloydTests : EngineTestsBase
{
    [Test]
    public async Task Search_FindCard()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = false;
        
        var response = await target.GetResponse("search floyd");

        response.Should().Contain("find and take");
        target.Context.HasItem<LowerElevatorAccessCard>().Should().BeTrue();
    }
    
    [Test]
    public async Task Search_AlreadyGone()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = false;
        Take<LowerElevatorAccessCard>();
        
        var response = await target.GetResponse("search floyd");

        response.Should().Contain("search discovers nothing");
        target.Context.HasItem<LowerElevatorAccessCard>().Should().BeTrue();
    }
    
    [Test]
    public async Task Search_Activated()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("search floyd");

        response.Should().Contain("giggles");
    }
    
    [Test]
    public async Task TurnOn_AlreadyOn()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("activate floyd");

        response.Should().Contain("already been");
    }
    
    [Test]
    public async Task PlayWithFloyd()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("play with floyd");

        response.Should().Contain("centichrons");
    }
    
    [Test]
    public async Task KissFloyd()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("kiss floyd");

        response.Should().Contain("shock");
    }
    
    [Test]
    public async Task KickFloyd()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("kick floyd");

        response.Should().Contain("wire");
    }
    
    [Test]
    public async Task KillFloyd()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("punch floyd");

        response.Should().Contain("Chase and Tag");
    }
    
    [Test]
    public async Task GiveSomethingToFloyd()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<Diary>(); 
        GetItem<Floyd>().IsOn = true;
        
        var response = await target.GetResponse("give the diary to floyd");

        response.Should().Contain("Neat");
        GetItem<Diary>().CurrentLocation.Should().BeOfType<Floyd>();
        GetItem<Floyd>().Items.Should().NotBeNull();
    }
    
    [Test]
    public async Task GiveSomethingToFloyd_AlreadyHolding()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<Diary>();
        Take<Key>();
        GetItem<Floyd>().IsOn = true;
        
        await target.GetResponse("give the diary to floyd");
        var response = await target.GetResponse("give the key to floyd");

        response.Should().Contain("shrugs");
        GetItem<Diary>().CurrentLocation.Should().BeOfType<Floyd>();
        GetItem<Key>().CurrentLocation.Should().BeOfType<RobotShop>();
        GetItem<Floyd>().Items.Should().NotBeNull();
    }
    
    [Test]
    public async Task GiveSomethingToFloyd_SeeHimHoldingIt()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<Diary>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().HasEverBeenOn = true;
        
        await target.GetResponse("give the diary to floyd");
        var response = await target.GetResponse("look");

        response.Should().Contain("multiple purpose robot is holding:");
        response.Should().Contain("A diary");
    }
}