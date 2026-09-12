using FluentAssertions;
using GameEngine.Hints;
using Model.Hints;
using Model.Interface;
using Moq;
using NUnit.Framework;

namespace UnitTests.Hints;

/// <summary>
///     Eval harness for the two-tier hint engine. Drives <see cref="HintService" /> with a recording stub
///     LLM — no OpenAI, no network. Asserts the orchestration: solve-then-reveal, the right inputs flow to
///     each tier, and the client-supplied history reaches both tiers. The service is stateless.
/// </summary>
[TestFixture]
public class HintServiceTests
{
    private const string Session = "test-session";

    private static IContext State()
    {
        var ctx = new Mock<IContext>();
        ctx.SetupGet(c => c.Moves).Returns(0);
        return ctx.Object;
    }

    private static HintRequest Req(string question, IReadOnlyList<HintExchange>? history = null) =>
        new(Session, State(), question, history ?? new List<HintExchange>());

    private static HintService Service(FakeProvider provider, RecordingLlm llm) => new(provider, llm);

    [Test]
    public async Task SolvesFirst_ThenRevealsTheSolverOutput()
    {
        var provider = new FakeProvider { Docs = "KB", PlayerContext = "at the rift" };
        var llm = new RecordingLlm { SolveResult = "extend the ladder across the rift" };

        var result = await Service(provider, llm).GetHint(Req("how do I cross the rift?"));

        // LLM 1 got the docs + context + question.
        llm.LastDocs.Should().Be("KB");
        llm.LastSolveContext.Should().Be("at the rift");
        llm.LastSolveQuestion.Should().Be("how do I cross the rift?");
        // LLM 2 got the solver's complete solution and produced what's actually returned.
        llm.LastSolution.Should().Be("extend the ladder across the rift");
        result.Text.Should().Be("reveal#0");
    }

    [Test]
    public async Task ClientSuppliedHistory_ReachesBothTiers()
    {
        // The client replays the prior conversation; both tiers receive it (the solver to resolve
        // follow-ups like "more", the revealer to pace disclosure). No server-side accumulation.
        var history = new List<HintExchange> { new("I'm stuck", "a vague nudge") };
        var llm = new RecordingLlm();

        await Service(new FakeProvider(), llm).GetHint(Req("more help", history));

        llm.LastSolveHistory.Should().HaveCount(1);
        llm.LastHistory.Should().HaveCount(1);
        llm.LastHistory[0].Question.Should().Be("I'm stuck");
        llm.LastHistory[0].Revealed.Should().Be("a vague nudge");
    }

    [Test]
    public async Task FirstAsk_NoHistory_SeesEmpty()
    {
        var llm = new RecordingLlm();
        await Service(new FakeProvider(), llm).GetHint(Req("I'm stuck"));
        llm.LastHistory.Should().BeEmpty();
    }

    [Test]
    public async Task EmptyReveal_FailsVisibly()
    {
        var llm = new RecordingLlm { ForceReveal = "   " }; // model degraded to whitespace

        var result = await Service(new FakeProvider(), llm).GetHint(Req("I'm stuck"));

        // The player gets a clear message, not a blank hint — flagged as a non-hint so clients
        // display it without recording it into the replayed conversation.
        result.Text.Should().Contain("unavailable");
        result.IsHint.Should().BeFalse();
    }

    [Test]
    public void ProactiveNudges_AreSurfacedByPriority()
    {
        var provider = new FakeProvider();
        provider.Proactive.Add(new FakeProactive(new ProactiveNudge("sleep", "You are tired.", 1)));
        provider.Proactive.Add(new FakeProactive(new ProactiveNudge("disease", "You are sick.", 5)));

        var nudges = Service(provider, new RecordingLlm()).ProactiveNudges(State());

        nudges.Should().HaveCount(2);
        nudges[0].Category.Should().Be("disease"); // higher priority first
    }
}
