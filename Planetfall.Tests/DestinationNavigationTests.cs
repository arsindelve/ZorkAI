using FluentAssertions;
using GameEngine;
using GameEngine.IntentEngine;
using NUnit.Framework;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

/// <summary>
/// Issue #268 — destination navigation must ASK when a name matches more than one adjacent room.
/// In the Elevator Lobby, "enter elevator" matches both the Upper and Lower elevators, so the engine
/// reuses its existing disambiguation flow ("which one?") rather than silently picking one.
/// </summary>
public class DestinationNavigationTests : EngineTestsBase
{
    [TestFixture]
    public class TheResolver : EngineTestsBase
    {
        [Test]
        public void ResolveAllAdjacent_InTheLobby_MatchesBothElevators()
        {
            var target = GetTarget();
            StartHere<ElevatorLobby>();

            var matches = DestinationNavigation.ResolveAllAdjacent("elevator", target.Context);

            matches.Should().HaveCount(2);
            matches.Select(m => m.Room.GetType())
                .Should().BeEquivalentTo([typeof(UpperElevator), typeof(LowerElevator)]);
        }

        [Test]
        public void BuildDisambiguation_NamesBothElevators()
        {
            var target = GetTarget();
            StartHere<ElevatorLobby>();
            var matches = DestinationNavigation.ResolveAllAdjacent("elevator", target.Context);

            var disambiguation = DestinationNavigation.BuildDisambiguation(matches);

            disambiguation.InteractionMessage.Should().Contain("Which do you mean");
            disambiguation.InteractionMessage.Should().Contain("Upper Elevator");
            disambiguation.InteractionMessage.Should().Contain("Lower Elevator");
        }

        [Test]
        public void ResolveAllAdjacent_WithADistinguishingName_PicksOne()
        {
            // "blue" only belongs to the Upper Elevator, so it resolves to a single room — the property
            // the disambiguation follow-up relies on.
            var target = GetTarget();
            StartHere<ElevatorLobby>();

            var matches = DestinationNavigation.ResolveAllAdjacent("blue", target.Context);

            matches.Should().ContainSingle();
            matches[0].Room.Should().BeOfType<UpperElevator>();
        }
    }

    [TestFixture]
    public class EnterElevator : EngineTestsBase
    {
        [Test]
        public async Task AsksWhichOne_AndDoesNotMove()
        {
            var target = GetTarget();
            StartHere<ElevatorLobby>();

            var response = await target.GetResponse("enter elevator");

            response.Should().Contain("Which do you mean");
            response.Should().Contain("Upper Elevator");
            response.Should().Contain("Lower Elevator");
            target.Context.CurrentLocation.Should().BeOfType<ElevatorLobby>();
        }

        [Test]
        public async Task ThenAnswerBlue_WithDoorClosed_ReportsClosedDoor_AndStaysPut()
        {
            // End-to-end: the "which one?" answer flows through the existing DisambiguationProcessor,
            // re-issues "go to the upper elevator", resolves to the single (gated) exit, and MoveEngine
            // surfaces the closed-door message — no teleport.
            var target = GetTarget();
            StartHere<ElevatorLobby>();

            await target.GetResponse("enter elevator");
            var response = await target.GetResponse("blue");

            response.Should().Contain("The door is closed.");
            target.Context.CurrentLocation.Should().BeOfType<ElevatorLobby>();
        }

        [Test]
        public async Task ThenAnswerBlue_WithDoorOpen_EntersTheUpperElevator()
        {
            var target = GetTarget();
            StartHere<ElevatorLobby>();
            GetItem<UpperElevatorDoor>().IsOpen = true;
            GetLocation<UpperElevator>().InLobby = true;

            await target.GetResponse("enter elevator");
            await target.GetResponse("blue");

            target.Context.CurrentLocation.Should().BeOfType<UpperElevator>();
        }
    }
}
