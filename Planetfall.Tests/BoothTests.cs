using System.Text;
using FluentAssertions;
using Planetfall.Item;
using Planetfall.Item.Feinstein;
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

    // The flip side of the announcement: when the window lapses while the player has wandered off, the
    // original stays silent (I-TURNOFF-TELEPORTATION only tells if HERE is a booth, globals.zil:1540) -
    // no phantom "ready light goes dark" should leak into an unrelated room. The booth still deactivates.
    [Test]
    public async Task Activation_ExpiresSilently_WhenPlayerHasLeftTheBooth()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        Take<TeleportationAccessCard>();

        await target.GetResponse("slide teleportation access card through slot");
        await target.GetResponse("west");
        target.Context.CurrentLocation.Should().BeOfType<LibraryLobby>();

        var everything = new StringBuilder();
        for (var i = 0; i < 32; i++)
            everything.Append(await target.GetResponse("wait"));

        // Fired silently: no announcement while the player is elsewhere...
        everything.ToString().Should().NotContain("ready light goes dark");
        // ...but the activation still lapsed.
        GetLocation<BoothThree>().IsEnabled.Should().BeFalse();

        // And it is genuinely off: returning to the booth and pressing a button is rebuffed.
        await target.GetResponse("east");
        var response = await target.GetResponse("press 1");
        response.Should().Contain("not aktivaatid");
        target.Context.CurrentLocation.Should().BeOfType<BoothThree>();
    }

    // Issue #520 (second defect, same rooms): Booths 1 and 2 carried a stray "\n" in the middle of the
    // panel sentence - a copy-paste artifact in the two booths that mention button "3" - so the room
    // description broke mid-sentence on a line of its own. Booth 3, which has no "3" button, was
    // always written without it.
    [Test]
    public async Task RoomDescription_HasNoStrayMidSentenceLineBreak()
    {
        var target = GetTarget();

        foreach (var look in new List<Func<Task<string>>>
                 {
                     () => { StartHere<BoothOne>(); return target.GetResponse("look")!; },
                     () => { StartHere<BoothTwo>(); return target.GetResponse("look")!; },
                     () => { StartHere<BoothThree>(); return target.GetResponse("look")!; }
                 })
        {
            var response = await look();
            response.Should().Contain("This is a tiny room with a large");
            response.Should().NotContain("labelled\n");
        }
    }

    // Issue #520: the original's TELEPORT robs the booth into the destination
    // (<ROB ,HERE .DEST>, globals.zil:1522), so anything lying on the booth floor travels with you.
    // The port relocated only the player and Floyd, stranding dropped items - and Booth 3 is on a
    // different continent from Booths 1 and 2.
    [Test]
    public async Task Teleport_TakesTheBoothsContentsWithYou()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        Take<TeleportationAccessCard>();
        Take<Brush>();

        await target.GetResponse("drop brush");
        GetItem<Brush>().CurrentLocation.Should().BeOfType<BoothTwo>();

        await target.GetResponse("slide teleportation access card through slot");
        var response = await target.GetResponse("press one");

        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        target.Context.CurrentLocation.Should().BeOfType<BoothOne>();

        GetItem<Brush>().CurrentLocation.Should().BeOfType<BoothOne>();
        GetLocation<BoothOne>().Items.Should().Contain(GetItem<Brush>());
        GetLocation<BoothTwo>().Items.Should().NotContain(GetItem<Brush>());
    }

    // The subtlety in the fix: in the ZIL the card slot is a LOCAL-GLOBALS object without TAKEBIT, so
    // ROB never picks it up. In the port each booth genuinely holds its own TeleportationSlot, so a
    // naive move-all would drag the slot along and corrupt both rooms. Only takeable things travel.
    [Test]
    public async Task Teleport_LeavesTheBoothsOwnCardSlotBehind()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        Take<TeleportationAccessCard>();
        Take<Brush>();

        await target.GetResponse("drop brush");
        await target.GetResponse("slide teleportation access card through slot");
        await target.GetResponse("press one");

        GetItem<TeleportationSlot<BoothTwo>>().CurrentLocation.Should().BeOfType<BoothTwo>();
        GetLocation<BoothTwo>().Items.Should().Contain(GetItem<TeleportationSlot<BoothTwo>>());
        GetLocation<BoothOne>().Items.Should().NotContain(GetItem<TeleportationSlot<BoothTwo>>());
        GetLocation<BoothOne>().Items.Should().Contain(GetItem<TeleportationSlot<BoothOne>>());
    }

    // Robbing the booth must not disturb what already worked: Floyd still comes along, and the
    // teleport still reads as a single "strange feeling" line with no stray item chatter.
    [Test]
    public async Task Teleport_WithItemsAndFloyd_StillMovesFloydAndSaysNothingExtra()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        Take<TeleportationAccessCard>();
        Take<Brush>();
        GetItem<Floyd>().CurrentLocation = GetLocation<BoothThree>();

        await target.GetResponse("drop brush");
        await target.GetResponse("slide teleportation access card through slot");
        var response = await target.GetResponse("press one");

        response.Should().Contain("Floyd gives a terrified squeal, and clutches at his guidance mechanism");
        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        GetItem<Floyd>().CurrentLocation.Should().BeOfType<BoothOne>();
        GetItem<Brush>().CurrentLocation.Should().BeOfType<BoothOne>();
    }

    // Teleporting Floyd used to be a bare CurrentLocation assignment, so he was never removed from the
    // departure booth's Items nor added to the destination's. CurrentLocation alone looked right, but
    // everything that resolves nouns through the room - scope checks, ConversationHandler's
    // CollectTalkableEntities - reads CurrentLocation.Items, so Floyd was unreachable in the room he
    // had just arrived in, and a phantom Floyd lingered in the booth he left. FloydMovementManager
    // can't self-heal it either: its isInTheRoom check compares CurrentLocation, which already agrees.
    // Seed him the way HandleFollowingPlayer does, not by assignment, or the desync can't reproduce.
    [Test]
    public async Task Teleport_ReParentsFloydIntoTheDestinationsItemList()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        Take<TeleportationAccessCard>();
        GetLocation<BoothThree>().ItemPlacedHere(GetItem<Floyd>());

        await target.GetResponse("slide teleportation access card through slot");
        await target.GetResponse("press one");

        target.Context.CurrentLocation.Should().BeOfType<BoothOne>();
        GetItem<Floyd>().CurrentLocation.Should().BeOfType<BoothOne>();
        GetLocation<BoothOne>().Items.Should().Contain(GetItem<Floyd>());
        GetLocation<BoothThree>().Items.Should().NotContain(GetItem<Floyd>());
    }

    // The player-visible half of the same defect: having arrived together, Floyd must still be
    // addressable in the destination booth.
    [Test]
    public async Task Teleport_LeavesFloydReachableInTheDestination()
    {
        var target = GetTarget();
        StartHere<BoothThree>();
        Take<TeleportationAccessCard>();
        GetLocation<BoothThree>().ItemPlacedHere(GetItem<Floyd>());

        await target.GetResponse("slide teleportation access card through slot");
        await target.GetResponse("press one");

        var response = await target.GetResponse("examine floyd");
        response.Should().Contain("robot");
    }

    // Booth 2's tan button is labelled "3", but its noun list carried "two" (a label Booth 2 does not
    // have - that is Booth 1's beige button) and omitted the spelled-out "three". So "press three"
    // cleared the outer guard, matched neither branch, and silently fell through to the narrator
    // without teleporting. "press 3" and the disambiguation path both worked, which hid it; Booths 1
    // and 3 list the spelled-out word correctly.
    [Test]
    public async Task BoothTwo_PressThree_Teleports()
    {
        var target = GetTarget();
        StartHere<BoothTwo>();
        Take<TeleportationAccessCard>();

        await target.GetResponse("slide teleportation access card through slot");
        var response = await target.GetResponse("press three");

        response.Should().Contain("You experience a strange feeling in the pit of your stomach");
        target.Context.CurrentLocation.Should().BeOfType<BoothThree>();
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