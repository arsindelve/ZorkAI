using FluentAssertions;
using GameEngine;
using GameEngine.Hints;
using Model.Hints;
using Planetfall.Hints;
using Planetfall.Item.Computer;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Computer;
using Planetfall.Location.Kalamontee.Tower;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Shuttle;
using UnitTests.Hints;

namespace Planetfall.Tests.Hints;

/// <summary>
///     The eval fixtures from Docs/hints/planetfall/04: (game state, question) -> properties of the answer.
///     Everything asserted here is upstream of phrasing — which puzzle was localized, which rung, what
///     source text the model was allowed to see, and what it was NOT allowed to see — so it runs with the
///     LLM stubbed. This is the regression net for the failures the first system had: misplacing the
///     player, and answering from knowledge the player hasn't earned.
/// </summary>
[TestFixture]
public class PlanetfallHintEvalTests : EngineTestsBase
{
    private StubLlm _llm = null!;
    private HintService _service = null!;

    [SetUp]
    public void SetUp()
    {
        GetTarget();
        _llm = new StubLlm();
        _service = new HintService(new PlanetfallHintProvider(), _llm);
    }

    private Task<HintResponse> Ask(string question, RoutedIntent routed, params HintExchange[] history)
    {
        _llm.Routed = routed;
        return _service.GetHint(new HintRequest("eval", Context, question, history));
    }

    private static HintExchange Echo(string question, HintResponse response)
    {
        return new HintExchange(question, response.Text, response.Topic, response.Rung, response.Kind.ToString());
    }

    private static readonly RoutedIntent Lore = new(HintIntent.Lore, false, null);

    // ---- A. localization / B. blocker ----------------------------------------------------------------

    [Test]
    public async Task FreshGame_OpenEnded_SaysToWait_NotToBoardAPodThatIsShut()
    {
        // Before the explosion the pod bulkhead is closed and 'port' fails; the only move is to wait.
        var result = await Ask("what do I do?", RoutedIntent.OpenEnded);

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("EXPLOSION");
        result.Rung.Should().Be(0);
        result.Text.Should().Contain("about to happen");
        result.Text.Should().NotContain("port").And.NotContain("pod");
    }

    [Test]
    public async Task AfterTheExplosion_TheBlockerIsThePod()
    {
        Repository.GetItem<BulkheadDoor>().IsOpen = true; // the explosion opens it

        var result = await Ask("ok now what?!", RoutedIntent.OpenEnded);

        result.Topic.Should().Be("ESCAPE_POD");
        result.Text.Should().Contain("coming apart");
    }

    [Test]
    public async Task FloydAwake_OpenEnded_BlocksOnTheMagnet()
    {
        Repository.GetItem<Floyd>().HasEverBeenOn = true;

        var result = await Ask("I'm stuck", RoutedIntent.OpenEnded);

        result.Topic.Should().Be("MAGNET");
    }

    [Test]
    public async Task HoldingTheLadder_OpenEnded_BlocksOnCrossingTheRift()
    {
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        Take<Ladder>();

        var result = await Ask("what now?", RoutedIntent.OpenEnded);

        result.Topic.Should().Be("CROSS_RIFT");
        result.Text.Should().Contain("rift");
    }

    // ---- D. laddering ---------------------------------------------------------------------------------

    [Test]
    public async Task More_ClimbsTheEscapePodLadder_ToTheExactCommands()
    {
        var first = await Ask("what do I do?", RoutedIntent.OpenEnded);
        var second = await Ask("more", RoutedIntent.More, Echo("what do I do?", first));
        var third = await Ask("just tell me", RoutedIntent.More, Echo("what do I do?", first), Echo("more", second));

        first.Rung.Should().Be(0);
        second.Rung.Should().Be(1);
        second.Text.Should().Contain("waiting");
        third.Rung.Should().Be(2);
        third.Text.Should().Contain("wait").And.Contain("explosion");
    }

