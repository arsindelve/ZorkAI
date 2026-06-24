using FluentAssertions;
using GameEngine.Hints;
using Model.Hints;
using Planetfall.Hints;
using ZorkAI.OpenAI;

namespace Planetfall.Tests.Hints;

/// <summary>
///     LIVE end-to-end test of the hint subsystem against real OpenAI (the actual LLM phrasing,
///     classification, and lore/mechanic answering). [Explicit] / credential-gated per repo convention
///     (needs OPEN_AI_KEY) — it is not part of the normal CI run. Run it to exercise every scenario and
///     read the real narrator output.
/// </summary>
[TestFixture]
[Explicit("Live OpenAI calls — requires OPEN_AI_KEY")]
public class PlanetfallHintLiveTests : EngineTestsBase
{
    private HintService _service = null!;

    [SetUp]
    public void SetUp()
    {
        GetTarget(); // fresh PlanetfallContext + Repository
        _service = new HintService(new PlanetfallHintProvider(), new InMemoryHintMemoryStore(),
            new OpenAiHintLanguageModel());
    }

    [Test]
    public async Task ProgressLadder_OnAFreshGame_EscalatesRungByRung()
    {
        const string session = "live-progress";

        var r0 = await _service.GetHint(new HintRequest(session, Context, null, false, null));
        var r1 = await _service.GetHint(new HintRequest(session, Context, "more", true, null));
        var r2 = await _service.GetHint(new HintRequest(session, Context, "more", true, null));

        TestContext.Out.WriteLine($"[rung 0] {r0.Text}");
        TestContext.Out.WriteLine($"[rung 1] {r1.Text}");
        TestContext.Out.WriteLine($"[rung 2] {r2.Text}");

        r0.Topic.Should().Be("ESCAPE_POD");
        r0.Rung.Should().Be(0);
        r1.Rung.Should().Be(1);
        r2.Rung.Should().Be(2);
        new[] { r0.Text, r1.Text, r2.Text }.Should().OnlyContain(t => !string.IsNullOrWhiteSpace(t));
    }

    [Test]
    public async Task Lore_WhyIsEverythingDeserted()
    {
        var r = await _service.GetHint(new HintRequest("live-lore", Context, "why is everything deserted?", false, null));
        TestContext.Out.WriteLine($"[lore] {r.Text}");
        r.Kind.Should().Be(HintKind.Lore);
        r.Text.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task Mechanic_WhyAmIGettingSick()
    {
        Context.Day = 4;
        var r = await _service.GetHint(new HintRequest("live-mech", Context, "why am I getting sick?", false, null));
        TestContext.Out.WriteLine($"[mechanic] {r.Text}");
        r.Kind.Should().Be(HintKind.Mechanic);
        r.Text.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task OutOfScope_GetsTheNarratorBrushoff()
    {
        var r = await _service.GetHint(new HintRequest("live-oos", Context, "what's the wifi password?", false, null));
        TestContext.Out.WriteLine($"[out-of-scope] {r.Text} (kind={r.Kind})");
        r.Kind.Should().Be(HintKind.Decline);
    }
}
