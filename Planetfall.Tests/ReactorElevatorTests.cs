using FluentAssertions;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Tests;

/// <summary>
///     Tests for the Reactor Elevator off Reactor Control (compone.zil, REACTOR-ELEVATOR). It is a
///     flavor dead-end: the door starts open but can't be operated, and the Up/Down buttons go
///     nowhere because the original movement table is all zeros.
/// </summary>
public class ReactorElevatorTests : EngineTestsBase
{
    [Test]
    public async Task NavigateToReactorElevator_EastFromReactorControl()
    {
        var target = GetTarget();
        StartHere<ReactorControl>();

        var response = await target.GetResponse("east");

        response.Should().Contain("Reactor Elevator");
        target.Context.CurrentLocation.Should().BeOfType<ReactorElevator>();
    }

    [Test]
    public async Task NavigateToReactorElevator_InFromReactorControl()
    {
        var target = GetTarget();
        StartHere<ReactorControl>();

        var response = await target.GetResponse("in");

        response.Should().Contain("Reactor Elevator");
        target.Context.CurrentLocation.Should().BeOfType<ReactorElevator>();
    }

    [Test]
    public async Task NavigateBackToReactorControl_WestFromElevator()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();

        var response = await target.GetResponse("west");

        response.Should().Contain("Reactor Control");
        target.Context.CurrentLocation.Should().BeOfType<ReactorControl>();
    }

    [Test]
    public async Task NavigateBackToReactorControl_OutFromElevator()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();

        var response = await target.GetResponse("out");

        response.Should().Contain("Reactor Control");
        target.Context.CurrentLocation.Should().BeOfType<ReactorControl>();
    }

    [Test]
    public async Task ReactorElevator_HasExpectedDescription()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();

        var response = await target.GetResponse("look");

        response.Should().Contain("This is an elevator with a door to the west, currently open");
        response.Should().Contain("an Up button, a Down button, and a small slot");
    }

    [Test]
    public async Task PushUpButton_GoesNowhere()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();

        var response = await target.GetResponse("press up button");

        response.Should().Contain("Nothing happens");
        target.Context.CurrentLocation.Should().BeOfType<ReactorElevator>();
    }

    [Test]
    public async Task PushDownButton_GoesNowhere()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();

        var response = await target.GetResponse("press down button");

        response.Should().Contain("Nothing happens");
        target.Context.CurrentLocation.Should().BeOfType<ReactorElevator>();
    }

    [Test]
    public async Task PushButton_Ambiguous_AsksWhichButton()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();

        var response = await target.GetResponse("push button");

        response.Should().Contain("Up button");
        response.Should().Contain("Down button");
    }

    [Test]
    public async Task ExamineSlot_DescribesSlot()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();

        var response = await target.GetResponse("examine slot");

        response.Should().Contain("small slot");
    }

    [Test]
    public async Task OpenDoor_WontBudge_BecauseAlreadyOpen()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();

        var response = await target.GetResponse("open door");

        response.Should().Contain("It is already open");
    }

    [Test]
    public async Task CloseDoor_PlayerCannotCloseItThemselves()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();

        var response = await target.GetResponse("close door");

        response.Should().Contain("You can't close it yourself");
        // The door must remain open so the player isn't trapped in the dead-end elevator.
        GetItem<ReactorElevatorDoor>().IsOpen.Should().BeTrue();
    }

    [Test]
    public async Task ReactorElevatorDoor_IsVisibleFromReactorControl()
    {
        var target = GetTarget();
        StartHere<ReactorControl>();

        var response = await target.GetResponse("examine door");

        response.Should().Contain("The door is open");
    }

    [Test]
    public void ReactorElevatorDoor_StartsOpen()
    {
        GetTarget();
        StartHere<ReactorControl>();

        GetItem<ReactorElevatorDoor>().IsOpen.Should().BeTrue();
    }

    /// <summary>
    ///     Issue #523. The room description hardcoded "currently open" while both of the room's exits
    ///     gate on the door's actual state, so a closed door made the room advertise a way out that the
    ///     player could not take. Same shape as #450 / #505 / #512 / #518: a state word copied into a
    ///     literal beside a Map that reads the real thing.
    /// </summary>
    [Test]
    public async Task Look_DoorClosed_DescribesTheDoorAsClosed()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();
        GetItem<ReactorElevatorDoor>().IsOpen = false;

        var response = await target.GetResponse("look");

        response.Should().Contain("door to the west, currently closed");
        response.Should().NotContain("currently open");
    }

    /// <summary>
    ///     The invariant the room has to keep, in both door states: whatever word the description uses,
    ///     walking west must agree with it. This is the generalised form of the lobby's
    ///     Look_BlueDoorWording_AlwaysAgreesWithWhetherNorthIsPassable.
    /// </summary>
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Look_DoorWording_AlwaysAgreesWithWhetherWestIsPassable(bool isOpen)
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();
        GetItem<ReactorElevatorDoor>().IsOpen = isOpen;

        var look = await target.GetResponse("look");
        var describedAsOpen = look!.Contains("door to the west, currently open");
        look.Should().Contain(describedAsOpen ? "currently open" : "currently closed");

        var move = await target.GetResponse("west");

        move!.Contains("Reactor Control").Should().Be(describedAsOpen);
    }

    /// <summary>
    ///     "examine door" is the natural way to double-check the room description, so the two must never
    ///     disagree - that was consequence two of #505 in the Elevator Lobby.
    /// </summary>
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task ExamineDoor_AgreesWithTheRoomDescription(bool isOpen)
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();
        GetItem<ReactorElevatorDoor>().IsOpen = isOpen;

        var look = await target.GetResponse("look");
        var examine = await target.GetResponse("examine door");

        look!.Contains("currently open").Should().Be(examine!.Contains("The door is open"));
        look.Contains("currently closed").Should().Be(examine.Contains("The door is closed"));
    }

    /// <summary>
    ///     Stating the door as a <see cref="Doorway" /> declares it as the GatingItem of the passage it
    ///     gates, which is what lets "enter/exit door" resolve to that exit (DoorReroute, issue #262).
    ///     Both sides of this door had hand-rolled MovementParameters that omitted it.
    /// </summary>
    [Test]
    public async Task EnterDoor_FromReactorControl_WalksIntoTheCar()
    {
        var target = GetTarget();
        StartHere<ReactorControl>();

        var response = await target.GetResponse("enter door");

        response.Should().Contain("Reactor Elevator");
        target.Context.CurrentLocation.Should().BeOfType<ReactorElevator>();
    }

    [Test]
    public async Task ExitDoor_FromTheCar_WalksBackToReactorControl()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();

        var response = await target.GetResponse("exit door");

        response.Should().Contain("Reactor Control");
        target.Context.CurrentLocation.Should().BeOfType<ReactorControl>();
    }

    /// <summary>
    ///     The far side of the same door. Reactor Control's description makes no claim about the door's
    ///     state, so nothing there can drift - but the exit still gates on it, and "examine door" has to
    ///     agree with whether east is passable.
    /// </summary>
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task FromReactorControl_ExamineDoor_AgreesWithWhetherEastIsPassable(bool isOpen)
    {
        var target = GetTarget();
        StartHere<ReactorControl>();
        GetItem<ReactorElevatorDoor>().IsOpen = isOpen;

        var examine = await target.GetResponse("examine door");
        var describedAsOpen = examine!.Contains("The door is open");

        var move = await target.GetResponse("east");

        move!.Contains("Reactor Elevator").Should().Be(describedAsOpen);
    }
}
