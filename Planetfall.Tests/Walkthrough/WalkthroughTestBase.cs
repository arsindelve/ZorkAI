using System.Diagnostics;
using System.Text;
using DynamoDb;
using FluentAssertions;
using GameEngine;

namespace Planetfall.Tests.Walkthrough;

public abstract class WalkthroughTestBase : EngineTestsBase
{
    private readonly DynamoDbSessionRepository _database = new();
    private GameEngine<PlanetfallGame, PlanetfallContext> _target;

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