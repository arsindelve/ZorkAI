using FluentAssertions;
using GameEngine;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Moq;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

/// <summary>
/// Regression tests for issue #282: on Deck Nine (the opening room), "force" verbs aimed at the
/// escape-pod bulkhead (push / kick / shake / hit / knock / bang) returned a completely empty
/// response — a blank line — while benign verbs (examine, open, close) worked.
///
/// Root cause: the BulkheadDoor is a single shared instance seeded into BOTH Deck Nine and the
/// Escape Pod (PlanetfallGame.Init loads the pod last, so the door's CurrentLocation ends up being
/// the pod even while the player stands on Deck Nine). A force verb has no matching processor, so it
/// produces a NoVerbMatch and the engine asks GetGeneratedNoMatchingVerbResponse to narrate it. That
/// method re-resolved the noun with Repository.GetItemInScope, which fails its accessibility check
/// for the door (its CurrentLocation points at the pod, not Deck Nine) and returned null — and the
/// method short-circuited to string.Empty, printing a blank line instead of reaching the narrator.
/// </summary>
public class BulkheadForceVerbTests : EngineTestsBase
{
    private const string NoEffectFlavor = "You give the bulkhead a solid whack, but nothing happens.";

    /// <summary>
    /// Reproduce the exact move-1 prod state: the player stands on Deck Nine, but the shared
    /// BulkheadDoor singleton's CurrentLocation is the Escape Pod (because PlanetfallGame.Init loads
    /// the pod after Deck Nine). The test harness's GetTarget re-inits Deck Nine last, so we re-run
    /// the pod's init to reach the same end-state, and assert the precondition so this stays honest.
    /// </summary>
    private GameEngine<PlanetfallGame, PlanetfallContext> ArrangeDeckNineWithSharedBulkhead()
    {
        var target = GetTarget();

        Repository.GetLocation<EscapePod>().Init();
        Repository.GetItem<BulkheadDoor>().CurrentLocation.Should().BeOfType<EscapePod>(
            "issue #282 only bites when the shared door's CurrentLocation is the pod, not Deck Nine");

        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Narrator ON: any no-effect narration returns this flavor line so we can prove the force
        // verb reached the narrator rather than short-circuiting to a blank line.
        Mock.Get(target.GenerationClient)
            .Setup(x => x.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
            .ReturnsAsync(NoEffectFlavor);

        return target;
    }

    // push/kick/shake are the force verbs the TestParser resolves to a plain SimpleIntent (verb +
    // "bulkhead"), which is the routing that reaches GetGeneratedNoMatchingVerbResponse. The bug's
    // other reported verbs (hit/knock/bang) normalize to the same NoVerbMatch path in prod via the
    // AI parser, which the deterministic TestParser doesn't model — so they aren't enumerated here,
    // but the single shared-routing fix covers all of them.
    [TestCase("push the bulkhead")]
    [TestCase("kick the bulkhead")]
    [TestCase("shake the bulkhead")]
    public async Task ForceVerbOnBulkhead_AtDeckNine_NeverReturnsABlankLine(string input)
    {
        var target = ArrangeDeckNineWithSharedBulkhead();

        var response = await target.GetResponse(input);

        response.Should().NotBeNullOrWhiteSpace(
            "a force verb on the bulkhead must fall through to the narrator, not print a blank line (#282)");
        response.Should().Contain(NoEffectFlavor);
    }

    // The two sanity checks below confirm the fix doesn't disturb the verbs that already worked:
    // these are handled by dedicated processors and never depended on GetItemInScope.

    [Test]
    public async Task ExamineBulkhead_StillWorks()
    {
        var target = ArrangeDeckNineWithSharedBulkhead();

        var examine = await target.GetResponse("examine bulkhead");

        examine.Should().Contain("nothing special about the narrow emergency bulkhead");
    }

    [Test]
    public async Task OpenBulkhead_StillWorks()
    {
        var target = ArrangeDeckNineWithSharedBulkhead();

        var open = await target.GetResponse("open the bulkhead");

        open.Should().Contain("Why open the door to the emergency escape pod");
    }
}
