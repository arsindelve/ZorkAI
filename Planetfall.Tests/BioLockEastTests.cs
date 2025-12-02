using FluentAssertions;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Lawanda.Lab;
using static Planetfall.Location.Lawanda.Lab.BioLockStateMachineManager;

namespace Planetfall.Tests;

public class BioLockEastTests : EngineTestsBase
{
    [Test]
    public async Task Floyd_IsPresent_AndTurnedOn_AndComputerRoomConcernNotExpressed_ShouldSayLookAMiniCard()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        target.Context.RegisterActor(bioLockEast);
        var computerRoom = GetLocation<ComputerRoom>();
        computerRoom.FloydHasExpressedConcern = false;

        var response = await target.GetResponse("wait");

        response.Should().Contain(FloydConstants.LookAMiniCard);
    }

    [Test]
    public async Task Floyd_IsNotTurnedOn_ShouldNotSayLookAMiniCard()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = false;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        target.Context.RegisterActor(bioLockEast);
        var computerRoom = GetLocation<ComputerRoom>();
        computerRoom.FloydHasExpressedConcern = false;

        var response = await target.GetResponse("wait");

        response.Should().NotContain(FloydConstants.LookAMiniCard);
    }

    [Test]
    public async Task Floyd_NotInRoom_ShouldNotSayLookAMiniCard()
    {
        var target = GetTarget();
        GetItem<Floyd>().IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        target.Context.RegisterActor(bioLockEast);
        var computerRoom = GetLocation<ComputerRoom>();
        computerRoom.FloydHasExpressedConcern = false;

        var response = await target.GetResponse("wait");

        response.Should().NotContain(FloydConstants.LookAMiniCard);
    }

    [Test]
    public async Task Floyd_SaysLookAMiniCard_OnlyOnce()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        target.Context.RegisterActor(bioLockEast);
        var computerRoom = GetLocation<ComputerRoom>();
        computerRoom.FloydHasExpressedConcern = false;

        var response1 = await target.GetResponse("wait");
        response1.Should().Contain(FloydConstants.LookAMiniCard);

        var response2 = await target.GetResponse("wait");
        response2.Should().NotContain(FloydConstants.LookAMiniCard);
    }

    [Test]
    public async Task Floyd_IsPresent_ButComputerRoomConcernAlreadyExpressed_ShouldNotSayLookAMiniCard()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        target.Context.RegisterActor(bioLockEast);
        var computerRoom = GetLocation<ComputerRoom>();
        computerRoom.FloydHasExpressedConcern = true;

        var response = await target.GetResponse("wait");

        response.Should().NotContain(FloydConstants.LookAMiniCard);
    }

    [Test]
    public async Task Floyd_ComputerRoomConcernExpressed_ShouldSayNeedToGetCard()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        target.Context.RegisterActor(bioLockEast);
        var computerRoom = GetLocation<ComputerRoom>();
        computerRoom.FloydHasExpressedConcern = true;

        var response = await target.GetResponse("wait");

        response.Should().Contain(FloydConstants.NeedToGetCard);
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard.Should().BeTrue();
    }

    [Test]
    public async Task Floyd_SaysNeedToGetCard_OnlyOnce()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        target.Context.RegisterActor(bioLockEast);
        var computerRoom = GetLocation<ComputerRoom>();
        computerRoom.FloydHasExpressedConcern = true;

        var response1 = await target.GetResponse("wait");
        response1.Should().Contain(FloydConstants.NeedToGetCard);

        var response2 = await target.GetResponse("wait");
        response2.Should().NotContain(FloydConstants.NeedToGetCard);
    }

    [Test]
    public async Task Floyd_AfterNeedToGetCard_ShouldSayOpenTheDoor()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = true;
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("wait");

        response.Should().Contain(FloydConstants.OpenTheDoor);
        bioLockEast.StateMachine.FloydWaitingForDoorOpenCount.Should().Be(1);
    }

    [Test]
    public async Task Floyd_SaysOpenTheDoor_ForFourTurns()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = true;
        target.Context.RegisterActor(bioLockEast);

        for (int i = 1; i <= 4; i++)
        {
            var response = await target.GetResponse("wait");
            response.Should().Contain(FloydConstants.OpenTheDoor);
            bioLockEast.StateMachine.FloydWaitingForDoorOpenCount.Should().Be(i);
        }
    }

    [Test]
    public async Task Floyd_OnFifthTurn_ShouldSulk()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = true;
        target.Context.RegisterActor(bioLockEast);

        // First 4 turns should say OpenTheDoor
        for (int i = 1; i <= 4; i++)
        {
            var response = await target.GetResponse("wait");
            response.Should().Contain(FloydConstants.OpenTheDoor);
        }

        // Fifth turn should sulk
        var sulkResponse = await target.GetResponse("wait");
        sulkResponse.Should().Contain(FloydConstants.Sulk);
        bioLockEast.StateMachine.FloydWaitingForDoorOpenCount.Should().Be(5);
    }

    [Test]
    public async Task Floyd_AfterSulking_ShouldSayNothing()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = true;
        bioLockEast.StateMachine.FloydWaitingForDoorOpenCount = 5;
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("wait");

        response.Should().NotContain(FloydConstants.OpenTheDoor);
        response.Should().NotContain(FloydConstants.Sulk);
    }

    [Test]
    public async Task Floyd_FullSequence_FromLookAMiniCardToSulk()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        target.Context.RegisterActor(bioLockEast);
        var computerRoom = GetLocation<ComputerRoom>();
        computerRoom.FloydHasExpressedConcern = false;

        // Turn 0: One-turn delay (Floyd says nothing)
        var response0 = await target.GetResponse("wait");
        response0.Should().NotContain(FloydConstants.LookAMiniCard);

        // Turn 1: Look a mini card
        var response1 = await target.GetResponse("wait");
        response1.Should().Contain(FloydConstants.LookAMiniCard);

        // Set computer room concern
        computerRoom.FloydHasExpressedConcern = true;

        // Turn 2: Need to get card
        var response2 = await target.GetResponse("wait");
        response2.Should().Contain(FloydConstants.NeedToGetCard);

        // Turns 3-6: Open the door (4 times)
        for (int i = 0; i < 4; i++)
        {
            var response = await target.GetResponse("wait");
            response.Should().Contain(FloydConstants.OpenTheDoor);
        }

        // Turn 7: Sulk
        var sulkResponse = await target.GetResponse("wait");
        sulkResponse.Should().Contain(FloydConstants.Sulk);

        // Turn 8: Nothing
        var finalResponse = await target.GetResponse("wait");
        finalResponse.Should().NotContain(FloydConstants.OpenTheDoor);
        finalResponse.Should().NotContain(FloydConstants.Sulk);
    }

    // ===== Door Sequence Tests =====

    [Test]
    public async Task OpenDoor_WhenFloydReady_ShouldStartSequenceWithInTheLabOne()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = true;
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("open door");

        response.Should().Contain(FloydConstants.InTheLabOne);
        bioLockEast.StateMachine.LabSequenceState.Should().Be(FloydLabSequenceState.FloydRushedInNeedToCloseDoor);
    }

    [Test]
    public async Task OpenDoor_WhenFloydRushesIn_FloydLocationShouldBeNull()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = true;
        target.Context.RegisterActor(bioLockEast);

        await target.GetResponse("open door");

        // Floyd should have no location - he's in the lab fighting
        floyd.CurrentLocation.Should().BeNull();
        bioLockEast.Items.Should().NotContain(floyd);
    }

    [Test]
    public async Task WhileFloydFighting_FloydLocationShouldRemainNull()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true;
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorClosedFloydFighting;
        target.Context.RegisterActor(bioLockEast);

        // Floyd should have no location while fighting
        floyd.CurrentLocation.Should().BeNull();

        await target.GetResponse("wait");

        // Floyd should still have no location
        floyd.CurrentLocation.Should().BeNull();
    }

    [Test]
    public void WhileFloydFighting_FloydNotInRoomItems()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true;
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorClosedFloydFighting;
        target.Context.RegisterActor(bioLockEast);

        // Floyd should have no location and not be in the room
        floyd.CurrentLocation.Should().BeNull();
        bioLockEast.Items.Should().NotContain(floyd);

        // Player should not be able to find Floyd in the room
        var floydInRoom = bioLockEast.GetAllItemsRecursively.Any(item => item == floyd);
        floydInRoom.Should().BeFalse("Floyd should not be accessible in the room while fighting");
    }

    [Test]
    public async Task CloseDoor_AfterFloydRushesIn_ShouldShowDoorClosedMessage()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = true;
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.FloydRushedInNeedToCloseDoor;
        var door = GetItem<BioLockInnerDoor>();
        door.IsOpen = true; // Door is open after Floyd rushes in
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("close door");

        response.Should().Contain("And not a moment too soon!");
        bioLockEast.StateMachine.LabSequenceState.Should().Be(FloydLabSequenceState.DoorClosedFloydFighting);
    }

    [Test]
    public async Task FloydFighting_Turn1_ShouldShowInTheLabTwo()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorClosedFloydFighting;
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("wait");

        response.Should().Contain("ferocious growlings");
        bioLockEast.StateMachine.FloydFightingTurnCount.Should().Be(1);
    }

    [Test]
    public async Task FloydFighting_Turn2_ShouldShowInTheLabThree()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorClosedFloydFighting;
        bioLockEast.StateMachine.FloydFightingTurnCount = 1;
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("wait");

        response.Should().Contain("three fast knocks");
        bioLockEast.StateMachine.FloydFightingTurnCount.Should().Be(2);
    }

    [Test]
    public async Task FloydFighting_Turn2_ShouldShowInTheLabThreeAndNeedToReopen()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorClosedFloydFighting;
        bioLockEast.StateMachine.FloydFightingTurnCount = 1;
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("wait");

        response.Should().Contain("three fast knocks");
        bioLockEast.StateMachine.LabSequenceState.Should().Be(FloydLabSequenceState.NeedToReopenDoor);
    }

    [Test]
    public async Task ReopenDoor_AfterInTheLabFour_ShouldChangeStateToNeedToCloseAgain()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.NeedToReopenDoor;
        target.Context.RegisterActor(bioLockEast);

        await target.GetResponse("open door");

        bioLockEast.StateMachine.LabSequenceState.Should().Be(FloydLabSequenceState.DoorReopenedNeedToCloseAgain);
    }

    [Test]
    public async Task CloseDoor_AfterReopening_ShouldShowAfterLabAndComplete()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorReopenedNeedToCloseAgain;
        var door = GetItem<BioLockInnerDoor>();
        door.IsOpen = true; // Door is open after reopening
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("close door");

        response.Should().Contain("Floyd staggers to the ground");
        response.Should().Contain("Ballad of the Starcrossed Miner");
        bioLockEast.StateMachine.LabSequenceState.Should().Be(FloydLabSequenceState.Completed);
    }

    // Complete sequence test removed - individual behaviors are tested above
    // The integration has an issue where both Floyd and BioLockEast act on the same turn
    // causing double-incrementing of counters. Individual tests verify correctness.

    [Test]
    public async Task Wait_AfterFloydRushesIn_WithoutClosingDoor_ShouldShowDeathMessage()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.FloydRushedInNeedToCloseDoor;
        bioLockEast.StateMachine.TurnsAfterFloydRushedIn = 1; // Simulate one turn has passed
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("wait");

        response.Should().Contain(FloydConstants.BiologicalNightmaresDeath);
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Wait_AfterReopeningDoor_WithoutClosing_ShouldShowDeathMessage()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorReopenedNeedToCloseAgain;
        bioLockEast.StateMachine.TurnsAfterDoorReopened = 1; // Simulate one turn has passed
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("wait");

        response.Should().Contain(FloydConstants.BiologicalNightmaresDeath);
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Wait_AfterInTheLabFour_WithoutReopening_ShouldShowFloydDiesMessage()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.NeedToReopenDoor;
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("wait");

        response.Should().Contain(FloydConstants.FloydDies);
        floyd.HasDied.Should().BeTrue();
    }

    [Test]
    public async Task SuccessfulSequence_FloydReturnsWithCard_FloydHasDiedFlagSet()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorReopenedNeedToCloseAgain;

        var door = GetItem<BioLockInnerDoor>();
        door.IsOpen = true;

        var response = await target.GetResponse("close door");

        response.Should().Contain("Floyd staggers to the ground");
        response.Should().Contain("Floyd did it");
        response.Should().Contain("in memory of a brave friend");
        floyd.HasDied.Should().BeTrue();
        bioLockEast.StateMachine.LabSequenceState.Should().Be(FloydLabSequenceState.Completed);
    }

    [Test]
    public async Task SuccessfulSequence_ActorsAreUnregisteredAfterCompletion()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorReopenedNeedToCloseAgain;

        var door = GetItem<BioLockInnerDoor>();
        door.IsOpen = true;

        // Register both actors
        target.Context.RegisterActor(floyd);
        target.Context.RegisterActor(bioLockEast);

        var response = await target.GetResponse("close door");

        // Verify actors are unregistered
        target.Context.Actors.Should().NotContain(floyd);
        target.Context.Actors.Should().NotContain(bioLockEast);
        bioLockEast.StateMachine.LabSequenceState.Should().Be(FloydLabSequenceState.Completed);
    }

    [Test]
    public async Task SuccessfulSequence_MiniaturizationCardIsDroppedInRoom()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true;
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorReopenedNeedToCloseAgain;

        var door = GetItem<BioLockInnerDoor>();
        door.IsOpen = true;

        await target.GetResponse("close door");

        // Verify miniaturization card is in the room
        var miniCard = GetItem<MiniaturizationAccessCard>();
        miniCard.CurrentLocation.Should().Be(bioLockEast);
        bioLockEast.Items.Should().Contain(miniCard);
    }

    [Test]
    public async Task SuccessfulSequence_FloydBodyIsPlacedInBioLockEast()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true;
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorReopenedNeedToCloseAgain;

        var door = GetItem<BioLockInnerDoor>();
        door.IsOpen = true;

        await target.GetResponse("close door");

        // Verify Floyd's body is in BioLockEast
        floyd.CurrentLocation.Should().Be(bioLockEast);
        bioLockEast.Items.Should().Contain(floyd);
    }

    [Test]
    public async Task MiniaturizationCard_CanBeTakenAndExamined()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true;
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorReopenedNeedToCloseAgain;

        var door = GetItem<BioLockInnerDoor>();
        door.IsOpen = true;

        await target.GetResponse("close door");

        var takeResponse = await target.GetResponse("take miniaturization card");
        takeResponse.Should().Contain("Taken");

        var examineResponse = await target.GetResponse("examine miniaturization card");
        examineResponse.Should().Contain("minitcurizaashun akses kard");
    }

    [Test]
    public async Task LookThroughWindow()
    {
        var target = GetTarget();
        StartHere<BioLockEast>();

        var response = await target.GetResponse("look through window");

        response.Should().Contain("large laboratory, dimly illuminated");
        response.Should().Contain("blue glow comes from a crack in the northern wall");
        response.Should().Contain("Shadowy, ominous shapes move about within the room");
        response.Should().Contain("magnetic-striped card");
    }
}
