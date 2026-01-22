using System.Reflection;
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
    public async Task RubFloyd()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        GetItem<Floyd>().IsOn = true;

        var response = await target.GetResponse("rub floyd");

        response.Should().Contain("contented sigh");
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

        response.Should()
            .Contain("You turn to look at Floyd, but a tremendous sense of loss overcomes you, and you turn away");
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

        response.Should()
            .Contain(
                "I'm afraid that Floyd has already been turned off, permanently, and gone to that great robot shop in the sky");
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
            BindingFlags.NonPublic | BindingFlags.Instance);

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
        mockChooser.Setup(r => r.RollDiceSuccess(20))
            .Returns(true); // Trigger wandering
        mockChooser.Setup(r => r.RollDice(5))
            .Returns(3); // Wander for 3 turns
        floyd.Chooser = mockChooser.Object;

        // Mock the generation client to return a departure message
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd says \"Floyd going to look around. Back soon.\" He wheels out the door.");

        // Execute a turn - Floyd should spontaneously wander
        var response = await target.GetResponse("wait");

        response.Should().Contain("Floyd says");
        response.Should().Contain("Floyd going");
        floyd.IsOffWandering.Should().BeTrue();
        floyd.WanderingTurnsRemaining.Should().Be(3);
        // Floyd should be removed from the room
        GetLocation<RobotShop>().Items.Should().NotContain(floyd);
        // Verify that the generation client was called
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Once);
    }

    [Test]
    public async Task FloydWanders_UsesCorrectPrompt_ForDeparture()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);
        target.Context.RegisterActor(floyd);

        // Mock the Chooser to trigger wandering
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDiceSuccess(20)).Returns(true);
        mockChooser.Setup(r => r.RollDice(5)).Returns(2);
        floyd.Chooser = mockChooser.Object;

        // Mock the generation client to capture the request
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd says \"Floyd going exploring. See you later.\" He glides out of the room.")
            .Callback<CompanionRequest>(request =>
            {
                // Verify the prompt contains key phrases from FloydPrompts.LeavingToExplore
                request.UserMessage!.Should().Contain("Generate Floyd's complete departure message");
                request.UserMessage!.Should().Contain("going to explore");
                request.UserMessage!.Should().Contain("Robot Shop"); // Location name
            });

        await target.GetResponse("wait");

        // Verify the mock was called
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Once);
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
        mockChooser.Setup(r => r.RollDiceSuccess(20))
            .Returns(false); // Don't trigger wandering
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

        // Mock the generation client to return a return message
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd bounds into the room. \"Floyd here now!\" he cries.");

        var response = await target.GetResponse("wait");

        // Check that Floyd returned
        response.Should().Contain("Floyd");
        floyd.IsOffWandering.Should().BeFalse();
        floyd.WanderingTurnsRemaining.Should().Be(0);
        floyd.CurrentLocation.Should().Be(GetLocation<RobotShop>());
        // Verify that the generation client was called
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Once);
    }

    [Test]
    public async Task FloydWanders_UsesCorrectPrompt_ForReturn()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.IsOffWandering = true;
        floyd.WanderingTurnsRemaining = 1;
        floyd.CurrentLocation = null;
        target.Context.RegisterActor(floyd);

        // Mock the generation client to capture the request
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd bounds into the room. \"Floyd here now!\" he cries.")
            .Callback<CompanionRequest>(request =>
            {
                // Verify the prompt contains key phrases from FloydPrompts.ReturningFromExploring
                request.UserMessage!.Should().Contain("Generate Floyd's return message");
                request.UserMessage!.Should().Contain("comes back from exploring");
                request.UserMessage!.Should().Contain("Robot Shop"); // Location name
            });

        await target.GetResponse("wait");

        // Verify the mock was called
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Once);
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

        // Mock the generation client to return a return message
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd rushes into the room and barrels into you. \"Oops, sorry,\" he says.");

        // Turn 1: 3 -> 2
        var response1 = await target.GetResponse("wait");
        response1.Should().NotContain("Floyd bounds");
        response1.Should().NotContain("Floyd rushes");
        floyd.WanderingTurnsRemaining.Should().Be(2);
        floyd.IsOffWandering.Should().BeTrue();

        // Turn 2: 2 -> 1
        var response2 = await target.GetResponse("wait");
        response2.Should().NotContain("Floyd bounds");
        response2.Should().NotContain("Floyd rushes");
        floyd.WanderingTurnsRemaining.Should().Be(1);
        floyd.IsOffWandering.Should().BeTrue();

        // Turn 3: 1 -> 0 (returns)
        var response3 = await target.GetResponse("wait");

        // Check that Floyd returned
        response3.Should().Contain("Floyd");
        floyd.WanderingTurnsRemaining.Should().Be(0);
        floyd.IsOffWandering.Should().BeFalse();
        // Verify that the generation client was called once (only on the return turn)
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Once);
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
        mockChooser.SetupSequence(r => r.RollDiceSuccess(5))
            .Returns(true) // Don't follow
            .Returns(false); // Not used in this test
        mockChooser.Setup(r => r.RollDice(5))
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
        mockChooser.Setup(r => r.RollDiceSuccess(20))
            .Returns(true);
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
        mockChooser.Setup(r => r.RollDiceSuccess(20))
            .Returns(true);
        floyd.Chooser = mockChooser.Object;

        var response = await target.GetResponse("wait");

        response.Should().NotContain("Floyd going exploring");
        floyd.IsOffWandering.Should().BeFalse();
    }

    [Test]
    public async Task FloydWanders_NoWandering_InBioLab()
    {
        var target = GetTarget();
        var bioLockEast = GetLocation<BioLockEast>();
        StartHere<BioLockEast>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = bioLockEast; // Floyd is IN the bio lab (which implements IFloydDoesNotTalkHere)
        bioLockEast.ItemPlacedHere(floyd);
        target.Context.RegisterActor(floyd);

        // Mock to trigger wandering
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDiceSuccess(20))
            .Returns(true);
        floyd.Chooser = mockChooser.Object;

        var response = await target.GetResponse("wait");

        // Floyd should not wander because BioLockEast implements IFloydDoesNotTalkHere
        response.Should().NotContain("Floyd going exploring");
        floyd.IsOffWandering.Should().BeFalse();
    }

    [Test]
    public async Task FloydWanders_NoFollow_WhenInBioLab()
    {
        var target = GetTarget();
        var bioLockEast = GetLocation<BioLockEast>();
        StartHere<BioLockEast>(); // Start in bio lab
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = bioLockEast; // Floyd is IN the bio lab (which implements IFloydDoesNotTalkHere)
        bioLockEast.ItemPlacedHere(floyd);
        target.Context.RegisterActor(floyd);

        // Move to adjacent location - Floyd shouldn't follow because he's in a IFloydDoesNotTalkHere location
        var response = await target.GetResponse("w");

        response.Should().NotContain("Floyd follows you");
        floyd.CurrentLocation.Should().Be(bioLockEast); // Floyd stays in bio lab
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

        // Mock the generation client to return a return message
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd glides back into the room, looking pleased with himself.");

        // Move to a different location (this triggers Act(), so countdown: 3 -> 2)
        await target.GetResponse("w");
        target.Context.CurrentLocation.Should().Be(GetLocation<MachineShop>());
        floyd.WanderingTurnsRemaining.Should().Be(2);

        // Wait for Floyd to count down (2 -> 1)
        await target.GetResponse("wait");
        floyd.WanderingTurnsRemaining.Should().Be(1);

        // Wait one more turn - Floyd should return to player's current location (1 -> 0)
        var response = await target.GetResponse("wait");

        // Check that Floyd returned
        response.Should().Contain("Floyd");
        floyd.IsOffWandering.Should().BeFalse();
        floyd.CurrentLocation.Should().Be(GetLocation<MachineShop>());
        target.Context.CurrentLocation.Should().Be(GetLocation<MachineShop>());
        // Verify that the generation client was called once (only on the return turn)
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Once);
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
        mockChooser.SetupSequence(r => r.RollDiceSuccess(20))
            .Returns(true) // Trigger wandering
            .Returns(false); // Don't wander again
        mockChooser.Setup(r => r.RollDice(5))
            .Returns(2); // Wander for 2 turns
        mockChooser.Setup(r => r.RollDice(15))
            .Returns(15); // No random actions
        floyd.Chooser = mockChooser.Object;

        // Mock the generation client to return departure and return messages
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.SetupSequence(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd says \"Floyd going exploring. See you later.\" He glides out of the room.")
            .ReturnsAsync("Floyd bounds into the room. \"Floyd here now!\" he cries.");

        // Turn 1: Floyd leaves
        var response1 = await target.GetResponse("wait");
        response1.Should().Contain("Floyd says");
        response1.Should().Contain("Floyd going");
        floyd.IsOffWandering.Should().BeTrue();
        floyd.WanderingTurnsRemaining.Should().Be(2);

        // Turn 2: Countdown (2 -> 1)
        var response2 = await target.GetResponse("wait");
        response2.Should().NotContain("Floyd");
        floyd.WanderingTurnsRemaining.Should().Be(1);

        // Turn 3: Floyd returns (1 -> 0)
        var response3 = await target.GetResponse("wait");

        // Check that Floyd returned
        response3.Should().Contain("Floyd");
        floyd.IsOffWandering.Should().BeFalse();
        floyd.WanderingTurnsRemaining.Should().Be(0);
        floyd.CurrentLocation.Should().Be(GetLocation<RobotShop>());
        // Verify that the generation client was called twice (once for departure, once for return)
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Exactly(2));

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

    #region CommentOnAction Tests

    [Test]
    public void CommentOnAction_DoesNotSetPrompt_WhenFloydNotInRoom()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        // Floyd is NOT placed in the room - he's somewhere else
        floyd.CurrentLocation = GetLocation<StorageWest>();

        floyd.CommentOnAction("Test prompt", target.Context);

        var pfContext = target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }

    [Test]
    public void CommentOnAction_DoesNotSetPrompt_WhenFloydNotOn()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = false; // Floyd is off
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        floyd.CommentOnAction("Test prompt", target.Context);

        var pfContext = target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }

    [Test]
    public void CommentOnAction_DoesNotSetPrompt_WhenPromptAlreadyPending()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        // Set a pending prompt to indicate Floyd already has a comment queued
        target.Context.PendingFloydActionCommentPrompt = "First prompt";

        floyd.CommentOnAction("Second prompt", target.Context);

        // Prompt should still be the first one
        target.Context.PendingFloydActionCommentPrompt.Should().Be("First prompt");
    }

    [Test]
    public void CommentOnAction_StoresPrompt_WhenConditionsMet()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        floyd.CommentOnAction("Test prompt about something interesting", target.Context);

        var pfContext = target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().Be("Test prompt about something interesting");
    }

    [Test]
    public void CommentOnAction_SecondCallIgnored_SameTurn()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        // First call should set the prompt
        floyd.CommentOnAction("First prompt", target.Context);
        target.Context.PendingFloydActionCommentPrompt.Should().Be("First prompt");

        // Second call should be ignored (flag already set)
        floyd.CommentOnAction("Second prompt", target.Context);

        // Should still have first prompt
        target.Context.PendingFloydActionCommentPrompt.Should().Be("First prompt");
    }

    [Test]
    public async Task Act_GeneratesComment_WhenPendingPromptSet()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        // Mock the generation client
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd tilts his head and says, \"Interesting!\"");

        // Set a pending prompt
        var pfContext = target.Context;
        pfContext.PendingFloydActionCommentPrompt = "Test prompt about something";

        // Call Act directly
        var result = await floyd.Act(target.Context, target.GenerationClient);

        result.Should().Contain("Floyd tilts his head");
        // Prompt should be cleared after use
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }

    [Test]
    public async Task Act_PerformsNormally_WhenNoPromptPending()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        // Mock the chooser to pick a random action from constants
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDiceSuccess(20)).Returns(false); // Don't wander
        mockChooser.Setup(r => r.RollDice(12)).Returns(1); // Trigger random action (8.3% chance)
        mockChooser.Setup(r => r.RollDice(6)).Returns(6); // Pick from FloydConstants.RandomActions (default case)
        mockChooser.Setup(r => r.Choose(It.IsAny<List<string>>())).Returns((List<string> list) => list[0]);
        floyd.Chooser = mockChooser.Object;

        // No pending prompt
        target.Context.PendingFloydActionCommentPrompt.Should().BeNull();

        // Call Act directly
        var result = await floyd.Act(target.Context, target.GenerationClient);

        // Should return something (the random action starts with "Floyd")
        result.Should().NotBeEmpty();
        result.Should().StartWith("Floyd");
    }

    [Test]
    public void ProcessBeginningOfTurn_ResetsPrompt()
    {
        var target = GetTarget();
        StartHere<RobotShop>();

        var pfContext = target.Context;
        // Set prompt to a non-null value
        pfContext.PendingFloydActionCommentPrompt = "Some prompt";

        // Call ProcessBeginningOfTurn
        target.Context.ProcessBeginningOfTurn();

        // Prompt should be reset
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }

    [Test]
    public async Task Act_PassesPromptToGenerationClient()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        string capturedPrompt = null!;

        // Mock the generation client to capture the request
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd responds")
            .Callback<CompanionRequest>(request => capturedPrompt = request.UserMessage!);

        // Set the pending prompt
        var pfContext = target.Context;
        pfContext.PendingFloydActionCommentPrompt = "This is my specific test prompt about the item";

        await floyd.Act(target.Context, target.GenerationClient);

        capturedPrompt.Should().Contain("This is my specific test prompt about the item");
    }

    [Test]
    public async Task FullFlow_CommentOnAction_ThenAct_GeneratesComment()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        // Mock the generation client
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd says, \"Ooh, neat!\"");

        // Step 1: Call CommentOnAction (stores prompt)
        floyd.CommentOnAction("Floyd should comment on finding a shiny object", target.Context);

        var pfContext = target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().NotBeNull();

        // Step 2: Call Act (generates comment from stored prompt)
        var result = await floyd.Act(target.Context, target.GenerationClient);

        result.Should().Contain("Floyd says");
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull(); // Cleared after use

        // Verify the generation client was called
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Once);
    }

    [Test]
    public void FullFlow_NextTurn_PromptReset()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        var pfContext = target.Context;

        // Turn 1: Floyd comments on action
        floyd.CommentOnAction("Some prompt", target.Context);
        pfContext.PendingFloydActionCommentPrompt.Should().NotBeNull();

        // Simulate new turn
        pfContext.ProcessBeginningOfTurn();

        // Prompt should be reset, allowing Floyd to comment again
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();

        // Floyd can comment on a new action
        floyd.CommentOnAction("New prompt for new turn", target.Context);
        pfContext.PendingFloydActionCommentPrompt.Should().Be("New prompt for new turn");
    }

    [Test]
    public void CommentOnAction_IgnoresRepeatedPrompt_AcrossTurns()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        var pfContext = target.Context;

        // First use of prompt - should work
        floyd.CommentOnAction("Unique prompt", target.Context);
        pfContext.PendingFloydActionCommentPrompt.Should().Be("Unique prompt");
        pfContext.UsedFloydActionCommentPrompts.Should().Contain("Unique prompt");

        // Simulate turn processing (clears pending prompt)
        pfContext.ProcessBeginningOfTurn();
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();

        // Try to use the same prompt again - should be ignored
        floyd.CommentOnAction("Unique prompt", target.Context);
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }

    [Test]
    public void CommentOnAction_TracksMultipleUsedPrompts()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        var pfContext = target.Context;

        // Use first prompt
        floyd.CommentOnAction("First prompt", target.Context);
        pfContext.ProcessBeginningOfTurn();

        // Use second prompt
        floyd.CommentOnAction("Second prompt", target.Context);
        pfContext.ProcessBeginningOfTurn();

        // Use third prompt
        floyd.CommentOnAction("Third prompt", target.Context);

        // All three should be tracked
        pfContext.UsedFloydActionCommentPrompts.Should().HaveCount(3);
        pfContext.UsedFloydActionCommentPrompts.Should().Contain("First prompt");
        pfContext.UsedFloydActionCommentPrompts.Should().Contain("Second prompt");
        pfContext.UsedFloydActionCommentPrompts.Should().Contain("Third prompt");
    }

    [Test]
    public void CommentOnAction_UsedPromptsPersistedAcrossSaves()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        var pfContext = target.Context;

        // Use a prompt
        floyd.CommentOnAction("Persisted prompt", target.Context);

        // Verify it's tracked (UsedImplicitly attribute means it will be serialized)
        pfContext.UsedFloydActionCommentPrompts.Should().Contain("Persisted prompt");
    }

    [Test]
    public void CommentOnAction_DifferentPromptWorksAfterOneUsed()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        var pfContext = target.Context;

        // Use first prompt
        floyd.CommentOnAction("First prompt", target.Context);
        pfContext.ProcessBeginningOfTurn();

        // Different prompt should still work
        floyd.CommentOnAction("Different prompt", target.Context);
        pfContext.PendingFloydActionCommentPrompt.Should().Be("Different prompt");
    }

    [Test]
    public void CommentOnAction_DoesNotTrackPrompt_WhenFloydNotPresent()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        // Floyd is in a different location
        floyd.CurrentLocation = GetLocation<StorageWest>();

        var pfContext = target.Context;

        // Try to use prompt when Floyd not present
        floyd.CommentOnAction("Should not be tracked", target.Context);

        // Prompt should NOT be tracked since Floyd wasn't present
        pfContext.UsedFloydActionCommentPrompts.Should().BeEmpty();
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }

    [Test]
    public void CommentOnAction_DoesNotTrackPrompt_WhenFloydOff()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = false; // Floyd is off
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        var pfContext = target.Context;

        // Try to use prompt when Floyd is off
        floyd.CommentOnAction("Should not be tracked", target.Context);

        // Prompt should NOT be tracked since Floyd was off
        pfContext.UsedFloydActionCommentPrompts.Should().BeEmpty();
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }

    [Test]
    public void CommentOnAction_PromptNotTracked_WhenAnotherPending()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        var pfContext = target.Context;

        // Use first prompt
        floyd.CommentOnAction("First prompt", target.Context);

        // Try second prompt same turn (should be ignored, NOT tracked)
        floyd.CommentOnAction("Second prompt same turn", target.Context);

        // Only first prompt should be tracked
        pfContext.UsedFloydActionCommentPrompts.Should().HaveCount(1);
        pfContext.UsedFloydActionCommentPrompts.Should().Contain("First prompt");
        pfContext.UsedFloydActionCommentPrompts.Should().NotContain("Second prompt same turn");
    }

    [Test]
    public async Task FullFlow_UsedPromptIgnored_AfterActProcesses()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        // Mock chooser to prevent random behavior on second Act() call
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDiceSuccess(20)).Returns(false); // Don't wander
        mockChooser.Setup(r => r.RollDice(15)).Returns(15); // No random action
        floyd.Chooser = mockChooser.Object;

        var pfContext = target.Context;

        // Mock the generation client
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd comments");

        // Turn 1: Use prompt, Act processes it
        floyd.CommentOnAction("One-time prompt", target.Context);
        await floyd.Act(target.Context, target.GenerationClient);

        // Start new turn
        pfContext.ProcessBeginningOfTurn();

        // Try to use same prompt again - should be ignored
        floyd.CommentOnAction("One-time prompt", target.Context);
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();

        // Act should have nothing to do (no pending prompt, and random behavior mocked out)
        var result = await floyd.Act(target.Context, target.GenerationClient);
        result.Should().BeEmpty();

        // Generation should only have been called once (from turn 1)
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Once);
    }

    #endregion

    #region SkipActingThisTurn Tests

    [Test]
    public async Task Act_ReturnsEmpty_WhenSkipActingFlagSet()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        // Mock the Chooser to trigger random behavior (roll 1-7 generates AI speech)
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDiceSuccess(20)).Returns(false); // Don't wander
        mockChooser.Setup(r => r.RollDice(15)).Returns(1); // Would trigger AI speech
        floyd.Chooser = mockChooser.Object;

        // Mock the generation client - this should NOT be called
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd says something random");

        // Set the skip flag
        floyd.SkipActingThisTurn(target.Context);

        // Call Act directly
        var result = await floyd.Act(target.Context, target.GenerationClient);

        // Should return empty despite mocked dice roll that would trigger speech
        result.Should().BeEmpty();

        // Generation client should NOT have been called
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Never);
    }

    [Test]
    public async Task Act_ReturnsEmpty_WhenSkipActingFlagSet_EvenWithPendingComment()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        var pfContext = target.Context;

        // Set a pending comment prompt
        pfContext.PendingFloydActionCommentPrompt = "Floyd should comment on something";

        // Also set the skip flag
        floyd.SkipActingThisTurn(target.Context);

        // Mock the generation client - should NOT be called
        var mockClient = Mock.Get(target.GenerationClient);
        mockClient.Setup(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()))
            .ReturnsAsync("Floyd comments");

        // Call Act directly
        var result = await floyd.Act(target.Context, target.GenerationClient);

        // Should return empty - skip flag overrides pending comment
        result.Should().BeEmpty();

        // Generation client should NOT have been called
        mockClient.Verify(x => x.GenerateCompanionSpeech(It.IsAny<CompanionRequest>()), Times.Never);
    }

    [Test]
    public void SkipActingThisTurn_SetsFlag()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();

        var pfContext = target.Context;
        pfContext.FloydShouldNotActThisTurn.Should().BeFalse();

        floyd.SkipActingThisTurn(target.Context);

        pfContext.FloydShouldNotActThisTurn.Should().BeTrue();
    }

    [Test]
    public void ProcessBeginningOfTurn_ResetsSkipFlag()
    {
        var target = GetTarget();
        StartHere<RobotShop>();

        var pfContext = target.Context;
        pfContext.FloydShouldNotActThisTurn = true;

        pfContext.ProcessBeginningOfTurn();

        pfContext.FloydShouldNotActThisTurn.Should().BeFalse();
    }

    [Test]
    public async Task Act_WorksNormally_AfterFlagReset()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);

        var pfContext = target.Context;

        // Mock the Chooser to trigger constant action
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDiceSuccess(20)).Returns(false); // Don't wander
        mockChooser.Setup(r => r.RollDice(12)).Returns(1); // Trigger random action (8.3% chance)
        mockChooser.Setup(r => r.RollDice(6)).Returns(6); // Pick from FloydConstants.RandomActions (default case)
        mockChooser.Setup(r => r.Choose(It.IsAny<List<string>>())).Returns((List<string> list) => list[0]);
        floyd.Chooser = mockChooser.Object;

        // Turn 1: Skip flag set, Floyd silent
        floyd.SkipActingThisTurn(target.Context);
        var result1 = await floyd.Act(target.Context, target.GenerationClient);
        result1.Should().BeEmpty();

        // Turn 2: Flag reset, Floyd acts normally
        pfContext.ProcessBeginningOfTurn();
        var result2 = await floyd.Act(target.Context, target.GenerationClient);
        result2.Should().NotBeEmpty();
        result2.Should().StartWith("Floyd");
    }

    #endregion
}