using FluentAssertions;
using GameEngine;
using NUnit.Framework;
using UnitTests;
using ZorkOne.Item;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests;

public class CyclopsTests : EngineTestsBase
{
    [Test]
    public async Task CantGoEastWhileAlive()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();

        var response = await target.GetResponse("E");

        response.Should().Contain("The east wall is solid rock");
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<CyclopsRoom>());
    }

    [Test]
    public async Task CantGoUpWhileAlive()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();

        var response = await target.GetResponse("Up");

        response.Should().Contain("The cyclops doesn't look like he'll let you past");
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<CyclopsRoom>());
    }

    [Test]
    public async Task Odysseus_Response()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        var response = await target.GetResponse("ulysses");

        response.Should()
            .Contain(
                "The cyclops, hearing the name of his father's deadly nemesis, flees the room by knocking down the wall on the east of the room");
    }

    [Test]
    public async Task AfterOdysseus_CanGoEast()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        await target.GetResponse("ulysses");
        var response = await target.GetResponse("E");

        response.Should().Contain("Strange Passage");
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<StrangePassage>());
    }

    [Test]
    public async Task AfterOdysseus_SayingItAgainGivesDefaultResponse()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        await target.GetResponse("ulysses");
        var response = await target.GetResponse("ulysses");

        response.Should().Contain("sailor");
    }

    [Test]
    public async Task AfterOdysseus_CanGoUp()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();

        await target.GetResponse("ulysses");
        var response = await target.GetResponse("Up");

        response.Should().Contain("Treasure Room");
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<TreasureRoom>());
    }

    [Test]
    public async Task BeforeOdysseus_CyclopsRoomDescription()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        var response = await target.GetResponse("look");

        response.Should()
            .Contain("A cyclops, who looks prepared to eat horses (much less mere adventurers), blocks the staircase");
    }

    [Test]
    public async Task AfterOdysseus_CyclopsRoomDescriptionChanges()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        await target.GetResponse("ulysses");
        var response = await target.GetResponse("look");

        response.Should().Contain("The east wall, previously solid, now has a cyclops-sized opening in it");
    }

    [Test]
    public async Task AfterOdysseus_IHaveTheSword_SwordStopsGlowing()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Sword>());

        var response = await target.GetResponse("ulysses");

        response.Should().Contain("no longer glowing");
    }

    [Test]
    public async Task AfterOdysseus_IDontHaveTheSword()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        var response = await target.GetResponse("ulysses");

        response.Should().NotContain("no longer glowing");
    }

    [Test]
    public async Task AfterOdysseus_LivingRoomDescriptionChanges()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        await target.GetResponse("ulysses");

        Repository.GetItem<Cyclops>().CurrentLocation.Should().BeNull();
    }

    [Test]
    public async Task AfterOdysseus_HesGone()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        await target.GetResponse("ulysses");
        await target.GetResponse("E");
        await target.GetResponse("E");
        var response = await target.GetResponse("look");

        response.Should().Contain("To the west is a cyclops-shaped opening in an old wooden door");
    }

    [Test]
    public async Task WhenAgitated_RoomDescriptionChanges()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Sword>());

        await target.GetResponse("kill the cyclops with the sword");
        var response = await target.GetResponse("look");

        response.Should().Contain("likes you very much");
    }

    [Test]
    public async Task WhenAgitated_YouDie()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MazeFifteen>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Sword>());

        // so we activate entering the room. 
        await target.GetResponse("SE");
        var response = await target.GetResponse("kill the cyclops with the sword");

        response.Should().Contain("The cyclops shrugs but otherwise ignores your pitiful attempt");
        response.Should().Contain("The cyclops seems somewhat agitated");

        response = await target.GetResponse("wait");
        response.Should().Contain("The cyclops appears to be getting more agitated");

        response = await target.GetResponse("wait");
        response.Should().Contain("The cyclops is moving about the room, looking for something");

        response = await target.GetResponse("wait");
        response.Should().Contain("The cyclops was looking for salt and pepper. I think he is gathering");

        response = await target.GetResponse("wait");
        response.Should().Contain("The cyclops is moving toward you in an unfriendly manner");

        response = await target.GetResponse("wait");
        response.Should().Contain("You have two choices: 1. Leave  2. Become dinner");

        response = await target.GetResponse("wait");
        response.Should().Contain("The cyclops, tired of all of your games and trickery, grabs you firmly");
        target.Context.DeathCounter.Should().Be(1);
    }

    [Test]
    public async Task LeaveAndComeBack_StillGoingToKillYou()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MazeFifteen>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Sword>());

        // so we activate entering the room. 
        await target.GetResponse("SE");
        await target.GetResponse("kill the cyclops with the sword");

        // Leave
        await target.GetResponse("NW");
        
        // Wait 
        for (var turnCounter = 0; turnCounter < 10; turnCounter++) await target.GetResponse("wait");

        // Come back
        var response = await target.GetResponse("SE");
        response.Should().Contain("The cyclops, tired of all of your games and trickery, grabs you firmly");
        target.Context.DeathCounter.Should().Be(1);
    }

    [Test]
    public async Task AttackingHimAgitates()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Sword>());

        var response = await target.GetResponse("kill the cyclops with the sword");

        response.Should().Contain("The cyclops shrugs but otherwise ignores your pitiful attempt");
        Repository.GetItem<Cyclops>().IsAgitated.Should().BeTrue();
    }

    [Test]
    public async Task GivingHimTheLunchAgitatesHim()
    {
    }

    [Test]
    public async Task GivingHimTheGarlic()
    {
    }

    [Test]
    public async Task GivingHimRandomStuffToEat()
    {
    }

    [Test]
    public async Task GivingHimTheWaterPutsHimToSleep()
    {
    }

    [Test]
    public async Task AfterSleep_CanGoUp()
    {
    }


    [Test]
    public async Task AfterSleep_CannotGoEast()
    {
    }


    [Test]
    public async Task AfterSleep_CanWakeHimUpByAttackingHim()
    {
    }
}