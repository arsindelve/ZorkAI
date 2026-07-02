using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;

namespace UnitTests.GlobalCommands;

public class GodModeTests : EngineTestsBase
{
    // Issue #241 follow-up: LoadAllLocations now Init()s every newly-created location, so the
    // explicit location.Init() that GodModeProcessor.Go() also performed ran Init() twice. For any
    // location using StartWithItem<T> (a bare Items.Add with no dedupe, e.g. the Kitchen's window,
    // sack and bottle), that double-Init() duplicated each starting item in the room. "god mode go"
    // must leave each starting item in the room exactly once.
    [Test]
    public async Task GodModeGo_DoesNotDuplicateStartingItems()
    {
        var engine = GetTarget();

        await engine.GetResponse("god mode go kitchen");

        var kitchen = Repository.GetLocation<Kitchen>();
        kitchen.Items.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public async Task GodModeResetTime_IsInvalidOutsidePlanetfall()
    {
        var engine = GetTarget();

        var response = await engine.GetResponse("god mode reset time");

        response.Should().Contain("Invalid use of God mode. Bad adventurer!");
    }

    // Issue #374: "god mode kill troll" lets playtesting skip the Troll Room's randomized combat
    // gate deterministically. It should leave the troll in exactly the state a real fatal blow
    // would: dead, gone from the room, axe dropped for the player to find.
    [Test]
    public async Task GodModeKill_Troll_KillsHimAndDropsTheAxe()
    {
        var engine = GetTarget();

        var response = await engine.GetResponse("god mode kill troll");

        var troll = Repository.GetItem<Troll>();
        troll.IsDead.Should().BeTrue();
        troll.CurrentLocation.Should().BeNull();
        troll.ItemBeingHeld.Should().BeNull();
        Repository.GetLocation<TrollRoom>().Items.Should().Contain(Repository.GetItem<BloodyAxe>());
        response.Should().Contain("is dead");
    }

    [Test]
    public async Task GodModeKill_UnrecognizedTarget_IsInvalid()
    {
        var engine = GetTarget();

        var response = await engine.GetResponse("god mode kill sword");

        response.Should().Contain("Invalid use of God mode. Bad adventurer!");
    }
}
