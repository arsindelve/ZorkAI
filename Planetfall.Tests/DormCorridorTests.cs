using FluentAssertions;
using Model;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

public class DormCorridorTests : EngineTestsBase
{
    // Issue #473: entering DormCorridor from CorridorJunction returned the walk-transition text
    // directly instead of prepending it to base.BeforeEnterLocation(...). base is the only place
    // VisitCount is incremented (and OnFirstTimeEnterLocation fires); in Brief mode LookProcessor
    // shows the full room description only when VisitCount == 1, so entering via the transition
    // path silently dropped the first-visit room description.
    [Test]
    public async Task EnterFromCorridorJunction_FirstVisit_ShowsRoomDescriptionAndIncrementsVisitCount()
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
