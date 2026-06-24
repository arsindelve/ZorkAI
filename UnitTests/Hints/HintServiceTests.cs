using FluentAssertions;
using GameEngine.Hints;
using Model.Interface;
using Moq;
using NUnit.Framework;

namespace UnitTests.Hints;

/// <summary>
///     Eval harness for the two-tier hint engine (Docs/hints/07). Drives <see cref="HintService" /> with
///     a recording stub LLM — no OpenAI, no network. Asserts the orchestration: solve-then-reveal, the
///     right inputs flow to each tier, and the chat history accumulates so the revealer can pace itself.
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

    private static HintService Service(FakeProvider provider, RecordingLlm llm) =>
        new(provider, new InMemoryHintMemoryStore(), llm);

    [Test]
    public async Task SolvesFirst_ThenRevealsTheSolverOutput()
    {
        var provider = new FakeProvider { Docs = "KB", PlayerContext = "at the rift" };
        var llm = new RecordingLlm { SolveResult = "extend the ladder across the rift" };

        var result = await Service(provider, llm).GetHint(new HintRequest(Session, State(), "how do I cross the rift?"));

        // LLM 1 got the docs + context + question.
        llm.LastDocs.Should().Be("KB");
        llm.LastSolveContext.Should().Be("at the rift");
        llm.LastSolveQuestion.Should().Be("how do I cross the rift?");
        // LLM 2 got the solver's complete solution and produced what's actually returned.
        llm.LastSolution.Should().Be("extend the ladder across the rift");
        result.Text.Should().Be("reveal#0");
    }

    [Test]
    public async Task ChatHistoryAccumulates_SoTheRevealerCanPace()
    {
        var llm = new RecordingLlm();
        var service = Service(new FakeProvider(), llm);

        await service.GetHint(new HintRequest(Session, State(), "I'm stuck"));
        // First reveal saw an empty history.
        llm.LastHistory.Should().BeEmpty();

        await service.GetHint(new HintRequest(Session, State(), "more help"));
        // Second reveal saw the first exchange — this is how disclosure escalates (no rung counter).
        llm.LastHistory.Should().HaveCount(1);
        llm.LastHistory[0].Question.Should().Be("I'm stuck");
        llm.LastHistory[0].Revealed.Should().Be("reveal#0");
    }

    [Test]
    public async Task HistoryIsPerSession()
    {
        var llm = new RecordingLlm();
        var service = Service(new FakeProvider(), llm);

        await service.GetHint(new HintRequest("session-A", State(), "q"));
        await service.GetHint(new HintRequest("session-B", State(), "q"));

        // Session B starts fresh — it must not see session A's history.
        llm.LastHistory.Should().BeEmpty();
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
