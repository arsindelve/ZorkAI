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
///     The stub echoes what it is handed, so a test can assert exactly which rung was revealed and what
///     reached the model. Disclosure state rides in the client-replayed history.
/// </summary>
[TestFixture]
public class HintServiceTests
{
    private const string Session = "test-session";

    private static IContext State(int deaths = 0)
    {
        var ctx = new Mock<IContext>();
        ctx.SetupGet(c => c.Moves).Returns(0);
        ctx.Setup(c => c.GetDeathCount()).Returns(deaths);
        return ctx.Object;
    }

    private static HintRequest Ask(string question, params HintExchange[] history)
    {
        return new HintRequest(Session, State(), question, history);
    }

    private static HintService Service(FakeProvider provider, StubLlm llm)
    {
        return new HintService(provider, llm);
    }

    private static FakeProvider Rift()
    {
        return FakeProvider.WithOpenPuzzle("RIFT", "A nudge", "B approach", "C solution");
    }

    // ---- progress: the ladder ---------------------------------------------------------------

    [Test]
    public async Task OpenEndedFirstAsk_RevealsTheVaguestRungOfTheActiveBlocker()
    {
        var llm = new StubLlm { Routed = RoutedIntent.OpenEnded };

        var result = await Service(Rift(), llm).GetHint(Ask("what do I do?"));

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("RIFT");
        result.Rung.Should().Be(0);
        result.TotalRungs.Should().Be(3);
        result.Text.Should().Be("A nudge");
    }

    [Test]
    public async Task EchoedRung_AdvancesOneAtATime_AndClampsAtTheLast()
    {
        var service = Service(Rift(), new StubLlm { Routed = RoutedIntent.More });

        var second = await service.GetHint(Ask("more", new HintExchange("?", "A nudge", "RIFT", 0)));
        second.Rung.Should().Be(1);
        second.Text.Should().Be("B approach");

        var third = await service.GetHint(Ask("more", new HintExchange("?", "A nudge", "RIFT", 0),
            new HintExchange("more", "B approach", "RIFT", 1)));
        third.Text.Should().Be("C solution");

        // Already at the last rung — clamps rather than running off the end.
        var again = await service.GetHint(Ask("more", new HintExchange("?", "A nudge", "RIFT", 0),
            new HintExchange("more", "B approach", "RIFT", 1), new HintExchange("more", "C solution", "RIFT", 2)));
        again.Rung.Should().Be(2);
    }

    [Test]
    public async Task ExhaustedLadder_SaysSo_InsteadOfSilentlyRepeating()
    {
        var service = Service(Rift(), new StubLlm { Routed = RoutedIntent.More });

        var result = await service.GetHint(Ask("more", new HintExchange("?", "A nudge", "RIFT", 0),
            new HintExchange("more", "B approach", "RIFT", 1), new HintExchange("more", "C solution", "RIFT", 2)));

        result.Rung.Should().Be(2);
        result.Text.Should().Be(HintService.PrefaceExhausted + "C solution");
    }