    [Test]
    public async Task AskingAboutALaterPuzzle_DoesNotLeakIt_AndPointsAtWhatIsInTheWay()
    {
        var result = await Ask("how do I cure the disease?", new RoutedIntent(HintIntent.Progress, false, "SPECK"));

        result.Topic.Should().Be("EXPLOSION"); // the only open prerequisite from the very start
        result.Text.Should().StartWith(HintService.PrefaceNotYet);
        result.Text.Should().NotContain("microbe").And.NotContain("laser").And.NotContain("384");
    }

    [Test]
    public async Task AskingAboutALaterPuzzle_RedirectsAlongItsOwnChain_NotToTheGloballyFirstNode()
    {
        // In Lawanda with the comms repair skipped: FLASK is the first open node overall, but the defense
        // repair needs the fromitz board, and that's what stands in the way.
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        Repository.GetLocation<LawandaPlatform>().VisitCount = 1;

        var result = await Ask("how do I fix the planetary defense?",
            new RoutedIntent(HintIntent.Progress, false, "DEFENSE_FIX"));

        result.Topic.Should().Be("FROMITZ");
        result.Text.Should().StartWith(HintService.PrefaceNotYet).And.Contain("replacement part");
        result.Text.Should().NotContain("fluid"); // not the tower chain
    }

    [Test]
    public void EarlyRungs_NeverMentionLateGameNouns()
    {
        var corpus = new PlanetfallHintProvider().PuzzleCorpus;
        string[] lateNouns = ["microbe", "laser", "bedistor", "fromitz", "miniaturiz", "mutant", "Veldina", "cryo"];

        foreach (var node in new[] { "EXPLOSION", "ESCAPE_POD", "LAND", "MAGNET", "FLOYD", "STEEL_KEY", "CROSS_RIFT" })
        {
            corpus.TryGetLadder(node, out var ladder).Should().BeTrue();
            foreach (var rung in ladder.Rungs)
            foreach (var noun in lateNouns)
                rung.Should().NotContainEquivalentOf(noun, $"{node} must not leak '{noun}'");
        }
    }

    [Test]
    public void TheMicrobeRung_CoversTheWholeVerifiedEscape()
    {
        // The walkthrough doesn't end at the speck: a microbe blocks the exit and has to be lured off the strip.
        new PlanetfallHintProvider().PuzzleCorpus.TryGetLadder("MICROBE", out var ladder).Should().BeTrue();

        ladder.Rungs[2].Should().Contain("set laser to 2").And.Contain("throw laser off strip").And.Contain("Auxiliary Booth");
    }

    [Test]
    public async Task InsideTheComputer_EachStageIsItsOwnPuzzle()
    {
        // Miniaturized, speck destroyed, microbe on the strip: the hint is about the microbe, not "already done".
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        Repository.GetLocation<LawandaPlatform>().VisitCount = 1;
        Repository.GetLocation<StripNearStation>().VisitCount = 1;
        Repository.GetItem<Relay>().SpeckDestroyed = true;

        var result = await Ask("a giant microbe landed on the strip! help!", new RoutedIntent(HintIntent.Progress, false, "MICROBE"));

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("MICROBE");
        result.Text.Should().Contain("tempted");

        Repository.GetItem<Microbe>().Dispatched = true;
        var after = await Ask("what now?", RoutedIntent.OpenEnded);
        after.Topic.Should().Be("GAS_MASK");
    }

    [Test]
    public async Task TheCommRoom_FirstAndSecondPour_AreDifferentHints()
    {
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        Repository.GetLocation<TowerCore>().VisitCount = 1;

        var first = await Ask("what do I do in the tower?", new RoutedIntent(HintIntent.Progress, false, "COMM_FIX"));
        first.Topic.Should().Be("COMM_POUR_1"); // COMM_FIX is locked behind the first pour
        first.Text.Should().StartWith(HintService.PrefaceNotYet).And.NotContain("gray button");

        // The router may just as well pick the tower itself, which is done: the answer is still the pour.
        var viaTower = await Ask("what do I do in the tower?", new RoutedIntent(HintIntent.Progress, false, "TOWER_UP"));
        viaTower.Topic.Should().Be("COMM_POUR_1");
        viaTower.Text.Should().Contain("colored lights"); // the pour's own first rung, unprefaced

        Repository.GetLocation<CommRoom>().CurrentColor = "gray";
        var second = await Ask("the light went gray, now what?", new RoutedIntent(HintIntent.Progress, false, "COMM_FIX"));
        second.Topic.Should().Be("COMM_FIX");
        second.Text.Should().Contain("other fluid");

        // ...and asking about the pour they just finished lands on the same next stage.
        var viaFirstPour = await Ask("the light went gray, now what?", new RoutedIntent(HintIntent.Progress, false, "COMM_POUR_1"));
        viaFirstPour.Topic.Should().Be("COMM_FIX");
    }

