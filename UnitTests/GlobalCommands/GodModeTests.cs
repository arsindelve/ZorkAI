using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.MazeLocation;

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

    // Issue #374 follow-up: Troll.GodModeKill() used to derive the axe's drop location from the
    // troll's own (mutable) CurrentLocation. "god mode take troll" moves that CurrentLocation to the
    // player's inventory, so a subsequent "god mode kill troll" dropped the axe (and left the
    // "corpse") in the player's inventory instead of TrollRoom. Die() now hardcodes TrollRoom for the
    // axe and uses Repository.DestroyItem to correctly detach the troll from wherever he currently is.
    [Test]
    public async Task GodModeKill_Troll_DropsAxeInTrollRoom_EvenIfTrollWasPreviouslyTaken()
    {
        var engine = GetTarget();

        await engine.GetResponse("god mode take troll");
        await engine.GetResponse("god mode kill troll");

        var troll = Repository.GetItem<Troll>();
        troll.IsDead.Should().BeTrue();
        troll.CurrentLocation.Should().BeNull();
        engine.Context.Items.Should().NotContain(troll);
        Repository.GetLocation<TrollRoom>().Items.Should().Contain(Repository.GetItem<BloodyAxe>());
    }

    // Issue #374: "god mode kill thief" mirrors AdventurerVersusThiefCombatEngine.DeathBlow - the
    // thief dies, vanishes, and his stash (including his stiletto) reappears wherever the kill
    // happened, exactly like a real fatal blow would.
    [Test]
    public async Task GodModeKill_Thief_KillsHimAndDropsHisStash()
    {
        var engine = GetTarget();

        var response = await engine.GetResponse("god mode kill thief");

        var thief = Repository.GetItem<Thief>();
        thief.IsDead.Should().BeTrue();
        thief.CurrentLocation.Should().BeNull();
        Repository.GetLocation<WestOfHouse>().Items.Should().Contain(Repository.GetItem<Stiletto>());
        response.Should().Contain("is dead");
    }

    // Issue #374: "god mode kill cyclops" reuses the same removal CyclopsRoom's "say Ulysses" flee
    // handler uses (the cyclops has no combat-death path in this engine), so it opens the same gate:
    // the room's east and up passages, which are blocked while HasItem<Cyclops>() is true.
    [Test]
    public async Task GodModeKill_Cyclops_RemovesHimAndUnblocksThePassage()
    {
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();

        var response = await engine.GetResponse("god mode kill cyclops");

        var cyclops = Repository.GetItem<Cyclops>();
        cyclops.IsDead.Should().BeTrue();
        cyclops.CurrentLocation.Should().BeNull();
        Repository.GetLocation<CyclopsRoom>().Items.Should().NotContain(cyclops);
        response.Should().Contain("is dead");

        await engine.GetResponse("up");
        engine.Context.CurrentLocation.Should().Be(Repository.GetLocation<TreasureRoom>());
    }
}
