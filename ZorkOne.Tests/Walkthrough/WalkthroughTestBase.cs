using System.Diagnostics;
using System.Text;
using DynamoDb;
using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests.Walkthrough;

public abstract class WalkthroughTestBase : EngineTestsBase
{
    private readonly DynamoDbSessionRepository _database = new();
    private GameEngine<ZorkI, ZorkIContext> _target;

    [OneTimeSetUp]
    public void Init()
    {
        _target = GetTarget();
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

    public void KillTroll()
    {
        // We can't have the randomness of trying to kill the troll. Let's God-Mode this dude.
        Repository.GetItem<Troll>().IsDead = true;
    }
    
    public void KillThief()
    {
        // We can't have the randomness of trying to kill the thief. Let's God-Mode this dude.
        Repository.GetItem<Thief>().IsDead = true;
    }

    public void PutTheTorchHere()
    {
        Repository.GetLocation<TreasureRoom>().ItemPlacedHere(Repository.GetItem<Torch>());
    }

    public void HaveOpenEgg()
    {
        _target.Context.ItemPlacedHere(Repository.GetItem<Egg>());
        Repository.GetItem<Egg>().IsOpen = true;
    }

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