using FluentAssertions;
using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Intent;
using Model.Item;
using Moq;
using Newtonsoft.Json;
using Planetfall.Item;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Tower;
using Planetfall.Location.Shuttle;

namespace Planetfall.Tests;

public class ElevatorTests : EngineTestsBase
{
    [Test]
    public async Task Look_BothDoorsClosed()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse("look");
        response.Should()
            .Contain(
                "A blue metal door to the north is closed and a larger red metal door to the south is also closed");
    }

    [Test]
    public async Task Look_OneDoorClosed()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        // A door only reads as open from the lobby when its car is standing at the lobby (issue #505).
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = true;

        var response = await target.GetResponse("look");
        response.Should()
            .Contain("A blue metal door to the north is open and a larger red metal door to the south is closed");
    }

    [Test]
    public async Task Look_OtherDoorClosed()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<LowerElevator>().InLobby = true;

        var response = await target.GetResponse("look");
        response.Should()
            .Contain("A blue metal door to the north is closed and a larger red metal door to the south is open");
    }

    [Test]
    public async Task Look_BothOpen()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<LowerElevator>().InLobby = true;
        GetLocation<UpperElevator>().InLobby = true;

        var response = await target.GetResponse("look");
        response.Should()
            .Contain("A blue metal door to the north is open and a larger red metal door to the south is also open");
    }

    [Test]
    [TestCase("blue")]
    [TestCase("red")]
    public async Task CloseDoor_AlreadyClosed(string door)
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse($"close {door} door");
        response.Should().Contain("It is closed");
    }

    [Test]
    [TestCase("blue")]
    [TestCase("red")]
    public async Task ExamineDoor_Closed(string door)
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse($"examine {door} door");
        response.Should().Contain("The door is closed");
    }


    [Test]
    [TestCase("blue")]
    [TestCase("red")]
    public async Task OpenDoor_Manually(string door)
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse($"open {door} door");
        response.Should().Contain("It won't budge");
    }

    [Test]
    [TestCase("door")]
    [TestCase("elevator door")]
    public async Task OpenDoor_Disambiguation(string door)
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse($"open {door}");
        response.Should().Contain("Do you mean");
        response.Should().Contain("lower elevator door");
        response.Should().Contain("upper elevator door");
    }

    [Test]
    [TestCase("button")]
    [TestCase("elevator button")]
    public async Task ElevatorButton_Disambiguation(string noun)
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse($"press {noun}");
        response.Should().Contain("Which");
        response.Should().Contain("red button");
        response.Should().Contain("blue button");
    }

    [Test]
    public async Task Enter_Lower_DoorClosed()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse("s");
        response.Should().Contain("is closed");
    }

    [Test]
    public async Task Enter_Upper_DoorClosed()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse("n");
        response.Should().Contain("is closed");
    }

    [Test]
    public async Task Enter_FromWaitingArea_DoorClosed()
    {
        var target = GetTarget();
        StartHere<WaitingArea>();
        GetLocation<LowerElevator>().InLobby = false;
        GetItem<LowerElevatorDoor>().IsOpen = false;

        var response = await target.GetResponse("s");
        response.Should().Contain("The door is closed");
        response.Should().NotContain("Lower Elevator");
    }

    [Test]
    public async Task Enter_FromWaitingArea_CarIsUpAtTheLobby()
    {
        var target = GetTarget();
        StartHere<WaitingArea>();
        // The car is up at the lobby with its door open at that end - there is nothing to step into here.
        GetLocation<LowerElevator>().InLobby = true;
        GetItem<LowerElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("s");
        response.Should().Contain("The door is closed");
        response.Should().NotContain("Lower Elevator");
    }

    [Test]
    public async Task Enter_FromWaitingArea_CarIsHere()
    {
        var target = GetTarget();
        StartHere<WaitingArea>();
        GetLocation<LowerElevator>().InLobby = false;
        GetItem<LowerElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("s");
        response.Should().Contain("Lower Elevator");
    }

    [Test]
    public async Task Enter_FromLobby_Upper_CarIsAwayAtTheTower()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetLocation<UpperElevator>().InLobby = false;
        GetItem<UpperElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("n");
        response.Should().Contain("The door is closed");
        response.Should().NotContain("Upper Elevator");
    }

    [Test]
    public async Task Enter_FromLobby_Lower_CarIsAwayAtTheWaitingArea()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetLocation<LowerElevator>().InLobby = false;
        GetItem<LowerElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("s");
        response.Should().Contain("The door is closed");
        response.Should().NotContain("Lower Elevator");
    }

    /// <summary>
    ///     The Tower Core is the far end of the upper shaft, the mirror of the Waiting Area on the lower
    ///     one - and it was still a bare Go&lt;UpperElevator&gt;(). With the shared flag left open by the car's
    ///     arrival at the lobby, you could walk north into a car that is not there; stepping back out
    ///     then landed you in the Elevator Lobby, having ridden nothing, because the car's own exit
    ///     resolves against where the car is rather than where you got in.
    /// </summary>
    [Test]
    public async Task Enter_FromTowerCore_CarIsDownAtTheLobby()
    {
        var target = GetTarget();
        StartHere<TowerCore>();
        // The car is down at the lobby with its door open at that end - there is nothing to step into here.
        GetLocation<UpperElevator>().InLobby = true;
        GetItem<UpperElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("n");
        response.Should().Contain("The door is closed");
        response.Should().NotContain("Upper Elevator");
        Context.CurrentLocation.Should().BeOfType<TowerCore>();
    }

    [Test]
    public async Task Enter_FromTowerCore_DoorClosed()
    {
        var target = GetTarget();
        StartHere<TowerCore>();
        GetLocation<UpperElevator>().InLobby = false;
        GetItem<UpperElevatorDoor>().IsOpen = false;

        var response = await target.GetResponse("n");
        response.Should().Contain("The door is closed");
        response.Should().NotContain("Upper Elevator");
    }

    [Test]
    public async Task Enter_FromTowerCore_CarIsHere()
    {
        var target = GetTarget();
        StartHere<TowerCore>();
        GetLocation<UpperElevator>().InLobby = false;
        GetItem<UpperElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("n");
        response.Should().Contain("Upper Elevator");
    }

    [Test]
    public async Task Press_Red()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse("press red button");
        response.Should().Contain("The red door begins vibrating a bit");
    }

    [Test]
    public async Task Press_Red_Again()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press red button");
        var response = await target.GetResponse("press red button");
        response.Should().Contain("Patience");
    }

    [Test]
    public async Task Press_Blue()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse("press blue button");
        response.Should().Contain("You hear a faint whirring noise from behind the blue door");
    }

    [Test]
    public async Task Press_Blue_Again()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press blue button");
        var response = await target.GetResponse("press blue button");
        response.Should().Contain("Patience");
    }

    [Test]
    public async Task Press_Blue_AndWait()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press blue button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");
        response.Should().Contain("The door at the north end of the room slides open");
    }

    [Test]
    public async Task Press_Red_AndWait()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press red button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");
        response.Should().Contain("The door at the south end of the room slides open");
    }

    [Test]
    public async Task Press_Red_AlreadyOpen()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press red button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("press red button");
        response.Should().Contain("Pushing the red button has no effect");
    }

    [Test]
    public async Task Press_Blue_AlreadyOpen()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press blue button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("press blue button");
        response.Should().Contain("Pushing the blue button has no effect");
    }

    [Test]
    public async Task OpenBlueDoor_AlreadyOpen()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press blue button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("open blue door");
        response.Should().Contain("It is open");
    }

    [Test]
    public async Task OpenRedDoor_AlreadyOpen()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press red button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("open red door");
        response.Should().Contain("It is open");
    }

    [Test]
    public async Task Press_Blue_Enter()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press blue button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        GetLocation<UpperElevator>().InLobby.Should().BeTrue();
        var response = await target.GetResponse("north");
        response.Should().Contain("Upper Elevator");
        response.Should()
            .Contain(
                "This is a tiny room with a sliding door to the south which is open. A control panel contains an Up button, a Down button, and a narrow slot.");
    }

    [Test]
    public async Task Press_Red_Enter()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press red button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("south");
        GetLocation<LowerElevator>().InLobby.Should().BeTrue();
        response.Should().Contain("Lower Elevator");
        response.Should()
            .Contain(
                "This is a medium-sized room with a sliding door to the north which is open. A control panel contains an Up button, a Down button, and a narrow slot.");
    }

    [Test]
    [TestCase("blue")]
    [TestCase("red")]
    public async Task TryToCloseDoorManually(string color)
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse($"press {color} button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse($"close {color} door");
        response.Should().Contain("The door seems designed to slide shut on its own");
    }

    [Test]
    public async Task UseMoveCommandInsideElevator_Red()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press red button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("south");
        var response = await target.GetResponse("up");
        response.Should().Contain("You'll have to");

        response = await target.GetResponse("down");
        response.Should().Contain("You'll have to");
    }

    [Test]
    public async Task UseMoveCommandInsideElevator_Blue()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press blue button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("north");
        var response = await target.GetResponse("up");
        response.Should().Contain("You'll have to");

        response = await target.GetResponse("down");
        response.Should().Contain("You'll have to");
    }

    [Test]
    public async Task EnterAndExitElevator_Upper()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press blue button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("north");
        var response = await target.GetResponse("south");
        response.Should().Contain("Elevator Lobby");
    }

    [Test]
    public async Task EnterAndExitElevator_Lower()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press red button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("south");
        var response = await target.GetResponse("north");
        response.Should().Contain("Elevator Lobby");
    }

    [Test]
    public async Task ExamineSlot_Upper()
    {
        var target = GetTarget();
        StartHere<UpperElevator>();

        var response = await target.GetResponse("examine slot");
        response.Should().Contain("The slot is about ten centimeters wide, but only about two centimeters deep");
    }

    [Test]
    public async Task ExamineSlot_Lower()
    {
        var target = GetTarget();
        StartHere<LowerElevator>();

        var response = await target.GetResponse("examine slot");
        response.Should().Contain("The slot is about ten centimeters wide, but only about two centimeters deep");
    }

    [Test]
    public async Task PutCardInSlot_Upper()
    {
        var target = GetTarget();
        StartHere<UpperElevator>();

        var response = await target.GetResponse("put upper elevator access card in slot");
        response.Should().Contain("The slot is shallow, so you can't put anything in it.");
    }

    [Test]
    public async Task PutCardInSlot_Lower()
    {
        var target = GetTarget();
        StartHere<LowerElevator>();

        var response = await target.GetResponse("put lower elevator access card in slot");
        response.Should().Contain("The slot is shallow, so you can't put anything in it.");
    }

    [Test]
    public async Task SlideWrongCard_Lower()
    {
        var target = GetTarget();
        StartHere<LowerElevator>();
        Take<UpperElevatorAccessCard>();

        var response = await target.GetResponse("slide upper access card through slot");
        response.Should().Contain("A sign flashes \"Inkorekt awtharazaashun kard...akses deeniid.\"");
        GetLocation<LowerElevator>().IsEnabled.Should().BeFalse();
    }

    [Test]
    public async Task SlideWrongCard_Upper()
    {
        var target = GetTarget();
        StartHere<UpperElevator>();
        Take<LowerElevatorAccessCard>();

        var response = await target.GetResponse("slide lower access card through slot");
        response.Should().Contain("A sign flashes \"Inkorekt awtharazaashun kard...akses deeniid.\"");
    }
    
    [Test]
    public async Task SlideCorrectCard_Lower()
    {
        var target = GetTarget();
        StartHere<LowerElevator>();
        Take<LowerElevatorAccessCard>();

        var response = await target.GetResponse("slide lower access card through slot");
        response.Should().Contain("A recorded voice chimes \"Elevator enabled.\"");
        GetLocation<LowerElevator>().IsEnabled.Should().BeTrue();
    }

    [Test]
    public async Task SlideCorrectCard_Upper()
    {
        var target = GetTarget();
        StartHere<UpperElevator>();
        Take<UpperElevatorAccessCard>();

        var response = await target.GetResponse("slide upper access card through slot");
        response.Should().Contain("A recorded voice chimes \"Elevator enabled.\"");
        GetLocation<UpperElevator>().IsEnabled.Should().BeTrue();
    }

    [Test]
    public async Task InElevator_Upper_PressButton_Disambiguate()
    {
        var target = GetTarget();
        StartHere<UpperElevator>();

        var response = await target.GetResponse("press button");
        response.Should().Contain("Which button do you mean");
        response.Should().Contain("Up button");
        response.Should().Contain("Down button");
    }

    [Test]
    public async Task InElevator_Lower_PressButton_Disambiguate()
    {
        var target = GetTarget();
        StartHere<LowerElevator>();

        var response = await target.GetResponse("press button");
        response.Should().Contain("Which button do you mean");
        response.Should().Contain("Up button");
        response.Should().Contain("Down button");
    }

    [Test]
    public async Task InElevator_Lower_NotActivated_PressButton_NothingHappens()
    {
        var target = GetTarget();
        StartHere<LowerElevator>();

        var response = await target.GetResponse("press down button");
        response.Should().Contain("Nothing happens");
    }

    [Test]
    public async Task InElevator_Upper_NotActivated_PressButton_NothingHappens()
    {
        var target = GetTarget();
        StartHere<UpperElevator>();

        var response = await target.GetResponse("press up button");
        response.Should().Contain("Nothing happens");
    }

    [Test]
    public async Task DisablesAfterFiveTurns_Upper()
    {
        var target = GetTarget();
        StartHere<UpperElevator>();
        Take<UpperElevatorAccessCard>();

        await target.GetResponse("slide upper access card through slot");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should().Contain("A recording says \"Elevator no longer enabled.\"");
        GetLocation<UpperElevator>().IsEnabled.Should().BeFalse();
        
        response = await target.GetResponse("press up button");
        response.Should().Contain("Nothing happens");
    }

    [Test]
    public async Task DisablesAfterFiveTurns_Lower()
    {
        var target = GetTarget();
        StartHere<LowerElevator>();
        Take<LowerElevatorAccessCard>();

        await target.GetResponse("slide lower access card through slot");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should().Contain("A recording says \"Elevator no longer enabled.\"");
        GetLocation<LowerElevator>().IsEnabled.Should().BeFalse();
        
        response = await target.GetResponse("press down button");
        response.Should().Contain("Nothing happens");
    }

    [Test]
    public async Task MoveWrongDirection_Upper()
    {
        var target = GetTarget();
        StartHere<LowerElevator>().InLobby = true;
        Take<LowerElevatorAccessCard>();

        await target.GetResponse("slide lower access card through slot");
        var response = await target.GetResponse("press up button");
        response.Should().Contain("Nothing happens");
        GetLocation<LowerElevator>().TurnsSinceMoving.Should().Be(0);
    }
    
    [Test]
    public async Task MoveWrongDirection_Lower()
    {
        var target = GetTarget();
        StartHere<UpperElevator>().InLobby = true;
        Take<UpperElevatorAccessCard>();

        await target.GetResponse("slide upper access card through slot");
        var response = await target.GetResponse("press down button");
        response.Should().Contain("Nothing happens");
        GetLocation<UpperElevator>().TurnsSinceMoving.Should().Be(0);
    }
    
    [Test]
    public async Task MoveRightDirection_Upper()
    {
        var target = GetTarget();
        StartHere<UpperElevator>().InLobby = true;
        Take<UpperElevatorAccessCard>();

        await target.GetResponse("slide upper access card through slot");
        var response = await target.GetResponse("press up button");
        response.Should().Contain("The elevator door slides shut. After a moment, you feel a sensation of vertical movement");
        GetLocation<UpperElevator>().TurnsSinceMoving.Should().Be(2);
        GetItem<UpperElevatorDoor>().IsOpen.Should().BeFalse();
    }
    
    [Test]
    public async Task MoveRightDirection_Lower()
    {
        var target = GetTarget();
        StartHere<LowerElevator>().InLobby = true;
        Take<LowerElevatorAccessCard>();

        await target.GetResponse("slide lower access card through slot");
        var response = await target.GetResponse("press down button");
        response.Should().Contain("The elevator door slides shut. After a moment, you feel a sensation of vertical movement");
        GetLocation<LowerElevator>().TurnsSinceMoving.Should().Be(2);
        GetItem<LowerElevatorDoor>().IsOpen.Should().BeFalse();
    }

    [Test]
    public async Task LowerElevator_RoundTripToTheWaitingArea()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        Take<LowerElevatorAccessCard>();

        await target.GetResponse("press red button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("south");
        response.Should().Contain("Lower Elevator");

        await target.GetResponse("slide lower access card through slot");
        await target.GetResponse("press down button");
        await target.GetResponse("wait");
        response = await target.GetResponse("wait");
        response.Should().Contain("The elevator door slides open");

        response = await target.GetResponse("north");
        response.Should().Contain("Waiting Area");
        GetLocation<LowerElevator>().InLobby.Should().BeFalse();

        // The car is standing right here, so the gated entrance lets us back in.
        response = await target.GetResponse("south");
        response.Should().Contain("Lower Elevator");

        await target.GetResponse("slide lower access card through slot");
        await target.GetResponse("press up button");
        await target.GetResponse("wait");
        response = await target.GetResponse("wait");
        response.Should().Contain("The elevator door slides open");

        response = await target.GetResponse("north");
        response.Should().Contain("Elevator Lobby");
    }

    [Test]
    public async Task PressButtonAgainWhileMoving_DoesNotRestartTheTrip_Upper()
    {
        var target = GetTarget();
        StartHere<UpperElevator>().InLobby = true;
        Take<UpperElevatorAccessCard>();

        await target.GetResponse("slide upper access card through slot");
        await target.GetResponse("press up button");
        GetLocation<UpperElevator>().TurnsSinceMoving.Should().Be(2);

        var response = await target.GetResponse("press up button");
        response.Should().Contain("Nothing happens");
        response.Should().NotContain("The elevator door slides shut");
        GetLocation<UpperElevator>().TurnsSinceMoving.Should().Be(3);

        // The trip still lands on schedule - the second press neither restarted nor extended it.
        response = await target.GetResponse("wait");
        response.Should().Contain("The elevator door slides open");
        GetLocation<UpperElevator>().InLobby.Should().BeFalse();
    }

    [Test]
    public async Task PressButtonAgainWhileMoving_DoesNotRestartTheTrip_Lower()
    {
        var target = GetTarget();
        StartHere<LowerElevator>().InLobby = true;
        Take<LowerElevatorAccessCard>();

        await target.GetResponse("slide lower access card through slot");
        await target.GetResponse("press down button");
        GetLocation<LowerElevator>().TurnsSinceMoving.Should().Be(2);

        var response = await target.GetResponse("press down button");
        response.Should().Contain("Nothing happens");
        response.Should().NotContain("The elevator door slides shut");
        GetLocation<LowerElevator>().TurnsSinceMoving.Should().Be(3);

        // The trip still lands on schedule - the second press neither restarted nor extended it.
        response = await target.GetResponse("wait");
        response.Should().Contain("The elevator door slides open");
        GetLocation<LowerElevator>().InLobby.Should().BeFalse();
    }

    [Test]
    public async Task LookWhileMoving_DescribesTheDoorAsClosed_Upper()
    {
        var target = GetTarget();
        StartHere<UpperElevator>().InLobby = true;
        Take<UpperElevatorAccessCard>();

        await target.GetResponse("slide upper access card through slot");
        await target.GetResponse("press up button");
        GetItem<UpperElevatorDoor>().IsOpen.Should().BeFalse();

        var response = await target.GetResponse("look");
        response.Should().Contain("a sliding door to the south which is closed");
        response.Should().NotContain("which is open");
    }

    [Test]
    public async Task LookWhileMoving_DescribesTheDoorAsClosed_Lower()
    {
        var target = GetTarget();
        StartHere<LowerElevator>().InLobby = true;
        Take<LowerElevatorAccessCard>();

        await target.GetResponse("slide lower access card through slot");
        await target.GetResponse("press down button");
        GetItem<LowerElevatorDoor>().IsOpen.Should().BeFalse();

        var response = await target.GetResponse("look");
        response.Should().Contain("a sliding door to the north which is closed");
        response.Should().NotContain("which is open");
    }

    [Test]
    public async Task UpperElevatorFullTest()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        Take<UpperElevatorAccessCard>();
        
        await target.GetResponse("press blue button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("north");
        response.Should().Contain("Upper Elevator");
        await target.GetResponse("slide upper access card through slot");
        response = await target.GetResponse("press up button");
        response.Should().Contain("The elevator door slides shut. After a moment, you feel a sensation of vertical movement");
        await target.GetResponse("wait");
        response = await target.GetResponse("wait");
        response.Should().Contain("The elevator door slides open");
        response = await target.GetResponse("south");
        response.Should().Contain("Tower Core");
        response = await target.GetResponse("north");
        response.Should().Contain("Upper Elevator");
        response = await target.GetResponse("wait");
        response.Should().Contain("no longer enabled");
        response = await target.GetResponse("press down button");
        response.Should().Contain("Nothing happens");
        await target.GetResponse("slide upper access card through slot");
        response = await target.GetResponse("press down button");
        response.Should().Contain("The elevator door slides shut. After a moment, you feel a sensation of vertical movement");
        await target.GetResponse("wait");
        response = await target.GetResponse("wait");
        response.Should().Contain("The elevator door slides open");
        response = await target.GetResponse("south");
        response.Should().Contain("Lobby");
        await target.GetResponse("wait");
        response.Should().NotContain("enabled");
        response = await target.GetResponse("north");
        response.Should().Contain("Upper Elevator");
        response = await target.GetResponse("press up button");
        response.Should().Contain("Nothing happens");
    }

    /// <summary>
    ///     Issue #505. The door flag is shared by both ends of the shaft, and the arrival code leaves it
    ///     open at whichever end the car parked at. The lobby's description used to read that bare flag,
    ///     so it announced a door as open while walking that way answered "The door is closed."
    /// </summary>
    [Test]
    public async Task Look_Upper_CarIsAwayAtTheTower_ReportsTheBlueDoorClosed()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = false;

        var response = await target.GetResponse("look");
        response.Should().Contain("A blue metal door to the north is closed");
        response.Should().NotContain("blue metal door to the north is open");
    }

    [Test]
    public async Task Look_Lower_CarIsAwayAtTheWaitingArea_ReportsTheRedDoorClosed()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<LowerElevator>().InLobby = false;

        var response = await target.GetResponse("look");
        response.Should().Contain("red metal door to the south is also closed");
        response.Should().NotContain("door to the south is open");
        response.Should().NotContain("door to the south is also open");
    }

    [Test]
    public async Task Look_Upper_CarIsHere_ReportsTheBlueDoorOpen_AndNorthIsPassable()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = true;

        var response = await target.GetResponse("look");
        response.Should().Contain("A blue metal door to the north is open");

        response = await target.GetResponse("n");
        response.Should().Contain("Upper Elevator");
    }

    [Test]
    public async Task Look_Lower_CarIsHere_ReportsTheRedDoorOpen_AndSouthIsPassable()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<LowerElevator>().InLobby = true;

        var response = await target.GetResponse("look");
        response.Should().Contain("red metal door to the south is open");

        response = await target.GetResponse("s");
        response.Should().Contain("Lower Elevator");
    }

    /// <summary>
    ///     The invariant behind issues #456, #450 and #505: whatever word the lobby uses for a door, the
    ///     matching movement must agree. Covers all four (IsOpen x InLobby) states of each shaft.
    /// </summary>
    [Test]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Look_BlueDoorWording_AlwaysAgreesWithWhetherNorthIsPassable(bool isOpen, bool inLobby)
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = isOpen;
        GetLocation<UpperElevator>().InLobby = inLobby;

        var look = await target.GetResponse("look");
        var describedAsOpen = look!.Contains("blue metal door to the north is open");
        look.Should().Contain(describedAsOpen ? "north is open" : "north is closed");

        var move = await target.GetResponse("n");
        var walkedIntoTheCar = move!.Contains("Upper Elevator");

        describedAsOpen.Should().Be(walkedIntoTheCar);
    }

    [Test]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Look_RedDoorWording_AlwaysAgreesWithWhetherSouthIsPassable(bool isOpen, bool inLobby)
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<LowerElevatorDoor>().IsOpen = isOpen;
        GetLocation<LowerElevator>().InLobby = inLobby;

        var look = await target.GetResponse("look");
        var describedAsOpen = look!.Contains("door to the south is open") ||
                              look.Contains("door to the south is also open");

        var move = await target.GetResponse("s");
        var walkedIntoTheCar = move!.Contains("Lower Elevator");

        describedAsOpen.Should().Be(walkedIntoTheCar);
    }

    /// <summary>
    ///     The "also" clause means "both doors are in the same state", so it has to be computed from the
    ///     effective states too - not from the raw flags, which can agree while the effective states differ.
    /// </summary>
    [Test]
    public async Task Look_RawFlagsAgreeButEffectiveStatesDiffer_OmitsAlso()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = true;
        GetLocation<LowerElevator>().InLobby = false;

        var response = await target.GetResponse("look");
        response.Should()
            .Contain("A blue metal door to the north is open and a larger red metal door to the south is closed");
    }

    [Test]
    public async Task Look_RawFlagsDifferButBothEffectivelyClosed_SaysAlso()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = false;
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<LowerElevator>().InLobby = false;

        var response = await target.GetResponse("look");
        response.Should()
            .Contain(
                "A blue metal door to the north is closed and a larger red metal door to the south is also closed");
    }

    /// <summary>
    ///     Issue #505, consequence two: "examine blue door" read the same shaft-wide flag, so the natural
    ///     way to double-check the room description corroborated the wrong answer.
    /// </summary>
    [Test]
    public async Task ExamineDoor_CarIsAway_ReportsClosed()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = false;
        GetLocation<LowerElevator>().InLobby = false;

        var response = await target.GetResponse("examine blue door");
        response.Should().Contain("The door is closed");
        response.Should().NotContain("The door is open");

        response = await target.GetResponse("examine red door");
        response.Should().Contain("The door is closed");
        response.Should().NotContain("The door is open");
    }

    [Test]
    public async Task ExamineDoor_CarIsHere_ReportsOpen()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = true;
        GetLocation<LowerElevator>().InLobby = true;

        var response = await target.GetResponse("examine blue door");
        response.Should().Contain("The door is open");

        response = await target.GetResponse("examine red door");
        response.Should().Contain("The door is open");
    }

    /// <summary>
    ///     Inside the car the flag *is* local, so the car's own door keeps reporting it directly.
    /// </summary>
    [Test]
    public async Task ExamineDoor_InsideTheCar_StillReportsTheCarsOwnDoor()
    {
        var target = GetTarget();
        StartHere<UpperElevator>().InLobby = false;
        GetItem<UpperElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("examine blue door");
        response.Should().Contain("The door is open");
    }

    /// <summary>
    ///     The two doors must not be transposed: with only one car at the lobby, each door has to report
    ///     its own shaft. Both examine tests above use matching states, which would not catch a swap.
    /// </summary>
    [Test]
    public async Task ExamineDoor_OnlyTheBlueCarIsHere_EachDoorReportsItsOwnShaft()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = true;
        GetLocation<LowerElevator>().InLobby = false;

        var response = await target.GetResponse("examine blue door");
        response.Should().Contain("The door is open");

        response = await target.GetResponse("examine red door");
        response.Should().Contain("The door is closed");
    }

    /// <summary>
    ///     The call button is how the player recovers from a car parked at the far end - the whole reason
    ///     issue #505 was not a progression block. It gated on the raw shared flag, so in exactly the
    ///     state the room now (correctly) calls closed it answered "has no effect" and never summoned.
    /// </summary>
    [Test]
    public async Task PressButton_Blue_CarIsAwayWithTheSharedFlagStillOpen_RecallsTheCar()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        // The state a completed ride leaves behind: the car is at the far end, and its arrival left the
        // shared flag open there.
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = false;

        var response = await target.GetResponse("press blue button");
        response.Should().Contain("You hear a faint whirring noise from behind the blue door");
        response.Should().NotContain("no effect");
        response.Should().NotContain("Patience");

        await target.GetResponse("wait");
        await target.GetResponse("wait");
        response = await target.GetResponse("wait");
        response.Should().Contain("The door at the north end of the room slides open");

        response = await target.GetResponse("n");
        response.Should().Contain("Upper Elevator");
    }

    [Test]
    public async Task PressButton_Red_CarIsAwayWithTheSharedFlagStillOpen_RecallsTheCar()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<LowerElevator>().InLobby = false;

        var response = await target.GetResponse("press red button");
        response.Should().Contain("The red door begins vibrating a bit");
        response.Should().NotContain("no effect");
        response.Should().NotContain("Patience");

        await target.GetResponse("wait");
        await target.GetResponse("wait");
        response = await target.GetResponse("wait");
        response.Should().Contain("The door at the south end of the room slides open");

        response = await target.GetResponse("s");
        response.Should().Contain("Lower Elevator");
    }

    /// <summary>
    ///     The same recall, reached by ordinary play rather than by setting flags: ride the blue car up,
    ///     step out at the Tower, and come back to the lobby. Only the player is moved back (as the
    ///     issue's god-mode teleport did); every bit of the elevator's state is whatever the ride left.
    /// </summary>
    [Test]
    public async Task PressButton_Blue_AfterRidingUpAndWalkingBack_RecallsTheCar()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        Take<UpperElevatorAccessCard>();

        await target.GetResponse("press blue button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("north");
        await target.GetResponse("slide upper access card through slot");
        await target.GetResponse("press up button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("south");
        response.Should().Contain("Tower Core");

        // Let the card's activation lapse, as it would while walking back.
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        GetLocation<UpperElevator>().IsEnabled.Should().BeFalse();

        // The car is parked at the top with the shared flag left open by its arrival.
        GetLocation<UpperElevator>().InLobby.Should().BeFalse();
        GetItem<UpperElevatorDoor>().IsOpen.Should().BeTrue();

        // Back at the lobby, on foot. Only the player moves - assigning CurrentLocation directly rather
        // than via StartHere, which would clear the actor list and freeze the elevator's own timers.
        Context.CurrentLocation = GetLocation<ElevatorLobby>();

        response = await target.GetResponse("look");
        response.Should().Contain("A blue metal door to the north is closed");

        response = await target.GetResponse("press blue button");
        response.Should().Contain("You hear a faint whirring noise from behind the blue door");

        await target.GetResponse("wait");
        await target.GetResponse("wait");
        response = await target.GetResponse("wait");
        response.Should().Contain("The door at the north end of the room slides open");

        response = await target.GetResponse("north");
        response.Should().Contain("Upper Elevator");
    }

    /// <summary>
    ///     ExamineInteractionProcessor answers a wider set of verbs than Verbs.ExamineVerbs lists -
    ///     "check", "look in" and "peek at" appear only in its own switch. Every one of them now goes
    ///     straight to the door item, which is the point of #532: the lobby's door object answers for the
    ///     lobby, so no phrasing can slip past a room-level guard and read the shaft flag raw. This used
    ///     to be the room intercepting each verb by hand, and a verb it forgot answered "open" on the same
    ///     turn the room, "examine" and the exit all said closed.
    /// </summary>
    [Test]
    [TestCase("examine")]
    [TestCase("x")]
    [TestCase("check")]
    [TestCase("look at")]
    [TestCase("look in")]
    [TestCase("peek at")]
    public async Task ExamineDoor_EveryVerbTheExamineProcessorAnswers_CarIsAway_ReportsClosed(string verb)
    {
        GetTarget();
        var lobby = StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = false;

        var result = await lobby.RespondToSimpleInteraction(
            new SimpleIntent { Verb = verb, Noun = "blue door" }, Context, Mock.Of<IGenerationClient>(),
            new ItemProcessorFactory(Mock.Of<IAITakeAndAndDropParser>()));

        result.InteractionMessage.Should().Contain("The door is closed");
    }

    /// <summary>
    ///     The arrival line describes the lobby's own doorway, so it must not print in whatever room the
    ///     player wandered off to - ElevatorIsEnabled right below it already guards this way. Clearing
    ///     the summon bookkeeping makes this repeatable rather than once per game.
    /// </summary>
    [Test]
    public async Task Summon_PlayerHasLeftTheLobby_DoesNotAnnounceTheDoorElsewhere()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press blue button");
        await target.GetResponse("west");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        response.Should().NotContain("slides open");
        // The car still arrived - it is only the announcement that is suppressed.
        GetLocation<UpperElevator>().InLobby.Should().BeTrue();
        GetItem<UpperElevatorDoor>().IsOpen.Should().BeTrue();
    }

    /// <summary>
    ///     A summon must survive the card's activation lapsing. Act() prioritises the enabled countdown,
    ///     which ends by removing the actor - that silently cancelled the pending summon and left
    ///     HasBeenSummoned set forever, so every later press answered "Patience, patience..." and the car
    ///     could never be recalled again.
    /// </summary>
    [Test]
    public async Task PressButton_WhileTheCardIsStillActive_SummonStillCompletes()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = false;
        // The car is away and still card-enabled, with its activation about to lapse.
        GetLocation<UpperElevator>().IsEnabled = true;
        GetLocation<UpperElevator>().TurnsSinceEnabled = 5;

        var response = await target.GetResponse("press blue button");
        response.Should().Contain("You hear a faint whirring noise from behind the blue door");

        await target.GetResponse("wait");
        await target.GetResponse("wait");
        response = await target.GetResponse("wait");
        response.Should().Contain("The door at the north end of the room slides open");

        GetLocation<UpperElevator>().HasBeenSummoned.Should().BeFalse();
        response = await target.GetResponse("n");
        response.Should().Contain("Upper Elevator");
    }

    /// <summary>
    ///     A fulfilled summon has to clear its own bookkeeping, or the "Patience, patience..." guard
    ///     stays armed forever and every later summon of that car short-circuits without registering the
    ///     actor - so the car still could never be recalled, even with the button's gate corrected.
    /// </summary>
    [Test]
    public async Task Summon_OnceComplete_ClearsTheSummonBookkeeping()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();

        await target.GetResponse("press blue button");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");
        response.Should().Contain("The door at the north end of the room slides open");

        GetLocation<UpperElevator>().HasBeenSummoned.Should().BeFalse();
        GetLocation<UpperElevator>().TurnsSinceSummoned.Should().Be(0);
    }

    /// <summary>
    ///     "open"/"close" are the third and fourth surfaces reading the shared flag: on the same turn the
    ///     room, the examine and the exit all say closed, "open red door" used to answer "It is open."
    /// </summary>
    [Test]
    [TestCase("blue")]
    [TestCase("red")]
    public async Task OpenDoor_CarIsAway_WontBudge(string door)
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = false;
        GetLocation<LowerElevator>().InLobby = false;

        var response = await target.GetResponse($"open {door} door");
        response.Should().Contain("It won't budge");
        response.Should().NotContain("It is open");
    }

    [Test]
    [TestCase("blue")]
    [TestCase("red")]
    public async Task CloseDoor_CarIsAway_AlreadyClosed(string door)
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = false;
        GetLocation<LowerElevator>().InLobby = false;

        var response = await target.GetResponse($"close {door} door");
        response.Should().Contain("It is closed");
        response.Should().NotContain("designed to slide shut");
    }

    [Test]
    public async Task OpenAndCloseDoor_OnlyTheBlueCarIsHere_EachDoorReportsItsOwnShaft()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetItem<LowerElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = true;
        GetLocation<LowerElevator>().InLobby = false;

        var response = await target.GetResponse("open blue door");
        response.Should().Contain("It is open");

        response = await target.GetResponse("open red door");
        response.Should().Contain("It won't budge");

        response = await target.GetResponse("close blue door");
        response.Should().Contain("designed to slide shut");

        response = await target.GetResponse("close red door");
        response.Should().Contain("It is closed");
    }

    // =============================================================================================
    // Issue #532: each shaft door was ONE Repository singleton seeded into two rooms, so the two
    // Init()s fought over its CurrentLocation and the loser could not resolve it as an object at all.
    //
    // ElevatorLobby.Map calls GetLocation<UpperElevator>() / GetLocation<LowerElevator>() to name its
    // destinations, and Repository.GetLocation lazily Inits - so merely looking at the lobby builds
    // both cars, whose Init runs last and takes ownership of both doors. From then on
    // Repository.GetItemInScope rejects the door in the lobby (IsItemAccessible tests
    // item.CurrentLocation == context.CurrentLocation), and the far-end rooms never seeded it at all.
    //
    // The verbs #505 taught the lobby to intercept - examine / open / close - masked this, because
    // they never reach scope resolution. The force verbs and the far-end rooms were left exposed.
    // =============================================================================================

    /// <summary>
    ///     Both lobby doors have to resolve as objects standing in the lobby. Asserting on the resolved
    ///     door's own state rather than its type is what catches the load-order variant of this bug,
    ///     where "blue door" silently resolved to the RED door because the bare noun "door" is contained
    ///     in "blue door" and the wrong object was the only accessible one.
    /// </summary>
    [Test]
    public async Task DoorNouns_FromTheLobby_ResolveToADoorStandingInTheLobby()
    {
        var target = GetTarget();
        var lobby = StartHere<ElevatorLobby>();
        // One look is the whole setup. ElevatorLobby.Map names both cars, Repository.GetLocation Inits
        // on first request, and each car's Init used to seize the lobby's door - so simply standing here
        // and looking around was enough to take both doors out of scope.
        await target.GetResponse("look");
        // Distinct states, so a transposition cannot hide behind two matching answers.
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetItem<LowerElevatorDoor>().IsOpen = false;
        GetLocation<UpperElevator>().InLobby = true;
        GetLocation<LowerElevator>().InLobby = true;

        var blue = Repository.GetItemInScope("blue door", Context);
        var red = Repository.GetItemInScope("red door", Context);

        blue.Should().NotBeNull("the blue door is described by this room, so it must resolve here (#532)");
        red.Should().NotBeNull("the red door is described by this room, so it must resolve here (#532)");
        blue!.CurrentLocation.Should().BeSameAs(lobby);
        red!.CurrentLocation.Should().BeSameAs(lobby);
        blue.Should().NotBeSameAs(red);
        ((IDoor)blue).IsOpen.Should().BeTrue("\"blue door\" must resolve to the upper shaft's door");
        ((IDoor)red).IsOpen.Should().BeFalse("\"red door\" must resolve to the lower shaft's door");
    }

    /// <summary>
    ///     What the lobby's player actually loses when the door falls out of scope: "enter blue door" is
    ///     the phrasing the room's own description invites, and DoorReroute (#262) turns it into the walk
    ///     north - but only if the noun resolves to the item the map declares as that exit's GatingItem.
    /// </summary>
    [Test]
    public async Task EnterDoor_FromTheLobby_CarIsHere_WalksIntoTheCar()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        await target.GetResponse("look");
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = true;

        var response = await target.GetResponse("enter blue door");

        response.Should().Contain("Upper Elevator");
        Context.CurrentLocation.Should().BeOfType<UpperElevator>();
    }

    [Test]
    public async Task EnterDoor_FromTheLobby_CarIsAway_RefusesAndStaysPut()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        await target.GetResponse("look");
        // The shared flag left open by an arrival at the FAR end - the lobby's door is really closed.
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = false;

        var response = await target.GetResponse("enter blue door");

        response.Should().Contain("The door is closed");
        Context.CurrentLocation.Should().BeOfType<ElevatorLobby>();
    }

    [Test]
    public async Task EnterDoor_AtTheTowerCore_CarIsHere_WalksIntoTheCar()
    {
        var target = GetTarget();
        StartHere<TowerCore>();
        GetLocation<UpperElevator>().InLobby = false;
        GetItem<UpperElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("enter door");

        response.Should().Contain("Upper Elevator");
        Context.CurrentLocation.Should().BeOfType<UpperElevator>();
    }

    [Test]
    public async Task EnterDoor_AtTheWaitingArea_CarIsHere_WalksIntoTheCar()
    {
        var target = GetTarget();
        StartHere<WaitingArea>();
        GetLocation<LowerElevator>().InLobby = false;
        GetItem<LowerElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("enter door");

        response.Should().Contain("Lower Elevator");
        Context.CurrentLocation.Should().BeOfType<LowerElevator>();
    }

    /// <summary>
    ///     The far ends of the two shafts both name a door in their own description ("A sliding door
    ///     leads north", "to the south is a metal door") and never seeded one, so nothing there could
    ///     resolve "door" at all.
    /// </summary>
    [Test]
    public void DoorNoun_AtTheTowerCore_ResolvesToADoorStandingThere()
    {
        GetTarget();
        var towerCore = StartHere<TowerCore>();

        var door = Repository.GetItemInScope("door", Context);

        door.Should().NotBeNull("the Tower Core's description names a sliding door (#532)");
        door!.CurrentLocation.Should().BeSameAs(towerCore);
    }

    [Test]
    public void DoorNoun_AtTheWaitingArea_ResolvesToADoorStandingThere()
    {
        GetTarget();
        var waitingArea = StartHere<WaitingArea>();

        var door = Repository.GetItemInScope("door", Context);

        door.Should().NotBeNull("the Waiting Area's description names a metal door (#532)");
        door!.CurrentLocation.Should().BeSameAs(waitingArea);
    }

    /// <summary>
    ///     And having resolved, the far-end door must answer from the far end's vantage point - open only
    ///     when the car is standing at THIS end - exactly as the entrance in the same room already does.
    /// </summary>
    [Test]
    public async Task ExamineDoor_AtTheTowerCore_CarIsHere_ReportsOpen()
    {
        var target = GetTarget();
        StartHere<TowerCore>();
        GetLocation<UpperElevator>().InLobby = false;
        GetItem<UpperElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("examine door");

        response.Should().Contain("The door is open");
    }

    [Test]
    public async Task ExamineDoor_AtTheTowerCore_CarIsDownAtTheLobby_ReportsClosed()
    {
        var target = GetTarget();
        StartHere<TowerCore>();
        // The shared flag is left open by the car's arrival at the LOBBY end - the exact state that
        // makes a raw-flag reading contradict the exit standing right beside it.
        GetLocation<UpperElevator>().InLobby = true;
        GetItem<UpperElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("examine door");

        response.Should().Contain("The door is closed");
        response.Should().NotContain("The door is open");
    }

    [Test]
    public async Task ExamineDoor_AtTheWaitingArea_CarIsHere_ReportsOpen()
    {
        var target = GetTarget();
        StartHere<WaitingArea>();
        GetLocation<LowerElevator>().InLobby = false;
        GetItem<LowerElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("examine door");

        response.Should().Contain("The door is open");
    }

    [Test]
    public async Task ExamineDoor_AtTheWaitingArea_CarIsUpAtTheLobby_ReportsClosed()
    {
        var target = GetTarget();
        StartHere<WaitingArea>();
        GetLocation<LowerElevator>().InLobby = true;
        GetItem<LowerElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("examine door");

        response.Should().Contain("The door is closed");
        response.Should().NotContain("The door is open");
    }

    [Test]
    public async Task OpenDoor_AtTheTowerCore_CarIsDownAtTheLobby_WontBudge()
    {
        var target = GetTarget();
        StartHere<TowerCore>();
        GetLocation<UpperElevator>().InLobby = true;
        GetItem<UpperElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("open door");

        response.Should().Contain("It won\'t budge");
        response.Should().NotContain("It is open");
    }

    [Test]
    public async Task OpenDoor_AtTheWaitingArea_CarIsUpAtTheLobby_WontBudge()
    {
        var target = GetTarget();
        StartHere<WaitingArea>();
        GetLocation<LowerElevator>().InLobby = true;
        GetItem<LowerElevatorDoor>().IsOpen = true;

        var response = await target.GetResponse("open door");

        response.Should().Contain("It won\'t budge");
        response.Should().NotContain("It is open");
    }

    /// <summary>
    ///     Each far-end door answers to the noun its own room's description uses - "a sliding door leads
    ///     north" at the Tower Core, "to the south is a metal door" at the Waiting Area - not just the
    ///     bare "door" the other tests here reach for.
    /// </summary>
    [Test]
    public async Task ExamineDoor_AtTheFarEnd_AnswersToTheNounTheRoomDescriptionUses()
    {
        var target = GetTarget();

        StartHere<TowerCore>();
        GetLocation<UpperElevator>().InLobby = false;
        GetItem<UpperElevatorDoor>().IsOpen = true;
        (await target.GetResponse("examine sliding door")).Should().Contain("The door is open");

        StartHere<WaitingArea>();
        GetLocation<LowerElevator>().InLobby = true;
        GetItem<LowerElevatorDoor>().IsOpen = true;
        (await target.GetResponse("examine metal door")).Should().Contain("The door is closed");
    }

    /// <summary>
    ///     A door in the car, a door in the lobby and a door at the far end are three objects, but one
    ///     shaft and therefore one open/closed state. Whichever object the player reaches, the state they
    ///     read is the shaft's.
    /// </summary>
    [Test]
    public async Task ShaftDoorState_IsSharedByEveryRoomThatSeesIt_Upper()
    {
        var target = GetTarget();
        StartHere<TowerCore>();
        GetLocation<UpperElevator>().InLobby = false;
        GetItem<UpperElevatorDoor>().IsOpen = true;

        (await target.GetResponse("examine door")).Should().Contain("The door is open");

        // Ride nothing - just shut the shaft and ask again from inside the car.
        GetItem<UpperElevatorDoor>().IsOpen = false;

        (await target.GetResponse("examine door")).Should().Contain("The door is closed");

        StartHere<UpperElevator>();
        (await target.GetResponse("examine door")).Should().Contain("The door is closed");
    }

    // =============================================================================================
    // Save-game compatibility. A location's Init() never runs again on restore - the saved Items list
    // IS the room's contents - so splitting the shaft doors changed what every in-flight session is
    // carrying. The stateless Lambda rehydrates from the session blob every turn, which makes a stale
    // blob permanent rather than merely first-turn.
    // =============================================================================================

    /// <summary>
    ///     Reproduces what a blob written before the split deserializes to: the shared shaft doors sit in
    ///     the Elevator Lobby's Items, and the landing doors were never placed anywhere. Left alone, the
    ///     lobby's verbs find the shared door - LocationBase routes over Items with no scope check - and
    ///     it reports the raw shaft flag, which is #505 again.
    /// </summary>
    private void ArrangeLobbyAsAPreSplitBlob()
    {
        var lobby = GetLocation<ElevatorLobby>();
        lobby.Items.Clear();
        lobby.ItemPlacedHere(GetItem<LowerElevatorDoor>());
        lobby.ItemPlacedHere(GetItem<UpperElevatorDoor>());
        GetLocation<TowerCore>().Items.Clear();
        GetLocation<WaitingArea>().Items.Clear();
    }

    [Test]
    public async Task AfterRestore_BlobSavedBeforeTheDoorSplit_LobbyStopsContradictingItself()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        ArrangeLobbyAsAPreSplitBlob();
        // The state a completed ride to the far end leaves behind: flag open, car away.
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = false;

        new PlanetfallGame().AfterRestore(Context);

        (await target.GetResponse("examine blue door")).Should().Contain("The door is closed");
        (await target.GetResponse("open blue door")).Should().Contain("It won\'t budge");
        (await target.GetResponse("north")).Should().Contain("The door is closed");
        Context.CurrentLocation.Should().BeOfType<ElevatorLobby>();
    }

    [Test]
    public void AfterRestore_BlobSavedBeforeTheDoorSplit_ReseatsTheLobbyDoors()
    {
        GetTarget();
        var lobby = StartHere<ElevatorLobby>();
        ArrangeLobbyAsAPreSplitBlob();

        new PlanetfallGame().AfterRestore(Context);

        lobby.Items.Should().Contain(GetItem<UpperElevatorLobbyDoor>());
        lobby.Items.Should().Contain(GetItem<LowerElevatorLobbyDoor>());
        // The shared door belongs to the car now; it must not be left answering for the lobby as well.
        lobby.Items.Should().NotContain(GetItem<UpperElevatorDoor>());
        lobby.Items.Should().NotContain(GetItem<LowerElevatorDoor>());
    }

    [Test]
    public async Task AfterRestore_BlobSavedBeforeTheDoorSplit_SeedsTheFarEndDoors()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        ArrangeLobbyAsAPreSplitBlob();

        new PlanetfallGame().AfterRestore(Context);

        GetLocation<TowerCore>().Items.Should().Contain(GetItem<UpperElevatorTowerDoor>());
        GetLocation<WaitingArea>().Items.Should().Contain(GetItem<LowerElevatorWaitingAreaDoor>());

        StartHere<TowerCore>();
        GetLocation<UpperElevator>().InLobby = false;
        GetItem<UpperElevatorDoor>().IsOpen = true;
        (await target.GetResponse("examine door")).Should().Contain("The door is open");
    }

    /// <summary>
    ///     It also runs on blobs that are already current, so it must not duplicate what is already there.
    /// </summary>
    [Test]
    public void AfterRestore_OnAnAlreadyCurrentBlob_LeavesExactlyOneDoorPerRoom()
    {
        GetTarget();
        var lobby = StartHere<ElevatorLobby>();

        new PlanetfallGame().AfterRestore(Context);
        new PlanetfallGame().AfterRestore(Context);

        lobby.Items.OfType<ElevatorDoorBase>().Should().HaveCount(2);
        GetLocation<TowerCore>().Items.OfType<ElevatorDoorBase>().Should().HaveCount(1);
        GetLocation<WaitingArea>().Items.OfType<ElevatorDoorBase>().Should().HaveCount(1);
    }

    /// <summary>
    ///     A landing door holds no state of its own, so it must not write any into the blob. The
    ///     attribute that guarantees that has to be Newtonsoft's - saves go through JsonConvert, which
    ///     ignores System.Text.Json's attribute of the same name and would round-trip the derived value
    ///     back through the write-through setter.
    /// </summary>
    [Test]
    public void LandingDoor_SerializesNoStateOfItsOwn()
    {
        GetTarget();

        var json = JsonConvert.SerializeObject(GetItem<UpperElevatorLobbyDoor>());

        json.Should().NotContain("IsOpen");
        json.Should().NotContain("HasEverBeenOpened");
    }

    /// <summary>
    ///     End to end through the real save/restore path: the shaft's state is the car door's, and it has
    ///     to survive a round trip untouched by the three landing doors that report it.
    /// </summary>
    [Test]
    public async Task SaveAndRestore_RoundTrip_PreservesTheShaftStateAndItsVantagePoints()
    {
        var target = GetTarget();
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = false;
        await target.GetResponse("look");

        target.RestoreGame(target.SaveGame());

        GetItem<UpperElevatorDoor>().IsOpen.Should().BeTrue();
        GetLocation<UpperElevator>().InLobby.Should().BeFalse();
        // ...and each vantage point still reads it correctly: closed at the lobby, open at the tower.
        StartHere<ElevatorLobby>();
        (await target.GetResponse("examine blue door")).Should().Contain("The door is closed");
        StartHere<TowerCore>();
        (await target.GetResponse("examine door")).Should().Contain("The door is open");
    }
}
