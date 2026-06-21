using GameEngine;
using ZorkOne;
using ZorkOne.Item;
using ZorkOne.Location.CoalMineLocation;

namespace UnitTests.SingleNounProcessors;

/// <summary>
///     Tests that the matchbook matches the original Zork I behavior: six usable matches,
///     each lit match burns for two turns, and a match struck in a drafty room (the lower
///     shaft / Drafty Room or the Timber Room) goes out instantly. See issue #193.
/// </summary>
[TestFixture]
public class MatchbookTests : EngineTestsBase
{
    [Test]
    public async Task SixthMatch_CanStillBeLit()
    {
        var engine = GetTarget();
        var matchbook = Take<Matchbook>();
        StartHere<LivingRoom>();

        // Five matches already used - the sixth should still light.
        matchbook.MatchesUsed = 5;

        var result = await engine.GetResponse("light matchbook");

        result.Should().Contain("One of the matches starts to burn.");
        matchbook.IsOn.Should().BeTrue();
        matchbook.MatchesUsed.Should().Be(6);
    }

    [Test]
    public async Task SeventhMatch_RunsOut()
    {
        var engine = GetTarget();
        var matchbook = Take<Matchbook>();
        StartHere<LivingRoom>();

        // All six matches have been used.
        matchbook.MatchesUsed = 6;

        var result = await engine.GetResponse("light matchbook");

        result.Should().Contain("run out of matches");
        matchbook.IsOn.Should().BeFalse();
    }

    [Test]
    public async Task LitMatch_BurnsForTwoTurns_BeforeGoingOut()
    {
        var engine = GetTarget();
        var matchbook = Take<Matchbook>();
        StartHere<LivingRoom>();

        await engine.GetResponse("light matchbook");
        matchbook.IsOn.Should().BeTrue("the match is burning on the turn it was struck");

        await engine.GetResponse("wait");
        matchbook.IsOn.Should().BeTrue("the match should still be burning one turn after being struck");

        var result = await engine.GetResponse("wait");
        matchbook.IsOn.Should().BeFalse("the match should burn out two turns after being struck");
        result.Should().Contain("The match has gone out");
    }

    [Test]
    public async Task LightingMatch_InDraftyRoom_GoesOutInstantly()
    {
        var engine = GetTarget();
        var matchbook = Take<Matchbook>();
        // The lantern keeps the drafty room lit so we can read the response plainly.
        var lantern = Take<Lantern>();
        lantern.IsOn = true;
        engine.Context.CurrentLocation = Repository.GetLocation<DraftyRoom>();

        var result = await engine.GetResponse("light matchbook");

        result.Should().Contain("drafty");
        matchbook.IsOn.Should().BeFalse();
        // The match is still consumed even though the draft snuffs it instantly.
        matchbook.MatchesUsed.Should().Be(1);
    }

    [Test]
    public async Task LightingMatch_InTimberRoom_GoesOutInstantly()
    {
        var engine = GetTarget();
        var matchbook = Take<Matchbook>();
        var lantern = Take<Lantern>();
        lantern.IsOn = true;
        engine.Context.CurrentLocation = Repository.GetLocation<TimberRoom>();

        var result = await engine.GetResponse("light matchbook");

        result.Should().Contain("drafty");
        matchbook.IsOn.Should().BeFalse();
        matchbook.MatchesUsed.Should().Be(1);
    }
}
