using FluentAssertions;
using GameEngine;
using Model.Interface;
using Moq;
using ZorkOne.ActorInteraction;
using ZorkOne.Interface;
using ZorkOne.Item;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests.People;

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
                "The cyclops, hearing the name of his father's deadly nemesis, " +
                "flees the room by knocking down the wall on the east of the room");
    }

    [Test]
    public async Task Asleep_Odysseus_Response()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        Repository.GetItem<Cyclops>().IsSleeping = true;

        var response = await target.GetResponse("ulysses");

        response.Should().Contain("sailor");
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
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        var thiefChooser = new Mock<IRandomChooser>();
        thiefChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Miss, ""));
        GetItem<Thief>().ThiefAttackingEngine = new (thiefChooser.Object);

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
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Lunch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        var response = await target.GetResponse("offer the cyclops the lunch");

        response.Should().Contain("I love hot peppers!");
        Repository.GetItem<Cyclops>().IsAgitated.Should().BeTrue();
    }

    [Test]
    public async Task GivingHimTheLunchAgitatesHim_ReverseSyntax()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Lunch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        var response = await target.GetResponse("give the lunch to the cyclops");

        response.Should().Contain("I love hot peppers!");
        Repository.GetItem<Cyclops>().IsAgitated.Should().BeTrue();
    }

    [Test]
    public async Task GivingHimTheLunch_ItIsGoneForever()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Lunch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        await target.GetResponse("give the lunch to the cyclops");

        Repository.GetItem<Lunch>().CurrentLocation.Should().BeNull();
        target.Context.HasItem<Lunch>().Should().BeFalse();
    }

    [Test]
    public async Task GivingHimTheGarlic()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Garlic>());
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        var response = await target.GetResponse("give the garlic to the cyclops");

        response.Should().Contain("The cyclops may be hungry, but there is a limit");
        Repository.GetItem<Cyclops>().IsAgitated.Should().BeFalse();
    }

    [Test]
    public async Task GivingHimRandomStuffToEat()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<BloodyAxe>());
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        var response = await target.GetResponse("give the axe to the cyclops");

        response.Should().Contain("The cyclops is not so stupid as to eat THAT!");
        Repository.GetItem<Cyclops>().IsAgitated.Should().BeFalse();
    }

    [Test]
    public async Task GivingHimTheWaterPutsHimToSleep()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Lunch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Bottle>());
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        await target.GetResponse("give the lunch to the cyclops");
        await target.GetResponse("give the bottle to the cyclops");

        Repository.GetItem<Cyclops>().IsSleeping.Should().BeTrue();
        Repository.GetItem<Cyclops>().IsAgitated.Should().BeFalse();
    }

    [Test]
    public async Task AfterSleep_CanGoUp()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        Repository.GetItem<Cyclops>().IsSleeping = true;
        var thiefChooser = new Mock<IRandomChooser>();
        thiefChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Miss, ""));
        GetItem<Thief>().ThiefAttackingEngine = new (thiefChooser.Object);

        var response = await target.GetResponse("Up");

        response.Should().Contain("Treasure Room");
        target.Context.CurrentLocation.Should().BeOfType<TreasureRoom>();
    }

    [Test]
    public async Task AfterSleep_CannotGoEast()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        Repository.GetItem<Cyclops>().IsSleeping = true;

        var response = await target.GetResponse("E");

        response.Should().Contain("The east wall is solid rock");
        target.Context.CurrentLocation.Should().BeOfType<CyclopsRoom>();
    }

    [Test]
    public async Task AfterSleep_CanWakeHimUpByAttackingHim()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Sword>());
        target.Context.ItemPlacedHere(Repository.GetItem<Lunch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Bottle>());

        await target.GetResponse("give the lunch to the cyclops");
        await target.GetResponse("give the bottle to the cyclops");
        var response = await target.GetResponse("kill the cyclops with the sword");

        response.Should().Contain("The cyclops shrugs but otherwise ignores your pitiful attempt");
        Repository.GetItem<Cyclops>().IsSleeping.Should().BeFalse();
        Repository.GetItem<Cyclops>().IsAgitated.Should().BeTrue();
    }

    [Test]
    public async Task AfterSleep_DropsTheBottle()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Lunch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Bottle>());

        await target.GetResponse("give the lunch to the cyclops");
        await target.GetResponse("give the bottle to the cyclops");

        Repository.GetItem<Bottle>().CurrentLocation.Should().BeOfType<CyclopsRoom>();
    }

    [Test]
    public async Task AfterSleep_DropsTheBottle_OnTheGround()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Lunch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Bottle>());

        await target.GetResponse("give the lunch to the cyclops");
        await target.GetResponse("give the bottle to the cyclops");
        var response = await target.GetResponse("look");

        response.Should().Contain("a glass bottle here");
    }

    [Test]
    public async Task AfterSleep_BottleHasNoWaterInIt()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Lunch>());
        target.Context.ItemPlacedHere(Repository.GetItem<Bottle>());

        await target.GetResponse("give the lunch to the cyclops");
        await target.GetResponse("give the bottle to the cyclops");

        Repository.GetItem<Water>().CurrentLocation.Should().BeNull();
        Repository.GetItem<Bottle>().Items.Should().BeEmpty();
    }

    [Test]
    public async Task AfterSleep_ExamineTheRoom()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        Repository.GetItem<Cyclops>().IsSleeping = true;

        var response = await target.GetResponse("look");
        response.Should().Contain("The cyclops is sleeping blissfully at the foot of the stairs");
    }

    [Test]
    public async Task AfterSleep_ExamineTheCyclops()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CyclopsRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());
        Repository.GetItem<Cyclops>().IsSleeping = true;

        var response = await target.GetResponse("examine cyclops");

        response.Should().Contain("The cyclops is sleeping like a baby, albeit a very ugly one");
    }
}