    [Test]
    public async Task StandingAtAnOptionalPuzzle_OpenEnded_HintsThatPuzzle_NotTheSpine()
    {
        // In the Comm Room with the lower card in hand: the spine says "take the lower elevator", but they
        // climbed the tower to do the comm repair, so that is what they want.
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        Repository.GetLocation<TowerCore>().VisitCount = 1;
        Take<Planetfall.Item.Kalamontee.Admin.LowerElevatorAccessCard>();
        StartHere<CommRoom>();

        var result = await Ask("what do I do?", RoutedIntent.OpenEnded);

        result.Topic.Should().Be("COMM_POUR_1");
    }

    [Test]
    public async Task AtTheLibrary_TheExplosionIsNoLongerUnexplained()
    {
        StartHere<LibraryLobby>();

        await Ask("why did the ship blow up?", Lore);

        _llm.LastLoreSource.Should().Contain("shot the Feinstein down");
        _llm.LastLoreSource.Should().NotContain("not something you can know yet");
    }

    // ---- F. grounding: the Feinstein explosion ---------------------------------------------------------

    [Test]
    public async Task WhyDoesTheShipBlowUp_AtTheStart_IsGroundedInNotKnowingYet()
    {
        // The question that broke the previous system. At the start the honest answer is "you can't know
        // yet": the invisiclues say so, the digest says so, and the Planetary Defense explanation is not in
        // the model's hands at all.
        var result = await Ask("why does the ship blow up?", Lore);

        result.Kind.Should().Be(HintKind.Lore);
        result.Topic.Should().BeNull();
        var source = _llm.LastLoreSource!;
        source.Should().Contain("not something you can know yet");
        source.Should().Contain("Hyperspatial Jump Machinery"); // the invisiclue's orienting rung
        source.Should().NotContain("shot the Feinstein down");
        source.Should().NotContain("discrimination circuit");
        source.Should().NotContain("why the Feinstein was destroyed"); // the late invisiclue
    }

    [Test]
    public async Task WhyDoesTheShipBlowUp_AtTheLibrary_IsAllowedTheRealAnswer()
    {
        StartHere<Library>();

        await Ask("why does the ship blow up?", Lore);

        _llm.LastLoreSource.Should().Contain("shot the Feinstein down");
    }

    [Test]
    public async Task ADeadEndQuestion_GoesToTheSolver_NotTheActiveBlocker()
    {
        // "What about the tin can?" has no puzzle node. It must not be answered with the escape pod.
        _llm.SolveResult = "The tin can can't be opened; there's no can opener in the game.";

        var result = await Ask("what about the tin can?", new RoutedIntent(HintIntent.Progress, false, null, Unlisted: true));

        result.Kind.Should().Be(HintKind.Grounded);
        result.Text.Should().Contain("can opener");
        _llm.LastRung.Should().BeNull();
        _llm.LastDocs.Should().Contain("VERIFIED WALKTHROUGH");
        _llm.LastSolveContext.Should().Contain("KEY STATE").And.Contain("OFFICIAL HINTS");
    }

    [Test]
    public async Task LoreQuestion_NeverReachesThePuzzleLadderOrTheSolver()
    {
        await Ask("who is Blather?", Lore);

        _llm.LastRung.Should().BeNull();
        _llm.LastDocs.Should().BeNull();
        _llm.LastLoreSource.Should().NotBeNull();
    }

    [Test]
    public async Task TheWalkthroughIsNeverInTheLoreSource()
    {
        StartHere<Library>();

        await Ask("how do I get all the points?", Lore);

        _llm.LastLoreSource.Should().NotContain("How to Get All 80 Points");
        _llm.LastLoreSource.Should().NotContain("[TestCase(");
    }

