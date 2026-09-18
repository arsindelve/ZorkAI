using FluentAssertions;
using GameEngine;
using GameEngine.Hints;
using Planetfall.Hints;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Tower;
using Planetfall.Location.Shuttle;
using UnitTests.Hints;

namespace Planetfall.Tests.Hints;

/// <summary>
///     Drives the real <see cref="PlanetfallHintProvider" /> against live Planetfall state (real Repository +
///     Context, the repo's standard test pattern) with a deterministic stub LLM. Covers the provider's wiring:
///     the progress mapper reads the real flags, the knowledge bundle is the real source, the nudges fire.
/// </summary>
[TestFixture]
public class PlanetfallHintProviderTests : EngineTestsBase
{
    [SetUp]
    public void SetUp() => GetTarget(); // Repository.Reset() + engine + a real PlanetfallContext (Context)

    private static PlanetfallHintProvider Provider() => new();

    private static ProgressState Progress(PlanetfallContext context) => Provider().ProgressMapper.Map(context);

    [Test]
    public void Docs_AreTheRealSourceAndWalkthroughs()
    {
        var docs = Provider().Docs;
        docs.Should().Contain("GAME SOURCE");
        docs.Should().Contain("VERIFIED WALKTHROUGH");
        docs.Should().Contain("class AdminCorridor"); // a real class only present in the actual source
        docs.Should().Contain("[TestCase(");
        docs.Should().Contain("WalkthroughTestOne");
        // the partial/situation-specific walkthroughs must NOT be bundled:
        docs.Should().NotContain("WalkthroughMutantChase");
        docs.Should().NotContain("WalkthroughBioLock");
        // the invisiclues are NOT swept into the source bundle (they're tier-gated separately)...
        docs.Should().NotContain("## Aboard the Feinstein");
        // ...and neither is the hint plumbing, which holds the untiered lore digest and every solution rung:
        docs.Should().NotContain("shot the Feinstein down");
        docs.Should().NotContain("class PlanetfallHintCorpus");
        docs.Should().NotContain("class PlanetfallPuzzleGraph");
    }

    [Test]
    public void Persona_CarriesTheGameIdentityAndStateGrounding()
    {
        // The shared solver prompt is game-agnostic; Planetfall's identity and the flags that matter to it
        // (Floyd alive/dead, health) travel on the persona, not baked into ZorkAI.OpenAI (issue #484).
        var persona = Provider().Persona;
        persona.GameName.Should().Be(new PlanetfallGame().GameName);
        persona.StateGrounding.Should().Contain("Floyd");
        persona.SystemPrompt.Should().Contain("narrator");
        persona.SystemPrompt.Should().Contain("never replaces"); // a joke decorates, never substitutes
    }

    [Test]
    public void DescribePlayerContext_LeadsWithKeyState_ThenFullSaveGame()
    {
        var context = Provider().DescribePlayerContext(Context);
        context.Should().StartWith("KEY STATE");
        context.Should().Contain("Floyd:");
        context.Should().Contain("AllItems");
        context.Should().Contain("AllLocations");
    }

    [Test]
    public void KeyState_ReflectsFloydDeath()
    {
        Repository.GetItem<Floyd>().HasDied = true;
        Provider().DescribeKeyState(Context).Should().Contain("Floyd: DEAD");
    }

    // ---- progress mapping against real state ----------------------------------------------------

    [Test]
    public void FreshGame_HasOnlyTheOpeningPuzzleOpen()
    {
        var progress = Progress(Context);

        progress.StatusOf("ESCAPE_POD").Should().Be(NodeStatus.Available);
        progress.StatusOf("LAND").Should().Be(NodeStatus.Locked);
        progress.StatusOf("MAGNET").Should().Be(NodeStatus.Locked);
        progress.StatusOf("PLIERS").Should().Be(NodeStatus.Locked);
        progress.Nodes.Values.Should().NotContain(NodeStatus.Done);
    }

    [Test]
    public void FloydActivated_BackfillsTheOpening()
    {
        Repository.GetItem<Floyd>().HasEverBeenOn = true;

        var progress = Progress(Context);

        progress.IsDone("FLOYD").Should().BeTrue();
        progress.IsDone("LAND").Should().BeTrue(); // you can't have woken Floyd without landing
        progress.IsDone("ESCAPE_POD").Should().BeTrue();
        progress.StatusOf("MAGNET").Should().Be(NodeStatus.Available);
    }

    [Test]
    public void CarryingTheLadder_BackfillsTheKeyChain_AndOpensTheRift()
    {
        Take<Ladder>();

        var progress = Progress(Context);

        progress.IsDone("LADDER").Should().BeTrue();
        progress.IsDone("STORAGE_WEST").Should().BeTrue(); // back-filled: the ladder was behind the padlock
        progress.IsDone("STEEL_KEY").Should().BeTrue();
        progress.IsDone("MAGNET").Should().BeTrue();
        progress.StatusOf("CROSS_RIFT").Should().Be(NodeStatus.Available);
        progress.StatusOf("KITCHEN_CARD").Should().Be(NodeStatus.Locked); // not until they've crossed
    }

    [Test]
    public void LadderAcrossTheRift_OpensTheOffices()
    {
        Repository.GetItem<Ladder>().IsAcrossRift = true;

        var progress = Progress(Context);

        progress.IsDone("CROSS_RIFT").Should().BeTrue();
        progress.StatusOf("KITCHEN_CARD").Should().Be(NodeStatus.Available);
        progress.StatusOf("SHUTTLE_CARD").Should().Be(NodeStatus.Available);
    }

