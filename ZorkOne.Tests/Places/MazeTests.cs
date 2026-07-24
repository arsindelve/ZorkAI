using FluentAssertions;
using GameEngine;
using Model;
using UnitTests;
using ZorkOne.Item;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests.Places;

public class MazeTests : EngineTestsBase
{
    // Issue #473 (same lifecycle-override bug as the Planetfall connector rooms, found via the
    // repo-wide grep the issue suggested): MazeEleven, entered from MazeNine, returned the
    // "You won't be able to get back up..." transition text directly instead of prepending it to
    // base.BeforeEnterLocation(...). base is the only place VisitCount is incremented (and
    // OnFirstTimeEnterLocation fires); in Brief mode LookProcessor shows the full room description
    // only when VisitCount == 1, so a lit first entry via this path dropped the maze description.
    [Test]
    public async Task EnterMazeElevenFromMazeNine_FirstVisit_ShowsMazeDescriptionAndIncrementsVisitCount()
    {
        var target = GetTarget();
        target.Context.Verbosity = Verbosity.Brief;

        // The maze is dark; carry a lit lantern so the room description (not the darkness text) shows.
        var lamp = Repository.GetItem<Lantern>();
        lamp.IsOn = true;
        target.Context.Take(lamp);
        target.Context.CurrentLocation = Repository.GetLocation<MazeNine>();

        var response = await target.GetResponse("down");

        target.Context.CurrentLocation.Should().BeOfType<MazeEleven>();
        // The transition warning must accompany, not replace, the first-visit maze description.
        response.Should().Contain("You won't be able to get back up to the tunnel");
        response.Should().Contain("twisty little passages, all alike");
        Repository.GetLocation<MazeEleven>().VisitCount.Should().Be(1);
    }
}
