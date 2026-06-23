using FluentAssertions;
using GameEngine;
using GameEngine.IntentEngine;
using NUnit.Framework;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Shuttle;

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
        public async Task ThenAnswerBlue_WithBothDoorsOpen_EntersTheUpperElevator()
        {
            // Both doors open => both elevators are passable, so "enter elevator" still asks which one;
            // answering "blue" then walks into the upper car. (With only one door open the resolver now
            // prefers the single reachable car and skips the question — see PrefersThePresentNeighbour.)
            var target = GetTarget();
            StartHere<ElevatorLobby>();
            GetItem<UpperElevatorDoor>().IsOpen = true;
            GetItem<LowerElevatorDoor>().IsOpen = true;
            GetLocation<UpperElevator>().InLobby = true;

            await target.GetResponse("enter elevator");
            await target.GetResponse("blue");

            target.Context.CurrentLocation.Should().BeOfType<UpperElevator>();
        }

        [Test]
        public async Task PrefersThePresentNeighbour_WhenOnlyOneElevatorIsOpen()
        {
            // Only the upper door is open => only the upper car is currently reachable, so "enter
            // elevator" should go straight there without asking which one.
            var target = GetTarget();
            StartHere<ElevatorLobby>();
            GetItem<UpperElevatorDoor>().IsOpen = true;
            GetLocation<UpperElevator>().InLobby = true;

            var response = await target.GetResponse("enter elevator");

            response.Should().NotContain("Which do you mean");
            target.Context.CurrentLocation.Should().BeOfType<UpperElevator>();
        }
    }

    /// <summary>
    /// The headline acceptance: a player can name a room by a natural short form that is a word of its
    /// title — "enter the mess", "go to the kitchen", "walk into the shuttle" — and travel there. These
    /// resolve deterministically via word-matching; colloquial aliases not in the title (galley, train)
    /// are resolved by the AI parser using the reachable-rooms list it is now given.
    /// </summary>
    [TestFixture]
    public class NaturalRoomNames : EngineTestsBase
    {
        [Test]
        public async Task EnterTheMess_FromTheCorridor_WalksToTheMessHall()
        {
            var target = GetTarget();
            StartHere<MessCorridor>();

            await target.GetResponse("enter the mess");

            target.Context.CurrentLocation.Should().BeOfType<MessHall>();
        }

        [Test]
        public async Task GoToTheKitchen_FromTheMessHall_WalksThere_WhenDoorOpen()
        {
            var target = GetTarget();
            StartHere<MessHall>();
            GetItem<KitchenDoor>().IsOpen = true;

            await target.GetResponse("go to the kitchen");

            target.Context.CurrentLocation.Should().BeOfType<Kitchen>();
        }

        [Test]
        public async Task GoToTheGalley_AfterAiNormalizesItToKitchen_WalksToTheKitchen()
        {
            // Documents the CONTRACT, not the AI: the live AI normalizes a non-title colloquialism
            // ("galley" -> "kitchen") in the prompt (untestable deterministically); the base-mapping
            // stubs that step, so this asserts only the engine's half — that the normalized destination
            // then word-matches and walks to the Kitchen.
            var target = GetTarget();
            StartHere<MessHall>();
            GetItem<KitchenDoor>().IsOpen = true;

            await target.GetResponse("go to the galley");

            target.Context.CurrentLocation.Should().BeOfType<Kitchen>();
        }

        [Test]
        public async Task GoToTheKitchen_Blocked_WhenDoorClosed_GivesTheGatedMessage_AndStaysPut()
        {
            var target = GetTarget();
            StartHere<MessHall>();
            GetItem<KitchenDoor>().IsOpen = false;

            var response = await target.GetResponse("go to the kitchen");

            response.Should().Contain("The door is closed.");
            target.Context.CurrentLocation.Should().BeOfType<MessHall>();
        }

        [Test]
        public async Task WalkIntoTheShuttle_WhenOnlyOneCarIsPresent_EntersThatCar()
        {
            var target = GetTarget();
            StartHere<KalamonteePlatform>();
            // Betty's car at the platform, Alfie's sent down the tunnel: "the shuttle" should mean the
            // one that's actually here, not ask.
            GetLocation<BettyControlEast>().TunnelPosition = 0;
            GetLocation<AlfieControlEast>().TunnelPosition = 190;

            var response = await target.GetResponse("walk into the shuttle");

            response.Should().NotContain("Which do you mean");
            target.Context.CurrentLocation.Should().BeOfType<ShuttleCarBetty>();
        }

        [Test]
        public async Task EnterTheTrain_ResolvesViaTheNonTitleSynonym()
        {
            // "train"/"transport" aren't in the title "Shuttle Car Alfie" — they're curated synonyms on
            // ShuttleCabin. Default: only Alfie is at the Kalamontee platform.
            var target = GetTarget();
            StartHere<KalamonteePlatform>();
            GetLocation<AlfieControlEast>().TunnelPosition = 0;
            GetLocation<BettyControlEast>().TunnelPosition = 24;

            await target.GetResponse("enter the train");

            target.Context.CurrentLocation.Should().BeOfType<ShuttleCarAlfie>();
        }

        [Test]
        public async Task GoToAlfie_ByName_EntersAlfiesCar()
        {
            var target = GetTarget();
            StartHere<KalamonteePlatform>();
            GetLocation<AlfieControlEast>().TunnelPosition = 0;
            GetLocation<BettyControlEast>().TunnelPosition = 24;

            await target.GetResponse("go to alfie");

            target.Context.CurrentLocation.Should().BeOfType<ShuttleCarAlfie>();
        }

        [Test]
        public async Task EnterTheShuttle_WhenBothCarsPresent_AsksWhichOne()
        {
            var target = GetTarget();
            StartHere<KalamonteePlatform>();
            // Bring both cars to the platform, so "the shuttle" is genuinely ambiguous.
            GetLocation<BettyControlEast>().TunnelPosition = 0;
            GetLocation<AlfieControlEast>().TunnelPosition = 0;

            var response = await target.GetResponse("enter the shuttle");

            response.Should().Contain("Which do you mean");
            response.Should().Contain("Shuttle Car Betty");
            response.Should().Contain("Shuttle Car Alfie");
            target.Context.CurrentLocation.Should().BeOfType<KalamonteePlatform>();
        }

        [Test]
        public async Task EnterTheShuttle_ThenAnswerBetty_EntersBettysCar()
        {
            var target = GetTarget();
            StartHere<KalamonteePlatform>();
            GetLocation<BettyControlEast>().TunnelPosition = 0;
            GetLocation<AlfieControlEast>().TunnelPosition = 0;

            await target.GetResponse("enter the shuttle");
            await target.GetResponse("betty");

            target.Context.CurrentLocation.Should().BeOfType<ShuttleCarBetty>();
        }
    }
}
