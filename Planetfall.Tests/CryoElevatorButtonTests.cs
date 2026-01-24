using FluentAssertions;
using GameEngine;
using Model;
using Model.Intent;
using Model.Interaction;
using Planetfall.Item.Lawanda.BioLab;
using Planetfall.Item.Lawanda.CryoElevator;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Tests;

public class CryoElevatorButtonTests : EngineTestsBase
{
    private CryoElevatorButton _button = null!;
    private PlanetfallContext _context = null!;

    [SetUp]
    public void Setup()
    {
        Repository.Reset();
        _context = new PlanetfallContext();
        _context.CurrentLocation = Repository.GetLocation<CryoElevatorLocation>();
        _button = Repository.GetItem<CryoElevatorButton>();

        // Ensure chase is active so button press works
        var chaseManager = Repository.GetItem<ChaseSceneManager>();
        chaseManager.ChaseActive = true;
    }

    [Test]
    public void CountdownSequence_OpensDoorsAfterTwoWaits()
    {
        // Initial state
        _button.CountdownActive.Should().BeFalse();
        _button.AlreadyArrived.Should().BeFalse();
        _button.TurnsRemaining.Should().Be(4);

        // Press button
        _button.CountdownActive = true;
        _button.TurnsRemaining = 4;
        _context.RegisterActor(_button);

        // Turn 1 (button was just pressed): Act should decrement to 3
        var result1 = _button.Act(_context, null!).Result;
        result1.Should().BeEmpty("door should not open yet");
        _button.TurnsRemaining.Should().Be(3);
        _button.AlreadyArrived.Should().BeFalse();

        // Turn 2 (first wait): Act should decrement to 2
        var result2 = _button.Act(_context, null!).Result;
        result2.Should().BeEmpty("door should not open yet");
        _button.TurnsRemaining.Should().Be(2);
        _button.AlreadyArrived.Should().BeFalse();

        // Turn 3 (second wait): Act should decrement to 1
        var result3 = _button.Act(_context, null!).Result;
        result3.Should().BeEmpty("door should not open yet");
        _button.TurnsRemaining.Should().Be(1);
        _button.AlreadyArrived.Should().BeFalse();

        // Turn 4 (third wait): Act should decrement to 0 and open door
        var result4 = _button.Act(_context, null!).Result;
        result4.Should().Contain("elevator door opens");
        _button.TurnsRemaining.Should().Be(0);
        _button.AlreadyArrived.Should().BeTrue();
    }

    [Test]
    public void PressButton_AfterArrival_ReturnsDeath()
    {
        // Simulate arrival (door already opened)
        _button.AlreadyArrived = true;
        _button.CountdownActive = false;

        // Press button again should trigger death
        var action = new SimpleIntent { Verb = "press", Noun = "button" };
        var result = _button.RespondToSimpleInteraction(action, _context, null!, null!).Result;

        result.Should().NotBeNull();
        result.Should().BeOfType<DeathInteractionResult>();
        result!.InteractionMessage.Should().Contain("Stunning");
    }

    [Test]
    public void PressButton_DuringCountdown_ReturnsNothingHappens()
    {
        // Simulate countdown in progress
        _button.CountdownActive = true;
        _button.AlreadyArrived = false;

        // Press button during countdown should return "Nothing happens"
        var action = new SimpleIntent { Verb = "press", Noun = "button" };
        var result = _button.RespondToSimpleInteraction(action, _context, null!, null!).Result;

        result.Should().NotBeNull();
        result!.InteractionMessage.Should().Contain("Nothing happens");
    }
}
