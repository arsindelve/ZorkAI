using FluentAssertions;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

public class ElevatorTests : EngineTestsBase
{
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
    
    // Entering from lobby when open

}