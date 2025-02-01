using FluentAssertions;
using Model.Interface;
using Model.Item;
using Moq;
using ZorkOne.ActorInteraction;
using ZorkOne.Interface;
using ZorkOne.Item;
using ZorkOne.Location.ForestLocation;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests.People;

public class ThiefTests : EngineTestsBase
{
    [Test]
    public async Task CannotTakeChaliceWhenAlive()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();

        var response = await target.GetResponse("take chalice");

        response.Should().Contain("stabbed in the back");
    }

    [Test]
    public async Task CanTakeChaliceWhenDead()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        GetItem<Thief>().IsDead = true;

        var response = await target.GetResponse("take chalice");

        response.Should().Contain("Taken");
    }

    [Test]
    public async Task CanTakeChaliceWhenUnconscious()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        GetItem<Thief>().IsUnconscious = true;

        var response = await target.GetResponse("take chalice");

        response.Should().Contain("Taken");
    }

    [Test]
    public async Task RunsToDefendRoom()
    {
        var target = GetTarget();
        StartHere<CyclopsRoom>();
        GetItem<Cyclops>().IsSleeping = true;

        var response = await target.GetResponse("up");

        response.Should().Contain("rushes to its defense");
    }

    [Test]
    public async Task Dead_DoesNotDefendRoom()
    {
        var target = GetTarget();
        StartHere<CyclopsRoom>();
        GetItem<Cyclops>().IsSleeping = true;
        GetItem<Thief>().IsDead = true;

        var response = await target.GetResponse("up");

        response.Should().NotContain("rushes to its defense");
    }

    [Test]
    public async Task Kill()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        Take<Torch>();
        Take<Sword>();
        List<(CombatOutcome outcome, string text)> options =
            [(CombatOutcome.Fatal, "It's curtains for the thief as your {weapon} removes his head. ")];
        var choose = new Mock<IRandomChooser>();
        choose.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns(options.Single());
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(choose.Object);

        var response = await target.GetResponse("kill thief with sword");

        response.Should().Contain("It's curtains for the thief");
        response.Should().Contain("black fog envelops him, and when the fog lifts");
        response.Should().Contain("The chalice is now safe to take");
        GetItem<Thief>().IsDead.Should().BeTrue();
    }

    [Test]
    public async Task GiveHimTheEgg_KillHim_GetTheEggBackOpen()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        Take<Torch>();
        Take<Sword>();
        Take<Egg>();
        List<(CombatOutcome outcome, string text)> options =
            [(CombatOutcome.Fatal, "It's curtains for the thief as your {weapon} removes his head. ")];
        var choose = new Mock<IRandomChooser>();
        choose.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns(options.Single());
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(choose.Object);

        await target.GetResponse("give the egg to the thief");
        var response = await target.GetResponse("kill thief with sword");

        response.Should().Contain("It's curtains for the thief");
        response.Should().Contain("black fog envelops him, and when the fog lifts");
        response.Should().Contain("As the thief dies, the power of his magic decreases, and his treasures reappear");
        response.Should().Contain("A jewel-encrusted egg, with a golden clockwork canary");
        Console.WriteLine(response);
        GetItem<Thief>().IsDead.Should().BeTrue();
    }

    [Test]
    public async Task GiveHimTreasures_KillHim_GetTheTreasures()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        Take<Torch>();
        Take<Sword>();
        Take<Sceptre>();
        Take<Trident>();
        List<(CombatOutcome outcome, string text)> options =
            [(CombatOutcome.Fatal, "It's curtains for the thief as your {weapon} removes his head. ")];
        var choose = new Mock<IRandomChooser>();
        choose.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns(options.Single());
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(choose.Object);

        await target.GetResponse("give the sceptre to the thief");
        await target.GetResponse("give the trident to the thief");
        var response = await target.GetResponse("kill thief with sword");

        response.Should().Contain("It's curtains for the thief");
        response.Should().Contain("black fog envelops him, and when the fog lifts");
        response.Should().Contain("As the thief dies, the power of his magic decreases, and his treasures reappear");
        Console.WriteLine(response);
        GetItem<Thief>().IsDead.Should().BeTrue();
    }

    [Test]
    public async Task KillHim_TakeChalice()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        Take<Torch>();
        Take<Sword>();
        List<(CombatOutcome outcome, string text)> options =
            [(CombatOutcome.Fatal, "It's curtains for the thief as your {weapon} removes his head. ")];
        var choose = new Mock<IRandomChooser>();
        choose.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns(options.Single());
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(choose.Object);

        await target.GetResponse("kill thief with sword");
        var response = await target.GetResponse("take chalice");

        response.Should().Contain("Taken");
    }

    [Test]
    public async Task KillHim_TakeStiletto()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        Take<Torch>();
        Take<Sword>();
        List<(CombatOutcome outcome, string text)> options =
            [(CombatOutcome.Fatal, "It's curtains for the thief as your {weapon} removes his head. ")];
        var choose = new Mock<IRandomChooser>();
        choose.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns(options.Single());
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(choose.Object);

        await target.GetResponse("kill thief with sword");
        var response = await target.GetResponse("take stiletto");

        response.Should().Contain("Taken");
    }

    [Test]
    public async Task Unconscious_TakeStiletto()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        Take<Torch>();
        Take<Sword>();
        List<(CombatOutcome outcome, string text)> options =
            [(CombatOutcome.Knockout, "It's curtains for the thief as your {weapon} removes his head. ")];
        var choose = new Mock<IRandomChooser>();
        choose.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns(options.Single());
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(choose.Object);

        await target.GetResponse("kill thief with sword");
        var response = await target.GetResponse("take stiletto");

        response.Should().Contain("white-hot");
    }

    [Test]
    public async Task Unconscious_GiveHimSomething_WakesUp()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        Take<Torch>();
        Take<Sword>();
        Take<Sceptre>();
        List<(CombatOutcome outcome, string text)> options = [(CombatOutcome.Knockout, "")];
        var choose = new Mock<IRandomChooser>();
        choose.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns(options.Single());
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(choose.Object);

        await target.GetResponse("kill thief with sword");
        var response = await target.GetResponse("give the sceptre to the thief");

        response.Should().Contain("Your proposed victim suddenly");
        response.Should().Contain("thanks you politely.");
    }

    [Test]
    public async Task Unconscious_TakeChalice()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        Take<Torch>();
        Take<Sword>();
        Take<Sceptre>();
        List<(CombatOutcome outcome, string text)> options = [(CombatOutcome.Knockout, "")];
        var choose = new Mock<IRandomChooser>();
        choose.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns(options.Single());
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(choose.Object);

        await target.GetResponse("kill thief with sword");
        var response = await target.GetResponse("take chalice");

        response.Should().Contain("Taken");
    }

    [Test]
    public async Task Unconscious_Revives()
    {
        var target = GetTarget();
        StartHere<CyclopsRoom>();
        GetItem<Cyclops>().IsSleeping = true;
        Take<Torch>();
        Take<Sword>();
        var adventurerChooser = new Mock<IRandomChooser>();
        var thiefChooser = new Mock<IRandomChooser>();
        adventurerChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Knockout, ""));
        thiefChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Miss, ""));
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(adventurerChooser.Object);
        GetItem<Thief>().ThiefAttackingEngine = new (thiefChooser.Object);

        await target.GetResponse("up");
        await target.GetResponse("kill thief with sword");
        var response = await target.GetResponse("wait");

        response.Should().Contain("briefly feigning ");
    }

    [Test]
    public async Task Unconscious_Look()
    {
        var target = GetTarget();
        StartHere<CyclopsRoom>();
        GetItem<Cyclops>().IsSleeping = true;
        Take<Torch>();
        Take<Sword>();
        var adventurerChooser = new Mock<IRandomChooser>();
        var thiefChooser = new Mock<IRandomChooser>();
        adventurerChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Knockout, ""));
        thiefChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Miss, ""));
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(adventurerChooser.Object);
        GetItem<Thief>().ThiefAttackingEngine = new (thiefChooser.Object);

        await target.GetResponse("up");
        await target.GetResponse("kill thief with sword");
        var response = await target.GetResponse("look");

        response.Should().Contain("lying unconscious");
    }

    [Test]
    public async Task Unconscious_LeaveAndComeBackRevives()
    {
        var target = GetTarget();
        StartHere<CyclopsRoom>();
        GetItem<Cyclops>().IsSleeping = true;
        Take<Torch>();
        Take<Sword>();
        var adventurerChooser = new Mock<IRandomChooser>();
        var thiefChooser = new Mock<IRandomChooser>();
        adventurerChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Knockout, ""));
        thiefChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Miss, ""));
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(adventurerChooser.Object);
        GetItem<Thief>().ThiefAttackingEngine = new (thiefChooser.Object);

        await target.GetResponse("up");
        await target.GetResponse("kill thief with sword");
        await target.GetResponse("down");
        var response = await target.GetResponse("up");

        response.Should().Contain("holding a large ");
        response.Should().NotContain("lying unconscious");
    }

    [Test]
    public async Task Unconscious_ThenKill()
    {
        var target = GetTarget();
        StartHere<CyclopsRoom>();
        GetItem<Cyclops>().IsSleeping = true;
        Take<Torch>();
        Take<Sword>();
        var adventurerChooser = new Mock<IRandomChooser>();
        var thiefChooser = new Mock<IRandomChooser>();
        adventurerChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Knockout, ""));
        thiefChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Miss, ""));
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(adventurerChooser.Object);
        GetItem<Thief>().ThiefAttackingEngine = new (thiefChooser.Object);

        await target.GetResponse("up");
        await target.GetResponse("kill thief with sword");
        var response = await target.GetResponse("kill thief with sword");

        response.Should().Contain("cannot defend himself");
    }

    [Test]
    public async Task StunnedAdventurer()
    {
        var target = GetTarget();
        StartHere<CyclopsRoom>();
        GetItem<Cyclops>().IsSleeping = true;
        Take<Torch>();
        Take<Sword>();
        target.Context.IsStunned = true;
        var adventurerChooser = new Mock<IRandomChooser>();
        var thiefChooser = new Mock<IRandomChooser>();
        adventurerChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Knockout, ""));
        thiefChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns((CombatOutcome.Miss, ""));
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(adventurerChooser.Object);
        GetItem<Thief>().ThiefAttackingEngine = new (thiefChooser.Object);

        await target.GetResponse("up");
        var response = await target.GetResponse("kill thief with sword");

        response.Should().Contain("still recovering");
        target.Context.IsStunned.Should().BeFalse();
    }

    [Test]
    public async Task ShowUp_WillNeverInNonMarkedLocations()
    {
        var target = GetTarget();
        target.Context.RegisterActor(GetItem<Thief>());
        StartHere<ForestPath>();
        var thiefChooser = new Mock<IRandomChooser>();
        thiefChooser.Setup(s => s.RollDice(ThiefRobsYouEngine.ThiefRobsYouChance)).Returns(true);
        GetItem<Thief>().ThiefRobbingEngine = new ThiefRobsYouEngine(thiefChooser.Object);
        
        await target.GetResponse("look");
    }
    
    [Test]
    public async Task ShowUp_WillAppearInMarkedLocations_NothingOfValue_FirstResult()
    {
        var target = GetTarget();
        StartHere<DeadEndFour>();
        target.Context.RegisterActor(GetItem<Thief>());
        var thiefChooser = new Mock<IRandomChooser>();
        thiefChooser.Setup(s => s.RollDice(ThiefRobsYouEngine.ThiefRobsYouChance)).Returns(true);
        thiefChooser.Setup(s => s.Choose(ThiefRobsYouEngine.NothingOfValue)).Returns(ThiefRobsYouEngine.NothingOfValue[0]);
        GetItem<Thief>().ThiefRobbingEngine = new ThiefRobsYouEngine(thiefChooser.Object);
        
        string? response = await target.GetResponse("look");
        response.Should().Contain("carrying a large bag");
    }
    
    [Test]
    public async Task ShowUp_WillAppearInMarkedLocations_NothingOfValue_SecondResult()
    {
        var target = GetTarget();
        StartHere<DeadEndFour>();
        target.Context.RegisterActor(GetItem<Thief>());
        var thiefChooser = new Mock<IRandomChooser>();
        thiefChooser.Setup(s => s.RollDice(ThiefRobsYouEngine.ThiefRobsYouChance)).Returns(true);
        thiefChooser.Setup(s => s.Choose(ThiefRobsYouEngine.NothingOfValue)).Returns(ThiefRobsYouEngine.NothingOfValue[1]);
        GetItem<Thief>().ThiefRobbingEngine = new ThiefRobsYouEngine(thiefChooser.Object);
        
        string? response = await target.GetResponse("look");
        response.Should().Contain("looking disgusted");
    }
    
    [Test]
    public async Task ShowUp_WillNotAppearInMarkedLocationsIfFailedDiceRoll_NothingOfValue()
    {
        var target = GetTarget();
        StartHere<DeadEndFour>();
        target.Context.RegisterActor(GetItem<Thief>());
        var thiefChooser = new Mock<IRandomChooser>();
        thiefChooser.Setup(s => s.RollDice(ThiefRobsYouEngine.ThiefRobsYouChance)).Returns(false);
        thiefChooser.Setup(s => s.Choose(ThiefRobsYouEngine.NothingOfValue)).Returns(ThiefRobsYouEngine.NothingOfValue[0]);
        GetItem<Thief>().ThiefRobbingEngine = new ThiefRobsYouEngine(thiefChooser.Object);
        
        string? response = await target.GetResponse("look");
        response.Should().NotContain("carrying a large bag");
    }
    
    [Test]
    public async Task ShowUp_WillAppearInMarkedLocations_SingleItemOfValue_FirstResult()
    {
        var target = GetTarget();
        StartHere<DeadEndFour>();
        GetLocation<DeadEndFour>().ItemPlacedHere(GetItem<Coffin>());
        target.Context.RegisterActor(GetItem<Thief>());
        var thiefChooser = new Mock<IRandomChooser>();
        thiefChooser.Setup(s => s.Choose(It.IsAny<List<IGivePointsWhenPlacedInTrophyCase>>())).Returns(GetItem<Coffin>());
        thiefChooser.Setup(s => s.RollDice(ThiefRobsYouEngine.ThiefRobsYouChance)).Returns(true);
        thiefChooser.Setup(s => s.Choose(ThiefRobsYouEngine.StealFromRoomResults)).Returns(ThiefRobsYouEngine.StealFromRoomResults[0]);
        GetItem<Thief>().ThiefRobbingEngine = new ThiefRobsYouEngine(thiefChooser.Object);
        
        string? response = await target.GetResponse("look");
        response.Should().Contain("abstracted some valuables from the room");
    }
}