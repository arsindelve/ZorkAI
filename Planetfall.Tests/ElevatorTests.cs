using FluentAssertions;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;

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
        GetItem<UpperElevatorDoor>().IsOpen = true;

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
}