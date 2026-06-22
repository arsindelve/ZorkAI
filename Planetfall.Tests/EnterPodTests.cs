using FluentAssertions;
using GameEngine;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

/// <summary>
/// Regression tests for issue #262: at Deck Nine, "enter pod" / "board pod" name a perfectly valid
/// noun (BulkheadDoor.NounsForMatching includes "pod"), but the bare noun used to fall through
/// EnterSubLocationEngine to a generic refusal — which the narrator dressed up as a mock of an
/// imaginary object — instead of the correct "The escape pod bulkhead is closed." The escape pod is
/// reached by moving through the BulkheadDoor, so "enter &lt;door&gt;" must defer to movement
/// (Direction.In) exactly as the full phrase "enter escape pod" (already a Move) does.
/// </summary>
public class EnterPodTests : EngineTestsBase
{
    [TestFixture]
    public class BulkheadClosed : EngineTestsBase
    {
        [Test]
        public async Task EnterPod_SaysBulkheadIsClosed_AndStaysOnDeckNine()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

            var response = await target.GetResponse("enter pod");

            response.Should().Contain("The escape pod bulkhead is closed.");
            target.Context.CurrentLocation.Should().BeOfType<DeckNine>();
        }

        [Test]
        public async Task BoardPod_SaysBulkheadIsClosed()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

            var response = await target.GetResponse("board pod");

            response.Should().Contain("The escape pod bulkhead is closed.");
            target.Context.CurrentLocation.Should().BeOfType<DeckNine>();
        }

        [Test]
        public async Task EnterBulkhead_SaysBulkheadIsClosed()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

            var response = await target.GetResponse("enter bulkhead");

            response.Should().Contain("The escape pod bulkhead is closed.");
            target.Context.CurrentLocation.Should().BeOfType<DeckNine>();
        }

        [Test]
        public async Task EnterDoor_SaysBulkheadIsClosed()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

            var response = await target.GetResponse("enter door");

            response.Should().Contain("The escape pod bulkhead is closed.");
            target.Context.CurrentLocation.Should().BeOfType<DeckNine>();
        }

        [Test]
        public async Task EnterPod_DoesNotGenericallyRefuseAValidNoun()
        {
            // The whole point of #262: a valid noun must not get the generic movement refusal that
            // the narrator turns into a mock. We get the door's specific message instead.
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

            var response = await target.GetResponse("enter pod");

            response.Should().NotContain("cannot go that way");
            response.Should().NotContain("can't enter that");
        }

        [Test]
        public async Task EnterPod_GivesSameMessageAsGoingWest()
        {
            // "enter pod" (the bare valid noun) should behave exactly like the directional move that
            // goes through the bulkhead. Two fresh games so state can't leak between the commands.
            var west = GetTarget();
            west.Context.CurrentLocation = Repository.GetLocation<DeckNine>();
            var westResponse = await west.GetResponse("west");

            var enter = GetTarget();
            enter.Context.CurrentLocation = Repository.GetLocation<DeckNine>();
            var enterResponse = await enter.GetResponse("enter pod");

            westResponse.Should().Contain("The escape pod bulkhead is closed.");
            enterResponse.Should().Contain("The escape pod bulkhead is closed.");
        }
    }

    [TestFixture]
    public class BulkheadOpen : EngineTestsBase
    {
        [Test]
        public async Task EnterPod_AfterExplosionOpensBulkhead_ActuallyEntersThePod()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

            // Wait out the explosion countdown; on turn 10 the blast blows the pod bulkhead open.
            for (var i = 0; i < 9; i++)
                await target.GetResponse("wait");

            var explosion = await target.GetResponse("wait");
            explosion.Should().Contain("door to port slides open");
            Repository.GetItem<BulkheadDoor>().IsOpen.Should().BeTrue();

            // Now the bare noun "pod" should carry us through the open bulkhead and into the pod,
            // exactly like "west" would.
            var response = await target.GetResponse("enter pod");

            target.Context.CurrentLocation.Should().BeOfType<EscapePod>();
            response.Should().Contain("The ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing.");
        }

        [Test]
        public async Task BoardPod_AfterExplosionOpensBulkhead_ActuallyEntersThePod()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

            for (var i = 0; i < 9; i++)
                await target.GetResponse("wait");

            await target.GetResponse("wait"); // explosion opens the bulkhead
            Repository.GetItem<BulkheadDoor>().IsOpen.Should().BeTrue();

            await target.GetResponse("board pod");

            target.Context.CurrentLocation.Should().BeOfType<EscapePod>();
        }
    }
}
