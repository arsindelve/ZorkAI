using FluentAssertions;
using Model;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

/// <summary>
/// Issue #473: the long DormCorridor &lt;-&gt; CorridorJunction walk returned its transition text
/// directly instead of prepending it to base.BeforeEnterLocation(...). base is the only place
/// VisitCount is incremented (and OnFirstTimeEnterLocation fires); in Brief mode LookProcessor shows
/// the full room description only when VisitCount == 1, so entering via the transition path silently
/// dropped the first-visit room description.
///
/// The two directions are the same bug mirrored, so they are pinned together here — merged from the
/// former one-test CorridorJunctionTests / DormCorridorTests.
/// </summary>
public class CorridorTransitionTests : EngineTestsBase
{
    [Test]
    public async Task EnterCorridorJunctionFromDormCorridor_FirstVisit_ShowsRoomDescriptionAndIncrementsVisitCount()
    {
        var target = GetTarget();
        Context.Verbosity = Verbosity.Brief;
        StartHere<DormCorridor>();

        var response = await target.GetResponse("east");

        Context.CurrentLocation.Should().BeOfType<CorridorJunction>();
        // The walk-transition text must accompany, not replace, the first-visit room description.
        response.Should().Contain("You walk down the long, featureless hallway");
        response.Should().Contain("A north-south corridor intersects the main corridor here");
        GetLocation<CorridorJunction>().VisitCount.Should().Be(1);
    }

    [Test]
    public async Task EnterDormCorridorFromCorridorJunction_FirstVisit_ShowsRoomDescriptionAndIncrementsVisitCount()
    {
        var target = GetTarget();
        Context.Verbosity = Verbosity.Brief;
        StartHere<CorridorJunction>();

        var response = await target.GetResponse("west");

        Context.CurrentLocation.Should().BeOfType<DormCorridor>();
        // The walk-transition text must accompany, not replace, the first-visit room description.
        response.Should().Contain("You walk down the long, featureless hallway");
        response.Should().Contain("wide, east-west hallway with openings to the north and south");
        GetLocation<DormCorridor>().VisitCount.Should().Be(1);
    }
}
