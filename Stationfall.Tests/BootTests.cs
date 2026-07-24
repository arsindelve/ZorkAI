using FluentAssertions;

namespace Stationfall.Tests;

/// <summary>
///     Phase 1 smoke tests: the game boots through the engine and the starting room works. This is the
///     end-to-end wiring check that everything (game class, context, first location, global commands)
///     is connected before Phase 2 starts adding real ship rooms and the departure walkthrough.
/// </summary>
[TestFixture]
public class BootTests : EngineTestsBase
{
    [Test]
    public async Task Look_InDeckTwelve_ShowsTheRoomAndItsDescription()
    {
        var engine = GetTarget();

        var response = await engine.GetResponse("look");

        response.Should().Contain("Deck Twelve");
        response.Should().Contain("Stellar Patrol Ship Duffy");
    }

    [Test]
    public void CurrentScore_AtStart_UsesThe80PointScaleAndBeginnerRank()
    {
        var engine = GetTarget();

        engine.Context.CurrentScore.Should().Contain("out of 80 points");
        engine.Context.CurrentScore.Should().Contain("Day 1");
        engine.Context.CurrentScore.Should().Contain("Insignificant Nobody");
    }

    [Test]
    public async Task GameName_IsStationfall()
    {
        var engine = GetTarget();

        // Sanity that the right game is wired into the engine.
        await Task.CompletedTask;
        engine.Context.Game.GameName.Should().Be("Stationfall");
    }
}
