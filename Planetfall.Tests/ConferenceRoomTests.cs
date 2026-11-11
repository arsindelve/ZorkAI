using FluentAssertions;
using Model.Interface;
using Moq;
using Planetfall;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
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
    public void NowOpen_DoesNotThrow_AndReturnsAMessage()
    {
        // Regression for #225: NowOpen() used to throw NotImplementedException. Even though the
        // CannotBeOpenedDescription guard normally shields it from the "open" verb, the throw is a
        // landmine that crashes the turn if that guard ever stops returning a reason. NowOpen must
        // return a safe message, never throw.
        var door = GetItem<ConferenceRoomDoor>();
        var location = GetLocation<RecArea>();

        var act = () => door.NowOpen(location);

        act.Should().NotThrow();
        door.NowOpen(location).Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task OpenConferenceDoor_ReturnsMessage_AndDoesNotThrow()
    {
        // Regression for #225: "open conference door" must return a message and never crash the turn.
        var target = GetTarget();
        StartHere<RecArea>();

        Func<Task> act = async () => await target.GetResponse("open conference door");

        await act.Should().NotThrowAsync();
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

        // Pin the unlock code to 999 so that dialing 12 can never accidentally open the door.
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDice(999)).Returns(999);
        GetItem<ConferenceRoomDoor>().Chooser = mockChooser.Object;

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

        // Dial away from its initial 0 first: dialing the setting it already shows is a no-op with its
        // own response (issue #501), so this asserts "zero" parses as a valid *new* setting.
        await target.GetResponse("set dial to 42");

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

    #region Floyd Comment Tests

    [Test]
    public async Task SetDial_CorrectCode_FloydCommentsWhenPresent()
    {
        var target = GetTarget();
        var recArea = StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "42";

        // Set up Floyd as present and active
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = recArea;
        recArea.ItemPlacedHere(floyd);

        await target.GetResponse("set dial to 42");

        // Verify Floyd's pending comment was set
        var pfContext = (PlanetfallContext)target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().NotBeNull();
        pfContext.PendingFloydActionCommentPrompt.Should().Contain("conference");
    }

    [Test]
    public async Task SetDial_CorrectCode_FloydDoesNotComment_WhenNotPresent()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "42";

        // Floyd is NOT in the room
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = GetLocation<ConferenceRoom>(); // Different location

        await target.GetResponse("set dial to 42");

        // Verify Floyd's pending comment was NOT set
        var pfContext = (PlanetfallContext)target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }

    [Test]
    public async Task SetDial_WrongCode_FloydDoesNotComment()
    {
        var target = GetTarget();
        var recArea = StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "42";

        // Set up Floyd as present and active
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = recArea;
        recArea.ItemPlacedHere(floyd);

        await target.GetResponse("set dial to 99"); // Wrong code

        // Verify Floyd's pending comment was NOT set (door didn't open)
        var pfContext = (PlanetfallContext)target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }

    #endregion

    #region Re-dialing The Current Setting

    [Test]
    public async Task SetDial_ToNumberItIsAlreadyOn_SaysThatIsWhatItIsSetToNow()
    {
        // Issue #501: the original has a dedicated branch for dialing the number the dial already
        // shows -- it must not re-report "The dial is now set to 5." as if something changed.
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "999";

        var first = await target.GetResponse("set dial to 5");
        first.Should().Contain("The dial is now set to 5");

        var second = await target.GetResponse("set dial to 5");
        second.Should().Contain("That's what the dial is set to now!");
        second.Should().NotContain("The dial is now set to 5");
        GetItem<ConferenceRoomDoor>().Code.Should().Be("5");
    }

    [Test]
    public async Task SetDial_ToZero_WhenDialStartsAtZero_SaysThatIsWhatItIsSetToNow()
    {
        // Same branch as above, for the dial's initial setting of 0.
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "999";

        string? response = await target.GetResponse("set dial to zero");

        response.Should().Contain("That's what the dial is set to now!");
        GetItem<ConferenceRoomDoor>().Code.Should().Be("0");
    }

    [Test]
    public async Task SetDial_ToCorrectCode_OpensDoor_EvenIfDialAlreadyShowsIt()
    {
        // The "already on that number" branch must never shadow the unlock check.
        var target = GetTarget();
        StartHere<RecArea>();
        var door = GetItem<ConferenceRoomDoor>();
        door.UnlockCode = "42";
        door.Code = "42";

        string? response = await target.GetResponse("set dial to 42");

        response.Should().Contain("The door swings open, and the dial resets to 0");
        door.IsOpen.Should().BeTrue();
    }

    #endregion

    #region Rec Area Description Tests

    [Test]
    public async Task RecAreaDescription_DoorOpen_DoesNotReportTheDial()
    {
        // Issue #501: in the original, the dial sentence belongs to the *closed* branch only. Once the
        // door is open the description ends at "...a door which is open." The dial is reset to 0 when
        // the door opens, so an unconditional dial clause reports a meaningless "set to 0" on a lock
        // that is already solved.
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().IsOpen = true;

        string? response = await target.GetResponse("look");

        response.Should().Contain("a door which is open");
        response.Should().NotContain("A dial on the door");
    }

    [Test]
    public async Task RecAreaDescription_DoorClosed_ReportsTheDialSetting()
    {
        // Issue #501: the closed branch must keep reporting the dial's current setting -- the fix
        // moves the clause, it must not drop it.
        var target = GetTarget();
        StartHere<RecArea>();
        var door = GetItem<ConferenceRoomDoor>();
        door.IsOpen = false;
        door.Code = "42";

        string? response = await target.GetResponse("look");

        response.Should().Contain("a door which is closed and locked");
        response.Should().Contain("A dial on the door is currently set to 42");
    }

    #endregion

    #region Examine Dial Tests

    [Test]
    public async Task ExamineDial_ShowsCurrentValue_WhenDialIsAtZero()
    {
        var target = GetTarget();
        StartHere<RecArea>();

        string? response = await target.GetResponse("examine dial");
        response.Should().Contain("The dial is set to 0");
    }

    [Test]
    public async Task ExamineDial_ShowsCurrentValue_AfterSettingDial()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "9999";

        await target.GetResponse("set dial to 42");

        string? response = await target.GetResponse("examine dial");
        response.Should().Contain("The dial is set to 42");
    }

    [Test]
    public async Task ExamineDial_ShowsCurrentValue_AfterSettingDialWithWord()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "9999";

        await target.GetResponse("set dial to seventy seven");

        string? response = await target.GetResponse("examine dial");
        response.Should().Contain("The dial is set to 77");
    }

    [Test]
    public async Task ExamineDial_ShowsZero_AfterDoorOpens()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "12";

        await target.GetResponse("set dial to 12");

        string? response = await target.GetResponse("examine dial");
        response.Should().Contain("The dial is set to 0");
    }

    [Test]
    public async Task ExamineDial_ShowsCurrentValue_WhenDoorIsOpen()
    {
        var target = GetTarget();
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().IsOpen = true;
        GetItem<ConferenceRoomDoor>().Code = "55";

        string? response = await target.GetResponse("examine dial");
        response.Should().Contain("The dial is set to 55");
    }

    #endregion
}