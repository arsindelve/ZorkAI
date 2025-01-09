using FluentAssertions;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

public class ElevatorTests : EngineTestsBase
{
    
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
}