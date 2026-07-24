using FluentAssertions;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Moq;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Tests;

public class RadiationLabTests : EngineTestsBase
{
    [Test]
    public async Task Issue447_LookThroughCrack_ShowsBioLabView_EndToEndThroughTheEngine()
    {
        // #447 end-to-end: prove the FULL pipeline, not just the handler. The real AI parser reliably
        // produces SimpleIntent(look, "crack") for "look through crack" (verified in the [Explicit]
        // OpenAIParserTests). Here we inject exactly that intent and drive the whole engine via GetResponse
        // to prove a look-verb SimpleIntent actually ROUTES to RadiationLab.RespondToSimpleInteraction (and
        // is not intercepted as a bare room-look) so the raw-input view gate fires and the Bio Lab view
        // shows. This is the routing half the handler-only tests below don't cover.
        var parsed = new SimpleIntent
        {
            Verb = "look", Noun = "crack", Adverb = "through", OriginalInput = "look through crack"
        };

        var parser = new Mock<IIntentParser>();
        parser.Setup(p => p.DetermineSystemIntentType(It.IsAny<string>())).Returns((IntentBase?)null);
        parser.Setup(p => p.DetermineGlobalIntentType(It.IsAny<string>())).Returns((IntentBase?)null);
        parser.Setup(p => p.ResolvePronounsAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync((string?)null);
        parser.Setup(p => p.DetermineComplexIntentType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(parsed);

        var target = GetTarget(parser.Object);
        StartHere<RadiationLab>();

        var response = await target.GetResponse("look through crack");

        response.Should().Contain("Sinister shapes lurk about within");
        response.Should().NotContain("too small to go through");
    }

    [Test]
    public async Task LookThroughCrack_ShowsBioLabView_EvenWhenParseDoesNotCleanlyExtractCrack()
    {
        // Issue #447: on prod "look through crack" parses non-deterministically (measured 5/8 wrong) — the
        // "through" preposition disrupts verb/noun extraction, so it does NOT reliably arrive as
        // verb ∈ LookVerbs + noun "crack". Unlike the Bio Lock window, the crack has NO alternative phrasing
        // that reaches the view (examine / look at / look in crack all return "too small"), so the handler
        // MUST be robust to however the command parses. This simulates the bad parse — the preposition
        // absorbed into the noun so Match(LookVerbs, ["crack"]) misses — and asserts the view still shows.
        var target = GetTarget();
        var room = StartHere<RadiationLab>();

        var badParse = new SimpleIntent
        {
            Verb = "look",
            Noun = "through crack",
            OriginalInput = "look through crack"
        };

        var result = await room.RespondToSimpleInteraction(
            badParse, target.Context, Mock.Of<IGenerationClient>(), Mock.Of<IItemProcessorFactory>());

        result.InteractionMessage.Should().Contain("Sinister shapes lurk about within");
    }

    [Test]
    public async Task PeerThroughCrack_WithDisruptedVerbAndNoun_StillShowsView()
    {
        // Same fragility with a look-family synonym: "peer through the crack" must reach the view even when
        // the verb is a LookVerbs synonym and the noun extraction is disrupted by the preposition.
        var target = GetTarget();
        var room = StartHere<RadiationLab>();

        var badParse = new SimpleIntent
        {
            Verb = "peer",
            Noun = "crack through",
            OriginalInput = "peer through the crack"
        };

        var result = await room.RespondToSimpleInteraction(
            badParse, target.Context, Mock.Of<IGenerationClient>(), Mock.Of<IItemProcessorFactory>());

        result.InteractionMessage.Should().Contain("Sinister shapes lurk about within");
    }

    [Test]
    public async Task ExamineCrack_WithoutThrough_StillSaysTooSmall()
    {
        // Regression guard: "examine crack" (no "through") must keep the "too small" response, NOT the
        // Bio Lab view. The robust through-gate only fires when the raw input actually says "through".
        var target = GetTarget();
        var room = StartHere<RadiationLab>();

        var examine = new SimpleIntent
        {
            Verb = "examine",
            Noun = "crack",
            OriginalInput = "examine crack"
        };

        var result = await room.RespondToSimpleInteraction(
            examine, target.Context, Mock.Of<IGenerationClient>(), Mock.Of<IItemProcessorFactory>());

        result.InteractionMessage.Should().Contain("too small to go through");
    }
}
