using FluentAssertions;
using Model;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

public class CorridorJunctionTests : EngineTestsBase
{
    // Issue #473: entering CorridorJunction from DormCorridor returned the walk-transition text
    // directly instead of prepending it to base.BeforeEnterLocation(...). base is the only place
    // VisitCount is incremented (and OnFirstTimeEnterLocation fires); in Brief mode LookProcessor
    // shows the full room description only when VisitCount == 1, so entering via the transition
    // path silently dropped the first-visit room description.
    [Test]
    public async Task EnterFromDormCorridor_FirstVisit_ShowsRoomDescriptionAndIncrementsVisitCount()
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
}
