using Azure;
using FluentAssertions;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Item;
using Moq;
using Planetfall.Item;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Lawanda;

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
    public async Task TurnOn_FloydIsDead()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.HasDied = true;
        floyd.HasEverBeenOn = true;

        var response = await target.GetResponse("activate floyd");

        response.Should().Contain("As you touch Floyd's on-off switch, it falls off in your hands");
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
    
    [Test]
    public async Task GiveSomethingToFloyd_TakeItBack()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<Diary>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().HasEverBeenOn = true;
        
        await target.GetResponse("give the diary to floyd");
        var response = await target.GetResponse("take diary");

        response.Should().Contain("Taken");
        GetItem<Floyd>().ItemBeingHeld.Should().BeNull();
        target.Context.HasItem<Diary>().Should().BeTrue();
    }
    
    [Test]
    public async Task ExamineFloyd_On()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().HasEverBeenOn = true;
        
        var response = await target.GetResponse("examine floyd");

        response.Should().Contain("From its design, the robot seems to be of the multi-purpose sort");
    }
    
    [Test]
    public async Task ExamineFloyd_Off()
    {
        var target = GetTarget();
        StartHere<RobotShop>();

        var response = await target.GetResponse("examine floyd");

        response.Should().Contain("The deactivated robot is leaning against the wall");
    }

    [Test]
    public async Task ExamineFloyd_Dead()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.HasDied = true;

        var response = await target.GetResponse("examine floyd");

        response.Should().Contain("You turn to look at Floyd, but a tremendous sense of loss overcomes you, and you turn away");
    }

    [Test]
    public async Task LookAtRoom_FloydIsDead()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.HasDied = true;
        floyd.CurrentLocation = GetLocation<RobotShop>();

        var response = await target.GetResponse("look");

        response.Should().Contain("Your former companion, Floyd, is lying on the ground in a pool of oil");
    }

    [Test]
    public async Task FloydIsDead_DoesNotPerformRandomActions()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.HasDied = true;
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        target.Context.RegisterActor(floyd);

        // Trigger multiple turns - Floyd should not perform any actions
        var response1 = await target.GetResponse("wait");
        var response2 = await target.GetResponse("wait");
        var response3 = await target.GetResponse("wait");

        response1.Should().NotContain("Floyd");
        response2.Should().NotContain("Floyd");
        response3.Should().NotContain("Floyd");
    }

    [Test]
    public async Task FloydIsDead_DoesNotFollowPlayer()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.HasDied = true;
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        target.Context.RegisterActor(floyd);

        // Move to another location
        var response = await target.GetResponse("w");

        response.Should().NotContain("Floyd follows you");
        floyd.CurrentLocation.Should().Be(GetLocation<RobotShop>());
    }

    [Test]
    public async Task TurnOff_FloydIsDead()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.HasDied = true;
        floyd.HasEverBeenOn = true;

        var response = await target.GetResponse("deactivate floyd");

        response.Should().Contain("I'm afraid that Floyd has already been turned off, permanently, and gone to that great robot shop in the sky");
    }

    [Test]
    public async Task DoesFloydOfferCard_AllSystemsGo()
    {
        var target = GetTarget();
        StartHere<MessHall>();
        Take<KitchenAccessCard>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().CurrentLocation = GetLocation<MessHall>();
        GetItem<Floyd>().Chooser = Mock.Of<IRandomChooser>(r => r.RollDiceSuccess(3) == true);
        
        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().Contain("Floyd claps his hands with excitement");
        GetItem<Floyd>().ItemBeingHeld.Should().BeOfType<LowerElevatorAccessCard>();
    }
    
    [Test]
    public async Task DoesFloydOfferCard_NotHere()
    {
        var target = GetTarget();
        StartHere<MessHall>();
        Take<KitchenAccessCard>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().CurrentLocation = GetLocation<Library>();
        GetItem<Floyd>().Chooser = Mock.Of<IRandomChooser>(r => r.RollDiceSuccess(3) == true);
        
        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().NotContain("Floyd claps his hands with excitement");
    }
    
    [Test]
    public async Task DoesFloydOfferCard_DiceRollFail()
    {
        var target = GetTarget();
        StartHere<MessHall>();
        Take<KitchenAccessCard>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().CurrentLocation = GetLocation<MessHall>();
        GetItem<Floyd>().Chooser = Mock.Of<IRandomChooser>(r => r.RollDiceSuccess(3) == false);
        
        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().NotContain("Floyd claps his hands with excitement");
    }
    
    [Test]
    public async Task DoesFloydOfferCard_HeIsOff()
    {
        var target = GetTarget();
        StartHere<MessHall>();
        Take<KitchenAccessCard>();
        GetItem<Floyd>().CurrentLocation = GetLocation<MessHall>();
        GetItem<Floyd>().Chooser = Mock.Of<IRandomChooser>(r => r.RollDiceSuccess(3) == true);
        
        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().NotContain("Floyd claps his hands with excitement");
    }
    
    [Test]
    public async Task DoesFloydOfferCard_NoLongerHasIt()
    {
        var target = GetTarget();
        StartHere<MessHall>();
        Take<KitchenAccessCard>();
        GetItem<Floyd>().IsOn = true;
        GetItem<Floyd>().Items.Clear();
        GetItem<Floyd>().CurrentLocation = GetLocation<MessHall>();
        GetItem<Floyd>().Chooser = Mock.Of<IRandomChooser>(r => r.RollDiceSuccess(3) == true);
        
        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().NotContain("Floyd claps his hands with excitement");
    }
    
    [Test]
    public async Task GenerateCompanionSpeech_ExcludesFloydFromRoomDescription()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = GetLocation<RobotShop>();

        // Mock the generation client to capture the request that was sent
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd says something")
            .Callback<CompanionRequest>(request =>
            {
                // Verify that Floyd's description is not included in the room description
                var floydDescription = floyd.GenericDescription(GetLocation<RobotShop>()).Trim();
                request.UserMessage!.Should().NotContain(floydDescription);

                // Specifically check that it doesn't contain "multiple purpose robot"
                request.UserMessage!.Should().NotContain("multiple purpose robot");

                // But verify that the room name is still included
                request.UserMessage!.Should().Contain("Robot Shop");
            });

        // Call GenerateCompanionSpeech directly through reflection to test the specific method
        var methodInfo = typeof(QuirkyCompanion).GetMethod("GenerateCompanionSpeech",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        methodInfo!.Should().NotBeNull();

        await (Task<string>)methodInfo!.Invoke(floyd, new object[] { target.Context, target.GenerationClient, null! })!;

        // Verify the mock was called
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Once);
    }

    [Test]
    public async Task FloydWakesUp_PlayerStaysInRoom_NormalMessage()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();

        // Activate Floyd (this counts as a turn, countdown goes from 3 to 2)
        await target.GetResponse("activate floyd");

        // Wait for the countdown to complete (2 more turns needed: 2->1, 1->wake)
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        // Should use the normal "comes to life" message
        response.Should().Contain("Suddenly, the robot comes to life");
        response.Should().Contain("Hi! I'm B-19-7");
        response.Should().NotContain("bounds into the room");
        floyd.IsOn.Should().BeTrue();
        floyd.CurrentLocation.Should().Be(GetLocation<RobotShop>());
    }

    [Test]
    public async Task FloydWakesUp_PlayerLeaves_BoundsIntoRoom()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();

        // Activate Floyd (countdown: 3->2)
        await target.GetResponse("activate floyd");

        // Leave the room (countdown: 2->1)
        await target.GetResponse("w");
        target.Context.CurrentLocation.Should().Be(GetLocation<MachineShop>());

        // Wait for Floyd to wake up (countdown: 1->wake)
        var response = await target.GetResponse("wait");

        // Should use the special "bounds into the room" message
        response.Should().Contain("The robot you were fiddling with in the Robot Shop bounds into the room");
        response.Should().Contain("\"Hi!\" he says, with a wide and friendly smile");
        response.Should().Contain("You turn Floyd on? Be Floyd's friend, yes?");
        response.Should().NotContain("Suddenly, the robot comes to life");
        floyd.IsOn.Should().BeTrue();
        floyd.CurrentLocation.Should().Be(GetLocation<MachineShop>());
    }

    [Test]
    public async Task FloydWakesUp_PlayerLeavesAndKeepsMoving_BoundsIntoCurrentLocation()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();

        // Activate Floyd (countdown: 3->2)
        await target.GetResponse("activate floyd");

        // Leave the room (go to Machine Shop) - movement also processes actors (countdown: 2->1)
        await target.GetResponse("w");

        // Move to another location (go to Mech Corridor South) - movement processes actors (countdown: 1->wake)
        var response = await target.GetResponse("n");
        target.Context.CurrentLocation.Should().Be(GetLocation<MechCorridorSouth>());

        // Floyd should appear in the current location (MechCorridorSouth)
        response.Should().Contain("The robot you were fiddling with in the Robot Shop bounds into the room");
        floyd.IsOn.Should().BeTrue();
        floyd.CurrentLocation.Should().Be(GetLocation<MechCorridorSouth>());
    }

    [Test]
    public async Task FloydComesAlive_WithInventory_MentionsRandomItem()
    {
        var target = GetTarget();
        StartHere<RobotShop>();

        // Manually add an item to inventory for Floyd to comment on
        var diary = Take<Diary>();

        var floyd = GetItem<Floyd>();
        floyd.IsOn = false;
        floyd.HasEverBeenOn = false;
        floyd.TurnOnCountdown = 1;
        floyd.CurrentLocation = GetLocation<RobotShop>();

        // Mock the Chooser to return the diary for deterministic testing
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.Choose(It.IsAny<List<IItem>>())).Returns(diary);
        floyd.Chooser = mockChooser.Object;

        target.Context.RegisterActor(floyd);

        // Player has diary in inventory - Floyd should mention it
        var response = await target.GetResponse("wait");

        response.Should().Contain("B-19-7");
        response.Should().Contain("That's a nice");
        response.Should().Contain("diary");
        response.Should().Contain("you are having there");
        response.Should().Contain("Hider-and-Seeker");
    }

    [Test]
    public async Task FloydComesAlive_WithoutInventory_NoItemMention()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = false;
        floyd.HasEverBeenOn = false;
        floyd.TurnOnCountdown = 1;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        target.Context.RegisterActor(floyd);

        // Remove all items from player inventory
        target.Context.Items.Clear();

        // Floyd should not mention any item
        var response = await target.GetResponse("wait");

        response.Should().Contain("B-19-7");
        response.Should().NotContain("That's a nice");
        response.Should().Contain("Hider-and-Seeker");
    }
}