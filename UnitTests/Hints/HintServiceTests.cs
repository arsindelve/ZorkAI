using FluentAssertions;
using GameEngine.Hints;
using Model.Hints;
using Model.Interface;
using Moq;
using NUnit.Framework;

namespace UnitTests.Hints;

/// <summary>
///     Eval harness for the game-agnostic hint engine (Docs/hints/07 + 04). Drives <see cref="HintService" />
///     with a deterministic stub LLM and a configurable fake provider — no OpenAI, no network, no clock.
///     The stub LLM echoes the rung text verbatim so a test can assert *which* rung was revealed.
/// </summary>
[TestFixture]
public class HintServiceTests
{
    private const string Session = "test-session";

    private static IContext State(int moves = 0, int deaths = 0)
    {
        var ctx = new Mock<IContext>();
        ctx.SetupGet(c => c.Moves).Returns(moves);
        ctx.Setup(c => c.GetDeathCount()).Returns(deaths);
        ctx.SetupGet(c => c.Score).Returns(0);
        return ctx.Object;
    }

    private static HintService Service(FakeProvider provider, IHintLanguageModel? llm = null)
    {
        return new HintService(provider, new InMemoryHintMemoryStore(), llm ?? new StubLlm());
    }

    // ---- progress: laddering ----------------------------------------------------------------

