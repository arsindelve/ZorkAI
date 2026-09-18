using FluentAssertions;
using GameEngine;
using GameEngine.Hints;
using Model.Hints;
using Planetfall.Hints;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
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
        _service = new HintService(PlanetfallHintProvider.HandWritten(), new OpenAiHintLanguageModel());
    }

    [Test]
    public async Task RepeatedAsks_EscalateViaChatHistory()
    {
        // The client owns the conversation now: it threads the running history into each request.
        const string session = "live-ladder";
        var history = new List<HintExchange>();

        async Task<string> Ask(string q)
        {
            var r = await _service.GetHint(new HintRequest(session, Context, q, history));
            // The client echoes topic/rung/kind: that is what makes the next ask the next rung.
            history.Add(new HintExchange(q, r.Text, r.Topic, r.Rung, r.Kind.ToString()));
            return r.Text;
        }

        var r1 = await Ask("I'm stuck, what do I do?");
        var r2 = await Ask("I need more help");
        var r3 = await Ask("just tell me exactly what to do");

        TestContext.Out.WriteLine($"[ask 1] {r1}");
        TestContext.Out.WriteLine($"[ask 2] {r2}");
        TestContext.Out.WriteLine($"[ask 3] {r3}");

        new[] { r1, r2, r3 }.Should().OnlyContain(t => !string.IsNullOrWhiteSpace(t));
    }

    [Test]
    public async Task DeadEnd_TheTruthIsToldDirectly()
    {
        var r = await _service.GetHint(new HintRequest("live-rx", Context, "is the reactor important?", []));
        TestContext.Out.WriteLine($"[reactor] {r.Text}");
        r.Text.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task Lore_AnswersInVoice()
    {
        var r = await _service.GetHint(new HintRequest("live-lore", Context, "why is everything deserted?", []));
        TestContext.Out.WriteLine($"[lore] {r.Text}");
        r.Text.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    ///     The conversation that broke the previous system, end to end on the real wiring: a "what do I do"
    ///     on Deck Nine, then "why does the ship blow up?", then pushing for more. Prints kind/topic/rung with
    ///     each answer so the routing and laddering can be read alongside the prose.
    /// </summary>
    [Test]
    public async Task TheFeinsteinConversation_Live()
    {
        var history = new List<HintExchange>();

        async Task Ask(string q)
        {
            var r = await _service.GetHint(new HintRequest("live-feinstein", Context, q, history));
            history.Add(new HintExchange(q, r.Text, r.Topic, r.Rung, r.Kind.ToString()));
            TestContext.Out.WriteLine($"YOU:   {q}\n       [{r.Kind} topic={r.Topic ?? "-"} rung={r.Rung}/{r.TotalRungs}]\nGUIDE: {r.Text}\n");
            r.Text.Should().NotBeNullOrWhiteSpace();
        }

        await Ask("I'm on Deck Nine. What should I do?");
        await Ask("why does the ship blow up?");
        await Ask("more");
        await Ask("no really, why did it explode?");
        await Ask("ok, what do I do now?");
        await Ask("more");
        await Ask("just tell me exactly");
        await Ask("who is Blather?");
        await Ask("how do I fix the planetary defense?");
        await Ask("what about the tin can?");
        await Ask("is the reactor important?");
    }

    /// <summary>
    ///     End-to-end on the REAL production wiring: embedded whole-source knowledge + serialized save-game
    ///     state + gpt-5.4-mini. Prints answers for the puzzles that historically broke, plus a state-aware
    ///     question (Floyd dead) to confirm the save-game state actually reaches the model.
    /// </summary>
    [Test]
    public async Task ProductionPath_Sweep()
    {
        async Task Ask(string label, string q)
        {
            var r = await _service.GetHint(new HintRequest(label, Context, q, []));
            TestContext.Out.WriteLine($"[{label}] {q}\n   -> {r.Text}\n");
            r.Text.Should().NotBeNullOrWhiteSpace();
        }

        await Ask("ladder", "I put the ladder across the rift but it fell in and is gone. what did I do wrong?");
        await Ask("elevators", "what's the difference between the upper and lower elevators?");
        await Ask("shuttle", "how do I take the shuttle to the other complex?");
        await Ask("reactor", "is the reactor important?");
        await Ask("lore", "what was the Project, and did it work?");

        // State-aware: a COHERENT late game — Floyd was woken, then died at the bio lab, cure is done.
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        Repository.GetItem<Floyd>().HasDied = true;
        Repository.GetItem<Planetfall.Item.Computer.Relay>().SpeckDestroyed = true;
        await Ask("floyd-dead", "can Floyd help me get past the mutants?");
    }
}
