using FluentAssertions;
using Planetfall.Item.Kalamontee;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

public class ConferenceRoomTests : EngineTestsBase
{
    [Test]
    public async Task SouthFromConferenceRoom_DoorClosed()
    {
        var target = GetTarget();
        StartHere<ConferenceRoom>();

        string? response = await target.GetResponse("s");
        response.Should().Contain("The door is closed");
    }
    
    [Test]
    public async Task SouthFromConferenceRoom_DoorOpen()
    {
        var target = GetTarget();
        StartHere<ConferenceRoom>();
        GetItem<ConferenceRoomDoor>().IsOpen = true;

        string? response = await target.GetResponse("s");
        response.Should().Contain("Rec Area");
    }
    
    [Test]
    public async Task NorthFromRecArea_DoorClosed()
    {
        var target = GetTarget();
        StartHere<RecArea>();

        string? response = await target.GetResponse("n");
        response.Should().Contain("The door is closed");
    }
    
    [Test]
    public async Task NorthFromRecArea_DoorOpen()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().IsOpen = true;

        string? response = await target.GetResponse("n");
        response.Should().Contain("Conference Room");
    }
    
    [Test]
    public async Task OpenDoor_FromRecArea()
    {
        var target = GetTarget();
        StartHere<RecArea>();

        string? response = await target.GetResponse("open door");
        response.Should().Contain("The door is locked. You probably have to turn the dial to some number to open it");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeFalse();
    }
    
    [Test]
    public async Task OpenDoor_FromConferenceRoom()
    {
        var target = GetTarget();
        StartHere<ConferenceRoom>();

        string? response = await target.GetResponse("open door");
        response.Should().Contain("The door seems to be locked from the other side");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeFalse();
    }
    
    [Test]
    public async Task CloseDoor_FromConferenceRoom()
    {
        var target = GetTarget();
        StartHere<ConferenceRoom>();
        GetItem<ConferenceRoomDoor>().IsOpen = true;

        string? response = await target.GetResponse("close door");
        response.Should().Contain("The door closes and you hear a click as it locks");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeFalse();
    }
    
    [Test]
    public async Task CloseDoor_FromRecArea()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().IsOpen = true;

        string? response = await target.GetResponse("close door");
        response.Should().Contain("The door closes and you hear a click as it locks");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeFalse();
    }
    
    [Test]
    public async Task SetDial_TooHigh()
    {
        var target = GetTarget();
        StartHere<RecArea>();

        string? response = await target.GetResponse("set dial to 5651");
        response.Should().Contain("The dial does not go that high");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeFalse();
    }
    
    [Test]
    public async Task SetDial_RandomNumber()
    {
        var target = GetTarget();
        StartHere<RecArea>();

        string? response = await target.GetResponse("set dial to 12");
        response.Should().Contain("The dial is now set to 12");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeFalse();
    }
    
    [Test]
    public async Task SetDial_NegativeNumber()
    {
        var target = GetTarget();
        StartHere<RecArea>();

        string? response = await target.GetResponse("set dial to -12");
        response.Should().Contain("The numbers on the dial do not go below zero");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeFalse();
    }
    
    [Test]
    public async Task SetDial_NotANumber()
    {
        var target = GetTarget();
        StartHere<RecArea>();

        string? response = await target.GetResponse("set dial to goo");
        response.Should().Contain("The dial can only be set to numbers");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeFalse();
    }
    
    [Test]
    public async Task SetDial_CorrectCode()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "12";

        string? response = await target.GetResponse("set dial to 12");
        response.Should().Contain("The door swings open, and the dial resets to 0");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeTrue();
        GetItem<ConferenceRoomDoor>().Code.Should().Be("0");
    }
    
    [Test]
    public async Task SetDial()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "12";

        string? response = await target.GetResponse("set dial");
        response.Should().Contain("You must");
    }
}