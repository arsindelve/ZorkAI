using System.Text;
using FluentAssertions;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
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
    public async Task ExamineKeyboard_DescribesTheKeypad_RatherThanDenyingItExists()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();

        // The booth's room description places "a keyboard with numeric keys" here. Examining it must
        // describe the keypad as scenery, not fall through to the narrator that falsely claims the
        // keyboard isn't here (issue #315).
        var response = await target.GetResponse("examine keyboard");
        response.Should().Contain("zero through nine");
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

    // Issue #399: sliding the card activates the booth for only 30 turns in the original
    // (<ENABLE <QUEUE I-TURNOFF-TELEPORTATION 30>>, globals.zil:1414). The port used to leave it
    // "Redee" forever. After the window lapses, pressing a button must report the booth is not
    // activated and must NOT teleport.
    [Test]
    public async Task Activation_Expires_AfterThirtyTurns()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        Take<TeleportationAccessCard>();

        await target.GetResponse("slide teleportation access card through slot");
        for (var i = 0; i < 31; i++)
            await target.GetResponse("wait");

        var response = await target.GetResponse("press 1");
        response.Should().Contain("not aktivaatid");
        target.Context.CurrentLocation.Should().BeOfType<BoothThree>();
    }

    // Complements the expiry test: a normal player slides then presses within a couple of turns,
    // so activation must still be live well inside the 30-turn window.
    [Test]
    public async Task Activation_StillLive_WithinThirtyTurns()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        Take<TeleportationAccessCard>();

        await target.GetResponse("slide teleportation access card through slot");
        for (var i = 0; i < 5; i++)
            await target.GetResponse("wait");

        var response = await target.GetResponse("press 1");
        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        target.Context.CurrentLocation.Should().BeOfType<BoothOne>();
    }

    // When the timer lapses while the player is standing in the booth, the original announces it
    // ("The ready light goes dark.", I-TURNOFF-TELEPORTATION, globals.zil:1538-1542).
    [Test]
    public async Task Activation_AnnouncesExpiry_WhenPlayerInBooth()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        Take<TeleportationAccessCard>();

        await target.GetResponse("slide teleportation access card through slot");

        var everything = new StringBuilder();
        for (var i = 0; i < 32; i++)
            everything.Append(await target.GetResponse("wait"));

        everything.ToString().Should().Contain("The ready light goes dark");
    }

    // Re-sliding the card restarts the countdown (the original re-QUEUEs the turn-off daemon).
    [Test]
    public async Task Activation_ReslidingCard_RestartsCountdown()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        Take<TeleportationAccessCard>();

        await target.GetResponse("slide teleportation access card through slot");
        for (var i = 0; i < 25; i++)
            await target.GetResponse("wait");

        // Re-slide well before expiry, then wait past the ORIGINAL window - the fresh countdown
        // should keep the booth live.
        await target.GetResponse("slide teleportation access card through slot");
        for (var i = 0; i < 10; i++)
            await target.GetResponse("wait");

        var response = await target.GetResponse("press 1");
        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        target.Context.CurrentLocation.Should().BeOfType<BoothOne>();
    }
}