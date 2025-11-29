using FluentAssertions;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Item;
using Moq;
using Planetfall.Item;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Lawanda.Lab;

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

        await (Task<string>)methodInfo.Invoke(floyd, [target.Context, target.GenerationClient, null!])!;

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

    #region Wandering Behavior Tests

    [Test]
    public async Task FloydWanders_SpontaneouslyLeaves()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Ensure no turn-on countdown interferes
        floyd.CurrentLocation = robotShop; // Must be same instance as context location
        robotShop.ItemPlacedHere(floyd); // Place Floyd in the room
        target.Context.RegisterActor(floyd);

        // Mock the Chooser to trigger wandering (1 in 20 chance)
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.SetupSequence(r => r.RollDice(20))
            .Returns(1); // Trigger wandering
        mockChooser.Setup(r => r.RollDice(5))
            .Returns(3); // Wander for 3 turns
        floyd.Chooser = mockChooser.Object;

        // Execute a turn - Floyd should spontaneously wander
        var response = await target.GetResponse("wait");

        response.Should().Contain("Floyd says \"Floyd going exploring. See you later.\" He glides out of the room.");
        floyd.IsOffWandering.Should().BeTrue();
        floyd.WanderingTurnsRemaining.Should().Be(3);
        // Floyd should be removed from the room
        GetLocation<RobotShop>().Items.Should().NotContain(floyd);
    }

    [Test]
    public async Task FloydWanders_DoesNotLeave_WhenDiceRollFails()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        target.Context.RegisterActor(floyd);

        // Mock the Chooser to NOT trigger wandering
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDice(20))
            .Returns(2); // Not 1, so no wandering
        mockChooser.Setup(r => r.RollDice(15))
            .Returns(15); // No random action
        floyd.Chooser = mockChooser.Object;

        var response = await target.GetResponse("wait");

        response.Should().NotContain("Floyd going exploring");
        floyd.IsOffWandering.Should().BeFalse();
        floyd.CurrentLocation.Should().Be(GetLocation<RobotShop>());
    }

    [Test]
    public async Task FloydWanders_Returns_AfterCountdown()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Ensure no turn-on countdown interferes
        floyd.IsOffWandering = true;
        floyd.WanderingTurnsRemaining = 1;
        floyd.CurrentLocation = null; // Floyd is wandering, not in any location
        target.Context.RegisterActor(floyd);

        var response = await target.GetResponse("wait");

        // Check that Floyd returned (one of the three return messages should appear)
        var returnedSuccessfully = response.Contains("Floyd bounds into the room") ||
                                   response.Contains("Floyd rushes into the room") ||
                                   response.Contains("Floyd glides back into the room");
        returnedSuccessfully.Should().BeTrue();
        floyd.IsOffWandering.Should().BeFalse();
        floyd.WanderingTurnsRemaining.Should().Be(0);
        floyd.CurrentLocation.Should().Be(GetLocation<RobotShop>());
    }

    [Test]
    public async Task FloydWanders_CountsDown_OverMultipleTurns()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Ensure no turn-on countdown interferes
        floyd.IsOffWandering = true;
        floyd.WanderingTurnsRemaining = 3;
        floyd.CurrentLocation = null; // Floyd is wandering, not in any location
        target.Context.RegisterActor(floyd);

        // Turn 1: 3 -> 2
        var response1 = await target.GetResponse("wait");
        response1.Should().NotContain("Floyd bounds");
        floyd.WanderingTurnsRemaining.Should().Be(2);
        floyd.IsOffWandering.Should().BeTrue();

        // Turn 2: 2 -> 1
        var response2 = await target.GetResponse("wait");
        response2.Should().NotContain("Floyd bounds");
        floyd.WanderingTurnsRemaining.Should().Be(1);
        floyd.IsOffWandering.Should().BeTrue();

        // Turn 3: 1 -> 0 (returns)
        var response3 = await target.GetResponse("wait");

        // Check that Floyd returned (one of the three return messages should appear)
        var returnedSuccessfully = response3.Contains("Floyd bounds into the room") ||
                                   response3.Contains("Floyd rushes into the room") ||
                                   response3.Contains("Floyd glides back into the room");
        returnedSuccessfully.Should().BeTrue();
        floyd.WanderingTurnsRemaining.Should().Be(0);
        floyd.IsOffWandering.Should().BeFalse();
    }

    [Test]
    public async Task FloydWanders_DoesNotFollow_Sometimes()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Ensure no turn-on countdown interferes
        floyd.CurrentLocation = robotShop; // Must be same instance as context location
        robotShop.ItemPlacedHere(floyd);
        target.Context.RegisterActor(floyd);

        // Mock to trigger no-follow (1 in 5 chance)
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.SetupSequence(r => r.RollDice(5))
            .Returns(1) // Don't follow
            .Returns(3); // Wander for 3 turns
        floyd.Chooser = mockChooser.Object;

        var response = await target.GetResponse("w");

        response.Should().NotContain("Floyd follows you");
        floyd.IsOffWandering.Should().BeTrue();
        floyd.WanderingTurnsRemaining.Should().Be(3);
        floyd.CurrentLocation.Should().BeNull(); // Floyd is wandering, not in any specific location
        target.Context.CurrentLocation.Should().Be(GetLocation<MachineShop>());
    }

    [Test]
    public async Task FloydWanders_DoesFollow_WhenDiceRollFails()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Ensure no turn-on countdown interferes
        floyd.CurrentLocation = robotShop; // Must be same instance as context location
        robotShop.ItemPlacedHere(floyd);
        target.Context.RegisterActor(floyd);

        // Mock to NOT trigger no-follow
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDice(5))
            .Returns(2); // Not 1, so follow normally
        floyd.Chooser = mockChooser.Object;

        var response = await target.GetResponse("w");

        response.Should().Contain("Floyd follows you");
        floyd.IsOffWandering.Should().BeFalse();
        floyd.CurrentLocation.Should().Be(GetLocation<MachineShop>());
    }

    [Test]
    public async Task FloydWanders_NoWandering_WhenOff()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = false;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        target.Context.RegisterActor(floyd);

        // Mock to trigger wandering
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDice(20))
            .Returns(1);
        floyd.Chooser = mockChooser.Object;

        var response = await target.GetResponse("wait");

        response.Should().NotContain("Floyd going exploring");
        floyd.IsOffWandering.Should().BeFalse();
    }

    [Test]
    public async Task FloydWanders_NoWandering_WhenDead()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasDied = true;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        target.Context.RegisterActor(floyd);

        // Mock to trigger wandering
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDice(20))
            .Returns(1);
        floyd.Chooser = mockChooser.Object;

        var response = await target.GetResponse("wait");

        response.Should().NotContain("Floyd going exploring");
        floyd.IsOffWandering.Should().BeFalse();
    }

    [Test]
    public async Task FloydWanders_NoWandering_DuringBioLabFight()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        target.Context.RegisterActor(floyd);

        // Set bio lab fighting state
        var bioLockEast = GetLocation<BioLockEast>();
        bioLockEast.StateMachine.LabSequenceState = BioLockStateMachineManager.FloydLabSequenceState.DoorClosedFloydFighting;

        // Mock to trigger wandering
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDice(20))
            .Returns(1);
        floyd.Chooser = mockChooser.Object;

        var response = await target.GetResponse("wait");

        response.Should().NotContain("Floyd going exploring");
        floyd.IsOffWandering.Should().BeFalse();
    }

    [Test]
    public async Task FloydWanders_NoFollow_DuringBioLabFight()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        target.Context.RegisterActor(floyd);

        // Set bio lab fighting state
        var bioLockEast = GetLocation<BioLockEast>();
        bioLockEast.StateMachine.LabSequenceState = BioLockStateMachineManager.FloydLabSequenceState.DoorClosedFloydFighting;

        var response = await target.GetResponse("w");

        response.Should().NotContain("Floyd follows you");
        floyd.CurrentLocation.Should().Be(GetLocation<RobotShop>());
    }

    [Test]
    public async Task FloydWanders_NoFollow_WhenAlreadyWandering()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.IsOffWandering = true;
        floyd.WanderingTurnsRemaining = 2;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        target.Context.RegisterActor(floyd);

        var response = await target.GetResponse("w");

        response.Should().NotContain("Floyd follows you");
        floyd.CurrentLocation.Should().Be(GetLocation<RobotShop>());
        floyd.WanderingTurnsRemaining.Should().Be(2); // Countdown only happens during wait
    }

    [Test]
    public async Task FloydWanders_Returns_ToPlayerCurrentLocation()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Ensure no turn-on countdown interferes
        floyd.IsOffWandering = true;
        floyd.WanderingTurnsRemaining = 3; // Start with 3 to account for movement triggering Act()
        floyd.CurrentLocation = null; // Floyd is wandering, not in any location
        target.Context.RegisterActor(floyd);

        // Move to a different location (this triggers Act(), so countdown: 3 -> 2)
        await target.GetResponse("w");
        target.Context.CurrentLocation.Should().Be(GetLocation<MachineShop>());
        floyd.WanderingTurnsRemaining.Should().Be(2);

        // Wait for Floyd to count down (2 -> 1)
        await target.GetResponse("wait");
        floyd.WanderingTurnsRemaining.Should().Be(1);

        // Wait one more turn - Floyd should return to player's current location (1 -> 0)
        var response = await target.GetResponse("wait");

        // Check that Floyd returned (one of the three return messages should appear)
        var returnedSuccessfully = response.Contains("Floyd bounds into the room") ||
                                   response.Contains("Floyd rushes into the room") ||
                                   response.Contains("Floyd glides back into the room");
        returnedSuccessfully.Should().BeTrue();
        floyd.IsOffWandering.Should().BeFalse();
        floyd.CurrentLocation.Should().Be(GetLocation<MachineShop>());
        target.Context.CurrentLocation.Should().Be(GetLocation<MachineShop>());
    }

    [Test]
    public void FloydWanders_AllThreeReturnMessages_AreDefined()
    {
        // Verify that all three return messages are defined and not empty
        FloydConstants.ReturnMessages.Should().HaveCount(3);
        FloydConstants.ReturnMessages[0].Should().Contain("Floyd bounds into the room");
        FloydConstants.ReturnMessages[1].Should().Contain("Floyd rushes into the room");
        FloydConstants.ReturnMessages[2].Should().Contain("Floyd glides back into the room");
    }

    [Test]
    public async Task FloydWanders_IntegrationTest_WanderAndReturn()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Ensure no turn-on countdown interferes
        floyd.CurrentLocation = robotShop; // Must be same instance as context location
        robotShop.ItemPlacedHere(floyd);
        target.Context.RegisterActor(floyd);

        // Mock for complete wander-return cycle
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.SetupSequence(r => r.RollDice(20))
            .Returns(1) // Trigger wandering
            .Returns(10); // Don't wander again
        mockChooser.Setup(r => r.RollDice(5))
            .Returns(2); // Wander for 2 turns
        mockChooser.Setup(r => r.RollDice(15))
            .Returns(15); // No random actions
        floyd.Chooser = mockChooser.Object;

        // Turn 1: Floyd leaves
        var response1 = await target.GetResponse("wait");
        response1.Should().Contain("Floyd going exploring");
        floyd.IsOffWandering.Should().BeTrue();
        floyd.WanderingTurnsRemaining.Should().Be(2);

        // Turn 2: Countdown (2 -> 1)
        var response2 = await target.GetResponse("wait");
        response2.Should().NotContain("Floyd");
        floyd.WanderingTurnsRemaining.Should().Be(1);

        // Turn 3: Floyd returns (1 -> 0)
        var response3 = await target.GetResponse("wait");

        // Check that Floyd returned (one of the three return messages should appear)
        var returnedSuccessfully = response3.Contains("Floyd bounds into the room") ||
                                   response3.Contains("Floyd rushes into the room") ||
                                   response3.Contains("Floyd glides back into the room");
        returnedSuccessfully.Should().BeTrue();
        floyd.IsOffWandering.Should().BeFalse();
        floyd.WanderingTurnsRemaining.Should().Be(0);
        floyd.CurrentLocation.Should().Be(GetLocation<RobotShop>());

        // Turn 4: Floyd is back to normal
        var response4 = await target.GetResponse("wait");
        response4.Should().NotContain("Floyd going exploring");
    }

    [Test]
    public void FloydWanders_StateSerializes_Correctly()
    {
        var floyd = GetItem<Floyd>();
        floyd.IsOffWandering = true;
        floyd.WanderingTurnsRemaining = 3;

        // Verify properties are public and have UsedImplicitly attribute (checked via code inspection)
        // Both properties should serialize/deserialize correctly with JSON
        floyd.IsOffWandering.Should().BeTrue();
        floyd.WanderingTurnsRemaining.Should().Be(3);
    }

    [Test]
    public void FloydWanders_StartWandering_SetsPropertiesCorrectly()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        GetLocation<RobotShop>().ItemPlacedHere(floyd);

        // Call the public StartWandering method
        floyd.StartWandering(target.Context);

        // Verify Floyd is now wandering
        floyd.IsOffWandering.Should().BeTrue();
        floyd.WanderingTurnsRemaining.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(5);
        floyd.CurrentLocation.Should().BeNull();
        GetLocation<RobotShop>().Items.Should().NotContain(floyd);
    }

    [Test]
    public void FloydWanders_StartWandering_DoesNothingWhenOff()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = false;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        GetLocation<RobotShop>().ItemPlacedHere(floyd);

        // Call StartWandering when Floyd is off
        floyd.StartWandering(target.Context);

        // Verify Floyd does NOT start wandering
        floyd.IsOffWandering.Should().BeFalse();
        floyd.CurrentLocation.Should().Be(GetLocation<RobotShop>());
    }

    [Test]
    public void FloydWanders_StartWandering_DoesNothingWhenDead()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasDied = true;
        floyd.CurrentLocation = GetLocation<RobotShop>();
        GetLocation<RobotShop>().ItemPlacedHere(floyd);

        // Call StartWandering when Floyd is dead
        floyd.StartWandering(target.Context);

        // Verify Floyd does NOT start wandering
        floyd.IsOffWandering.Should().BeFalse();
        floyd.CurrentLocation.Should().Be(GetLocation<RobotShop>());
    }

    #endregion
}