    [Test]
    public async Task MoreAfterALoreAnswer_ContinuesTheLore_NotThePuzzleLadder()
    {
        // The router reads a bare "more" as progress/continue; the history shows the last answer was
        // lore (no topic), so "more" means more lore.
        var provider = Rift();
        provider.LoreText = "THE SOURCE";
        // Even when the router guesses a puzzle from the catalog, a continuation is about the last answer.
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, true, "RIFT") };

        var result = await Service(provider, llm).GetHint(Ask("more",
            new HintExchange("?", "A nudge", "RIFT", 0),
            new HintExchange("why did the ship blow up?", "You can't know yet.")));

        result.Kind.Should().Be(HintKind.Lore);
        llm.LastLoreSource.Should().Be("THE SOURCE");
        llm.LastRung.Should().BeNull();
    }

    [Test]
    public async Task AFreshPuzzleQuestion_AfterALoreAnswer_IsStillAPuzzleHint()
    {
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "RIFT") };

        var result = await Service(Rift(), llm).GetHint(Ask("ok so how do I cross the rift?",
            new HintExchange("why did the ship blow up?", "You can't know yet.")));

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("RIFT");
    }

    [Test]
    public async Task BareHintButton_AfterALoreAnswer_StillHintsThePuzzle()
    {
        // The Hint button (empty question) is always a puzzle hint, whatever was last discussed.
        var result = await Service(Rift(), new StubLlm()).GetHint(Ask("",
            new HintExchange("why did the ship blow up?", "You can't know yet.")));

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("RIFT");
    }

    [Test]
    public async Task ReAskingTheSameTopic_AlsoAdvances()
    {
        // Asking about the rift again, even without saying "more", means they want more.
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "RIFT") };

        var result = await Service(Rift(), llm)
            .GetHint(Ask("how do I cross the rift?", new HintExchange("rift?", "A nudge", "RIFT", 0)));

        result.Rung.Should().Be(1);
    }

    [Test]
    public async Task Continue_WithoutATopic_ResumesTheLastTopicInHistory()
    {
        var provider = new FakeProvider()
            .Add("FLOYD", NodeStatus.Available, "wake him")
            .Add("RIFT", NodeStatus.Available, "A nudge", "B approach");
        var llm = new StubLlm { Routed = RoutedIntent.More };

        var result = await Service(provider, llm).GetHint(Ask("more", new HintExchange("rift?", "A nudge", "RIFT", 0)));

        // Not FLOYD (the first open node) — the thread in play was RIFT.
        result.Topic.Should().Be("RIFT");
        result.Text.Should().Be("B approach");
    }

    [Test]
    public async Task BareHintRequest_SkipsRouting_AndContinuesTheThread()
    {
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.OutOfScope, false, null) };

        var result = await Service(Rift(), llm).GetHint(Ask("", new HintExchange("rift?", "A nudge", "RIFT", 0)));

        llm.RouteCalls.Should().Be(0);
        result.Text.Should().Be("B approach");
    }

    [Test]
    public async Task ExplicitTopic_IsHonoredOverTheActiveBlocker()
    {
        var provider = new FakeProvider()
            .Add("FLOYD", NodeStatus.Available, "wake him")
            .Add("RIFT", NodeStatus.Available, "cross it");
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "RIFT") };

        var result = await Service(provider, llm).GetHint(Ask("how do I cross the rift?"));

        result.Topic.Should().Be("RIFT");
        result.Text.Should().Be("cross it");
    }

    [Test]
    public async Task UnknownTopicFromTheRouter_IsIgnored()
    {
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "NOT_A_NODE") };

        var result = await Service(Rift(), llm).GetHint(Ask("how do I fly?"));

        result.Topic.Should().Be("RIFT");
    }

    [Test]
    public async Task ExplicitTopicAlreadyDone_Declines()
    {
        var provider = new FakeProvider()
            .Add("RIFT", NodeStatus.Done, "cross it")
            .Add("TOWER", NodeStatus.Available, "go up");
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "RIFT") };

        var result = await Service(provider, llm).GetHint(Ask("how do I cross the rift?"));

        result.Kind.Should().Be(HintKind.Decline);
        result.Text.Should().Be(HintService.DeclineAlreadyDone);
    }

    [Test]
    public async Task ExplicitTopicNotYetReachable_RedirectsToTheBlocker_WithoutLeakingIt()
    {
        var provider = new FakeProvider()
            .Add("RIFT", NodeStatus.Available, "cross it")
            .Add("CURE", NodeStatus.Locked, "laser the microbe");
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "CURE") };

        var result = await Service(provider, llm).GetHint(Ask("how do I cure the disease?"));

        result.Topic.Should().Be("RIFT");
        result.Text.Should().StartWith(HintService.PrefaceNotYet).And.EndWith("cross it");
        result.Text.Should().NotContain("microbe");
    }

    [Test]
    public async Task SolvedThread_IsDropped_AndTheNextBlockerPicked()
    {
        // They were being hinted on RIFT, then solved it; "more" must move on, not repeat a done puzzle.
        var provider = new FakeProvider()
            .Add("RIFT", NodeStatus.Done, "rift hint")
            .Add("TOWER", NodeStatus.Available, "tower hint");
        var llm = new StubLlm { Routed = RoutedIntent.More };

        var result = await Service(provider, llm).GetHint(Ask("more", new HintExchange("?", "rift hint", "RIFT", 0)));

        result.Topic.Should().Be("TOWER");
        result.Rung.Should().Be(0);
        result.Text.Should().Be("tower hint");
    }

    [Test]
    public async Task NothingOpen_Declines()
    {
        var provider = new FakeProvider().Add("RIFT", NodeStatus.Done, "cross it");

        var result = await Service(provider, new StubLlm()).GetHint(Ask("what now?"));

        result.Kind.Should().Be(HintKind.Decline);
        result.Text.Should().Be(HintService.DeclineNothingLeft);
    }

    // ---- frustration ------------------------------------------------------------------------

    [Test]
    public async Task ClearlyStuckPlayer_StartsAFreshTopicFurtherDown()
    {
        // 4 deaths / DeathsForOneRung(2) = floor 2 — straight to the solution rung.
        var request = new HintRequest(Session, State(deaths: 4), "what do I do?", new List<HintExchange>());

        var result = await Service(Rift(), new StubLlm()).GetHint(request);

        result.Rung.Should().Be(2);
        result.Text.Should().Be("C solution");
    }

    [Test]
    public async Task FrustrationFloor_NeverRewindsAThreadAlreadyUnderway()
    {
        var history = new List<HintExchange> { new("?", "A nudge", "RIFT", 0) };
        var request = new HintRequest(Session, State(deaths: 4), "more", history);

        var result = await Service(Rift(), new StubLlm { Routed = RoutedIntent.More }).GetHint(request);

        result.Rung.Should().Be(1);
    }

    // ---- what reaches the model ---------------------------------------------------------------

    [Test]
    public async Task ThePhraser_IsHandedOnlyTheCurrentRung_AndTheKeyState()
    {
        var provider = Rift();
        provider.KeyState = "Floyd is dead.";
        var llm = new StubLlm();

        await Service(provider, llm).GetHint(Ask("what do I do?"));

        llm.LastRung.Should().Be("A nudge");
        llm.LastKeyState.Should().Be("Floyd is dead.");
        llm.LastDocs.Should().BeNull(); // the solver never ran
    }

    [Test]
    public async Task TheRouter_IsHandedTheTopicCatalog_AndTheHistory()
    {
        var provider = new FakeProvider()
            .Add("FLOYD", NodeStatus.Available, "wake him")
            .Add("RIFT", NodeStatus.Available, "cross it");
        var llm = new StubLlm();
        var history = new HintExchange("earlier", "something", "FLOYD", 0);

        await Service(provider, llm).GetHint(Ask("how do I cross the rift?", history));

        llm.LastRouteQuestion.Should().Be("how do I cross the rift?");
        llm.LastTopics.Select(t => t.Id).Should().Equal("FLOYD", "RIFT");
        llm.LastHistory.Should().ContainSingle().Which.Topic.Should().Be("FLOYD");
    }

    [Test]
    public async Task SilentPhraser_FallsBackToTheAuthoredRung()
    {
        var llm = new StubLlm { ForcePhrase = "   " };

        var result = await Service(Rift(), llm).GetHint(Ask("what do I do?"));

        result.Text.Should().Be("A nudge");
    }

    // ---- lore & mechanic ----------------------------------------------------------------------

    [Test]
    public async Task LoreQuestion_IsAnsweredFromTheGroundedSourceOnly()
    {
        var provider = Rift();
        provider.LoreText = "THE SHIP EXPLODED; YOU CAN'T KNOW WHY YET";
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Lore, false, null) };

        var result = await Service(provider, llm).GetHint(Ask("why did the ship blow up?"));

        result.Kind.Should().Be(HintKind.Lore);
        result.Topic.Should().BeNull();
        llm.LastLoreSource.Should().Be("THE SHIP EXPLODED; YOU CAN'T KNOW WHY YET");
        llm.LastDocs.Should().BeNull(); // never the whole-source solver
        llm.LastRung.Should().BeNull(); // never a puzzle rung
    }

    [Test]
    public async Task MechanicQuestion_UsesTheSameSource_WithMechanicKind()
    {
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Mechanic, false, null) };

        var result = await Service(Rift(), llm).GetHint(Ask("why am I so sick?"));

        result.Kind.Should().Be(HintKind.Mechanic);
        result.Text.Should().StartWith("lore:");
    }

    [Test]
    public async Task LoreQuestion_WhenTheModelIsSilent_Declines()
    {
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Lore, false, null), ForceLore = "" };

        var result = await Service(Rift(), llm).GetHint(Ask("who is Blather?"));

        result.Kind.Should().Be(HintKind.Decline);
        result.Text.Should().Be(HintService.DeclineUnavailable);
    }

    [Test]
    public async Task OutOfScopeQuestion_Declines()
    {
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.OutOfScope, false, null) };

        var result = await Service(Rift(), llm).GetHint(Ask("what's the wifi password?"));

        result.Kind.Should().Be(HintKind.Decline);
        result.Text.Should().Be(HintService.DeclineOutOfScope);
    }

    // ---- soft-locks -----------------------------------------------------------------------------

    [Test]
    public async Task HardSoftLock_ShortCircuitsToTheRestoreMessage()
    {
        var provider = Rift();
        provider.SoftLocks.Add(new FakeSoftLock(new SoftLockVerdict(SoftLockKind.Hard, "You can't win from here — restore.")));

        var result = await Service(provider, new StubLlm()).GetHint(Ask("what do I do?"));

        result.Kind.Should().Be(HintKind.SoftLock);
        result.SoftLock.Should().Be(SoftLockKind.Hard);
        result.Text.Should().Be("You can't win from here — restore.");
    }

    [Test]
    public async Task WarningSoftLock_AttachesACaveatToTheHint()
    {
        var provider = Rift();
        provider.SoftLocks.Add(new FakeSoftLock(new SoftLockVerdict(SoftLockKind.Warning, "Time is short.")));

        var result = await Service(provider, new StubLlm()).GetHint(Ask("what do I do?"));

        result.Kind.Should().Be(HintKind.Progress);
        result.SoftLock.Should().Be(SoftLockKind.Warning);
        result.Text.Should().Contain("Time is short.").And.Contain("A nudge");
    }

    // ---- fallback: no authored ladder ----------------------------------------------------------

    [Test]
    public async Task NoAuthoredLadder_FallsBackToSolvingOverTheDocs_ThenReveals()
    {
        var provider = new FakeProvider().Add("ODD", NodeStatus.Available); // open, but no ladder
        provider.Docs = "THE SOURCE";
        provider.LoreText = "THE INVISICLUES SO FAR";
        var llm = new StubLlm { SolveResult = "extend the ladder" };

        var result = await Service(provider, llm).GetHint(Ask("what about the odd thing?"));

        result.Kind.Should().Be(HintKind.Grounded);
        result.Topic.Should().BeNull();
        // The solver got the docs plus the tier-gated invisiclues and the full situation...
        llm.LastDocs.Should().Contain("THE SOURCE").And.Contain("THE INVISICLUES SO FAR");
        llm.LastSolveContext.Should().Be("FULL CONTEXT");
        // ...the revealer got only the key state and the solution, and produced what's returned.
        llm.LastRevealContext.Should().Be("KEYSTATE");
        llm.LastSolution.Should().Be("extend the ladder");
        result.Text.Should().Be("reveal#0:extend the ladder");
    }

    [Test]
    public async Task Fallback_WhenTheRevealerIsSilent_Declines_AndNeverLeaksTheSolution()
    {
        var provider = new FakeProvider().Add("ODD", NodeStatus.Available);
        var llm = new StubLlm { SolveResult = "THE WHOLE ANSWER", ForceReveal = "" };

        var result = await Service(provider, llm).GetHint(Ask("what about the odd thing?"));

        result.Kind.Should().Be(HintKind.Decline);
        result.Text.Should().Be(HintService.DeclineUnavailable);
        result.Text.Should().NotContain("THE WHOLE ANSWER");
    }

    [Test]
    public async Task Fallback_WhenTheSolverIsSilent_Declines()
    {
        var provider = new FakeProvider().Add("ODD", NodeStatus.Available);
        var llm = new StubLlm { SolveResult = "" };

        var result = await Service(provider, llm).GetHint(Ask("what about the odd thing?"));

        result.Kind.Should().Be(HintKind.Decline);
        llm.LastSolution.Should().BeNull(); // the revealer never ran
    }

    // ---- proactive --------------------------------------------------------------------------------

    [Test]
    public void ProactiveNudges_AreSurfacedByPriority()
    {
        var provider = Rift();
        provider.Proactive.Add(new FakeProactive(new ProactiveNudge("sleep", "You are tired.", 1)));
        provider.Proactive.Add(new FakeProactive(new ProactiveNudge("disease", "You are sick.", 5)));

        var nudges = Service(provider, new StubLlm()).ProactiveNudges(State());

        nudges.Should().HaveCount(2);
        nudges[0].Category.Should().Be("disease"); // higher priority first
    }
}