    [Test]
    public async Task NewTopic_NoFrustration_RevealsVaguestRung()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A nudge", "B approach", "C solution");
        var result = await Service(provider).GetHint(new HintRequest(Session, State(), null, false, null));

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("RIFT");
        result.Rung.Should().Be(0);
        result.TotalRungs.Should().Be(3);
        result.Text.Should().Be("A nudge");
    }

    [Test]
    public async Task More_AdvancesOneRungAtATime_AndClampsAtLast()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A nudge", "B approach", "C solution");
        var service = Service(provider);

        (await service.GetHint(new HintRequest(Session, State(), null, false, null))).Rung.Should().Be(0);
        (await service.GetHint(new HintRequest(Session, State(), null, true, null))).Text.Should().Be("B approach");
        (await service.GetHint(new HintRequest(Session, State(), null, true, null))).Text.Should().Be("C solution");
        // Already at the last rung — clamps, does not run off the end.
        (await service.GetHint(new HintRequest(Session, State(), null, true, null))).Rung.Should().Be(2);
    }

    [Test]
    public async Task Frustration_StartsFurtherDownTheLadder()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A nudge", "B approach", "C solution");
        // 4 deaths / DeathsForOneRung(2) = floor 2 -> a clearly-stuck player starts at the solution rung.
        var result = await Service(provider).GetHint(new HintRequest(Session, State(deaths: 4), null, false, null));

        result.Rung.Should().Be(2);
        result.Text.Should().Be("C solution");
    }

    [Test]
    public async Task ExplicitTopic_IsHonored()
    {
        var provider = FakeProvider.WithOpenPuzzles(
            ("FLOYD", new[] { "find floyd" }),
            ("RIFT", new[] { "cross it" }));
        var result = await Service(provider).GetHint(new HintRequest(Session, State(), null, false, "RIFT"));

        result.Topic.Should().Be("RIFT");
        result.Text.Should().Be("cross it");
    }

    // ---- progress: reconciliation -----------------------------------------------------------

    [Test]
    public async Task SolvedTopic_IsClosed_AndNextBlockerPicked()
    {
        // RIFT is already Done; the engine must not hint it — it picks the next open blocker.
        var provider = FakeProvider.WithStatuses(
            new[] { ("RIFT", NodeStatus.Done, new[] { "rift hint" }), ("TOWER", NodeStatus.Available, new[] { "tower hint" }) });

        var result = await Service(provider).GetHint(new HintRequest(Session, State(), null, false, null));

        result.Topic.Should().Be("TOWER");
        result.Text.Should().Be("tower hint");
    }

    // ---- soft-lock --------------------------------------------------------------------------

    [Test]
    public async Task HardSoftLock_ShortCircuitsToRestoreMessage()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A", "B", "C");
        provider.SoftLocks.Add(new FakeSoftLock(new SoftLockVerdict(SoftLockKind.Hard, "You can't win from here — restore.")));

        var result = await Service(provider).GetHint(new HintRequest(Session, State(), null, false, null));

        result.Kind.Should().Be(HintKind.SoftLock);
        result.SoftLock.Should().Be(SoftLockKind.Hard);
        result.Text.Should().Be("You can't win from here — restore.");
    }

    [Test]
    public async Task BestEndingOnlySoftLock_AttachesCaveatToHint()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A nudge", "B", "C");
        provider.SoftLocks.Add(new FakeSoftLock(new SoftLockVerdict(SoftLockKind.BestEndingOnly, "Best ending now lost.")));

        var result = await Service(provider).GetHint(new HintRequest(Session, State(), null, false, null));

        result.Kind.Should().Be(HintKind.Progress);
        result.SoftLock.Should().Be(SoftLockKind.BestEndingOnly);
        result.Text.Should().Contain("Best ending now lost.").And.Contain("A nudge");
    }

    // ---- intent routing: lore & mechanic ----------------------------------------------------

    [Test]
    public async Task LoreQuestion_RoutesToLoreSource()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A", "B", "C");
        provider.Lore = new FakeLore(new LoreAnswer(true, "Everything is deserted because of the plague."));
        var llm = new StubLlm { Intent = HintIntent.Lore };

        var result = await Service(provider, llm).GetHint(
            new HintRequest(Session, State(), "why is everything deserted?", false, null));

        result.Kind.Should().Be(HintKind.Lore);
        result.Text.Should().Contain("plague");
    }

    [Test]
    public async Task MechanicQuestion_RoutesToMechanicExplainer()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A", "B", "C");
        provider.Mechanic = new FakeMechanic(new LoreAnswer(true, "You caught The Disease; it worsens daily."));
        var llm = new StubLlm { Intent = HintIntent.Mechanic };

        var result = await Service(provider, llm).GetHint(
            new HintRequest(Session, State(), "why am I getting sick?", false, null));

        result.Kind.Should().Be(HintKind.Mechanic);
        result.Text.Should().Contain("Disease");
    }

    [Test]
    public async Task MechanicNotRecognised_FallsBackToLore()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A", "B", "C");
        provider.Mechanic = new FakeMechanic(new LoreAnswer(false, ""));
        provider.Lore = new FakeLore(new LoreAnswer(true, "lore answer"));
        var llm = new StubLlm { Intent = HintIntent.Mechanic };

        var result = await Service(provider, llm).GetHint(
            new HintRequest(Session, State(), "why?", false, null));

        result.Kind.Should().Be(HintKind.Lore);
        result.Text.Should().Be("lore answer");
    }

    [Test]
    public async Task RedHerringQuestion_AnsweredHonestly_BeforeIntentRouting()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A", "B", "C");
        provider.Herrings["reactor"] = "The reactor is a dead end — don't waste your time.";
        // The LLM would classify this as Progress and we'd hint the rift — but the red-herring guard
        // must intercept it first and answer the truth.
        var llm = new StubLlm { Intent = HintIntent.Progress };

        var result = await Service(provider, llm).GetHint(
            new HintRequest(Session, State(), "how do I use the reactor?", false, null));

        result.Kind.Should().Be(HintKind.Lore);
        result.Text.Should().Contain("dead end");
        result.Topic.Should().BeNull(); // not routed to a puzzle
    }

    [Test]
    public async Task RedHerringAndKey_MatchesOnlyWhenAllTermsPresent()
    {
        var provider = FakeProvider.WithOpenPuzzle("SLEEP", "A", "B", "C");
        provider.Herrings["bed&infirmary"] = "The infirmary bed kills you — sleep in a dorm bunk.";
        var service = Service(provider, new StubLlm { Intent = HintIntent.Progress });

        // Both terms present -> the context-specific dead-end answer.
        var deadly = await service.GetHint(new HintRequest(Session, State(), "is the infirmary bed safe?", false, null));
        deadly.Kind.Should().Be(HintKind.Lore);
        deadly.Text.Should().Contain("kills you");

        // "bed" alone (the safe dorm bed) must NOT trigger it — it routes to a normal hint.
        var safe = await service.GetHint(new HintRequest(Session, State(), "where is a bed to sleep in?", false, null));
        safe.Kind.Should().Be(HintKind.Progress);
    }

    [Test]
    public async Task OutOfScopeQuestion_Declines()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A", "B", "C");
        var llm = new StubLlm { Intent = HintIntent.OutOfScope };

        var result = await Service(provider, llm).GetHint(
            new HintRequest(Session, State(), "what's the wifi password?", false, null));

        result.Kind.Should().Be(HintKind.Decline);
    }

    [Test]
    public async Task MoreRequest_IsAlwaysProgress_NeverClassified()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A", "B", "C");
        // LLM would say OutOfScope, but More must bypass classification entirely.
        var llm = new StubLlm { Intent = HintIntent.OutOfScope };
        var service = Service(provider, llm);

        await service.GetHint(new HintRequest(Session, State(), null, false, null));
        var result = await service.GetHint(new HintRequest(Session, State(), "more", true, null));

        result.Kind.Should().Be(HintKind.Progress);
        result.Text.Should().Be("B");
    }

    // ---- proactive --------------------------------------------------------------------------

    [Test]
    public void ProactiveNudges_AreSurfacedByPriority()
    {
        var provider = FakeProvider.WithOpenPuzzle("RIFT", "A", "B", "C");
        provider.Proactive.Add(new FakeProactive(new ProactiveNudge("sleep", "You are tired.", 1)));
        provider.Proactive.Add(new FakeProactive(new ProactiveNudge("disease", "You are sick.", 5)));

        var nudges = Service(provider).ProactiveNudges(State());

        nudges.Should().HaveCount(2);
        nudges[0].Category.Should().Be("disease"); // higher priority first
    }
}
