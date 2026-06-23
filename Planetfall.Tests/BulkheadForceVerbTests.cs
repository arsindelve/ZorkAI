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

    [Test]
    public async Task BenignVerbsOnBulkhead_StillWork()
    {
        // Sanity check that the fix doesn't disturb the verbs that already worked: these are handled
        // by dedicated processors and never depended on GetItemInScope.
        var target = ArrangeDeckNineWithSharedBulkhead();

        var examine = await target.GetResponse("examine bulkhead");
        examine.Should().Contain("nothing special about the narrow emergency bulkhead");

        target = ArrangeDeckNineWithSharedBulkhead();
        var open = await target.GetResponse("open the bulkhead");
        open.Should().Contain("Why open the door to the emergency escape pod");
    }
}
