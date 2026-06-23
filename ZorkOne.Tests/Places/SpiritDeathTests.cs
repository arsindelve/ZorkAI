using FluentAssertions;
using GameEngine;
using ZorkOne.Command;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Tests.Places;

/// <summary>
///     Tests for issue #17 — once the player has visited the altar (South Temple), dying turns them
///     into a spirit at the Entrance to Hades rather than reincarnating them in the forest. They walk
///     back to the altar and pray to be resurrected. A third death (or being killed while already a
///     spirit) is a permanent game over. See JIGS-UP / DEAD-FUNCTION in the original ZIL.
/// </summary>
[TestFixture]
public class SpiritDeathTests : EngineTestsBase
{
    [Test]
    public async Task Death_AfterVisitingAltar_BecomesSpiritAtEntranceToHades()
    {
        var target = GetTarget();

        // Visiting the altar is the trigger for the spirit-death mechanic.
        Repository.GetLocation<Altar>().VisitCount = 1;

        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        var response = await target.GetResponse("jump");

        response.Should().Contain("gates of Hell");
        response.Should().Contain("indistinct");
        target.Context.CurrentLocation.Should().BeOfType<EntranceToHades>();
        target.Context.IsDead.Should().BeTrue();
    }

    [Test]
    public async Task Death_WithoutVisitingAltar_ReincarnatesInForest()
    {
        var target = GetTarget();

        // Never visited the altar -> the familiar forest reincarnation.
        Repository.GetLocation<Altar>().VisitCount.Should().Be(0);

        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        var response = await target.GetResponse("jump");

        response.Should().Contain("died");
        target.Context.CurrentLocation.Should().BeOfType<ForestOne>();
        target.Context.IsDead.Should().BeFalse();
    }

    [Test]
    public async Task ThirdDeath_IsPermanentGameOver()
    {
        var target = GetTarget();
        target.Context.DeathCounter = 2; // already died twice; this is the third death

        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        var response = await target.GetResponse("jump");

        response.Should().Contain("suicidal");
        response.Should().Contain("Land of the Living Dead");
    }

    [Test]
    public async Task ThirdDeath_EndsTheGame_SubsequentCommandsAreBlocked()
    {
        var target = GetTarget();
        target.Context.DeathCounter = 2; // this jump is the third death
        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        await target.GetResponse("jump");

        // The game is over: no further action is possible, not even moving.
        (await target.GetResponse("look")).Should().Contain("adventure has come to an end");
        (await target.GetResponse("north")).Should().Contain("adventure has come to an end");
        target.Context.HasPermanentlyDied.Should().BeTrue();
    }

    [Test]
    public async Task KilledWhileASpirit_EndsTheGame_CannotPrayBackToLife()
    {
        var target = GetTarget();
        target.Context.IsDead = true;

        // A spirit dies again -> permanent game over.
        new DeathProcessor().Process("You drown. ", target.Context);
        target.Context.HasPermanentlyDied.Should().BeTrue();

        // Even praying at the altar can no longer bring them back.
        target.Context.CurrentLocation = Repository.GetLocation<Altar>();
        var response = await target.GetResponse("pray");

        response.Should().Contain("adventure has come to an end");
        target.Context.CurrentLocation.Should().BeOfType<Altar>();
    }

    [Test]
    public async Task Death_CostsTenPoints()
    {
        var target = GetTarget();
        target.Context.AddPoints(50);
        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        await target.GetResponse("jump");

        target.Context.Score.Should().Be(40);
    }

    [Test]
    public async Task SpiritDeath_ScattersInventory_LampGoesToLivingRoom()
    {
        var target = GetTarget();
        Repository.GetLocation<Altar>().VisitCount = 1;

        var lantern = Repository.GetItem<Lantern>();
        target.Context.Take(lantern);

        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        await target.GetResponse("jump");

        target.Context.HasItem<Lantern>().Should().BeFalse();
        lantern.CurrentLocation.Should().Be(Repository.GetLocation<LivingRoom>());
    }

    [Test]
    public async Task AsSpirit_VerbsAreOverridden()
    {
        var target = GetTarget();
        target.Context.IsDead = true;
        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();

        (await target.GetResponse("wait")).Should().Contain("eternity");
        (await target.GetResponse("score")).Should().Contain("How can you think of your score");
        (await target.GetResponse("inventory")).Should().Contain("no possessions");
        (await target.GetResponse("diagnose")).Should().Contain("You are dead");
        (await target.GetResponse("look")).Should().Contain("strange and unearthly");
        (await target.GetResponse("take bell")).Should().Contain("hand passes through");
    }

    [Test]
    public async Task AsSpirit_PrayingAtAltar_Resurrects()
    {
        var target = GetTarget();
        target.Context.IsDead = true;
        target.Context.CurrentLocation = Repository.GetLocation<Altar>();

        var response = await target.GetResponse("pray");

        response.Should().Contain("trumpet");
        target.Context.IsDead.Should().BeFalse();
        target.Context.CurrentLocation.Should().BeOfType<ForestOne>();
    }

    [Test]
    public async Task AsSpirit_PrayingAwayFromAltar_IsNotHeard()
    {
        var target = GetTarget();
        target.Context.IsDead = true;
        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();

        var response = await target.GetResponse("pray");

        response.Should().Contain("prayers are not heard");
        target.Context.IsDead.Should().BeTrue();
    }

    [Test]
    public void AsSpirit_IsAlwaysLit_EvenInADarkRoom()
    {
        var target = GetTarget();
        target.Context.IsDead = true;
        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();

        target.Context.ItIsDarkHere.Should().BeFalse();
    }

    [Test]
    public async Task KilledWhileAlreadyDead_IsPermanentGameOver()
    {
        var target = GetTarget();
        target.Context.IsDead = true;

        var result = new DeathProcessor().Process("You drown. ", target.Context);

        result.InteractionMessage.Should().Contain("already dead");
    }
}
