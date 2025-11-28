using System.Diagnostics;
using System.Text;
using ChatLambda;
using DynamoDb;
using FluentAssertions;
using GameEngine;
using JetBrains.Annotations;
using Model.Interface;
using Moq;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Tests.Walkthrough;

public abstract class WalkthroughTestBase : EngineTestsBase
{
    private readonly DynamoDbSessionRepository _database = new();
    private GameEngine<PlanetfallGame, PlanetfallContext> _target;
    private Mock<IRandomChooser> _floydChooser;
    private Mock<IChatWithFloyd> _chatWithFloyd;

    [OneTimeSetUp]
    public void Init()
    {
        _target = GetTarget();
        
        _floydChooser = new Mock<IRandomChooser>();
        _floydChooser.Setup(s => s.RollDiceSuccess(3)).Returns(true);

        _chatWithFloyd = new Mock<IChatWithFloyd>();
        _chatWithFloyd.Setup(s => s.AskFloydAsync("go north")).ReturnsAsync(new CompanionResponse(
            "Floyd's response",
            new CompanionMetadata("GoSomewhere", new Dictionary<string, object> { { "direction", "north" } })
        ));
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
    public void ResetTime()
    {
        Repository.GetItem<Chronometer>().CurrentTime = 2000;
    }

    protected async Task Do(string input, params string[] outputs)
    {
        var floyd = Repository.GetItem<Floyd>();
        floyd.Chooser = _floydChooser.Object;
        floyd.ChatWithFloyd = _chatWithFloyd.Object;
        
        var result = await _target.GetResponse(input);
        if (Debugger.IsAttached)
        {
            Console.WriteLine(result);
            var sessionId = Environment.MachineName + "8";
            var bytesToEncode = Encoding.UTF8.GetBytes(_target.Context.Engine!.SaveGame());
            var encodedText = Convert.ToBase64String(bytesToEncode);
            await _database.WriteSessionState(sessionId, encodedText, _target.SessionTableName);
        }

        foreach (var output in outputs)
            result.Should().Contain(output);
    }

    protected async Task DoWithSetup(string input, string? setup, params string[] outputs)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, outputs);
    }
}