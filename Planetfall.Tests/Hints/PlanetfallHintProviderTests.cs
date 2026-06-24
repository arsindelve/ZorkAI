using FluentAssertions;
using GameEngine;
using GameEngine.Hints;
using Model.Hints;
using Planetfall.Hints;

namespace Planetfall.Tests.Hints;

/// <summary>
///     Tests the Planetfall provider's wiring: the knowledge bundle is the real game source + walkthroughs,
///     the player-context is the real serialized save game, and the survival nudges fire. No OpenAI.
/// </summary>
[TestFixture]
public class PlanetfallHintProviderTests : EngineTestsBase
{
    [SetUp]
    public void SetUp() => GetTarget(); // Repository.Reset() + a real engine/context (Context, GetTarget)

    private static PlanetfallHintProvider Provider() => new();

    [Test]
    public void Docs_AreTheRealSourceAndWalkthroughs()
    {
        var docs = Provider().Docs;
        docs.Should().Contain("GAME SOURCE");
        docs.Should().Contain("VERIFIED WALKTHROUGH");
        // a real class only present in the actual source:
        docs.Should().Contain("class AdminCorridor");
        // a real walkthrough TestCase line, from the one complete walkthrough:
        docs.Should().Contain("[TestCase(");
        docs.Should().Contain("WalkthroughTestOne");
        // the partial/situation-specific walkthroughs must NOT be bundled:
        docs.Should().NotContain("WalkthroughMutantChase");
        docs.Should().NotContain("WalkthroughBioLock");
        // the lore + dialect pointer is in the preamble:
        docs.Should().Contain("library computer");
    }

    [Test]
    public void DescribePlayerContext_LeadsWithKeyState_ThenFullSaveGame()
    {
        var context = Provider().DescribePlayerContext(Context);
        // Salient highlight up top so the model can't miss decision-critical flags...
        context.Should().StartWith("KEY STATE");
        context.Should().Contain("Floyd:");
        // ...then the complete save-game JSON behind it.
        context.Should().Contain("AllItems");
        context.Should().Contain("AllLocations");
    }

    [Test]
    public void KeyState_ReflectsFloydDeath()
    {
        Repository.GetItem<Planetfall.Item.Kalamontee.Mech.FloydPart.Floyd>().HasDied = true;
        Provider().DescribePlayerContext(Context).Should().Contain("Floyd: DEAD");
    }

    [Test]
    public void DescribePlayerContext_ReflectsLiveState()
    {
        // Move the player and assert the serialized context actually changes — proving it's live state.
        var before = Provider().DescribePlayerContext(Context);
        Context.CurrentLocation = Repository.GetLocation<Planetfall.Location.Kalamontee.StorageWest>();
        var after = Provider().DescribePlayerContext(Context);
        after.Should().NotBe(before);
    }

    [Test]
    public void TiredPlayer_ProducesASleepNudge()
    {
        Context.Tired = TiredLevel.Tired;
        var service = new HintService(Provider(), new InMemoryHintMemoryStore(), new NullLlm());
        service.ProactiveNudges(Context).Should().Contain(n => n.Category == "sleep");
    }
}

/// <summary>No-op LLM for the proactive test (which never calls the model).</summary>
internal sealed class NullLlm : IHintLanguageModel
{
    public Task<string> Solve(string docs, string playerContext, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona) => Task.FromResult("");

    public Task<string> Reveal(string playerContext, string solution, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona) => Task.FromResult("");
}
