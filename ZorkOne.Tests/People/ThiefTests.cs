using FluentAssertions;
using Moq;
using ZorkOne.ActorInteraction;
using ZorkOne.Interface;
using ZorkOne.Item;
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
        List<(CombatOutcome outcome, string text)> options = [(CombatOutcome.Fatal, "It's curtains for the thief as your {weapon} removes his head. ")];
        var choose = new Mock<IRandomChooser>();
        choose.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns(options.Single());
        GetItem<Thief>().ThiefAttackEngine = new AdventurerVersusThiefCombatEngine(choose.Object);

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
        List<(CombatOutcome outcome, string text)> options = [(CombatOutcome.Fatal, "It's curtains for the thief as your {weapon} removes his head. ")];
        var choose = new Mock<IRandomChooser>();
        choose.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns(options.Single());
        GetItem<Thief>().ThiefAttackEngine = new AdventurerVersusThiefCombatEngine(choose.Object);

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
        List<(CombatOutcome outcome, string text)> options = [(CombatOutcome.Fatal, "It's curtains for the thief as your {weapon} removes his head. ")];
        var choose = new Mock<IRandomChooser>();
        choose.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>())).Returns(options.Single());
        GetItem<Thief>().ThiefAttackEngine = new AdventurerVersusThiefCombatEngine(choose.Object);

        await target.GetResponse("give the sceptre to the thief");
        await target.GetResponse("give the trident to the thief");
        var response = await target.GetResponse("kill thief with sword");

        response.Should().Contain("It's curtains for the thief");
        response.Should().Contain("black fog envelops him, and when the fog lifts");
        response.Should().Contain("As the thief dies, the power of his magic decreases, and his treasures reappear");
        Console.WriteLine(response);
        GetItem<Thief>().IsDead.Should().BeTrue();
    }
    
    //Can take chalice
    //can take stiletto
    
    // White hot stiletto when unconscious
    // Can take chalice when unconscious
    
    
    
}