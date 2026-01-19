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

    #region Word-Based Number Input Tests

    [Test]
    [TestCase("zero", "0")]
    [TestCase("one", "1")]
    [TestCase("two", "2")]
    [TestCase("three", "3")]
    [TestCase("ten", "10")]
    [TestCase("twelve", "12")]
    [TestCase("twenty", "20")]
    [TestCase("fifty", "50")]
    [TestCase("one hundred", "100")]
    [TestCase("two hundred", "200")]
    [TestCase("five hundred", "500")]
    [TestCase("nine hundred ninety nine", "999")]
    public async Task SetDial_WordNumber_ValidNumbers(string word, string expected)
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "9999"; // Set unlock code high so door doesn't open

        string? response = await target.GetResponse($"set dial to {word}");
        response.Should().Contain($"The dial is now set to {expected}");
        GetItem<ConferenceRoomDoor>().Code.Should().Be(expected);
    }

    [Test]
    public async Task SetDial_WordNumber_CorrectUnlockCode()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "42";

        string? response = await target.GetResponse("set dial to forty two");
        response.Should().Contain("The door swings open, and the dial resets to 0");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeTrue();
        GetItem<ConferenceRoomDoor>().Code.Should().Be("0");
    }

    [Test]
    public async Task SetDial_WordNumber_CorrectUnlockCode_Alternative()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "100";

        string? response = await target.GetResponse("set dial to one hundred");
        response.Should().Contain("The door swings open, and the dial resets to 0");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeTrue();
    }

    [Test]
    [TestCase("ONE", "1")]
    [TestCase("TwElVe", "12")]
    [TestCase("FIFTY", "50")]
    [TestCase("Twenty Three", "23")]
    public async Task SetDial_WordNumber_CaseInsensitive(string word, string expected)
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "9999";

        string? response = await target.GetResponse($"set dial to {word}");
        response.Should().Contain($"The dial is now set to {expected}");
    }

    [Test]
    [TestCase("one thousand")]
    [TestCase("two thousand")]
    [TestCase("ten thousand")]
    public async Task SetDial_WordNumber_TooHigh(string word)
    {
        var target = GetTarget();
        StartHere<RecArea>();

        string? response = await target.GetResponse($"set dial to {word}");
        response.Should().Contain("The dial does not go that high");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeFalse();
    }

    [Test]
    [TestCase("hello")]
    [TestCase("world")]
    [TestCase("goo")]
    [TestCase("red")]
    public async Task SetDial_WordNumber_NonNumericWords(string word)
    {
        var target = GetTarget();
        StartHere<RecArea>();

        string? response = await target.GetResponse($"set dial to {word}");
        response.Should().Contain("The dial can only be set to numbers");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeFalse();
    }

    [Test]
    public async Task SetDial_WordNumber_MixedWithDigits()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "9999";

        // Both digit and word formats should work
        var response1 = await target.GetResponse("set dial to 15");
        response1.Should().Contain("The dial is now set to 15");

        var response2 = await target.GetResponse("set dial to twenty five");
        response2.Should().Contain("The dial is now set to 25");

        var response3 = await target.GetResponse("set dial to 100");
        response3.Should().Contain("The dial is now set to 100");

        var response4 = await target.GetResponse("set dial to three");
        response4.Should().Contain("The dial is now set to 3");
    }

    [Test]
    public async Task SetDial_WordNumber_ZeroIsValid()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "9999";

        string? response = await target.GetResponse("set dial to zero");
        response.Should().Contain("The dial is now set to 0");
        GetItem<ConferenceRoomDoor>().Code.Should().Be("0");
    }

    [Test]
    public async Task TurnDial_WordNumber_Works()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "9999";

        string? response = await target.GetResponse("turn dial to seventy seven");
        response.Should().Contain("The dial is now set to 77");
        GetItem<ConferenceRoomDoor>().Code.Should().Be("77");
    }

    [Test]
    public async Task SetDial_WordNumber_OpenDoorWithWord()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "7";

        string? response = await target.GetResponse("set dial to seven");
        response.Should().Contain("The door swings open");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeTrue();
    }

    [Test]
    public async Task SetDial_WordNumber_TeenNumbers()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "9999";

        var response = await target.GetResponse("set dial to thirteen");
        response.Should().Contain("The dial is now set to 13");

        response = await target.GetResponse("set dial to fifteen");
        response.Should().Contain("The dial is now set to 15");

        response = await target.GetResponse("set dial to nineteen");
        response.Should().Contain("The dial is now set to 19");
    }

    #endregion
}