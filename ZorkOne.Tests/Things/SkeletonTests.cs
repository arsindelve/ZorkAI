using FluentAssertions;
using GameEngine;
using Model.Intent;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests.Things;

/// <summary>
/// Issue #36: "messing with" the skeleton in Maze Five must trigger the classic skeleton curse —
/// a ghost banishes everything in the room and the player's entire inventory to the Land of the
/// Living Dead. The original SKELETON action routine (zork1/1actions.zil:931) fires on
/// TAKE / RUB / MOVE / PUSH / RAISE / LOWER / ATTACK / KICK / KISS.
/// </summary>
[TestFixture]
public class SkeletonTests : EngineTestsBase
{
    /// <summary>
    /// Puts the player in Maze Five with the original room contents and a lit lamp plus a treasure
    /// in inventory, so the room is visible and we have something to banish.
    /// </summary>
    private (GameEngine<ZorkI, ZorkIContext> target, Sword treasure, Lantern lamp) ArriveAtSkeleton()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<MazeFive>();
        room.Init();

        var lamp = Repository.GetItem<Lantern>();
        lamp.IsOn = true;
        target.Context.Take(lamp);

        var treasure = Repository.GetItem<Sword>();
        target.Context.Take(treasure);

        target.Context.CurrentLocation = room;
        return (target, treasure, lamp);
    }

    [TestCase("take skeleton")]
    [TestCase("move skeleton")]
    [TestCase("rub skeleton")]
    [TestCase("push skeleton")]
    [TestCase("kick skeleton")]
    [TestCase("kiss skeleton")]
    [TestCase("raise skeleton")]
    [TestCase("lower skeleton")]
    public async Task MessingWithSkeleton_TriggersCurse(string command)
    {
        var (target, treasure, lamp) = ArriveAtSkeleton();

        var response = await target.GetResponse(command);

        // The full ghost-and-curse message (the curse sentence used to be a //TODO).
        response.Should().Contain("ghost");
        response.Should().Contain("Land of the Living Dead");

        var dead = Repository.GetLocation<LandOfTheDead>();

        // The player's entire inventory is banished — lamp included.
        target.Context.Items.Should().BeEmpty();
        treasure.CurrentLocation.Should().Be(dead);
        lamp.CurrentLocation.Should().Be(dead);

        // Every other item in the room is banished too...
        Repository.GetItem<RustyKnife>().CurrentLocation.Should().Be(dead);
        Repository.GetItem<SkeletonKey>().CurrentLocation.Should().Be(dead);
        Repository.GetItem<BagOfCoins>().CurrentLocation.Should().Be(dead);
        Repository.GetItem<BurnedOutLantern>().CurrentLocation.Should().Be(dead);

        // ...but the bones themselves stay put (the original robs the room's other contents only).
        Repository.GetItem<Skeleton>().CurrentLocation.Should().Be(Repository.GetLocation<MazeFive>());
    }

    // ATTACK (and its synonyms — kill/stab/etc.) is part of the curse verb set, but the unit-test
    // intent parser doesn't route bare "attack <noun>" to a simple interaction (combat only engages
    // registered foes), so it can't be exercised through GetResponse. Drive the handler directly to
    // prove the KillVerbs branch fires the curse and banishes the room + inventory.
    [TestCase("attack")]
    [TestCase("kill")]
    [TestCase("stab")]
    public async Task AttackSkeleton_FiresCurse_ViaHandler(string verb)
    {
        var (target, treasure, lamp) = ArriveAtSkeleton();
        var skeleton = Repository.GetItem<Skeleton>();

        var result = await skeleton.RespondToSimpleInteraction(
            new SimpleIntent { Verb = verb, Noun = "skeleton", OriginalInput = $"{verb} skeleton" },
            target.Context, Client.Object, null!);

        result.Should().NotBeNull();
        result!.InteractionMessage.Should().Contain("ghost");
        result.InteractionMessage.Should().Contain("Land of the Living Dead");

        var dead = Repository.GetLocation<LandOfTheDead>();
        target.Context.Items.Should().BeEmpty();
        treasure.CurrentLocation.Should().Be(dead);
        lamp.CurrentLocation.Should().Be(dead);
        Repository.GetItem<RustyKnife>().CurrentLocation.Should().Be(dead);
        Repository.GetItem<Skeleton>().CurrentLocation.Should().Be(Repository.GetLocation<MazeFive>());
    }

    [Test]
    public async Task TakeSkeleton_ResolvesToSkeleton_NoDisambiguation()
    {
        var (target, _, _) = ArriveAtSkeleton();

        var response = await target.GetResponse("take skeleton");

        // "skeleton" must unambiguously mean the bones — not prompt "skeleton key or ...?".
        response.Should().NotContain("Do you mean");
        response.Should().Contain("ghost");
    }

    [Test]
    public async Task TakeKey_StillResolvesToTheKey()
    {
        var (target, _, _) = ArriveAtSkeleton();

        var response = await target.GetResponse("take key");

        response.Should().NotContain("Do you mean");
        target.Context.HasItem<SkeletonKey>().Should().BeTrue();
        // Picking up the key must not have fired the curse.
        Repository.GetItem<Skeleton>().CurrentLocation.Should().Be(Repository.GetLocation<MazeFive>());
    }

    // The disambiguation prompt for "take skeleton" was caused by the key over-claiming the bare
    // noun "skeleton". In the original, "skeleton" is only the key's *adjective* — so the bones own
    // the noun, and "skeleton key" still resolves to the key.
    [Test]
    public void NounOwnership_SkeletonIsTheBones_KeyKeepsItsAdjectivePhrase()
    {
        GetTarget();
        var bones = Repository.GetItem<Skeleton>();
        var key = Repository.GetItem<SkeletonKey>();

        bones.HasMatchingNoun("skeleton").HasItem.Should().BeTrue();
        key.HasMatchingNoun("skeleton").HasItem.Should().BeFalse();

        key.HasMatchingNoun("key").HasItem.Should().BeTrue();
        key.HasMatchingNoun("skeleton key").HasItem.Should().BeTrue();
    }
}
