using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

// Issue #337: Shore and Sandy Beach both expose the river in their room descriptions, but neither
// location wired up the original ZIL "local-global" water/river objects
// (zork1/1dungeon.zil: both SHORE and SANDY-BEACH declare `(GLOBAL GLOBAL-WATER RIVER)`), so
// swim/drink/take fell through to the engine's generic no-op responses instead of the original
// handlers.
public class ShoreTests : EngineTestsBase
{
    [TestCase("swim")]
    [TestCase("swim in river")]
    [TestCase("swim in water")]
    [TestCase("bathe")]
    public async Task Swim_AtShore_GivesTheOriginalRefusal(string input)
    {
        var target = GetTarget();
        StartHere<Shore>();

        var response = await target.GetResponse(input);

        response.Should().Contain("Swimming isn't usually allowed in the dungeon.");
    }

    [Test]
    public async Task DrinkWater_AtShore_QuenchesThirst()
    {
        var target = GetTarget();
        StartHere<Shore>();

        var response = await target.GetResponse("drink water");

        response.Should().Contain("Thank you very much. I was rather thirsty (from all this talking, probably).");
    }

    [Test]
    public async Task TakeWater_AtShore_SlipsThroughFingers()
    {
        var target = GetTarget();
        StartHere<Shore>();

        var response = await target.GetResponse("take water");

        response.Should().Contain("The water slips through your fingers.");
    }

    [Test]
    public async Task ExamineRiver_AtShore_DescribesIt()
    {
        var target = GetTarget();
        StartHere<Shore>();

        var response = await target.GetResponse("examine river");

        response.Should().Contain("The river is wide, swift, and cold.");
    }

    [Test]
    public async Task Swim_AtSandyBeach_GivesTheOriginalRefusal_EvenInTheDark()
    {
        // Sandy Beach is a DarkLocation, but RespondToSpecificLocationInteraction (like the
        // existing "launch" handling here) runs before any darkness gate, so this must work
        // without a light source too.
        var target = GetTarget();
        StartHere<SandyBeach>();

        var response = await target.GetResponse("swim in river");

        response.Should().Contain("Swimming isn't usually allowed in the dungeon.");
    }

    [Test]
    public async Task DrinkWater_AtSandyBeach_QuenchesThirst()
    {
        var target = GetTarget();
        StartHere<SandyBeach>();
        Take<Lantern>();
        GetItem<Lantern>().IsOn = true;

        var response = await target.GetResponse("drink water");

        response.Should().Contain("Thank you very much. I was rather thirsty (from all this talking, probably).");
    }
}