    [Test]
    public async Task TheLoreSource_WithholdsSolutionRungs()
    {
        // The Admin/Mech section is unlocked once landed; its "how do I cross the rift?" answer must arrive
        // without rung D, which is the solution.
        Repository.GetItem<Floyd>().HasEverBeenOn = true;

        await Ask("what is this place?", Lore);

        var source = _llm.LastLoreSource!;
        source.Should().Contain("Jumping is a bad idea.");
        source.Should().Contain("You'll need an item which you may not have seen yet.");
        source.Should().NotContain("It's behind the padlocked door.");
        source.Should().NotContain("Extend the ladder and put it across the rift.");
    }

    // ---- spoiler tiers -------------------------------------------------------------------------------

    [Test]
    public void SpoilerTier_RisesWithProgress()
    {
        var provider = new PlanetfallHintProvider();

        PlanetfallLoreSource.TierOf(Context, provider.ProgressMapper.Map(Context)).Should().Be(0);

        Repository.GetItem<Floyd>().HasEverBeenOn = true; // landed
        PlanetfallLoreSource.TierOf(Context, provider.ProgressMapper.Map(Context)).Should().Be(1);

        Repository.GetLocation<LawandaPlatform>().VisitCount = 1; // in Lawanda, but not yet at the library
        PlanetfallLoreSource.TierOf(Context, provider.ProgressMapper.Map(Context)).Should().Be(1);

        StartHere<LibraryLobby>(); // where the terminal is
        PlanetfallLoreSource.TierOf(Context, provider.ProgressMapper.Map(Context)).Should().Be(2);

        Repository.GetItem<Planetfall.Item.Computer.Relay>().SpeckDestroyed = true; // cured
        PlanetfallLoreSource.TierOf(Context, provider.ProgressMapper.Map(Context)).Should().Be(3);
    }

    [Test]
    public void InvisicluesSections_UnlockByArea()
    {
        var provider = new PlanetfallHintProvider();

        var atStart = provider.LoreSource.GroundedText(Context, provider.ProgressMapper.Map(Context));
        atStart.Should().Contain("## Aboard the Feinstein");
        atStart.Should().Contain("## The Pod Trip");
        atStart.Should().NotContain("## The Dormitory Area");
        atStart.Should().NotContain("## The Systems and Library Area");

        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        var landed = provider.LoreSource.GroundedText(Context, provider.ProgressMapper.Map(Context));
        landed.Should().Contain("## The Dormitory Area");
        landed.Should().Contain("## The Admin/Mech Area");
        landed.Should().NotContain("## The Systems and Library Area");

        Repository.GetLocation<LawandaPlatform>().VisitCount = 1;
        var lawanda = provider.LoreSource.GroundedText(Context, provider.ProgressMapper.Map(Context));
        lawanda.Should().Contain("## The Systems and Library Area");
        lawanda.Should().NotContain("## For Your Amusement"); // endgame only
    }

    // ---- S. survival --------------------------------------------------------------------------------

    [Test]
    public async Task SickPlayer_MechanicQuestion_SeesTheirCondition_InTheKeyState()
    {
        Context.Day = 4;
        Context.SicknessCounter = 4;

        var result = await Ask("why am I so sick?", new RoutedIntent(HintIntent.Mechanic, false, null));

        result.Kind.Should().Be(HintKind.Mechanic);
        _llm.LastKeyState.Should().Contain("Day 4");
        _llm.LastLoreSource.Should().Contain("The Disease");
    }

    [Test]
    public async Task DiseaseFarAdvanced_ProgressHint_CarriesTheWarning()
    {
        Context.SicknessCounter = PlanetfallHintRules.DiseaseWarningLevel;

        var result = await Ask("what do I do?", RoutedIntent.OpenEnded);

        result.SoftLock.Should().Be(SoftLockKind.Warning);
        result.Text.Should().Contain("Disease");
    }

    [Test]
    public void WellRestedWellFedPlayer_GetsNoSurvivalNudge()
    {
        _service.ProactiveNudges(Context).Should().BeEmpty();
    }
}
