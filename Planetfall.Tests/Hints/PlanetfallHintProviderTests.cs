using FluentAssertions;
using GameEngine;
using GameEngine.Hints;
using Planetfall.Hints;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Admin;

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
        // and the invisiclues are NOT swept into the source bundle (they're tier-gated separately):
        docs.Should().NotContain("## Aboard the Feinstein");
    }

    [Test]
    public void Persona_CarriesTheGameIdentityAndStateGrounding()
    {
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
        progress.StatusOf("UPPER_CARD").Should().Be(NodeStatus.Locked); // not until they've crossed
    }

    [Test]
    public void LadderAcrossTheRift_OpensTheOffices()
    {
        Repository.GetItem<Ladder>().IsAcrossRift = true;

        var progress = Progress(Context);

        progress.IsDone("CROSS_RIFT").Should().BeTrue();
        progress.StatusOf("UPPER_CARD").Should().Be(NodeStatus.Available);
        progress.StatusOf("SHUTTLE_CARD").Should().Be(NodeStatus.Available);
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
    public void TheOptionalRepairs_AreNeverTheFirstBlocker()
    {
        // Everything up to the shuttle is done; the open set holds the mandatory Lawanda chain and the
        // optional systems. The mandatory spine must come first.
        Repository.GetLocation<SystemsMonitors>().Fixed.Add("KUMUUNIKAASHUNZ");
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        Repository.GetItem<Floyd>().HasGottenTheFromitzBoard = true; // implies SHUTTLE

        var provider = Provider();
        var progress = provider.ProgressMapper.Map(Context);
        var blockers = provider.PuzzleGraph.ActiveBlockers(progress, Context);

        blockers.First().Should().NotBe("DEFENSE_FIX");
        blockers.Should().Contain("DEFENSE_FIX"); // still offered, just not first
    }

    // ---- rules --------------------------------------------------------------------------------------

    [Test]
    public void TiredPlayer_ProducesASleepNudge()
    {
        Context.Tired = TiredLevel.Tired;

        var service = new HintService(Provider(), new RoutingStubLlm());

        service.ProactiveNudges(Context).Should().Contain(n => n.Category == "sleep");
    }

    [Test]
    public void DiseaseLateInTheGame_IsAWarning_NotAHardLock()
    {
        Context.Day = 7;

        var verdicts = Provider().SoftLockRules.Select(r => r.Evaluate(Context, Progress(Context))).ToList();

        verdicts.Should().Contain(v => v.Kind == SoftLockKind.Warning);
        verdicts.Should().NotContain(v => v.Kind == SoftLockKind.Hard);
    }
}
