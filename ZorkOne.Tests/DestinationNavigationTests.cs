using FluentAssertions;
using GameEngine;
using GameEngine.IntentEngine;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Moq;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests;

/// <summary>
/// Issue #268 — destination-based navigation. The player names a ROOM and, if it is a direct exit of
/// the room they are standing in, the engine walks there in one move (reusing all the normal movement
/// gating). Naming an unreachable room is refused, not teleported.
/// </summary>
public class DestinationNavigationTests : EngineTestsBase
{
    [TestFixture]
    public class TheResolver : EngineTestsBase
    {
        [Test]
        public void ResolveAllAdjacent_FindsTheAdjacentRoomByName()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var matches = DestinationNavigation.ResolveAllAdjacent("the kitchen", target.Context);

            matches.Should().HaveCount(1);
            matches[0].Room.Should().BeOfType<Kitchen>();
        }

        [Test]
        public void ResolveAllAdjacent_DedupesARoomReachableViaTwoExits()
        {
            // Behind House reaches the Kitchen via BOTH "west" and "in". That is one logical
            // destination, so it must resolve to a single match — never a false "which one?".
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var matches = DestinationNavigation.ResolveAllAdjacent("kitchen", target.Context);

            matches.Should().HaveCount(1);
        }

        [Test]
        public void ResolveAllAdjacent_MatchesEvenWhenTheExitIsGated()
        {
            // A closed window still "leads to" the kitchen; we match it so MoveEngine can later report
            // the closed-window message rather than the resolver pretending the room isn't there.
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = false;

            var matches = DestinationNavigation.ResolveAllAdjacent("kitchen", target.Context);

            matches.Should().HaveCount(1);
            matches[0].Room.Should().BeOfType<Kitchen>();
        }

        [Test]
        public void ResolveAllAdjacent_ReturnsNothing_ForANonAdjacentRoom()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var matches = DestinationNavigation.ResolveAllAdjacent("reactor", target.Context);

            matches.Should().BeEmpty();
        }
    }

    [TestFixture]
    public class GoToNamedRoom : EngineTestsBase
    {
        [Test]
        public async Task GoToTheKitchen_WalksThere_WhenWindowOpen()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            await target.GetResponse("go to the kitchen");

            target.Context.CurrentLocation.Should().BeOfType<Kitchen>();
        }

        [Test]
        public async Task GoToTheKitchen_Blocked_WhenWindowClosed_GivesTheGatedMessage_AndStaysPut()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = false;

            var response = await target.GetResponse("go to the kitchen");

            response.Should().Contain("The kitchen window is closed.");
            target.Context.CurrentLocation.Should().BeOfType<BehindHouse>();
        }

        [Test]
        public async Task EnterTheKitchen_WalksThere_WhenWindowOpen()
        {
            // Issue #268 flagship example: "enter the kitchen" (a room name, not an item in scope)
            // routes through the EnterSubLocationEngine fallback into destination navigation.
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            await target.GetResponse("enter the kitchen");

            target.Context.CurrentLocation.Should().BeOfType<Kitchen>();
        }
    }

    [TestFixture]
    public class WhenTheRoomCannotBeReached : EngineTestsBase
    {
        [Test]
        public async Task TheEngineRefuses_AndDoesNotMove()
        {
            // Direct engine test with generation disabled so the refusal is the deterministic canned
            // line rather than an AI narration.
            Repository.Reset();
            var context = new ZorkIContext { CurrentLocation = Repository.GetLocation<BehindHouse>() };
            var disabledClient = new Mock<IGenerationClient>();
            disabledClient.Setup(c => c.IsDisabled).Returns(true);

            var engine = new DestinationNavigationEngine();
            var result = await engine.Process(
                new GoToDestinationIntent { Destination = "reactor" }, context, disabledClient.Object);

            result.ResultMessage.Should().Be("You can't get there from here. ");
            context.CurrentLocation.Should().BeOfType<BehindHouse>();
        }
    }
}
