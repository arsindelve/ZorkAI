using FluentAssertions;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class LaserTests : EngineTestsBase
{
    [Test]
    public async Task SetDialToExistingSetting()
    {
        var target = GetTarget();
        Take<Laser>();

        var response = await target.GetResponse("set laser to 5");

        response.Should().Contain("That's where it's set now");
        GetItem<Laser>().Setting.Should().Be(5);
    }
    
    [Test]
    public async Task SetDialToNewSetting()
    {
        var target = GetTarget();
        Take<Laser>();

        var response = await target.GetResponse("set laser to 6");

        response.Should().Contain("now set to 6");
        GetItem<Laser>().Setting.Should().Be(6);
    }
    
    [Test]
    public async Task SetDialToTooLowNumber()
    {
        var target = GetTarget();
        Take<Laser>();

        var response = await target.GetResponse("set laser to 0");

        response.Should().Contain("he dial can only be set to numbers between 1 and 6");
        GetItem<Laser>().Setting.Should().Be(5);
    }
    
    [Test]
    public async Task SetDialToTooHighNumber()
    {
        var target = GetTarget();
        Take<Laser>();

        var response = await target.GetResponse("set laser to 90");

        response.Should().Contain("he dial can only be set to numbers between 1 and 6");
        GetItem<Laser>().Setting.Should().Be(5);
    }
    
    [Test]
    public async Task SetDialToTNonsense()
    {
        var target = GetTarget();
        Take<Laser>();

        var response = await target.GetResponse("set laser to bob");

        response.Should().Contain("he dial can only be set to numbers between 1 and 6");
        GetItem<Laser>().Setting.Should().Be(5);
    }

    [Test]
    public async Task ShootLaserWithoutBattery()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<Laser>().Items.Clear();
        GetItem<OldBattery>().CurrentLocation = null;

        var response = await target.GetResponse("shoot laser");

        response.Should().Contain("Click");
    }
    
    [Test]
    public async Task ShootLaserWithDeadBattery()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 0;
        
        var response = await target.GetResponse("shoot laser");

        response.Should().Contain("Click");
    }
    
    [Test]
    public async Task ShootLaserNotHoldingIt()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        
        var response = await target.GetResponse("shoot laser");

        response.Should().Contain("You're not holding the laser");
    }
    
    [Test]
    public async Task ShootLaserWithGoodBattery()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 5;
        
        var response = await target.GetResponse("shoot laser");

        response.Should().NotContain("Click");
        response.Should().Contain("beam of light");
    }
    
    [Test]
    public async Task ShootLaserWithGoodBatteryDepleatIt()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 2;
        
        var response = await target.GetResponse("shoot laser");
        response.Should().NotContain("Click");
        response.Should().Contain("beam of light");
        
        response = await target.GetResponse("shoot laser");
        response.Should().NotContain("Click");
        response.Should().Contain("beam of light");
        
        response = await target.GetResponse("shoot laser");
        response.Should().Contain("Click");
        response.Should().NotContain("beam of light");
    }
    
    [Test]
    [TestCase(1, "red")]
    [TestCase(2, "orange")]
    [TestCase(3, "yellow")]
    [TestCase(4, "green")]
    [TestCase(5, "blue")]
    [TestCase(6, "violet")]
    public async Task LasterColors(int dial, string color)
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<Laser>().Setting = dial;
        GetItem<OldBattery>().ChargesRemaining = 5;
        
        var response = await target.GetResponse("shoot laser");

        response.Should().NotContain("Click");
        response.Should().Contain("beam of light");
        response.Should().Contain(color);
    }
}