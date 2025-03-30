using FluentAssertions;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class BoothTests : EngineTestsBase
{
    [Test]
    public async Task Disambiguation_One()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        
        var response = await target.GetResponse("press button");
        response.Should().Contain("Which button do you mean");
        
        response = await target.GetResponse("one");
        response.Should().Contain("not aktivaatid");
    }
    
    [Test]
    public async Task Disambiguation_Two()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        
        var response = await target.GetResponse("press button");
        response.Should().Contain("Which button do you mean");
        
        response = await target.GetResponse("two");
        response.Should().Contain("not aktivaatid");
    }
    
    [Test]
    public async Task Disambiguation_Three()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        
        var response = await target.GetResponse("press button");
        response.Should().Contain("Which button do you mean");
        
        response = await target.GetResponse("three");
        response.Should().Contain("not aktivaatid");
    }
    
    [Test]
    public async Task Disambiguation_1()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        
        var response = await target.GetResponse("press button");
        response.Should().Contain("Which button do you mean");
        
        response = await target.GetResponse("1");
        response.Should().Contain("not aktivaatid");
    }
    
    [Test]
    public async Task Disambiguation_2()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        
        var response = await target.GetResponse("press button");
        response.Should().Contain("Which button do you mean");
        
        response = await target.GetResponse("2");
        response.Should().Contain("not aktivaatid");
    }
    
    [Test]
    public async Task Disambiguation_3()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        
        var response = await target.GetResponse("press button");
        response.Should().Contain("Which button do you mean");
        
        response = await target.GetResponse("3");
        response.Should().Contain("not aktivaatid");
    }
    
    [Test]
    public async Task Disambiguation_Brown()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        
        var response = await target.GetResponse("press button");
        response.Should().Contain("Which button do you mean");
        
        response = await target.GetResponse("brown");
        response.Should().Contain("not aktivaatid");
    }
    
    [Test]
    public async Task Disambiguation_Beige()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        
        var response = await target.GetResponse("press button");
        response.Should().Contain("Which button do you mean");
        
        response = await target.GetResponse("beige");
        response.Should().Contain("not aktivaatid");
    }
    
    [Test]
    public async Task Disambiguation_Tan()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        
        var response = await target.GetResponse("press button");
        response.Should().Contain("Which button do you mean");
        
        response = await target.GetResponse("tan");
        response.Should().Contain("not aktivaatid");
    }
    
    [Test]
    public async Task ActivateBooth()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        Take<TeleportationAccessCard>();
        
        var response = await target.GetResponse("slide teleportation access card through slot");
        response.Should().Contain("Redee");
    }
    
    [Test]
    public async Task ActivateBooth_VersionTwo()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        Take<TeleportationAccessCard>();
        
        var response = await target.GetResponse("slide teleportation card through slot");
        response.Should().Contain("Redee");
    }
    
    [Test]
    public async Task ActivateBooth_VersionThree()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        Take<TeleportationAccessCard>();
        
        var response = await target.GetResponse("slide teleportation through slot");
        response.Should().Contain("Redee");
    }
    
    [Test]
    public async Task ActivateBooth_WrongCard()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        Take<KitchenAccessCard>();
        
        var response = await target.GetResponse("slide kitchen access card through slot");
        response.Should().Contain("Inkorekt awtharazaashun");
    }
    
    [Test]
    public async Task Go_TwoToThree()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        Take<TeleportationAccessCard>();
        
        await target.GetResponse("slide teleportation access card through slot");
        var response = await target.GetResponse("press 3");
        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        target.Context.CurrentLocation.Should().BeOfType<BoothThree>();
    }
    
    [Test]
    public async Task Go_TwoToOne()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        Take<TeleportationAccessCard>();
        
        await target.GetResponse("slide teleportation access card through slot");
        var response = await target.GetResponse("press one");
        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        target.Context.CurrentLocation.Should().BeOfType<BoothOne>();
    }
    
    [Test]
    public async Task Go_OneToThree()
    {
        var target = GetTarget();
        StartHere<BoothOne>();
        Take<TeleportationAccessCard>();
        
        await target.GetResponse("slide teleportation access card through slot");
        var response = await target.GetResponse("press three");
        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        target.Context.CurrentLocation.Should().BeOfType<BoothThree>();
    }
    
    [Test]
    public async Task Go_OneToTwo()
    {
        var target = GetTarget();
        StartHere<BoothOne>();
        Take<TeleportationAccessCard>();
        
        await target.GetResponse("slide teleportation access card through slot");
        var response = await target.GetResponse("press two");
        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        target.Context.CurrentLocation.Should().BeOfType<BoothTwo>();
    }
    
    [Test]
    public async Task Go_ThreeToTwo()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        Take<TeleportationAccessCard>();
        
        await target.GetResponse("slide teleportation access card through slot");
        var response = await target.GetResponse("press two");
        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        target.Context.CurrentLocation.Should().BeOfType<BoothTwo>();
    }
    
    [Test]
    public async Task Go_ThreeToOne()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        Take<TeleportationAccessCard>();
        
        await target.GetResponse("slide teleportation access card through slot");
        var response = await target.GetResponse("press one");
        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        response.Should().NotContain("Floyd gives a terrified squeal, and clutches at his guidance mechanism");
        target.Context.CurrentLocation.Should().BeOfType<BoothOne>();
    }
    
    [Test]
    public async Task FloydComes()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        Take<TeleportationAccessCard>();
        GetItem<Floyd>().CurrentLocation = GetLocation<BoothThree>();
        
        await target.GetResponse("slide teleportation access card through slot");
        var response = await target.GetResponse("press one");
        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        response.Should().Contain("Floyd gives a terrified squeal, and clutches at his guidance mechanism");
        target.Context.CurrentLocation.Should().BeOfType<BoothOne>();
        GetItem<Floyd>().CurrentLocation.Should().BeOfType<BoothOne>();
    }
}