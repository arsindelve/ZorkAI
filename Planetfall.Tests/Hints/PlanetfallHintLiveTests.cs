using FluentAssertions;
using GameEngine.Hints;
using Planetfall.Hints;
using ZorkAI.OpenAI;

namespace Planetfall.Tests.Hints;

/// <summary>
///     LIVE end-to-end test of the two-tier hint engine against real OpenAI (Solve + Reveal).
///     [Explicit] / credential-gated (OPEN_AI_KEY) — not part of CI. Run it to read the real output.
/// </summary>
[TestFixture]
[Explicit("Live OpenAI calls — requires OPEN_AI_KEY")]
public class PlanetfallHintLiveTests : EngineTestsBase
{
    private HintService _service = null!;

    [SetUp]
    public void SetUp()
    {
        GetTarget();
        _service = new HintService(new PlanetfallHintProvider(), new InMemoryHintMemoryStore(),
            new OpenAiHintLanguageModel());
    }

    [Test]
    public async Task RepeatedAsks_EscalateViaChatHistory()
    {
        const string session = "live-ladder";
        var r1 = await _service.GetHint(new HintRequest(session, Context, "I'm stuck, what do I do?"));
        var r2 = await _service.GetHint(new HintRequest(session, Context, "I need more help"));
        var r3 = await _service.GetHint(new HintRequest(session, Context, "just tell me exactly what to do"));

        TestContext.Out.WriteLine($"[ask 1] {r1.Text}");
        TestContext.Out.WriteLine($"[ask 2] {r2.Text}");
        TestContext.Out.WriteLine($"[ask 3] {r3.Text}");

        new[] { r1.Text, r2.Text, r3.Text }.Should().OnlyContain(t => !string.IsNullOrWhiteSpace(t));
    }

    [Test]
    public async Task DeadEnd_TheTruthIsToldDirectly()
    {
        var r = await _service.GetHint(new HintRequest("live-rx", Context, "is the reactor important?"));
        TestContext.Out.WriteLine($"[reactor] {r.Text}");
        r.Text.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task Lore_AnswersInVoice()
    {
        var r = await _service.GetHint(new HintRequest("live-lore", Context, "why is everything deserted?"));
        TestContext.Out.WriteLine($"[lore] {r.Text}");
        r.Text.Should().NotBeNullOrWhiteSpace();
    }
}