    [Test]
    public void TakingThePliersEarly_DoesNotPretendTheyHaveReachedLawanda()
    {
        // The pliers sit in the Kalamontee Tool Room beside the magnet. Picking them up must not back-fill
        // the shuttle ride and everything before it.
        Take<Pliers>();

        var progress = Progress(Context);

        progress.IsDone("PLIERS").Should().BeTrue();
        progress.IsDone("LAND").Should().BeTrue();
        progress.IsDone("SHUTTLE").Should().BeFalse();
        progress.IsDone("CROSS_RIFT").Should().BeFalse();
        PlanetfallLoreSource.TierOf(Context, progress).Should().BeLessThan(2);
    }

    [Test]
    public void ReachingTheTowerCore_CompletesTheElevatorChain()
    {
        // The elevator doors open programmatically and never record "opened"; having been in the room is
        // the signal.
        Repository.GetLocation<TowerCore>().VisitCount = 1;

        var progress = Progress(Context);

        progress.IsDone("TOWER_UP").Should().BeTrue();
        progress.IsDone("OPEN_ELEVATOR").Should().BeTrue();
        progress.IsDone("UPPER_CARD").Should().BeTrue();
        progress.IsDone("FILL_FLASK_A").Should().BeTrue();
        progress.IsDone("CROSS_RIFT").Should().BeTrue();
        progress.StatusOf("COMM_FIX").Should().Be(NodeStatus.Available);
    }

    [Test]
    public void ArrivingOnTheLawandaPlatform_CompletesTheShuttle_AndStaysComplete()
    {
        Repository.GetLocation<LawandaPlatform>().VisitCount = 1;
        StartHere<Planetfall.Location.Kalamontee.Crag>(); // and then they walked all the way back

        var progress = Progress(Context);

        progress.IsDone("SHUTTLE").Should().BeTrue();
        progress.IsDone("LOWER_ELEVATOR").Should().BeTrue();
        progress.StatusOf("LASER").Should().Be(NodeStatus.Available);
    }

    [Test]
    public void ReachingTheKalamonteePlatform_CompletesTheLowerElevator()
    {
        Repository.GetLocation<KalamonteePlatform>().VisitCount = 1;

        var progress = Progress(Context);

        progress.IsDone("LOWER_ELEVATOR").Should().BeTrue();
        progress.IsDone("LOWER_CARD").Should().BeTrue();
        progress.IsDone("KITCHEN").Should().BeTrue();
    }

    [Test]
    public void CommunicationsFixed_BackfillsTheEntireTowerChain()
    {
        Repository.GetLocation<SystemsMonitors>().Fixed.Add("KUMUUNIKAASHUNZ");

        var progress = Progress(Context);

        progress.IsDone("COMM_FIX").Should().BeTrue();
        progress.IsDone("TOWER_UP").Should().BeTrue();
        progress.IsDone("FILL_FLASK_A").Should().BeTrue();
        progress.IsDone("CROSS_RIFT").Should().BeTrue();
        progress.IsDone("ESCAPE_POD").Should().BeTrue();
    }

    [Test]
    public void TheTowerChain_IsOptional_SoTheSpineComesFirst()
    {
        // Across the rift with the ladder: the offices (mandatory) must be hinted before the flask and the
        // upper card, which only serve the optional communications repair.
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        Repository.GetItem<Ladder>().IsAcrossRift = true;

        var provider = Provider();
        var blockers = provider.PuzzleGraph.ActiveBlockers(provider.ProgressMapper.Map(Context), Context);

        blockers.First().Should().Be("KITCHEN_CARD");
        blockers.Should().Contain("UPPER_CARD").And.Contain("FLASK");
        provider.PuzzleGraph.Nodes.Where(n => n.Optional).Select(n => n.Id)
            .Should().Contain(["UPPER_CARD", "FLASK", "FILL_FLASK_A", "OPEN_ELEVATOR", "TOWER_UP", "PLIERS"]);
    }

    [Test]
    public void TheOptionalRepairs_AreNeverTheFirstBlocker()
    {
        Repository.GetLocation<SystemsMonitors>().Fixed.Add("KUMUUNIKAASHUNZ");
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        Repository.GetItem<Floyd>().HasGottenTheFromitzBoard = true; // implies SHUTTLE

        var provider = Provider();
        var blockers = provider.PuzzleGraph.ActiveBlockers(provider.ProgressMapper.Map(Context), Context);

        blockers.First().Should().Be("LASER");
        blockers.Should().Contain("DEFENSE_FIX"); // still offered, just not first
    }

    // ---- rules --------------------------------------------------------------------------------------

    [Test]
    public void TiredPlayer_ProducesASleepNudge()
    {
        Context.Tired = TiredLevel.Tired;

        var service = new HintService(Provider(), new StubLlm());

        service.ProactiveNudges(Context).Should().Contain(n => n.Category == "sleep");
    }

    [Test]
    public void DiseaseFarAdvanced_IsAWarning_NotAHardLock()
    {
        Context.SicknessCounter = PlanetfallHintRules.DiseaseWarningLevel;

        var verdicts = Provider().SoftLockRules.Select(r => r.Evaluate(Context, Progress(Context))).ToList();

        verdicts.Should().Contain(v => v.Kind == SoftLockKind.Warning);
        verdicts.Should().NotContain(v => v.Kind == SoftLockKind.Hard);
    }

    [Test]
    public void TheDiseaseRules_FollowTheSicknessClock_NotTheCalendar()
    {
        // Day 6 but treated with the experimental medicine: the game says mildly ill, so must the hints.
        Context.Day = 6;
        Context.SicknessCounter = 2;

        var provider = Provider();
        provider.SoftLockRules.Select(r => r.Evaluate(Context, Progress(Context)))
            .Should().OnlyContain(v => v.Kind == SoftLockKind.None);
        new HintService(provider, new StubLlm()).ProactiveNudges(Context)
            .Should().NotContain(n => n.Category == "disease");
    }
}
