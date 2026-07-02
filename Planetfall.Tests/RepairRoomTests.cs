using FluentAssertions;
using GameEngine;
using Planetfall;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class RepairRoomTests : EngineTestsBase
{
    private async Task TriggerAchillesEulogy(GameEngine<PlanetfallGame, PlanetfallContext> target,
        RepairRoom repairRoom)
    {
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        repairRoom.ItemPlacedHere(floyd);
        target.Context.RegisterActor(repairRoom);

        await target.GetResponse("wait");
    }

    [TestCase("robot")]
    [TestCase("damaged robot")]
    [TestCase("broken robot")]
    public async Task ExamineRobot_BeforeEulogy_ReturnsNeutralText_NotFloydReference(string noun)
    {
        var target = GetTarget();
        StartHere<RepairRoom>();

        var response = await target.GetResponse($"examine {noun}");

        response.Should().Contain("damaged robot");
        response.Should().NotContain("Floyd");
        response.Should().NotContain("Achilles");
        response.Should().NotContain("nothing special");
    }

    // "robot" is deliberately excluded here: it collides with Floyd's own noun list
    // ("robot" is also in Floyd.NounsForMatching), so once Floyd is physically in the room
    // for the eulogy, "examine robot" triggers a disambiguation prompt instead of resolving
    // to BrokenRobot. That's a separate, tracked bug (see PR #371 review) - not this fix's scope.
    [TestCase("damaged robot")]
    [TestCase("broken robot")]
    public async Task ExamineRobot_AfterEulogy_ReturnsAchillesSpecificText_NotGenericFallback(string noun)
    {
        var target = GetTarget();
        var repairRoom = StartHere<RepairRoom>();
        await TriggerAchillesEulogy(target, repairRoom);

        var response = await target.GetResponse($"examine {noun}");

        response.Should().Contain("Achilles");
        response.Should().Contain("Floyd");
        response.Should().NotContain("nothing special");
    }

    [TestCase("robot")]
    [TestCase("damaged robot")]
    [TestCase("broken robot")]
    public async Task TakeRobot_BeforeEulogy_RefusesWithNeutralMessage(string noun)
    {
        var target = GetTarget();
        StartHere<RepairRoom>();

        var response = await target.GetResponse($"take {noun}");

        response.Should().Contain("You leave him where he fell");
        response.Should().NotContain("Floyd");
    }

    // "robot" excluded for the same noun-collision-with-Floyd reason as the examine test above.
    [TestCase("damaged robot")]
    [TestCase("broken robot")]
    public async Task TakeRobot_AfterEulogy_RefusesWithSolemnFloydReferenceMessage(string noun)
    {
        var target = GetTarget();
        var repairRoom = StartHere<RepairRoom>();
        await TriggerAchillesEulogy(target, repairRoom);

        var response = await target.GetResponse($"take {noun}");

        response.Should().Contain("You leave him as Floyd found him");
    }

    [Test]
    public async Task TakeRobot_BeforeEulogy_DoesNotMoveItem()
    {
        var target = GetTarget();
        var repairRoom = StartHere<RepairRoom>();

        await target.GetResponse("take robot");

        GetItem<BrokenRobot>().CurrentLocation.Should().Be(repairRoom);
    }
}
