using System.Diagnostics;
using System.Text;
using DynamoDb;
using FluentAssertions;
using GameEngine;
using GameEngine.IntentEngine;
using JetBrains.Annotations;
using Model.Interface;
using Moq;
using ZorkOne.ActorInteraction;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests.Walkthrough;

public abstract class WalkthroughTestBase : EngineTestsBase
{
    private readonly DynamoDbSessionRepository _database = new();
    private Mock<IRandomChooser> _adventurerChooser;
    private Mock<IRandomChooser> _attackerChooser;
    private Mock<IRandomChooser> _thiefAppears;
    private Mock<IRandomChooser> _caveSouthChooser;
    private GameEngine<ZorkI, ZorkIContext> _target;

    [OneTimeSetUp]
    public void Init()
    {
        _target = GetTarget();

        _adventurerChooser = new Mock<IRandomChooser>();
        _attackerChooser = new Mock<IRandomChooser>();
        _thiefAppears = new Mock<IRandomChooser>();
        _caveSouthChooser = new Mock<IRandomChooser>();

        // We always kill
        _adventurerChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>()))
            .Returns((CombatOutcome.Fatal, ""));

        // He always misses
        _attackerChooser.Setup(s => s.Choose(It.IsAny<List<(CombatOutcome outcome, string text)>>()))
            .Returns((CombatOutcome.Miss, ""));
        
        // He never appears
        _thiefAppears.Setup(s => s.RollDiceSuccess(ThiefRobsYouEngine.ThiefRobsYouChance)).Returns(false);
        
        _caveSouthChooser.Setup(s => s.RollDiceSuccess(2)).Returns(true);

        GetItem<Thief>().ThiefRobbingEngine = new ThiefRobsYouEngine(_thiefAppears.Object);
        GetItem<Thief>().ThiefAttackedEngine = new AdventurerVersusThiefCombatEngine(_adventurerChooser.Object);
        GetItem<Thief>().ThiefAttackingEngine = new ThiefCombatEngine(_attackerChooser.Object);
        GetItem<Troll>().TrollAttackEngine = new TrollCombatEngine(_attackerChooser.Object);
        GetLocation<TrollRoom>().KillDecisionEngine =
            new KillSomeoneDecisionEngine<Troll>(new AdventurerVersusTrollCombatEngine(_adventurerChooser.Object));
            
        var caveSouth = GetLocation<CaveSouth>();
        caveSouth.RandomChooser = _caveSouthChooser.Object;
    }

    protected void InvokeGodMode(string setup)
    {
        // Ooooooh! Reflection!!
        var method = GetType().GetMethod(setup);
        if (method == null)
            throw new ArgumentException("Method " + setup + " doesn't exist");

        // Invoke the method on the current instance
        method.Invoke(this, null);
    }

    [UsedImplicitly]
    public void PutTheTorchHere()
    {
        Repository.GetLocation<TreasureRoom>().ItemPlacedHere(Repository.GetItem<Torch>());
    }

    [UsedImplicitly]
    public void GoToRoundRoom()
    {
        // Entering the loud room when it's draining will cause us to flee the room in a random
        // direction. For the test we need to remove the randomness and end up in the Round Room
        _target.Context.CurrentLocation = Repository.GetLocation<RoundRoom>();
    }

    protected async Task Do(string input, params string[] outputs)
    {
        var result = await _target.GetResponse(input);
        if (Debugger.IsAttached)
        {
            Console.WriteLine(result);
            var sessionId = Environment.MachineName;
            var bytesToEncode = Encoding.UTF8.GetBytes(_target.Context.Engine!.SaveGame());
            var encodedText = Convert.ToBase64String(bytesToEncode);
            await _database.WriteSession(sessionId, encodedText, _target.SessionTableName);
        }

        foreach (var output in outputs)
            result.Should().Contain(output);
    }
}
