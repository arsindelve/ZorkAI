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

        var response = await target.GetResponse("push up button");

        response.Should().Contain("Nothing happens");
        target.Context.CurrentLocation.Should().BeOfType<ReactorElevator>();
    }

    [Test]
    public async Task PushDownButton_GoesNowhere()
    {
        var target = GetTarget();
        StartHere<ReactorElevator>();

        var response = await target.GetResponse("push down button");

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
}
