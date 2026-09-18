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

    private static HintExchange Rung(string topic, int rung, string text = "…")
    {
        return new HintExchange("?", text, topic, rung, nameof(HintKind.Progress));
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

        var second = await service.GetHint(Ask("more", Rung("RIFT", 0)));
        second.Rung.Should().Be(1);
        second.Text.Should().Be("B approach");

        var third = await service.GetHint(Ask("more", Rung("RIFT", 0), Rung("RIFT", 1)));
        third.Text.Should().Be("C solution");

        var again = await service.GetHint(Ask("more", Rung("RIFT", 0), Rung("RIFT", 1), Rung("RIFT", 2)));
        again.Rung.Should().Be(2);
    }

    [Test]
    public async Task ExhaustedLadder_SaysSo_InsteadOfSilentlyRepeating()
    {
        var service = Service(Rift(), new StubLlm { Routed = RoutedIntent.More });

        var result = await service.GetHint(Ask("more", Rung("RIFT", 0), Rung("RIFT", 1), Rung("RIFT", 2)));

        result.Rung.Should().Be(2);
        result.Text.Should().Be(HintService.PrefaceExhausted + "C solution");
    }

    [Test]
    public async Task ReAskingTheSameTopic_AlsoAdvances()
    {
        // Asking about the rift again, even without saying "more", means they want more.
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "RIFT") };

        var result = await Service(Rift(), llm).GetHint(Ask("how do I cross the rift?", Rung("RIFT", 0)));

        result.Rung.Should().Be(1);
    }

    [Test]
    public async Task Continue_WithoutATopic_ResumesTheLastTopicInHistory()
    {
        var provider = new FakeProvider()
            .Add("FLOYD", NodeStatus.Available, "wake him")
            .Add("RIFT", NodeStatus.Available, "A nudge", "B approach");
        var llm = new StubLlm { Routed = RoutedIntent.More };

        var result = await Service(provider, llm).GetHint(Ask("more", Rung("RIFT", 0)));

        // Not FLOYD (the first open node) — the thread in play was RIFT.
        result.Topic.Should().Be("RIFT");
        result.Text.Should().Be("B approach");
    }

    [Test]
    public async Task BareHintRequest_SkipsRouting_AndContinuesTheThread()
    {
        var llm = new StubLlm { Routed = null }; // an unavailable router must not matter

        var result = await Service(Rift(), llm).GetHint(Ask("", Rung("RIFT", 0)));

        llm.RouteCalls.Should().Be(0);
        result.Text.Should().Be("B approach");
    }

    [Test]
    public async Task BareHintButton_AfterALoreAnswer_StillHintsThePuzzle()
    {
        // The Hint button (empty question) is always a puzzle hint, whatever was last discussed.
        var result = await Service(Rift(), new StubLlm()).GetHint(Ask("",
            new HintExchange("why did the ship blow up?", "You can't know yet.", Kind: nameof(HintKind.Lore))));

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("RIFT");
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
    public async Task ExplicitTopicAlreadyDone_WithNothingAfterIt_GoesToTheSolver()
    {
        // They hold the card and ask how to use the booth: no ladder models that, so the game itself answers.
        var provider = new FakeProvider()
            .Add("RIFT", NodeStatus.Done, "cross it")
            .Add("TOWER", NodeStatus.Available, "go up");
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "RIFT"), SolveResult = "you're across" };

        var result = await Service(provider, llm).GetHint(Ask("how do I cross the rift?"));

        result.Kind.Should().Be(HintKind.Grounded);
        result.Text.Should().Be("reveal#0:you're across");
        llm.LastRung.Should().BeNull(); // and never the unrelated open puzzle
    }

    [Test]
    public async Task ExplicitTopicAlreadyDone_WithAnOpenNextStage_HintsThatStage()
    {
        // "The light went gray, now what?" — the first pour is done; the second is what they need.
        var provider = new FakeProvider()
            .Add("POUR_1", NodeStatus.Done, "pour once")
            .AddWithPrerequisites("POUR_2", NodeStatus.Available, ["POUR_1"], "pour the other fluid")
            .Add("ELSEWHERE", NodeStatus.Available, "something unrelated");
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "POUR_1") };

        var result = await Service(provider, llm).GetHint(Ask("the light went gray, now what?"));

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("POUR_2");
        result.Rung.Should().Be(0);
        result.Text.Should().Be("pour the other fluid");
    }

    [Test]
    public async Task ExplicitTopicNotYetReachable_RedirectsToItsNearestOpenPrerequisite()
    {
        // FLASK is the first open node overall, but it has nothing to do with the cure. The cure needs the
        // laser; the laser is what they can actually do next.
        var provider = new FakeProvider()
            .Add("FLASK", NodeStatus.Available, "get the flask")
            .AddWithPrerequisites("LASER", NodeStatus.Available, ["SHUTTLE"], "arm the laser")
            .AddWithPrerequisites("CURE", NodeStatus.Locked, ["LASER", "MINI_CARD"], "laser the microbe")
            .AddWithPrerequisites("MINI_CARD", NodeStatus.Locked, ["BIOLOCK"], "take the card");
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "CURE") };

        var result = await Service(provider, llm).GetHint(Ask("how do I cure the disease?"));

        result.Topic.Should().Be("LASER");
        // One step away: just the prerequisite's hint, no "further down the road" lecture.
        result.Text.Should().Be("arm the laser");
        result.Text.Should().NotContain("microbe");
    }

    [Test]
    public async Task ExplicitTopicFarAhead_SaysSo_BeforeRedirecting()
    {
        var provider = new FakeProvider()
            .Add("MAGNET", NodeStatus.Available, "get the magnet")
            .AddWithPrerequisites("KEY", NodeStatus.Locked, ["MAGNET"], "use the magnet")
            .AddWithPrerequisites("LADDER", NodeStatus.Locked, ["KEY"], "take the ladder");
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "LADDER") };

        var result = await Service(provider, llm).GetHint(Ask("how do I get the ladder?"));

        result.Topic.Should().Be("MAGNET");
        result.Text.Should().Be(HintService.PrefaceNotYet + "get the magnet");
    }

    [Test]
    public async Task ExplicitTopicNotYetReachable_WithNoOpenPrerequisite_FallsBackToTheBlocker()
    {
        var provider = new FakeProvider()
            .Add("RIFT", NodeStatus.Available, "cross it")
            .Add("CURE", NodeStatus.Locked, "laser the microbe"); // no prerequisite edges declared
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "CURE") };

        var result = await Service(provider, llm).GetHint(Ask("how do I cure the disease?"));

        result.Topic.Should().Be("RIFT");
        result.Text.Should().StartWith(HintService.PrefaceNotYet).And.EndWith("cross it");
    }

    [Test]
    public async Task ContinuedThreadNowLocked_MovesOnToTheBlocker_WithoutLeakingIt()
    {
        // They were being hinted on RIFT, then restored an earlier save; the client-held history survives.
        var provider = new FakeProvider()
            .Add("MAGNET", NodeStatus.Available, "get the magnet")
            .Add("RIFT", NodeStatus.Locked, "A nudge", "B approach", "C solution");
        var llm = new StubLlm { Routed = RoutedIntent.More };

        var result = await Service(provider, llm).GetHint(Ask("more", Rung("RIFT", 1)));

        result.Topic.Should().Be("MAGNET");
        result.Text.Should().Be("get the magnet");
    }

    [Test]
    public async Task SolvedThread_IsDropped_AndTheNextBlockerPicked()
    {
        var provider = new FakeProvider()
            .Add("RIFT", NodeStatus.Done, "rift hint")
            .Add("TOWER", NodeStatus.Available, "tower hint");
        var llm = new StubLlm { Routed = RoutedIntent.More };

        var result = await Service(provider, llm).GetHint(Ask("more", Rung("RIFT", 0)));

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

    // ---- unlisted topics: the fallback -------------------------------------------------------

    [Test]
    public async Task UnlistedTopic_IsSolvedOverTheDocs_NotHintedAsTheBlocker()
    {
        var provider = Rift();
        var llm = new StubLlm
        {
            Routed = new RoutedIntent(HintIntent.Progress, false, null, Unlisted: true),
            SolveResult = "The tin can is a dead end."
        };

        var result = await Service(provider, llm).GetHint(Ask("what about the tin can?"));

        result.Kind.Should().Be(HintKind.Grounded);
        result.Text.Should().Be("reveal#0:The tin can is a dead end.");
        llm.LastRung.Should().BeNull(); // the rift ladder was never touched
    }

    [Test]
    public async Task UnknownTopicFromTheRouter_IsTreatedAsUnlisted()
    {
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "NOT_A_NODE") };

        var result = await Service(Rift(), llm).GetHint(Ask("how do I fly the helicopter?"));

        result.Kind.Should().Be(HintKind.Grounded);
    }

    // ---- continuations follow the kind of the last answer ------------------------------------

    [Test]
    public async Task MoreAfterALoreAnswer_ContinuesTheLore_NotThePuzzleLadder()
    {
        var provider = Rift();
        provider.LoreText = "THE SOURCE";
        // Even when the router guesses a puzzle from the catalog, a continuation is about the last answer.
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, true, "RIFT") };

        var result = await Service(provider, llm).GetHint(Ask("more", Rung("RIFT", 0),
            new HintExchange("why did the ship blow up?", "You can't know yet.", Kind: nameof(HintKind.Lore))));

        result.Kind.Should().Be(HintKind.Lore);
        llm.LastLoreSource.Should().Be("THE SOURCE");
        llm.LastRung.Should().BeNull();
    }

    [Test]
    public async Task MoreAfterAProgressRung_ClimbsTheLadder_WhateverTheRouterGuessed()
    {
        // A bare "more" after a rung is the next rung — even if the router called it unlisted or
        // attached some other puzzle from the catalog.
        var provider = new FakeProvider()
            .Add("FLOYD", NodeStatus.Available, "wake him")
            .Add("RIFT", NodeStatus.Available, "A nudge", "B approach", "C solution");
        var service = Service(provider, new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, true, null, Unlisted: true) });

        var unlisted = await service.GetHint(Ask("just tell me", Rung("RIFT", 1)));
        unlisted.Kind.Should().Be(HintKind.Progress);
        unlisted.Topic.Should().Be("RIFT");
        unlisted.Text.Should().Be("C solution");

        service = Service(provider, new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, true, "FLOYD") });
        var guessed = await service.GetHint(Ask("more", Rung("RIFT", 0)));
        guessed.Topic.Should().Be("RIFT");
        guessed.Text.Should().Be("B approach");
    }

    [Test]
    public async Task MoreAfterAMechanicAnswer_ContinuesAsMechanic()
    {
        var llm = new StubLlm { Routed = RoutedIntent.More };

        var result = await Service(Rift(), llm).GetHint(Ask("more",
            new HintExchange("why am I sick?", "You've caught The Disease.", Kind: nameof(HintKind.Mechanic))));

        result.Kind.Should().Be(HintKind.Mechanic);
    }

    [Test]
    public async Task MoreAfterAGroundedAnswer_ContinuesTheFallbackConversation()
    {
        var llm = new StubLlm { Routed = RoutedIntent.More, SolveResult = "still a dead end" };
        var earlier = new HintExchange("what about the tin can?", "Not much use, that.", Kind: nameof(HintKind.Grounded));

        var result = await Service(Rift(), llm).GetHint(Ask("more", earlier));

        result.Kind.Should().Be(HintKind.Grounded);
        result.Text.Should().Be("reveal#1:still a dead end"); // the revealer saw the longer history
        llm.LastRung.Should().BeNull();
    }

    [Test]
    public async Task MoreAfterADecline_IsAPuzzleHint_NotLore()
    {
        var llm = new StubLlm { Routed = RoutedIntent.More };

        var result = await Service(Rift(), llm).GetHint(Ask("ok then what do I do?",
            new HintExchange("how do I get the magnet?", HintService.DeclineOutOfScope, Kind: nameof(HintKind.Decline))));

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("RIFT");
    }

    [Test]
    public async Task AFreshPuzzleQuestion_AfterALoreAnswer_IsStillAPuzzleHint()
    {
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Progress, false, "RIFT") };

        var result = await Service(Rift(), llm).GetHint(Ask("ok so how do I cross the rift?",
            new HintExchange("why did the ship blow up?", "You can't know yet.", Kind: nameof(HintKind.Lore))));

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("RIFT");
    }

    // ---- frustration ------------------------------------------------------------------------

    [Test]
    public async Task ClearlyStuckPlayer_StartsAFreshTopicFurtherDown_ButNeverAtTheSolution()
    {
        var service = Service(Rift(), new StubLlm());

        // 4 deaths / DeathsForOneRung(2) = 2, capped at the approach rung.
        var stuck = await service.GetHint(new HintRequest(Session, State(deaths: 4), "what do I do?", []));
        stuck.Rung.Should().Be(1);
        stuck.Text.Should().Be("B approach");

        // Dying a lot early must not turn every later puzzle's first hint into the answer.
        var veryStuck = await service.GetHint(new HintRequest(Session, State(deaths: 40), "what do I do?", []));
        veryStuck.Rung.Should().Be(1);
    }

    [Test]
    public async Task FrustrationFloor_NeverRewindsAThreadAlreadyUnderway()
    {
        var request = new HintRequest(Session, State(deaths: 4), "more", [Rung("RIFT", 0)]);

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

        await Service(provider, llm).GetHint(Ask("how do I cross the rift?", Rung("FLOYD", 0)));

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

    [Test]
    public async Task UnavailableRouter_Declines_RatherThanGuessing()
    {
        // A lore question answered with the walkthrough's next step is the failure this engine exists to
        // prevent; when the router is down, say so instead.
        var llm = new StubLlm { Routed = null };

        var result = await Service(Rift(), llm).GetHint(Ask("why did the ship blow up?"));

        result.Kind.Should().Be(HintKind.Decline);
        result.Text.Should().Be(HintService.DeclineUnavailable);
        llm.LastRung.Should().BeNull();
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
    public async Task LoreQuestion_TheSourceDoesNotCover_FallsThroughToThePuzzleTheRouterSaw()
    {
        // "What does this memo mean?" read as lore; the digest has no memo. The router also noticed which
        // puzzle the memo belongs to, so that puzzle's rung answers instead of an improvised story.
        var provider = new FakeProvider()
            .Add("RIFT", NodeStatus.Available, "cross it")
            .Add("MEMO", NodeStatus.Available, "the memo is a warning");
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Lore, false, "MEMO"), ForceLore = HintSignals.NotInSource };

        var result = await Service(provider, llm).GetHint(Ask("what does this memo mean?"));

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("MEMO");
        result.Text.Should().Be("the memo is a warning");
        llm.LastDocs.Should().BeNull();
    }

    [Test]
    public async Task MechanicQuestion_TheSourceDoesNotCover_WithNoPuzzleNamed_GoesToTheSolver()
    {
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Mechanic, false, null), ForceLore = HintSignals.NotInSource, SolveResult = "the cube holds a fused part" };

        var result = await Service(Rift(), llm).GetHint(Ask("what's wrong with the cube?"));

        result.Kind.Should().Be(HintKind.Grounded);
        result.Text.Should().Be("reveal#0:the cube holds a fused part");
        llm.LastRung.Should().BeNull(); // not the active blocker's ladder
    }

    [Test]
    public async Task LoreQuestion_AnsweredAsLater_IsStillLore_NotAFallThrough()
    {
        // "You can't know that yet" is an answer, not a miss.
        var llm = new StubLlm { Routed = new RoutedIntent(HintIntent.Lore, false, "RIFT"), ForceLore = "You can't know that yet." };

        var result = await Service(Rift(), llm).GetHint(Ask("why did the ship blow up?"));

        result.Kind.Should().Be(HintKind.Lore);
        llm.LastRung.Should().BeNull();
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
        result.Text.Should().Be("Time is short.\n\nA nudge");
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
        // The solver got the static docs untouched, and the full situation with the gated hints appended...
        llm.LastDocs.Should().Be("THE SOURCE");
        llm.LastSolveContext.Should().Contain("FULL CONTEXT").And.Contain("THE INVISICLUES SO FAR");
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
